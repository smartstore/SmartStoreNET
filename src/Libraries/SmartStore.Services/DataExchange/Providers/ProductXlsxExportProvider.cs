using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Stores;

namespace SmartStore.Services.DataExchange.Export.Providers
{
	/// <summary>
	/// Exports Excel formatted product data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreProductXlsx")]
	[FriendlyName("Product Excel Export")]
	public class ProductXlsxExportProvider : ExportProviderBase
	{
		private readonly ILanguageService _languageService;
		private readonly IProductService _productService;
		private readonly IStoreMappingService _storeMappingService;

		private string[] Properties
		{
			get
			{
				return new string[]
                {
                    "ProductTypeId",
                    "ParentGroupedProductId",
					"VisibleIndividually",
                    "Name",
                    "ShortDescription",
                    "FullDescription",
                    "ProductTemplateId",
                    "ShowOnHomePage",
					"HomePageDisplayOrder",
                    "MetaKeywords",
                    "MetaDescription",
                    "MetaTitle",
                    "SeName",
                    "AllowCustomerReviews",
                    "Published",
                    "SKU",
                    "ManufacturerPartNumber",
                    "Gtin",
                    "IsGiftCard",
                    "GiftCardTypeId",
                    "RequireOtherProducts",
					"RequiredProductIds",
                    "AutomaticallyAddRequiredProducts",
                    "IsDownload",
                    "DownloadId",
                    "UnlimitedDownloads",
                    "MaxNumberOfDownloads",
                    "DownloadActivationTypeId",
                    "HasSampleDownload",
                    "SampleDownloadId",
                    "HasUserAgreement",
                    "UserAgreementText",
                    "IsRecurring",
                    "RecurringCycleLength",
                    "RecurringCyclePeriodId",
                    "RecurringTotalCycles",
                    "IsShipEnabled",
                    "IsFreeShipping",
                    "AdditionalShippingCharge",
					"IsEsd",
                    "IsTaxExempt",
                    "TaxCategoryId",
                    "ManageInventoryMethodId",
                    "StockQuantity",
                    "DisplayStockAvailability",
                    "DisplayStockQuantity",
                    "MinStockQuantity",
                    "LowStockActivityId",
                    "NotifyAdminForQuantityBelow",
                    "BackorderModeId",
                    "AllowBackInStockSubscriptions",
                    "OrderMinimumQuantity",
                    "OrderMaximumQuantity",
                    "AllowedQuantities",
                    "DisableBuyButton",
                    "DisableWishlistButton",
					"AvailableForPreOrder",
                    "CallForPrice",
                    "Price",
                    "OldPrice",
                    "ProductCost",
                    "SpecialPrice",
                    "SpecialPriceStartDateTimeUtc",
                    "SpecialPriceEndDateTimeUtc",
                    "CustomerEntersPrice",
                    "MinimumCustomerEnteredPrice",
                    "MaximumCustomerEnteredPrice",
                    "Weight",
                    "Length",
                    "Width",
                    "Height",
                    "CreatedOnUtc",
                    "CategoryIds",
                    "ManufacturerIds",
                    "Picture1",
                    "Picture2",
                    "Picture3",
					"DeliveryTimeId",
                    "QuantityUnitId",
					"BasePriceEnabled",
					"BasePriceMeasureUnit",
					"BasePriceAmount",
					"BasePriceBaseAmount",
					"BundleTitleText",
					"BundlePerItemShipping",
					"BundlePerItemPricing",
					"BundlePerItemShoppingCart",
					"BundleItemSkus",
                    "AvailableStartDateTimeUtc",
                    "AvailableEndDateTimeUtc",
                    "StoreIds",
                    "LimitedToStores"
                };
			}
		}

		public ProductXlsxExportProvider(
			ILanguageService languageService,
			IProductService productService,
			IStoreMappingService storeMappingService)
		{
			_languageService = languageService;
			_productService = productService;
			_storeMappingService = storeMappingService;
		}

		private void WriteCell(ExcelWorksheet worksheet, int row, ref int column, object value)
		{
			worksheet.Cells[row, column].Value = value;
			++column;
		}

