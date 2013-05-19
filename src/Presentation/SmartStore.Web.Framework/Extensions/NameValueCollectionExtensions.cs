using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.IO;
using System.Diagnostics;
using System.Collections.Specialized;
using SmartStore.Services.Catalog;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;

namespace SmartStore
{
	/// <remarks>codehint: sm-add</remarks>
	public static class NameValueCollectionExtension
	{
		private static string AttributeFormatedName(int productAttributeId, int attributeId, int productVariantId = 0) {
			if (productVariantId == 0)
				return "product_attribute_{0}_{1}".FormatWith(productAttributeId, attributeId);
			else
				return "product_attribute_{0}_{1}_{2}".FormatWith(productVariantId, productAttributeId, attributeId);
		}

		public static void AddProductAttribute(this NameValueCollection collection, int productAttributeId, int attributeId, int valueId, int productVariantId = 0) {
			if (productAttributeId != 0 && attributeId != 0 && valueId != 0) {
				string name = AttributeFormatedName(productAttributeId, attributeId, productVariantId);

				collection.Add(name, valueId.ToString());
			}
		}
		public static void ConvertQueryData(this NameValueCollection collection, List<List<int>> queryData, int productVariantId = 0) {
			if (collection == null || queryData == null || queryData.Count <= 0)
				return;

			var enm = queryData.Where(i => i.Count > 3);

			if (productVariantId != 0)
				enm = enm.Where(i => i[0] == productVariantId);

			foreach (var itm in enm) {
				string name = AttributeFormatedName(itm[1], itm[2], itm[0]);

				collection.Add(name, itm[3].ToString());
			}
		}

		/// <summary>Takes selected elements from collection and creates a attribute XML string from it.</summary>
		/// <param name="formatWithProductVariantId">how the name of the controls are formatted. frontend includes productVariantId, backend does not.</param>
		/// <remarks>codehint: sm-edit (moved)</remarks>
		public static string CreateSelectedAttributesXml(this NameValueCollection collection, int productVariantId, IList<ProductVariantAttribute> variantAttributes,
			IProductAttributeParser productAttributeParser, ILocalizationService localizationService, IDownloadService downloadService, CatalogSettings catalogSettings, 
			HttpRequestBase request, List<string> warnings, bool formatWithProductVariantId = true)
		{
			string controlId;
			string selectedAttributes = string.Empty;

			foreach (var attribute in variantAttributes) {
				controlId = AttributeFormatedName(attribute.ProductAttributeId, attribute.Id, formatWithProductVariantId ? productVariantId : 0);
				//if (formatWithProductVariantId)
				//	controlId = "product_attribute_{0}_{1}_{2}".FormatWith(productVariantId, attribute.ProductAttributeId, attribute.Id);
				//else
				//	controlId = "product_attribute_{0}_{1}".FormatWith(attribute.ProductAttributeId, attribute.Id);

				switch (attribute.AttributeControlType) {
					case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares: {
                            var ctrlAttributes = collection[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes)) {
                                int selectedAttributeId = int.Parse(ctrlAttributes);
								if (selectedAttributeId > 0)
									selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes, attribute, selectedAttributeId.ToString());
							}
						}
						break;

					case AttributeControlType.Checkboxes: {
							var cblAttributes = collection[controlId];
							if (!String.IsNullOrEmpty(cblAttributes)) {
								foreach (var item in cblAttributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) {
									int selectedAttributeId = int.Parse(item);
									if (selectedAttributeId > 0)
										selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes,	attribute, selectedAttributeId.ToString());
								}
							}
						}
						break;

					case AttributeControlType.TextBox:
					case AttributeControlType.MultilineTextbox: {
							var txtAttribute = collection[controlId];
							if (!String.IsNullOrEmpty(txtAttribute)) {
								string enteredText = txtAttribute.Trim();
								selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes,	attribute, enteredText);
							}
						}
						break;

					case AttributeControlType.Datepicker: {
							var date = collection[controlId + "_day"];
							var month = collection[controlId + "_month"];
							var year = collection[controlId + "_year"];
							DateTime? selectedDate = null;
							try {
								selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
							}
							catch { }
							if (selectedDate.HasValue) {
								selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes,	attribute, selectedDate.Value.ToString("D"));
							}
						}
						break;

					case AttributeControlType.FileUpload: {
							var httpPostedFile = request.Files[controlId];
							if ((httpPostedFile != null) && (!String.IsNullOrEmpty(httpPostedFile.FileName))) {
								int fileMaxSize = catalogSettings.FileUploadMaximumSizeBytes;
								if (httpPostedFile.ContentLength > fileMaxSize) {
									warnings.Add(string.Format(localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), (int)(fileMaxSize / 1024)));
								}
								else {
									//save an uploaded file
									var download = new Download() {
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

	}	// class
}
