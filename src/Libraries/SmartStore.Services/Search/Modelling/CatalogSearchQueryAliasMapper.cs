using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.Search.Modelling
{
	public class CatalogSearchQueryAliasMapper : ICatalogSearchQueryAliasMapper
	{
		private const string ALL_ATTRIBUTE_ID_BY_ALIAS_KEY = "search.attribute.id.alias.mappings.all";
		private const string ALL_ATTRIBUTE_ALIAS_BY_ID_KEY = "search.attribute.alias.id.mappings.all";

		private const string ALL_VARIANT_ID_BY_ALIAS_KEY = "search.variant.id.alias.mappings.all";
		private const string ALL_VARIANT_ALIAS_BY_ID_KEY = "search.variant.alias.id.mappings.all";

		private readonly ICacheManager _cacheManager;
		private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
		private readonly IRepository<ProductVariantAttributeValue> _productVariantAttributeValueRepository;
		private readonly ISpecificationAttributeService _specificationAttributeService;

		public CatalogSearchQueryAliasMapper(
			ICacheManager cacheManager,
			IRepository<LocalizedProperty> localizedPropertyRepository,
			IRepository<ProductVariantAttributeValue> productVariantAttributeValueRepository,
			ISpecificationAttributeService specificationAttributeService)
		{
			_cacheManager = cacheManager;
			_localizedPropertyRepository = localizedPropertyRepository;
			_productVariantAttributeValueRepository = productVariantAttributeValueRepository;
			_specificationAttributeService = specificationAttributeService;
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
			_localizedPropertyRepository.TableUntracked
				.Where(x => x.LocaleKeyGroup == localeKeyGroup && x.LocaleKey == "Alias" && x.LocaleValue != null && x.LocaleValue != string.Empty)
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
				IPagedList <SpecificationAttribute> attributes = null;
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
			_cacheManager.RemoveByPattern(ALL_ATTRIBUTE_ID_BY_ALIAS_KEY);
			_cacheManager.RemoveByPattern(ALL_ATTRIBUTE_ALIAS_BY_ID_KEY);
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
				var optionIdMappings = new Dictionary<int, int>();
				IPagedList<ProductVariantAttributeValue> options = null;
				var variantId = 0;
				var pageIndex = 0;

				var query = _productVariantAttributeValueRepository.TableUntracked
					.Expand(x => x.ProductVariantAttribute)
					.Expand("ProductVariantAttribute.ProductAttribute")
					.OrderBy(x => x.Id);

				do
				{
					options = new PagedList<ProductVariantAttributeValue>(query, pageIndex++, 500);

					foreach (var option in options)
					{
						var variant = option.ProductVariantAttribute.ProductAttribute;

						optionIdMappings[option.Id] = variant.Id;

						if (variant.Alias.HasValue())
						{
							result[CreateKey("vari", 0, variant.Alias)] = variant.Id;
						}

						if (option.Alias.HasValue())
						{
							result[CreateOptionKey("vari.option", 0, variant.Id, option.Alias)] = option.Id;
						}
					}
				}
				while (options.HasNextPage);

				options.Clear();

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
				IPagedList<ProductVariantAttributeValue> options = null;
				var pageIndex = 0;

				var query = _productVariantAttributeValueRepository.TableUntracked
					.Expand(x => x.ProductVariantAttribute)
					.Expand("ProductVariantAttribute.ProductAttribute")
					.OrderBy(x => x.Id);

				do
				{
					options = new PagedList<ProductVariantAttributeValue>(query, pageIndex++, 500);

					foreach (var option in options)
					{
						var variant = option.ProductVariantAttribute.ProductAttribute;

						if (variant.Alias.HasValue())
						{
							result[CreateKey("vari", 0, variant.Id)] = variant.Alias;
						}

						if (option.Alias.HasValue())
						{
							result[CreateOptionKey("attr.option", 0, option.Id)] = option.Alias;
						}
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
			_cacheManager.RemoveByPattern(ALL_VARIANT_ID_BY_ALIAS_KEY);
			_cacheManager.RemoveByPattern(ALL_VARIANT_ALIAS_BY_ID_KEY);
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
	}
}
