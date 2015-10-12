using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.GoogleMerchantCenter.Models;
using SmartStore.GoogleMerchantCenter.Services;
using SmartStore.Services.DataExchange;
using SmartStore.Services.Directory;

namespace SmartStore.GoogleMerchantCenter.Providers
{
	[SystemName("Feeds.GoogleMerchantCenterProductXml")]
	[FriendlyName("Google Merchant Center XML product feed")]
	[DisplayOrder(1)]
	[ExportSupporting(
		ExportSupport.CreateInitialPublicDeployment,
		ExportSupport.ProjectionNoGroupedProducts,
		ExportSupport.ProjectionAttributeCombinationAsProduct,
		ExportSupport.ProjectionDescription,
		ExportSupport.ProjectionUseOwnProductNo,
		ExportSupport.ProjectionBrand,
		ExportSupport.ProjectionMainPictureUrl,
		ExportSupport.ProjectionSpecialPrice)]
	public class ProductExportXmlProvider : IExportProvider
	{
		private const string _googleNamespace = "http://base.google.com/ns/1.0";

		private readonly IGoogleFeedService _googleFeedService;
		private readonly IMeasureService _measureService;
		private readonly MeasureSettings _measureSettings;

		public ProductExportXmlProvider(
			IGoogleFeedService googleFeedService,
			IMeasureService measureService,
			MeasureSettings measureSettings)
		{
			_googleFeedService = googleFeedService;
			_measureService = measureService;
			_measureSettings = measureSettings;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

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

		public static string SystemName
		{
			get { return "Feeds.GoogleMerchantCenterProductXml"; }
		}

		public static string Unspecified
		{
			get { return "__nospec__"; }
		}

		public ExportConfigurationInfo ConfigurationInfo
		{
			get
			{
				return new ExportConfigurationInfo
				{
					PartialViewName = "~/Plugins/SmartStore.GoogleMerchantCenter/Views/FeedGoogleMerchantCenter/ProfileConfiguration.cshtml",
					ModelType = typeof(ProfileConfigurationModel),
					Initialize = obj =>
					{
						var model = (obj as ProfileConfigurationModel);

						model.AvailableGoogleCategories = _googleFeedService.GetTaxonomyList();
					}
				};
			}
		}

		public ExportEntityType EntityType
		{
			get { return ExportEntityType.Product; }
		}

		public string FileExtension
		{
			get { return "XML"; }
		}

		public void Execute(IExportExecuteContext context)
		{
			var settings = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8,
				CheckCharacters = false
			};

			var path = context.FilePath;
			dynamic currency = context.Currency;
			string measureWeightSystemKey = "";

			var measureWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);

			if (measureWeight != null)
				measureWeightSystemKey = measureWeight.SystemKeyword;

			var config = (context.ConfigurationData as ProfileConfigurationModel) ?? new ProfileConfigurationModel();

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var writer = XmlWriter.Create(stream, settings))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("rss");
				writer.WriteAttributeString("version", "2.0");
				writer.WriteAttributeString("xmlns", "g", null, _googleNamespace);
				writer.WriteStartElement("channel");
				writer.WriteElementString("title", "{0} - Feed for Google Merchant Center".FormatInvariant((string)context.Store.Name));
				writer.WriteElementString("link", "http://base.google.com/base/");
				writer.WriteElementString("description", "Information about products");

				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					int[] productIds = segment.Select(x => (int)((dynamic)x).Id).ToArray();
					var googleProducts = _googleFeedService.GetGoogleProductRecords(productIds);

