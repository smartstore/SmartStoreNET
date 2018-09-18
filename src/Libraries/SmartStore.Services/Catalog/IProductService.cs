using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial interface IProductService
    {
		#region Products

		/// <summary>
		/// Delete a product
		/// </summary>
		/// <param name="product">Product</param>
		void DeleteProduct(Product product);

        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        /// <returns>Product collection</returns>
        IList<Product> GetAllProductsDisplayedOnHomePage();
        
        /// <summary>
        /// Gets product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Product</returns>
        Product GetProductById(int productId);

        /// <summary>
        /// Gets products by identifier
        /// </summary>
        /// <param name="productIds">Product identifiers</param>
		/// <param name="flags">Which navigation properties to eager load</param>
        /// <returns>Products</returns>
        IList<Product> GetProductsByIds(int[] productIds, ProductLoadFlags flags = ProductLoadFlags.None);

		/// <summary>
		/// Get product by system name.
		/// </summary>
		/// <param name="systemName">System name</param>
		/// <returns>Product entity.</returns>
		Product GetProductBySystemName(string systemName);

		/// <summary>
		/// Inserts a product
		/// </summary>
		/// <param name="product">Product</param>
		void InsertProduct(Product product);

        /// <summary>
        /// Updates the product
        /// </summary>
        /// <param name="product">Product</param>
		void UpdateProduct(Product product);

        /// <summary>
        /// Update product review totals
        /// </summary>
        /// <param name="product">Product</param>
        void UpdateProductReviewTotals(Product product);
        
        /// <summary>
        /// Get low stock products
        /// </summary>
        /// <returns>Result</returns>
        IList<Product> GetLowStockProducts();

        /// <summary>
        /// Gets a product by SKU
        /// </summary>
        /// <param name="sku">SKU</param>
        /// <returns>Product</returns>
        Product GetProductBySku(string sku);

        /// <summary>
        /// Gets a product by GTIN
        /// </summary>
		/// <param name="gtin">GTIN</param>
        /// <returns>Product</returns>
		Product GetProductByGtin(string gtin);

		/// <summary>
		/// Gets a product by manufacturer part number (MPN)
		/// </summary>
		/// <param name="manufacturerPartNumber">Manufacturer part number</param>
		/// <returns>Product</returns>
		Product GetProductByManufacturerPartNumber(string manufacturerPartNumber);

		/// <summary>
		/// Gets a product by name
		/// </summary>
		/// <param name="name">Product name</param>
		/// <returns>Product</returns>
		Product GetProductByName(string name);

		/// <summary>
		/// Adjusts inventory
		/// </summary>
		/// <param name="sci">Shopping cart item</param>
		/// <param name="decrease">A value indicating whether to increase or descrease product stock quantity</param>
		/// <returns>Adjust inventory result</returns>
		AdjustInventoryResult AdjustInventory(OrganizedShoppingCartItem sci, bool decrease);

		/// <summary>
		/// Adjusts inventory
		/// </summary>
		/// <param name="orderItem">Order item</param>
		/// <param name="decrease">A value indicating whether to increase or descrease product stock quantity</param>
		/// <param name="quantity">Quantity</param>
		/// <returns>Adjust inventory result</returns>
		AdjustInventoryResult AdjustInventory(OrderItem orderItem, bool decrease, int quantity);

        /// <summary>
        /// Adjusts inventory
        /// </summary>
		/// <param name="product">Product</param>
		/// <param name="decrease">A value indicating whether to increase or descrease product stock quantity</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="attributesXml">Attributes in XML format</param>
		/// <returns>Adjust inventory result</returns>
		AdjustInventoryResult AdjustInventory(Product product, bool decrease, int quantity, string attributesXml);

        /// <summary>
        /// Update HasTierPrices property (used for performance optimization)
        /// </summary>
		/// <param name="product">Product</param>
        void UpdateHasTierPricesProperty(Product product);

		/// <summary>
		/// Update LowestAttributeCombinationPrice property (used for performance optimization)
		/// </summary>
		/// <param name="product">Product</param>
		void UpdateLowestAttributeCombinationPriceProperty(Product product);

        /// <summary>
        /// Update HasDiscountsApplied property (used for performance optimization)
        /// </summary>
		/// <param name="product">Product</param>
        void UpdateHasDiscountsApplied(Product product);

		/// <summary>
		/// Get product tags by product identifiers
		/// </summary>
		/// <param name="productIds">Product identifiers</param>
		/// <returns>Map of product tags</returns>
		Multimap<int, ProductTag> GetProductTagsByProductIds(int[] productIds);

		/// <summary>
		/// Get applied discounts by product identifiers
		/// </summary>
		/// <param name="productIds">Product identifiers</param>
		/// <returns>Map of applied discounts</returns>
		Multimap<int, Discount> GetAppliedDiscountsByProductIds(int[] productIds);

        #endregion

        #region Related products

        /// <summary>
        /// Deletes a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        void DeleteRelatedProduct(RelatedProduct relatedProduct);

        /// <summary>
        /// Gets a related product collection by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Related product collection</returns>
        IList<RelatedProduct> GetRelatedProductsByProductId1(int productId1, bool showHidden = false);

        /// <summary>
        /// Gets a related product
        /// </summary>
        /// <param name="relatedProductId">Related product identifier</param>
        /// <returns>Related product</returns>
        RelatedProduct GetRelatedProductById(int relatedProductId);

        /// <summary>
        /// Inserts a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        void InsertRelatedProduct(RelatedProduct relatedProduct);

        /// <summary>
        /// Updates a related product
        /// </summary>
        /// <param name="relatedProduct">Related product</param>
        void UpdateRelatedProduct(RelatedProduct relatedProduct);

		/// <summary>
		/// Ensure existence of all mutually related products
		/// </summary>
		/// <param name="productId1">First product identifier</param>
		/// <returns>Number of inserted related products</returns>
		int EnsureMutuallyRelatedProducts(int productId1);

        #endregion

        #region Cross-sell products

        /// <summary>
        /// Deletes a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell</param>
        void DeleteCrossSellProduct(CrossSellProduct crossSellProduct);

        /// <summary>
        /// Gets a cross-sell product collection by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Cross-sell product collection</returns>
        IList<CrossSellProduct> GetCrossSellProductsByProductId1(int productId1, bool showHidden = false);

		/// <summary>
		/// Gets a cross-sell product collection by many product identifiers
		/// </summary>
		/// <param name="productIds">A sequence of alpha product identifiers</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>Cross-sell product collection</returns>
		IList<CrossSellProduct> GetCrossSellProductsByProductIds(IEnumerable<int> productIds, bool showHidden = false);

		/// <summary>
		/// Gets a cross-sell product
		/// </summary>
		/// <param name="crossSellProductId">Cross-sell product identifier</param>
		/// <returns>Cross-sell product</returns>
		CrossSellProduct GetCrossSellProductById(int crossSellProductId);

        /// <summary>
        /// Inserts a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell product</param>
        void InsertCrossSellProduct(CrossSellProduct crossSellProduct);

        /// <summary>
        /// Updates a cross-sell product
        /// </summary>
        /// <param name="crossSellProduct">Cross-sell product</param>
        void UpdateCrossSellProduct(CrossSellProduct crossSellProduct);
        
        /// <summary>
        /// Gets a cross-sells
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="numberOfProducts">Number of products to return</param>
        /// <returns>Cross-sells</returns>
		IList<Product> GetCrosssellProductsByShoppingCart(IList<OrganizedShoppingCartItem> cart, int numberOfProducts);

		/// <summary>
		/// Ensure existence of all mutually cross selling products
		/// </summary>
		/// <param name="productId1">First product identifier</param>
		/// <returns>Number of inserted cross selling products</returns>
		int EnsureMutuallyCrossSellProducts(int productId1);

        #endregion
        
        #region Tier prices

        /// <summary>
        /// Deletes a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        void DeleteTierPrice(TierPrice tierPrice);

        /// <summary>
        /// Gets a tier price
        /// </summary>
        /// <param name="tierPriceId">Tier price identifier</param>
        /// <returns>Tier price</returns>
        TierPrice GetTierPriceById(int tierPriceId);

		/// <summary>
		/// Gets tier prices by product identifiers
		/// </summary>
		/// <param name="productIds">Product identifiers</param>
		/// <param name="customer">Filter tier prices by customer</param>
		/// <param name="storeId">Filter tier prices by store</param>
		/// <returns>Map of tier prices</returns>
		Multimap<int, TierPrice> GetTierPricesByProductIds(int[] productIds, Customer customer = null, int storeId = 0);

        /// <summary>
        /// Inserts a tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        void InsertTierPrice(TierPrice tierPrice);

        /// <summary>
        /// Updates the tier price
        /// </summary>
        /// <param name="tierPrice">Tier price</param>
        void UpdateTierPrice(TierPrice tierPrice);

        #endregion

        #region Product pictures

        /// <summary>
        /// Deletes a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        void DeleteProductPicture(ProductPicture productPicture);

        /// <summary>
        /// Gets a product pictures by product identifier
        /// </summary>
        /// <param name="productId">The product identifier</param>
        /// <returns>Product pictures</returns>
        IList<ProductPicture> GetProductPicturesByProductId(int productId);

		/// <summary>
		/// Get product pictures by product identifiers
		/// </summary>
		/// <param name="productIds">Product identifiers</param>
		/// <param name="onlyFirstPicture">Whether to only load the first picture for each product</param>
		/// <returns>Product pictures</returns>
		Multimap<int, ProductPicture> GetProductPicturesByProductIds(int[] productIds, bool onlyFirstPicture = false);

        /// <summary>
        /// Gets a product picture
        /// </summary>
        /// <param name="productPictureId">Product picture identifier</param>
        /// <returns>Product picture</returns>
        ProductPicture GetProductPictureById(int productPictureId);

        /// <summary>
        /// Inserts a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        void InsertProductPicture(ProductPicture productPicture);

        /// <summary>
        /// Updates a product picture
        /// </summary>
        /// <param name="productPicture">Product picture</param>
        void UpdateProductPicture(ProductPicture productPicture);

        #endregion

		#region Bundled products

		/// <summary>
		/// Inserts a product bundle item
		/// </summary>
		/// <param name="bundleItem">Product bundle item</param>
		void InsertBundleItem(ProductBundleItem bundleItem);

		/// <summary>
		/// Updates a product bundle item
		/// </summary>
		/// <param name="bundleItem">Product bundle item</param>
		void UpdateBundleItem(ProductBundleItem bundleItem);

		/// <summary>
		/// Deletes a product bundle item
		/// </summary>
		/// <param name="bundleItem">Product bundle item</param>
		void DeleteBundleItem(ProductBundleItem bundleItem);

		/// <summary>
		/// Get a product bundle item by item identifier
		/// </summary>
		/// <param name="bundleItemId">Product bundle item identifier</param>
		/// <returns>Product bundle item</returns>
		ProductBundleItem GetBundleItemById(int bundleItemId);

		/// <summary>
		/// Gets a list of bundle items for a particular product identifier
		/// </summary>
		/// <param name="bundleProductId">Product identifier</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>List of bundle items</returns>
		IList<ProductBundleItemData> GetBundleItems(int bundleProductId, bool showHidden = false);

		/// <summary>
		/// Get bundle items by product identifiers
		/// </summary>
		/// <param name="productIds">Product identifiers</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>Map of bundle items</returns>
		Multimap<int, ProductBundleItem> GetBundleItemsByProductIds(int[] productIds, bool showHidden = false);

		#endregion
    }

	[Flags]
	public enum ProductLoadFlags
	{
		None = 0,
		WithCategories = 1 << 0,
		WithManufacturers = 1 << 1,
		WithPictures = 1 << 2,
		WithReviews = 1 << 3,
		WithSpecificationAttributes = 1 << 4,
		WithAttributes = 1 << 5,
		WithAttributeValues = 1 << 7,
		WithAttributeCombinations = 1 << 8,
		WithTags = 1 << 9,
		WithTierPrices = 1 << 10,
		WithDiscounts = 1 << 11,
		WithBundleItems = 1 << 12,
		WithDeliveryTime = 1 << 13,
		Full = WithCategories | WithManufacturers | WithPictures | WithReviews | WithSpecificationAttributes | WithAttributes | WithAttributeValues | WithAttributeCombinations | WithTags | WithTierPrices | WithDiscounts | WithBundleItems | WithDeliveryTime
	}
}
