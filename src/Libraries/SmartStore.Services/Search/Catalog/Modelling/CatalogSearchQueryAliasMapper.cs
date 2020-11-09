using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Catalog;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Search.Extensions;

namespace SmartStore.Services.Search.Modelling
{
    public class CatalogSearchQueryAliasMapper : ICatalogSearchQueryAliasMapper
    {
        private const string ALL_ATTRIBUTE_ID_BY_ALIAS_KEY = "search:attribute.id.alias.mappings.all";
        private const string ALL_ATTRIBUTE_ALIAS_BY_ID_KEY = "search:attribute.alias.id.mappings.all";
        private const string ALL_COMMONFACET_ALIAS_BY_KIND_KEY = "search:commonfacet.alias.kind.mappings.all";

        private const string ALL_VARIANT_ID_BY_ALIAS_KEY = "search:variant.id.alias.mappings.all";
        private const string ALL_VARIANT_ALIAS_BY_ID_KEY = "search:variant.alias.id.mappings.all";

        private readonly ICacheManager _cacheManager;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<ProductVariantAttributeValue> _productVariantAttributeValueRepository;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ISettingService _settingService;
        private readonly ILanguageService _languageService;

        public CatalogSearchQueryAliasMapper(
            ICacheManager cacheManager,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<ProductVariantAttributeValue> productVariantAttributeValueRepository,
            ISpecificationAttributeService specificationAttributeService,
            ISettingService settingService,
            ILanguageService languageService)
        {
            _cacheManager = cacheManager;
            _localizedPropertyRepository = localizedPropertyRepository;
            _productAttributeRepository = productAttributeRepository;
            _productVariantAttributeValueRepository = productVariantAttributeValueRepository;
            _specificationAttributeService = specificationAttributeService;
            _settingService = settingService;
            _languageService = languageService;
        }

        protected string CreateKey(string prefix, int languageId, string alias)
        {
            return $"{prefix}.{languageId}.{alias}";
        }
        protected string CreateKey(string prefix, int languageId, int attributeId)
        {
            return $"{prefix}.{languageId}.{attributeId}";
        }

        protected string CreateOptionKey(string prefix, int languageId, int attributeId, string optionAlias)
        {
            return $"{prefix}.{languageId}.{attributeId}.{optionAlias}";
        }
        protected string CreateOptionKey(string prefix, int languageId, int optionId)
        {
            return $"{prefix}.{languageId}.{optionId}";
        }

        protected void CachedLocalizedAlias(string localeKeyGroup, Action<LocalizedProperty> caching)
        {
            var properties = _localizedPropertyRepository.TableUntracked
                .Where(x => x.LocaleKeyGroup == localeKeyGroup && x.LocaleKey == "Alias")
                .ToList();

            // SQL CE: leads to an error when checked in the query.
            properties
                .Where(x => !string.IsNullOrWhiteSpace(x.LocaleValue))
                .ToList()
                .ForEach(caching);
        }

        #region Specification Attributes

        protected virtual IDictionary<string, int> GetAttributeIdByAliasMappings()
        {
            return _cacheManager.Get(ALL_ATTRIBUTE_ID_BY_ALIAS_KEY, () =>
            {
                var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var optionIdMappings = new Dictionary<int, int>();
                IPagedList<SpecificationAttribute> attributes = null;
                var attributeId = 0;
                var pageIndex = 0;

                var query = _specificationAttributeService.GetSpecificationAttributes()
                    .Expand(x => x.SpecificationAttributeOptions)
                    .OrderBy(x => x.Id);

                do
                {
                    attributes = new PagedList<SpecificationAttribute>(query, pageIndex++, 500);

                    foreach (var attribute in attributes)
                    {
                        if (attribute.Alias.HasValue())
                        {
                            result[CreateKey("attr", 0, attribute.Alias)] = attribute.Id;
                        }

                        foreach (var option in attribute.SpecificationAttributeOptions)
                        {
                            optionIdMappings[option.Id] = option.SpecificationAttributeId;

                            if (option.Alias.HasValue())
                            {
                                result[CreateOptionKey("attr.option", 0, attribute.Id, option.Alias)] = option.Id;
                            }
                        }
                    }
                }
                while (attributes.HasNextPage);

                attributes.Clear();

                CachedLocalizedAlias("SpecificationAttribute", x => result[CreateKey("attr", x.LanguageId, x.LocaleValue)] = x.EntityId);
                CachedLocalizedAlias("SpecificationAttributeOption", x =>
                {
                    if (optionIdMappings.TryGetValue(x.EntityId, out attributeId))
                        result[CreateOptionKey("attr.option", x.LanguageId, attributeId, x.LocaleValue)] = x.EntityId;
                });

                return result;
            });
        }

