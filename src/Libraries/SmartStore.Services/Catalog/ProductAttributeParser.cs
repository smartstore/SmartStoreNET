using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Collections;
using Newtonsoft.Json;
using System.Web;

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
        /// <param name="attributes">Attributes</param>
        /// <returns>Selected product variant attribute identifiers</returns>
        private IEnumerable<int> ParseProductVariantAttributeIds(string attributes)
        {
            var ids = new List<int>();
            if (String.IsNullOrEmpty(attributes))
                yield break;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributes);

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

        public virtual Multimap<int, string> DeserializeProductVariantAttributes(string attributes)
        {
            var attrs = new Multimap<int, string>();
            if (String.IsNullOrEmpty(attributes))
                return attrs;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributes);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductVariantAttribute");
                foreach (var node1 in nodeList1.Cast<XmlElement>()) // codehint: sm-edit
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
        /// <param name="attributes">Attributes</param>
        /// <returns>Selected product variant attributes</returns>
        public virtual IList<ProductVariantAttribute> ParseProductVariantAttributes(string attributes)
        {
            var pvaCollection = new List<ProductVariantAttribute>();

            // codehint: sm-edit
            var ids = ParseProductVariantAttributeIds(attributes);
            return this.ParseProductVariantAttributes(ids.ToList()).ToList();
        }

        public virtual IEnumerable<ProductVariantAttribute> ParseProductVariantAttributes(ICollection<int> ids)
        {

            if (ids != null)
            {
                if (ids.Count == 1)
                {
                    var pva = _productAttributeService.GetProductVariantAttributeById(ids.ElementAt(0));
                    if (pva != null)
                    {
                        return new ProductVariantAttribute[] { pva };
                    }
                }
                else
                {
                    return _productAttributeService.GetProductVariantAttributesByIds(ids.ToArray()).ToList();
                }
            }

            return Enumerable.Empty<ProductVariantAttribute>();
        }

        /// <summary>
        /// Get product variant attribute values
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Product variant attribute values</returns>
        public virtual IEnumerable<ProductVariantAttributeValue> ParseProductVariantAttributeValues(string attributes)
        {
            var pvaValues = Enumerable.Empty<ProductVariantAttributeValue>();

            var attrs = DeserializeProductVariantAttributes(attributes);
            var pvaCollection = ParseProductVariantAttributes(attrs.Keys);

            foreach (var pva in pvaCollection)
            {
                if (!pva.ShouldHaveValues())
                    continue;

                var pvaValuesStr = attrs[pva.Id]; //ParseValues(attributes, pva.Id);
                var ids = from id in pvaValuesStr
                          where id.HasValue()
                          select id.ToInt();
                var values = _productAttributeService.GetProductVariantAttributeValuesByIds(ids.ToArray());

                pvaValues = pvaValues.Concat(values);
            }

            return pvaValues;
        }

        /// <summary>
        /// Gets selected product variant attribute value
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="productVariantAttributeId">Product variant attribute identifier</param>
        /// <returns>Product variant attribute value</returns>
        public virtual IList<string> ParseValues(string attributes, int productVariantAttributeId)
        {
            var selectedProductVariantAttributeValues = new List<string>();
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributes);

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
        /// <param name="attributes">Attributes</param>
        /// <param name="pva">Product variant attribute</param>
        /// <param name="value">Value</param>
        /// <returns>Attributes</returns>
        public virtual string AddProductAttribute(string attributes, ProductVariantAttribute pva, string value)
        {
			return pva.AddProductAttribute(attributes, value);
        }

        /// <summary>
        /// Are attributes equal
        /// </summary>
        /// <param name="attributes1">The attributes of the first product</param>
        /// <param name="attributes2">The attributes of the second product</param>
        /// <returns>Result</returns>
        public virtual bool AreProductAttributesEqual(string attributes1, string attributes2)
        {
            // codehint: sm-edit (massiv)

            var attrs1 = DeserializeProductVariantAttributes(attributes1);
            var attrs2 = DeserializeProductVariantAttributes(attributes2);

            if (attrs1.Count == attrs2.Count)
            {
                var pva1Collection = ParseProductVariantAttributes(attrs2.Keys);
                var pva2Collection = ParseProductVariantAttributes(attrs1.Keys);
                foreach (var pva1 in pva1Collection)
                {
                    foreach (var pva2 in pva2Collection)
                    {
                        if (pva1.Id == pva2.Id)
                        {
                            var pvaValues1Str = attrs2[pva1.Id]; // ParseValues(attributes2, pva1.Id);
                            var pvaValues2Str = attrs1[pva2.Id]; // ParseValues(attributes1, pva2.Id);
                            if (pvaValues1Str.Count == pvaValues2Str.Count)
                            {
                                foreach (string str1 in pvaValues1Str)
                                {
                                    bool hasAttribute = pvaValues2Str.Any(x => x.IsCaseInsensitiveEqual(str1));
                                    if (!hasAttribute)
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Finds a product variant attribute combination by attributes stored in XML 
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Found product variant attribute combination</returns>
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
				//existing combinations
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
        /// <param name="attributes">Attributes</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        /// <returns>Attributes</returns>
        public string AddGiftCardAttribute(string attributes, string recipientName,
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
                if (String.IsNullOrEmpty(attributes))
                {
                    var element1 = xmlDoc.CreateElement("Attributes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(attributes);
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
        /// <param name="attributes">Attributes</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="recipientEmail">Recipient email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="giftCardMessage">Message</param>
        public void GetGiftCardAttribute(string attributes, out string recipientName,
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
                xmlDoc.LoadXml(attributes);

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
