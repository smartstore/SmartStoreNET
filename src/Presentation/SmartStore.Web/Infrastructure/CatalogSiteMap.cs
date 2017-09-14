using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Logging;
using SmartStore.Core.Search;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Search;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Infrastructure
{
	public class CatalogSiteMap : SiteMapBase
	{
		private static object s_lock = new object();

		private readonly ICategoryService _categoryService;
		private readonly IPictureService _pictureService;
		private readonly CatalogSettings _catalogSettings;
		private readonly ICatalogSearchService _catalogSearchService;

		public CatalogSiteMap(
			ICategoryService categoryService,
			IPictureService pictureService,
			CatalogSettings catalogSettings,
			ICatalogSearchService catalogSearchService)
		{
			_categoryService = categoryService;
			_pictureService = pictureService;
			_catalogSettings = catalogSettings;
			_catalogSearchService = catalogSearchService;

			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		public override string Name
		{
			get { return "catalog"; }
		}

		public override bool ApplyPermissions
		{
			get { return false; }
		}

		protected override string GetCacheKey()
		{
			var customerRolesIds = Services.WorkContext.CurrentCustomer.CustomerRoles.Where(cr => cr.Active).Select(cr => cr.Id).ToList();
			string cacheKey = "{0}-{1}-{2}".FormatInvariant(
				Services.WorkContext.WorkingLanguage.Id,
				Services.StoreContext.CurrentStore.Id,
				string.Join(",", customerRolesIds));

			return cacheKey;
		}

		public override void ResolveElementCounts(TreeNode<MenuItem> curNode, bool deep = false)
		{
			try
			{
				using (Services.Chronometer.Step("SiteMap.ResolveElementsCount() for {0}".FormatInvariant(curNode.Value.Text.EmptyNull())))
				{
					// Perf: only resolve counts for categories in the current path.
					while (curNode != null)
					{
						if (curNode.Children.Any(x => !x.Value.ElementsCount.HasValue))
						{
							lock (s_lock)
							{
								if (curNode.Children.Any(x => !x.Value.ElementsCount.HasValue))
								{
									var nodes = deep ? curNode.SelectNodes(x => true, false) : curNode.Children.AsEnumerable();

									foreach (var node in nodes)
									{
										var categoryIds = new List<int>();

										if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
										{
											// Include subcategories
											node.Traverse(x =>
											{
												categoryIds.Add(x.Value.EntityId);
											}, true);
										}
										else
										{
											categoryIds.Add(node.Value.EntityId);
										}

										var context = new CatalogSearchQuery()
											.VisibleOnly()
											.VisibleIndividuallyOnly(true)
											.WithCategoryIds(null, categoryIds.ToArray())
											.HasStoreId(Services.StoreContext.CurrentStoreIdIfMultiStoreMode)
											.BuildHits(false);

										node.Value.ElementsCount = _catalogSearchService.Search(context).TotalHitsCount;
									}
								}
							}
						}

						curNode = curNode.Parent;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}
		}

		protected override TreeNode<MenuItem> Build()
		{
			var curParent = new TreeNode<MenuItem>(new MenuItem
			{
				EntityId = 0,
				Text = "Home",
				RouteName = "HomePage"
			});

			Category prevCat = null;

			var categories = _categoryService.GetAllCategories(storeId: Services.StoreContext.CurrentStore.Id);

			foreach (var category in categories)
			{
				var menuItem = new MenuItem
				{
					EntityId = category.Id,
					Text = category.GetLocalized(x => x.Name),
					BadgeText = category.GetLocalized(x => x.BadgeText),
					BadgeStyle = (BadgeStyle)category.BadgeStyle,
					RouteName = "Category"
				};
				menuItem.RouteValues.Add("SeName", category.GetSeName());

				if (category.ParentCategoryId == 0 && category.Published && category.PictureId != null)
				{
					menuItem.ImageUrl = _pictureService.GetPictureUrl(category.PictureId.Value);
				}

				// Determine parent
				if (prevCat != null)
				{
					if (category.ParentCategoryId != curParent.Value.EntityId)
					{
						if (category.ParentCategoryId == prevCat.Id)
						{
							// level +1
							curParent = curParent.LastChild;
						}
						else
						{
							// level -x
							while (!curParent.IsRoot)
							{
								if (curParent.Value.EntityId == category.ParentCategoryId)
								{
									break;
								}
								curParent = curParent.Parent;
							}
						}
					}
				}

				// add to parent
				curParent.Append(menuItem);

				prevCat = category;
			}

			return curParent.Root;
		}
	}

	public class CatalogSiteMapCacheInvalidationHook : IDbSaveHook
	{
		private readonly ISiteMap _siteMap;
		private readonly IDbContext _dbContext;

		private bool _invalidated;

		private static readonly HashSet<string> _countAffectingProductProps = new HashSet<string>();

		static CatalogSiteMapCacheInvalidationHook()
		{
			AddCountAffectingProps(_countAffectingProductProps, 
				x => x.AvailableEndDateTimeUtc, 
				x => x.AvailableStartDateTimeUtc,
				x => x.Deleted,
				x => x.LowStockActivityId,
				x => x.LimitedToStores,
				x => x.ManageInventoryMethodId,
				x => x.MinStockQuantity,
				x => x.Published,
				x => x.SubjectToAcl,
				x => x.VisibleIndividually);
		}

		static void AddCountAffectingProps(HashSet<string> props, params Expression<Func<Product, object>>[] lambdas)
		{
			foreach (var lambda in lambdas)
			{
				props.Add(lambda.ExtractPropertyInfo().Name);
			}
		}

		public CatalogSiteMapCacheInvalidationHook(
			ISiteMapService siteMapService, 
			IDbContext dbContext)
		{
			_siteMap = siteMapService.GetSiteMap("catalog");
			_dbContext = dbContext;
		}

		public void OnBeforeSave(HookedEntity entry)
		{
			var entity = entry.Entity;

			if (entity is Product && entry.InitialState == EntityState.Modified)
			{
				var modProps = _dbContext.GetModifiedProperties(entity);

				if (modProps.Keys.Any(x => _countAffectingProductProps.Contains(x)))
				{
					Invalidate(true);
				}
			}
		}

		public void OnAfterSave(HookedEntity entry)
		{
			if (_invalidated)
			{
				// Don't bother processing.
				return;
			}
			
			// INFO: Acl & StoreMapping affect element counts

			var entity = entry.Entity;

			if (entity is Product)
			{
				if (entry.InitialState == EntityState.Added)
				{
					Invalidate(true);
				}
			}
			else if (entity is Category)
			{
				Invalidate();
			}
			else if (entity is Language || entity is CustomerRole)
			{
				InvalidateWhen(entry.InitialState == EntityState.Modified || entry.InitialState == EntityState.Deleted);
			}
			else if (entity is Setting)
			{
				var name = (entity as Setting).Name.ToLowerInvariant();
				InvalidateWhen(name == "catalogsettings.showcategoryproductnumber" || name == "catalogsettings.showcategoryproductnumberincludingsubcategories");
			}
			else if (entity is ProductCategory)
			{
				Invalidate(true);
			}
			else if (entity is AclRecord)
			{
				var acl = entity as AclRecord;
				if (!acl.IsIdle)
				{
					if (acl.EntityName == "Product")
					{
						Invalidate(true);
					}
					else if (acl.EntityName == "Category")
					{
						Invalidate(false);
					}
				}
			}
			else if (entity is StoreMapping)
			{
				var stm = entity as StoreMapping;
				if (stm.EntityName == "Product")
				{
					Invalidate(true);
				}
				else if (stm.EntityName == "Category")
				{
					Invalidate(false);
				}
			}
			else if (entity is LocalizedProperty)
			{
				var lp = entity as LocalizedProperty;
				var key = lp.LocaleKey;
				if (lp.LocaleKeyGroup == "Category" && (key == "Name" || key == "FullName" || key == "Description" || key == "BadgeText"))
				{
					Invalidate();
				}
			}
		}

		private void Invalidate(bool whenAnyNodeHasCount = false)
		{
			InvalidateWhen(!whenAnyNodeHasCount || _siteMap.Root.Flatten().Any(x => x.ElementsCount.HasValue));
		}

		private void InvalidateWhen(bool condition)
		{
			if (condition && !_invalidated)
			{
				_siteMap.ClearCache();
				_invalidated = true;
			}
		}

		public void OnBeforeSaveCompleted()
		{
		}

		public void OnAfterSaveCompleted()
		{
		}
	}
}