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

        protected override object GetValue(CartRuleContext context)
        {
            var cartProductIds = _shoppingCartService.GetCartItems(context.Customer, ShoppingCartType.ShoppingCart, context.Store.Id)
                .Select(x => x.Item.ProductId)
                .ToList();

            return cartProductIds;
        }
    }
}
