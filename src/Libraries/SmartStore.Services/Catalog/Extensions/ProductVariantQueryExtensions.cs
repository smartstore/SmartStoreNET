using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;

namespace SmartStore.Services.Catalog.Extensions
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
			var result = string.Empty;

			foreach (var pva in variantAttributes)
			{
				var selectedItems = query.Variants.Where(x => 
					x.ProductId == productId &&
					x.BundleItemId == bundleItemId &&
					x.AttributeId == pva.ProductAttributeId &&
					x.VariantAttributeId == pva.Id);

				switch (pva.AttributeControlType)
				{
					case AttributeControlType.DropdownList:
					case AttributeControlType.RadioList:
					case AttributeControlType.Boxes:
						{
							var firstItemValue = selectedItems.FirstOrDefault()?.Value;
							if (firstItemValue.HasValue())
							{
								var selectedAttributeId = firstItemValue.SplitSafe(",").SafeGet(0).ToInt();
								if (selectedAttributeId > 0)
								{
									result = productAttributeParser.AddProductAttribute(result, pva, selectedAttributeId.ToString());
								}
							}
						}
						break;

					case AttributeControlType.Checkboxes:
						foreach (var item in selectedItems)
						{
							var selectedAttributeId = item.Value.SplitSafe(",").SafeGet(0).ToInt();
							if (selectedAttributeId > 0)
							{
								result = productAttributeParser.AddProductAttribute(result, pva, selectedAttributeId.ToString());
							}
						}
						break;

					case AttributeControlType.TextBox:
					case AttributeControlType.MultilineTextbox:
						{
							var selectedValue = string.Join(",", selectedItems.Select(x => x.Value));
							if (selectedValue.HasValue())
							{
								result = productAttributeParser.AddProductAttribute(result, pva, selectedValue);
							}
						}
						break;

					case AttributeControlType.Datepicker:
						{
							var firstItemDate = selectedItems.FirstOrDefault()?.Date;
							if (firstItemDate.HasValue)
							{
								result = productAttributeParser.AddProductAttribute(result, pva, firstItemDate.Value.ToString("D"));
							}
						}
						break;

					case AttributeControlType.FileUpload:
						if (request == null)
						{
							var firstItemValue = selectedItems.FirstOrDefault()?.Value;
							Guid downloadGuid;
							Guid.TryParse(firstItemValue, out downloadGuid);
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
	}
}
