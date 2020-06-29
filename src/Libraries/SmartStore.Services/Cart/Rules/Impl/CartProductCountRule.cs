using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class CartProductCountRule : IRule
    {
        private readonly IShoppingCartService _shoppingCartService;

        public CartProductCountRule(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var cart = _shoppingCartService.GetCartItems(context.Customer, ShoppingCartType.ShoppingCart, context.Store.Id);
            var productCount = cart.GetTotalProducts();

            return expression.Operator.Match(productCount, expression.Value);
        }
    }
}
