using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Services.Search.Modelling;

namespace SmartStore.Services.Hooks
{
	public class SearchQueryAliasHook : DbSaveHook<BaseEntity>
	{
		private readonly Lazy<ICatalogSearchQueryAliasMapper> _catalogSearchQueryAliasMapper;

		private static readonly HashSet<Type> _candidateTypes = new HashSet<Type>(new Type[]
		{
			typeof(SpecificationAttribute),
			typeof(SpecificationAttributeOption),
			typeof(LocalizedProperty)
		});

		public SearchQueryAliasHook(Lazy<ICatalogSearchQueryAliasMapper> catalogSearchQueryAliasMapper)
		{
			_catalogSearchQueryAliasMapper = catalogSearchQueryAliasMapper;
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
				if (entry.IsModified("Alias"))
				{
					_catalogSearchQueryAliasMapper.Value.ClearCache();
				}
			}
			else if (type == typeof(SpecificationAttributeOption))
			{
				if (entry.IsModified("Alias"))
				{
					_catalogSearchQueryAliasMapper.Value.ClearCache();
				}
			}
			else if (type == typeof(LocalizedProperty))
			{
				// note: not fired when SpecificationAttribute or SpecificationAttributeOption deleted.
				// not necessary anyway because cache cleared by above code.
				var localizedProp = (LocalizedProperty)entity;

				if (localizedProp.LocaleKey.IsCaseInsensitiveEqual("Alias") &&
					(localizedProp.LocaleKeyGroup.IsCaseInsensitiveEqual("SpecificationAttribute") || localizedProp.LocaleKeyGroup.IsCaseInsensitiveEqual("SpecificationAttributeOption")))
				{
					if (entry.IsModified("LocaleValue"))
					{
						_catalogSearchQueryAliasMapper.Value.ClearCache();
					}
				}
			}
		}
	}
}
