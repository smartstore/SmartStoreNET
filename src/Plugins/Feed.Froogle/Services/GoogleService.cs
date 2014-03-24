using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Catalog;
using SmartStore.Plugin.Feed.Froogle.Domain;
using SmartStore.Plugin.Feed.Froogle.Models;
using SmartStore.Web.Framework.Plugins;
using Telerik.Web.Mvc;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Services.Stores;
using SmartStore.Core.Domain.Stores;
using System.Web.Mvc;
using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Services.Directory;
using SmartStore.Core;

namespace SmartStore.Plugin.Feed.Froogle.Services
{
    public partial class GoogleService : IGoogleService
    {
		private const string _googleNamespace = "http://base.google.com/ns/1.0";

		private readonly PluginHelperFeed _helper;
        private readonly IRepository<GoogleProductRecord> _gpRepository;
		private readonly IProductService _productService;
		private readonly IManufacturerService _manufacturerService;
		private readonly IStoreService _storeService;
		private readonly ICategoryService _categoryService;
		private readonly IMeasureService _measureService;
		private readonly MeasureSettings _measureSettings;
		private readonly IPriceCalculationService _priceCalculationService;
		private readonly IWorkContext _workContext;

		public GoogleService(
			IRepository<GoogleProductRecord> gpRepository,
			IProductService productService,
			IManufacturerService manufacturerService,
			IStoreService storeService,
			ICategoryService categoryService,
			FroogleSettings settings,
			IMeasureService measureService,
			MeasureSettings measureSettings,
			IPriceCalculationService priceCalculationService,
			IWorkContext workContext)
        {
            _gpRepository = gpRepository;
			_productService = productService;
			_manufacturerService = manufacturerService;
			_storeService = storeService;
			_categoryService = categoryService;
			Settings = settings;
			_measureService = measureService;
			_measureSettings = measureSettings;
			_priceCalculationService = priceCalculationService;
			_workContext = workContext;

			_helper = new PluginHelperFeed("PromotionFeed.Froogle", "SmartStore.Plugin.Feed.Froogle", () =>
			{
				return Settings as PromotionFeedSettings;
			});
        }

		public FroogleSettings Settings { get; set; }
		public PluginHelperFeed Helper { get { return _helper; } }

