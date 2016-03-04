using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product attribute parser
    /// </summary>
    public partial class ProductAttributeParser : IProductAttributeParser
    {
		// 0 = ProductId, 1 = AttributeXml Hash
		private const string ATTRIBUTECOMBINATION_BY_ID_HASH = "SmartStore.parsedattributecombination.id-{0}-{1}";

		private readonly IProductAttributeService _productAttributeService;
		private readonly IRepository<ProductVariantAttributeCombination> _pvacRepository;
		private readonly ICacheManager _cacheManager;

		public ProductAttributeParser(
			IProductAttributeService productAttributeService,
			IRepository<ProductVariantAttributeCombination> pvacRepository,
			ICacheManager cacheManager)
        {
            _productAttributeService = productAttributeService;
			_pvacRepository = pvacRepository;
			_cacheManager = cacheManager;
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

		//public virtual Multimap<int, string> DeserializeProductVariantAttributes(string attributesXml)
		//{
		//    var attrs = new Multimap<int, string>();
		//    if (String.IsNullOrEmpty(attributesXml))
		//        return attrs;

		//    try
		//    {
		//        var xmlDoc = new XmlDocument();
		//        xmlDoc.LoadXml(attributesXml);

		//        var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductVariantAttribute");
		//        foreach (var node1 in nodeList1.Cast<XmlElement>())
		//        {
		//            string sid = node1.GetAttribute("ID").Trim();
		//            if (sid.HasValue())
		//            {
		//                int id = 0;
		//                if (int.TryParse(sid, out id))
		//                {

		//                    var nodeList2 = node1.SelectNodes(@"ProductVariantAttributeValue/Value").Cast<XmlElement>();
		//                    foreach (var node2 in nodeList2)
		//                    {
		//                        string value = node2.InnerText.Trim();
		//                        attrs.Add(id, value);
		//                    }
		//                }
		//            }
		//        }
		//    }
		//    catch (Exception exc)
		//    {
		//        Debug.Write(exc.ToString());
		//    }

		//    return attrs;
		//}

		public virtual Multimap<int, string> DeserializeProductVariantAttributes(string attributesXml)
		{
			var attrs = new Multimap<int, string>();
			if (String.IsNullOrEmpty(attributesXml))
				return attrs;

			try
			{
				var doc = XDocument.Parse(attributesXml);

				// Attributes/ProductVariantAttribute
				foreach (var node1 in doc.Descendants("ProductVariantAttribute"))
				{
					string sid = node1.Attribute("ID").Value;
					if (sid.HasValue())
					{
						int id = 0;
						if (int.TryParse(sid, out id))
						{
							// ProductVariantAttributeValue/Value
							foreach (var node2 in node1.Descendants("Value"))
							{
								attrs.Add(id, node2.Value);
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

		public IList<string> ParseProductVariantAttributeValues(string attributesXml, IEnumerable<ProductVariantAttribute> attributes, int languageId = 0)
		{
			var values = new List<string>();

			if (attributesXml.IsEmpty())
				return values;

			var allValueIds = new List<int>();
			var combinedAttributes = DeserializeProductVariantAttributes(attributesXml);

			foreach (var pva in attributes.Where(x => x.ShouldHaveValues()).OrderBy(x => x.DisplayOrder))
			{
				if (combinedAttributes.ContainsKey(pva.Id))
				{
					var pvaValuesStr = combinedAttributes[pva.Id];
					var ids = pvaValuesStr.Where(x => x.HasValue()).Select(x => x.ToInt());

					allValueIds.AddRange(ids);
				}
			}

			foreach (int id in allValueIds.Distinct())
			{
				foreach (var attribute in attributes)
				{
					var attributeValue = attribute.ProductVariantAttributeValues.FirstOrDefault(x => x.Id == id);
					if (attributeValue != null)
					{
						var value = attributeValue.GetLocalized(x => x.Name, languageId, true, false);

						if (!values.Any(x => x.IsCaseInsensitiveEqual(value)))
							values.Add(value);
						break;
					}
				}
			}

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

		public virtual bool AreProductAttributesEqual(string attributeXml1, string attributeXml2)
        {
			if (attributeXml1.IsCaseInsensitiveEqual(attributeXml2))
				return true;

			var attributes1 = DeserializeProductVariantAttributes(attributeXml1);
            var attributes2 = DeserializeProductVariantAttributes(attributeXml2);

			if (attributes1.Count != attributes2.Count)
				return false;		

			foreach (var kvp in attributes1)
			{
				if (!attributes2.ContainsKey(kvp.Key))
				{
					// the second list does not contain this id: not equal!
					return false;
				}

				// compare the values
				var values1 = kvp.Value;
				var values2 = attributes2[kvp.Key];

				if (values1.Count != values2.Count)
				{
					// number of values differ: not equal!
					return false;
				}

				foreach (var value1 in values1)
				{
					var str1 = value1.TrimSafe();

					if (!values2.Any(x => x.TrimSafe().IsCaseInsensitiveEqual(str1)))
					{
						// the second values list for this attribute does not contain this value: not equal!
						return false;
					}
				}
			}

            return true;
        }

		public virtual ProductVariantAttributeCombination FindProductVariantAttributeCombination(
			int productId, 
			string attributesXml)
		{
			if (attributesXml.IsEmpty())
				return null;

			var attributesHash = attributesXml.Hash(Encoding.UTF8);
            var cacheKey = ATTRIBUTECOMBINATION_BY_ID_HASH.FormatInvariant(productId, attributesHash);

			var result = _cacheManager.Get(cacheKey, () => 
			{
				var query = from x in _pvacRepository.TableUntracked
							where x.ProductId == productId
							select new
							{
								x.Id,
								x.AttributesXml
							};

				var combinations = query.ToList();
				if (combinations.Count == 0)
					return null;

				foreach (var combination in combinations)
				{
					bool attributesEqual = AreProductAttributesEqual(combination.AttributesXml, attributesXml);
					if (attributesEqual)
						return _productAttributeService.GetProductVariantAttributeCombinationById(combination.Id);
				}

				return null;
			});

			return result;
		}

		public virtual List<List<int>> DeserializeQueryData(string jsonData)
		{
			if (jsonData.HasValue())
			{
				if (jsonData.StartsWith("["))
				{
					return JsonConvert.DeserializeObject<List<List<int>>>(jsonData);
				}

				return new List<List<int>> { JsonConvert.DeserializeObject<List<int>>(jsonData) };
			}
			return new List<List<int>>();
		}

		public virtual void DeserializeQueryData(List<List<int>> queryData, string attributesXml, int productId, int bundleItemId = 0)
		{
			Guard.ArgumentNotNull(() => queryData);

			if (attributesXml.HasValue() && productId != 0)
			{
				var attributeValues = ParseProductVariantAttributeValues(attributesXml).ToList();

				foreach (var value in attributeValues)
				{
					var lst = new List<int>
					{
						productId,
						value.ProductVariantAttribute.ProductAttributeId,
						value.ProductVariantAttributeId,
						value.Id
					};

					if (bundleItemId != 0)
						lst.Add(bundleItemId);

					queryData.Add(lst);
				}
			}
		}

		public virtual string SerializeQueryData(string attributesXml, int productId, bool urlEncode = true)
		{
			var data = new List<List<int>>();

			DeserializeQueryData(data, attributesXml, productId);

			return SerializeQueryData(data, urlEncode);
		}

		public virtual string SerializeQueryData(List<List<int>> queryData, bool urlEncode = true)
		{
			if (queryData.Count > 0)
			{
				var result = JsonConvert.SerializeObject(queryData);

				return (urlEncode ? HttpUtility.UrlEncode(result) : result);
			}

			return "";
		}

		private string CreateProductUrl(string queryString, string productSeName)
		{
			var url = UrlHelper.GenerateUrl(
				"Product",
				null,
				null,
				new RouteValueDictionary(new { SeName = productSeName }),
				RouteTable.Routes,
				HttpContext.Current.Request.RequestContext,
				false);

			if (queryString.HasValue())
			{
				url = string.Concat(url, url.Contains("?") ? "&" : "?", "attributes=", queryString);
			}

			return url;
		}

		public virtual string GetProductUrlWithAttributes(string attributesXml, int productId, string productSeName)
		{
			return CreateProductUrl(SerializeQueryData(attributesXml, productId), productSeName);
		}

		public virtual string GetProductUrlWithAttributes(List<List<int>> queryData, string productSeName)
		{
			return CreateProductUrl(SerializeQueryData(queryData), productSeName);
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
