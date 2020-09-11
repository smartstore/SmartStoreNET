using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product attribute formatter interface
    /// </summary>
    public partial interface IProductAttributeFormatter
    {
        /// <summary>
        /// Formats attributes.
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="attributes">Attributes</param>
        /// <returns>Formatted attributes.</returns>
		string FormatAttributes(Product product, string attributes);

        /// <summary>
        /// Formats attributes.
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributes">Attributes</param>
        /// <param name="customer">Customer</param>
        /// <param name="separator">Separator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <param name="renderPrices">A value indicating whether to render prices</param>
        /// <param name="renderProductAttributes">A value indicating whether to render product attributes</param>
        /// <param name="renderGiftCardAttributes">A value indicating whether to render gift card attributes</param>
        /// <param name="allowHyperlinks">A value indicating whether to render HTML hyperlinks</param>
        /// <returns>Formatted attributes.</returns>
        string FormatAttributes(
            Product product,
            string attributes,
            Customer customer,
            string separator = "<br />",
            bool htmlEncode = true,
            bool renderPrices = true,
            bool renderProductAttributes = true,
            bool renderGiftCardAttributes = true,
            bool allowHyperlinks = true);
    }
}