        private GoogleProductRecord GetByProductId(int productId)
        {
            if (productId == 0)
                return null;

            var query = from gp in _gpRepository.Table
                        where gp.ProductId == productId
                        orderby gp.Id
                        select gp;
            var record = query.FirstOrDefault();
            return record;
        }
        private void InsertGoogleProductRecord(GoogleProductRecord googleProductRecord)
        {
            if (googleProductRecord == null)
                throw new ArgumentNullException("googleProductRecord");

            _gpRepository.Insert(googleProductRecord);
        }
        private void UpdateGoogleProductRecord(GoogleProductRecord googleProductRecord)
        {
            if (googleProductRecord == null)
                throw new ArgumentNullException("googleProductRecord");

            _gpRepository.Update(googleProductRecord);
        }
		private bool SpecialPrice(Product product, out string specialPriceDate)
		{
			specialPriceDate = "";

			try
			{
				if (Settings.SpecialPrice && product.SpecialPrice.HasValue && product.SpecialPriceStartDateTimeUtc.HasValue && product.SpecialPriceEndDateTimeUtc.HasValue)
				{
					if (!(product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing))
					{
						string dateFormat = "yyyy-MM-ddTHH:mmZ";
						string startDate = product.SpecialPriceStartDateTimeUtc.Value.ToString(dateFormat);
						string endDate = product.SpecialPriceEndDateTimeUtc.Value.ToString(dateFormat);

						specialPriceDate = "{0}/{1}".FormatWith(startDate, endDate);
					}
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
			return specialPriceDate.HasValue();
		}
		private string ProductCategory(GoogleProductRecord googleProduct)
		{
			string productCategory = "";

			if (googleProduct != null)
				productCategory = googleProduct.Taxonomy;

			if (productCategory.IsNullOrEmpty())
				productCategory = Settings.DefaultGoogleCategory;

			return productCategory;
		}
		private string Condition()
		{
			if (Settings.Condition.IsCaseInsensitiveEqual(PluginHelperBase.NotSpecified))
				return "";

			if (Settings.Condition.IsNullOrEmpty())
				return "new";

			return Settings.Condition;
		}
		private string Availability(Product product)
		{
			if (Settings.Availability.IsCaseInsensitiveEqual(PluginHelperBase.NotSpecified))
				return "";

			if (Settings.Availability.IsNullOrEmpty())
			{
				if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock && product.StockQuantity <= 0)
				{
					switch (product.BackorderMode)
					{
						case BackorderMode.NoBackorders:
							return "out of stock";
						case BackorderMode.AllowQtyBelow0:
						case BackorderMode.AllowQtyBelow0AndNotifyCustomer:
							return "available for order";
					}
				}
				return "in stock";
			}
			return Settings.Availability;
		}
		private string Gender(GoogleProductRecord googleProduct)
		{
			if (Settings.Gender.IsCaseInsensitiveEqual(PluginHelperBase.NotSpecified))
				return "";

			if (googleProduct != null && googleProduct.Gender.HasValue())
				return googleProduct.Gender;

			return Settings.Gender;
		}
		private string AgeGroup(GoogleProductRecord googleProduct)
		{
			if (Settings.AgeGroup.IsCaseInsensitiveEqual(PluginHelperBase.NotSpecified))
				return "";

			if (googleProduct != null && googleProduct.AgeGroup.HasValue())
				return googleProduct.AgeGroup;

			return Settings.AgeGroup;
		}
		private string Color(GoogleProductRecord googleProduct)
		{
			if (googleProduct != null && googleProduct.Color.HasValue())
				return googleProduct.Color;

			return Settings.Color;
		}
		private string Size(GoogleProductRecord googleProduct)
		{
			if (googleProduct != null && googleProduct.Size.HasValue())
				return googleProduct.Size;

			return Settings.Size;
		}
		private string Material(GoogleProductRecord googleProduct)
		{
			if (googleProduct != null && googleProduct.Material.HasValue())
				return googleProduct.Material;

			return Settings.Material;
		}
		private string Pattern(GoogleProductRecord googleProduct)
		{
			if (googleProduct != null && googleProduct.Pattern.HasValue())
				return googleProduct.Pattern;

			return Settings.Pattern;
		}
		private string ItemGroupId(GoogleProductRecord googleProduct)
		{
			if (googleProduct != null && googleProduct.ItemGroupId.HasValue())
				return googleProduct.ItemGroupId;

			return "";
		}
		private bool BasePriceSupported(int baseAmount, string unit)
		{
			if (baseAmount == 1 || baseAmount == 10 || baseAmount == 100)
				return true;

			if (baseAmount == 75 && unit == "cl")
				return true;

			if ((baseAmount == 50 || baseAmount == 1000) && unit == "kg")
				return true;

			return false;
		}
		private string BasePriceUnits(string value)
		{
			const string defaultValue = "kg";

			if (value.IsNullOrEmpty())
				return defaultValue;

			// TODO: Product.BasePriceMeasureUnit should be localized
			switch (value.ToLower())
			{
				case "mg":
				case "milligramm":
				case "milligram":
					return "mg";
				case "g":
				case "gramm":
				case "gram":
					return "g";
				case "kg":
				case "kilogramm":
				case "kilogram":
					return "kg";

				case "ml":
				case "milliliter":
				case "millilitre":
					return "ml";
				case "cl":
				case "zentiliter":
				case "centilitre":
					return "cl";
				case "l":
				case "liter":
				case "litre":
					return "l";
				case "cbm":
				case "kubikmeter":
				case "cubic metre":
					return "cbm";

				case "cm":
				case "zentimeter":
				case "centimetre":
					return "cm";
				case "m":
				case "meter":
					return "m";

				case "qm²":
				case "quadratmeter":
				case "square metre":
					return "sqm";

				default:
					return defaultValue;
			}
		}
		private string WriteItem(XmlWriter writer, Store store, Product product, Currency currency)
		{
			var manu = _manufacturerService.GetProductManufacturersByProductId(product.Id).FirstOrDefault();
			var mainImageUrl = Helper.MainProductImageUrl(store, product);
			var googleProduct = GetByProductId(product.Id);
			var category = ProductCategory(googleProduct);

			if (category.IsNullOrEmpty())
				return Helper.Resource("MissingDefaultCategory");

			var brand = (manu != null && manu.Manufacturer.Name.HasValue() ? manu.Manufacturer.Name : Settings.Brand);
			var mpn = Helper.ManufacturerPartNumber(product);

			bool identifierExists = product.Gtin.HasValue() || brand.HasValue() || mpn.HasValue();

			writer.WriteElementString("g", "id", _googleNamespace, product.Id.ToString());

			writer.WriteStartElement("title");
			writer.WriteCData(product.Name.Truncate(70));
			writer.WriteEndElement();

			var description = Helper.BuildProductDescription(product, manu, d =>
			{
				if (product.FullDescription.IsNullOrEmpty() && product.ShortDescription.IsNullOrEmpty())
				{
					var rnd = new Random();

					switch (rnd.Next(1, 5))
					{
						case 1: return d.Grow(Settings.AppendDescriptionText1, " ");
						case 2: return d.Grow(Settings.AppendDescriptionText2, " ");
						case 3: return d.Grow(Settings.AppendDescriptionText3, " ");
						case 4: return d.Grow(Settings.AppendDescriptionText4, " ");
						case 5: return d.Grow(Settings.AppendDescriptionText5, " ");
					}
				}
				return d;
			});

			writer.WriteStartElement("description");
			writer.WriteCData(description);
			writer.WriteEndElement();

			writer.WriteStartElement("g", "google_product_category", _googleNamespace);
			writer.WriteCData(category);
			writer.WriteFullEndElement(); 

			string productType = _categoryService.GetCategoryBreadCrumb(product);
			if (productType.HasValue())
			{
				writer.WriteStartElement("g", "product_type", _googleNamespace);
				writer.WriteCData(productType);
				writer.WriteFullEndElement();
			}

			writer.WriteElementString("link", Helper.ProductDetailUrl(store, product));
			writer.WriteElementString("g", "image_link", _googleNamespace, mainImageUrl);

			foreach (string additionalImageUrl in Helper.AdditionalProductImages(store, product, mainImageUrl))
			{
				writer.WriteElementString("g", "additional_image_link", _googleNamespace, additionalImageUrl);
			}

			writer.WriteElementString("g", "condition", _googleNamespace, Condition());
			writer.WriteElementString("g", "availability", _googleNamespace, Availability(product));

			decimal priceBase = _priceCalculationService.GetFinalPrice(product, null, _workContext.CurrentCustomer, decimal.Zero, true, 1);
			decimal price = Helper.ConvertFromStoreCurrency(priceBase, currency);
			string specialPriceDate;

			if (SpecialPrice(product, out specialPriceDate))
			{
				writer.WriteElementString("g", "sale_price", _googleNamespace, Helper.DecimalUsFormat(price) + " " + currency.CurrencyCode);
				writer.WriteElementString("g", "sale_price_effective_date", _googleNamespace, specialPriceDate);

				// get regular price
				decimal specialPrice = product.SpecialPrice.Value;
				product.SpecialPrice = null;
				priceBase = _priceCalculationService.GetFinalPrice(product, null, _workContext.CurrentCustomer, decimal.Zero, true, 1);
				product.SpecialPrice = specialPrice;
				price = Helper.ConvertFromStoreCurrency(priceBase, currency);
			}

			writer.WriteElementString("g", "price", _googleNamespace, Helper.DecimalUsFormat(price) + " " + currency.CurrencyCode);

			writer.WriteCData("gtin", product.Gtin, "g", _googleNamespace);
			writer.WriteCData("brand", brand, "g", _googleNamespace);
			writer.WriteCData("mpn", mpn, "g", _googleNamespace);

			writer.WriteCData("gender", Gender(googleProduct), "g", _googleNamespace);
			writer.WriteCData("age_group", AgeGroup(googleProduct), "g", _googleNamespace);
			writer.WriteCData("color", Color(googleProduct), "g", _googleNamespace);
			writer.WriteCData("size", Size(googleProduct), "g", _googleNamespace);
			writer.WriteCData("material", Material(googleProduct), "g", _googleNamespace);
			writer.WriteCData("pattern", Pattern(googleProduct), "g", _googleNamespace);
			writer.WriteCData("item_group_id", ItemGroupId(googleProduct), "g", _googleNamespace);

			writer.WriteElementString("g", "online_only", _googleNamespace, Settings.OnlineOnly ? "y" : "n");
			writer.WriteElementString("g", "identifier_exists", _googleNamespace, identifierExists ? "TRUE" : "FALSE");

			if (Settings.ExpirationDays > 0)
			{
				writer.WriteElementString("g", "expiration_date", _googleNamespace, DateTime.UtcNow.AddDays(Settings.ExpirationDays).ToString("yyyy-MM-dd"));
			}

			if (Settings.ExportShipping)
			{
				string weightInfo, weight = Helper.DecimalUsFormat(product.Weight);
				string systemKey = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).SystemKeyword;

				if (systemKey.IsCaseInsensitiveEqual("gram"))
					weightInfo = weight + " g";
				else if (systemKey.IsCaseInsensitiveEqual("lb"))
					weightInfo = weight + " lb";
				else if (systemKey.IsCaseInsensitiveEqual("ounce"))
					weightInfo = weight + " oz";
				else
					weightInfo = weight + " kg";

				writer.WriteElementString("g", "shipping_weight", _googleNamespace, weightInfo);
			}

			if (Settings.ExportBasePrice && product.BasePriceHasValue)
			{
				string measureUnit = BasePriceUnits(product.BasePriceMeasureUnit);

				if (BasePriceSupported(product.BasePriceBaseAmount ?? 0, measureUnit))
				{
					string basePriceMeasure = "{0} {1}".FormatWith(Helper.DecimalUsFormat(product.BasePriceAmount ?? decimal.Zero), measureUnit);
					string basePriceBaseMeasure = "{0} {1}".FormatWith(product.BasePriceBaseAmount, measureUnit);

					writer.WriteElementString("g", "unit_pricing_measure", _googleNamespace, basePriceMeasure);
					writer.WriteElementString("g", "unit_pricing_base_measure", _googleNamespace, basePriceBaseMeasure);
				}
			}

			return null;
		}
		
