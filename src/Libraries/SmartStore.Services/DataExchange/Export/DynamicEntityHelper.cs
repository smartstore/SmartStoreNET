using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using SmartStore.Collections;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Html;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.DataExchange.Export.Events;
using SmartStore.Services.DataExchange.Export.Internal;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;

namespace SmartStore.Services.DataExchange.Export
{
	public partial class DataExporter
	{
		private readonly string[] _orderCustomerAttributes = new string[]
		{
			SystemCustomerAttributeNames.Gender,
			SystemCustomerAttributeNames.DateOfBirth,
			SystemCustomerAttributeNames.VatNumber,
			SystemCustomerAttributeNames.VatNumberStatusId,
			SystemCustomerAttributeNames.TimeZoneId,
			SystemCustomerAttributeNames.CustomerNumber,
			SystemCustomerAttributeNames.ImpersonatedCustomerId
		};

		private void PrepareProductDescription(DataExporterContext ctx, dynamic dynObject, Product product)
		{
			try
			{
				var languageId = (ctx.Projection.LanguageId ?? 0);
				string description = "";

				// description merging
				if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.None)
				{
					// export empty description
				}
				else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ShortDescriptionOrNameIfEmpty)
				{
					description = dynObject.FullDescription;

					if (description.IsEmpty())
						description = dynObject.ShortDescription;
					if (description.IsEmpty())
						description = dynObject.Name;
				}
				else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ShortDescription)
				{
					description = dynObject.ShortDescription;
				}
				else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.Description)
				{
					description = dynObject.FullDescription;
				}
				else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.NameAndShortDescription)
				{
					description = ((string)dynObject.Name).Grow((string)dynObject.ShortDescription, " ");
				}
				else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.NameAndDescription)
				{
					description = ((string)dynObject.Name).Grow((string)dynObject.FullDescription, " ");
				}
				else if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ManufacturerAndNameAndShortDescription ||
					ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ManufacturerAndNameAndDescription)
				{
					var productManus = ctx.ProductExportContext.ProductManufacturers.GetOrLoad(product.Id);

					if (productManus != null && productManus.Any())
						description = productManus.First().Manufacturer.GetLocalized(x => x.Name, languageId, true, false);

					description = description.Grow((string)dynObject.Name, " ");

					if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ManufacturerAndNameAndShortDescription)
						description = description.Grow((string)dynObject.ShortDescription, " ");
					else
						description = description.Grow((string)dynObject.FullDescription, " ");
				}

				// append text
				if (ctx.Projection.AppendDescriptionText.HasValue() && ((string)dynObject.ShortDescription).IsEmpty() && ((string)dynObject.FullDescription).IsEmpty())
				{
					string[] appendText = ctx.Projection.AppendDescriptionText.SplitSafe(",");
					if (appendText.Length > 0)
					{
						var rnd = (new Random()).Next(0, appendText.Length - 1);

						description = description.Grow(appendText.SafeGet(rnd), " ");
					}
				}

				// remove critical characters
				if (description.HasValue() && ctx.Projection.RemoveCriticalCharacters)
				{
					foreach (var str in ctx.Projection.CriticalCharacters.SplitSafe(","))
						description = description.Replace(str, "");
				}

				// convert to plain text
				if (description.HasValue() && ctx.Projection.DescriptionToPlainText)
				{
					//Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
					//description = HttpUtility.HtmlDecode(reg.Replace(description, ""));

					description = HtmlUtils.ConvertHtmlToPlainText(description);
					description = HtmlUtils.StripTags(HttpUtility.HtmlDecode(description));
				}

				dynObject.FullDescription = description;
			}
			catch { }
		}

		private decimal? ConvertPrice(DataExporterContext ctx, Product product, decimal? price)
		{
			if (price.HasValue)
			{
				if (ctx.Projection.ConvertNetToGrossPrices)
				{
					decimal taxRate;
					price = _taxService.Value.GetProductPrice(product, product.TaxCategoryId, price.Value, true, ctx.ContextCustomer, ctx.ContextCurrency,
						_taxSettings.Value.PricesIncludeTax, out taxRate);
				}

				if (price != decimal.Zero)
				{
					price = _currencyService.Value.ConvertFromPrimaryStoreCurrency(price.Value, ctx.ContextCurrency, ctx.Store);
				}
			}

			return price;
		}

		private decimal CalculatePrice(
			DataExporterContext ctx,
			Product product,
			ProductVariantAttributeCombination combination,
			ICollection<ProductVariantAttributeValue> attributeValues)
		{
			var price = product.Price;
			var priceCalculationContext = ctx.ProductExportContext as PriceCalculationContext;

			if (combination != null)
			{
				// price for attribute combination
				var attributesTotalPriceBase = decimal.Zero;

				if (attributeValues != null)
				{
					attributeValues.Each(x => attributesTotalPriceBase += _priceCalculationService.Value.GetProductVariantAttributeValuePriceAdjustment(x, product, ctx.ContextCustomer, priceCalculationContext));
				}

				price = _priceCalculationService.Value.GetFinalPrice(product, null, ctx.ContextCustomer, attributesTotalPriceBase, true, 1, null, priceCalculationContext);
			}
			else if (ctx.Projection.PriceType.HasValue)
			{
				// price for product
				if (ctx.Projection.PriceType.Value == PriceDisplayType.LowestPrice)
				{
					bool displayFromMessage;
					price = _priceCalculationService.Value.GetLowestPrice(product, ctx.ContextCustomer, priceCalculationContext, out displayFromMessage);
				}
				else if (ctx.Projection.PriceType.Value == PriceDisplayType.PreSelectedPrice)
				{
					price = _priceCalculationService.Value.GetPreselectedPrice(product, ctx.ContextCustomer, ctx.ContextCurrency, priceCalculationContext);
				}
				else if (ctx.Projection.PriceType.Value == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
				{
					price = _priceCalculationService.Value.GetFinalPrice(product, null, ctx.ContextCustomer, decimal.Zero, false, 1, null, priceCalculationContext);
				}
			}

			return ConvertPrice(ctx, product, price) ?? price;
		}

		private List<dynamic> GetLocalized<T>(DataExporterContext ctx, T entity, params Expression<Func<T, string>>[] keySelectors)
			where T : BaseEntity, ILocalizedEntity
		{
			if (ctx.Languages.Count <= 1)
				return null;

			var localized = new List<dynamic>();

			var localeKeyGroup = typeof(T).Name;
			var isSlugSupported = typeof(ISlugSupported).IsAssignableFrom(typeof(T));

			foreach (var language in ctx.Languages)
			{
				var languageCulture = language.Value.LanguageCulture.EmptyNull().ToLower();

				// add SeName
				if (isSlugSupported)
				{
					var value = _urlRecordService.Value.GetActiveSlug(entity.Id, localeKeyGroup, language.Value.Id);
					if (value.HasValue())
					{
						dynamic exp = new HybridExpando();
						exp.Culture = languageCulture;
						exp.LocaleKey = "SeName";
						exp.LocaleValue = value;

						localized.Add(exp);
					}
				}

				foreach (var keySelector in keySelectors)
				{
					var member = keySelector.Body as MemberExpression;
					var propInfo = member.Member as PropertyInfo;
					string localeKey = propInfo.Name;
					var value = _localizedEntityService.Value.GetLocalizedValue(language.Value.Id, entity.Id, localeKeyGroup, localeKey);

					// we better not export empty values. the risk is to high that they are imported and unnecessary fill databases.
					if (value.HasValue())
					{
						dynamic exp = new HybridExpando();
						exp.Culture = languageCulture;
						exp.LocaleKey = localeKey;
						exp.LocaleValue = value;

						localized.Add(exp);
					}
				}
			}

			return (localized.Count == 0 ? null : localized);
		}

		private dynamic ToDynamic(DataExporterContext ctx, ExportProfile profile)
		{
			if (profile == null)
				return null;

			dynamic result = new DynamicEntity(profile);
			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Currency currency)
		{
			if (currency == null)
				return null;

			dynamic result = new DynamicEntity(currency);

			result.Name = currency.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			result._Localized = GetLocalized(ctx, currency, x => x.Name);

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Language language)
		{
			if (language == null)
				return null;

			dynamic result = new DynamicEntity(language);
			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Country country)
		{
			if (country == null)
				return null;

			dynamic result = new DynamicEntity(country);

			result.Name = country.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			result._Localized = GetLocalized(ctx, country, x => x.Name);

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Address address)
		{
			if (address == null)
				return null;

			dynamic result = new DynamicEntity(address);

			result.Country = ToDynamic(ctx, address.Country);

			if (address.StateProvinceId.GetValueOrDefault() > 0)
			{
				dynamic sp = new DynamicEntity(address.StateProvince);

				sp.Name = address.StateProvince.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
				sp._Localized = GetLocalized(ctx, address.StateProvince, x => x.Name);

				result.StateProvince = sp;
			}
			else
			{
				result.StateProvince = null;
			}

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, RewardPointsHistory points)
		{
			if (points == null)
				return null;

			dynamic result = new DynamicEntity(points);

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Customer customer)
		{
			if (customer == null)
				return null;

			dynamic result = new DynamicEntity(customer);

			result.BillingAddress = null;
			result.ShippingAddress = null;
			result.Addresses = null;

			result.RewardPointsHistory = null;
			result._RewardPointsBalance = 0;

			result._GenericAttributes = null;
			result._HasNewsletterSubscription = false;

			result._FullName = null;
			result._AvatarPictureUrl = null;

			result.CustomerRoles = customer.CustomerRoles
				.Select(x =>
				{
					dynamic dyn = new DynamicEntity(x);
					return dyn;
				})
				.ToList();

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Store store)
		{
			if (store == null)
				return null;

			dynamic result = new DynamicEntity(store);

			result.PrimaryStoreCurrency = ToDynamic(ctx, store.PrimaryStoreCurrency);
			result.PrimaryExchangeRateCurrency = ToDynamic(ctx, store.PrimaryExchangeRateCurrency);

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, DeliveryTime deliveryTime)
		{
			if (deliveryTime == null)
				return null;

			dynamic result = new DynamicEntity(deliveryTime);

			result.Name = deliveryTime.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			result._Localized = GetLocalized(ctx, deliveryTime, x => x.Name);

			return result;
		}

		private void ToDeliveryTime(DataExporterContext ctx, dynamic parent, int? deliveryTimeId)
		{
			if (ctx.DeliveryTimes != null)
			{
				if (deliveryTimeId.HasValue && ctx.DeliveryTimes.ContainsKey(deliveryTimeId.Value))
					parent.DeliveryTime = ToDynamic(ctx, ctx.DeliveryTimes[deliveryTimeId.Value]);
				else
					parent.DeliveryTime = null;
			}
		}

		private dynamic ToDynamic(DataExporterContext ctx, QuantityUnit quantityUnit)
		{
			if (quantityUnit == null)
				return null;

			dynamic result = new DynamicEntity(quantityUnit);

			result.Name = quantityUnit.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			result.Description = quantityUnit.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);

			result._Localized = GetLocalized(ctx, quantityUnit,
				x => x.Name,
				x => x.Description);

			return result;
		}

		private void ToQuantityUnit(DataExporterContext ctx, dynamic parent, int? quantityUnitId)
		{
			if (ctx.QuantityUnits != null)
			{
				if (quantityUnitId.HasValue && ctx.QuantityUnits.ContainsKey(quantityUnitId.Value))
					parent.QuantityUnit = ToDynamic(ctx, ctx.QuantityUnits[quantityUnitId.Value]);
				else
					parent.QuantityUnit = null;
			}
		}

		private dynamic ToDynamic(DataExporterContext ctx, Picture picture, int thumbPictureSize, int detailsPictureSize)
		{
			if (picture == null)
				return null;

			// TODO: (mc) Refactor > GetPictureInfo

			dynamic result = new DynamicEntity(picture);
			var pictureInfo = _pictureService.Value.GetPictureInfo(picture);
			var host = _services.StoreService.GetHost(ctx.Store);

			if (pictureInfo != null)
			{
				result._FileName = System.IO.Path.GetFileName(pictureInfo.Path);
				result._RelativeUrl = _pictureService.Value.GetUrl(pictureInfo, 0, FallbackPictureType.NoFallback);
				result._ThumbImageUrl = _pictureService.Value.GetUrl(pictureInfo, thumbPictureSize, FallbackPictureType.NoFallback, host);
				result._ImageUrl = _pictureService.Value.GetUrl(pictureInfo, detailsPictureSize, FallbackPictureType.NoFallback, host);
				result._FullSizeImageUrl = _pictureService.Value.GetUrl(pictureInfo, 0, FallbackPictureType.NoFallback, host);

				//result._ThumbLocalPath = _pictureService.Value.GetThumbLocalPath(picture);
			}

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, ProductVariantAttribute pva)
		{
			if (pva == null)
				return null;

			dynamic result = new DynamicEntity(pva);

			dynamic attribute = new DynamicEntity(pva.ProductAttribute);

			attribute.Name = pva.ProductAttribute.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			attribute.Description = pva.ProductAttribute.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);

			attribute.Values = pva.ProductVariantAttributeValues
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic dyn = new DynamicEntity(x);

					dyn.Name = x.GetLocalized(y => y.Name, ctx.Projection.LanguageId ?? 0, true, false);
					dyn._Localized = GetLocalized(ctx, x, y => y.Name);

					return dyn;
				})
				.ToList();

			attribute._Localized = GetLocalized(ctx, pva.ProductAttribute,
				x => x.Name,
				x => x.Description);

			result.Attribute = attribute;

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, ProductVariantAttributeCombination pvac)
		{
			if (pvac == null)
				return null;

			dynamic result = new DynamicEntity(pvac);

			ToDeliveryTime(ctx, result, pvac.DeliveryTimeId);
			ToQuantityUnit(ctx, result, pvac.QuantityUnitId);

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Manufacturer manufacturer)
		{
			if (manufacturer == null)
				return null;

			dynamic result = new DynamicEntity(manufacturer);

			result.Picture = null;
			result.Name = manufacturer.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);

			if (!ctx.IsPreview)
			{
				result.SeName = manufacturer.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
				result.Description = manufacturer.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);
				result.MetaKeywords = manufacturer.GetLocalized(x => x.MetaKeywords, ctx.Projection.LanguageId ?? 0, true, false);
				result.MetaDescription = manufacturer.GetLocalized(x => x.MetaDescription, ctx.Projection.LanguageId ?? 0, true, false);
				result.MetaTitle = manufacturer.GetLocalized(x => x.MetaTitle, ctx.Projection.LanguageId ?? 0, true, false);

				result._Localized = GetLocalized(ctx, manufacturer,
					x => x.Name,
					x => x.Description,
					x => x.MetaKeywords,
					x => x.MetaDescription,
					x => x.MetaTitle);
			}

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Category category)
		{
			if (category == null)
				return null;

			dynamic result = new DynamicEntity(category);

			result.Picture = null;
			result.Name = category.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			result.FullName = category.GetLocalized(x => x.FullName, ctx.Projection.LanguageId ?? 0, true, false);

			if (!ctx.IsPreview)
			{
				result.SeName = category.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
				result.Description = category.GetLocalized(x => x.Description, ctx.Projection.LanguageId ?? 0, true, false);
				result.BottomDescription = category.GetLocalized(x => x.BottomDescription, ctx.Projection.LanguageId ?? 0, true, false);
				result.MetaKeywords = category.GetLocalized(x => x.MetaKeywords, ctx.Projection.LanguageId ?? 0, true, false);
				result.MetaDescription = category.GetLocalized(x => x.MetaDescription, ctx.Projection.LanguageId ?? 0, true, false);
				result.MetaTitle = category.GetLocalized(x => x.MetaTitle, ctx.Projection.LanguageId ?? 0, true, false);

				result._CategoryTemplateViewPath = ctx.CategoryTemplates.ContainsKey(category.CategoryTemplateId)
					? ctx.CategoryTemplates[category.CategoryTemplateId]
					: "";

				result._Localized = GetLocalized(ctx, category,
					x => x.Name,
					x => x.FullName,
					x => x.Description,
					x => x.BottomDescription,
					x => x.MetaKeywords,
					x => x.MetaDescription,
					x => x.MetaTitle);
			}

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Product product, string seName = null)
		{
			if (product == null)
				return null;

			dynamic result = new DynamicEntity(product);

			result.AppliedDiscounts = null;
			result.TierPrices = null;
			result.ProductAttributes = null;
			result.ProductAttributeCombinations = null;
			result.ProductPictures = null;
			result.ProductCategories = null;
			result.ProductManufacturers = null;
			result.ProductTags = null;
			result.ProductSpecificationAttributes = null;
			result.ProductBundleItems = null;

			result.Name = product.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);

			if (!ctx.IsPreview)
			{
				result.SeName = seName ?? product.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
				result.ShortDescription = product.GetLocalized(x => x.ShortDescription, ctx.Projection.LanguageId ?? 0, true, false);
				result.FullDescription = product.GetLocalized(x => x.FullDescription, ctx.Projection.LanguageId ?? 0, true, false, true);
				result.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, ctx.Projection.LanguageId ?? 0, true, false);
				result.MetaDescription = product.GetLocalized(x => x.MetaDescription, ctx.Projection.LanguageId ?? 0, true, false);
				result.MetaTitle = product.GetLocalized(x => x.MetaTitle, ctx.Projection.LanguageId ?? 0, true, false);
				result.BundleTitleText = product.GetLocalized(x => x.BundleTitleText, ctx.Projection.LanguageId ?? 0, true, false);

				result._ProductTemplateViewPath = ctx.ProductTemplates.ContainsKey(product.ProductTemplateId)
					? ctx.ProductTemplates[product.ProductTemplateId]
					: "";

				result._BasePriceInfo = product.GetBasePriceInfo(_services.Localization, _priceFormatter.Value, _currencyService.Value, _taxService.Value,
					_priceCalculationService.Value, ctx.ContextCustomer, ctx.ContextCurrency, decimal.Zero, true);

				ToDeliveryTime(ctx, result, product.DeliveryTimeId);
				ToQuantityUnit(ctx, result, product.QuantityUnitId);

				result._Localized = GetLocalized(ctx, product,
					x => x.Name,
					x => x.ShortDescription,
					x => x.FullDescription,
					x => x.MetaKeywords,
					x => x.MetaDescription,
					x => x.MetaTitle,
					x => x.BundleTitleText);
			}

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Product product, bool isParent, DynamicProductContext productContext)
		{
			product.MergeWithCombination(productContext.Combination);

			var languageId = ctx.Projection.LanguageId ?? 0;
			var numberOfPictures = ctx.Projection.NumberOfPictures ?? int.MaxValue;
			var productDetailsPictureSize = ctx.Projection.PictureSize > 0 ? ctx.Projection.PictureSize : _mediaSettings.Value.ProductDetailsPictureSize;

			IEnumerable<ProductPicture> productPictures = ctx.ProductExportContext.ProductPictures.GetOrLoad(product.Id);
			var productManufacturers = ctx.ProductExportContext.ProductManufacturers.GetOrLoad(product.Id);
			var productCategories = ctx.ProductExportContext.ProductCategories.GetOrLoad(product.Id);
			var productAttributes = ctx.ProductExportContext.Attributes.GetOrLoad(product.Id);
			var productTags = ctx.ProductExportContext.ProductTags.GetOrLoad(product.Id);
			var specificationAttributes = ctx.ProductExportContext.SpecificationAttributes.GetOrLoad(product.Id);
            Multimap<int, string> variantAttributes = null;
            ICollection<ProductVariantAttributeValue> variantAttributeValues = null;
            string attributesXml = null;

            dynamic dynObject = ToDynamic(ctx, product, productContext.SeName);
            dynObject._IsParent = isParent;
            dynObject._CategoryName = null;
            dynObject._CategoryPath = null;
            dynObject._AttributeCombination = null;
            dynObject._AttributeCombinationValues = null;
            dynObject._AttributeCombinationId = 0;

            if (productContext.Combination != null)
            {
                var pictureIds = productContext.Combination.GetAssignedPictureIds();
                productPictures = productPictures.Where(x => pictureIds.Contains(x.PictureId));

                attributesXml = productContext.Combination.AttributesXml;
                variantAttributes = _productAttributeParser.Value.DeserializeProductVariantAttributes(attributesXml);
                variantAttributeValues = _productAttributeParser.Value.ParseProductVariantAttributeValues(variantAttributes, productAttributes);

                dynObject._AttributeCombinationId = productContext.Combination.Id;
                dynObject._UniqueId = string.Concat(product.Id, "-", productContext.Combination.Id);

                if (ctx.Supports(ExportFeatures.UsesAttributeCombination))
                {
                    dynObject._AttributeCombination = variantAttributes;
                    dynObject._AttributeCombinationValues = variantAttributeValues;
                }

                if (ctx.Projection.AttributeCombinationValueMerging == ExportAttributeValueMerging.AppendAllValuesToName)
                {
                    var valueNames = variantAttributeValues
                        .Select(x => x.GetLocalized(y => y.Name, languageId, true, false))
                        .ToList();

                    dynObject.Name = ((string)dynObject.Name).Grow(string.Join(", ", valueNames), " ");
                }
            }
            else
            {
                dynObject._UniqueId = product.Id.ToString();
            }

            productPictures = productPictures.Take(numberOfPictures);

            #region Gerneral data

            if (attributesXml.HasValue())
            {
                var query = new ProductVariantQuery();
                _productUrlHelper.Value.DeserializeQuery(query, product.Id, attributesXml, 0, productAttributes);

                dynObject._DetailUrl = productContext.AbsoluteProductUrl + _productUrlHelper.Value.ToQueryString(query);
            }
            else
            {
                dynObject._DetailUrl = productContext.AbsoluteProductUrl;
            }            

			dynObject.Price = CalculatePrice(ctx, product, productContext.Combination, variantAttributeValues);

			// Category path
			{
				var categoryPath = string.Empty;
				var pc = productCategories.OrderBy(x => x.DisplayOrder).FirstOrDefault();

				if (pc != null)
				{
					var node = _categoryService.Value.GetCategoryTree(pc.CategoryId, true, ctx.Store.Id);
					if (node != null)
					{
						categoryPath = _categoryService.Value.GetCategoryPath(node, ctx.Projection.LanguageId, null, " > ");
					}
				}

				dynObject._CategoryPath = categoryPath;
			}

			if (ctx.Countries != null)
			{
				dynObject.CountryOfOrigin = product.CountryOfOriginId.HasValue && ctx.Countries.ContainsKey(product.CountryOfOriginId.Value)
					? ToDynamic(ctx, ctx.Countries[product.CountryOfOriginId.Value])
					: null;
			}

			dynObject.ProductPictures = productPictures
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic dyn = new DynamicEntity(x);

					dyn.Picture = ToDynamic(ctx, x.Picture, _mediaSettings.Value.ProductThumbPictureSize, productDetailsPictureSize);

					return dyn;
				})
				.ToList();

			dynObject.ProductManufacturers = productManufacturers
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic dyn = new DynamicEntity(x);

					dyn.Manufacturer = ToDynamic(ctx, x.Manufacturer);

					dyn.Manufacturer.Picture = x.Manufacturer != null && x.Manufacturer.PictureId.HasValue
						? ToDynamic(ctx, x.Manufacturer.Picture, _mediaSettings.Value.ManufacturerThumbPictureSize, _mediaSettings.Value.ManufacturerThumbPictureSize)
						: null;

					return dyn;
				})
				.ToList();

			dynObject.ProductCategories = productCategories
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic dyn = new DynamicEntity(x);

					dyn.Category = ToDynamic(ctx, x.Category);

					if (x.Category != null && x.Category.PictureId.HasValue)
						dyn.Category.Picture = ToDynamic(ctx, x.Category.Picture, _mediaSettings.Value.CategoryThumbPictureSize, _mediaSettings.Value.CategoryThumbPictureSize);

					if (dynObject._CategoryName == null)
						dynObject._CategoryName = (string)dyn.Category.Name;

					return dyn;
				})
				.ToList();

			dynObject.ProductAttributes = productAttributes
				.OrderBy(x => x.DisplayOrder)
				.Select(x => ToDynamic(ctx, x))
				.ToList();

			// Do not export combinations if a combination is exported as a product.
			if (productContext.Combinations != null && productContext.Combination == null)
			{
				dynObject.ProductAttributeCombinations = productContext.Combinations
                    .Select(x =>
					{
						dynamic dyn = ToDynamic(ctx, x);
						var assignedPictures = new List<dynamic>();

						foreach (int pictureId in x.GetAssignedPictureIds().Take(numberOfPictures))
						{
							var assignedPicture = productPictures.FirstOrDefault(y => y.PictureId == pictureId);
							if (assignedPicture != null && assignedPicture.Picture != null)
							{
								assignedPictures.Add(ToDynamic(ctx, assignedPicture.Picture, _mediaSettings.Value.ProductThumbPictureSize, productDetailsPictureSize));
							}
						}

						dyn.Pictures = assignedPictures;

						return dyn;
					})
					.ToList();
			}
			else
			{
				dynObject.ProductAttributeCombinations = Enumerable.Empty<ProductVariantAttributeCombination>();
			}

			if (product.HasTierPrices)
			{
				var tierPrices = ctx.ProductExportContext.TierPrices.GetOrLoad(product.Id)
					.RemoveDuplicatedQuantities();

				dynObject.TierPrices = tierPrices
					.Select(x =>
					{
						dynamic dyn = new DynamicEntity(x);

						return dyn;
					})
					.ToList();
			}

			if (product.HasDiscountsApplied)
			{
				var appliedDiscounts = ctx.ProductExportContext.AppliedDiscounts.GetOrLoad(product.Id);

				dynObject.AppliedDiscounts = appliedDiscounts
					.Select(x => ToDynamic(ctx, x))
					.ToList();
			}

			dynObject.ProductTags = productTags
				.Select(x =>
				{
					dynamic dyn = new DynamicEntity(x);

					dyn.Name = x.GetLocalized(y => y.Name, languageId, true, false);
					dyn.SeName = x.GetSeName(languageId);
					dyn._Localized = GetLocalized(ctx, x, y => y.Name);

					return dyn;
				})
				.ToList();

			dynObject.ProductSpecificationAttributes = specificationAttributes
				.Select(x => ToDynamic(ctx, x))
				.ToList();

			if (product.ProductType == ProductType.BundledProduct)
			{
				var bundleItems = ctx.ProductExportContext.ProductBundleItems.GetOrLoad(product.Id);

				dynObject.ProductBundleItems = bundleItems
					.Select(x =>
					{
						dynamic dyn = new DynamicEntity(x);

						dyn.Name = x.GetLocalized(y => y.Name, languageId, true, false);
						dyn.ShortDescription = x.GetLocalized(y => y.ShortDescription, languageId, true, false);
						dyn._Localized = GetLocalized(ctx, x, y => y.Name, y => y.ShortDescription);

						return dyn;
					})
					.ToList();
			}

			#endregion

			#region More data based on export features

			if (ctx.Supports(ExportFeatures.CanProjectDescription))
			{
				PrepareProductDescription(ctx, dynObject, product);
			}

			if (ctx.Supports(ExportFeatures.OffersBrandFallback))
			{
				string brand = null;
				var productManus = ctx.ProductExportContext.ProductManufacturers.GetOrLoad(product.Id);

				if (productManus != null && productManus.Any())
					brand = productManus.First().Manufacturer.GetLocalized(x => x.Name, languageId, true, false);

				if (brand.IsEmpty())
					brand = ctx.Projection.Brand;

				dynObject._Brand = brand;
			}
			
			if (ctx.Supports(ExportFeatures.CanIncludeMainPicture))
			{
				if (productPictures != null && productPictures.Any())
				{
					var firstPicture = productPictures.First().Picture;
					dynObject._MainPictureUrl = _pictureService.Value.GetUrl(firstPicture, ctx.Projection.PictureSize, host: _services.StoreService.GetHost(ctx.Store));
					dynObject._MainPictureRelativeUrl = _pictureService.Value.GetUrl(firstPicture, ctx.Projection.PictureSize);
				}
				else if (!_catalogSettings.Value.HideProductDefaultPictures)
				{
					dynObject._MainPictureUrl = _pictureService.Value.GetFallbackUrl(ctx.Projection.PictureSize, host: _services.StoreService.GetHost(ctx.Store));
					dynObject._MainPictureRelativeUrl = _pictureService.Value.GetFallbackUrl(ctx.Projection.PictureSize);
				}
				else
				{
					dynObject._MainPictureUrl = null;
					dynObject._MainPictureRelativeUrl = null;
				}
			}

			if (ctx.Supports(ExportFeatures.UsesSkuAsMpnFallback) && product.ManufacturerPartNumber.IsEmpty())
			{
				dynObject.ManufacturerPartNumber = product.Sku;
			}

			if (ctx.Supports(ExportFeatures.OffersShippingTimeFallback))
			{
				dynamic deliveryTime = dynObject.DeliveryTime;
				dynObject._ShippingTime = (deliveryTime == null ? ctx.Projection.ShippingTime : deliveryTime.Name);
			}

			if (ctx.Supports(ExportFeatures.OffersShippingCostsFallback))
			{
				dynObject._FreeShippingThreshold = ctx.Projection.FreeShippingThreshold;

				if (product.IsFreeShipping || (ctx.Projection.FreeShippingThreshold.HasValue && (decimal)dynObject.Price >= ctx.Projection.FreeShippingThreshold.Value))
					dynObject._ShippingCosts = decimal.Zero;
				else
					dynObject._ShippingCosts = ctx.Projection.ShippingCosts;
			}

			if (ctx.Supports(ExportFeatures.UsesOldPrice))
			{
				if (product.OldPrice != decimal.Zero && product.OldPrice != (decimal)dynObject.Price && !(product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing))
				{
					if (ctx.Projection.ConvertNetToGrossPrices)
					{
						decimal taxRate;
						dynObject._OldPrice = _taxService.Value.GetProductPrice(product, product.TaxCategoryId, product.OldPrice, true, ctx.ContextCustomer,
							ctx.ContextCurrency, _taxSettings.Value.PricesIncludeTax, out taxRate);
					}
					else
					{
						dynObject._OldPrice = product.OldPrice;
					}
				}
				else
				{
					dynObject._OldPrice = null;
				}
			}

			if (ctx.Supports(ExportFeatures.UsesSpecialPrice))
			{
				dynObject._SpecialPrice = null;			// special price which is valid now
				dynObject._FutureSpecialPrice = null;   // special price which is valid now and in future
				dynObject._RegularPrice = null;			// price as if a special price would not exist

				if (!(product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing))
				{
					if (product.SpecialPrice.HasValue && product.SpecialPriceEndDateTimeUtc.HasValue)
					{
						var endDate = DateTime.SpecifyKind(product.SpecialPriceEndDateTimeUtc.Value, DateTimeKind.Utc);
						if (endDate > DateTime.UtcNow)
						{
							dynObject._FutureSpecialPrice = ConvertPrice(ctx, product, product.SpecialPrice.Value);
						}
					}

					var specialPrice = _priceCalculationService.Value.GetSpecialPrice(product);

					dynObject._SpecialPrice = ConvertPrice(ctx, product, specialPrice);

					if (specialPrice.HasValue || dynObject._FutureSpecialPrice != null)
					{
						decimal tmpSpecialPrice = product.SpecialPrice.Value;
						product.SpecialPrice = null;

						dynObject._RegularPrice = CalculatePrice(ctx, product, productContext.Combination, variantAttributeValues);

						product.SpecialPrice = tmpSpecialPrice;
					}
				}
			}

			#endregion

			return dynObject;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Order order)
		{
			if (order == null)
				return null;

			dynamic result = new DynamicEntity(order);

			result.OrderNumber = order.GetOrderNumber();
			result.OrderStatus = order.OrderStatus.GetLocalizedEnum(_services.Localization, ctx.Projection.LanguageId ?? 0);
			result.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_services.Localization, ctx.Projection.LanguageId ?? 0);
			result.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_services.Localization, ctx.Projection.LanguageId ?? 0);

			result.Customer = null;
			result.BillingAddress = null;
			result.ShippingAddress = null;
			result.Shipments = null;

			result.Store = ctx.Stores.ContainsKey(order.StoreId)
				? ToDynamic(ctx, ctx.Stores[order.StoreId])
				: null;

			if (!ctx.IsPreview)
			{
				result.RedeemedRewardPointsEntry = ToDynamic(ctx, order.RedeemedRewardPointsEntry);
			}

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, OrderItem orderItem)
		{
			if (orderItem == null)
				return null;

			dynamic result = new DynamicEntity(orderItem);

			orderItem.Product.MergeWithCombination(orderItem.AttributesXml, _productAttributeParser.Value);

			result.Product = ToDynamic(ctx, orderItem.Product);

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Shipment shipment)
		{
			if (shipment == null)
				return null;

			dynamic result = new DynamicEntity(shipment);

			result.ShipmentItems = shipment.ShipmentItems
				.Select(x =>
				{
					dynamic exp = new DynamicEntity(x);

					return exp;
				})
				.ToList();

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, Discount discount)
		{
			if (discount == null)
				return null;

			dynamic result = new DynamicEntity(discount);

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, ProductSpecificationAttribute psa)
		{
			if (psa == null)
				return null;

			var option = psa.SpecificationAttributeOption;

			dynamic result = new DynamicEntity(psa);

			dynamic dynAttribute = new DynamicEntity(option.SpecificationAttribute);

			dynAttribute.Name = option.SpecificationAttribute.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			dynAttribute._Localized = GetLocalized(ctx, option.SpecificationAttribute, x => x.Name);

			dynAttribute.Alias = option.SpecificationAttribute.GetLocalized(x => x.Alias, ctx.Projection.LanguageId ?? 0, true, false);
			dynAttribute._Localized = GetLocalized(ctx, option.SpecificationAttribute, x => x.Alias);

			dynamic dynOption = new DynamicEntity(option);

			dynOption.Name = option.GetLocalized(x => x.Name, ctx.Projection.LanguageId ?? 0, true, false);
			dynOption._Localized = GetLocalized(ctx, option, x => x.Name);

			dynOption.Alias = option.GetLocalized(x => x.Alias, ctx.Projection.LanguageId ?? 0, true, false);
			dynOption._Localized = GetLocalized(ctx, option, x => x.Alias);

			dynOption.SpecificationAttribute = dynAttribute;

			result.SpecificationAttributeOption = dynOption;

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, GenericAttribute genericAttribute)
		{
			if (genericAttribute == null)
				return null;

			dynamic result = new DynamicEntity(genericAttribute);

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, NewsLetterSubscription subscription)
		{
			if (subscription == null)
				return null;

			dynamic result = new DynamicEntity(subscription);

			result.Store = ctx.Stores.ContainsKey(subscription.StoreId)
				? ToDynamic(ctx, ctx.Stores[subscription.StoreId])
				: null;

			return result;
		}

		private dynamic ToDynamic(DataExporterContext ctx, ShoppingCartItem shoppingCartItem)
		{
			if (shoppingCartItem == null)
				return null;

			dynamic result = new DynamicEntity(shoppingCartItem);

			shoppingCartItem.Product.MergeWithCombination(shoppingCartItem.AttributesXml, _productAttributeParser.Value);

			result.Store = ctx.Stores.ContainsKey(shoppingCartItem.StoreId)
				? ToDynamic(ctx, ctx.Stores[shoppingCartItem.StoreId])
				: null;

			result.Customer = ToDynamic(ctx, shoppingCartItem.Customer);
			result.Product = ToDynamic(ctx, shoppingCartItem.Product);

			return result;
		}


		private List<dynamic> Convert(DataExporterContext ctx, Product product)
		{
            var result = new List<dynamic>();
            var productContext = new DynamicProductContext();
            productContext.SeName = product.GetSeName(ctx.Projection.LanguageId ?? 0, true, false);
            productContext.Combinations = ctx.ProductExportContext.AttributeCombinations.GetOrLoad(product.Id);

            productContext.AbsoluteProductUrl = _productUrlHelper.Value.GetAbsoluteProductUrl(
                product.Id,
                productContext.SeName,
                null,
                ctx.Store,
                ctx.ContextLanguage);

            if (ctx.Projection.AttributeCombinationAsProduct && productContext.Combinations.Where(x => x.IsActive).Any())
			{
				if (ctx.Supports(ExportFeatures.UsesAttributeCombinationParent))
				{
					var dynObject = ToDynamic(ctx, product, true, productContext);
					result.Add(dynObject);
				}

				var dbContext = _dbContext as DbContext;

				foreach (var combination in productContext.Combinations.Where(x => x.IsActive))
				{
					product = _dbContext.Attach(product);
					var entry = dbContext.Entry(product);

					// The returned object is not the entity and is not being tracked by the context.
					// It also does not have any relationships set to other objects.
					// CurrentValues only includes database (thus primitive) values.
					var productClone = entry.CurrentValues.ToObject() as Product;
					_dbContext.DetachEntity(product);

                    productContext.Combination = combination;

					var dynObject = ToDynamic(ctx, productClone, false, productContext);
					result.Add(dynObject);
				}
			}
			else
			{
				var dynObject = ToDynamic(ctx, product, false, productContext);
				result.Add(dynObject);
			}

			if (result.Any())
			{
				_services.EventPublisher.Publish(new RowExportingEvent
				{
					Row = result.First(),
					EntityType = ExportEntityType.Product,
					ExportRequest = ctx.Request,
					ExecuteContext = ctx.ExecuteContext
				});
			}

			return result;
		}

		private List<dynamic> Convert(DataExporterContext ctx, Order order)
		{
			var result = new List<dynamic>();

			ctx.OrderExportContext.Addresses.Collect(order.ShippingAddressId.HasValue ? order.ShippingAddressId.Value : 0);
			ctx.OrderExportContext.Addresses.GetOrLoad(order.BillingAddressId);

			var customers = ctx.OrderExportContext.Customers.GetOrLoad(order.CustomerId);
			var genericAttributes = ctx.OrderExportContext.CustomerGenericAttributes.GetOrLoad(order.CustomerId);
			var rewardPointsHistories = ctx.OrderExportContext.RewardPointsHistories.GetOrLoad(order.CustomerId);
			var orderItems = ctx.OrderExportContext.OrderItems.GetOrLoad(order.Id);
			var shipments = ctx.OrderExportContext.Shipments.GetOrLoad(order.Id);

			dynamic dynObject = ToDynamic(ctx, order);

			dynObject.Customer = ToDynamic(ctx, customers.FirstOrDefault(x => x.Id == order.CustomerId));

			// We do not export all customer generic attributes because otherwise the export file gets too large.
			dynObject.Customer._GenericAttributes = genericAttributes
				.Where(x => x.Value.HasValue() && _orderCustomerAttributes.Contains(x.Key))
				.Select(x => ToDynamic(ctx, x))
				.ToList();

			dynObject.Customer.RewardPointsHistory = rewardPointsHistories
				.Select(x => ToDynamic(ctx, x))
				.ToList();

			if (rewardPointsHistories.Count > 0)
			{
				dynObject.Customer._RewardPointsBalance = rewardPointsHistories
					.OrderByDescending(x => x.CreatedOnUtc)
					.ThenByDescending(x => x.Id)
					.FirstOrDefault()
					.PointsBalance;
			}

			if (ctx.OrderExportContext.Addresses.ContainsKey(order.BillingAddressId))
			{
				dynObject.BillingAddress = ToDynamic(ctx, ctx.OrderExportContext.Addresses[order.BillingAddressId].FirstOrDefault());
			}

			if (order.ShippingAddressId.HasValue && ctx.OrderExportContext.Addresses.ContainsKey(order.ShippingAddressId.Value))
			{
				dynObject.ShippingAddress = ToDynamic(ctx, ctx.OrderExportContext.Addresses[order.ShippingAddressId.Value].FirstOrDefault());
			}

			dynObject.OrderItems = orderItems
				.Select(x => ToDynamic(ctx, x))
				.ToList();

			dynObject.Shipments = shipments
				.Select(x => ToDynamic(ctx, x))
				.ToList();

			result.Add(dynObject);

			_services.EventPublisher.Publish(new RowExportingEvent
			{
				Row = dynObject,
				EntityType = ExportEntityType.Order,
				ExportRequest = ctx.Request,
				ExecuteContext = ctx.ExecuteContext
			});

			return result;
		}

		private List<dynamic> Convert(DataExporterContext ctx, Manufacturer manufacturer)
		{
			var result = new List<dynamic>();

			var productManufacturers = ctx.ManufacturerExportContext.ProductManufacturers.GetOrLoad(manufacturer.Id);

			dynamic dynObject = ToDynamic(ctx, manufacturer);

			if (manufacturer.PictureId.HasValue)
			{
				var numberOfPictures = (ctx.Projection.NumberOfPictures ?? int.MaxValue);
				var pictures = ctx.ManufacturerExportContext.Pictures.GetOrLoad(manufacturer.PictureId.Value).Take(numberOfPictures);

				if (pictures.Any())
					dynObject.Picture = ToDynamic(ctx, pictures.First(), _mediaSettings.Value.ManufacturerThumbPictureSize, _mediaSettings.Value.ManufacturerThumbPictureSize);
			}

			dynObject.ProductManufacturers = productManufacturers
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic dyn = new DynamicEntity(x);

					return dyn;
				})
				.ToList();

			result.Add(dynObject);

			_services.EventPublisher.Publish(new RowExportingEvent
			{
				Row = dynObject,
				EntityType = ExportEntityType.Manufacturer,
				ExportRequest = ctx.Request,
				ExecuteContext = ctx.ExecuteContext
			});

			return result;
		}

		private List<dynamic> Convert(DataExporterContext ctx, Category category)
		{
			var result = new List<dynamic>();

			var productCategories = ctx.CategoryExportContext.ProductCategories.GetOrLoad(category.Id);

			dynamic dynObject = ToDynamic(ctx, category);

			if (category.PictureId.HasValue)
			{
				var numberOfPictures = (ctx.Projection.NumberOfPictures ?? int.MaxValue);
				var pictures = ctx.CategoryExportContext.Pictures.GetOrLoad(category.PictureId.Value).Take(numberOfPictures);

				if (pictures.Any())
					dynObject.Picture = ToDynamic(ctx, pictures.First(), _mediaSettings.Value.CategoryThumbPictureSize, _mediaSettings.Value.CategoryThumbPictureSize);
			}

			dynObject.ProductCategories = productCategories
				.OrderBy(x => x.DisplayOrder)
				.Select(x =>
				{
					dynamic dyn = new DynamicEntity(x);

					return dyn;
				})
				.ToList();

			result.Add(dynObject);

			_services.EventPublisher.Publish(new RowExportingEvent
			{
				Row = dynObject,
				EntityType = ExportEntityType.Category,
				ExportRequest = ctx.Request,
				ExecuteContext = ctx.ExecuteContext
			});

			return result;
		}

		private List<dynamic> Convert(DataExporterContext ctx, Customer customer)
		{
			var result = new List<dynamic>();

			var genericAttributes = ctx.CustomerExportContext.GenericAttributes.GetOrLoad(customer.Id);

			dynamic dynObject = ToDynamic(ctx, customer);

			dynObject.BillingAddress = ToDynamic(ctx, customer.BillingAddress);
			dynObject.ShippingAddress = ToDynamic(ctx, customer.ShippingAddress);

			dynObject.Addresses = customer.Addresses
				.Select(x => ToDynamic(ctx, x))
				.ToList();

			dynObject._GenericAttributes = genericAttributes
				.Select(x => ToDynamic(ctx, x))
				.ToList();

			dynObject._HasNewsletterSubscription = ctx.NewsletterSubscriptions.Contains(customer.Email, StringComparer.CurrentCultureIgnoreCase);

			var attrFirstName = genericAttributes.FirstOrDefault(x => x.Key == SystemCustomerAttributeNames.FirstName);
			var attrLastName = genericAttributes.FirstOrDefault(x => x.Key == SystemCustomerAttributeNames.LastName);

			string firstName = (attrFirstName == null ? "" : attrFirstName.Value);
			string lastName = (attrLastName == null ? "" : attrLastName.Value);

			if (firstName.IsEmpty() && lastName.IsEmpty())
			{
				var address = customer.Addresses.FirstOrDefault();
				if (address != null)
				{
					firstName = address.FirstName;
					lastName = address.LastName;
				}
			}

			dynObject._FullName = firstName.Grow(lastName, " ");

			if (_customerSettings.Value.AllowCustomersToUploadAvatars)
			{
				var pictureId = genericAttributes.FirstOrDefault(x => x.Key == SystemCustomerAttributeNames.AvatarPictureId);
				if (pictureId != null)
				{
					// reduce traffic and do not export default avatar
					dynObject._AvatarPictureUrl = _pictureService.Value.GetUrl(pictureId.Value.ToInt(), _mediaSettings.Value.AvatarPictureSize, false, _services.StoreService.GetHost(ctx.Store));
				}
			}

			result.Add(dynObject);

			_services.EventPublisher.Publish(new RowExportingEvent
			{
				Row = dynObject,
				EntityType = ExportEntityType.Customer,
				ExportRequest = ctx.Request,
				ExecuteContext = ctx.ExecuteContext
			});

			return result;
		}

		private List<dynamic> Convert(DataExporterContext ctx, NewsLetterSubscription subscription)
		{
			var result = new List<dynamic>();
			dynamic dynObject = ToDynamic(ctx, subscription);
			result.Add(dynObject);

			_services.EventPublisher.Publish(new RowExportingEvent
			{
				Row = dynObject,
				EntityType = ExportEntityType.NewsLetterSubscription,
				ExportRequest = ctx.Request,
				ExecuteContext = ctx.ExecuteContext
			});

			return result;
		}

		private List<dynamic> Convert(DataExporterContext ctx, ShoppingCartItem shoppingCartItem)
		{
			var result = new List<dynamic>();
			dynamic dynObject = ToDynamic(ctx, shoppingCartItem);

			result.Add(dynObject);

			_services.EventPublisher.Publish(new RowExportingEvent
			{
				Row = dynObject,
				EntityType = ExportEntityType.ShoppingCartItem,
				ExportRequest = ctx.Request,
				ExecuteContext = ctx.ExecuteContext
			});

			return result;
		}
	}


    internal class DynamicProductContext
    {
        public string SeName { get; set; }
        public string AbsoluteProductUrl { get; set; }
        public ICollection<ProductVariantAttributeCombination> Combinations { get; set; }
        public ProductVariantAttributeCombination Combination { get; set; }
    }
}
