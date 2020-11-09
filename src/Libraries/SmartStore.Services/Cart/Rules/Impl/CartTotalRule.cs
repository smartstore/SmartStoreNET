using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;
using SmartStore.Services.Orders;
using SmartStore.Utilities.Threading;

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
            var sessionKey = context.SessionKey;
            var lockKey = "rule:cart:carttotalrule:" + sessionKey.ToString();

            if (KeyedLock.IsLockHeld(lockKey))
            {
                return false;
            }

            // We must prevent the rule from indirectly calling itself. It would cause a stack overflow on cart page.
            using (KeyedLock.Lock(lockKey))
            {
                var cart = _shoppingCartService.GetCartItems(context.Customer, ShoppingCartType.ShoppingCart, context.Store.Id);

                var cartTotal = ((decimal?)_orderTotalCalculationService.GetShoppingCartTotal(cart)) ?? decimal.Zero;

                // Currency values must be rounded, otherwise unexpected results may occur.
                var money = new Money(cartTotal, context.WorkContext.WorkingCurrency);
                cartTotal = money.RoundedAmount;

                var result = expression.Operator.Match(cartTotal, expression.Value);
                return result;
            }
        }
    }
}
