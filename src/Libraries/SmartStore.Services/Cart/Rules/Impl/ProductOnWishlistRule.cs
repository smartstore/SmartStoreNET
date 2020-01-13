using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class ProductOnWishlistRule : ListRuleBase<int>
    {
        private readonly IShoppingCartService _shoppingCartService;

        public ProductOnWishlistRule(IShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        protected override IEnumerable<int> GetValues(CartRuleContext context)
        {
            var wishlistProductIds = _shoppingCartService.GetCartItems(context.Customer, ShoppingCartType.Wishlist, context.Store.Id)
                .Select(x => x.Item.ProductId);

            return wishlistProductIds;
        }
    }
}
