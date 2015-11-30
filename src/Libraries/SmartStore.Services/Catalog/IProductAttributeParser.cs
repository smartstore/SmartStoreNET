using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Collections;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product attribute parser interface
    /// </summary>
    public partial interface IProductAttributeParser
    {
        #region Product attributes

        /// <summary>
        /// Gets selected product variant attributes as a map of integer ids with their corresponding values.
        /// </summary>
		/// <param name="attributesXml">XML formatted attributes</param>
        /// <returns>The deserialized map</returns>
        Multimap<int, string> DeserializeProductVariantAttributes(string attributesXml);

		/// <summary>
		/// Gets selected product variant attributes
		/// </summary>
		/// <param name="attributesXml">XML formatted attributes</param>
		/// <returns>Selected product variant attributes</returns>
		IList<ProductVariantAttribute> ParseProductVariantAttributes(string attributesXml);

        /// <summary>
        /// Get product variant attribute values
        /// </summary>
		/// <param name="attributesXml">XML formatted attributes</param>
        /// <returns>Product variant attribute values</returns>
        IEnumerable<ProductVariantAttributeValue> ParseProductVariantAttributeValues(string attributesXml);

        /// <summary>
        /// Gets selected product variant attribute value
        /// </summary>
		/// <param name="attributesXml">XML formatted attributes</param>
        /// <param name="productVariantAttributeId">Product variant attribute identifier</param>
        /// <returns>Product variant attribute value</returns>
        IList<string> ParseValues(string attributesXml, int productVariantAttributeId);

        /// <summary>
        /// Adds an attribute
        /// </summary>
		/// <param name="attributesXml">XML formatted attributes</param>
        /// <param name="pva">Product variant attribute</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        string AddProductAttribute(string attributesXml, ProductVariantAttribute pva, string value);

        /// <summary>
        /// Are attributes equal
        /// </summary>
        /// <param name="attributeXml1">The attributes of the first product</param>
        /// <param name="attributeXml2">The attributes of the second product</param>
		/// <param name="attributes">Collection of already loaded product attribute mappings to reduce database round trips</param>
        /// <returns>Result</returns>
		bool AreProductAttributesEqual(string attributeXml1, string attributeXml2, IEnumerable<ProductVariantAttribute> attributes = null);

        /// <summary>
        /// Finds a product variant attribute combination by attributes stored in XML 
        /// </summary>
		/// <param name="product">Product</param>
		/// <param name="attributesXml">XML formatted attributes</param>
        /// <returns>Found product variant attribute combination</returns>
		ProductVariantAttributeCombination FindProductVariantAttributeCombination(Product product, string attributesXml);

		/// <summary>
		/// Finds a product variant attribute combination by attributes stored in XML 
		/// </summary>
		/// <param name="productId">Product identifier</param>
		/// <param name="attributesXml">XML formatted attributes</param>
		/// <returns>Found product variant attribute combination</returns>
		ProductVariantAttributeCombination FindProductVariantAttributeCombination(int productId, string attributesXml);

		/// <summary>
		/// Deserializes attribute data from an URL query string
		/// </summary>
		/// <param name="jsonData">Json data query string</param>
		/// <returns>List items with following structure: Product.Id, ProductAttribute.Id, Product_ProductAttribute_Mapping.Id, ProductVariantAttributeValue.Id</returns>
		List<List<int>> DeserializeQueryData(string jsonData);

		/// <summary>
		/// Serializes attribute data
		/// </summary>
		/// <param name="productId">Product identifier</param>
		/// <param name="attributesXml">XML formatted attributes</param>
		/// <param name="urlEncode">Whether to URL encode</param>
		/// <returns>Json string with attribute data</returns>
		string SerializeQueryData(int productId, string attributesXml, bool urlEncode = true);

        #endregion

        #region Gift card attributes

        /// <summary>
        /// Add gift card attrbibutes
        /// </summary>
		/// <param name="attributesXml">XML formatted attributes</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        /// <returns>Attributes</returns>
        string AddGiftCardAttribute(string attributesXml, string recipientName,
            string recipientEmail, string senderName, string senderEmail, string giftCardMessage);

        /// <summary>
        /// Get gift card attrbibutes
        /// </summary>
		/// <param name="attributesXml">XML formatted attributes</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        void GetGiftCardAttribute(string attributesXml, out string recipientName,
            out string recipientEmail, out string senderName,
            out string senderEmail, out string giftCardMessage);

        #endregion
    }
}
