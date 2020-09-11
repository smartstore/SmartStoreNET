using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product tag service interface
    /// </summary>
    public partial interface IProductTagService : IScopedService
    {
        /// <summary>
        /// Delete a product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        void DeleteProductTag(ProductTag productTag);

        /// <summary>
        /// Gets all product tags
        /// </summary>
        /// <param name="includeHidden">Whether to include hidden product tags.</param>
        /// <returns>Product tags</returns>
        IList<ProductTag> GetAllProductTags(bool includeHidden = false);

        /// <summary>
        /// Gets all product tag names
        /// </summary>
        /// <returns>Product tag names as list</returns>
        IList<string> GetAllProductTagNames();

        /// <summary>
        /// Gets product tag
        /// </summary>
        /// <param name="productTagId">Product tag identifier</param>
        /// <returns>Product tag</returns>
        ProductTag GetProductTagById(int productTagId);

        /// <summary>
        /// Gets product tag by name
        /// </summary>
        /// <param name="name">Product tag name</param>
        /// <returns>Product tag</returns>
        ProductTag GetProductTagByName(string name);

        /// <summary>
        /// Inserts a product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        void InsertProductTag(ProductTag productTag);

        /// <summary>
        /// Updates the product tag
        /// </summary>
        /// <param name="productTag">Product tag</param>
        void UpdateProductTag(ProductTag productTag);

        /// <summary>
        /// Updates the product tags.
        /// </summary>
        /// <param name="product">Product.</param>
        /// <param name="tagNames">New tags for the product.</param>
        void UpdateProductTags(Product product, string[] tagNames);

        /// <summary>
        /// Get number of products
        /// </summary>
        /// <param name="productTagId">Product tag identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="includeHidden">Whether to include hidden product tags.</param>
        /// <returns>Number of products</returns>
        int GetProductCount(int productTagId, int storeId, bool includeHidden = false);
    }
}
