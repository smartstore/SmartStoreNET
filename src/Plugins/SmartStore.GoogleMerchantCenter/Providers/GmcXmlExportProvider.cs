using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.GoogleMerchantCenter.Models;
using SmartStore.GoogleMerchantCenter.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.DataExchange.Export;
using SmartStore.Services.Directory;

namespace SmartStore.GoogleMerchantCenter.Providers
{
	[SystemName("Feeds.GoogleMerchantCenterProductXml")]
	[FriendlyName("Google Merchant Center XML product feed")]
	[DisplayOrder(1)]
	[ExportFeatures(Features =
		ExportFeatures.CreatesInitialPublicDeployment |
		ExportFeatures.CanOmitGroupedProducts |
		ExportFeatures.CanProjectAttributeCombinations |
		ExportFeatures.CanProjectDescription |
		ExportFeatures.UsesSkuAsMpnFallback |
		ExportFeatures.OffersBrandFallback |
		ExportFeatures.CanIncludeMainPicture |
		ExportFeatures.UsesSpecialPrice |
		ExportFeatures.UsesAttributeCombination)]
	public class GmcXmlExportProvider : ExportProviderBase
	{
		private const string _googleNamespace = "http://base.google.com/ns/1.0";

		private readonly IGoogleFeedService _googleFeedService;
		private readonly IMeasureService _measureService;
		private readonly MeasureSettings _measureSettings;

