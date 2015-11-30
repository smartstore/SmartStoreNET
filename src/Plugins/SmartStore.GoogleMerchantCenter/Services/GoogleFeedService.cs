using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Autofac;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Logging;
using SmartStore.GoogleMerchantCenter.Domain;
using SmartStore.GoogleMerchantCenter.Models;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework.Plugins;
using Telerik.Web.Mvc;

namespace SmartStore.GoogleMerchantCenter.Services
{
    public partial class GoogleFeedService : IGoogleFeedService
    {
		private const string _googleNamespace = "http://base.google.com/ns/1.0";

		private readonly FeedPluginHelper _helper;
        private readonly IRepository<GoogleProductRecord> _gpRepository;
		private readonly IProductService _productService;
		private readonly IManufacturerService _manufacturerService;
		private readonly IMeasureService _measureService;
		private readonly MeasureSettings _measureSettings;
		private readonly IDbContext _dbContext;
		private readonly AdminAreaSettings _adminAreaSettings;
		private readonly ICurrencyService _currencyService;
		private readonly ICommonServices _services;

		public GoogleFeedService(
			IRepository<GoogleProductRecord> gpRepository,
			IProductService productService,
			IManufacturerService manufacturerService,
			FroogleSettings settings,
			IMeasureService measureService,
			MeasureSettings measureSettings,
			IDbContext dbContext,
			AdminAreaSettings adminAreaSettings,
			ICurrencyService currencyService,
			ICommonServices services,
			IComponentContext ctx)
        {
            _gpRepository = gpRepository;
			_productService = productService;
			_manufacturerService = manufacturerService;
			Settings = settings;
			_measureService = measureService;
			_measureSettings = measureSettings;
			_dbContext = dbContext;
			_adminAreaSettings = adminAreaSettings;
			_currencyService = currencyService;
			_services = services;

			_helper = new FeedPluginHelper(ctx, "SmartStore.GoogleMerchantCenter", "SmartStore.GoogleMerchantCenter", () =>
			{
				return Settings as PromotionFeedSettings;
			});
        }

		public FroogleSettings Settings { get; set; }
		public FeedPluginHelper Helper { get { return _helper; } }

        public GoogleProductRecord GetGoogleProductRecord(int productId)
        {
            if (productId == 0)
                return null;

            var query = 
				from gp in _gpRepository.Table
				where gp.ProductId == productId
				select gp;

            var record = query.FirstOrDefault();
            return record;
        }

        public void InsertGoogleProductRecord(GoogleProductRecord record)
        {
            if (record == null)
                throw new ArgumentNullException("googleProductRecord");

            _gpRepository.Insert(record);
        }

		public void UpdateGoogleProductRecord(GoogleProductRecord record)
        {
			if (record == null)
				throw new ArgumentNullException("record");

			_gpRepository.Update(record);
        }

