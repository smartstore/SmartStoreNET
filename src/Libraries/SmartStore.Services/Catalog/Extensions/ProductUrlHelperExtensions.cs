using System.Linq;
using System.Runtime.CompilerServices;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Orders;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Catalog.Extensions
{
    public static class ProductUrlHelperExtensions
    {
        /// <summary>
        /// Creates a product URL including variant query string.
        /// </summary>
        /// <param name="helper">Product URL helper</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="productSeName">Product SEO name</param>
        /// <param name="bundleItemId">Bundle item identifier. Use 0 if it's not a bundle item.</param>
        /// <param name="variantValues">Variant values</param>
        /// <returns>Product URL</returns>
        public static string GetProductUrl(
            this ProductUrlHelper helper,
            int productId,
            string productSeName,
            int bundleItemId,
            params ProductVariantAttributeValue[] variantValues)
        {
            Guard.NotZero(productId, nameof(productId));

            var query = new ProductVariantQuery();

            foreach (var value in variantValues)
            {
                var attribute = value.ProductVariantAttribute;

                query.AddVariant(new ProductVariantQueryItem(value.Id.ToString())
                {
                    ProductId = productId,
                    BundleItemId = bundleItemId,
                    AttributeId = attribute.ProductAttributeId,
                    VariantAttributeId = attribute.Id,
                    Alias = attribute.ProductAttribute.Alias,
                    ValueAlias = value.Alias
                });
            }

            return helper.GetProductUrl(query, productSeName);
        }

        /// <summary>
        /// Creates a product URL including variant query string.
        /// </summary>
        /// <param name="helper">Product URL helper</param>
        /// <param name="product">Product entity</param>
        /// <param name="variantValues">Variant values</param>
        /// <returns>Product URL</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetProductUrl(
            this ProductUrlHelper helper,
            Product product,
            params ProductVariantAttributeValue[] variantValues)
        {
            Guard.NotNull(product, nameof(product));

            return helper.GetProductUrl(product.Id, product.GetSeName(), 0, variantValues);
        }

        /// <summary>
        /// Creates a product URL including variant query string.
        /// </summary>
        /// <param name="helper">Product URL helper</param>
        /// <param name="productSeName">Product SEO name</param>
        /// <param name="cartItem">Organized shopping cart item</param>
        /// <returns>Product URL</returns>
        public static string GetProductUrl(
            this ProductUrlHelper helper,
            string productSeName,
            OrganizedShoppingCartItem cartItem)
        {
            Guard.NotNull(cartItem, nameof(cartItem));

            var query = new ProductVariantQuery();
            var product = cartItem.Item.Product;

            if (product.ProductType != ProductType.BundledProduct)
            {
                helper.DeserializeQuery(query, product.Id, cartItem.Item.AttributesXml);
            }
            else if (cartItem.ChildItems != null && product.BundlePerItemPricing)
            {
                foreach (var childItem in cartItem.ChildItems.Where(x => x.Item.Id != cartItem.Item.Id))
                {
                    helper.DeserializeQuery(query, childItem.Item.ProductId, childItem.Item.AttributesXml, childItem.BundleItemData.Item.Id);
                }
            }

            return helper.GetProductUrl(query, productSeName);
        }

        /// <summary>
        /// Creates a product URL including variant query string.
        /// </summary>
        /// <param name="helper">Product URL helper</param>
        /// <param name="productSeName">Product SEO name</param>
        /// <param name="orderItem">Order item</param>
        /// <returns>Product URL</returns>
        public static string GetProductUrl(
            this ProductUrlHelper helper,
            string productSeName,
            OrderItem orderItem)
        {
            Guard.NotNull(orderItem, nameof(orderItem));

            var query = new ProductVariantQuery();

            if (orderItem.Product.ProductType != ProductType.BundledProduct)
            {
                helper.DeserializeQuery(query, orderItem.ProductId, orderItem.AttributesXml);
            }
            else if (orderItem.Product.BundlePerItemPricing && orderItem.BundleData.HasValue())
            {
                var bundleData = orderItem.GetBundleData();

                bundleData.ForEach(x => helper.DeserializeQuery(query, x.ProductId, x.AttributesXml, x.BundleItemId));
            }

            return helper.GetProductUrl(query, productSeName);
        }
    }
}