        protected virtual IDictionary<string, string> GetAttributeAliasByIdMappings()
        {
            return _cacheManager.Get(ALL_ATTRIBUTE_ALIAS_BY_ID_KEY, () =>
            {
                var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                IPagedList<SpecificationAttribute> attributes = null;
                var pageIndex = 0;

                var query = _specificationAttributeService.GetSpecificationAttributes()
                    .Expand(x => x.SpecificationAttributeOptions)
                    .OrderBy(x => x.Id);

                do
                {
                    attributes = new PagedList<SpecificationAttribute>(query, pageIndex++, 500);

                    foreach (var attribute in attributes)
                    {
                        if (attribute.Alias.HasValue())
                        {
                            result[CreateKey("attr", 0, attribute.Id)] = attribute.Alias;
                        }

                        foreach (var option in attribute.SpecificationAttributeOptions.Where(x => x.Alias.HasValue()))
                        {
                            result[CreateOptionKey("attr.option", 0, option.Id)] = option.Alias;
                        }
                    }
                }
                while (attributes.HasNextPage);

                attributes.Clear();

                CachedLocalizedAlias("SpecificationAttribute", x => result[CreateKey("attr", x.LanguageId, x.EntityId)] = x.LocaleValue);
                CachedLocalizedAlias("SpecificationAttributeOption", x => result[CreateOptionKey("attr.option", x.LanguageId, x.EntityId)] = x.LocaleValue);

                return result;
            });
        }

        public void ClearAttributeCache()
        {
            _cacheManager.Remove(ALL_ATTRIBUTE_ID_BY_ALIAS_KEY);
            _cacheManager.Remove(ALL_ATTRIBUTE_ALIAS_BY_ID_KEY);
        }

        public int GetAttributeIdByAlias(string attributeAlias, int languageId = 0)
        {
            var result = 0;

            if (attributeAlias.HasValue())
            {
                var mappings = GetAttributeIdByAliasMappings();

                if (!mappings.TryGetValue(CreateKey("attr", languageId, attributeAlias), out result) && languageId != 0)
                {
                    mappings.TryGetValue(CreateKey("attr", 0, attributeAlias), out result);
                }
            }

            return result;
        }

        public int GetAttributeOptionIdByAlias(string optionAlias, int attributeId, int languageId = 0)
        {
            var result = 0;

            if (optionAlias.HasValue() && attributeId != 0)
            {
                var mappings = GetAttributeIdByAliasMappings();

                if (!mappings.TryGetValue(CreateOptionKey("attr.option", languageId, attributeId, optionAlias), out result) && languageId != 0)
                {
                    mappings.TryGetValue(CreateOptionKey("attr.option", 0, attributeId, optionAlias), out result);
                }
            }

            return result;
        }

        public string GetAttributeAliasById(int attributeId, int languageId = 0)
        {
            string result = null;

            if (attributeId != 0)
            {
                var mappings = GetAttributeAliasByIdMappings();

                if (!mappings.TryGetValue(CreateKey("attr", languageId, attributeId), out result) && languageId != 0)
                {
                    mappings.TryGetValue(CreateKey("attr", 0, attributeId), out result);
                }
            }

            return result;
        }

