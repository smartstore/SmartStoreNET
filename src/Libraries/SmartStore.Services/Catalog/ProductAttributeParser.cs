using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Xml;
using Newtonsoft.Json;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product attribute parser
    /// </summary>
    public partial class ProductAttributeParser : IProductAttributeParser
    {
        private readonly IProductAttributeService _productAttributeService;

        public ProductAttributeParser(IProductAttributeService productAttributeService)
        {
            this._productAttributeService = productAttributeService;
        }

		#region Product attributes

		/// <summary>
        /// Gets selected product variant attribute identifiers
        /// </summary>
        /// <param name="attributesXml">Attributes</param>
        /// <returns>Selected product variant attribute identifiers</returns>
        private IEnumerable<int> ParseProductVariantAttributeIds(string attributesXml)
        {
            var ids = new List<int>();
            if (String.IsNullOrEmpty(attributesXml))
                yield break;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodeList = xmlDoc.SelectNodes(@"//Attributes/ProductVariantAttribute");
                foreach (var node in nodeList.Cast<XmlElement>())
                {
                    string sid = node.GetAttribute("ID").Trim();
                    if (sid.HasValue())
                    {
                        int id = 0;
                        if (int.TryParse(sid, out id))
                        {
                            yield return id;
                        }
                    }
                }
            }
            finally { }
        }

        public virtual Multimap<int, string> DeserializeProductVariantAttributes(string attributesXml)
        {
            var attrs = new Multimap<int, string>();
            if (String.IsNullOrEmpty(attributesXml))
                return attrs;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductVariantAttribute");
                foreach (var node1 in nodeList1.Cast<XmlElement>())
                {
                    string sid = node1.GetAttribute("ID").Trim();
                    if (sid.HasValue())
                    {
                        int id = 0;
                        if (int.TryParse(sid, out id))
                        {
                            
                            var nodeList2 = node1.SelectNodes(@"ProductVariantAttributeValue/Value").Cast<XmlElement>();
                            foreach (var node2 in nodeList2)
                            {
                                string value = node2.InnerText.Trim();
                                attrs.Add(id, value);
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return attrs;
        }

        /// <summary>
        /// Gets selected product variant attributes
        /// </summary>
        /// <param name="attributesXml">Attributes</param>
        /// <returns>Selected product variant attributes</returns>
		public virtual IList<ProductVariantAttribute> ParseProductVariantAttributes(string attributesXml)
		{
			var ids = ParseProductVariantAttributeIds(attributesXml);

			return _productAttributeService.GetProductVariantAttributesByIds(ids.ToList());
		}

        public virtual IEnumerable<ProductVariantAttributeValue> ParseProductVariantAttributeValues(string attributeXml)
        {
            //var pvaValues = Enumerable.Empty<ProductVariantAttributeValue>();

			var allIds = new List<int>();
            var attrs = DeserializeProductVariantAttributes(attributeXml);
			var pvaCollection = _productAttributeService.GetProductVariantAttributesByIds(attrs.Keys);

            foreach (var pva in pvaCollection)
            {
                if (!pva.ShouldHaveValues())
                    continue;

                var pvaValuesStr = attrs[pva.Id];

                var ids =
					from id in pvaValuesStr
					where id.HasValue()
					select id.ToInt();

				allIds.AddRange(ids);

                //var values = _productAttributeService.GetProductVariantAttributeValuesByIds(ids.ToArray());
                //pvaValues = pvaValues.Concat(values);
            }

			int[] allDistinctIds = allIds.Distinct().ToArray();

			var values = _productAttributeService.GetProductVariantAttributeValuesByIds(allDistinctIds);

            return values;
        }

        /// <summary>
        /// Gets selected product variant attribute value
        /// </summary>
        /// <param name="attributesXml">Attributes</param>
        /// <param name="productVariantAttributeId">Product variant attribute identifier</param>
        /// <returns>Product variant attribute value</returns>
        public virtual IList<string> ParseValues(string attributesXml, int productVariantAttributeId)
        {
            var selectedProductVariantAttributeValues = new List<string>();
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductVariantAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["ID"] != null)
                    {
                        string str1 = node1.Attributes["ID"].InnerText.Trim();
                        int id = 0;
                        if (int.TryParse(str1, out id))
                        {
                            if (id == productVariantAttributeId)
                            {
                                var nodeList2 = node1.SelectNodes(@"ProductVariantAttributeValue/Value");
                                foreach (XmlNode node2 in nodeList2)
                                {
                                    string value = node2.InnerText.Trim();
                                    selectedProductVariantAttributeValues.Add(value);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return selectedProductVariantAttributeValues;
        }

        /// <summary>
        /// Adds an attribute
        /// </summary>
        /// <param name="attributesXml">Attributes</param>
        /// <param name="pva">Product variant attribute</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        public virtual string AddProductAttribute(string attributesXml, ProductVariantAttribute pva, string value)
        {
			return pva.AddProductAttribute(attributesXml, value);
        }

		public virtual bool AreProductAttributesEqual(string attributeXml1, string attributeXml2, IEnumerable<ProductVariantAttribute> attributes = null)
        {
			if (attributeXml1.IsCaseInsensitiveEqual(attributeXml2))
				return true;

            var attributes1 = DeserializeProductVariantAttributes(attributeXml1);
            var attributes2 = DeserializeProductVariantAttributes(attributeXml2);

			if (attributes1.Count != attributes2.Count)
				return false;

			IEnumerable<ProductVariantAttribute> pvaCollection1 = null;
			IEnumerable<ProductVariantAttribute> pvaCollection2 = null;

			pvaCollection1 = _productAttributeService.GetProductVariantAttributesByIds(attributes2.Keys, attributes);

			if (attributes2.Keys.SequenceEqual(attributes1.Keys))	// often the case
				pvaCollection2 = pvaCollection1;
			else
				pvaCollection2 = _productAttributeService.GetProductVariantAttributesByIds(attributes1.Keys, attributes);

			foreach (var pva1 in pvaCollection1)
			{
				foreach (var pva2 in pvaCollection2)
				{
					if (pva1.Id == pva2.Id)
					{
						var pvaValues1 = attributes2[pva1.Id];
						var pvaValues2 = attributes1[pva2.Id];

						if (pvaValues1.Count != pvaValues2.Count)
							return false;

						foreach (string value1 in pvaValues1)
						{
							string str1 = value1.TrimSafe();

							if (!pvaValues2.Any(x => x.TrimSafe().IsCaseInsensitiveEqual(str1)))
								return false;
						}
					}
				}
			}

            return true;
        }

		public virtual ProductVariantAttributeCombination FindProductVariantAttributeCombination(Product product, string attributesXml)
        {
			if (product == null)
				throw new ArgumentNullException("product");

            return FindProductVariantAttributeCombination(product.Id, attributesXml);
        }

		public virtual ProductVariantAttributeCombination FindProductVariantAttributeCombination(int productId, string attributesXml)
		{
			if (attributesXml.HasValue())
			{
				var combinations = _productAttributeService.GetAllProductVariantAttributeCombinations(productId);
				if (combinations.Count == 0)
					return null;

				foreach (var combination in combinations)
				{
					bool attributesEqual = AreProductAttributesEqual(combination.AttributesXml, attributesXml);
					if (attributesEqual)
						return combination;
				}
			}
			return null;
		}

		/// <summary>
		/// Deserializes attribute data from an URL query string
		/// </summary>
		/// <param name="jsonData">Json data query string</param>
		/// <returns>List items with following structure: Product.Id, ProductAttribute.Id, Product_ProductAttribute_Mapping.Id, ProductVariantAttributeValue.Id</returns>
		public virtual List<List<int>> DeserializeQueryData(string jsonData)
		{
			if (jsonData.HasValue())
			{
				if (jsonData.StartsWith("["))
					return JsonConvert.DeserializeObject<List<List<int>>>(jsonData);

				return new List<List<int>>() { JsonConvert.DeserializeObject<List<int>>(jsonData) };
			}
			return new List<List<int>>();
		}
		
		/// <summary>
		/// Serializes attribute data
		/// </summary>
		/// <param name="productId">Product identifier</param>
		/// <param name="attributesXml">Attribute XML string</param>
		/// <param name="urlEncode">Whether to URL encode</param>
		/// <returns>Json string with attribute data</returns>
		public virtual string SerializeQueryData(int productId, string attributesXml, bool urlEncode = true)
		{
			if (attributesXml.HasValue() && productId != 0)
			{
				var data = new List<List<int>>();
				var attributeValues = ParseProductVariantAttributeValues(attributesXml).ToList();

				foreach (var value in attributeValues)
				{
					data.Add(new List<int>
					{
						productId,
						value.ProductVariantAttribute.ProductAttributeId,
						value.ProductVariantAttributeId,
						value.Id
					});
				}

				if (data.Count > 0)
				{
					string result = JsonConvert.SerializeObject(data);
					return (urlEncode ? HttpUtility.UrlEncode(result) : result);
				}
			}
			return "";
		}

        #endregion

        #region Gift card attributes

        /// <summary>
        /// Add gift card attrbibutes
        /// </summary>
        /// <param name="attributesXml">Attributes</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        /// <returns>Attributes</returns>
        public string AddGiftCardAttribute(string attributesXml, string recipientName,
            string recipientEmail, string senderName, string senderEmail, string giftCardMessage)
        {
            string result = string.Empty;
            try
            {
                recipientName = recipientName.Trim();
                recipientEmail = recipientEmail.Trim();
                senderName = senderName.Trim();
                senderEmail = senderEmail.Trim();

                var xmlDoc = new XmlDocument();
                if (String.IsNullOrEmpty(attributesXml))
                {
                    var element1 = xmlDoc.CreateElement("Attributes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(attributesXml);
                }

                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

                var giftCardElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo");
                if (giftCardElement == null)
                {
                    giftCardElement = xmlDoc.CreateElement("GiftCardInfo");
                    rootElement.AppendChild(giftCardElement);
                }

                var recipientNameElement = xmlDoc.CreateElement("RecipientName");
                recipientNameElement.InnerText = recipientName;
                giftCardElement.AppendChild(recipientNameElement);

                var recipientEmailElement = xmlDoc.CreateElement("RecipientEmail");
                recipientEmailElement.InnerText = recipientEmail;
                giftCardElement.AppendChild(recipientEmailElement);

                var senderNameElement = xmlDoc.CreateElement("SenderName");
                senderNameElement.InnerText = senderName;
                giftCardElement.AppendChild(senderNameElement);

                var senderEmailElement = xmlDoc.CreateElement("SenderEmail");
                senderEmailElement.InnerText = senderEmail;
                giftCardElement.AppendChild(senderEmailElement);

                var messageElement = xmlDoc.CreateElement("Message");
                messageElement.InnerText = giftCardMessage;
                giftCardElement.AppendChild(messageElement);

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return result;
        }

        /// <summary>
        /// Get gift card attrbibutes
        /// </summary>
        /// <param name="attributesXml">Attributes</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        public void GetGiftCardAttribute(string attributesXml, out string recipientName,
            out string recipientEmail, out string senderName,
            out string senderEmail, out string giftCardMessage)
        {
            recipientName = string.Empty;
            recipientEmail = string.Empty;
            senderName = string.Empty;
            senderEmail = string.Empty;
            giftCardMessage = string.Empty;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var recipientNameElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/RecipientName");
                var recipientEmailElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/RecipientEmail");
                var senderNameElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/SenderName");
                var senderEmailElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/SenderEmail");
                var messageElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes/GiftCardInfo/Message");

                if (recipientNameElement != null)
                    recipientName = recipientNameElement.InnerText;
                if (recipientEmailElement != null)
                    recipientEmail = recipientEmailElement.InnerText;
                if (senderNameElement != null)
                    senderName = senderNameElement.InnerText;
                if (senderEmailElement != null)
                    senderEmail = senderEmailElement.InnerText;
                if (messageElement != null)
                    giftCardMessage = messageElement.InnerText;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
        }

        #endregion
    }
}
