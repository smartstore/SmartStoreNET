using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;
using SmartStore.Services.Orders;
using SmartStore.Utilities.Threading;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class CartSubtotalRule : IRule
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        public CartSubtotalRule(IShoppingCartService shoppingCartService, IOrderTotalCalculationService orderTotalCalculationService)
        {
            _shoppingCartService = shoppingCartService;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var sessionKey = context.SessionKey;
            var lockKey = "rule:cart:cartsubtotalrule:" + sessionKey.ToString();

            if (KeyedLock.IsLockHeld(lockKey))
            {
                //$"locked expression {expression.Id}: {lockKey}".Dump();
                return false;
            }

            // We must prevent the rule from indirectly calling itself. It would cause a stack overflow on cart page.
            using (KeyedLock.Lock(lockKey))
            {
                var cart = _shoppingCartService.GetCartItems(context.Customer, ShoppingCartType.ShoppingCart, context.Store.Id);

                _orderTotalCalculationService.GetShoppingCartSubTotal(cart, out _, out _, out var cartSubtotal, out _);

                // Currency values must be rounded, otherwise unexpected results may occur.
                var money = new Money(cartSubtotal, context.WorkContext.WorkingCurrency);
                cartSubtotal = money.RoundedAmount;

                var result = expression.Operator.Match(cartSubtotal, expression.Value);
                //$"unlocked expression {expression.Id}: {lockKey}".Dump();
                return result;
            }
        }
    }
}