        public string GetAttributeOptionAliasById(int optionId, int languageId = 0)
        {
            string result = null;

            if (optionId != 0)
            {
                var mappings = GetAttributeAliasByIdMappings();

                if (!mappings.TryGetValue(CreateOptionKey("attr.option", languageId, optionId), out result) && languageId != 0)
                {
                    mappings.TryGetValue(CreateOptionKey("attr.option", 0, optionId), out result);
                }
            }

            return result;
        }

        #endregion

        #region Product Variants

        protected virtual IDictionary<string, int> GetVariantIdByAliasMappings()
        {
            return _cacheManager.Get(ALL_VARIANT_ID_BY_ALIAS_KEY, () =>
            {
                var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                IPagedList<ProductAttribute> variants = null;
                IPagedList<ProductVariantAttributeValue> options = null;
                var variantId = 0;
                var pageIndex = 0;

                var variantQuery = _productAttributeRepository.TableUntracked
                    .Where(x => !string.IsNullOrEmpty(x.Alias))
                    .OrderBy(x => x.Id);

                var optionQuery = _productVariantAttributeValueRepository.TableUntracked
                    .Expand(x => x.ProductVariantAttribute)
                    .Expand("ProductVariantAttribute.ProductAttribute")
                    .Where(x => !string.IsNullOrEmpty(x.Alias))
                    .OrderBy(x => x.Id);

                do
                {
                    variants = new PagedList<ProductAttribute>(variantQuery, pageIndex++, 500);

                    foreach (var variant in variants)
                    {
                        result[CreateKey("vari", 0, variant.Alias)] = variant.Id;
                    }
                }
                while (variants.HasNextPage);
                pageIndex = 0;
                variants.Clear();

                do
                {
                    options = new PagedList<ProductVariantAttributeValue>(optionQuery, pageIndex++, 500);

                    foreach (var option in options)
                    {
                        var variant = option.ProductVariantAttribute.ProductAttribute;
                        result[CreateOptionKey("vari.option", 0, variant.Id, option.Alias)] = option.Id;
                    }
                }
                while (options.HasNextPage);
                options.Clear();

                var optionIdMappings = _productVariantAttributeValueRepository.TableUntracked
                    .Expand(x => x.ProductVariantAttribute.ProductAttribute)
                    .Select(x => new
                    {
                        OptionId = x.Id,
                        VariantId = x.ProductVariantAttribute.ProductAttribute.Id
                    })
                    .ToDictionary(x => x.OptionId, x => x.VariantId);

                CachedLocalizedAlias("ProductAttribute", x => result[CreateKey("vari", x.LanguageId, x.LocaleValue)] = x.EntityId);
                CachedLocalizedAlias("ProductVariantAttributeValue", x =>
                {
                    if (optionIdMappings.TryGetValue(x.EntityId, out variantId))
                        result[CreateOptionKey("vari.option", x.LanguageId, variantId, x.LocaleValue)] = x.EntityId;
                });

                return result;
            });
        }

        protected virtual IDictionary<string, string> GetVariantAliasByIdMappings()
        {
            return _cacheManager.Get(ALL_VARIANT_ALIAS_BY_ID_KEY, () =>
            {
                var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                IPagedList<ProductAttribute> variants = null;
                IPagedList<ProductVariantAttributeValue> options = null;
                var pageIndex = 0;

                var variantQuery = _productAttributeRepository.TableUntracked
                    .Where(x => !string.IsNullOrEmpty(x.Alias))
                    .OrderBy(x => x.Id);

                var optionQuery = _productVariantAttributeValueRepository.TableUntracked
                    .Where(x => !string.IsNullOrEmpty(x.Alias))
                    .OrderBy(x => x.Id);

                do
                {
                    variants = new PagedList<ProductAttribute>(variantQuery, pageIndex++, 500);

                    foreach (var variant in variants)
                    {
                        result[CreateKey("vari", 0, variant.Id)] = variant.Alias;
                    }
                }
                while (variants.HasNextPage);
                pageIndex = 0;
                variants.Clear();

                do
                {
                    options = new PagedList<ProductVariantAttributeValue>(optionQuery, pageIndex++, 500);

                    foreach (var option in options)
                    {
                        result[CreateOptionKey("attr.option", 0, option.Id)] = option.Alias;
                    }
                }
                while (options.HasNextPage);
                options.Clear();

                CachedLocalizedAlias("ProductAttribute", x => result[CreateKey("vari", x.LanguageId, x.EntityId)] = x.LocaleValue);
                CachedLocalizedAlias("ProductVariantAttributeValue", x => result[CreateOptionKey("vari.option", x.LanguageId, x.EntityId)] = x.LocaleValue);

                return result;
            });
        }

