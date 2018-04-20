using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.GoogleMerchantCenter.Models;
using SmartStore.GoogleMerchantCenter.Services;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.DataExchange.Export;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;

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
		private readonly ICommonServices _services;
		private readonly IProductAttributeService _productAttributeService;
		private readonly MeasureSettings _measureSettings;
		private Multimap<string, int> _attributeMappings;

		public GmcXmlExportProvider(
			IGoogleFeedService googleFeedService,
			IMeasureService measureService,
			ICommonServices services,
			IProductAttributeService productAttributeService,
			MeasureSettings measureSettings)
		{
			_googleFeedService = googleFeedService;
			_measureService = measureService;
			_services = services;
			_productAttributeService = productAttributeService;
			_measureSettings = measureSettings;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		private Multimap<string, int> AttributeMappings
		{
			get
			{
				if (_attributeMappings == null)
				{
					_attributeMappings = _productAttributeService.GetExportFieldMappings("gmc");
				}

				return _attributeMappings;
			}
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

		private string GetAttributeValue(
			Multimap<int, ProductVariantAttributeValue> attributeValues,
			string fieldName,
			int languageId,
			string productEditTabValue,
			string defaultValue)
		{
			// 1. attribute export mapping.
			if (attributeValues != null && AttributeMappings.ContainsKey(fieldName))
			{
				foreach (var attributeId in AttributeMappings[fieldName])
				{
					if (attributeValues.ContainsKey(attributeId))
					{
						var attributeValue = attributeValues[attributeId].FirstOrDefault(x => x.ProductVariantAttribute.ProductAttributeId == attributeId);
						if (attributeValue != null)
						{
							return attributeValue.GetLocalized(x => x.Name, languageId, true, false).Value.EmptyNull();
						}
					}
				}
			}

			// 2. explicit set to unspecified.
			if (defaultValue.IsCaseInsensitiveEqual(Unspecified))
			{
				return string.Empty;
			}

			// 3. product edit tab value.
			if (productEditTabValue.HasValue())
			{
				return productEditTabValue;
			}

			return defaultValue.EmptyNull();
		}

		private string GetBaseMeasureWeight()
		{
			var measureWeightEntity = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
			var measureWeight = measureWeightEntity != null
				? measureWeightEntity.SystemKeyword.EmptyNull().ToLower()
				: string.Empty;

			switch (measureWeight)
			{
				case "gram":
				case "gramme":
					return "g";
				case "mg":
				case "milligramme":
				case "milligram":
					return "mg";
				case "lb":
					return "lb";
				case "ounce":
				case "oz":
					return "oz";
				default:
					return "kg";
			}
		}

		public static string SystemName => "Feeds.GoogleMerchantCenterProductXml";

		public static string Unspecified => "__nospec__";

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

						model.LanguageSeoCode = _services.WorkContext.WorkingLanguage.UniqueSeoCode.EmptyNull().ToLower();

						model.AvailableCategories = model.DefaultGoogleCategory.HasValue()
							? new List<SelectListItem> { new SelectListItem { Text = model.DefaultGoogleCategory, Value = model.DefaultGoogleCategory, Selected = true } }
							: new List<SelectListItem>();
					}
				};
			}
		}

		public override string FileExtension => "XML";

		protected override void Export(ExportExecuteContext context)
		{
			Currency currency = context.Currency.Entity;
			var languageId = context.Projection.LanguageId ?? 0;
			var dateFormat = "yyyy-MM-ddTHH:mmZ";
			var defaultCondition = "new";
			var defaultAvailability = "in stock";
			var measureWeight = GetBaseMeasureWeight();

			var config = (context.ConfigurationData as ProfileConfigurationModel) ?? new ProfileConfigurationModel();

			if (config.Condition.IsCaseInsensitiveEqual(Unspecified))
			{
				defaultCondition = string.Empty;
			}
			else if (config.Condition.HasValue())
			{
				defaultCondition = config.Condition;
			}

			if (config.Availability.IsCaseInsensitiveEqual(Unspecified))
			{
				defaultAvailability = string.Empty;
			}
			else if (config.Availability.HasValue())
			{
				defaultAvailability = config.Availability;
			}


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
							var isParent = (bool)product._IsParent;
							string brand = product._Brand;
							string gtin = product.Gtin;
							string mpn = product.ManufacturerPartNumber;
							var availability = defaultAvailability;

							var attributeValues = !isParent && product._AttributeCombinationValues != null
								? ((ICollection<ProductVariantAttributeValue>)product._AttributeCombinationValues).ToMultimap(x => x.ProductVariantAttribute.ProductAttributeId, x => x)
								: new Multimap<int, ProductVariantAttributeValue>();

							var specialPrice = product._FutureSpecialPrice as decimal?;
							if (!specialPrice.HasValue)
								specialPrice = product._SpecialPrice;

							if (category.IsEmpty())
								category = config.DefaultGoogleCategory;

							if (category.IsEmpty())
								context.Log.Error(T("Plugins.Feed.Froogle.MissingDefaultCategory"));

							if (entity.ManageInventoryMethod == ManageInventoryMethod.ManageStock && entity.StockQuantity <= 0)
							{
								if (entity.BackorderMode == BackorderMode.NoBackorders)
									availability = "out of stock";
								else if (entity.BackorderMode == BackorderMode.AllowQtyBelow0 || entity.BackorderMode == BackorderMode.AllowQtyBelow0AndNotifyCustomer)
									availability = entity.AvailableForPreOrder ? "preorder" : "out of stock";
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

							var condition = GetAttributeValue(attributeValues, "condition", languageId, null, defaultCondition);
							WriteString(writer, "condition", condition);

							WriteString(writer, "availability", availability);

							if (availability == "preorder" && entity.AvailableStartDateTimeUtc.HasValue && entity.AvailableStartDateTimeUtc.Value > DateTime.UtcNow)
							{
								var availabilityDate = entity.AvailableStartDateTimeUtc.Value.ToString(dateFormat);

								WriteString(writer, "availability_date", availabilityDate);
							}

							if (config.SpecialPrice && specialPrice.HasValue)
							{
								WriteString(writer, "sale_price", string.Concat(specialPrice.Value.FormatInvariant(), " ", currency.CurrencyCode));

								if (entity.SpecialPriceStartDateTimeUtc.HasValue && entity.SpecialPriceEndDateTimeUtc.HasValue)
								{
									var specialPriceDate = "{0}/{1}".FormatInvariant(
										entity.SpecialPriceStartDateTimeUtc.Value.ToString(dateFormat), entity.SpecialPriceEndDateTimeUtc.Value.ToString(dateFormat));

									WriteString(writer, "sale_price_effective_date", specialPriceDate);
								}

								price = (product._RegularPrice as decimal?) ?? price;
							}

							WriteString(writer, "price", string.Concat(price.FormatInvariant(), " ", currency.CurrencyCode));

							WriteString(writer, "gtin", gtin);
							WriteString(writer, "brand", brand);
							WriteString(writer, "mpn", mpn);

							var identifierExists = brand.HasValue() && (gtin.HasValue() || mpn.HasValue());
							WriteString(writer, "identifier_exists", identifierExists ? "yes" : "no");

							var gender = GetAttributeValue(attributeValues, "gender", languageId, gmc?.Gender, config.Gender);
							WriteString(writer, "gender", gender);

							var ageGroup = GetAttributeValue(attributeValues, "age_group", languageId, gmc?.AgeGroup, config.AgeGroup);
							WriteString(writer, "age_group", ageGroup);

							var color = GetAttributeValue(attributeValues, "color", languageId, gmc?.Color, config.Color);
							WriteString(writer, "color", color);

							var size = GetAttributeValue(attributeValues, "size", languageId, gmc?.Size, config.Size);
							WriteString(writer, "size", size);

							var material = GetAttributeValue(attributeValues, "material", languageId, gmc?.Material, config.Material);
							WriteString(writer, "material", material);

							var pattern = GetAttributeValue(attributeValues, "pattern", languageId, gmc?.Pattern, config.Pattern);
							WriteString(writer, "pattern", pattern);

							var itemGroupId = gmc != null && gmc.ItemGroupId.HasValue() ? gmc.ItemGroupId : string.Empty;
							if (itemGroupId.HasValue())
							{
								WriteString(writer, "item_group_id", itemGroupId);
							}

							if (config.ExpirationDays > 0)
							{
								WriteString(writer, "expiration_date", DateTime.UtcNow.AddDays(config.ExpirationDays).ToString("yyyy-MM-dd"));
							}

							if (config.ExportShipping)
							{
								var weight = string.Concat(((decimal)product.Weight).FormatInvariant(), " ", measureWeight);
								WriteString(writer, "shipping_weight", weight);
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
							}

							var customLabel0 = GetAttributeValue(attributeValues, "custom_label_0", languageId, gmc?.CustomLabel0, null);
							var customLabel1 = GetAttributeValue(attributeValues, "custom_label_1", languageId, gmc?.CustomLabel1, null);
							var customLabel2 = GetAttributeValue(attributeValues, "custom_label_2", languageId, gmc?.CustomLabel2, null);
							var customLabel3 = GetAttributeValue(attributeValues, "custom_label_3", languageId, gmc?.CustomLabel3, null);
							var customLabel4 = GetAttributeValue(attributeValues, "custom_label_4", languageId, gmc?.CustomLabel4, null);

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