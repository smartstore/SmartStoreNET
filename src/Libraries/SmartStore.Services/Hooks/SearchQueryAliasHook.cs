using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Localization;
using SmartStore.Core.Search;
using SmartStore.Services.Search.Modelling;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Hooks
{
	public class SearchQueryAliasHook : DbSaveHook<BaseEntity>
	{
		private readonly IComponentContext _ctx;
		private readonly Lazy<ICatalogSearchQueryAliasMapper> _catalogSearchQueryAliasMapper;
		private readonly Lazy<IRepository<LocalizedProperty>> _localizedPropertyRepository;

		private string _errorMessage;

		private static readonly HashSet<Type> _candidateTypes = new HashSet<Type>(new Type[]
		{
			typeof(LocalizedProperty),
			typeof(SpecificationAttribute),
			typeof(SpecificationAttributeOption),
			typeof(ProductSpecificationAttribute),
			typeof(ProductAttribute),
			typeof(ProductAttributeOption),
			typeof(ProductVariantAttribute),
			typeof(ProductVariantAttributeValue)
		});

		public SearchQueryAliasHook(
			IComponentContext ctx,
			Lazy<ICatalogSearchQueryAliasMapper> catalogSearchQueryAliasMapper,
			Lazy<IRepository<LocalizedProperty>> localizedPropertyRepository)
		{
			_ctx = ctx;
			_catalogSearchQueryAliasMapper = catalogSearchQueryAliasMapper;
			_localizedPropertyRepository = localizedPropertyRepository;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		#region Utilities

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

		private void RevertChanges(HookedEntity entry, string errorMessage)
		{
			// throw exception in OnBeforeSaveCompleted
			_errorMessage = errorMessage;

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

		private bool HasAliasDuplicate<TEntity>(HookedEntity entry, BaseEntity baseEntity) where TEntity : BaseEntity
		{
			if (entry.InitialState == EntityState.Added || entry.InitialState == EntityState.Modified)
			{
				var entity = baseEntity as ISearchAlias;
				if (entity != null)
				{
					entity.Alias = SeoExtensions.GetSeName(entity.Alias);
					if (entity.Alias.HasValue())
					{
						var repository = _ctx.Resolve<IRepository<TEntity>>();
						var allEntities = repository.TableUntracked as IQueryable<ISearchAlias>;

						if (allEntities != null && allEntities.Any(x => x.Id != entity.Id && x.Alias == entity.Alias))
						{
							RevertChanges(entry, string.Concat(T("Common.Error.AliasAlreadyExists", entity.Alias), " ", T("Common.Error.ChooseDifferentValue")));
							return true;
						}
					}
				}
			}

			return false;
		}

		private bool HasEntityDuplicate<TEntity>(
			HookedEntity entry,
			BaseEntity baseEntity,
			Func<TEntity, string> getName,
			Expression<Func<TEntity, bool>> getDuplicate) where TEntity : BaseEntity
		{
			if (entry.InitialState == EntityState.Added || entry.InitialState == EntityState.Modified)
			{
				var repository = _ctx.Resolve<IRepository<TEntity>>();
				var allEntities = repository.TableUntracked as IQueryable<TEntity>;

				var existingEntity = allEntities.FirstOrDefault(getDuplicate);
				if (existingEntity != null && existingEntity.Id != baseEntity.Id)
				{
					RevertChanges(entry, string.Concat(T("Common.Error.OptionAlreadyExists", getName(existingEntity).NaIfEmpty()), " ", T("Common.Error.ChooseDifferentValue")));
					return true;
				}
			}

			return false;
		}

		#endregion

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
			if (_errorMessage.HasValue())
			{
				var message = string.Copy(_errorMessage);
				_errorMessage = null;

				throw new SmartException(message);
			}
		}

		private void HookObject(BaseEntity baseEntity, HookedEntity entry)
		{
			var type = baseEntity.GetUnproxiedType();

			if (!_candidateTypes.Contains(type))
				return;

			if (type == typeof(SpecificationAttribute))
			{
				if (HasAliasDuplicate<SpecificationAttribute>(entry, baseEntity))
					return;

				if (IsPropertyModified(entry, "Alias"))
					_catalogSearchQueryAliasMapper.Value.ClearAttributeCache();
			}
			else if (type == typeof(SpecificationAttributeOption))
			{
				var entity = (SpecificationAttributeOption)baseEntity;

				if (HasEntityDuplicate<SpecificationAttributeOption>(entry, baseEntity, x => x.Name,
					x => x.SpecificationAttributeId == entity.SpecificationAttributeId && x.Name == entity.Name))
					return;

				if (HasAliasDuplicate<SpecificationAttributeOption>(entry, baseEntity))
					return;

				if (IsPropertyModified(entry, "Alias"))
					_catalogSearchQueryAliasMapper.Value.ClearAttributeCache();
			}
			else if (type == typeof(ProductSpecificationAttribute))
			{
				var entity = (ProductSpecificationAttribute)baseEntity;

				if (HasEntityDuplicate<ProductSpecificationAttribute>(entry, baseEntity, x => x.SpecificationAttributeOption?.Name,
					x => x.ProductId == entity.ProductId && x.SpecificationAttributeOptionId == entity.SpecificationAttributeOptionId))
					return;
			}
			else if (type == typeof(ProductAttribute))
			{
				if (HasAliasDuplicate<ProductAttribute>(entry, baseEntity))
					return;

				if (IsPropertyModified(entry, "Alias"))
					_catalogSearchQueryAliasMapper.Value.ClearVariantCache();
			}
			//else if (type == typeof(ProductAttributeOption))
			//{
			//	var entity = (ProductAttributeOption)baseEntity;

			//	if (HasEntityDuplicate<ProductAttributeOption>(entry, baseEntity, x => x.Name, 
			//		x => x.ProductAttributeId == entity.ProductAttributeId && x.Name == entity.Name))
			//		return;

			//	// ClearVariantCache() not necessary
			//	if (HasAliasDuplicate<ProductAttributeOption>(entry, baseEntity))
			//		return;
			//}
			else if (type == typeof(ProductVariantAttribute))
			{
				var entity = (ProductVariantAttribute)baseEntity;

				if (HasEntityDuplicate<ProductVariantAttribute>(entry, baseEntity, x => x.ProductAttribute?.Name, 
					x => x.ProductId == entity.ProductId && x.ProductAttributeId == entity.ProductAttributeId))
					return;
			}
			else if (type == typeof(ProductVariantAttributeValue))
			{
				var entity = (ProductVariantAttributeValue)baseEntity;

				if (HasEntityDuplicate<ProductVariantAttributeValue>(entry, baseEntity, x => x.Name,
					x => x.ProductVariantAttributeId == entity.ProductVariantAttributeId && x.Name == entity.Name))
					return;

				if (HasAliasDuplicate<ProductVariantAttributeValue>(entry, baseEntity))
					return;

				if (IsPropertyModified(entry, "Alias"))
					_catalogSearchQueryAliasMapper.Value.ClearVariantCache();
			}
			else if (type == typeof(LocalizedProperty))
			{
				// note: not fired when SpecificationAttribute or SpecificationAttributeOption deleted.
				// not necessary anyway because cache cleared by above code.
				var prop = (LocalizedProperty)baseEntity;
				var keyGroup = prop.LocaleKeyGroup;

				if (!prop.LocaleKey.IsCaseInsensitiveEqual("Alias"))
					return;

				if (!keyGroup.IsCaseInsensitiveEqual("SpecificationAttribute") &&
					!keyGroup.IsCaseInsensitiveEqual("SpecificationAttributeOption") &&
					!keyGroup.IsCaseInsensitiveEqual("ProductAttribute") &&
					!keyGroup.IsCaseInsensitiveEqual("ProductAttributeOption") &&
					!keyGroup.IsCaseInsensitiveEqual("ProductVariantAttributeValue"))
				{
					return;
				}

				// check alias duplicate
				if (entry.InitialState == EntityState.Added || entry.InitialState == EntityState.Modified)
				{
					prop.LocaleValue = SeoExtensions.GetSeName(prop.LocaleValue);
					if (prop.LocaleValue.HasValue())
					{
						var aliasExists = _localizedPropertyRepository.Value.TableUntracked.Any(x =>
							x.Id != prop.Id &&
							x.LocaleKey == "Alias" &&
							x.LocaleKeyGroup == prop.LocaleKeyGroup &&
							x.LanguageId == prop.LanguageId &&
							x.LocaleValue == prop.LocaleValue);

						if (aliasExists)
						{
							RevertChanges(entry, string.Concat(T("Common.Error.AliasAlreadyExists", prop.LocaleValue), " ", T("Common.Error.ChooseDifferentValue")));
							return;
						}
					}
				}

				if (IsPropertyModified(entry, "LocaleValue"))
				{
					if (keyGroup.IsCaseInsensitiveEqual("SpecificationAttribute") || keyGroup.IsCaseInsensitiveEqual("SpecificationAttributeOption"))
					{
						_catalogSearchQueryAliasMapper.Value.ClearAttributeCache();
					}
					else if (keyGroup.IsCaseInsensitiveEqual("ProductAttribute") || keyGroup.IsCaseInsensitiveEqual("ProductVariantAttributeValue"))
					{
						// not necessary for ProductAttributeOption
						_catalogSearchQueryAliasMapper.Value.ClearVariantCache();
					}
				}
			}
		}
	}
}