        public void ClearVariantCache()
        {
            _cacheManager.Remove(ALL_VARIANT_ID_BY_ALIAS_KEY);
            _cacheManager.Remove(ALL_VARIANT_ALIAS_BY_ID_KEY);
        }

        public int GetVariantIdByAlias(string variantAlias, int languageId = 0)
        {
            var result = 0;

            if (variantAlias.HasValue())
            {
                var mappings = GetVariantIdByAliasMappings();

                if (!mappings.TryGetValue(CreateKey("vari", languageId, variantAlias), out result) && languageId != 0)
                {
                    mappings.TryGetValue(CreateKey("vari", 0, variantAlias), out result);
                }
            }

            return result;
        }

        public int GetVariantOptionIdByAlias(string optionAlias, int variantId, int languageId = 0)
        {
            var result = 0;

            if (optionAlias.HasValue() && variantId != 0)
            {
                var mappings = GetVariantIdByAliasMappings();

                if (!mappings.TryGetValue(CreateOptionKey("vari.option", languageId, variantId, optionAlias), out result) && languageId != 0)
                {
                    mappings.TryGetValue(CreateOptionKey("vari.option", 0, variantId, optionAlias), out result);
                }
            }

            return result;
        }

        public string GetVariantAliasById(int variantId, int languageId = 0)
        {
            string result = null;

            if (variantId != 0)
            {
                var mappings = GetVariantAliasByIdMappings();

                if (!mappings.TryGetValue(CreateKey("vari", languageId, variantId), out result) && languageId != 0)
                {
                    mappings.TryGetValue(CreateKey("vari", 0, variantId), out result);
                }
            }

            return result;
        }

        public string GetVariantOptionAliasById(int optionId, int languageId = 0)
        {
            string result = null;

            if (optionId != 0)
            {
                var mappings = GetVariantAliasByIdMappings();

                if (!mappings.TryGetValue(CreateOptionKey("vari.option", languageId, optionId), out result) && languageId != 0)
                {
                    mappings.TryGetValue(CreateOptionKey("vari.option", 0, optionId), out result);
                }
            }

            return result;
        }

        #endregion

        #region Common Facets

        protected virtual IDictionary<string, string> GetCommonFacetAliasByGroupKindMappings()
        {
            return _cacheManager.Get(ALL_COMMONFACET_ALIAS_BY_KIND_KEY, () =>
            {
                var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                var groupKinds = new FacetGroupKind[]
                {
                    FacetGroupKind.Category,
                    FacetGroupKind.Brand,
                    FacetGroupKind.Price,
                    FacetGroupKind.Rating,
                    FacetGroupKind.DeliveryTime,
                    FacetGroupKind.Availability,
                    FacetGroupKind.NewArrivals
                };

                foreach (var language in _languageService.GetAllLanguages())
                {
                    foreach (var groupKind in groupKinds)
                    {
                        var key = FacetUtility.GetFacetAliasSettingKey(groupKind, language.Id);
                        var value = _settingService.GetSettingByKey<string>(key);
                        if (value.HasValue())
                        {
                            result.Add(key, value);
                        }
                    }
                }

                return result;
            });
        }

        public void ClearCommonFacetCache()
        {
            _cacheManager.Remove(ALL_COMMONFACET_ALIAS_BY_KIND_KEY);
        }

        public string GetCommonFacetAliasByGroupKind(FacetGroupKind kind, int languageId)
        {
            var mappings = GetCommonFacetAliasByGroupKindMappings();

            return mappings.Get(FacetUtility.GetFacetAliasSettingKey(kind, languageId));
        }

        #endregion
    }
}
