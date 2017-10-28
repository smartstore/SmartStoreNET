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

		private bool IsPropertyModified(IHookedEntity entry, string propertyName)
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
							result =
								(prop.CurrentValue != null && !prop.CurrentValue.Equals(prop.OriginalValue)) ||
								(prop.OriginalValue != null && !prop.OriginalValue.Equals(prop.CurrentValue));
							break;
					}
				}
			}

			return result;
		}

		private void RevertChanges(IHookedEntity entry, string errorMessage)
		{
			// throw exception in OnBeforeSaveCompleted
			_errorMessage = errorMessage;

			// revert changes
			if (entry.State == EntityState.Modified)
			{
				entry.State = EntityState.Unchanged;
			}
			else if (entry.State == EntityState.Added)
			{
				entry.State = EntityState.Detached;
			}
		}

		private bool HasAliasDuplicate<TEntity>(
			IHookedEntity entry,
			BaseEntity baseEntity,
			Func<IQueryable<TEntity>, TEntity, bool> hasDuplicate = null) 
			where TEntity : BaseEntity
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

						//if (allEntities != null && allEntities.Any(x => x.Id != entity.Id && x.Alias == entity.Alias))
						if (allEntities != null)
						{
							var duplicateExists = hasDuplicate == null
								? allEntities.Any(x => x.Id != entity.Id && x.Alias == entity.Alias)
								: hasDuplicate(repository.TableUntracked, (TEntity)entity);

							if (duplicateExists)
							{
								RevertChanges(entry, string.Concat(T("Common.Error.AliasAlreadyExists", entity.Alias), " ", T("Common.Error.ChooseDifferentValue")));
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		private bool HasAliasDuplicate<T1, T2>(IHookedEntity entry, BaseEntity baseEntity) where T1 : BaseEntity where T2 : BaseEntity
		{
			if (entry.InitialState == EntityState.Added || entry.InitialState == EntityState.Modified)
			{
				var entity = baseEntity as ISearchAlias;
				if (entity != null)
				{
					entity.Alias = SeoExtensions.GetSeName(entity.Alias);
					if (entity.Alias.HasValue())
					{
						var entities1 = _ctx.Resolve<IRepository<T1>>().TableUntracked as IQueryable<ISearchAlias>;
						var entities2 = _ctx.Resolve<IRepository<T2>>().TableUntracked as IQueryable<ISearchAlias>;

						var duplicate1 = entities1.FirstOrDefault(x => x.Alias == entity.Alias);
						var duplicate2 = entities2.FirstOrDefault(x => x.Alias == entity.Alias);

						if (duplicate1 != null || duplicate2 != null)
						{
							var type = entry.EntityType;

							if (duplicate1 != null && duplicate1.Id == entity.Id && type == typeof(T1))
								return false;
							if (duplicate2 != null && duplicate2.Id == entity.Id && type == typeof(T2))
								return false;

							RevertChanges(entry, string.Concat(T("Common.Error.AliasAlreadyExists", entity.Alias), " ", T("Common.Error.ChooseDifferentValue")));
							return true;
						}
					}
				}
			}

			return false;
		}

		private bool HasAliasDuplicate(LocalizedProperty property)
		{
			var existingProperties = _localizedPropertyRepository.Value.Table.Where(x =>
				 x.Id != property.Id &&
				 x.LocaleKey == "Alias" &&
				 x.LocaleKeyGroup == property.LocaleKeyGroup &&
				 x.LanguageId == property.LanguageId &&
				 x.LocaleValue == property.LocaleValue).ToList();

			if (existingProperties.Count == 0)
			{
				// Check cases where alias has to be globally unique.
				string otherKeyGroup = null;

				if (property.LocaleKeyGroup.IsCaseInsensitiveEqual("SpecificationAttribute"))
					otherKeyGroup = "ProductAttribute";
				else if (property.LocaleKeyGroup.IsCaseInsensitiveEqual("ProductAttribute"))
					otherKeyGroup = "SpecificationAttribute";

				if (otherKeyGroup.HasValue())
				{
					existingProperties = _localizedPropertyRepository.Value.Table.Where(x =>
						x.LocaleKey == "Alias" &&
						x.LocaleKeyGroup == otherKeyGroup &&
						x.LanguageId == property.LanguageId &&
						x.LocaleValue == property.LocaleValue).ToList();
				}

				if (existingProperties.Count == 0)
					return false;
			}

			var toDelete = new List<LocalizedProperty>();

			foreach (var prop in existingProperties)
			{
				// Check if the related entity exists. The user would not be able to solve an invalidated alias when the related entity does not exist anymore.
				var relatedEntityExists = true;

				if (prop.LocaleKeyGroup.IsCaseInsensitiveEqual("SpecificationAttribute"))
				{
					relatedEntityExists = _ctx.Resolve<IRepository<SpecificationAttribute>>().GetById(prop.EntityId) != null;
				}
				else if (prop.LocaleKeyGroup.IsCaseInsensitiveEqual("SpecificationAttributeOption"))
				{
					relatedEntityExists = _ctx.Resolve<IRepository<SpecificationAttributeOption>>().GetById(prop.EntityId) != null;
				}
				else if (prop.LocaleKeyGroup.IsCaseInsensitiveEqual("ProductAttribute"))
				{
					relatedEntityExists = _ctx.Resolve<IRepository<ProductAttribute>>().GetById(prop.EntityId) != null;
				}
				else if (prop.LocaleKeyGroup.IsCaseInsensitiveEqual("ProductAttributeOption"))
				{
					relatedEntityExists = _ctx.Resolve<IRepository<ProductAttributeOption>>().GetById(prop.EntityId) != null;
				}
				//else if (prop.LocaleKeyGroup.IsCaseInsensitiveEqual("ProductVariantAttributeValue"))
				//{
				//	relatedEntityExists = _ctx.Resolve<IRepository<ProductVariantAttributeValue>>().GetById(prop.EntityId) != null;
				//}

				if (relatedEntityExists)
				{
					// We cannot delete any localized property because we are going to throw duplicate alias exception in OnBeforeSaveCompleted.
					return true;
				}
				else
				{
					// Delete accidentally dead localized properties.
					toDelete.Add(prop);
				}
			}

			if (toDelete.Any())
			{
				try
				{
					_localizedPropertyRepository.Value.DeleteRange(toDelete);
				}
				catch (Exception exception)
				{
					exception.Dump();
				}
			}

			return false;
		}

		private bool HasEntityDuplicate<TEntity>(
			IHookedEntity entry,
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

		protected override void OnDeleting(BaseEntity entity, IHookedEntity entry)
		{
			HookObject(entity, entry);
		}

		protected override void OnInserting(BaseEntity entity, IHookedEntity entry)
		{
			HookObject(entity, entry);
		}

		protected override void OnUpdating(BaseEntity entity, IHookedEntity entry)
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

		private void HookObject(BaseEntity baseEntity, IHookedEntity entry)
		{
			var type = entry.EntityType;

			if (!_candidateTypes.Contains(type))
				throw new NotSupportedException();

			if (type == typeof(SpecificationAttribute))
			{
				if (HasAliasDuplicate<ProductAttribute, SpecificationAttribute>(entry, baseEntity))
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
				if (HasAliasDuplicate<ProductAttribute, SpecificationAttribute>(entry, baseEntity))
					return;

				if (IsPropertyModified(entry, "Alias"))
					_catalogSearchQueryAliasMapper.Value.ClearVariantCache();
			}
			else if (type == typeof(ProductAttributeOption))
			{
				var entity = (ProductAttributeOption)baseEntity;

				if (HasEntityDuplicate<ProductAttributeOption>(entry, baseEntity, x => x.Name,
					x => x.ProductAttributeOptionsSetId == entity.ProductAttributeOptionsSetId && x.Name == entity.Name))
					return;

				// ClearVariantCache() not necessary
				if (HasAliasDuplicate<ProductAttributeOption>(entry, baseEntity))
					return;
			}
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

				if (HasAliasDuplicate<ProductVariantAttributeValue>(entry, baseEntity, 
					(all, e) => all.Any(x => x.Id != e.Id && x.ProductVariantAttributeId == e.ProductVariantAttributeId && x.Alias == e.Alias)))
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

				// validating ProductVariantAttributeValue goes too far here.
				if (!keyGroup.IsCaseInsensitiveEqual("SpecificationAttribute") &&
					!keyGroup.IsCaseInsensitiveEqual("SpecificationAttributeOption") &&
					!keyGroup.IsCaseInsensitiveEqual("ProductAttribute") &&
					!keyGroup.IsCaseInsensitiveEqual("ProductAttributeOption"))
				{
					return;
				}

				// check alias duplicate
				if (entry.InitialState == EntityState.Added || entry.InitialState == EntityState.Modified)
				{
					prop.LocaleValue = SeoExtensions.GetSeName(prop.LocaleValue);
					if (prop.LocaleValue.HasValue() && HasAliasDuplicate(prop))
					{
						RevertChanges(entry, string.Concat(T("Common.Error.AliasAlreadyExists", prop.LocaleValue), " ", T("Common.Error.ChooseDifferentValue")));
						return;
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
