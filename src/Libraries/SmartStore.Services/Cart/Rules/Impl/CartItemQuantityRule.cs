using System;
using System.Linq;
using SmartStore.Core.Domain.Orders;
using SmartStore.Rules;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class CartItemQuantityRule : IRule
    {
        private readonly IShoppingCartService _shoppingCartService;

        public CartItemQuantityRule(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var rawValues = (expression.Value as string).SplitSafe("|");
            var productId = rawValues.Length > 0 ? rawValues[0].ToInt() : 0;
            var minQuantity = rawValues.Length > 1 ? rawValues[1].ToInt() : 0;
            var maxQuantity = rawValues.Length > 2 ? rawValues[2].ToInt() : 0;

            if (productId == 0)
            {
                return false;
            }

            var cart = _shoppingCartService.GetCartItems(context.Customer, ShoppingCartType.ShoppingCart, context.Store.Id);
            var item = cart.FirstOrDefault(x => x.Item.ProductId == productId);
            if (item == null)
            {
                return false;
            }

            //TODO...
            return false;
        }
    }
}
