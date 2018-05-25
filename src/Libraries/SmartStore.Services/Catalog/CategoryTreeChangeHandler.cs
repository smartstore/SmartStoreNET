using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Catalog
{
	public enum CategoryTreeChangeReason
	{
		ElementCounts,
		Data,
		Localization,
		StoreMapping,
		Acl,
		Hierarchy
	}

	public class CategoryTreeChangedEvent
	{
		public CategoryTreeChangedEvent(CategoryTreeChangeReason reason)
		{
			Reason = reason;
		}

		public CategoryTreeChangeReason Reason { get; private set; }
	}

	public class CategoryTreeChangeHook : IDbSaveHook
	{
		private readonly ICommonServices _services;
		private readonly ICategoryService _categoryService;

		private readonly bool[] _handledReasons = new bool[(int)CategoryTreeChangeReason.Hierarchy + 1];
		private bool _invalidated;

		// Hierarchy affecting category prop names
		private static readonly string[] _h = new string[] { "ParentCategoryId", "Published", "Deleted", "DisplayOrder" };
		// Visibility affecting category prop names
		private static readonly string[] _a = new string[] { "LimitedToStores", "SubjectToAcl" };
		// Data affecting category prop names
		private static readonly string[] _d = new string[] { "Name", "Alias", "PictureId", "BadgeText", "BadgeStyle" };

		private static readonly HashSet<Type> _candidateTypes = new HashSet<Type>
		{
			typeof(Product),
			typeof(Category),
			typeof(ProductCategory),
			typeof(Setting),
			typeof(Language),
			typeof(LocalizedProperty),
			typeof(StoreMapping),
			typeof(AclRecord)
		};

		public CategoryTreeChangeHook(ICommonServices services, ICategoryService categoryService)
		{
			_services = services;
			_categoryService = categoryService;
		}

		public void OnBeforeSave(IHookedEntity entry)
		{
			if (_invalidated)
				return;

			if (entry.InitialState != EntityState.Modified)
				throw new NotSupportedException();

			var cache = _services.Cache;
			var entity = entry.Entity;
			
			if (entity is Product)
			{
				var modProps = _services.DbContext.GetModifiedProperties(entity);
				var toxicPropNames = Product.GetVisibilityAffectingPropertyNames();
				if (modProps.Keys.Any(x => toxicPropNames.Contains(x)))
				{
					// No eviction, just notification
					PublishEvent(CategoryTreeChangeReason.ElementCounts);
				}
			}
			else if (entity is ProductCategory)
			{
				var modProps = _services.DbContext.GetModifiedProperties(entity);
				if (modProps.ContainsKey("CategoryId"))
				{
					// No eviction, just notification
					PublishEvent(CategoryTreeChangeReason.ElementCounts);
				}
			}
			else if (entity is Category category)
			{
				var modProps = _services.DbContext.GetModifiedProperties(entity);

				if (modProps.Keys.Any(x => _h.Contains(x)))
				{
					// Hierarchy affecting properties has changed. Nuke every tree.
					cache.RemoveByPattern(CategoryService.CATEGORY_TREE_PATTERN_KEY);
					PublishEvent(CategoryTreeChangeReason.Hierarchy);
					_invalidated = true;
				}
				else if (modProps.Keys.Any(x => _a.Contains(x)))
				{
					if (modProps.ContainsKey("LimitedToStores"))
					{
						// Don't nuke store agnostic trees
						cache.RemoveByPattern(BuildCacheKeyPattern("*", "*", "[^0]*"));
						PublishEvent(CategoryTreeChangeReason.StoreMapping);
					}
					if (modProps.ContainsKey("SubjectToAcl"))
					{
						// Don't nuke ACL agnostic trees
						cache.RemoveByPattern(BuildCacheKeyPattern("*", "[^0]*", "*"));
						PublishEvent(CategoryTreeChangeReason.Acl);
					}
				}
				else if (modProps.Keys.Any(x => _d.Contains(x)))
				{
					// Only data has changed. Don't nuke trees, update corresponding cache entries instead.
					var keys = cache.Keys(CategoryService.CATEGORY_TREE_PATTERN_KEY).ToArray();
					foreach (var key in keys)
					{
						var tree = cache.Get<TreeNode<ICategoryNode>>(key);
						if (tree != null)
						{
							var node = tree.SelectNodeById(entity.Id);
							if (node != null)
							{
								var value = node.Value as CategoryNode;
								if (value == null)
								{
									// Cannot update. Nuke tree.
									cache.Remove(key);
								}
								else
								{
									value.Name = category.Name;
									value.Alias = category.Alias;
									value.PictureId = category.PictureId;
									value.BadgeText = category.BadgeText;
									value.BadgeStyle = category.BadgeStyle;

									// Persist to cache store
									cache.Put(key, tree, CategoryService.CategoryTreeCacheDuration);
								}
							}
						}
					}

					// Publish event only once
					PublishEvent(CategoryTreeChangeReason.Data);
				}
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		public void OnAfterSave(IHookedEntity entry)
		{
			if (_invalidated)
				return;

			if (!_candidateTypes.Contains(entry.EntityType))
				throw new NotSupportedException();

			// INFO: Acl & StoreMapping affect element counts

			var cache = _services.Cache;
			var isNewOrDeleted = entry.InitialState == EntityState.Added || entry.InitialState == EntityState.Deleted;
			var entity = entry.Entity;

			if (entity is Product)
			{
				// INFO: 'Modified' case already handled in 'OnBeforeSave()'
				if (entry.InitialState == EntityState.Deleted || (entry.InitialState == EntityState.Added && ((Product)entity).Published))
				{
					// No eviction, just notification, but for PUBLISHED products only
					PublishEvent(CategoryTreeChangeReason.ElementCounts);
				}
			}
			else if (entity is ProductCategory && isNewOrDeleted)
			{
				// INFO: 'Modified' case already handled in 'OnBeforeSave()'
				// New or deleted product category mappings affect counts
				PublishEvent(CategoryTreeChangeReason.ElementCounts);
			}
			else if (entity is Category && isNewOrDeleted)
			{
				// INFO: 'Modified' case already handled in 'OnBeforeSave()'
				// Hierarchy affecting change, nuke all.
				cache.RemoveByPattern(CategoryService.CATEGORY_TREE_PATTERN_KEY);
				PublishEvent(CategoryTreeChangeReason.Hierarchy);
				_invalidated = true;
			}
			else if (entity is Setting)
			{
				var name = (entity as Setting).Name.ToLowerInvariant();
				if (name == "catalogsettings.showcategoryproductnumber" || name == "catalogsettings.showcategoryproductnumberincludingsubcategories")
				{
					PublishEvent(CategoryTreeChangeReason.ElementCounts);
				}
			}
			else if (entity is Language && entry.InitialState == EntityState.Deleted)
			{
				PublishEvent(CategoryTreeChangeReason.Localization);
			}
			else if (entity is LocalizedProperty)
			{
				var lp = entity as LocalizedProperty;
				var key = lp.LocaleKey;
				if (lp.LocaleKeyGroup == "Category" && (key == "Name" || key == "BadgeText"))
				{
					PublishEvent(CategoryTreeChangeReason.Localization);
				}
			}
			else if (entity is StoreMapping)
			{
				var stm = entity as StoreMapping;
				if (stm.EntityName == "Product")
				{
					PublishEvent(CategoryTreeChangeReason.ElementCounts);
				}
				else if (stm.EntityName == "Category")
				{
					// Don't nuke store agnostic trees
					cache.RemoveByPattern(BuildCacheKeyPattern("*", "*", "[^0]*"));
					PublishEvent(CategoryTreeChangeReason.StoreMapping);
				}
			}
			else if (entity is AclRecord)
			{
				var acl = entity as AclRecord;
				if (!acl.IsIdle)
				{
					if (acl.EntityName == "Product")
					{
						PublishEvent(CategoryTreeChangeReason.ElementCounts);
					}
					else if (acl.EntityName == "Category")
					{
						// Don't nuke ACL agnostic trees
						cache.RemoveByPattern(BuildCacheKeyPattern("*", "[^0]*", "*"));
						PublishEvent(CategoryTreeChangeReason.Acl);
					}
				}
			}
		}

		private void PublishEvent(CategoryTreeChangeReason reason)
		{
			if (_handledReasons[(int)reason] == false)
			{
				_services.EventPublisher.Publish(new CategoryTreeChangedEvent(reason));
				_handledReasons[(int)reason] = true;
			}	
		}

		private string BuildCacheKeyPattern(string includeHiddenToken = "*", string rolesToken = "*", string storeToken = "*")
		{
			return CategoryService.CATEGORY_TREE_KEY.FormatInvariant(includeHiddenToken, rolesToken, storeToken);
		}

		public void OnBeforeSaveCompleted()
		{
		}

		public void OnAfterSaveCompleted()
		{
		}
	}
}
