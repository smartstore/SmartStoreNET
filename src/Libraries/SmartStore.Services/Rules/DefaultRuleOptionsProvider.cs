using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Plugins;
using SmartStore.Core.Search;
using SmartStore.Rules;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Search;
using SmartStore.Services.Shipping;

namespace SmartStore.Services.Rules
{
    public class DefaultRuleOptionsProvider : IRuleOptionsProvider
    {
        protected readonly ICommonServices _services;
        protected readonly Lazy<IRepository<SpecificationAttributeOption>> _attrOptionRepository;
        protected readonly Lazy<IRepository<ProductVariantAttributeValue>> _variantValueRepository;
        protected readonly Lazy<ICurrencyService> _currencyService;
        protected readonly Lazy<ICustomerService> _customerService;
        protected readonly Lazy<ILanguageService> _languageService;
        protected readonly Lazy<ICountryService> _countryService;
        protected readonly Lazy<IDeliveryTimeService> _deliveryTimeService;
        protected readonly Lazy<ICatalogSearchService> _catalogSearchService;
        protected readonly Lazy<IProductService> _productService;
        protected readonly Lazy<IProductTagService> _productTagService;
        protected readonly Lazy<ICategoryService> _categoryService;
        protected readonly Lazy<IManufacturerService> _manufacturerService;
        protected readonly Lazy<IShippingService> _shippingService;
        protected readonly Lazy<IRuleStorage> _ruleStorage;
        protected readonly Lazy<IProviderManager> _providerManager;
        protected readonly Lazy<SearchSettings> _searchSettings;

        public DefaultRuleOptionsProvider(
            ICommonServices services,
            Lazy<IRepository<SpecificationAttributeOption>> attrOptionRepository,
            Lazy<IRepository<ProductVariantAttributeValue>> variantValueRepository,
            Lazy<ICurrencyService> currencyService,
            Lazy<ICustomerService> customerService,
            Lazy<ILanguageService> languageService,
            Lazy<ICountryService> countryService,
            Lazy<IDeliveryTimeService> deliveryTimeService,
            Lazy<ICatalogSearchService> catalogSearchService,
            Lazy<IProductService> productService,
            Lazy<IProductTagService> productTagService,
            Lazy<ICategoryService> categoryService,
            Lazy<IManufacturerService> manufacturerService,
            Lazy<IShippingService> shippingService,
            Lazy<IRuleStorage> ruleStorage,
            Lazy<IProviderManager> providerManager,
            Lazy<SearchSettings> searchSettings)
        {
            _services = services;
            _attrOptionRepository = attrOptionRepository;
            _variantValueRepository = variantValueRepository;
            _currencyService = currencyService;
            _customerService = customerService;
            _languageService = languageService;
            _countryService = countryService;
            _deliveryTimeService = deliveryTimeService;
            _catalogSearchService = catalogSearchService;
            _productService = productService;
            _productTagService = productTagService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _shippingService = shippingService;
            _ruleStorage = ruleStorage;
            _providerManager = providerManager;
            _searchSettings = searchSettings;
        }

        public virtual bool Matches(string dataSource)
        {
            switch (dataSource.EmptyNull())
            {
                case "CartRule":
                case "Category":
                case "Country":
                case "Currency":
                case "DeliveryTime":
                case "CustomerRole":
                case "Language":
                case "Manufacturer":
                case "PaymentMethod":
                case "Product":
                case "ProductTag":
                case "ShippingMethod":
                case "ShippingRateComputationMethod":
                case "TargetGroup":
                case "VariantValue":
                case "AttributeOption":
                    return true;
                default:
                    return false;
            }
        }

        public virtual RuleOptionsResult GetOptions(
            RuleOptionsRequestReason reason,
            IRuleExpression expression,
            int pageIndex,
            int pageSize,
            string searchTerm)
        {
            Guard.NotNull(expression, nameof(expression));

            return GetOptions(reason, expression.Descriptor, expression.RawValue, pageIndex, pageSize, searchTerm);
        }

