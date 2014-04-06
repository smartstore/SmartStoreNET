using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product attribute service interface
    /// </summary>
    public partial interface IProductAttributeService
    {
        #region Product attributes

        /// <summary>
        /// Deletes a product attribute
        /// </summary>
        /// <param name="productAttribute">Product attribute</param>
        void DeleteProductAttribute(ProductAttribute productAttribute);

        /// <summary>
        /// Gets all product attributes
        /// </summary>
        /// <returns>Product attribute collection</returns>
        IList<ProductAttribute> GetAllProductAttributes();

        /// <summary>
        /// Gets a product attribute 
        /// </summary>
        /// <param name="productAttributeId">Product attribute identifier</param>
        /// <returns>Product attribute </returns>
        ProductAttribute GetProductAttributeById(int productAttributeId);

        /// <summary>
        /// Inserts a product attribute
        /// </summary>
        /// <param name="productAttribute">Product attribute</param>
        void InsertProductAttribute(ProductAttribute productAttribute);

        /// <summary>
        /// Updates the product attribute
        /// </summary>
        /// <param name="productAttribute">Product attribute</param>
        void UpdateProductAttribute(ProductAttribute productAttribute);

        #endregion

        #region Product variant attributes mappings (ProductVariantAttribute)

        /// <summary>
        /// Deletes a product variant attribute mapping
        /// </summary>
        /// <param name="productVariantAttribute">Product variant attribute mapping</param>
        void DeleteProductVariantAttribute(ProductVariantAttribute productVariantAttribute);

        /// <summary>
        /// Gets product variant attribute mappings by product identifier
        /// </summary>
		/// <param name="productId">The product identifier</param>
        /// <returns>Product variant attribute mapping collection</returns>
		IList<ProductVariantAttribute> GetProductVariantAttributesByProductId(int productId);

        /// <summary>
        /// Gets a product variant attribute mapping
        /// </summary>
        /// <param name="productVariantAttributeId">Product variant attribute mapping identifier</param>
        /// <returns>Product variant attribute mapping</returns>
        ProductVariantAttribute GetProductVariantAttributeById(int productVariantAttributeId);

        // codehint: sm-add
        /// <summary>
        /// Gets multiple product variant attribute mappings by their keys
        /// </summary>
        /// <param name="ids">a list of keys</param>
        /// <returns>Product variant attribute mappings</returns>
        IEnumerable<ProductVariantAttribute> GetProductVariantAttributesByIds(params int[] ids);

        /// <summary>
        /// Inserts a product variant attribute mapping
        /// </summary>
        /// <param name="productVariantAttribute">The product variant attribute mapping</param>
        void InsertProductVariantAttribute(ProductVariantAttribute productVariantAttribute);

        /// <summary>
        /// Updates the product variant attribute mapping
        /// </summary>
        /// <param name="productVariantAttribute">The product variant attribute mapping</param>
        void UpdateProductVariantAttribute(ProductVariantAttribute productVariantAttribute);

        #endregion

        #region Product variant attribute values (ProductVariantAttributeValue)

        /// <summary>
        /// Deletes a product variant attribute value
        /// </summary>
        /// <param name="productVariantAttributeValue">Product variant attribute value</param>
        void DeleteProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue);

        /// <summary>
        /// Gets product variant attribute values by product identifier
        /// </summary>
        /// <param name="productVariantAttributeId">The product variant attribute mapping identifier</param>
        /// <returns>Product variant attribute mapping collection</returns>
        IList<ProductVariantAttributeValue> GetProductVariantAttributeValues(int productVariantAttributeId);

        /// <summary>
        /// Gets a product variant attribute value
        /// </summary>
        /// <param name="productVariantAttributeValueId">Product variant attribute value identifier</param>
        /// <returns>Product variant attribute value</returns>
        ProductVariantAttributeValue GetProductVariantAttributeValueById(int productVariantAttributeValueId);

        // codehint: sm-add
        /// <summary>
        /// Gets multiple product variant attribute value
        /// </summary>
        /// <param name="productVariantAttributeValueIds">Product variant attribute value identifiers</param>
        /// <returns>List of Product variant attribute values</returns>
        IEnumerable<ProductVariantAttributeValue> GetProductVariantAttributeValuesByIds(params int[] productVariantAttributeValueIds);

        /// <summary>
        /// Inserts a product variant attribute value
        /// </summary>
        /// <param name="productVariantAttributeValue">The product variant attribute value</param>
        void InsertProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue);

        /// <summary>
        /// Updates the product variant attribute value
        /// </summary>
        /// <param name="productVariantAttributeValue">The product variant attribute value</param>
        void UpdateProductVariantAttributeValue(ProductVariantAttributeValue productVariantAttributeValue);

        #endregion

        #region Product variant attribute combinations (ProductVariantAttributeCombination)

        /// <summary>
        /// Deletes a product variant attribute combination
        /// </summary>
        /// <param name="combination">Product variant attribute combination</param>
        void DeleteProductVariantAttributeCombination(ProductVariantAttributeCombination combination);

        /// <summary>
        /// Gets all product variant attribute combinations
        /// </summary>
		/// <param name="productId">Product identifier</param>
        /// <returns>Product variant attribute combination collection</returns>
        IList<ProductVariantAttributeCombination> GetAllProductVariantAttributeCombinations(int productId);

		/// <summary>
		/// Get the lowest price of all combinations for a product
		/// </summary>
		/// <param name="productId">Product identifier</param>
		/// <returns>Lowest price</returns>
		decimal? GetLowestCombinationPrice(int productId);

        /// <summary>
        /// Gets a product variant attribute combination
        /// </summary>
        /// <param name="productVariantAttributeCombinationId">Product variant attribute combination identifier</param>
        /// <returns>Product variant attribute combination</returns>
        ProductVariantAttributeCombination GetProductVariantAttributeCombinationById(int productVariantAttributeCombinationId);

        /// <summary>
        /// Inserts a product variant attribute combination
        /// </summary>
        /// <param name="combination">Product variant attribute combination</param>
        void InsertProductVariantAttributeCombination(ProductVariantAttributeCombination combination);

        /// <summary>
        /// Updates a product variant attribute combination
        /// </summary>
        /// <param name="combination">Product variant attribute combination</param>
        void UpdateProductVariantAttributeCombination(ProductVariantAttributeCombination combination);

		/// <summary>
		/// Creates all variant attributes combinations
		/// </summary>
		void CreateAllProductVariantAttributeCombinations(Product product);

        /// <summary>
        /// Gets a value indicating the existence of any attribute combination for a product
        /// </summary>
        bool VariantHasAttributeCombinations(int productId);

        #endregion

		#region Product bundle item attribute filter

		/// <summary>
		/// Inserts a product bundle item attribute filter
		/// </summary>
		/// <param name="attributeFilter">Product bundle item attribute filter</param>
		void InsertProductBundleItemAttributeFilter(ProductBundleItemAttributeFilter attributeFilter);

		/// <summary>
		/// Updates the product bundle item attribute filter
		/// </summary>
		/// <param name="attributeFilter">Product bundle item attribute filter</param>
		void UpdateProductBundleItemAttributeFilter(ProductBundleItemAttributeFilter attributeFilter);

		/// <summary>
		/// Deletes a product bundle item attribute filter
		/// </summary>
		/// <param name="attributeFilter">Product bundle item attribute filter</param>
		void DeleteProductBundleItemAttributeFilter(ProductBundleItemAttributeFilter attributeFilter);

		/// <summary>
		/// Deletes all attribute filters of a bundle item
		/// </summary>
		/// <param name="bundleItem">Bundle item</param>
		void DeleteProductBundleItemAttributeFilter(ProductBundleItem bundleItem);

		/// <summary>
		/// Deletes product bundle item attribute filters
		/// </summary>
		/// <param name="attributeId">Attribute identifier</param>
		/// <param name="attributeValueId">Attribute value identifier</param>
		void DeleteProductBundleItemAttributeFilter(int attributeId, int attributeValueId);

		/// <summary>
		/// Deletes product bundle item attribute filters
		/// </summary>
		/// <param name="attributeId">Attribute identifier</param>
		void DeleteProductBundleItemAttributeFilter(int attributeId);

		#endregion
    }
}