		public virtual string[] GetTaxonomyList()
		{
			try
			{
				string fileDir = Path.Combine(Helper.Plugin.OriginalAssemblyFile.Directory.FullName, "Files");
				string fileName = "taxonomy.{0}.txt".FormatWith(Helper.Language.LanguageCulture ?? "de-DE");
				string path = Path.Combine(fileDir, fileName);

				if (!File.Exists(path))
					path = Path.Combine(fileDir, "taxonomy.en-US.txt");

				string[] lines = File.ReadAllLines(path, Encoding.UTF8);

				return lines;
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
			return new string[] { };
		}
		public virtual void UpdateInsert(int pk, string name, string value)
		{
			if (pk == 0 || name.IsNullOrEmpty())
				return;

			var product = GetByProductId(pk);
			bool insert = (product == null);

			if (insert)
			{
				product = new GoogleProductRecord()
				{
					ProductId = pk
				};
			}

			switch(name)
			{
				case "GoogleCategory":
					product.Taxonomy = value;
					break;
				case "Gender":
					product.Gender = value;
					break;
				case "AgeGroup":
					product.AgeGroup = value;
					break;
				case "Color":
					product.Color = value;
					break;
				case "GoogleSize":
					product.Size = value;
					break;
			}

			if (insert)
			{
				InsertGoogleProductRecord(product);
			}
			else
			{
				UpdateGoogleProductRecord(product);
			}
		}
		public virtual GridModel<GoogleProductModel> GetGridModel(GridCommand command, string searchProductName = null)
		{
			var searchContext = new ProductSearchContext()
			{
				Keywords = searchProductName,
				PageIndex = command.Page - 1,
				PageSize = command.PageSize,
				ShowHidden = true
			};

			var products = _productService.SearchProducts(searchContext);

			var data = products.Select(x =>
			{
				var gModel = new GoogleProductModel()
				{
					ProductId = x.Id,
					ProductName = x.Name
				};

				var googleProduct = GetByProductId(x.Id);

				if (googleProduct != null)
				{
					gModel.GoogleCategory = googleProduct.Taxonomy;
					gModel.Gender = googleProduct.Gender;
					gModel.AgeGroup = googleProduct.AgeGroup;
					gModel.Color = googleProduct.Color;
					gModel.GoogleSize = googleProduct.Size;

					if (gModel.Gender.HasValue())
						gModel.GenderLocalize = Helper.Resource("Gender" + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(gModel.Gender));

					if (gModel.AgeGroup.HasValue())
						gModel.AgeGroupLocalize = Helper.Resource("AgeGroup" + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(gModel.AgeGroup));
				}

				return gModel;
			})
			.ToList();

			var model = new GridModel<GoogleProductModel>()
			{
				Data = data,
				Total = products.TotalCount
			};

			return model;
		}
		public virtual void CreateFeed(Stream stream, Store store)
		{
			string breakingError = null;
			var xmlSettings = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8
			};

			using (var writer = XmlWriter.Create(stream, xmlSettings))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("rss");
				writer.WriteAttributeString("version", "2.0");
				writer.WriteAttributeString("xmlns", "g", null, _googleNamespace);
				writer.WriteStartElement("channel");
				writer.WriteElementString("title", "{0} - Feed for Google Merchant Center".FormatWith(store.Name));
				writer.WriteElementString("link", "http://base.google.com/base/");
				writer.WriteElementString("description", "Information about products");

				var currency = Helper.GetUsedCurrency(Settings.CurrencyId);
				var searchContext = new ProductSearchContext()
				{
					OrderBy = ProductSortingEnum.CreatedOn,
					PageSize = int.MaxValue,
					StoreId = store.Id,
					VisibleIndividuallyOnly = true
				};

				var products = _productService.SearchProducts(searchContext);

				foreach (var product in products)
				{
					var qualifiedProducts = Helper.QualifiedProductsByProduct(_productService, product, store);

					foreach (var qualifiedProduct in qualifiedProducts)
					{
						writer.WriteStartElement("item");

						try
						{
							breakingError = WriteItem(writer, store, qualifiedProduct, currency);
						}
						catch (Exception exc)
						{
							exc.Dump();
						}

						writer.WriteEndElement(); // item
					}

					if (breakingError.HasValue())
						break;
				}

				writer.WriteEndElement(); // channel
				writer.WriteEndElement(); // rss
				writer.WriteEndDocument();
			}

			if (breakingError.HasValue())
				throw new SmartException(breakingError);
		}
		public virtual void CreateFeed()
		{
			Helper.StartCreatingFeeds(_storeService, (stream, store) =>
			{
				CreateFeed(stream, store);
				return true;
			});
		}
		public virtual void SetupModel(FeedFroogleModel model, ScheduleTask task = null)
		{
			var stores = _storeService.GetAllStores().ToList();

			model.AvailableCurrencies = Helper.AvailableCurrencies();
			model.AvailableGoogleCategories = GetTaxonomyList();
			model.GeneratedFiles = Helper.FeedFiles(stores);
			model.Helper = Helper;

			model.AvailableStores = new List<SelectListItem>();
			model.AvailableStores.Add(new SelectListItem() { Text = Helper.Resource("Admin.Common.All"), Value = "0" });
			model.AvailableStores.AddRange(_storeService.GetAllStores().ToSelectListItems());

			if (task != null)
			{
				model.GenerateStaticFileEachMinutes = task.Seconds / 60;
				model.TaskEnabled = task.Enabled;
			}
		}
    }
}
