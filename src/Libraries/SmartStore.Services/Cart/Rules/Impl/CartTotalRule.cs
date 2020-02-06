using System.Threading;
using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class CartTotalRule : IRule
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private int _reentrancyNum = 0;

        public CartTotalRule(
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _shoppingCartService = shoppingCartService;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var result = true;

            // We must prevent the rule from indirectly calling itself. It would cause a stack overflow on cart page
            // and wrong discount calculation (due to MergeWithCombination, if the cart contains a product several times).
            if (Interlocked.CompareExchange(ref _reentrancyNum, 1, 0) == 0)
            {
                try
                {
                    var cart = _shoppingCartService.GetCartItems(context.Customer, ShoppingCartType.ShoppingCart, context.Store.Id);

                    var cartTotal = ((decimal?)_orderTotalCalculationService.GetShoppingCartTotal(cart)) ?? decimal.Zero;

                    // Currency values must be rounded, otherwise unexpected results may occur.
                    var money = new Money(cartTotal, context.WorkContext.WorkingCurrency);
                    cartTotal = money.RoundedAmount;

                    result = expression.Operator.Match(cartTotal, expression.Value);
                }
                finally
                {
                    _reentrancyNum = 0;
                }
            }

            return result;
        }
    }
}