					foreach (dynamic product in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						int productId = product.Id;
						var gmc = googleProducts.FirstOrDefault(x => x.ProductId == productId);

						if (gmc != null && !gmc.Export)
							continue;

						writer.WriteStartElement("item");

						try
						{
							string category = (gmc == null ? null : gmc.Taxonomy);
							string productType = product._CategoryPath;
							string mainImageUrl = product._MainPictureUrl;
							var specialPrice = product._SpecialPrice as decimal?;
							var price = (decimal)product.Price;
							string brand = product._Brand;
							string gtin = product.Gtin;
							string mpn = product.ManufacturerPartNumber;
							string condition = "new";
							string availability = "in stock";

							if (category.IsEmpty())
								category = config.DefaultGoogleCategory;

							if (category.IsEmpty())
								context.Log.Error(T("Plugins.Feed.Froogle.MissingDefaultCategory"));

							if (config.Condition.IsCaseInsensitiveEqual(Unspecified))
							{
								condition = "";
							}
							else if (config.Condition.HasValue())
							{
								condition = config.Condition;
							}

							if (config.Availability.IsCaseInsensitiveEqual(Unspecified))
							{
								availability = "";
							}
							else if (config.Availability.HasValue())
							{
								availability = config.Availability;
							}
							else
							{
								if ((int)product.ManageInventoryMethodId == (int)ManageInventoryMethod.ManageStock && (int)product.StockQuantity <= 0)
								{
									var backorderId = (int)product.BackorderModeId;
									if (backorderId == (int)BackorderMode.NoBackorders)
										availability = "out of stock";
									else if (backorderId == (int)BackorderMode.AllowQtyBelow0 || backorderId == (int)BackorderMode.AllowQtyBelow0AndNotifyCustomer)
										availability = ((bool)product.AvailableForPreOrder ? "preorder" : "out of stock");
								}
							}

							writer.WriteElementString("g", "id", _googleNamespace, productId.ToString());

							writer.WriteStartElement("title");
							writer.WriteCData(((string)product.Name).Truncate(70).RemoveInvalidXmlChars());
							writer.WriteEndElement();

							writer.WriteStartElement("description");
							writer.WriteCData(((string)product.FullDescription).RemoveInvalidXmlChars());
							writer.WriteEndElement();

							writer.WriteStartElement("g", "google_product_category", _googleNamespace);
							writer.WriteCData(category.RemoveInvalidXmlChars());
							writer.WriteFullEndElement();

							if (productType.HasValue())
							{
								writer.WriteStartElement("g", "product_type", _googleNamespace);
								writer.WriteCData(productType.RemoveInvalidXmlChars());
								writer.WriteFullEndElement();
							}

							writer.WriteElementString("link", (string)product._DetailUrl);

							if (mainImageUrl.HasValue())
							{
								writer.WriteElementString("g", "image_link", _googleNamespace, mainImageUrl);
							}

							if (config.AdditionalImages)
							{
								var imageCount = 0;
								foreach (dynamic productPicture in product.ProductPictures)
								{
									string pictureUrl = productPicture.Picture._ImageUrl;
									if (pictureUrl.HasValue() && (mainImageUrl.IsEmpty() || !mainImageUrl.IsCaseInsensitiveEqual(pictureUrl)) && ++imageCount <= 10)
									{
										writer.WriteElementString("g", "additional_image_link", _googleNamespace, pictureUrl);
									}
								}
							}

							writer.WriteElementString("g", "condition", _googleNamespace, condition);
							writer.WriteElementString("g", "availability", _googleNamespace, availability);

							if (config.SpecialPrice && specialPrice.HasValue)
							{
								DateTime? specialPriceStart = product.SpecialPriceStartDateTimeUtc;
								DateTime? specialPriceEnd = product.SpecialPriceEndDateTimeUtc;
								if (specialPriceStart.HasValue && specialPriceEnd.HasValue)
								{
									var dateFormat = "yyyy-MM-ddTHH:mmZ";
									var specialPriceDate = "{0}/{1}".FormatInvariant(specialPriceStart.Value.ToString(dateFormat), specialPriceEnd.Value.ToString(dateFormat));

									writer.WriteElementString("g", "sale_price", _googleNamespace, price.FormatInvariant() + " " + (string)currency.CurrencyCode);
									writer.WriteElementString("g", "sale_price_effective_date", _googleNamespace, specialPriceDate);

									price = (product._RegularPrice as decimal?) ?? price;
								}
							}

							writer.WriteElementString("g", "price", _googleNamespace, price.FormatInvariant() + " " + (string)currency.CurrencyCode);

							writer.WriteCData("gtin", gtin, "g", _googleNamespace);
							writer.WriteCData("brand", brand, "g", _googleNamespace);
							writer.WriteCData("mpn", mpn, "g", _googleNamespace);

							if (config.Gender.IsCaseInsensitiveEqual(Unspecified))
								writer.WriteCData("gender", "", "g", _googleNamespace);
							else
								writer.WriteCData("gender", gmc != null && gmc.Gender.HasValue() ? gmc.Gender : config.Gender, "g", _googleNamespace);

							if (config.AgeGroup.IsCaseInsensitiveEqual(Unspecified))
								writer.WriteCData("age_group", "", "g", _googleNamespace);
							else
								writer.WriteCData("age_group", gmc != null && gmc.AgeGroup.HasValue() ? gmc.AgeGroup : config.AgeGroup, "g", _googleNamespace);

							writer.WriteCData("color", gmc != null && gmc.Color.HasValue() ? gmc.Color : config.Color, "g", _googleNamespace);
							writer.WriteCData("size", gmc != null && gmc.Size.HasValue() ? gmc.Size : config.Size, "g", _googleNamespace);
							writer.WriteCData("material", gmc != null && gmc.Material.HasValue() ? gmc.Material : config.Material, "g", _googleNamespace);
							writer.WriteCData("pattern", gmc != null && gmc.Pattern.HasValue() ? gmc.Pattern : config.Pattern, "g", _googleNamespace);
							writer.WriteCData("item_group_id", gmc != null && gmc.ItemGroupId.HasValue() ? gmc.ItemGroupId : "", "g", _googleNamespace);

							writer.WriteElementString("g", "online_only", _googleNamespace, config.OnlineOnly ? "y" : "n");
							writer.WriteElementString("g", "identifier_exists", _googleNamespace, gtin.HasValue() || brand.HasValue() || mpn.HasValue() ? "TRUE" : "FALSE");

							if (config.ExpirationDays > 0)
							{
								writer.WriteElementString("g", "expiration_date", _googleNamespace, DateTime.UtcNow.AddDays(config.ExpirationDays).ToString("yyyy-MM-dd"));
							}

							if (config.ExportShipping)
							{
								string weightInfo;
								var weight = ((decimal)product.Weight).FormatInvariant();

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

							if (config.ExportBasePrice && (bool)product.BasePriceHasValue)
							{
								var measureUnit = BasePriceUnits((string)product.BasePriceMeasureUnit);
								var basePriceBaseAmount = product.BasePriceBaseAmount as int?;

								if (BasePriceSupported(basePriceBaseAmount ?? 0, measureUnit))
								{
									var basePriceAmount = product.BasePriceAmount as decimal?;
									var basePriceMeasure = "{0} {1}".FormatInvariant((basePriceAmount ?? decimal.Zero).FormatInvariant(), measureUnit);
									var basePriceBaseMeasure = "{0} {1}".FormatInvariant(basePriceBaseAmount ?? 1, measureUnit);

									writer.WriteElementString("g", "unit_pricing_measure", _googleNamespace, basePriceMeasure);
									writer.WriteElementString("g", "unit_pricing_base_measure", _googleNamespace, basePriceBaseMeasure);
								}
							}

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, productId);
						}

						writer.WriteEndElement(); // item
					}
				}

				writer.WriteEndElement(); // channel
				writer.WriteEndElement(); // rss
				writer.WriteEndDocument();
			}
		}

		public void ExecuteEnded(IExportExecuteContext context)
		{
			// nothing to do
		}
	}
}