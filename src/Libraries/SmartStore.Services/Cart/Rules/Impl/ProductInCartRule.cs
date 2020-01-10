using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class ProductInCartRule : ListRuleBase<int>
    {
        private readonly IShoppingCartService _shoppingCartService;

        public ProductInCartRule(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        protected override IEnumerable<int> GetValues(CartRuleContext context)
        {
            var cartProductIds = _shoppingCartService.GetCartItems(context.Customer, ShoppingCartType.ShoppingCart, context.Store.Id)
                .Select(x => x.Item.ProductId);

            return cartProductIds;
        }
    }
}
