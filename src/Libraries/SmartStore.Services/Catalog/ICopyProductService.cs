using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Copy product service.
    /// </summary>
    public partial interface ICopyProductService
    {
        /// <summary>
        /// Creates a duplicate of a product with all it's dependencies.
        /// </summary>
        /// <param name="product">The product to copy.</param>
        /// <param name="newName">The name of the product duplicate.</param>
        /// <param name="isPublished">A value indicating whether the product duplicate should be published.</param>
        /// <param name="copyAssociatedProducts">A value indicating whether to copy associated products.</param>
        /// <returns>Product duplicate.</returns>
        Product CopyProduct(
            Product product,
            string newName,
            bool isPublished,
            bool copyAssociatedProducts = true);
    }
}