		private string GetLocalized(List<dynamic> allLocalized, Language language, string property)
		{
			if (allLocalized != null)
			{
				string localizeValue = allLocalized
					.Where(x => property.IsCaseInsensitiveEqual((string)x.LocaleKey) && language.LanguageCulture.IsCaseInsensitiveEqual((string)x.Culture))
					.Select(x => (string)x.LocaleValue)
					.FirstOrDefault();

				return localizeValue;
			}
			return "";
		}

		public static string SystemName
		{
			get { return "Exports.SmartStoreProductXlsx"; }
		}

		public override ExportEntityType EntityType
		{
			get { return ExportEntityType.Product; }
		}

		public override string FileExtension
		{
			get { return "XLSX"; }
		}

		protected override void Export(IExportExecuteContext context)
		{
			using (var xlPackage = new ExcelPackage(context.DataStream))
			{
				// uncomment this line if you want the XML written out to the outputDir
				//xlPackage.DebugMode = true; 

				// get handle to the existing worksheet
				var worksheet = xlPackage.Workbook.Worksheets.Add("Products");

				// get handle to the cells range of the worksheet
				var cells = worksheet.Cells;
				string[] properties = Properties;
				var languages = _languageService.GetAllLanguages(true);

				var headlines = new string[properties.Length + languages.Count * 3];
				var languageFields = new string[languages.Count * 3];
				var row = 2;
				var j = 0;

				foreach (var lang in languages)
				{
					languageFields.SetValue("Name[" + lang.UniqueSeoCode + "]", j++);
					languageFields.SetValue("ShortDescription[" + lang.UniqueSeoCode + "]", j++);
					languageFields.SetValue("FullDescription[" + lang.UniqueSeoCode + "]", j++);
				}

				properties.CopyTo(headlines, 0);
				languageFields.CopyTo(headlines, properties.Length);

                for (int i = 0; i < headlines.Length; i++)
                {
                    cells[1, i + 1].Value = headlines[i];
                    cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
                    cells[1, i + 1].Style.Font.Bold = true;
                }

				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic product in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						Product entity = product.Entity;

						try
						{
							int column = 1;

							List<dynamic> productCategories = product.ProductCategories;
							List<dynamic> productManus = product.ProductManufacturers;
							List<dynamic> productPictures = product.ProductPictures;
							List<dynamic> localized = product._Localized;

							WriteCell(worksheet, row, ref column, entity.ProductTypeId);
							WriteCell(worksheet, row, ref column, entity.ParentGroupedProductId);
							WriteCell(worksheet, row, ref column, entity.VisibleIndividually);
							WriteCell(worksheet, row, ref column, (string)product.Name);
							WriteCell(worksheet, row, ref column, (string)product.ShortDescription);
							WriteCell(worksheet, row, ref column, (string)product.FullDescription);
							WriteCell(worksheet, row, ref column, entity.ProductTemplateId);
							WriteCell(worksheet, row, ref column, entity.ShowOnHomePage);
							WriteCell(worksheet, row, ref column, entity.HomePageDisplayOrder);
							WriteCell(worksheet, row, ref column, (string)product.MetaKeywords);
							WriteCell(worksheet, row, ref column, (string)product.MetaDescription);
							WriteCell(worksheet, row, ref column, (string)product.MetaTitle);
							WriteCell(worksheet, row, ref column, (string)product.SeName);
							WriteCell(worksheet, row, ref column, entity.AllowCustomerReviews);
							WriteCell(worksheet, row, ref column, entity.Published);
							WriteCell(worksheet, row, ref column, (string)product.Sku);
							WriteCell(worksheet, row, ref column, (string)product.ManufacturerPartNumber);
							WriteCell(worksheet, row, ref column, (string)product.Gtin);
							WriteCell(worksheet, row, ref column, entity.IsGiftCard);
							WriteCell(worksheet, row, ref column, entity.GiftCardTypeId);
							WriteCell(worksheet, row, ref column, entity.RequireOtherProducts);
							WriteCell(worksheet, row, ref column, entity.RequiredProductIds);
							WriteCell(worksheet, row, ref column, entity.AutomaticallyAddRequiredProducts);
							WriteCell(worksheet, row, ref column, entity.IsDownload);
							WriteCell(worksheet, row, ref column, entity.DownloadId);
							WriteCell(worksheet, row, ref column, entity.UnlimitedDownloads);
							WriteCell(worksheet, row, ref column, entity.MaxNumberOfDownloads);
							WriteCell(worksheet, row, ref column, entity.DownloadActivationTypeId);
							WriteCell(worksheet, row, ref column, entity.HasSampleDownload);
							WriteCell(worksheet, row, ref column, entity.SampleDownloadId);
							WriteCell(worksheet, row, ref column, entity.HasUserAgreement);
							WriteCell(worksheet, row, ref column, entity.UserAgreementText);
							WriteCell(worksheet, row, ref column, entity.IsRecurring);
							WriteCell(worksheet, row, ref column, entity.RecurringCycleLength);
							WriteCell(worksheet, row, ref column, entity.RecurringCyclePeriodId);
							WriteCell(worksheet, row, ref column, entity.RecurringTotalCycles);
							WriteCell(worksheet, row, ref column, entity.IsShipEnabled);
							WriteCell(worksheet, row, ref column, entity.IsFreeShipping);
							WriteCell(worksheet, row, ref column, entity.AdditionalShippingCharge);
							WriteCell(worksheet, row, ref column, entity.IsEsd);
							WriteCell(worksheet, row, ref column, entity.IsTaxExempt);
							WriteCell(worksheet, row, ref column, entity.TaxCategoryId);
							WriteCell(worksheet, row, ref column, entity.ManageInventoryMethodId);
							WriteCell(worksheet, row, ref column, entity.StockQuantity);
							WriteCell(worksheet, row, ref column, entity.DisplayStockAvailability);
							WriteCell(worksheet, row, ref column, entity.DisplayStockQuantity);
							WriteCell(worksheet, row, ref column, entity.MinStockQuantity);
							WriteCell(worksheet, row, ref column, entity.LowStockActivityId);
							WriteCell(worksheet, row, ref column, entity.NotifyAdminForQuantityBelow);
							WriteCell(worksheet, row, ref column, entity.BackorderModeId);
							WriteCell(worksheet, row, ref column, entity.AllowBackInStockSubscriptions);
							WriteCell(worksheet, row, ref column, entity.OrderMinimumQuantity);
							WriteCell(worksheet, row, ref column, entity.OrderMaximumQuantity);
							WriteCell(worksheet, row, ref column, entity.AllowedQuantities);
							WriteCell(worksheet, row, ref column, entity.DisableBuyButton);
							WriteCell(worksheet, row, ref column, entity.DisableWishlistButton);
							WriteCell(worksheet, row, ref column, entity.AvailableForPreOrder);
							WriteCell(worksheet, row, ref column, entity.CallForPrice);
							WriteCell(worksheet, row, ref column, (decimal)product.Price);
							WriteCell(worksheet, row, ref column, (decimal)product.OldPrice);
							WriteCell(worksheet, row, ref column, (decimal)product.ProductCost);
							WriteCell(worksheet, row, ref column, (decimal?)product.SpecialPrice);
							WriteCell(worksheet, row, ref column, entity.SpecialPriceStartDateTimeUtc.HasValue ? entity.SpecialPriceStartDateTimeUtc.Value.ToOADate() : (double)0.0);
							WriteCell(worksheet, row, ref column, entity.SpecialPriceEndDateTimeUtc.HasValue ? entity.SpecialPriceEndDateTimeUtc.Value.ToOADate() : (double)0.0);
							WriteCell(worksheet, row, ref column, entity.CustomerEntersPrice);
							WriteCell(worksheet, row, ref column, entity.MinimumCustomerEnteredPrice);
							WriteCell(worksheet, row, ref column, entity.MaximumCustomerEnteredPrice);
							WriteCell(worksheet, row, ref column, entity.Weight);
							WriteCell(worksheet, row, ref column, entity.Length);
							WriteCell(worksheet, row, ref column, entity.Width);
							WriteCell(worksheet, row, ref column, entity.Height);
							WriteCell(worksheet, row, ref column, entity.CreatedOnUtc.ToOADate());

							WriteCell(worksheet, row, ref column, string.Join(";", productCategories.Select(x => (int)x.CategoryId)));
							WriteCell(worksheet, row, ref column, string.Join(";", productManus.Select(x => (int)x.ManufacturerId)));

							WriteCell(worksheet, row, ref column, productPictures.Count > 0 ? (string)productPictures[0].Picture._ThumbLocalPath : null);
							WriteCell(worksheet, row, ref column, productPictures.Count > 1 ? (string)productPictures[1].Picture._ThumbLocalPath : null);
							WriteCell(worksheet, row, ref column, productPictures.Count > 2 ? (string)productPictures[2].Picture._ThumbLocalPath : null);

							WriteCell(worksheet, row, ref column, entity.DeliveryTimeId);
							WriteCell(worksheet, row, ref column, entity.QuantityUnitId);
							WriteCell(worksheet, row, ref column, entity.BasePriceEnabled);
							WriteCell(worksheet, row, ref column, entity.BasePriceMeasureUnit);
							WriteCell(worksheet, row, ref column, entity.BasePriceAmount);
							WriteCell(worksheet, row, ref column, entity.BasePriceBaseAmount);
							WriteCell(worksheet, row, ref column, entity.BundleTitleText);
							WriteCell(worksheet, row, ref column, entity.BundlePerItemShipping);
							WriteCell(worksheet, row, ref column, entity.BundlePerItemShipping);
							WriteCell(worksheet, row, ref column, entity.BundlePerItemShoppingCart);

							if ((int)product.ProductTypeId == (int)ProductType.BundledProduct)
							{
								List<dynamic> bundleItems = product.ProductBundleItems;

								var searchContext = new ProductSearchContext
								{
									ProductIds = bundleItems.Select(x => (int)x.ProductId).ToList(),
									PageSize = int.MaxValue,
									ShowHidden = true
								};

								var query = _productService.PrepareProductSearchQuery<string>(searchContext, x => x.Sku);
								var skus = query.ToList();

								WriteCell(worksheet, row, ref column, string.Join(",", skus));
							}
							else
							{
								WriteCell(worksheet, row, ref column, "");
							}

							WriteCell(worksheet, row, ref column, entity.AvailableStartDateTimeUtc);
							WriteCell(worksheet, row, ref column, entity.AvailableEndDateTimeUtc);

							if ((bool)product.LimitedToStores)
							{
								var storeIds = _storeMappingService.GetStoreMappingsFor("Product", (int)product.Id)
									.Select(x => x.StoreId)
									.ToList();

								WriteCell(worksheet, row, ref column, string.Join(";", storeIds));
							}
							else
							{
								WriteCell(worksheet, row, ref column, "");
							}

							WriteCell(worksheet, row, ref column, entity.LimitedToStores);

							foreach (var lang in languages)
							{
								WriteCell(worksheet, row, ref column, GetLocalized(localized, lang, "Name"));
								WriteCell(worksheet, row, ref column, GetLocalized(localized, lang, "ShortDescription"));
								WriteCell(worksheet, row, ref column, GetLocalized(localized, lang, "FullDescription"));
							}

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, entity.Id);
						}

						++row;
					}
				}

				// we had better add some document properties to the spreadsheet 

				// set some core property values
				//var storeName = _storeInformationSettings.StoreName;
				//var storeUrl = _storeInformationSettings.StoreUrl;
				//xlPackage.Workbook.Properties.Title = string.Format("{0} products", storeName);
				//xlPackage.Workbook.Properties.Author = storeName;
				//xlPackage.Workbook.Properties.Subject = string.Format("{0} products", storeName);
				//xlPackage.Workbook.Properties.Keywords = string.Format("{0} products", storeName);
				//xlPackage.Workbook.Properties.Category = "Products";
				//xlPackage.Workbook.Properties.Comments = string.Format("{0} products", storeName);

				// set some extended property values
				//xlPackage.Workbook.Properties.Company = storeName;
				//xlPackage.Workbook.Properties.HyperlinkBase = new Uri(storeUrl);

				// save the new spreadsheet
				xlPackage.Save();
			}

			// EPPLus had serious memory leak problems in V3. We enforce the garbage collector to release unused memory, it's not perfect, but better than nothing.
			GC.Collect();
		}
	}
}
