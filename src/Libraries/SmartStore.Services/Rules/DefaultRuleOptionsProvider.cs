using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using SmartStore.Core;
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
        protected readonly Lazy<ICurrencyService> _currencyService;
        protected readonly Lazy<ICustomerService> _customerService;
        protected readonly Lazy<ILanguageService> _languageService;
        protected readonly Lazy<ICountryService> _countryService;
        protected readonly Lazy<ICatalogSearchService> _catalogSearchService;
        protected readonly Lazy<IProductService> _productService;
        protected readonly Lazy<ICategoryService> _categoryService;
        protected readonly Lazy<IManufacturerService> _manufacturerService;
        protected readonly Lazy<IShippingService> _shippingService;
        protected readonly Lazy<ISpecificationAttributeService> _specificationAttributeService;
        protected readonly Lazy<IRuleStorage> _ruleStorage;
        protected readonly Lazy<IProviderManager> _providerManager;
        protected readonly Lazy<SearchSettings> _searchSettings;

        public DefaultRuleOptionsProvider(
            ICommonServices services,
            Lazy<ICurrencyService> currencyService,
            Lazy<ICustomerService> customerService,
            Lazy<ILanguageService> languageService,
            Lazy<ICountryService> countryService,
            Lazy<ICatalogSearchService> catalogSearchService,
            Lazy<IProductService> productService,
            Lazy<ICategoryService> categoryService,
            Lazy<IManufacturerService> manufacturerService,
            Lazy<IShippingService> shippingService,
            Lazy<ISpecificationAttributeService> specificationAttributeService,
            Lazy<IRuleStorage> ruleStorage,
            Lazy<IProviderManager> providerManager,
            Lazy<SearchSettings> searchSettings)
        {
            _services = services;
            _currencyService = currencyService;
            _customerService = customerService;
            _languageService = languageService;
            _countryService = countryService;
            _catalogSearchService = catalogSearchService;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _shippingService = shippingService;
            _specificationAttributeService = specificationAttributeService;
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
                case "CustomerRole":
                case "Language":
                case "Manufacturer":
                case "PaymentMethod":
                case "Product":
                case "ShippingMethod":
                case "ShippingRateComputationMethod":
                case "TargetGroup":
                case "Attribute":
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
            Guard.NotNull(expression.Descriptor, nameof(expression.Descriptor));

            var result = new RuleOptionsResult();
            var selectList = reason == RuleOptionsRequestReason.LeftSelectListOptions ? expression.Descriptor.LeftSelectList : expression.Descriptor.SelectList;

            if (!(selectList is RemoteRuleValueSelectList list))
            {
                return result;
            }

            var language = _services.WorkContext.WorkingLanguage;
            var byId = expression.Descriptor.RuleType == RuleType.Int || expression.Descriptor.RuleType == RuleType.IntArray;
            List<RuleValueSelectListOption> options = null;

            switch (list.DataSource)
            {
                case "Product":
                    if (reason == RuleOptionsRequestReason.SelectedDisplayNames)
                    {
                        options = _productService.Value.GetProductsByIds(expression.RawValue.ToIntArray())
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
                        options = _ruleStorage.Value.GetRuleSetsByIds(expression.RawValue.ToIntArray(), false)
                            .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.Name })
                            .ToList();
                    }
                    else
                    {
                        var ruleSets = _ruleStorage.Value.GetAllRuleSets(false, false, expression.Descriptor.Scope, pageIndex, pageSize, false, true);
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
                        options = _categoryService.Value.GetCategoriesByIds(expression.RawValue.ToIntArray())
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
                case "Attribute":
                    if (reason == RuleOptionsRequestReason.SelectedDisplayNames)
                    {
                        options = new List<RuleValueSelectListOption>();
                    }
                    else
                    {
                        var specAttributes = new PagedList<SpecificationAttribute>(_specificationAttributeService.Value.GetSpecificationAttributes(), pageIndex, pageSize);
                        result.IsPaged = true;
                        result.HasMoreData = specAttributes.HasNextPage;

                        options = specAttributes
                            .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.GetLocalized(y => y.Name, language, true, false) })
                            .ToList();
                    }
                    break;
                case "AttributeOption":
                    if (expression.Metadata.TryGetValue("ParentId", out var objParentId))
                    {
                        var parentId = (int)objParentId;
                    }
                    else
                    {
                        options = new List<RuleValueSelectListOption>();
                    }
                    break;
                default:
                    throw new SmartException($"Unknown data source \"{list.DataSource.NaIfEmpty()}\".");
            }

            if (reason == RuleOptionsRequestReason.SelectedDisplayNames)
            {
                // Get display names of selected options.
                if (expression.RawValue.HasValue())
                {
                    var selectedValues = expression.RawValue.SplitSafe(",");
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