        public virtual RuleOptionsResult GetOptions(
            RuleOptionsRequestReason reason,
            RuleDescriptor descriptor,
            string value,
            int pageIndex,
            int pageSize,
            string searchTerm)
        {
            Guard.NotNull(descriptor, nameof(descriptor));

            var result = new RuleOptionsResult();

            if (!(descriptor.SelectList is RemoteRuleValueSelectList list))
            {
                return result;
            }

            var language = _services.WorkContext.WorkingLanguage;
            var byId = descriptor.RuleType == RuleType.Int || descriptor.RuleType == RuleType.IntArray;
            List<RuleValueSelectListOption> options = null;

            switch (list.DataSource)
            {
                case "Product":
                    if (reason == RuleOptionsRequestReason.SelectedDisplayNames)
                    {
                        options = _productService.Value.GetProductsByIds(value.ToIntArray())
                            .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.GetLocalized(y => y.Name, language, true, false), Hint = x.Sku })
                            .ToList();
                    }
                    else
                    {
                        result.IsPaged = true;
                        options = SearchProducts(result, searchTerm, pageIndex * pageSize, pageSize);
                    }
                    break;
                case "Country":
                    options = _countryService.Value.GetAllCountries(true)
                        .Select(x => new RuleValueSelectListOption { Value = byId ? x.Id.ToString() : x.TwoLetterIsoCode, Text = x.GetLocalized(y => y.Name, language, true, false) })
                        .ToList();
                    break;
                case "Currency":
                    options = _currencyService.Value.GetAllCurrencies(true)
                        .Select(x => new RuleValueSelectListOption { Value = byId ? x.Id.ToString() : x.CurrencyCode, Text = x.GetLocalized(y => y.Name, language, true, false) })
                        .ToList();
                    break;
                case "DeliveryTime":
                    options = _deliveryTimeService.Value.GetAllDeliveryTimes()
                        .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.GetLocalized(y => y.Name, language, true, false) })
                        .ToList();
                    break;
                case "CustomerRole":
                    options = _customerService.Value.GetAllCustomerRoles(true)
                        .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.Name })
                        .ToList();
                    break;
                case "Language":
                    options = _languageService.Value.GetAllLanguages(true)
                        .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = GetCultureDisplayName(x) ?? x.Name })
                        .ToList();
                    break;
                case "Store":
                    options = _services.StoreService.GetAllStores()
                        .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.Name })
                        .ToList();
                    break;
                case "CartRule":
                case "TargetGroup":
                    if (reason == RuleOptionsRequestReason.SelectedDisplayNames)
                    {
                        options = _ruleStorage.Value.GetRuleSetsByIds(value.ToIntArray(), false)
                            .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.Name })
                            .ToList();
                    }
                    else
                    {
                        var ruleSets = _ruleStorage.Value.GetAllRuleSets(false, false, descriptor.Scope, pageIndex, pageSize, false, true);
                        result.IsPaged = true;
                        result.HasMoreData = ruleSets.HasNextPage;

                        options = ruleSets
                            .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.Name })
                            .ToList();
                    }
                    break;
                case "Category":
                    if (reason == RuleOptionsRequestReason.SelectedDisplayNames)
                    {
                        options = _categoryService.Value.GetCategoriesByIds(value.ToIntArray())
                            .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.GetCategoryPath(_categoryService.Value).NullEmpty() ?? x.Name })
                            .ToList();
                    }
                    else
                    {
                        var categories = _categoryService.Value.GetCategoryTree(0, true).Flatten(false);
                        options = categories
                            .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.GetCategoryPath(_categoryService.Value).NullEmpty() ?? x.Name })
                            .ToList();
                    }
                    break;
                case "Manufacturer":
                    options = _manufacturerService.Value.GetAllManufacturers(true)
                        .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.GetLocalized(y => y.Name, language, true, false) })
                        .ToList();
                    break;
                case "PaymentMethod":
                    options = _providerManager.Value.GetAllProviders<IPaymentMethod>()
                        .Select(x => x.Metadata)
                        .Select(x => new RuleValueSelectListOption { Value = x.SystemName, Text = GetLocalized(x, "FriendlyName") ?? x.FriendlyName.NullEmpty() ?? x.SystemName, Hint = x.SystemName })
                        .OrderBy(x => x.Text)
                        .ToList();
                    break;
                case "ShippingRateComputationMethod":
                    options = _providerManager.Value.GetAllProviders<IShippingRateComputationMethod>()
                        .Select(x => x.Metadata)
                        .Select(x => new RuleValueSelectListOption { Value = x.SystemName, Text = GetLocalized(x, "FriendlyName") ?? x.FriendlyName.NullEmpty() ?? x.SystemName, Hint = x.SystemName })
                        .OrderBy(x => x.Text)
                        .ToList();
                    break;
                case "ShippingMethod":
                    options = _shippingService.Value.GetAllShippingMethods()
                        .Select(x => new RuleValueSelectListOption { Value = byId ? x.Id.ToString() : x.Name, Text = byId ? x.GetLocalized(y => y.Name, language, true, false) : x.Name })
                        .ToList();
                    break;
                case "ProductTag":
                    options = _productTagService.Value.GetAllProductTags(true)
                        .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.GetLocalized(y => y.Name, language, true, false) })
                        .OrderBy(x => x.Text)
                        .ToList();
                    break;
                case "VariantValue":
                    if (reason == RuleOptionsRequestReason.SelectedDisplayNames)
                    {
                        var ids = value.ToIntArray();
                        var variantValues = _variantValueRepository.Value.TableUntracked
                            .Where(x => ids.Contains(x.Id))
                            .ToList();

                        options = variantValues
                            .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.GetLocalized(y => y.Name, language, true, false) })
                            .ToList();
                    }
                    else if (descriptor.Metadata.TryGetValue("ParentId", out var objParentId))
                    {
                        options = new List<RuleValueSelectListOption>();
                        var pIndex = -1;
                        var existingValues = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
                        var multiValueTypeIds = new int[] { (int)AttributeControlType.Checkboxes, (int)AttributeControlType.RadioList, (int)AttributeControlType.DropdownList, (int)AttributeControlType.Boxes };
                        var query = _variantValueRepository.Value.TableUntracked
                            .Where(x =>
                                x.ProductVariantAttribute.ProductAttributeId == (int)objParentId &&
                                x.ProductVariantAttribute.ProductAttribute.AllowFiltering &&
                                multiValueTypeIds.Contains(x.ProductVariantAttribute.AttributeControlTypeId) &&
                                x.ValueTypeId == (int)ProductVariantAttributeValueType.Simple
                            )
                            .OrderBy(x => x.DisplayOrder);

                        while (true)
                        {
                            var variantValues = PagedList.Create(query, ++pIndex, 500);
                            foreach (var variantValue in variantValues)
                            {
                                var name = variantValue.GetLocalized(x => x.Name, language, true, false);
                                if (!existingValues.Contains(name))
                                {
                                    existingValues.Add(name);
                                    options.Add(new RuleValueSelectListOption { Value = variantValue.Id.ToString(), Text = name });
                                }
                            }
                            if (!variantValues.HasNextPage)
                            {
                                break;
                            }
                        }
                    }
                    break;
                case "AttributeOption":
                    if (reason == RuleOptionsRequestReason.SelectedDisplayNames)
                    {
                        var ids = value.ToIntArray();
                        var attributeOptions = _attrOptionRepository.Value.TableUntracked
                            .Where(x => ids.Contains(x.Id))
                            .ToList();

                        options = attributeOptions
                            .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.GetLocalized(y => y.Name, language, true, false) })
                            .ToList();
                    }
                    else if (descriptor.Metadata.TryGetValue("ParentId", out var objParentId))
                    {
                        var query = _attrOptionRepository.Value.TableUntracked
                            .Where(x => x.SpecificationAttributeId == (int)objParentId)
                            .OrderBy(x => x.DisplayOrder);

                        var attributeOptions = PagedList.Create(query, pageIndex, pageSize);

                        result.IsPaged = true;
                        result.HasMoreData = attributeOptions.HasNextPage;

                        options = attributeOptions
                            .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.GetLocalized(y => y.Name, language, true, false) })
                            .ToList();
                    }
                    break;
                default:
                    throw new SmartException($"Unknown data source \"{list.DataSource.NaIfEmpty()}\".");
            }

            if (options != null)
            {
                if (reason == RuleOptionsRequestReason.SelectedDisplayNames)
                {
                    // Get display names of selected options.
                    if (value.HasValue())
                    {
                        var selectedValues = value.SplitSafe(",");
                        result.Options.AddRange(options.Where(x => selectedValues.Contains(x.Value)));
                    }
                }
                else
                {
                    // Get select list options.
                    if (!result.IsPaged && searchTerm.HasValue() && options.Any())
                    {
                        // Apply the search term if the options are not paged.
                        result.Options.AddRange(options.Where(x => (x.Text?.IndexOf(searchTerm, 0, StringComparison.CurrentCultureIgnoreCase) ?? -1) != -1));
                    }
                    else
                    {
                        result.Options.AddRange(options);
                    }
                }
            }

            return result;
        }

        protected virtual string GetCultureDisplayName(Language language)
        {
            if (language?.LanguageCulture?.HasValue() ?? false)
            {
                try
                {
                    return new CultureInfo(language.LanguageCulture).DisplayName;
                }
                catch { }
            }

            return null;
        }

        protected virtual string GetLocalized(ProviderMetadata metadata, string propertyName)
        {
            var resourceName = metadata.ResourceKeyPattern.FormatInvariant(metadata.SystemName, propertyName);

            return _services.Localization.GetResource(resourceName, _services.WorkContext.WorkingLanguage.Id, false, "", true).NullEmpty();
        }

        protected virtual List<RuleValueSelectListOption> SearchProducts(RuleOptionsResult result, string term, int skip, int take)
        {
            List<RuleValueSelectListOption> products;
            var fields = new List<string> { "name" };

            if (_searchSettings.Value.SearchFields.Contains("sku"))
            {
                fields.Add("sku");
            }
            if (_searchSettings.Value.SearchFields.Contains("shortdescription"))
            {
                fields.Add("shortdescription");
            }

            var searchQuery = new CatalogSearchQuery(fields.ToArray(), term);

            if (_searchSettings.Value.UseCatalogSearchInBackend)
            {
                searchQuery = searchQuery
                    .Slice(skip, take)
                    .SortBy(ProductSortingEnum.NameAsc);

                var searchResult = _catalogSearchService.Value.Search(searchQuery);
                result.HasMoreData = searchResult.Hits.HasNextPage;

                products = searchResult.Hits
                    .Select(x => new RuleValueSelectListOption
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name,
                        Hint = x.Sku
                    })
                    .ToList();
            }
            else
            {
                var query = _catalogSearchService.Value.PrepareQuery(searchQuery);

                var pageIndex = take == 0 ? 0 : Math.Max(skip / take, 0);
                result.HasMoreData = (pageIndex + 1) * take < query.Count();

                products = query
                    .Select(x => new RuleValueSelectListOption
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name,
                        Hint = x.Sku
                    })
                    .OrderBy(x => x.Text)
                    .Skip(() => skip)
                    .Take(() => take)
                    .ToList();
            }

            return products;
        }
    }
}
