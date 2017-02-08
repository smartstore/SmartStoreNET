using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.Search.Modelling
{
	public class CatalogSearchQueryAliasMapper : ICatalogSearchQueryAliasMapper
	{
		private const string ALL_ID_BY_ALIAS_MAPPINGS_KEY = "search.id.alias.mappings.all";
		private const string ALL_ALIAS_BY_ID_MAPPINGS_KEY = "search.alias.id.mappings.all";

		private readonly ICacheManager _cacheManager;
		private readonly ISpecificationAttributeService _specificationAttributeService;
		private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;

		public CatalogSearchQueryAliasMapper(
			ICacheManager cacheManager,
			ISpecificationAttributeService specificationAttributeService,
			IRepository<LocalizedProperty> localizedPropertyRepository)
		{
			_cacheManager = cacheManager;
			_specificationAttributeService = specificationAttributeService;
			_localizedPropertyRepository = localizedPropertyRepository;
		}

		protected string CreateAttributeKey(int languageId, string attributeAlias)
		{
			return $"attr.{languageId}.{attributeAlias}";
		}
		protected string CreateAttributeKey(int languageId, int attributeId)
		{
			return $"attr.{languageId}.{attributeId}";
		}

		protected string CreateOptionKey(int languageId, int attributeId, string optionAlias)
		{
			return $"attr.option.{languageId}.{attributeId}.{optionAlias}";
		}
		protected string CreateOptionKey(int languageId, int optionId)
		{
			return $"attr.option.{languageId}.{optionId}";
		}

		protected virtual IDictionary<string, int> GetCachedIdByAliasMappings()
		{
			return _cacheManager.Get(ALL_ID_BY_ALIAS_MAPPINGS_KEY, () =>
			{
				var dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

				var attributes = _specificationAttributeService.GetSpecificationAttributes()
					.Expand(x => x.SpecificationAttributeOptions)
					.ToList();

				var attrinutId = 0;
				var optionIds = attributes.SelectMany(x => x.SpecificationAttributeOptions).ToDictionary(x => x.Id, x => x.SpecificationAttributeId);

				var locAttributes = _localizedPropertyRepository.TableUntracked
					.Where(x => x.LocaleKeyGroup == "SpecificationAttribute" && x.LocaleKey == "Alias" && x.LocaleValue != null && x.LocaleValue != string.Empty)
					.ToList();

				var locOptions = _localizedPropertyRepository.TableUntracked
					.Where(x => x.LocaleKeyGroup == "SpecificationAttributeOption" && x.LocaleKey == "Alias" && x.LocaleValue != null && x.LocaleValue != string.Empty)
					.ToList();

				foreach (var attribute in attributes)
				{
					if (attribute.Alias.HasValue())
					{
						dictionary[CreateAttributeKey(0, attribute.Alias)] = attribute.Id;
					}

					foreach (var option in attribute.SpecificationAttributeOptions.Where(x => x.Alias.HasValue()))
					{
						dictionary[CreateOptionKey(0, attribute.Id, option.Alias)] = option.Id;
					}
				}

				foreach (var locAttribute in locAttributes)
				{
					dictionary[CreateAttributeKey(locAttribute.LanguageId, locAttribute.LocaleValue)] = locAttribute.EntityId;
				}

				foreach (var locOption in locOptions)
				{
					if (optionIds.TryGetValue(locOption.EntityId, out attrinutId))
					{
						dictionary[CreateOptionKey(locOption.LanguageId, attrinutId, locOption.LocaleValue)] = locOption.EntityId;
					}
				}

				return dictionary;
			});
		}

		protected virtual IDictionary<string, string> GetCachedAliasByIdMappings()
		{
			return _cacheManager.Get(ALL_ALIAS_BY_ID_MAPPINGS_KEY, () =>
			{
				var dictionary = new Dictionary<string, string>();

				var attributes = _specificationAttributeService.GetSpecificationAttributes()
					.Expand(x => x.SpecificationAttributeOptions)
					.ToList();

				var locAttributes = _localizedPropertyRepository.TableUntracked
					.Where(x => x.LocaleKeyGroup == "SpecificationAttribute" && x.LocaleKey == "Alias" && x.LocaleValue != null && x.LocaleValue != string.Empty)
					.ToList();

				var locOptions = _localizedPropertyRepository.TableUntracked
					.Where(x => x.LocaleKeyGroup == "SpecificationAttributeOption" && x.LocaleKey == "Alias" && x.LocaleValue != null && x.LocaleValue != string.Empty)
					.ToList();

				foreach (var attribute in attributes)
				{
					if (attribute.Alias.HasValue())
					{
						dictionary[CreateAttributeKey(0, attribute.Id)] = attribute.Alias;
					}

					foreach (var option in attribute.SpecificationAttributeOptions.Where(x => x.Alias.HasValue()))
					{
						dictionary[CreateOptionKey(0, option.Id)] = option.Alias;
					}
				}

				foreach (var locAttribute in locAttributes)
				{
					dictionary[CreateAttributeKey(locAttribute.LanguageId, locAttribute.EntityId)] = locAttribute.LocaleValue;
				}

				foreach (var locOption in locOptions)
				{
					dictionary[CreateOptionKey(locOption.LanguageId, locOption.EntityId)] = locOption.LocaleValue;
				}

				return dictionary;
			});
		}

		public void ClearCache()
		{
			_cacheManager.RemoveByPattern(ALL_ID_BY_ALIAS_MAPPINGS_KEY);
			_cacheManager.RemoveByPattern(ALL_ALIAS_BY_ID_MAPPINGS_KEY);
		}

		public int GetAttributeIdByAlias(string attributeAlias, int languageId = 0)
		{
			var result = 0;

			if (attributeAlias.HasValue())
			{
				var mappings = GetCachedIdByAliasMappings();

				if (!mappings.TryGetValue(CreateAttributeKey(languageId, attributeAlias), out result))
				{
					if (languageId != 0)
					{
						mappings.TryGetValue(CreateAttributeKey(0, attributeAlias), out result);
					}
				}
			}

			return result;
		}

		public int GetOptionIdByAlias(string optionAlias, int attributeId, int languageId = 0)
		{
			var result = 0;

			if (optionAlias.HasValue() && attributeId != 0)
			{
				var mappings = GetCachedIdByAliasMappings();

				if (!mappings.TryGetValue(CreateOptionKey(languageId, attributeId, optionAlias), out result))
				{
					if (languageId != 0)
					{
						mappings.TryGetValue(CreateOptionKey(0, attributeId, optionAlias), out result);
					}
				}
			}

			return result;
		}

		public string GetAttributeAliasById(int attributeId, int languageId = 0)
		{
			string result = null;

			if (attributeId != 0)
			{
				var mappings = GetCachedAliasByIdMappings();

				if (!mappings.TryGetValue(CreateAttributeKey(languageId, attributeId), out result))
				{
					if (languageId != 0)
					{
						mappings.TryGetValue(CreateAttributeKey(0, attributeId), out result);
					}
				}
			}

			return result;
		}

		public string GetOptionAliasById(int optionId, int languageId = 0)
		{
			string result = null;

			if (optionId != 0)
			{
				var mappings = GetCachedAliasByIdMappings();

				if (!mappings.TryGetValue(CreateOptionKey(languageId, optionId), out result))
				{
					if (languageId != 0)
					{
						mappings.TryGetValue(CreateOptionKey(0, optionId), out result);
					}
				}
			}

			return result;
		}
	}
}
