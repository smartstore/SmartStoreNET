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
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Html;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Customers;
using SmartStore.Services.DataExchange.Export.Events;
using SmartStore.Services.DataExchange.Export.Internal;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Media.Imaging;
using SmartStore.Services.Seo;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Export
{
    public partial class DataExporter
    {
        private readonly string[] _orderCustomerAttributes = new string[]
        {
            SystemCustomerAttributeNames.VatNumber,
            SystemCustomerAttributeNames.ImpersonatedCustomerId
        };

        private void PrepareProductDescription(DataExporterContext ctx, dynamic dynObject, Product product)
        {
            try
            {
                var languageId = ctx.LanguageId;
                string description = "";

                // Description merging.
                if (ctx.Projection.DescriptionMerging == ExportDescriptionMerging.None)
                {
                    // Export empty description.
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
                    {
                        var translations = ctx.Translations[nameof(Manufacturer)];
                        var manufacturer = productManus.First().Manufacturer;
                        description = translations.GetValue(languageId, manufacturer.Id, nameof(manufacturer.Name)) ?? manufacturer.Name;
                    }

                    description = description.Grow((string)dynObject.Name, " ");
                    description = ctx.Projection.DescriptionMerging == ExportDescriptionMerging.ManufacturerAndNameAndShortDescription
                        ? description.Grow((string)dynObject.ShortDescription, " ")
                        : description.Grow((string)dynObject.FullDescription, " ");
                }

                // Append text.
                if (ctx.Projection.AppendDescriptionText.HasValue() && ((string)dynObject.ShortDescription).IsEmpty() && ((string)dynObject.FullDescription).IsEmpty())
                {
                    string[] appendText = ctx.Projection.AppendDescriptionText.SplitSafe(",");
                    if (appendText.Length > 0)
                    {
                        var rnd = CommonHelper.GenerateRandomInteger(0, appendText.Length - 1);
                        description = description.Grow(appendText.SafeGet(rnd), " ");
                    }
                }

                // Remove critical characters.
                if (description.HasValue() && ctx.Projection.RemoveCriticalCharacters)
                {
                    foreach (var str in ctx.Projection.CriticalCharacters.SplitSafe(","))
                    {
                        description = description.Replace(str, "");
                    }
                }

                // Convert to plain text.
                if (description.HasValue() && ctx.Projection.DescriptionToPlainText)
                {
                    //Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
                    //description = HttpUtility.HtmlDecode(reg.Replace(description, ""));

                    description = HtmlUtils.ConvertHtmlToPlainText(description);
                    description = HtmlUtils.StripTags(HttpUtility.HtmlDecode(description));
                }

                dynObject.FullDescription = description.TrimSafe();
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
            var productContext = ctx.ProductExportContext as PriceCalculationContext;
            var associatedProductContext = ctx.AssociatedProductContext as PriceCalculationContext;

            if (combination != null)
            {
                // price for attribute combination
                var attributesTotalPriceBase = decimal.Zero;

                if (attributeValues != null)
                {
                    attributeValues.Each(x => attributesTotalPriceBase += _priceCalculationService.Value.GetProductVariantAttributeValuePriceAdjustment(x, product, ctx.ContextCustomer, productContext));
                }

                price = _priceCalculationService.Value.GetFinalPrice(product, null, ctx.ContextCustomer, attributesTotalPriceBase, true, 1, null, productContext);
            }
            else if (ctx.Projection.PriceType.HasValue)
            {
                var priceType = ctx.Projection.PriceType.Value;

                if (product.ProductType == ProductType.GroupedProduct)
                {
                    var associatedProducts = productContext.AssociatedProducts.GetOrLoad(product.Id);
                    if (associatedProducts.Any())
                    {
                        var firstAssociatedProduct = associatedProducts.First();

                        if (priceType == PriceDisplayType.PreSelectedPrice)
                        {
                            price = _priceCalculationService.Value.GetPreselectedPrice(firstAssociatedProduct, ctx.ContextCustomer, ctx.ContextCurrency, associatedProductContext);
                        }
                        else if (priceType == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
                        {
                            price = _priceCalculationService.Value.GetFinalPrice(firstAssociatedProduct, null, ctx.ContextCustomer, decimal.Zero, false, 1, null, associatedProductContext);
                        }
                        else if (priceType == PriceDisplayType.LowestPrice)
                        {
                            price = _priceCalculationService.Value.GetLowestPrice(product, ctx.ContextCustomer, associatedProductContext, associatedProducts, out _) ?? decimal.Zero;
                        }
                    }
                }
                else
                {
                    if (priceType == PriceDisplayType.PreSelectedPrice)
                    {
                        price = _priceCalculationService.Value.GetPreselectedPrice(product, ctx.ContextCustomer, ctx.ContextCurrency, productContext);
                    }
                    else if (priceType == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
                    {
                        price = _priceCalculationService.Value.GetFinalPrice(product, null, ctx.ContextCustomer, decimal.Zero, false, 1, null, productContext);
                    }
                    else if (priceType == PriceDisplayType.LowestPrice)
                    {
                        price = _priceCalculationService.Value.GetLowestPrice(product, ctx.ContextCustomer, productContext, out _);
                    }
                }
            }

            return ConvertPrice(ctx, product, price) ?? price;
        }

        private List<dynamic> GetLocalized<T>(
            DataExporterContext ctx,
            LocalizedPropertyCollection translations,
            UrlRecordCollection urlRecords,
            T entity,
            params Expression<Func<T, string>>[] keySelectors)
            where T : BaseEntity, ILocalizedEntity
        {
            Guard.NotNull(translations, nameof(translations));

            if (ctx.Languages.Count <= 1)
            {
                return null;
            }

            var localized = new List<dynamic>();
            var localeKeyGroup = entity.GetEntityName();
            //var isSlugSupported = typeof(ISlugSupported).IsAssignableFrom(typeof(T));

            foreach (var language in ctx.Languages)
            {
                var languageCulture = language.Value.LanguageCulture.EmptyNull().ToLower();

                // Add SEO name.
                if (urlRecords != null)
                {
                    var value = urlRecords.GetSlug(language.Value.Id, entity.Id, false);
                    if (value.HasValue())
                    {
                        dynamic exp = new HybridExpando();
                        exp.Culture = languageCulture;
                        exp.LocaleKey = "SeName";
                        exp.LocaleValue = value;

                        localized.Add(exp);
                    }
                }

                // Add localized property value.
                foreach (var keySelector in keySelectors)
                {
                    var member = keySelector.Body as MemberExpression;
                    var propInfo = member.Member as PropertyInfo;
                    string localeKey = propInfo.Name;
                    var value = translations.GetValue(language.Value.Id, entity.Id, localeKey);

                    // We do not export empty values to not fill databases with it.
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

            return localized.Any() ? localized : null;
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
            {
                return null;
            }

            dynamic result = new DynamicEntity(currency);
            var translations = ctx.Translations[nameof(Currency)];

            result.Name = translations.GetValue(ctx.LanguageId, currency.Id, nameof(currency.Name)) ?? currency.Name;
            result._Localized = GetLocalized(ctx, translations, null, currency, x => x.Name);

            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, Language language)
        {
            if (language == null)
            {
                return null;
            }

            dynamic result = new DynamicEntity(language);
            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, Country country)
        {
            if (country == null)
            {
                return null;
            }

            dynamic result = new DynamicEntity(country);
            var translations = ctx.Translations[nameof(Country)];

            result.Name = translations.GetValue(ctx.LanguageId, country.Id, nameof(country.Name)) ?? country.Name;
            result._Localized = GetLocalized(ctx, translations, null, country, x => x.Name);

            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, Address address)
        {
            if (address == null)
            {
                return null;
            }

            dynamic result = new DynamicEntity(address);

            result.Country = ToDynamic(ctx, address.Country);

            if (address.StateProvinceId.GetValueOrDefault() > 0)
            {
                dynamic sp = new DynamicEntity(address.StateProvince);
                var translations = ctx.Translations[nameof(StateProvince)];

                sp.Name = translations.GetValue(ctx.LanguageId, address.StateProvince.Id, nameof(StateProvince.Name)) ?? address.StateProvince.Name;
                sp._Localized = GetLocalized(ctx, translations, null, address.StateProvince, x => x.Name);

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
            {
                return null;
            }

            dynamic result = new DynamicEntity(points);
            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, Customer customer)
        {
            if (customer == null)
            {
                return null;
            }

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

            result.CustomerRoles = customer.CustomerRoleMappings
                .Select(x =>
                {
                    dynamic dyn = new DynamicEntity(x.CustomerRole);
                    return dyn;
                })
                .ToList();

            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, Store store)
        {
            if (store == null)
            {
                return null;
            }

            dynamic result = new DynamicEntity(store);

            result.PrimaryStoreCurrency = ToDynamic(ctx, store.PrimaryStoreCurrency);
            result.PrimaryExchangeRateCurrency = ToDynamic(ctx, store.PrimaryExchangeRateCurrency);

            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, DeliveryTime deliveryTime)
        {
            if (deliveryTime == null)
            {
                return null;
            }

            dynamic result = new DynamicEntity(deliveryTime);
            var translations = ctx.Translations[nameof(DeliveryTime)];

            result.Name = translations.GetValue(ctx.LanguageId, deliveryTime.Id, nameof(deliveryTime.Name)) ?? deliveryTime.Name;
            result._Localized = GetLocalized(ctx, translations, null, deliveryTime, x => x.Name);

            return result;
        }

        private void ToDeliveryTime(DataExporterContext ctx, dynamic parent, int? deliveryTimeId)
        {
            if (ctx.DeliveryTimes != null)
            {
                parent.DeliveryTime = deliveryTimeId.HasValue && ctx.DeliveryTimes.ContainsKey(deliveryTimeId.Value)
                    ? ToDynamic(ctx, ctx.DeliveryTimes[deliveryTimeId.Value])
                    : null;
            }
        }

        private dynamic ToDynamic(DataExporterContext ctx, QuantityUnit quantityUnit)
        {
            if (quantityUnit == null)
            {
                return null;
            }

            dynamic result = new DynamicEntity(quantityUnit);
            var translations = ctx.Translations[nameof(QuantityUnit)];

            result.Name = translations.GetValue(ctx.LanguageId, quantityUnit.Id, nameof(quantityUnit.Name)) ?? quantityUnit.Name;
            result.NamePlural = translations.GetValue(ctx.LanguageId, quantityUnit.Id, nameof(quantityUnit.NamePlural)) ?? quantityUnit.NamePlural;
            result.Description = translations.GetValue(ctx.LanguageId, quantityUnit.Id, nameof(quantityUnit.Description)) ?? quantityUnit.Description;

            result._Localized = GetLocalized(ctx, translations, null, quantityUnit,
                x => x.Name,
                x => x.NamePlural,
                x => x.Description);

            return result;
        }

        private void ToQuantityUnit(DataExporterContext ctx, dynamic parent, int? quantityUnitId)
        {
            if (ctx.QuantityUnits != null)
            {
                parent.QuantityUnit = quantityUnitId.HasValue && ctx.QuantityUnits.ContainsKey(quantityUnitId.Value)
                    ? ToDynamic(ctx, ctx.QuantityUnits[quantityUnitId.Value])
                    : null;
            }
        }

        private dynamic ToDynamic(DataExporterContext ctx, MediaFile file, int thumbPictureSize, int detailsPictureSize)
        {
            return ToDynamic(ctx, _mediaService.Value.ConvertMediaFile(file), thumbPictureSize, detailsPictureSize);
        }

        private dynamic ToDynamic(DataExporterContext ctx, MediaFileInfo file, int thumbPictureSize, int detailsPictureSize)
        {
            if (file == null)
            {
                return null;
            }

            try
            {
                var host = _services.StoreService.GetHost(ctx.Store);

                dynamic result = new DynamicEntity(file.File);

                result._FileName = file.Name;
                result._RelativeUrl = _mediaService.Value.GetUrl(file, null, null);
                result._ThumbImageUrl = _mediaService.Value.GetUrl(file, thumbPictureSize, host);
                result._ImageUrl = _mediaService.Value.GetUrl(file, detailsPictureSize, host);
                result._FullSizeImageUrl = _mediaService.Value.GetUrl(file, null, host);

                return result;
            }
            catch (Exception ex)
            {
                ctx.Log.ErrorFormat(ex, $"Failed to get details for file with ID {file.File.Id}.");
                return null;
            }
        }

        private dynamic ToDynamic(DataExporterContext ctx, ProductVariantAttribute pva)
        {
            if (pva == null)
            {
                return null;
            }

            var languageId = ctx.LanguageId;
            var attribute = pva.ProductAttribute;

            dynamic result = new DynamicEntity(pva);
            dynamic dynAttribute = new DynamicEntity(attribute);
            var paTranslations = ctx.TranslationsPerPage[nameof(ProductAttribute)];
            var pvavTranslations = ctx.TranslationsPerPage[nameof(ProductVariantAttributeValue)];

            dynAttribute.Name = paTranslations.GetValue(languageId, attribute.Id, nameof(attribute.Name)) ?? attribute.Name;
            dynAttribute.Description = paTranslations.GetValue(languageId, attribute.Id, nameof(attribute.Description)) ?? attribute.Description;

            dynAttribute.Values = pva.ProductVariantAttributeValues
                .OrderBy(x => x.DisplayOrder)
                .Select(x =>
                {
                    dynamic dyn = new DynamicEntity(x);

                    dyn.Name = pvavTranslations.GetValue(languageId, x.Id, nameof(x.Name)) ?? x.Name;
                    dyn._Localized = GetLocalized(ctx, pvavTranslations, null, x, y => y.Name);

                    return dyn;
                })
                .ToList();

            dynAttribute._Localized = GetLocalized(ctx, paTranslations, null, attribute,
                x => x.Name,
                x => x.Description);

            result.Attribute = dynAttribute;

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
            {
                return null;
            }

            dynamic result = new DynamicEntity(manufacturer);
            var translations = ctx.Translations[nameof(Manufacturer)];
            var urlRecords = ctx.UrlRecords[nameof(Manufacturer)];

            result.Picture = null;
            result.Name = translations.GetValue(ctx.LanguageId, manufacturer.Id, nameof(manufacturer.Name)) ?? manufacturer.Name;

            if (!ctx.IsPreview)
            {
                result.SeName = ctx.UrlRecords[nameof(Manufacturer)].GetSlug(ctx.LanguageId, manufacturer.Id);
                result.Description = translations.GetValue(ctx.LanguageId, manufacturer.Id, nameof(manufacturer.Description)) ?? manufacturer.Description;
                result.BottomDescription = translations.GetValue(ctx.LanguageId, manufacturer.Id, nameof(manufacturer.BottomDescription)) ?? manufacturer.BottomDescription;
                result.MetaKeywords = translations.GetValue(ctx.LanguageId, manufacturer.Id, nameof(manufacturer.MetaKeywords)) ?? manufacturer.MetaKeywords;
                result.MetaDescription = translations.GetValue(ctx.LanguageId, manufacturer.Id, nameof(manufacturer.MetaDescription)) ?? manufacturer.MetaDescription;
                result.MetaTitle = translations.GetValue(ctx.LanguageId, manufacturer.Id, nameof(manufacturer.MetaTitle)) ?? manufacturer.MetaTitle;

                result._Localized = GetLocalized(ctx, translations, urlRecords, manufacturer,
                    x => x.Name,
                    x => x.Description,
                    x => x.BottomDescription,
                    x => x.MetaKeywords,
                    x => x.MetaDescription,
                    x => x.MetaTitle);
            }

            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, Category category)
        {
            if (category == null)
            {
                return null;
            }

            dynamic result = new DynamicEntity(category);
            var translations = ctx.Translations[nameof(Category)];
            var urlRecords = ctx.UrlRecords[nameof(Category)];

            result.Picture = null;
            result.Name = translations.GetValue(ctx.LanguageId, category.Id, nameof(category.Name)) ?? category.Name;
            result.FullName = translations.GetValue(ctx.LanguageId, category.Id, nameof(category.FullName)) ?? category.FullName;

            if (!ctx.IsPreview)
            {
                result.SeName = ctx.UrlRecords[nameof(Category)].GetSlug(ctx.LanguageId, category.Id);
                result.Description = translations.GetValue(ctx.LanguageId, category.Id, nameof(category.Description)) ?? category.Description;
                result.BottomDescription = translations.GetValue(ctx.LanguageId, category.Id, nameof(category.BottomDescription)) ?? category.BottomDescription;
                result.MetaKeywords = translations.GetValue(ctx.LanguageId, category.Id, nameof(category.MetaKeywords)) ?? category.MetaKeywords;
                result.MetaDescription = translations.GetValue(ctx.LanguageId, category.Id, nameof(category.MetaDescription)) ?? category.MetaDescription;
                result.MetaTitle = translations.GetValue(ctx.LanguageId, category.Id, nameof(category.MetaTitle)) ?? category.MetaTitle;

                result._CategoryTemplateViewPath = ctx.CategoryTemplates.ContainsKey(category.CategoryTemplateId)
                    ? ctx.CategoryTemplates[category.CategoryTemplateId]
                    : "";

                result._Localized = GetLocalized(ctx, translations, urlRecords, category,
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
            {
                return null;
            }

            dynamic result = new DynamicEntity(product);
            var translations = ctx.TranslationsPerPage[nameof(Product)];
            var urlRecords = ctx.UrlRecordsPerPage[nameof(Product)];
            var localizedName = translations.GetValue(ctx.LanguageId, product.Id, nameof(product.Name)) ?? product.Name;

            result.AppliedDiscounts = null;
            result.Downloads = null;
            result.TierPrices = null;
            result.ProductAttributes = null;
            result.ProductAttributeCombinations = null;
            result.ProductPictures = null;
            result.ProductCategories = null;
            result.ProductManufacturers = null;
            result.ProductTags = null;
            result.ProductSpecificationAttributes = null;
            result.ProductBundleItems = null;
            result.Name = localizedName;

            if (!ctx.IsPreview)
            {
                result.SeName = seName ?? ctx.UrlRecordsPerPage[nameof(Product)].GetSlug(ctx.LanguageId, product.Id);
                result.ShortDescription = translations.GetValue(ctx.LanguageId, product.Id, nameof(product.ShortDescription)) ?? product.ShortDescription;
                result.FullDescription = translations.GetValue(ctx.LanguageId, product.Id, nameof(product.FullDescription)) ?? product.FullDescription;
                result.MetaKeywords = translations.GetValue(ctx.LanguageId, product.Id, nameof(product.MetaKeywords)) ?? product.MetaKeywords;
                result.MetaDescription = translations.GetValue(ctx.LanguageId, product.Id, nameof(product.MetaDescription)) ?? product.MetaDescription;
                result.MetaTitle = translations.GetValue(ctx.LanguageId, product.Id, nameof(product.MetaTitle)) ?? product.MetaTitle;
                result.BundleTitleText = translations.GetValue(ctx.LanguageId, product.Id, nameof(product.BundleTitleText)) ?? product.BundleTitleText;

                result._ProductTemplateViewPath = ctx.ProductTemplates.ContainsKey(product.ProductTemplateId)
                    ? ctx.ProductTemplates[product.ProductTemplateId]
                    : string.Empty;

                // A standard, language independent base price formatting: <formattedPrice> / <basePriceBaseAmount> <basePriceMeasureUnit>
                var basePriceInfo = string.Empty;

                if (product.BasePriceHasValue && product.BasePriceAmount != decimal.Zero)
                {
                    var finalPrice = _priceCalculationService.Value.GetFinalPrice(product, ctx.ContextCustomer, true);
                    var productPrice = _taxService.Value.GetProductPrice(product, finalPrice, ctx.ContextCustomer, ctx.ContextCurrency, out var taxrate);
                    productPrice = _currencyService.Value.ConvertFromPrimaryStoreCurrency(productPrice, ctx.ContextCurrency);

                    var priceIncludesTax = _services.WorkContext.TaxDisplayType == TaxDisplayType.IncludingTax;
                    var priceTemplate = _services.Localization.GetResource("Products.BasePriceInfo.LanguageInsensitive");
                    var formattedPriceBase = System.Convert.ToDecimal((productPrice / product.BasePriceAmount) * product.BasePriceBaseAmount);
                    var formattedPrice = _priceFormatter.Value.FormatPrice(formattedPriceBase, true, ctx.ContextCurrency, ctx.ContextLanguage, priceIncludesTax, false);

                    basePriceInfo = priceTemplate.FormatInvariant(formattedPrice, product.BasePriceBaseAmount, product.BasePriceMeasureUnit);
                }

                result._BasePriceInfo = basePriceInfo;

                ToDeliveryTime(ctx, result, product.DeliveryTimeId);
                ToQuantityUnit(ctx, result, product.QuantityUnitId);

                result.CountryOfOrigin = product.CountryOfOriginId.HasValue
                    ? ToDynamic(ctx, product.CountryOfOrigin)
                    : null;

                result._Localized = GetLocalized(ctx, translations, urlRecords, product,
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

            var numberOfPictures = ctx.Projection.NumberOfMediaFiles ?? int.MaxValue;
            var productDetailsPictureSize = _mediaSettings.Value.ProductDetailsPictureSize;
            ProcessImageQuery imageQuery = null;

            if (ctx.Projection.PictureSize > 0)
            {
                productDetailsPictureSize = _mediaSettings.Value.GetNextValidThumbnailSize(ctx.Projection.PictureSize);
                imageQuery = new ProcessImageQuery { MaxWidth = productDetailsPictureSize };
            }

            IEnumerable<ProductMediaFile> productPictures = ctx.ProductExportContext.ProductPictures.GetOrLoad(product.Id);
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
                var mediaIds = productContext.Combination.GetAssignedMediaIds();
                if (mediaIds.Any())
                {
                    productPictures = productPictures.Where(x => mediaIds.Contains(x.MediaFileId));
                }

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
                    var translations = ctx.TranslationsPerPage[nameof(ProductVariantAttributeValue)];
                    var valueNames = variantAttributeValues
                        .Select(x => translations.GetValue(ctx.LanguageId, x.Id, nameof(x.Name)) ?? x.Name)
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

                    dyn.Picture = ToDynamic(ctx, x.MediaFile, _mediaSettings.Value.ProductThumbPictureSize, productDetailsPictureSize);

                    return dyn;
                })
                .ToList();

            dynObject.ProductManufacturers = productManufacturers
                .OrderBy(x => x.DisplayOrder)
                .Select(x =>
                {
                    dynamic dyn = new DynamicEntity(x);

                    dyn.Manufacturer = ToDynamic(ctx, x.Manufacturer);

                    dyn.Manufacturer.Picture = x.Manufacturer != null && x.Manufacturer.MediaFileId.HasValue
                        ? ToDynamic(ctx, x.Manufacturer.MediaFile, _mediaSettings.Value.ManufacturerThumbPictureSize, _mediaSettings.Value.ManufacturerThumbPictureSize)
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

                    if (x.Category != null && x.Category.MediaFileId.HasValue)
                        dyn.Category.Picture = ToDynamic(ctx, x.Category.MediaFile, _mediaSettings.Value.CategoryThumbPictureSize, _mediaSettings.Value.CategoryThumbPictureSize);

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

                        foreach (int pictureId in x.GetAssignedMediaIds().Take(numberOfPictures))
                        {
                            var assignedPicture = productPictures.FirstOrDefault(y => y.MediaFileId == pictureId);
                            if (assignedPicture != null && assignedPicture.MediaFile != null)
                            {
                                assignedPictures.Add(ToDynamic(ctx, assignedPicture.MediaFile, _mediaSettings.Value.ProductThumbPictureSize, productDetailsPictureSize));
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

            if (product.IsDownload)
            {
                var downloads = ctx.ProductExportContext.Downloads.GetOrLoad(product.Id);

                dynObject.Downloads = downloads
                    .Select(x => ToDynamic(ctx, x))
                    .ToList();
            }

            dynObject.ProductTags = productTags
                .Select(x =>
                {
                    dynamic dyn = new DynamicEntity(x);
                    var translations = ctx.TranslationsPerPage[nameof(ProductTag)];
                    var localizedName = translations.GetValue(ctx.LanguageId, x.Id, nameof(x.Name)) ?? x.Name;

                    dyn.Name = localizedName;
                    dyn.SeName = SeoExtensions.GetSeName(localizedName, _seoSettings.Value);
                    dyn._Localized = GetLocalized(ctx, translations, null, x, y => y.Name);

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
                        var translations = ctx.TranslationsPerPage[nameof(ProductBundleItem)];

                        dyn.Name = translations.GetValue(ctx.LanguageId, x.Id, nameof(x.Name)) ?? x.Name;
                        dyn.ShortDescription = translations.GetValue(ctx.LanguageId, x.Id, nameof(x.ShortDescription)) ?? x.ShortDescription;
                        dyn._Localized = GetLocalized(ctx, translations, null, x, y => y.Name, y => y.ShortDescription);

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
                {
                    var translations = ctx.Translations[nameof(Manufacturer)];
                    var manufacturer = productManus.First().Manufacturer;
                    brand = translations.GetValue(ctx.LanguageId, manufacturer.Id, nameof(manufacturer.Name)) ?? manufacturer.Name;
                }
                if (brand.IsEmpty())
                {
                    brand = ctx.Projection.Brand;
                }

                dynObject._Brand = brand;
            }

            if (ctx.Supports(ExportFeatures.CanIncludeMainPicture))
            {
                if (productPictures != null && productPictures.Any())
                {
                    var file = _mediaService.Value.ConvertMediaFile(productPictures.Select(x => x.MediaFile).First());

                    dynObject._MainPictureUrl = _mediaService.Value.GetUrl(file, imageQuery, _services.StoreService.GetHost(ctx.Store));
                    dynObject._MainPictureRelativeUrl = _mediaService.Value.GetUrl(file, imageQuery);
                }
                else if (!_catalogSettings.Value.HideProductDefaultPictures)
                {
                    // Get fallback image URL.
                    dynObject._MainPictureUrl = _mediaService.Value.GetUrl(null, imageQuery, _services.StoreService.GetHost(ctx.Store));
                    dynObject._MainPictureRelativeUrl = _mediaService.Value.GetUrl(null, imageQuery);
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
                dynObject._ShippingTime = deliveryTime == null ? ctx.Projection.ShippingTime : deliveryTime.Name;
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
                dynObject._SpecialPrice = null;         // special price which is valid now
                dynObject._FutureSpecialPrice = null;   // special price which is valid now and in future
                dynObject._RegularPrice = null;         // price as if a special price would not exist

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
            {
                return null;
            }

            dynamic result = new DynamicEntity(order);

            result.OrderNumber = order.GetOrderNumber();
            result.OrderStatus = order.OrderStatus.GetLocalizedEnum(_services.Localization, ctx.LanguageId);
            result.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_services.Localization, ctx.LanguageId);
            result.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_services.Localization, ctx.LanguageId);

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
            {
                return null;
            }

            dynamic result = new DynamicEntity(orderItem);

            orderItem.Product.MergeWithCombination(orderItem.AttributesXml, _productAttributeParser.Value);
            result.Product = ToDynamic(ctx, orderItem.Product);

            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, Shipment shipment)
        {
            if (shipment == null)
            {
                return null;
            }

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
            {
                return null;
            }

            dynamic result = new DynamicEntity(discount);
            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, Download download)
        {
            if (download == null)
            {
                return null;
            }

            dynamic result = new DynamicEntity(download);
            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, ProductSpecificationAttribute psa)
        {
            if (psa == null)
            {
                return null;
            }

            var option = psa.SpecificationAttributeOption;
            var attribute = option.SpecificationAttribute;

            dynamic result = new DynamicEntity(psa);
            dynamic dynAttribute = new DynamicEntity(attribute);
            var saTranslations = ctx.TranslationsPerPage[nameof(SpecificationAttribute)];
            var saoTranslations = ctx.TranslationsPerPage[nameof(SpecificationAttributeOption)];

            dynAttribute.Name = saTranslations.GetValue(ctx.LanguageId, attribute.Id, nameof(attribute.Name)) ?? attribute.Name;
            dynAttribute._Localized = GetLocalized(ctx, saTranslations, null, attribute, x => x.Name);

            dynAttribute.Alias = saTranslations.GetValue(ctx.LanguageId, attribute.Id, nameof(attribute.Alias)) ?? attribute.Alias;
            dynAttribute._Localized = GetLocalized(ctx, saTranslations, null, attribute, x => x.Alias);

            dynamic dynOption = new DynamicEntity(option);

            dynOption.Name = saoTranslations.GetValue(ctx.LanguageId, option.Id, nameof(option.Name)) ?? option.Name;
            dynOption._Localized = GetLocalized(ctx, saoTranslations, null, option, x => x.Name);

            dynOption.Alias = saoTranslations.GetValue(ctx.LanguageId, option.Id, nameof(option.Alias)) ?? option.Alias;
            dynOption._Localized = GetLocalized(ctx, saoTranslations, null, option, x => x.Alias);

            dynOption.SpecificationAttribute = dynAttribute;
            result.SpecificationAttributeOption = dynOption;

            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, GenericAttribute genericAttribute)
        {
            if (genericAttribute == null)
            {
                return null;
            }

            dynamic result = new DynamicEntity(genericAttribute);
            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, NewsLetterSubscription subscription)
        {
            if (subscription == null)
            {
                return null;
            }

            dynamic result = new DynamicEntity(subscription);

            result.Store = ctx.Stores.ContainsKey(subscription.StoreId)
                ? ToDynamic(ctx, ctx.Stores[subscription.StoreId])
                : null;

            return result;
        }

        private dynamic ToDynamic(DataExporterContext ctx, ShoppingCartItem shoppingCartItem)
        {
            if (shoppingCartItem == null)
            {
                return null;
            }

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

            productContext.SeName = ctx.UrlRecordsPerPage[nameof(Product)].GetSlug(ctx.LanguageId, product.Id);
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

            if (manufacturer.MediaFileId.HasValue)
            {
                var numberOfFiles = ctx.Projection.NumberOfMediaFiles ?? int.MaxValue;
                var files = ctx.ManufacturerExportContext.Files.GetOrLoad(manufacturer.MediaFileId.Value).Take(numberOfFiles);

                if (files.Any())
                {
                    dynObject.Picture = ToDynamic(ctx, files.First(), _mediaSettings.Value.ManufacturerThumbPictureSize, _mediaSettings.Value.ManufacturerThumbPictureSize);
                }
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

            if (category.MediaFileId.HasValue)
            {
                var numberOfFiles = ctx.Projection.NumberOfMediaFiles ?? int.MaxValue;
                var files = ctx.CategoryExportContext.Files.GetOrLoad(category.MediaFileId.Value).Take(numberOfFiles);

                if (files.Any())
                {
                    dynObject.Picture = ToDynamic(ctx, files.First(), _mediaSettings.Value.CategoryThumbPictureSize, _mediaSettings.Value.CategoryThumbPictureSize);
                }
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
            dynObject._FullName = customer.GetFullName();
            dynObject._AvatarPictureUrl = null;

            if (_customerSettings.Value.AllowCustomersToUploadAvatars)
            {
                // Reduce traffic and do not export default avatar.
                var fileId = genericAttributes.FirstOrDefault(x => x.Key == SystemCustomerAttributeNames.AvatarPictureId)?.Value?.ToInt() ?? 0;
                var file = _mediaService.Value.GetFileById(fileId, MediaLoadFlags.AsNoTracking);
                if (file != null)
                {
                    dynObject._AvatarPictureUrl = _mediaService.Value.GetUrl(file, new ProcessImageQuery { MaxSize = _mediaSettings.Value.AvatarPictureSize }, _services.StoreService.GetHost(ctx.Store));
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
