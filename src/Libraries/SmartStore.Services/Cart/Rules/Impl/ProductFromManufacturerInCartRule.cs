using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class ProductFromManufacturerInCartRule : ListRuleBase<int>
    {
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IShoppingCartService _shoppingCartService;

        public ProductFromManufacturerInCartRule(
            IRepository<ProductManufacturer> productManufacturerRepository,
            IShoppingCartService shoppingCartService)
        {
            _productManufacturerRepository = productManufacturerRepository;
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
                var manufacturerIds = _productManufacturerRepository.TableUntracked
                    .Where(x => productIds.Contains(x.ProductId))
                    .Select(x => x.ManufacturerId)
                    .ToList();

                return manufacturerIds.Distinct();
            }

            return Enumerable.Empty<int>();
        }
    }
}
