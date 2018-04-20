using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SmartStore.Collections;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Logging;

namespace SmartStore.Services.Catalog
{
	public partial class ProductAttributeParser : IProductAttributeParser
    {
		// 0 = ProductId, 1 = AttributeXml
		private const string ATTRIBUTECOMBINATION_BY_IDXML_KEY = "parsedattributecombination.id-{0}-{1}";

		// 0 = AttributeXml
		private const string ATTRIBUTEVALUES_BY_XML_KEY = "parsedattributevalues-{0}";

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

			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		#region Product attributes

		public virtual void PrefetchProductVariantAttributes(IEnumerable<string> attributesXml)
		{
			if (attributesXml == null || !attributesXml.Any())
				return;

			// Determine uncached attributes
			var unfetched = attributesXml
				.Where(xml => xml.HasValue())
				.Distinct()
				.Where(xml => !_requestCache.Contains(ATTRIBUTEVALUES_BY_XML_KEY.FormatInvariant(xml)))
				.ToArray();

			var infos = new List<AttributeMapInfo>(unfetched.Length);

			foreach (var xml in unfetched)
			{
				var valueIds = new HashSet<int>();
				var map = DeserializeProductVariantAttributes(xml);
				var attributes = _productAttributeService.GetProductVariantAttributesByIds(map.Keys);

				foreach (var attribute in attributes)
				{
					// Only types that have attribute values! Otherwise entered text is misinterpreted as an attribute value id.
					if (!attribute.ShouldHaveValues())
						continue;

					var ids =
						from id in map[attribute.Id]
						where id.HasValue()
						select id.ToInt();

					valueIds.UnionWith(ids);
				}

				var info = new AttributeMapInfo
				{
					AttributesXml = xml,
					DeserializedMap = map,
					AllValueIds = valueIds.ToArray()
				};

				infos.Add(info);
			}

			// Get all value ids across all maps (each map has many attributes)
			var allValueIds = infos.SelectMany(x => x.AllValueIds)
				.Distinct()
				.ToArray();

			// Load ALL requested attribute values into a single dictionary in one go (key is Id)
			var attributeValues = _productAttributeService.GetProductVariantAttributeValuesByIds(allValueIds).ToDictionarySafe(x => x.Id);

			// Create a single cache entry for each passed xml
			foreach (var info in infos)
			{
				var cachedValues = new List<ProductVariantAttributeValue>();

				// Ensure value id order in cached result list is correct
				foreach (var id in info.AllValueIds)
				{
					if (attributeValues.TryGetValue(id, out var value))
					{
						cachedValues.Add(value);
					}
				}

				// Put it in cache
				var cacheKey = ATTRIBUTEVALUES_BY_XML_KEY.FormatInvariant(info.AttributesXml);
				_requestCache.Put(cacheKey, cachedValues);
			}
		}

		public virtual IList<ProductVariantAttribute> ParseProductVariantAttributes(string attributesXml)
		{
			var values = ParseProductVariantAttributeValues(attributesXml);
			var attrMap = DeserializeProductVariantAttributes(attributesXml);
			var attrs = _productAttributeService.GetProductVariantAttributesByIds(attrMap.Keys, values.Select(x => x.ProductVariantAttribute).Distinct().ToList());

			return attrs;
		}

		public virtual IEnumerable<ProductVariantAttributeValue> ParseProductVariantAttributeValues(string attributeXml)
		{
			if (attributeXml.IsEmpty())
				return new List<ProductVariantAttributeValue>();

			var cacheKey = ATTRIBUTEVALUES_BY_XML_KEY.FormatInvariant(attributeXml);

			var result = _requestCache.Get(cacheKey, () =>
			{
				var allValueIds = new HashSet<int>();
				var attrMap = DeserializeProductVariantAttributes(attributeXml);
				var attributes = _productAttributeService.GetProductVariantAttributesByIds(attrMap.Keys);

				foreach (var attribute in attributes)
				{
					// Only types that have attribute values! Otherwise entered text is misinterpreted as an attribute value id.
					if (!attribute.ShouldHaveValues())
						continue;

					var ids =
						from id in attrMap[attribute.Id]
						where id.HasValue()
						select id.ToInt();

					allValueIds.UnionWith(ids);
				}

				var values = _productAttributeService.GetProductVariantAttributeValuesByIds(allValueIds.ToArray());

				return values;
			});

			return result;
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
						if (int.TryParse(sid, out var id))
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
			catch (Exception exception)
			{
				Logger.Error(exception);
			}

			return attrs;
		}

		public virtual ICollection<ProductVariantAttributeValue> ParseProductVariantAttributeValues(Multimap<int, string> attributeCombination, IEnumerable<ProductVariantAttribute> attributes)
		{
			var result = new HashSet<ProductVariantAttributeValue>();

			if (attributeCombination == null || !attributeCombination.Any())
				return result;

			var allValueIds = new HashSet<int>();

			foreach (var pva in attributes.Where(x => x.ShouldHaveValues()).OrderBy(x => x.DisplayOrder).ToArray())
			{
				if (attributeCombination.ContainsKey(pva.Id))
				{
					var pvaValuesStr = attributeCombination[pva.Id];
					var ids = pvaValuesStr.Where(x => x.HasValue()).Select(x => x.ToInt());

					allValueIds.UnionWith(ids);
				}
			}

			foreach (int id in allValueIds)
			{
				foreach (var attribute in attributes)
				{
					var attributeValue = attribute.ProductVariantAttributeValues.FirstOrDefault(x => x.Id == id);
					if (attributeValue != null)
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
                        if (int.TryParse(str1, out var id))
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
            catch (Exception exception)
            {
				Logger.Error(exception);
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
			if (attributeXml1 == attributeXml2)
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

		public virtual ProductVariantAttributeCombination FindProductVariantAttributeCombination(int productId, string attributesXml)
		{
			if (attributesXml.IsEmpty())
				return null;

			var cacheKey = ATTRIBUTECOMBINATION_BY_IDXML_KEY.FormatInvariant(productId, attributesXml);

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

		public string AddGiftCardAttribute(
			string attributesXml,
			string recipientName,
            string recipientEmail,
			string senderName,
			string senderEmail,
			string giftCardMessage)
        {
            var result = string.Empty;

            try
            {
                recipientName = recipientName.TrimSafe();
                recipientEmail = recipientEmail.TrimSafe();
                senderName = senderName.TrimSafe();
                senderEmail = senderEmail.TrimSafe();

                var xmlDoc = new XmlDocument();
                if (string.IsNullOrEmpty(attributesXml))
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
            catch (Exception exception)
            {
				Logger.Error(exception);
            }

            return result;
        }

        public void GetGiftCardAttribute(
			string attributesXml,
			out string recipientName,
            out string recipientEmail,
			out string senderName,
            out string senderEmail,
			out string giftCardMessage)
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
            catch (Exception exception)
            {
				Logger.Error(exception);
			}
        }

        #endregion

		class AttributeMapInfo
		{
			public string AttributesXml { get; set; }
			public Multimap<int, string> DeserializedMap { get; set; }
			public int[] AllValueIds { get; set; }
		}
    }
}
