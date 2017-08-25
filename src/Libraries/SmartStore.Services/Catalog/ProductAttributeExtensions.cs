using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Catalog
{
	public static class ProductAttributeExtensions
    {
        /// <summary>
        /// A value indicating whether this product variant attribute should have values
        /// </summary>
        /// <param name="productVariantAttribute">Product variant attribute</param>
        /// <returns>Result</returns>
        public static bool ShouldHaveValues(this ProductVariantAttribute productVariantAttribute)
        {
            if (productVariantAttribute == null)
                return false;

            if (productVariantAttribute.AttributeControlType == AttributeControlType.TextBox ||
                productVariantAttribute.AttributeControlType == AttributeControlType.MultilineTextbox ||
                productVariantAttribute.AttributeControlType == AttributeControlType.Datepicker || 
                productVariantAttribute.AttributeControlType == AttributeControlType.FileUpload)
                return false;

            // all other attribute control types support values
            return true;
        }
		
		public static string AddProductAttribute(this ProductVariantAttribute pva, string attributes, string value)
		{
			string result = string.Empty;
			try
			{
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

				XmlElement pvaElement = null;
				//find existing
				var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/ProductVariantAttribute");
				foreach (XmlNode node1 in nodeList1)
				{
					if (node1.Attributes != null && node1.Attributes["ID"] != null)
					{
						string str1 = node1.Attributes["ID"].InnerText.Trim();
						int id = 0;
						if (int.TryParse(str1, out id))
						{
							if (id == pva.Id)
							{
								pvaElement = (XmlElement)node1;
								break;
							}
						}
					}
				}

				//create new one if not found
				if (pvaElement == null)
				{
					pvaElement = xmlDoc.CreateElement("ProductVariantAttribute");
					pvaElement.SetAttribute("ID", pva.Id.ToString());
					rootElement.AppendChild(pvaElement);
				}

				var pvavElement = xmlDoc.CreateElement("ProductVariantAttributeValue");
				pvaElement.AppendChild(pvavElement);

				var pvavVElement = xmlDoc.CreateElement("Value");
				pvavVElement.InnerText = value;
				pvavElement.AppendChild(pvavVElement);

				result = xmlDoc.OuterXml;
			}
			catch (Exception exc)
			{
				Debug.Write(exc.ToString());
			}
			return result;
		}

		/// <summary>
		/// Searches the alias and returns values for fragments that begins with fieldPrefix
		/// </summary>
		/// <param name="attributeValues">Product variant attribute values</param>
		/// <param name="fieldPrefix">Field prefix</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>Localized value names mapped by field names</returns>
		public static Dictionary<string, string> GetMappedValuesFromAlias(this IList<ProductVariantAttributeValue> attributeValues, string fieldPrefix, int languageId)
		{
			Guard.NotNull(attributeValues, nameof(attributeValues));
			Guard.NotEmpty(fieldPrefix, nameof(fieldPrefix));

			var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if (!fieldPrefix.EndsWith(":"))
				fieldPrefix = fieldPrefix + ":";

			// TODO: do not use value alias, create a new attribute export field

			//foreach (var value in attributeValues.Where(x => x.Alias.HasValue()))
			//{
			//	foreach (var item in value.Alias.SplitSafe(null).Where(x => x.EmptyNull().StartsWith(fieldPrefix)))
			//	{
			//		var fieldName = item.Substring(fieldPrefix.Length);
			//		if (fieldName.HasValue() && !result.ContainsKey(fieldName))
			//		{
			//			result.Add(fieldName, value.GetLocalized(x => x.Name, languageId, true, false));
			//		}
			//	}
			//}

			return result;
		}
	}
}
