using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Catalog
{
	public enum CategoryTreeChangeReason
	{
		Category,
		Hierarchy,
		Localization,
		StoreMapping,
		Acl,
		ElementCounts
	}

	public class CategoryTreeChangedEvent
	{
		public CategoryTreeChangedEvent(CategoryTreeChangeReason reason, params string[] affectedFields)
		{
			Reason = reason;
			AffectedFields = affectedFields;
		}

		public CategoryTreeChangeReason Reason { get; private set; }
		public string[] AffectedFields { get; private set; }
	}

	public class CategoryTreeChangeHook : IDbSaveHook
	{
		private readonly ICommonServices _services;
		private readonly ICategoryService _categoryService;

		private readonly bool[] _handledReasons = new bool[(int)CategoryTreeChangeReason.ElementCounts - 1];
		//private bool _invalidated;

		private static readonly HashSet<string> _hierarchyAffectingCategoryProps = new HashSet<string>
		{
			"Deleted",
			"Published",
			"ParentCategoryId",
			"DisplayOrder",
			"SubjectToAcl",
			"LimitedToStores"
		};

		public CategoryTreeChangeHook(ICommonServices services, ICategoryService categoryService)
		{
			_services = services;
			_categoryService = categoryService;
		}

		public void OnBeforeSave(HookedEntity entry)
		{
			var entity = entry.Entity;

			if (entity is Category && entry.InitialState == EntityState.Modified)
			{
				var modProps = _services.DbContext.GetModifiedProperties(entity);

				if (modProps.Keys.Any(x => _hierarchyAffectingCategoryProps.Contains(x)))
				{
					Invalidate();
				}
			}
		}

		public void OnAfterSave(HookedEntity entry)
		{
			//if (_invalidated)
			//{
			//	// Don't bother processing.
			//	return;
			//}

			// INFO: Acl & StoreMapping affect element counts

			var entity = entry.Entity;

			if (entity is Category)
			{
				Invalidate();
			}
			//else if (entity is CustomerRole)
			//{
			//	Invalidate(entry.InitialState == EntityState.Modified || entry.InitialState == EntityState.Deleted);
			//}
			//else if (entity is AclRecord)
			//{
			//	var acl = entity as AclRecord;
			//	if (!acl.IsIdle)
			//	{
			//		if (acl.EntityName == "Category")
			//		{
			//			Invalidate();
			//		}
			//	}
			//}
			else if (entity is StoreMapping)
			{
				var stm = entity as StoreMapping;
				if (stm.EntityName == "Category")
				{
					Invalidate("*", "*", "[^0]*");
					PublishEvent(CategoryTreeChangeReason.StoreMapping);
				}
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
		}

		private void PublishEvent(CategoryTreeChangeReason reason, params string[] affectedFields)
		{
			if (_handledReasons[(int)reason] == false)
			{
				_services.EventPublisher.Publish(new CategoryTreeChangedEvent(reason, affectedFields));
				_handledReasons[(int)reason] = true;
			}	
		}

		private void Invalidate(params string[] tokens)
		{
			_services.Cache.RemoveByPattern(CategoryService.CATEGORY_TREE_KEY.FormatInvariant(tokens));
			//_invalidated = true;
		}

		public void OnBeforeSaveCompleted()
		{
		}

		public void OnAfterSaveCompleted()
		{
		}
	}
}
