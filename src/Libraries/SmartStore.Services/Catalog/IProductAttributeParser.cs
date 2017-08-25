using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
	/// <summary>
	/// Product attribute parser interface
	/// </summary>
	public partial interface IProductAttributeParser
    {
		#region Product attributes

		/// <summary>
		/// Parses, prefetches & caches all passed attribute definitions for the current request
		/// </summary>
		/// <param name="attributesXml">All attribute definitions to prefetch</param>
		void PrefetchProductVariantAttributes(IEnumerable<string> attributesXml);

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
		/// Get list of product variant attribute values
		/// </summary>
		/// <param name="attributeCombination">Map of combined attributes</param>
		/// <param name="attributes">Product variant attributes</param>
		/// <returns>Collection of product variant attribute values</returns>
		ICollection<ProductVariantAttributeValue> ParseProductVariantAttributeValues(Multimap<int, string> attributeCombination, IEnumerable<ProductVariantAttribute> attributes);

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
		/// Creates formatted xml for a list of product variant attribute values
		/// </summary>
		/// <param name="attributes">The attributes map</param>
		/// <returns>Attributes XML</returns>
		string CreateAttributesXml(Multimap<int, string> attributes);

		/// <summary>
		/// Are attributes equal
		/// </summary>
		/// <param name="attributeXml1">The attributes of the first product</param>
		/// <param name="attributeXml2">The attributes of the second product</param>
		/// <returns>Result</returns>
		bool AreProductAttributesEqual(string attributeXml1, string attributeXml2);

		/// <summary>
		/// Finds a product variant attribute combination by attributes stored in XML 
		/// </summary>
		/// <param name="productId">Product identifier</param>
		/// <param name="attributesXml">XML formatted attributes</param>
		/// <returns>Found product variant attribute combination</returns>
		ProductVariantAttributeCombination FindProductVariantAttributeCombination(int productId, string attributesXml);

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
