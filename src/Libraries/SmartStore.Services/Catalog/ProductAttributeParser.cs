using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
	public partial class ProductAttributeParser : IProductAttributeParser
    {
		// 0 = ProductId, 1 = AttributeXml Hash
		private const string ATTRIBUTECOMBINATION_BY_ID_HASH = "SmartStore.parsedattributecombination.id-{0}-{1}";

		private readonly IProductAttributeService _productAttributeService;
		private readonly IRepository<ProductVariantAttributeCombination> _pvacRepository;
		private readonly IRequestCache _requestCache;

		public ProductAttributeParser(
			IProductAttributeService productAttributeService,
			IRepository<ProductVariantAttributeCombination> pvacRepository,
			IRequestCache requestCache)
        {
            _productAttributeService = productAttributeService;
			_pvacRepository = pvacRepository;
			_requestCache = requestCache;
        }

		#region Product attributes

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

		public virtual IList<ProductVariantAttribute> ParseProductVariantAttributes(string attributesXml)
		{
			var ids = ParseProductVariantAttributeIds(attributesXml);

			return _productAttributeService.GetProductVariantAttributesByIds(ids.ToList());
		}

        public virtual IEnumerable<ProductVariantAttributeValue> ParseProductVariantAttributeValues(string attributeXml)
        {
			var attributeIds = DeserializeProductVariantAttributes(attributeXml);
			var valueIds = new HashSet<int>(attributeIds.SelectMany(x => x.Value).Select(x => x.ToInt()));

			var values = _productAttributeService.GetProductVariantAttributeValuesByIds(valueIds.ToArray());

			return values.Where(x => x.ProductVariantAttribute.ShouldHaveValues());

			//var allIds = new List<int>();
			//var attrs = DeserializeProductVariantAttributes(attributeXml);
			//var pvaCollection = _productAttributeService.GetProductVariantAttributesByIds(attrs.Keys);

			//foreach (var pva in pvaCollection)
			//{
			//	if (!pva.ShouldHaveValues())
			//		continue;

			//	var pvaValuesStr = attrs[pva.Id];

			//	var ids =
			//		from id in pvaValuesStr
			//		where id.HasValue()
			//		select id.ToInt();

			//	allIds.AddRange(ids);
			//}

			//int[] allDistinctIds = allIds.Distinct().ToArray();

			//var values = _productAttributeService.GetProductVariantAttributeValuesByIds(allDistinctIds);
			//return values;
		}

		public virtual IList<ProductVariantAttributeValue> ParseProductVariantAttributeValues(Multimap<int, string> attributeCombination, IEnumerable<ProductVariantAttribute> attributes)
		{
			var result = new List<ProductVariantAttributeValue>();

			if (attributeCombination == null || !attributeCombination.Any())
				return result;

			var allValueIds = new List<int>();

			foreach (var pva in attributes.Where(x => x.ShouldHaveValues()).OrderBy(x => x.DisplayOrder))
			{
				if (attributeCombination.ContainsKey(pva.Id))
				{
					var pvaValuesStr = attributeCombination[pva.Id];
					var ids = pvaValuesStr.Where(x => x.HasValue()).Select(x => x.ToInt());

					allValueIds.AddRange(ids);
				}
			}

			foreach (int id in allValueIds.Distinct())
			{
				foreach (var attribute in attributes)
				{
					var attributeValue = attribute.ProductVariantAttributeValues.FirstOrDefault(x => x.Id == id);
					if (attributeValue != null && !result.Any(x => x.Id == attributeValue.Id))
					{
						result.Add(attributeValue);
						break;
					}
				}
			}

			return result;
		}

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

        public virtual string AddProductAttribute(string attributesXml, ProductVariantAttribute pva, string value)
        {
			return pva.AddProductAttribute(attributesXml, value);
        }

		public virtual string CreateAttributesXml(Multimap<int, string> attributes)
		{
			Guard.NotNull(attributes, nameof(attributes));

			if (attributes.Count == 0)
				return null;

			var doc = new XmlDocument();
			var root = doc.AppendChild(doc.CreateElement("Attributes"));

			foreach (var attr in attributes)
			{
				var xelAttr = root.AppendChild(doc.CreateElement("ProductVariantAttribute")) as XmlElement;
				xelAttr.SetAttribute("ID", attr.Key.ToString());

				foreach (var val in attr.Value)
				{
					var xelAttrValue = xelAttr.AppendChild(doc.CreateElement("ProductVariantAttributeValue"));
					var xelValue = xelAttrValue.AppendChild(doc.CreateElement("Value"));
					xelValue.InnerText = val;
				}
			}

			return doc.OuterXml;
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

			var result = _requestCache.Get(cacheKey, () => 
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

		#endregion

		#region Gift card attributes

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
