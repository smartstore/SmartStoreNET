using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using SmartStore.Services.Catalog;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;

namespace SmartStore
{
	public static class NameValueCollectionExtension
	{
        public static IDictionary<string, string> ToDictionary(this NameValueCollection collection)
        {
            Guard.ArgumentNotNull(() => collection);
            
            var query = from key in collection.AllKeys
                        where key != null
                        select key;

            Func<string, string> elementSelector = key => collection[key];

            return query.ToDictionary<string, string, string>(key => key, elementSelector);
        }
        
        private static string AttributeFormatedName(int productAttributeId, int attributeId, int productId = 0) 
        {
			if (productId == 0)
				return "product_attribute_{0}_{1}".FormatWith(productAttributeId, attributeId);
			else
				return "product_attribute_{0}_{1}_{2}".FormatWith(productId, productAttributeId, attributeId);
		}

		public static void AddProductAttribute(this NameValueCollection collection, int productAttributeId, int attributeId, int valueId, int productId = 0) 
        {
			if (productAttributeId != 0 && attributeId != 0 && valueId != 0)
			{
				string name = AttributeFormatedName(productAttributeId, attributeId, productId);

				collection.Add(name, valueId.ToString());
			}
		}

		public static void ConvertQueryData(this NameValueCollection collection, List<List<int>> queryData, int productId = 0) 
        {
			if (collection == null || queryData == null || queryData.Count <= 0)
				return;

			var enm = queryData.Where(i => i.Count > 3);

			if (productId != 0)
				enm = enm.Where(i => i[0] == productId);

			foreach (var itm in enm)
			{
				string name = AttributeFormatedName(itm[1], itm[2], itm[0]);

				collection.Add(name, itm[3].ToString());
			}
		}

		/// <summary>Takes selected elements from collection and creates a attribute XML string from it.</summary>
		/// <param name="formatWithProductId">how the name of the controls are formatted. frontend includes productId, backend does not.</param>
		public static string CreateSelectedAttributesXml(this NameValueCollection collection, int productId, IList<ProductVariantAttribute> variantAttributes,
			IProductAttributeParser productAttributeParser, ILocalizationService localizationService, IDownloadService downloadService, CatalogSettings catalogSettings, 
			HttpRequestBase request, List<string> warnings, bool formatWithProductId = true)
		{
			string controlId;
			string selectedAttributes = "";

			foreach (var attribute in variantAttributes)
			{
				controlId = AttributeFormatedName(attribute.ProductAttributeId, attribute.Id, formatWithProductId ? productId : 0);

				switch (attribute.AttributeControlType)
				{
					case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
						{
                            var ctrlAttributes = collection[controlId];
                            if (ctrlAttributes.HasValue())
							{
                                int selectedAttributeId = int.Parse(ctrlAttributes);
								if (selectedAttributeId > 0)
									selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes, attribute, selectedAttributeId.ToString());
							}
						}
						break;

					case AttributeControlType.Checkboxes:
						{
							var cblAttributes = collection[controlId];
							if (cblAttributes.HasValue())
							{
								foreach (var item in cblAttributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
								{
									int selectedAttributeId = int.Parse(item);
									if (selectedAttributeId > 0)
										selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes,	attribute, selectedAttributeId.ToString());
								}
							}
						}
						break;

					case AttributeControlType.TextBox:
					case AttributeControlType.MultilineTextbox:
						{
							var txtAttribute = collection[controlId];
							if (txtAttribute.HasValue())
							{
								string enteredText = txtAttribute.Trim();
								selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes,	attribute, enteredText);
							}
						}
						break;

					case AttributeControlType.Datepicker:
						{
							var date = collection[controlId + "_day"];
							var month = collection[controlId + "_month"];
							var year = collection[controlId + "_year"];
							DateTime? selectedDate = null;

							try
							{
								selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
							}
							catch { }

							if (selectedDate.HasValue)
							{
								selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes,	attribute, selectedDate.Value.ToString("D"));
							}
						}
						break;

					case AttributeControlType.FileUpload:
						{
							var httpPostedFile = request.Files[controlId];
							if (httpPostedFile != null && httpPostedFile.FileName.HasValue())
							{
								int fileMaxSize = catalogSettings.FileUploadMaximumSizeBytes;
								if (httpPostedFile.ContentLength > fileMaxSize)
								{
									warnings.Add(string.Format(localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), (int)(fileMaxSize / 1024)));
								}
								else
								{
									//save an uploaded file
									var download = new Download()
									{
										DownloadGuid = Guid.NewGuid(),
										UseDownloadUrl = false,
										DownloadUrl = "",
										DownloadBinary = httpPostedFile.GetDownloadBits(),
										ContentType = httpPostedFile.ContentType,
										Filename = System.IO.Path.GetFileNameWithoutExtension(httpPostedFile.FileName),
										Extension = System.IO.Path.GetExtension(httpPostedFile.FileName),
										IsNew = true
									};
									downloadService.InsertDownload(download);
									//save attribute
									selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes, attribute, download.DownloadGuid.ToString());
								}
							}
						}
						break;

					default:
						break;
				}
			}
			return selectedAttributes;
		}
	}
}
