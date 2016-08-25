using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;

namespace SmartStore
{
	public static class NameValueCollectionExtensions
	{
		// TODO: find place to make it public static
		private static string AttributeFormatedName(int productAttributeId, int attributeId, int productId = 0, int bundleItemId = 0)
		{
			if (productId == 0)
				return "product_attribute_{0}_{1}".FormatInvariant(productAttributeId, attributeId);
			else
				return "product_attribute_{0}_{1}_{2}_{3}".FormatInvariant(productId, bundleItemId, productAttributeId, attributeId);
		}

		public static void AddProductAttribute(this NameValueCollection collection, int productAttributeId, int attributeId, int valueId, int productId = 0, int bundleItemId = 0)
		{
			if (productAttributeId != 0 && attributeId != 0 && valueId != 0)
			{
				var name = AttributeFormatedName(productAttributeId, attributeId, productId, bundleItemId);

				collection.Add(name, valueId.ToString());
			}
		}

		/// <summary>
		/// Get selected attributes from query string
		/// </summary>
		/// <param name="collection">Name value collection with selected attributes</param>
		/// <param name="queryString">Query string parameters</param>
		/// <param name="queryData">Attribute query data items with following structure: 
		/// <c>Product.Id, ProductAttribute.Id, Product_ProductAttribute_Mapping.Id, ProductVariantAttributeValue.Id, [BundleItem.Id]</c></param>
		/// <param name="productId">Product identifier to filter</param>
		public static void GetSelectedAttributes(this NameValueCollection collection, NameValueCollection queryString, List<List<int>> attributes, int productId = 0)
		{
			Guard.NotNull(collection, nameof(collection));

			// ambiguous parameters: let other query string parameters win over the json formatted attributes parameter
			if (queryString != null && queryString.Count > 0)
			{
				var items = queryString.AllKeys
					.Where(x => x.EmptyNull().StartsWith("product_attribute_"))
					.SelectMany(queryString.GetValues, (k, v) => new { key = k.EmptyNull(), value = v.TrimSafe() });

				foreach (var item in items)
				{
					var ids = item.key.Replace("product_attribute_", "").SplitSafe("_");
					if (ids.Count() > 3)
					{
						if (productId == 0 || (productId != 0 && productId == ids[0].ToInt()))
						{
							collection.Add(item.key, item.value);
						}
					}
				}
			}

			if (attributes != null && attributes.Count > 0)
			{
				var items = attributes.Where(i => i.Count > 3);

				if (productId != 0)
					items = items.Where(i => i[0] == productId);

				foreach (var item in items)
				{
					var name = AttributeFormatedName(item[1], item[2], item[0], item.Count > 4 ? item[4] : 0);

					collection.Add(name, item[3].ToString());
				}
			}
		}

		/// <summary>Takes selected elements from collection and creates a attribute XML string from it.</summary>
		/// <param name="formatWithProductId">how the name of the controls are formatted. frontend includes productId, backend does not.</param>
		public static string CreateSelectedAttributesXml(this NameValueCollection collection, 
			int productId, 
			IEnumerable<ProductVariantAttribute> variantAttributes,
			IProductAttributeParser productAttributeParser, 
			ILocalizationService localizationService, 
			IDownloadService downloadService, 
			CatalogSettings catalogSettings,
			HttpRequestBase request, List<string> warnings, 
			bool formatWithProductId = true, 
			int bundleItemId = 0)
		{
			if (collection == null)
				return "";

			string controlId;
			string selectedAttributes = "";

			foreach (var attribute in variantAttributes)
			{
				controlId = AttributeFormatedName(attribute.ProductAttributeId, attribute.Id, formatWithProductId ? productId : 0, bundleItemId);

				switch (attribute.AttributeControlType)
				{
					case AttributeControlType.DropdownList:
					case AttributeControlType.RadioList:
					case AttributeControlType.ColorSquares:
						{
							var ctrlAttributes = collection[controlId];
							if (ctrlAttributes.HasValue())
							{
								var selectedAttributeId = ctrlAttributes.SplitSafe(",").SafeGet(0).ToInt();
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
									var selectedAttributeId = item.SplitSafe(",").SafeGet(0).ToInt();
									if (selectedAttributeId > 0)
										selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes, attribute, selectedAttributeId.ToString());
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
								selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes, attribute, enteredText);
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
								selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes, attribute, selectedDate.Value.ToString("D"));
							}
						}
						break;

					case AttributeControlType.FileUpload:
						if (request == null)
						{
							Guid downloadGuid;
							Guid.TryParse(collection[controlId], out downloadGuid);
							var download = downloadService.GetDownloadByGuid(downloadGuid);
							if (download != null)
							{
								download.IsTransient = false;
								downloadService.UpdateDownload(download);

								selectedAttributes = productAttributeParser.AddProductAttribute(selectedAttributes, attribute, download.DownloadGuid.ToString());
							}
						}
						else
						{
							var postedFile = request.Files[controlId];
							if (postedFile != null && postedFile.FileName.HasValue())
							{
								int fileMaxSize = catalogSettings.FileUploadMaximumSizeBytes;
								if (postedFile.ContentLength > fileMaxSize)
								{
									warnings.Add(string.Format(localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), (int)(fileMaxSize / 1024)));
								}
								else
								{
									//save an uploaded file
									var download = new Download
									{
										DownloadGuid = Guid.NewGuid(),
										UseDownloadUrl = false,
										DownloadUrl = "",
										ContentType = postedFile.ContentType,
										Filename = System.IO.Path.GetFileNameWithoutExtension(postedFile.FileName),
										Extension = System.IO.Path.GetExtension(postedFile.FileName),
										IsNew = true,
										UpdatedOnUtc = DateTime.UtcNow
									};

									downloadService.InsertDownload(download, postedFile.InputStream != null ? postedFile.InputStream.ToByteArray() : null);

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

		public static string AddGiftCardAttribute(this NameValueCollection collection, string attributes, int productId, IProductAttributeParser productAttributeParser, int bundleItemId = 0)
		{
			string recipientName = "";
			string recipientEmail = "";
			string senderName = "";
			string senderEmail = "";
			string giftCardMessage = "";

			string strProductId = "";
			if (productId != 0)
				strProductId = "_{0}_{1}".FormatWith(productId, bundleItemId);

			foreach (string formKey in collection.AllKeys)
			{
				if (formKey.Equals(string.Format("giftcard{0}.RecipientName", strProductId), StringComparison.InvariantCultureIgnoreCase))
				{
					recipientName = collection[formKey];
					continue;
				}
				if (formKey.Equals(string.Format("giftcard{0}.RecipientEmail", strProductId), StringComparison.InvariantCultureIgnoreCase))
				{
					recipientEmail = collection[formKey];
					continue;
				}
				if (formKey.Equals(string.Format("giftcard{0}.SenderName", strProductId), StringComparison.InvariantCultureIgnoreCase))
				{
					senderName = collection[formKey];
					continue;
				}
				if (formKey.Equals(string.Format("giftcard{0}.SenderEmail", strProductId), StringComparison.InvariantCultureIgnoreCase))
				{
					senderEmail = collection[formKey];
					continue;
				}
				if (formKey.Equals(string.Format("giftcard{0}.Message", strProductId), StringComparison.InvariantCultureIgnoreCase))
				{
					giftCardMessage = collection[formKey];
					continue;
				}
			}

			return productAttributeParser.AddGiftCardAttribute(attributes, recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
		}
	}
}
