using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Price calculation service
    /// </summary>
    public partial interface IPriceCalculationService
    {
        /// <summary>
        /// Creates a price calculation context
        /// </summary>
        /// <param name="products">Products. <c>null</c> to lazy load data if required.</param>
        /// <param name="customer">Customer, <c>null</c> to use current customer.</param>
        /// <param name="storeId">Store identifier, <c>null</c> to use current store.</param>
        /// <param name="includeHidden">Indicates whether to load hidden records.</param>
        /// <returns>Price calculation context</returns>
        PriceCalculationContext CreatePriceCalculationContext(IEnumerable<Product> products = null, Customer customer = null, int? storeId = null, bool includeHidden = true);

        /// <summary>
        /// Get product special price (is valid)
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>Product special price</returns>
        decimal? GetSpecialPrice(Product product);

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        decimal GetFinalPrice(Product product, bool includeDiscounts);

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        decimal GetFinalPrice(Product product,
            Customer customer,
            bool includeDiscounts);

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
        decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts);

        /// <summary>
        /// Gets the final price
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <param name="bundleItem">A product bundle item</param>
        /// <param name="context">Object with cargo data for better performance</param>
        /// <returns>Final price</returns>
        decimal GetFinalPrice(Product product,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            ProductBundleItemData bundleItem = null,
            PriceCalculationContext context = null,
            bool isTierPrice = false);
        /// <summary>
        /// Gets the final price including bundle per-item pricing
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="bundleItems">Bundle items</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
        /// <param name="bundleItem">A product bundle item</param>
        /// <param name="context">Object with cargo data for better performance</param>
        /// <returns>Final price</returns>
        decimal GetFinalPrice(Product product,
            IEnumerable<ProductBundleItemData> bundleItems,
            Customer customer,
            decimal additionalCharge,
            bool includeDiscounts,
            int quantity,
            ProductBundleItemData bundleItem = null,
            PriceCalculationContext context = null);

        /// <summary>
        /// Get the lowest possible price for a product.
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="context">Object with cargo data for better performance</param>
        /// <param name="displayFromMessage">Whether to display the from message.</param>
        /// <returns>The lowest price.</returns>
        decimal GetLowestPrice(Product product, Customer customer, PriceCalculationContext context, out bool displayFromMessage);

        /// <summary>
        /// Get the lowest price of a grouped product.
        /// </summary>
        /// <param name="product">Grouped product</param>
        /// <param name="customer">The customer</param>
        /// <param name="context">Object with cargo data for better performance</param>
        /// <param name="associatedProducts">Products associated to product</param>
        /// <param name="lowestPriceProduct">The associated product with the lowest price</param>
        /// <returns>The lowest price.</returns>
        decimal? GetLowestPrice(Product product, Customer customer, PriceCalculationContext context, IEnumerable<Product> associatedProducts, out Product lowestPriceProduct);

        /// <summary>
        /// Get the initial price including preselected attributes
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="currency">The currency</param>
        /// <param name="context">Object with cargo data for better performance</param>
        /// <returns>Preselected price</returns>
        decimal GetPreselectedPrice(Product product, Customer customer, Currency currency, PriceCalculationContext context);

        /// <summary>
        /// Gets the product cost
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Shopping cart item attributes in XML</param>
        /// <returns>Product cost</returns>
        decimal GetProductCost(Product product, string attributesXml);

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(Product product);

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(Product product,
            Customer customer);

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(Product product,
            Customer customer,
            decimal additionalCharge);

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(Product product,
            Customer customer,
            decimal additionalCharge,
            out Discount appliedDiscount);

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="quantity">Product quantity</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="bundleItem">A product bundle item</param>
        /// <param name="context">Object with cargo data for better performance</param>
        /// <param name="finalPrice">Final product price without discount.</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(Product product,
            Customer customer,
            decimal additionalCharge,
            int quantity,
            out Discount appliedDiscount,
            ProductBundleItemData bundleItem = null,
            PriceCalculationContext context = null,
            decimal? finalPrice = null);


        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart item sub total</returns>
		decimal GetSubTotal(OrganizedShoppingCartItem shoppingCartItem, bool includeDiscounts);

        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart unit price (one item)</returns>
		decimal GetUnitPrice(OrganizedShoppingCartItem shoppingCartItem, bool includeDiscounts);




        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <returns>Discount amount</returns>
        decimal GetDiscountAmount(OrganizedShoppingCartItem shoppingCartItem);

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
		decimal GetDiscountAmount(OrganizedShoppingCartItem shoppingCartItem, out Discount appliedDiscount);


        /// <summary>
        /// Gets the price adjustment of a variant attribute value
        /// </summary>
        /// <param name="attributeValue">Product variant attribute value</param>
        /// <returns>Price adjustment of a variant attribute value</returns>
        decimal GetProductVariantAttributeValuePriceAdjustment(ProductVariantAttributeValue attributeValue,
            Product product, Customer customer, PriceCalculationContext context, int productQuantity = 1);
    }
}
