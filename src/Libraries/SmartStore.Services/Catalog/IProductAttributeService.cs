using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;

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
        /// Gets all product attributes.
        /// </summary>
        /// <param name="pageIndex">Page index,</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="untracked">Indicates whether to load entities tracked or untracked.</param>
        /// <returns>Product attributes.</returns>
        IPagedList<ProductAttribute> GetAllProductAttributes(int pageIndex, int pageSize, bool untracked = true);

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

        /// <summary>
        /// Gets the export mappings for a given field prefix.
        /// </summary>
        /// <param name="fieldPrefix">The export field prefix, e.g. gmc.</param>
        /// <returns>A multimap with export field names to ProductAttribute.Id mappings.</returns>
        Multimap<string, int> GetExportFieldMappings(string fieldPrefix);

        #endregion

        #region Product attribute options

        /// <summary>
        /// Gets an attribute option by id
        /// </summary>
        /// <param name="id">Product attribute option identifier</param>
        /// <returns>Product attribute option</returns>
        ProductAttributeOption GetProductAttributeOptionById(int id);

        /// <summary>
        /// Gets all attribute options by options set identifier
        /// </summary>
        /// <param name="optionsSetId">Attribute options set identifier</param>
        /// <returns>List of attribute options</returns>
        IList<ProductAttributeOption> GetProductAttributeOptionsByOptionsSetId(int optionsSetId);

        /// <summary>
        /// Gets all attribute options by attribute identifier
        /// </summary>
        /// <param name="attributeId">Attribute identifier</param>
        /// <returns>List of attribute options</returns>
        IList<ProductAttributeOption> GetProductAttributeOptionsByAttributeId(int attributeId);

        /// <summary>
        /// Deletes an attribute option
        /// </summary>
        /// <param name="productAttributeOption">Product attribute option</param>
        void DeleteProductAttributeOption(ProductAttributeOption productAttributeOption);

        /// <summary>
        /// Inserts an attribute option
        /// </summary>
        /// <param name="productAttributeOption">Product attribute option</param>
        void InsertProductAttributeOption(ProductAttributeOption productAttributeOption);

        /// <summary>
        /// Updates an attribute option
        /// </summary>
        /// <param name="productAttributeOption">Product attribute option</param>
        void UpdateProductAttributeOption(ProductAttributeOption productAttributeOption);

        #endregion

        #region Product attribute options sets

        /// <summary>
        /// Gets an attribute options set by id
        /// </summary>
        /// <param name="id">Product attribute options set identifier</param>
        /// <returns>Product attribute options set</returns>
        ProductAttributeOptionsSet GetProductAttributeOptionsSetById(int id);

        /// <summary>
        /// Gets all attribute options sets by attribute identifier
        /// </summary>
        /// <param name="productAttributeId"></param>
        /// <returns></returns>
        IList<ProductAttributeOptionsSet> GetProductAttributeOptionsSetsByAttributeId(int productAttributeId);

        /// <summary>
        /// Deletes an attribute options set
        /// </summary>
        /// <param name="productAttributeOptionsSet">Product attribute options set</param>
        void DeleteProductAttributeOptionsSet(ProductAttributeOptionsSet productAttributeOptionsSet);

        /// <summary>
        /// Inserts an attribute options set
        /// </summary>
        /// <param name="productAttributeOptionsSet">Product attribute options set</param>
        void InsertProductAttributeOptionsSet(ProductAttributeOptionsSet productAttributeOptionsSet);

        /// <summary>
        /// Updates an attribute options set
        /// </summary>
        /// <param name="productAttributeOptionsSet">Product attribute options set</param>
        void UpdateProductAttributeOptionsSet(ProductAttributeOptionsSet productAttributeOptionsSet);

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
        /// Gets product variant attribute mappings by multiple product identifiers
        /// </summary>
        /// <param name="productIds">The product identifiers</param>
        /// <param name="controlType">An optional control type filter. <c>null</c> loads all controls regardless of type.</param>
        /// <returns>A map with product id as key and a collection of variant attributes as value.</returns>
        Multimap<int, ProductVariantAttribute> GetProductVariantAttributesByProductIds(int[] productIds, AttributeControlType? controlType);

        /// <summary>
        /// Gets a product variant attribute mapping
        /// </summary>
        /// <param name="productVariantAttributeId">Product variant attribute mapping identifier</param>
        /// <returns>Product variant attribute mapping</returns>
        ProductVariantAttribute GetProductVariantAttributeById(int productVariantAttributeId);

        /// <summary>
        /// Gets product variant attribute mappings
        /// </summary>
        /// <param name="productVariantAttributeIds">Enumerable of product variant attribute mapping identifiers</param>
        /// <param name="attributes">Collection of already loaded product attribute mappings to reduce database round trips</param>
        /// <returns></returns>
        IList<ProductVariantAttribute> GetProductVariantAttributesByIds(IEnumerable<int> productVariantAttributeIds, IEnumerable<ProductVariantAttribute> attributes = null);

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

        /// <summary>
        /// Copies attribute options (if any) to product variant attribute values. Existing values are ignored (identified by name field).
        /// </summary>
        /// <param name="productVariantAttribute">The product variant attribute mapping</param>
        /// <param name="productAttributeOptionsSetId">Product attribute options set identifier</param>
        /// <param name="deleteExistingValues">Indicates whether to delete all existing product variant attribute values</param>
        /// <returns>Number of inserted product variant attribute values</returns>
        int CopyAttributeOptions(ProductVariantAttribute productVariantAttribute, int productAttributeOptionsSetId, bool deleteExistingValues);

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
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="untracked">Specifies whether loaded entities should be tracked by the state manager</param>
        /// <returns>Product variant attribute combination collection</returns>
        IPagedList<ProductVariantAttributeCombination> GetAllProductVariantAttributeCombinations(int productId, int pageIndex, int pageSize, bool untracked = true);

        /// <summary>
        /// Gets a distinct list of picture identifiers. 
        /// Only pictures that are explicitly assigned to combinations are taken into account.
        /// </summary>
        /// <param name="productId">Product id</param>
        /// <returns>Picture ids</returns>
        IList<int> GetAllProductVariantAttributeCombinationPictureIds(int productId);

        /// <summary>
        /// Gets product variant attribute combinations by multiple product identifiers
        /// </summary>
        /// <param name="productIds">The product identifiers</param>
        /// <returns>A map with product id as key and a collection of product variant attribute combinations as value.</returns>
        Multimap<int, ProductVariantAttributeCombination> GetProductVariantAttributeCombinations(int[] productIds);

        /// <summary>
        /// Get the lowest price of all combinations for a product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Lowest price</returns>
        decimal? GetLowestCombinationPrice(int productId);

        /// <summary>
		/// Gets a product variant attribute combination by identifier
        /// </summary>
        /// <param name="productVariantAttributeCombinationId">Product variant attribute combination identifier</param>
        /// <returns>Product variant attribute combination</returns>
        ProductVariantAttributeCombination GetProductVariantAttributeCombinationById(int productVariantAttributeCombinationId);

        /// <summary>
        /// Gets a product variant attribute combination by SKU
        /// </summary>
        /// <param name="sku">SKU</param>
        /// <returns>Product variant attribute combination</returns>
        ProductVariantAttributeCombination GetProductVariantAttributeCombinationBySku(string sku);

        /// <summary>
        /// Gets a product variant attribute combination by GTIN.
        /// </summary>
        /// <param name="gtin">GTIN.</param>
        /// <returns>Product variant attribute combination.</returns>
        ProductVariantAttributeCombination GetAttributeCombinationByGtin(string gtin);

        /// <summary>
        /// Gets a product variant attribute combination by manufacturer part number.
        /// </summary>
        /// <param name="manufacturerPartNumber">Manufacturer part number.</param>
        /// <returns>Product variant attribute combination.</returns>
        ProductVariantAttributeCombination GetAttributeCombinationByMpn(string manufacturerPartNumber);

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

    public static class IProductAttributeServiceExtensions
    {
        /// <summary>
        /// Gets all product variant attribute combinations
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Product variant attribute combination collection</returns>
        public static IList<ProductVariantAttributeCombination> GetAllProductVariantAttributeCombinations(this IProductAttributeService service, int productId)
        {
            return service.GetAllProductVariantAttributeCombinations(productId, 0, int.MaxValue, true);
        }
    }
}
