using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Search.Modelling;

namespace SmartStore.Services.Hooks
{
	public class SearchQueryAliasHook : DbSaveHook<BaseEntity>
	{
		private readonly Lazy<ICatalogSearchQueryAliasMapper> _catalogSearchQueryAliasMapper;
		private readonly Lazy<ISpecificationAttributeService> _specificationAttributeService;
		private readonly Lazy<ILocalizedEntityService> _localizedEntityService;

		private static readonly HashSet<Type> _candidateTypes = new HashSet<Type>(new Type[]
		{
			typeof(SpecificationAttribute),
			typeof(SpecificationAttributeOption),
			typeof(LocalizedProperty)
		});

		public SearchQueryAliasHook(
			Lazy<ICatalogSearchQueryAliasMapper> catalogSearchQueryAliasMapper,
			Lazy<ISpecificationAttributeService> specificationAttributeService,
			Lazy<ILocalizedEntityService> localizedEntityService)
		{
			_catalogSearchQueryAliasMapper = catalogSearchQueryAliasMapper;
			_specificationAttributeService = specificationAttributeService;
			_localizedEntityService = localizedEntityService;
		}

		protected override void OnDeleting(BaseEntity entity, HookedEntity entry)
		{
			HookObject(entity, entry);
		}

		protected override void OnInserting(BaseEntity entity, HookedEntity entry)
		{
			HookObject(entity, entry);
		}

		protected override void OnUpdating(BaseEntity entity, HookedEntity entry)
		{
			HookObject(entity, entry);
		}

		private void HookObject(BaseEntity entity, HookedEntity entry)
		{
			var type = entity.GetUnproxiedType();

			if (!_candidateTypes.Contains(type))
				return;

			if (type == typeof(SpecificationAttribute))
			{
				var attribute = (SpecificationAttribute)entity;
				var oldAlias = entry.Entry.Property("Alias").OriginalValue as string;

				if (!attribute.Alias.IsCaseInsensitiveEqual(oldAlias))
				{
					// remove all cached data. update results in too many changes.
					_catalogSearchQueryAliasMapper.Value.RemoveAllAttributes();
				}
			}
			else if (type == typeof(SpecificationAttributeOption))
			{
				var option = (SpecificationAttributeOption)entity;
				var oldAlias = entry.Entry.Property("Alias").OriginalValue as string;

				if (!option.Alias.IsCaseInsensitiveEqual(oldAlias))
				{
					var attribute = _specificationAttributeService.Value.GetSpecificationAttributeById(option.SpecificationAttributeId);
					if (attribute != null)
					{
						// try to remove old alias mapping
						_catalogSearchQueryAliasMapper.Value.RemoveAttribute(attribute.Alias, oldAlias);

						// add new mapping
						if (entry.InitialState != EntityState.Deleted)
						{
							_catalogSearchQueryAliasMapper.Value.AddAttribute(attribute.Alias, option.Alias, new SearchQueryAliasMapping(attribute.Id, option.Id));
						}
					}
				}
			}
			else if (type == typeof(LocalizedProperty))
			{
				var property = (LocalizedProperty)entity;
				if (!property.LocaleKey.IsCaseInsensitiveEqual("Alias"))
					return;

				if (property.LocaleKeyGroup.IsCaseInsensitiveEqual("SpecificationAttribute"))
				{
					var oldAlias = entry.Entry.Property("LocaleValue").OriginalValue as string;

					if (!property.LocaleValue.IsCaseInsensitiveEqual(oldAlias))
					{
						// remove all cached data. update results in too many changes.
						_catalogSearchQueryAliasMapper.Value.RemoveAllAttributes();
					}
				}
				else if (property.LocaleKeyGroup.IsCaseInsensitiveEqual("SpecificationAttributeOption"))
				{
					var oldAlias = entry.Entry.Property("LocaleValue").OriginalValue as string;

					if (!property.LocaleValue.IsCaseInsensitiveEqual(oldAlias))
					{
						var option = _specificationAttributeService.Value.GetSpecificationAttributeOptionById(property.EntityId);
						if (option != null)
						{
							var attributeAlias = _localizedEntityService.Value.GetLocalizedValue(property.LanguageId, option.SpecificationAttributeId, "SpecificationAttribute", "Alias");
							if (attributeAlias.HasValue())
							{
								// try to remove old alias mapping
								_catalogSearchQueryAliasMapper.Value.RemoveAttribute(attributeAlias, oldAlias);

								// add new mapping
								if (entry.InitialState != EntityState.Deleted)
								{
									_catalogSearchQueryAliasMapper.Value.AddAttribute(attributeAlias, property.LocaleValue, 
										new SearchQueryAliasMapping(option.SpecificationAttributeId, property.EntityId));
								}
							}
						}
					}
				}

			}
		}
	}
}
