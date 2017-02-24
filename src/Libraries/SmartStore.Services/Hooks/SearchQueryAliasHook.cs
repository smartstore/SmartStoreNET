using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Localization;
using SmartStore.Services.Search.Modelling;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Hooks
{
	public class SearchQueryAliasHook : DbSaveHook<BaseEntity>
	{
		private readonly Lazy<ICatalogSearchQueryAliasMapper> _catalogSearchQueryAliasMapper;
		private readonly Lazy<IRepository<LocalizedProperty>> _localizedPropertyRepository;
		private string _duplicateAlias;

		private static readonly HashSet<Type> _candidateTypes = new HashSet<Type>(new Type[]
		{
			typeof(LocalizedProperty),
			typeof(SpecificationAttribute),
			typeof(SpecificationAttributeOption),
			typeof(ProductAttribute),
			typeof(ProductVariantAttributeValue)
		});

		public SearchQueryAliasHook(
			Lazy<ICatalogSearchQueryAliasMapper> catalogSearchQueryAliasMapper,
			Lazy<IRepository<LocalizedProperty>> localizedPropertyRepository)
		{
			_catalogSearchQueryAliasMapper = catalogSearchQueryAliasMapper;
			_localizedPropertyRepository = localizedPropertyRepository;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

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

		public override void OnBeforeSaveCompleted()
		{
			if (_duplicateAlias.HasValue())
			{
				var alias = string.Copy(_duplicateAlias);
				_duplicateAlias = null;

				throw new SmartException(T("Common.Error.AliasAlreadyExists", alias));
			}
		}

		private bool IsPropertyModified(HookedEntity entry, string propertyName)
		{
			var result = false;

			if (entry.State != EntityState.Detached)
			{
				var prop = entry.Entry.Property(propertyName);
				if (prop != null)
				{
					switch (entry.State)
					{
						case EntityState.Added:
							// OriginalValues cannot be used for entities in the Added state.
							result = prop.CurrentValue != null;
							break;
						case EntityState.Deleted:
							// CurrentValues cannot be used for entities in the Deleted state.
							result = prop.OriginalValue != null;
							break;
						default:
							result = prop.CurrentValue != null && !prop.CurrentValue.Equals(prop.OriginalValue);
							break;
					}
				}
			}

			return result;
		}

		private void HookObject(BaseEntity entity, HookedEntity entry)
		{
			var type = entity.GetUnproxiedType();

			if (!_candidateTypes.Contains(type))
				return;

			if (type == typeof(SpecificationAttribute) || type == typeof(SpecificationAttributeOption))
			{
				if (IsPropertyModified(entry, "Alias"))
				{
					_catalogSearchQueryAliasMapper.Value.ClearAttributeCache();
				}
			}
			else if (type == typeof(ProductAttribute) || type == typeof(ProductVariantAttributeValue))
			{
				if (IsPropertyModified(entry, "Alias"))
				{
					_catalogSearchQueryAliasMapper.Value.ClearVariantCache();
				}
			}
			else if (type == typeof(LocalizedProperty))
			{
				// note: not fired when SpecificationAttribute or SpecificationAttributeOption deleted.
				// not necessary anyway because cache cleared by above code.
				var prop = (LocalizedProperty)entity;
				if (!prop.LocaleKey.IsCaseInsensitiveEqual("Alias"))
					return;

				var isAttribute = prop.LocaleKeyGroup.IsCaseInsensitiveEqual("SpecificationAttribute") || prop.LocaleKeyGroup.IsCaseInsensitiveEqual("SpecificationAttributeOption");
				var isVariant = prop.LocaleKeyGroup.IsCaseInsensitiveEqual("ProductAttribute") || prop.LocaleKeyGroup.IsCaseInsensitiveEqual("ProductVariantAttributeValue");

				if (!isAttribute && !isVariant)
					return;

				if (IsPropertyModified(entry, "LocaleValue"))
				{
					if (isAttribute)
						_catalogSearchQueryAliasMapper.Value.ClearAttributeCache();
					else if (isVariant)
						_catalogSearchQueryAliasMapper.Value.ClearVariantCache();
				}

				// check duplicates
				if (entry.InitialState == EntityState.Added || entry.InitialState == EntityState.Modified)
				{
					prop.LocaleValue = SeoExtensions.GetSeName(prop.LocaleValue);

					var existingAlias = _localizedPropertyRepository.Value.TableUntracked
						.FirstOrDefault(x => x.LocaleKey == "Alias" && x.LocaleKeyGroup == prop.LocaleKeyGroup && x.LanguageId == prop.LanguageId && x.LocaleValue == prop.LocaleValue);

					// duplicate found
					if (existingAlias != null && existingAlias.Id != prop.Id)
					{
						// throw exception in OnBeforeSaveCompleted
						_duplicateAlias = string.Copy(prop.LocaleValue);

						// revert changes
						if (entry.Entry.State == System.Data.Entity.EntityState.Modified)
						{
							entry.Entry.State = System.Data.Entity.EntityState.Unchanged;
						}
						else if (entry.Entry.State == System.Data.Entity.EntityState.Added)
						{
							entry.Entry.State = System.Data.Entity.EntityState.Detached;
						}
					}
				}
			}
		}
	}
}