		public void DeleteGoogleProductRecord(GoogleProductRecord record)
		{
			if (record == null)
				throw new ArgumentNullException("record");

			_gpRepository.Delete(record);
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

			if (productCategory.IsEmpty())
				productCategory = Settings.DefaultGoogleCategory;

			return productCategory;
		}
		private string Condition()
		{
			if (Settings.Condition.IsCaseInsensitiveEqual(PluginHelper.NotSpecified))
				return "";

			if (Settings.Condition.IsEmpty())
				return "new";

			return Settings.Condition;
		}
		private string Availability(Product product)
		{
			if (Settings.Availability.IsCaseInsensitiveEqual(PluginHelper.NotSpecified))
				return "";

			if (Settings.Availability.IsEmpty())
			{
				if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock && product.StockQuantity <= 0)
				{
					switch (product.BackorderMode)
					{
						case BackorderMode.NoBackorders:
							return "out of stock";
						case BackorderMode.AllowQtyBelow0:
						case BackorderMode.AllowQtyBelow0AndNotifyCustomer:
							if (product.AvailableForPreOrder)
								return "preorder";
							return "out of stock";
					}
				}
				return "in stock";
			}
			return Settings.Availability;
		}
		private string Gender(GoogleProductRecord googleProduct)
		{
			if (Settings.Gender.IsCaseInsensitiveEqual(PluginHelper.NotSpecified))
				return "";

			if (googleProduct != null && googleProduct.Gender.HasValue())
				return googleProduct.Gender;

			return Settings.Gender;
		}
		private string AgeGroup(GoogleProductRecord googleProduct)
		{
			if (Settings.AgeGroup.IsCaseInsensitiveEqual(PluginHelper.NotSpecified))
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

			if (value.IsEmpty())
				return defaultValue;

			// TODO: Product.BasePriceMeasureUnit should be localized
			switch (value.ToLowerInvariant())
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
		private void WriteItem(FeedFileCreationContext fileCreation, XmlWriter writer, Product product, Currency currency, string measureWeightSystemKey)
		{
			GoogleProductRecord googleProduct = null;

			try
			{
				googleProduct = GetGoogleProductRecord(product.Id);

				if (googleProduct != null && !googleProduct.Export)
					return;
			}
			catch (Exception exc)
			{
				fileCreation.Logger.Error(exc.Message, exc);
			}

			writer.WriteStartElement("item");

			try
			{
				var manu = _manufacturerService.GetProductManufacturersByProductId(product.Id).FirstOrDefault();
				var mainImageUrl = Helper.GetMainProductImageUrl(fileCreation.Store, product);
				var category = ProductCategory(googleProduct);

				if (category.IsEmpty())
					fileCreation.ErrorMessage = Helper.GetResource("MissingDefaultCategory");

				string manuName = (manu != null ? manu.Manufacturer.GetLocalized(x => x.Name, Settings.LanguageId, true, false) : null);
				string productName = product.GetLocalized(x => x.Name, Settings.LanguageId, true, false);
				string shortDescription = product.GetLocalized(x => x.ShortDescription, Settings.LanguageId, true, false);
				string fullDescription = product.GetLocalized(x => x.FullDescription, Settings.LanguageId, true, false);

				var brand = (manuName ?? Settings.Brand);
				var mpn = Helper.GetManufacturerPartNumber(product);

				bool identifierExists = product.Gtin.HasValue() || brand.HasValue() || mpn.HasValue();

				writer.WriteElementString("g", "id", _googleNamespace, product.Id.ToString());

				writer.WriteStartElement("title");
				writer.WriteCData(productName.Truncate(70));
				writer.WriteEndElement();

				var description = Helper.BuildProductDescription(productName, shortDescription, fullDescription, manuName, d =>
				{
					if (fullDescription.IsEmpty() && shortDescription.IsEmpty())
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
				writer.WriteCData(description.RemoveInvalidXmlChars());
				writer.WriteEndElement();

				writer.WriteStartElement("g", "google_product_category", _googleNamespace);
				writer.WriteCData(category);
				writer.WriteFullEndElement();

				string productType = Helper.GetCategoryPath(product);
				if (productType.HasValue())
				{
					writer.WriteStartElement("g", "product_type", _googleNamespace);
					writer.WriteCData(productType);
					writer.WriteFullEndElement();
				}

				writer.WriteElementString("link", Helper.GetProductDetailUrl(fileCreation.Store, product));
				writer.WriteElementString("g", "image_link", _googleNamespace, mainImageUrl);

				foreach (string additionalImageUrl in Helper.GetAdditionalProductImages(fileCreation.Store, product, mainImageUrl))
				{
					writer.WriteElementString("g", "additional_image_link", _googleNamespace, additionalImageUrl);
				}

				writer.WriteElementString("g", "condition", _googleNamespace, Condition());
				writer.WriteElementString("g", "availability", _googleNamespace, Availability(product));

				decimal price = Helper.GetProductPrice(product, currency, fileCreation.Store);
				string specialPriceDate;

				if (SpecialPrice(product, out specialPriceDate))
				{
					writer.WriteElementString("g", "sale_price", _googleNamespace, price.FormatInvariant() + " " + currency.CurrencyCode);
					writer.WriteElementString("g", "sale_price_effective_date", _googleNamespace, specialPriceDate);

					// get regular price ignoring any special price
					decimal specialPrice = product.SpecialPrice.Value;
					product.SpecialPrice = null;
					price = Helper.GetProductPrice(product, currency, fileCreation.Store);
					product.SpecialPrice = specialPrice;

					_dbContext.SetToUnchanged<Product>(product);
				}

				writer.WriteElementString("g", "price", _googleNamespace, price.FormatInvariant() + " " + currency.CurrencyCode);

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
					string weightInfo, weight = product.Weight.FormatInvariant();

					if (measureWeightSystemKey.IsCaseInsensitiveEqual("gram"))
						weightInfo = weight + " g";
					else if (measureWeightSystemKey.IsCaseInsensitiveEqual("lb"))
						weightInfo = weight + " lb";
					else if (measureWeightSystemKey.IsCaseInsensitiveEqual("ounce"))
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
						string basePriceMeasure = "{0} {1}".FormatWith((product.BasePriceAmount ?? decimal.Zero).FormatInvariant(), measureUnit);
						string basePriceBaseMeasure = "{0} {1}".FormatWith(product.BasePriceBaseAmount, measureUnit);

						writer.WriteElementString("g", "unit_pricing_measure", _googleNamespace, basePriceMeasure);
						writer.WriteElementString("g", "unit_pricing_base_measure", _googleNamespace, basePriceBaseMeasure);
					}
				}
			}
			catch (Exception exc)
			{
				fileCreation.Logger.Error(exc.Message, exc);
			}

			writer.WriteEndElement(); // item
		}
		
		public string[] GetTaxonomyList()
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

		public void UpdateInsert(int pk, string name, string value)
		{
			if (pk == 0 || name.IsEmpty())
				return;

			var product = GetGoogleProductRecord(pk);
			bool insert = (product == null);
			var utcNow = DateTime.UtcNow;

			if (product == null)
			{
				product = new GoogleProductRecord
				{
					ProductId = pk,
					CreatedOnUtc = utcNow
				};
			}

			switch (name)
			{
				case "Taxonomy":
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
				case "Size":
					product.Size = value;
					break;
				case "Material":
					product.Material = value;
					break;
				case "Pattern":
					product.Pattern = value;
					break;
				case "Exporting":
					product.Export = value.ToBool(true);
					break;
			}

			product.UpdatedOnUtc = utcNow;
			product.IsTouched = product.IsTouched();

			if (!insert && !product.IsTouched)
			{
				_gpRepository.Delete(product);
				return;
			}

			if (insert)
			{
				_gpRepository.Insert(product);
			}
			else
			{
				_gpRepository.Update(product);
			}
		}
		public GridModel<GoogleProductModel> GetGridModel(GridCommand command, string searchProductName = null, string touched = null)
		{
			var model = new GridModel<GoogleProductModel>();
			var textInfo = CultureInfo.InvariantCulture.TextInfo;

			// there's no way to share a context instance across repositories which makes GoogleProductObjectContext pretty useless here.

			var whereClause = new StringBuilder("(NOT ([t2].[Deleted] = 1)) AND ([t2].[VisibleIndividually] = 1)");

			if (searchProductName.HasValue())
			{
				whereClause.AppendFormat(" AND ([t2].[Name] LIKE '%{0}%')", searchProductName.Replace("'", "''"));
			}

			if (touched.HasValue())
			{
				if (touched.IsCaseInsensitiveEqual("touched"))
					whereClause.Append(" AND ([t2].[IsTouched] = 1)");
				else
					whereClause.Append(" AND ([t2].[IsTouched] = 0 OR [t2].[IsTouched] IS NULL)");
			}

			string sql = null;
			string sqlCount = null;
			var isSqlServer = DataSettings.Current.IsSqlServer;

			if (isSqlServer)
			{
				// fastest possible paged data query
				sql =
					"SELECT [TotalCount], [t3].[Id], [t3].[Name], [t3].[SKU], [t3].[ProductTypeId], [t3].[value] AS [Taxonomy], [t3].[value2] AS [Gender], [t3].[value3] AS [AgeGroup], [t3].[value4] AS [Color], [t3].[value5] AS [Size], [t3].[value6] AS [Material], [t3].[value7] AS [Pattern], [t3].[value8] AS [Export]" +
					" FROM (" +
					"    SELECT COUNT(id) OVER() [TotalCount], ROW_NUMBER() OVER (ORDER BY [t2].[Name]) AS [ROW_NUMBER], [t2].[Id], [t2].[Name], [t2].[SKU], [t2].[ProductTypeId], [t2].[value], [t2].[value2], [t2].[value3], [t2].[value4], [t2].[value5], [t2].[value6], [t2].[value7], [t2].[value8]" +
					"    FROM (" +
					"        SELECT [t0].[Id], [t0].[Name], [t0].[SKU], [t0].[ProductTypeId], [t1].[Taxonomy] AS [value], [t1].[Gender] AS [value2], [t1].[AgeGroup] AS [value3], [t1].[Color] AS [value4], [t1].[Size] AS [value5], [t1].[Material] AS [value6], [t1].[Pattern] AS [value7], COALESCE([t1].[Export],1) AS [value8], [t0].[Deleted], [t0].[VisibleIndividually], [t1].[IsTouched]" +
					"        FROM [Product] AS [t0]" +
					"        LEFT OUTER JOIN [GoogleProduct] AS [t1] ON [t0].[Id] = [t1].[ProductId]" +
					"        ) AS [t2]" +
					"    WHERE " + whereClause.ToString() +
					"    ) AS [t3]" +
					" WHERE [t3].[ROW_NUMBER] BETWEEN {0} + 1 AND {0} + {1}" +
					" ORDER BY [t3].[ROW_NUMBER]";
			}
			else
			{
				// OFFSET... FETCH NEXT requires SQL Server 2012 or SQL CE 4
				sql =
					"SELECT [t2].[Id], [t2].[Name], [t2].[SKU], [t2].[ProductTypeId], [t2].[value] AS [Taxonomy], [t2].[value2] AS [Gender], [t2].[value3] AS [AgeGroup], [t2].[value4] AS [Color], [t2].[value5] AS [Size], [t2].[value6] AS [Material], [t2].[value7] AS [Pattern], [t2].[value8] AS [Export]" +
					" FROM (" +
					"     SELECT [t0].[Id], [t0].[Name], [t0].[SKU], [t0].[ProductTypeId], [t1].[Taxonomy] AS [value], [t1].[Gender] AS [value2], [t1].[AgeGroup] AS [value3], [t1].[Color] AS [value4], [t1].[Size] AS [value5], [t1].[Material] AS [value6], [t1].[Pattern] AS [value7], COALESCE([t1].[Export],1) AS [value8], [t0].[Deleted], [t0].[VisibleIndividually], [t1].[IsTouched] AS [IsTouched]" +
					"     FROM [Product] AS [t0]" +
					"     LEFT OUTER JOIN [GoogleProduct] AS [t1] ON [t0].[Id] = [t1].[ProductId]" +
					" ) AS [t2]" +
					" WHERE " + whereClause.ToString() +
					" ORDER BY [t2].[Name]" +
					" OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY";

				sqlCount =
					"SELECT COUNT(*)" +
					" FROM (" +
					"     SELECT [t0].[Id], [t0].[Name], [t0].[Deleted], [t0].[VisibleIndividually], [t1].[IsTouched] AS [IsTouched]" +
					"     FROM [Product] AS [t0]" +
					"     LEFT OUTER JOIN [GoogleProduct] AS [t1] ON [t0].[Id] = [t1].[ProductId]" +
					" ) AS [t2]" +
					" WHERE " + whereClause.ToString();
			}


			var data = _gpRepository.Context.SqlQuery<GoogleProductModel>(sql, (command.Page - 1) * command.PageSize, command.PageSize).ToList();

			data.ForEach(x =>
			{
				if (x.ProductType != ProductType.SimpleProduct)
				{
					string key = "Admin.Catalog.Products.ProductType.{0}.Label".FormatWith(x.ProductType.ToString());
					x.ProductTypeName = Helper.GetResource(key);
				}

				var googleProduct = GetGoogleProductRecord(x.Id);
				if (x.Gender.HasValue())
					x.GenderLocalize = Helper.GetResource("Gender" + textInfo.ToTitleCase(x.Gender));

				if (x.AgeGroup.HasValue())
					x.AgeGroupLocalize = Helper.GetResource("AgeGroup" + textInfo.ToTitleCase(x.AgeGroup));

				x.ExportingLocalize = Helper.GetResource(x.Export == 0 ? "Admin.Common.No" : "Admin.Common.Yes");
			});

			model.Data = data;
			model.Total = (data.Count > 0 ? data.First().TotalCount : 0);

			if (data.Count > 0)
			{
				if (isSqlServer)
					model.Total = data.First().TotalCount;
				else
					model.Total = _gpRepository.Context.SqlQuery<int>(sqlCount).FirstOrDefault();
			}
			else
			{
				model.Total = 0;
			}

			return model;

			#region old code

			//var searchContext = new ProductSearchContext()
			//{
			//	Keywords = searchProductName,
			//	PageIndex = command.Page - 1,
			//	PageSize = command.PageSize,
			//	VisibleIndividuallyOnly = true,
			//	ShowHidden = true
			//};

			//var products = _productService.SearchProducts(searchContext);

			//var data = products.Select(x =>
			//{
			//	var gModel = new GoogleProductModel()
			//	{
			//		ProductId = x.Id,
			//		Name = x.Name
			//	};

			//	var googleProduct = GetByProductId(x.Id);

			//	if (googleProduct != null)
			//	{
			//		gModel.Taxonomy = googleProduct.Taxonomy;
			//		gModel.Gender = googleProduct.Gender;
			//		gModel.AgeGroup = googleProduct.AgeGroup;
			//		gModel.Color = googleProduct.Color;
			//		gModel.Size = googleProduct.Size;
			//		gModel.Material = googleProduct.Material;
			//		gModel.Pattern = googleProduct.Pattern;

			//		if (gModel.Gender.HasValue())
			//			gModel.GenderLocalize = Helper.GetResource("Gender" + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(gModel.Gender));

			//		if (gModel.AgeGroup.HasValue())
			//			gModel.AgeGroupLocalize = Helper.GetResource("AgeGroup" + CultureInfo.InvariantCulture.TextInfo.ToTitleCase(gModel.AgeGroup));
			//	}

			//	return gModel;
			//})
			//.ToList();

			//var model = new GridModel<GoogleProductModel>()
			//{
			//	Data = data,
			//	Total = products.TotalCount
			//};

			//return model;

			#endregion old code
		}

		private void CreateFeed(FeedFileCreationContext fileCreation, TaskExecutionContext taskContext)
		{
			var xmlSettings = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8,
				CheckCharacters = false
			};
			
			using (var writer = XmlWriter.Create(fileCreation.Stream, xmlSettings))
			{
				try
				{
					fileCreation.Logger.Information("Log file - Google Merchant Center feed.");

					var searchContext = new ProductSearchContext
					{
						OrderBy = ProductSortingEnum.CreatedOn,
						PageSize = Settings.PageSize,
						StoreId = fileCreation.Store.Id,
						VisibleIndividuallyOnly = true
					};

					var currency = _currencyService.GetCurrencyById(Settings.CurrencyId);
					var measureWeightSystemKey = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).SystemKeyword;

					if (currency == null || !currency.Published)
						currency = _services.WorkContext.WorkingCurrency;

					writer.WriteStartDocument();
					writer.WriteStartElement("rss");
					writer.WriteAttributeString("version", "2.0");
					writer.WriteAttributeString("xmlns", "g", null, _googleNamespace);
					writer.WriteStartElement("channel");
					writer.WriteElementString("title", "{0} - Feed for Google Merchant Center".FormatWith(fileCreation.Store.Name));
					writer.WriteElementString("link", "http://base.google.com/base/");
					writer.WriteElementString("description", "Information about products");

					for (int i = 0; i < 9999999; ++i)
					{
						searchContext.PageIndex = i;
						
						// Perf
						_dbContext.DetachAll();

						var products = _productService.SearchProducts(searchContext);

						if (fileCreation.TotalRecords == 0)
							fileCreation.TotalRecords = products.TotalCount * fileCreation.StoreCount;	// approx

						foreach (var product in products)
						{
							fileCreation.Report();

							if (product.ProductType == ProductType.SimpleProduct || product.ProductType == ProductType.BundledProduct)
							{
								WriteItem(fileCreation, writer, product, currency, measureWeightSystemKey);
							}
							else if (product.ProductType == ProductType.GroupedProduct)
							{
								var associatedSearchContext = new ProductSearchContext
								{
									OrderBy = ProductSortingEnum.CreatedOn,
									PageSize = int.MaxValue,
									StoreId = fileCreation.Store.Id,
									VisibleIndividuallyOnly = false,
									ParentGroupedProductId = product.Id
								};

								foreach (var associatedProduct in _productService.SearchProducts(associatedSearchContext))
								{
									WriteItem(fileCreation, writer, associatedProduct, currency, measureWeightSystemKey);
								}
							}

							if (taskContext.CancellationToken.IsCancellationRequested)
							{
								fileCreation.Logger.Warning("A cancellation has been requested");
								break;
							}
						}

						if (!products.HasNextPage || taskContext.CancellationToken.IsCancellationRequested)
							break;
					}

					writer.WriteEndElement(); // channel
					writer.WriteEndElement(); // rss
					writer.WriteEndDocument();

					if (fileCreation.ErrorMessage.HasValue())
						fileCreation.Logger.Error(fileCreation.ErrorMessage);
				}
				catch (Exception exc)
				{
					fileCreation.Logger.Error(exc.Message, exc);
				}
			}
		}

		public void CreateFeed(TaskExecutionContext context)
		{
			Helper.StartCreatingFeeds(fileCreation =>
			{
				CreateFeed(fileCreation, context);
				return true;
			});
		}

		public void SetupModel(FeedFroogleModel model)
		{
			Helper.SetupConfigModel(model, "FeedFroogle");

			//model.GenerateStaticFileEachMinutes = Helper.ScheduleTask.Seconds / 60;
			model.TaskEnabled = Helper.ScheduleTask.Enabled;
			model.ScheduleTaskId = Helper.ScheduleTask.Id;

			model.AvailableCurrencies = Helper.AvailableCurrencies();
			model.AvailableGoogleCategories = GetTaxonomyList();

			var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
			model.GridEditUrl = urlHelper.Action("GoogleProductEdit", "FeedFroogle",
				new { Namespaces = "SmartStore.GoogleMerchantCenter.Controllers", area = "SmartStore.GoogleMerchantCenter" });

			model.GridPageSize = _adminAreaSettings.GridPageSize;
		}
    }
}
