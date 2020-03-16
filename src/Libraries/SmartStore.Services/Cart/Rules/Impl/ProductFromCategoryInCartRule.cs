using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class ProductFromCategoryInCartRule : ListRuleBase<int>
    {
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IShoppingCartService _shoppingCartService;

        public ProductFromCategoryInCartRule(
            IRepository<ProductCategory> productCategoryRepository,
            IShoppingCartService shoppingCartService)
        {
            _productCategoryRepository = productCategoryRepository;
            _shoppingCartService = shoppingCartService;
        }

        protected override IEnumerable<int> GetValues(CartRuleContext context)
        {
            var cart = _shoppingCartService.GetCartItems(context.Customer, ShoppingCartType.ShoppingCart, context.Store.Id);
            var productIds = cart.Select(x => x.Item.ProductId).ToArray();

            if (productIds.Any())
            {
                // It's unnecessary to check things like ACL, limited-to-stores, published, deleted etc. here
                // because the products are from shopping cart and it cannot contain hidden products.
                var categoryIds = _productCategoryRepository.TableUntracked
                    .Where(x => productIds.Contains(x.ProductId))
                    .Select(x => x.CategoryId)
                    .ToList();

                return categoryIds.Distinct();
            }

            return Enumerable.Empty<int>();
        }
    }
}
