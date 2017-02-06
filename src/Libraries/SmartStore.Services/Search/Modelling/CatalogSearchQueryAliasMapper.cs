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
		private const string ALL_MAPPINGS_KEY = "search.alias.mappings.all";

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

		protected string CreateOptionKey(int languageId, int attributeId, string optionAlias)
		{
			return $"attr.option.{languageId}.{attributeId}.{optionAlias}";
		}

		protected virtual IDictionary<string, int> GetAllCachedMappings()
		{
			return _cacheManager.Get(ALL_MAPPINGS_KEY, () =>
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

		public void ClearCache()
		{
			_cacheManager.RemoveByPattern(ALL_MAPPINGS_KEY);
		}

		public int GetAttributeIdByAlias(string attributeAlias, int languageId = 0)
		{
			var result = 0;

			if (attributeAlias.HasValue())
			{
				var mappings = GetAllCachedMappings();

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
				var mappings = GetAllCachedMappings();

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
	}
}