		public GmcXmlExportProvider(
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

		private void WriteString(XmlWriter writer, string fieldName, string value)
		{
			if (value != null)
			{
				writer.WriteElementString("g", fieldName, _googleNamespace, value);
			}
		}

		private void WriteString(
			XmlWriter writer,
			Dictionary<string, string> mappedValues,
			string fieldName,
			string value)
		{
			// TODO
			if (mappedValues == null)
			{
				// regular product
				WriteString(writer, fieldName, value);
			}
			else
			{
				// export attribute combination
				if (mappedValues.ContainsKey(fieldName))
				{
					WriteString(writer, fieldName, mappedValues[fieldName].EmptyNull());
				}
				else
				{
					WriteString(writer, fieldName, value);
				}
			}
		}

		public static string SystemName
		{
			get { return "Feeds.GoogleMerchantCenterProductXml"; }
		}

		public static string Unspecified
		{
			get { return "__nospec__"; }
		}

		public override ExportConfigurationInfo ConfigurationInfo
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

		public override string FileExtension
		{
			get { return "XML"; }
		}

		protected override void Export(ExportExecuteContext context)
		{
			dynamic currency = context.Currency;
			var languageId = (int)context.Language.Id;
			var measureWeightSystemKey = "";
			var dateFormat = "yyyy-MM-ddTHH:mmZ";

			var measureWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);

			if (measureWeight != null)
				measureWeightSystemKey = measureWeight.SystemKeyword;

			var config = (context.ConfigurationData as ProfileConfigurationModel) ?? new ProfileConfigurationModel();

			using (var writer = XmlWriter.Create(context.DataStream, ExportXmlHelper.DefaultSettings))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("rss");
				writer.WriteAttributeString("version", "2.0");
				writer.WriteAttributeString("xmlns", "g", null, _googleNamespace);
				writer.WriteStartElement("channel");
				writer.WriteElementString("title", "{0} - Feed for Google Merchant Center".FormatInvariant((string)context.Store.Name));
				writer.WriteElementString("link", "http://base.google.com/base/");
				writer.WriteElementString("description", "Information about products");

				while (context.Abort == DataExchangeAbortion.None && context.DataSegmenter.ReadNextSegment())
				{
					var segment = context.DataSegmenter.CurrentSegment;

					int[] productIds = segment.Select(x => (int)((dynamic)x).Id).ToArray();
					var googleProducts = _googleFeedService.GetGoogleProductRecords(productIds).ToDictionarySafe(x => x.ProductId);

					foreach (dynamic product in segment)
					{
						if (context.Abort != DataExchangeAbortion.None)
							break;

						Product entity = product.Entity;
						var gmc = googleProducts.Get(entity.Id);

						if (gmc != null && !gmc.Export)
							continue;

						writer.WriteStartElement("item");

						try
						{
							string category = (gmc == null ? null : gmc.Taxonomy);
							string productType = product._CategoryPath;
							string mainImageUrl = product._MainPictureUrl;
							var price = (decimal)product.Price;
							var uniqueId = (string)product._UniqueId;
							string brand = product._Brand;
							string gtin = product.Gtin;
							string mpn = product.ManufacturerPartNumber;
							string condition = "new";
							string availability = "in stock";

							var combinationValues = product._AttributeCombinationValues as IList<ProductVariantAttributeValue>;
							var mappedValues = (combinationValues != null ? combinationValues.GetMappedValuesFromAlias("gmc", languageId) : null);								

							var specialPrice = product._FutureSpecialPrice as decimal?;
							if (!specialPrice.HasValue)
								specialPrice = product._SpecialPrice;

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
								if (entity.ManageInventoryMethod == ManageInventoryMethod.ManageStock && entity.StockQuantity <= 0)
								{
									if (entity.BackorderMode == BackorderMode.NoBackorders)
										availability = "out of stock";
									else if (entity.BackorderMode == BackorderMode.AllowQtyBelow0 || entity.BackorderMode == BackorderMode.AllowQtyBelow0AndNotifyCustomer)
										availability = (entity.AvailableForPreOrder ? "preorder" : "out of stock");
								}
							}

							WriteString(writer, "id", uniqueId);

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
								WriteString(writer, "image_link", mainImageUrl);
							}

							if (config.AdditionalImages)
							{
								var imageCount = 0;
								foreach (dynamic productPicture in product.ProductPictures)
								{
									string pictureUrl = productPicture.Picture._ImageUrl;
									if (pictureUrl.HasValue() && (mainImageUrl.IsEmpty() || !mainImageUrl.IsCaseInsensitiveEqual(pictureUrl)) && ++imageCount <= 10)
									{
										WriteString(writer, "additional_image_link", pictureUrl);
									}
								}
							}

							WriteString(writer, "condition", condition);
							WriteString(writer, "availability", availability);

							if (availability == "preorder" && entity.AvailableStartDateTimeUtc.HasValue && entity.AvailableStartDateTimeUtc.Value > DateTime.UtcNow)
							{
								var availabilityDate = entity.AvailableStartDateTimeUtc.Value.ToString(dateFormat);

								WriteString(writer, "availability_date", availabilityDate);
							}

							if (config.SpecialPrice && specialPrice.HasValue)
							{
								WriteString(writer, "sale_price", specialPrice.Value.FormatInvariant() + " " + (string)currency.CurrencyCode);

								if (entity.SpecialPriceStartDateTimeUtc.HasValue && entity.SpecialPriceEndDateTimeUtc.HasValue)
								{
									var specialPriceDate = "{0}/{1}".FormatInvariant(
										entity.SpecialPriceStartDateTimeUtc.Value.ToString(dateFormat), entity.SpecialPriceEndDateTimeUtc.Value.ToString(dateFormat));

									WriteString(writer, "sale_price_effective_date", specialPriceDate);
								}

								price = (product._RegularPrice as decimal?) ?? price;
							}

							WriteString(writer, "price", price.FormatInvariant() + " " + (string)currency.CurrencyCode);

							WriteString(writer, "gtin", gtin);
							WriteString(writer, "brand", brand);
							WriteString(writer, "mpn", mpn);

							var identifierExists = brand.HasValue() && (gtin.HasValue() || mpn.HasValue());
							WriteString(writer, "identifier_exists", identifierExists ? "yes" : "no");

							if (config.Gender.IsCaseInsensitiveEqual(Unspecified))
								WriteString(writer, "gender", "");
							else
								WriteString(writer, "gender", gmc != null && gmc.Gender.HasValue() ? gmc.Gender : config.Gender);

							if (config.AgeGroup.IsCaseInsensitiveEqual(Unspecified))
								WriteString(writer, "age_group", "");
							else
								WriteString(writer, "age_group", gmc != null && gmc.AgeGroup.HasValue() ? gmc.AgeGroup : config.AgeGroup);

							WriteString(writer, "color", gmc != null && gmc.Color.HasValue() ? gmc.Color : config.Color);
							WriteString(writer, "size", gmc != null && gmc.Size.HasValue() ? gmc.Size : config.Size);
							WriteString(writer, "material", gmc != null && gmc.Material.HasValue() ? gmc.Material : config.Material);
							WriteString(writer, "pattern", gmc != null && gmc.Pattern.HasValue() ? gmc.Pattern : config.Pattern);
							WriteString(writer, "item_group_id", gmc != null && gmc.ItemGroupId.HasValue() ? gmc.ItemGroupId : "");

							if (config.ExpirationDays > 0)
							{
								WriteString(writer, "expiration_date", DateTime.UtcNow.AddDays(config.ExpirationDays).ToString("yyyy-MM-dd"));
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

								WriteString(writer, "shipping_weight", weightInfo);
							}

							if (config.ExportBasePrice && entity.BasePriceHasValue)
							{
								var measureUnit = BasePriceUnits((string)product.BasePriceMeasureUnit);

								if (BasePriceSupported(entity.BasePriceBaseAmount ?? 0, measureUnit))
								{
									var basePriceMeasure = "{0} {1}".FormatInvariant((entity.BasePriceAmount ?? decimal.Zero).FormatInvariant(), measureUnit);
									var basePriceBaseMeasure = "{0} {1}".FormatInvariant(entity.BasePriceBaseAmount ?? 1, measureUnit);

									WriteString(writer, "unit_pricing_measure", basePriceMeasure);
									WriteString(writer, "unit_pricing_base_measure", basePriceBaseMeasure);
								}
							}

							if (gmc != null)
							{
								WriteString(writer, "multipack", gmc.Multipack > 1 ? gmc.Multipack.ToString() : null);
								WriteString(writer, "is_bundle", gmc.IsBundle.HasValue ? (gmc.IsBundle.Value ? "yes" : "no") : null);
								WriteString(writer, "adult", gmc.IsAdult.HasValue ? (gmc.IsAdult.Value ? "yes" : "no") : null);
								WriteString(writer, "energy_efficiency_class", gmc.EnergyEfficiencyClass.HasValue() ? gmc.EnergyEfficiencyClass : null);

								WriteString(writer, "custom_label_0", gmc.CustomLabel0.HasValue() ? gmc.CustomLabel0 : null);
								WriteString(writer, "custom_label_1", gmc.CustomLabel1.HasValue() ? gmc.CustomLabel1 : null);
								WriteString(writer, "custom_label_2", gmc.CustomLabel2.HasValue() ? gmc.CustomLabel2 : null);
								WriteString(writer, "custom_label_3", gmc.CustomLabel3.HasValue() ? gmc.CustomLabel3 : null);
								WriteString(writer, "custom_label_4", gmc.CustomLabel4.HasValue() ? gmc.CustomLabel4 : null);
							}

							++context.RecordsSucceeded;
						}
						catch (Exception exception)
						{
							context.RecordException(exception, entity.Id);
						}

						writer.WriteEndElement(); // item
					}
				}

				writer.WriteEndElement(); // channel
				writer.WriteEndElement(); // rss
				writer.WriteEndDocument();
			}
		}
	}
}