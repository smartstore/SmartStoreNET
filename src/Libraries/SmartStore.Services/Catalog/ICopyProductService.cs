
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Copy product service
    /// </summary>
    public partial interface ICopyProductService
    {
		/// <summary>
		/// Create a copy of product with all depended data
		/// </summary>
		/// <param name="product">The product to copy</param>
		/// <param name="newName">The name of product duplicate</param>
		/// <param name="isPublished">A value indicating whether the product duplicate should be published</param>
		/// <param name="copyImages">A value indicating whether the product images should be copied</param>
		/// <param name="copyAssociatedProducts">A value indicating whether the copy associated products</param>
		/// <returns>Product copy</returns>
		Product CopyProduct(Product product, string newName, bool isPublished, bool copyImages, bool copyAssociatedProducts = true);
    }
}
