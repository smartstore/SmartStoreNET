using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Specification attribute service interface
    /// </summary>
    public partial interface ISpecificationAttributeService
    {
        #region Specification attribute

        /// <summary>
        /// Gets a specification attribute
        /// </summary>
        /// <param name="specificationAttributeId">The specification attribute identifier</param>
        /// <returns>Specification attribute</returns>
        SpecificationAttribute GetSpecificationAttributeById(int specificationAttributeId);

        /// <summary>
        /// Gets specification attributes
        /// </summary>
		/// <returns>Specification attribute query</returns>
        IQueryable<SpecificationAttribute> GetSpecificationAttributes();

        /// <summary>
        /// Gets specification attributes by identifier
        /// </summary>
        /// <param name="ids">Identifiers</param>
        /// <returns>Specification attribute query</returns>
        IQueryable<SpecificationAttribute> GetSpecificationAttributesByIds(int[] ids);

        /// <summary>
        /// Deletes a specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        void DeleteSpecificationAttribute(SpecificationAttribute specificationAttribute);

        /// <summary>
        /// Inserts a specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        void InsertSpecificationAttribute(SpecificationAttribute specificationAttribute);

        /// <summary>
        /// Updates the specification attribute
        /// </summary>
        /// <param name="specificationAttribute">The specification attribute</param>
        void UpdateSpecificationAttribute(SpecificationAttribute specificationAttribute);

        #endregion

        #region Specification attribute option

        /// <summary>
        /// Gets a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        /// <returns>Specification attribute option</returns>
        SpecificationAttributeOption GetSpecificationAttributeOptionById(int specificationAttributeOption);

        /// <summary>
        /// Gets specification attribute options by specification attribute id
        /// </summary>
        /// <param name="specificationAttributeId">The specification attribute identifier</param>
        /// <returns>Specification attribute option</returns>
        IList<SpecificationAttributeOption> GetSpecificationAttributeOptionsBySpecificationAttribute(int specificationAttributeId);

        /// <summary>
        /// Gets specification attribute options by specification attribute id
        /// </summary>
        /// <param name="specificationAttributeIds">Specification attribute identifiers</param>
        /// <returns>Map of specification attribute options</returns>
        Multimap<int, SpecificationAttributeOption> GetSpecificationAttributeOptionsBySpecificationAttributeIds(int[] specificationAttributeIds);

        /// <summary>
        /// Deletes a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        void DeleteSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption);

        /// <summary>
        /// Inserts a specification attribute option
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        void InsertSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption);

        /// <summary>
        /// Updates the specification attribute
        /// </summary>
        /// <param name="specificationAttributeOption">The specification attribute option</param>
        void UpdateSpecificationAttributeOption(SpecificationAttributeOption specificationAttributeOption);

        #endregion

        #region Product specification attribute

        /// <summary>
        /// Deletes a product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute</param>
        void DeleteProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);

        /// <summary>
        /// Gets a product specification attribute mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Product specification attribute mapping collection</returns>
        IList<ProductSpecificationAttribute> GetProductSpecificationAttributesByProductId(int productId);

        /// <summary>
        /// Gets a product specification attribute mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="allowFiltering">0 to load attributes with AllowFiltering set to false, 0 to load attributes with AllowFiltering set to true, null to load all attributes</param>
        /// <param name="showOnProductPage">0 to load attributes with ShowOnProductPage set to false, 0 to load attributes with ShowOnProductPage set to true, null to load all attributes</param>
        /// <returns>Product specification attribute mapping collection</returns>
        IList<ProductSpecificationAttribute> GetProductSpecificationAttributesByProductId(int productId,
            bool? allowFiltering, bool? showOnProductPage);

        /// <summary>
        /// Gets a product specification attribute mapping 
        /// </summary>
        /// <param name="productSpecificationAttributeId">Product specification attribute mapping identifier</param>
        /// <returns>Product specification attribute mapping</returns>
        ProductSpecificationAttribute GetProductSpecificationAttributeById(int productSpecificationAttributeId);

        /// <summary>
        /// Get product specification attributes by product identifiers
        /// </summary>
        /// <param name="productIds">Product identifiers</param>
        /// <returns>Map of product specification attributes</returns>
        Multimap<int, ProductSpecificationAttribute> GetProductSpecificationAttributesByProductIds(int[] productIds);

        /// <summary>
        /// Inserts a product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute mapping</param>
        void InsertProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);

        /// <summary>
        /// Updates the product specification attribute mapping
        /// </summary>
        /// <param name="productSpecificationAttribute">Product specification attribute mapping</param>
        void UpdateProductSpecificationAttribute(ProductSpecificationAttribute productSpecificationAttribute);

        #endregion
    }
}
