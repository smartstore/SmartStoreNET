using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class CartTotalRule : IRule
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        public CartTotalRule(
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _shoppingCartService = shoppingCartService;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            // TODO: stack overflow! This rule calls itself through IDiscountService, which must be prevented.
            return true;

            var cart = _shoppingCartService.GetCartItems(context.Customer, ShoppingCartType.ShoppingCart, context.Store.Id);

            var cartTotal = ((decimal?)_orderTotalCalculationService.GetShoppingCartTotal(cart)) ?? decimal.Zero;

            // Currency values must be rounded, otherwise unexpected results may occur.
            var money = new Money(cartTotal, context.WorkContext.WorkingCurrency);
            cartTotal = money.RoundedAmount;

            return expression.Operator.Match(cartTotal, expression.Value);
        }
    }
}
