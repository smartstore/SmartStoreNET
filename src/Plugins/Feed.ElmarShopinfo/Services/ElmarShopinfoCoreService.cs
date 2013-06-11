using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using SmartStore.Core;
using SmartStore.Services.Catalog;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.Plugins;
using System;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using System.Globalization;
using SmartStore.Plugin.Feed.ElmarShopinfo.Models;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Infrastructure;
using System.Web;
using System.Collections.Generic;
using SmartStore.Core.Data;
using SmartStore.Services.Payments;
using System.Web.Mvc;
using SmartStore.Core.Fakes;
using System.Web.Routing;
using SmartStore.Services.Stores;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Plugin.Feed.ElmarShopinfo.Services
{
	public partial class ElmarShopinfoCoreService : IElmarShopinfoCoreService
	{
		private const string _captions = "'Produktnummmer';'Hersteller';'Verfuegbarkeit';'Versandkosten';'EAN';'ISBN';'LangText';'Produktname';'Bild-URL';'Preis';'Beschreibung';'Sonderangebot';'Warengruppe';'Einheit';'Produkt-URL';'Garantie'";
		private readonly PluginHelperFeed _helper;
		private readonly IProductService _productService;
		private readonly IManufacturerService _manufacturerService;
		private readonly ICategoryService _categoryService;
		private readonly IRepository<ProductCategory> _productCategoryRepository;
		private readonly IPaymentService _paymentService;
		private readonly IStoreService _storeService;

		public ElmarShopinfoCoreService(
			ElmarShopinfoSettings settings,
			IProductService productService,
			IManufacturerService manufacturerService,
			ICategoryService categoryService,
			IRepository<ProductCategory> productCategoryRepository,
			IPaymentService paymentService, 
			IStoreService storeService) {

			Settings = settings;
			_productService = productService;
			_manufacturerService = manufacturerService;
			_categoryService = categoryService;
			_productCategoryRepository = productCategoryRepository;
			_paymentService = paymentService;
			_storeService = storeService;

			_helper = new PluginHelperFeed("PromotionFeed.ElmarShopinfo", "SmartStore.Plugin.Feed.ElmarShopinfo", () => {
				return Settings as PromotionFeedSettings;
			});
		}

		public ElmarShopinfoSettings Settings { get; set; }
		public PluginHelperFeed Helper { get { return _helper; } }

		private string Availability(ProductVariant variant) {
			if (Settings.Availability.IsCaseInsensitiveEqual(PluginHelperFeed.NotSpecified))
				return "";

			if (Settings.Availability.IsNullOrEmpty()) {
				if (variant.ManageInventoryMethod == ManageInventoryMethod.ManageStock && variant.StockQuantity <= 0) {
					switch (variant.BackorderMode) {
						case BackorderMode.NoBackorders:
							return Helper.Resource("Products.Availability.OutOfStock");	// out of stock
					}
				}
				return Helper.Resource("Products.Availability.InStock");	// in stock
			}

			return Settings.Availability;
		}
		private void WriteMapping(XmlWriter writer, int column, string name, string type) {
			writer.WriteStartElement("Mapping");
			writer.WriteAttributeString("column", column.ToString());
			writer.WriteAttributeString("columnName", name);
			writer.WriteAttributeString("type", type);
			writer.WriteEndElement();
		}
		private void WritePayment(XmlWriter writer) {
			var payments = _paymentService.LoadActivePaymentMethods();

			if (payments.Exists(p => p.PluginDescriptor.SystemName == "Payments.PayPalStandard" || p.PluginDescriptor.SystemName == "Payments.PostFinanceECommerce")) {
				writer.WriteNode("Item", () => {
					writer.WriteElementString("Name", "debit");
				});
			}

			if (payments.Exists(p => p.PluginDescriptor.SystemName == "Payments.CashOnDelivery")) {
				writer.WriteNode("Item", () => {
					writer.WriteElementString("Name", "on delivery");
				});
			}

			if (payments.Exists(p => p.PluginDescriptor.SystemName == "Payments.PayPalDirect" || p.PluginDescriptor.SystemName == "Payments.PayPalStandard")) {
				writer.WriteNode("Item", () => {
					writer.WriteElementString("Name", "paypal");
				});
			}

			if (payments.Exists(p => p.PluginDescriptor.SystemName == "Payments.Sofortueberweisung")) {
				writer.WriteNode("Item", () => {
					writer.WriteElementString("Name", "money transfer");
				});
			}

			if (payments.Exists(p => p.PluginDescriptor.SystemName == "Payments.PayPalDirect" || p.PluginDescriptor.SystemName == "Payments.IPaymentCreditCard" || p.PluginDescriptor.SystemName == "Payments.PostFinanceECommerce")) {
				writer.WriteNode("Item", () => {
					writer.WriteElementString("Name", "amex");
				});
				writer.WriteNode("Item", () => {
					writer.WriteElementString("Name", "visa");
				});
			}

			if (payments.Exists(p => p.PluginDescriptor.SystemName == "Payments.IPaymentCreditCard" || p.PluginDescriptor.SystemName == "Payments.PostFinanceECommerce")) {
				writer.WriteNode("Item", () => {
					writer.WriteElementString("Name", "diners");
				});
				writer.WriteNode("Item", () => {
					writer.WriteElementString("Name", "eurocard");
				});
			}
		}
		private string WriteItem(StreamWriter writer, Store store, Product product, ProductVariant variant, ProductManufacturer manu, Currency currency, StringBuilder sb) {
			sb.Clear();

			string shippingTime = (variant.DeliveryTime == null ? Settings.ShippingTime : variant.DeliveryTime.Name);
			string brand = (manu != null && manu.Manufacturer.Name.HasValue() ? manu.Manufacturer.Name : Settings.Brand);
			string measureUnit = (variant.BasePrice != null && variant.BasePrice.Enabled ? variant.BasePrice.MeasureUnit : "");
			string imageUrl = Helper.MainProductImageUrl(store, product, variant);
			string description = Helper.BuildProductDescription(product, variant, manu);
			
			string specialPrice = (Settings.ExportSpecialPrice ? Helper.SpecialPrice(variant, true) : "");
			string price = (specialPrice.HasValue() ? specialPrice : Helper.DecimalUsFormat(Helper.ConvertFromStoreCurrency(variant.Price, currency)));

			sb.AppendFormat("'{0}';'{1}';'{2}';'{3}';'{4}';'{5}';'{6}';'{7}';'{8}';'{9}';'{10}';'{11}';'{12}';'{13}';'{14}';'{15}'",
				Helper.ReplaceCsvChars(variant.Sku),
				Helper.ReplaceCsvChars(brand),
				Helper.ReplaceCsvChars(Availability(variant)),
				Helper.ReplaceCsvChars(Helper.ShippingCost(variant)),
				Helper.ReplaceCsvChars(variant.Gtin),
				Settings.ExportEanAsIsbn ? Helper.ReplaceCsvChars(variant.Gtin) : "",
				Helper.ReplaceCsvChars(description),
				Helper.ReplaceCsvChars(variant.FullProductName),
				Helper.ReplaceCsvChars(imageUrl),
				price,
				Helper.ReplaceCsvChars(product.ShortDescription),
				specialPrice.HasValue() ? "1" : "0",
				Helper.ReplaceCsvChars(Helper.CategoryName(product)),
				Helper.ReplaceCsvChars(measureUnit),
				Helper.ReplaceCsvChars(Helper.ProductDetailUrl(store, product)),
				Helper.ReplaceCsvChars(Settings.Warranty)
			);

			writer.WriteLine(sb.ToString());
			return null;
		}
		private string CreateXml(Stream stream, Store store, GeneratedFeedFile feedFile, int productCount, IPagedList<Category> categories, bool hasGiftCards, Currency currency)
		{
			var xmlSettings = new XmlWriterSettings {
				Encoding = Encoding.GetEncoding("ISO-8859-1"),
				Indent = true
			};

			using (var writer = XmlWriter.Create(stream, xmlSettings)) {
				writer.WriteStartDocument();
				writer.WriteStartElement("osp", "Shop", "http://elektronischer-markt.de/schema");

				writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
				writer.WriteAttributeString("xsi", "schemaLocation", null, "http://elektronischer-markt.de/schema http://kuhlins.de/elmar/schema/shop.xsd");

				writer.WriteNode("Common", () => {
					writer.WriteElementString("Version", "1.1");
					writer.WriteElementString("Language", Helper.Language.UniqueSeoCode.ToLower());
					writer.WriteElementString("Currency", currency.CurrencyCode);
				});

				writer.WriteCData("Name", store.Name);
				writer.WriteCData("Url", store.Url);

				writer.WriteNode("Requests", () => {
					writer.WriteNode("OfflineRequest", () => {
						writer.WriteNode("UpdateMethods", () => {
							writer.WriteNode("DirectDownload", () => {
								writer.WriteAttributeString("day", Settings.UpdateDay);
								writer.WriteAttributeString("from", Settings.UpdateHourFrom);
								writer.WriteAttributeString("to", Settings.UpdateHourTo);
							});
						});

						writer.WriteNode("Format", () => {
							writer.WriteNode("Tabular", () => {
								writer.WriteNode("CSV", () => {
									writer.WriteCData("Url", feedFile.FileUrl);
									writer.WriteNode("Header", () => {
										writer.WriteAttributeString("columns", (_captions.Count(c => c == ';') + 1).ToString());
										writer.WriteCData(_captions);
									});
									writer.WriteNode("SpecialCharacters", () => {
										writer.WriteAttributeString("delimiter", ";");
										writer.WriteAttributeString("escaped", "\\");
										writer.WriteAttributeString("quoted", "'");
									});
								});
								writer.WriteNode("Mappings", () => {
									WriteMapping(writer, 1, "Produktnummmer", "privateid");
									WriteMapping(writer, 2, "Hersteller", "brand");
									WriteMapping(writer, 3, "Verfuegbarkeit", "deliverable");
									WriteMapping(writer, 4, "Versandkosten", "deliverydetails");
									WriteMapping(writer, 5, "EAN", "ean");
									WriteMapping(writer, 6, "ISBN", "isbn");
									WriteMapping(writer, 7, "LangText", "longdescription");
									WriteMapping(writer, 8, "Produktname", "name");
									WriteMapping(writer, 9, "Bild-URL", "pictureurl");
									WriteMapping(writer, 10, "Preis", "price");
									WriteMapping(writer, 11, "Beschreibung", "shortdescription");
									WriteMapping(writer, 12, "Sonderangebot", "specialdiscount");
									WriteMapping(writer, 13, "Warengruppe", "type");
									WriteMapping(writer, 14, "Einheit", "unit");
									WriteMapping(writer, 15, "Produkt-URL", "url");
									WriteMapping(writer, 16, "Garantie", "warranty");
								});
							});
						});
					});
				});

				writer.WriteCData("Logo", Helper.StoreLogoUrl);

				writer.WriteNode("Address", () => {
					writer.WriteAttributeString("sale", Settings.BranchOffice ? "yes" : "no");
					writer.WriteCData("Company", Settings.AddressName);
					writer.WriteCData("Street", Settings.AddressStreet);
					writer.WriteCData("Postcode", Settings.AddressPostalCode);
					writer.WriteCData("City", Settings.AddressCity);
				});

				writer.WriteNode("Contact", () => {
					writer.WriteCData("PublicMailAddress", Settings.PublicMail);
					writer.WriteCData("PrivateMailAddress", Settings.PrivateMail);
					if (Settings.OrderPhone.HasValue()) {
						writer.WriteNode("OrderPhone", () => {
							writer.WriteCData("Number", Settings.OrderPhone);
						});
					}
					if (Settings.OrderFax.HasValue()) {
						writer.WriteNode("OrderFax", () => {
							writer.WriteCData("Number", Settings.OrderFax);
						});
					}
					if (Settings.Hotline.HasValue()) {
						writer.WriteNode("Hotline", () => {
							writer.WriteCData("Number", Settings.Hotline);
							if (Settings.ExportHotlineCost)
								writer.WriteElementString("CostPerMinute", Helper.DecimalUsFormat(Settings.HotlineCostPerMinute));
						});
					}
				});

				writer.WriteNode("Categories", () => {
					writer.WriteElementString("TotalProductCount", productCount.ToString());

					foreach (var item in categories.Where(c => c.PictureId > 0)) {
						writer.WriteNode("Item", () => {
							writer.WriteCData("Name", item.Name);
							writer.WriteElementString("ProductCount", item.PictureId.ToString());
							writer.WriteCData("Mapping", Settings.CategoryMapping);
						});
					}
				});

				writer.WriteNode("Payment", () => {
					WritePayment(writer);
				});

				writer.WriteNode("ForwardExpenses", () => {
					writer.WriteElementString("FlatRate", Helper.DecimalUsFormat(Settings.ShippingCost));
				});

				if (hasGiftCards) {
					writer.WriteNode("Features", () => {
						writer.WriteNode("GiftService", () => {
							writer.WriteAttributeString("surcharge", "no");
						});
					});
				}

				writer.WriteNode("Technology", () => {
					writer.WriteElementString("SSL", null);
					writer.WriteElementString("Search", null);
				});

				writer.WriteElementString("Self-Description", "-");

				writer.WriteEndElement();	// osp:Shop
				writer.WriteEndDocument();
			}
			return null;
		}

		public virtual void CreateFeed(Store store, GeneratedFeedFile feedFile, Stream streamCsv, Stream streamXml) {
			string breakingError = null;
			int productCount = 0;
			bool hasGiftCards = false;
			var sb = new StringBuilder();
			var currency = Helper.GetUsedCurrency(Settings.CurrencyId);
			var categories = _categoryService.GetAllCategories();
			Category category;

			var ctx = new ProductSearchContext()
			{
				OrderBy = ProductSortingEnum.CreatedOn,
				PageSize = int.MaxValue,
				StoreId = store.Id
			};

			var products = _productService.SearchProducts(ctx);

			foreach (var item in categories) {
				item.PictureId = 0;		// we misuse that as counter
			}

			var categoryMapping = (
				from m in _productCategoryRepository.Table
				select new {
					CategoryID = m.CategoryId,
					ProductID = m.ProductId
				}).ToList();


			using (var writer = new StreamWriter(streamCsv, Encoding.Default)) {
				sb.Append(_captions);
				writer.WriteLine(sb.ToString());

				foreach (var product in products) {
					var manufacturer = _manufacturerService.GetProductManufacturersByProductId(product.Id).FirstOrDefault();
					var variants = _productService.GetProductVariantsByProductId(product.Id, false);

					foreach (var variant in variants.Where(v => v.Published)) {
						try {
							breakingError = WriteItem(writer, store, product, variant, manufacturer, currency, sb);
							++productCount;

							if (variant.IsGiftCard)
								hasGiftCards = true;

							foreach (var mapping in categoryMapping.Where(m => m.ProductID == product.Id)) {
								if ((category = categories.FirstOrDefault(c => c.Id == mapping.CategoryID)) != null) {
									++category.PictureId;	// we misuse that as counter
								}
							}
						}
						catch (Exception exc) {
							exc.Dump();
						}

						if (breakingError.HasValue())
							break;
					}
					if (breakingError.HasValue())
						break;
				}
			}

			breakingError = CreateXml(streamXml, store, feedFile, productCount, categories, hasGiftCards, currency);

			if (breakingError.HasValue())
				throw new SmartException(breakingError);
		}
		public virtual void CreateFeed()
		{
			var storeLocation = Helper.StoreLocation;
			var stores = new List<Store>();

			if (Settings.StoreId != 0)
			{
				var storeById = _storeService.GetStoreById(Settings.StoreId);
				if (storeById != null)
					stores.Add(storeById);
			}

			if (stores.Count == 0)
			{
				stores.AddRange(_storeService.GetAllStores());
			}

			foreach (var store in stores)
			{
				var feedFile = Helper.FeedFileByStore(store, storeLocation, Settings.StaticFileNameXml);
				if (feedFile != null)
				{
					using (var streamCsv = new FileStream(feedFile.FilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
					using (var streamXml = new FileStream(feedFile.CustomProperties["SecondFilePath"] as string, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
					{
						CreateFeed(store, feedFile, streamCsv, streamXml);
					}
				}
			}
		}
		public virtual void SetupModel(FeedElmarShopinfoModel model, ScheduleTask task = null) {
			CultureInfo culture = new CultureInfo(Helper.Language.LanguageCulture);
			var stores = _storeService.GetAllStores().ToList();

			model.AvailableCurrencies = Helper.AvailableCurrencies();
			model.GeneratedFiles = Helper.FeedFiles(stores, Settings.StaticFileNameXml);
			model.ElmarCategories = new List<SelectListItem>();
			model.HourList = new List<SelectListItem> {
				new	SelectListItem { Text = Helper.Resource("Common.Unspecified"), Value = "" }
			};

			model.AvailableStores.Add(new SelectListItem() { Text = Helper.Resource("Admin.Common.All"), Value = "0" });
			model.AvailableStores.AddRange(_storeService.GetAllStoresAsListItems(stores));

			model.DayList = new List<SelectListItem> {
				new	SelectListItem { Text = Helper.Resource("Daily"), Value = "daily" },
				new	SelectListItem { Text = culture.DateTimeFormat.GetDayName(DayOfWeek.Monday), Value = "mon" },
				new	SelectListItem { Text = culture.DateTimeFormat.GetDayName(DayOfWeek.Tuesday), Value = "tue" },
				new	SelectListItem { Text = culture.DateTimeFormat.GetDayName(DayOfWeek.Wednesday), Value = "wed" },
				new	SelectListItem { Text = culture.DateTimeFormat.GetDayName(DayOfWeek.Thursday), Value = "thu" },
				new	SelectListItem { Text = culture.DateTimeFormat.GetDayName(DayOfWeek.Friday), Value = "fri" },
				new	SelectListItem { Text = culture.DateTimeFormat.GetDayName(DayOfWeek.Saturday), Value = "sat" },
				new	SelectListItem { Text = culture.DateTimeFormat.GetDayName(DayOfWeek.Sunday), Value = "sun" }
			};

			if (task != null) {
				model.GenerateStaticFileEachMinutes = task.Seconds / 60;
				model.TaskEnabled = task.Enabled;
			}

			var elmarCategories = Helper.Resource("ElmarCategories").SplitSafe(";");
			foreach (string item in elmarCategories) {
				model.ElmarCategories.Add(new SelectListItem {
					Text = item,
					Value = item
				});
			}

			var listItem = model.ElmarCategories.FirstOrDefault(c => c.Value.IsCaseInsensitiveEqual(Settings.CategoryMapping));
			if (listItem != null) {
				listItem.Selected = true;
			}

			for (int i = 0; i < 24; ++i) {
				model.HourList.Add(new SelectListItem {
					Text = "{0:00}:00".FormatWith(i),
					Value = "{0}00".FormatWith(i)
				});
			}			
		}
	}	// class
}
