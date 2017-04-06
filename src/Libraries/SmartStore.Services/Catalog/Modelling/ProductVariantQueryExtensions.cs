using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;

namespace SmartStore.Services.Catalog.Modelling
{
	public static class ProductVariantQueryExtensions
	{
		public static string CreateSelectedAttributesXml(
			this ProductVariantQuery query,
			int productId,
			int bundleItemId,
			IEnumerable<ProductVariantAttribute> variantAttributes,
			IProductAttributeParser productAttributeParser,
			ILocalizationService localization,
			IDownloadService downloadService,
			CatalogSettings catalogSettings,
			HttpRequestBase request,
			List<string> warnings)
		{
			var result = "";

			foreach (var pva in variantAttributes)
			{
				var selected = query.GetVariant(productId, bundleItemId, pva.ProductAttributeId, pva.Id);
				var selectedValue = selected?.Value;

				switch (pva.AttributeControlType)
				{
					case AttributeControlType.DropdownList:
					case AttributeControlType.RadioList:
					case AttributeControlType.ColorSquares:
						if (selectedValue.HasValue())
						{
							var selectedAttributeId = selectedValue.SplitSafe(",").SafeGet(0).ToInt();
							if (selectedAttributeId > 0)
							{
								result = productAttributeParser.AddProductAttribute(result, pva, selectedAttributeId.ToString());
							}
						}
						break;

					case AttributeControlType.Checkboxes:
						if (selectedValue.HasValue())
						{
							foreach (var item in selectedValue.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
							{
								var selectedAttributeId = item.SplitSafe(",").SafeGet(0).ToInt();
								if (selectedAttributeId > 0)
								{
									result = productAttributeParser.AddProductAttribute(result, pva, selectedAttributeId.ToString());
								}
							}
						}
						break;

					case AttributeControlType.TextBox:
					case AttributeControlType.MultilineTextbox:
						if (selectedValue.HasValue())
						{
							result = productAttributeParser.AddProductAttribute(result, pva, selectedValue);
						}
						break;

					case AttributeControlType.Datepicker:
						var date = selected?.Date;
						if (date.HasValue)
						{
							result = productAttributeParser.AddProductAttribute(result, pva, date.Value.ToString("D"));
						}
						break;

					case AttributeControlType.FileUpload:
						if (request == null)
						{
							Guid downloadGuid;
							Guid.TryParse(selectedValue, out downloadGuid);
							var download = downloadService.GetDownloadByGuid(downloadGuid);
							if (download != null)
							{
								download.IsTransient = false;
								downloadService.UpdateDownload(download);

								result = productAttributeParser.AddProductAttribute(result, pva, download.DownloadGuid.ToString());
							}
						}
						else
						{
							var postedFile = request.Files[ProductVariantQueryItem.CreateKey(productId, bundleItemId, pva.ProductAttributeId, pva.Id)];
							if (postedFile != null && postedFile.FileName.HasValue())
							{
								if (postedFile.ContentLength > catalogSettings.FileUploadMaximumSizeBytes)
								{
									warnings.Add(localization.GetResource("ShoppingCart.MaximumUploadedFileSize").FormatInvariant((int)(catalogSettings.FileUploadMaximumSizeBytes / 1024)));
								}
								else
								{
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

									result = productAttributeParser.AddProductAttribute(result, pva, download.DownloadGuid.ToString());
								}
							}
						}
						break;
				}
			}

			return result;
		}

		public static string AddGiftCardAttribute(
			this ProductVariantQuery query,
			int productId,
			int bundleItemId,
			string attributesXml,
			IProductAttributeParser productAttributeParser)
		{
			return productAttributeParser.AddGiftCardAttribute(
				attributesXml,
				query.GetGiftCardValue(productId, bundleItemId, "RecipientName"),
				query.GetGiftCardValue(productId, bundleItemId, "RecipientEmail"),
				query.GetGiftCardValue(productId, bundleItemId, "SenderName"),
				query.GetGiftCardValue(productId, bundleItemId, "SenderEmail"),
				query.GetGiftCardValue(productId, bundleItemId, "Message"));
		}

		/// <summary>
		/// Gets the URL of product detail page including variant query string
		/// </summary>
		/// <param name="query">Product variant query</param>
		/// <param name="productSeName">Product SEO name</param>
		/// <returns>URL of the product page including variant query string</returns>
		public static string GetProductUrlWithVariants(this ProductVariantQuery query, string productSeName)
		{
			var url = UrlHelper.GenerateUrl(
				"Product",
				null,
				null,
				new RouteValueDictionary(new { SeName = productSeName }),
				RouteTable.Routes,
				HttpContext.Current.Request.RequestContext,
				false);

			var queryString = query.ToString();

			if (queryString.HasValue())
			{
				url = string.Concat(url, url.Contains("?") ? "&" : "?", queryString);
			}

			return url;
		}
	}
}
