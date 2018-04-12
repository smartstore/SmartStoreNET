using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Services;
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

		const string SiteMapName = "catalog";

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
			get { return SiteMapName; }
		}

		public override bool ApplyPermissions
		{
			get { return false; }
		}

		protected override string GetCacheKey()
		{
			string cacheKey = "{0}-{1}-{2}".FormatInvariant(
				Services.WorkContext.WorkingLanguage.Id,
				Services.StoreContext.CurrentStore.Id,
				Services.WorkContext.CurrentCustomer.GetRolesIdent());

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
			var categoryTree = _categoryService.GetCategoryTree(0, false, Services.StoreContext.CurrentStore.Id);

			var allPictureIds = categoryTree.Flatten().Select(x => x.PictureId.GetValueOrDefault());
			var allPictureInfos = _pictureService.GetPictureInfos(allPictureIds);

			return ConvertNode(categoryTree.Root, allPictureInfos).Root;
		}

		private TreeNode<MenuItem> ConvertNode(TreeNode<ICategoryNode> node, IDictionary<int, PictureInfo> allPictureInfos)
		{
			var cat = node.Value;
			var name = cat.Id > 0 ? cat.GetLocalized(x => x.Name) : null;

			var menuItem = new MenuItem
			{
				EntityId = cat.Id,
				Text = name?.Value ?? cat.Name,
				Rtl = name?.CurrentLanguage?.Rtl ?? false,
				BadgeText = cat.Id > 0 ? cat.GetLocalized(x => x.BadgeText) : null,
				BadgeStyle = (BadgeStyle)cat.BadgeStyle,
				RouteName = cat.Id > 0 ? "Category" : "HomePage"
			};

			if (cat.Id > 0)
			{
				menuItem.RouteValues.Add("SeName", cat.GetSeName());

				if (cat.ParentCategoryId == 0 && cat.Published && cat.PictureId != null)
				{
					menuItem.ImageId = cat.PictureId;
					//menuItem.ImageUrl = _pictureService.GetUrl(allPictureInfos.Get(cat.PictureId.Value), 0, false);
				}
			}

			var convertedNode = new TreeNode<MenuItem>(menuItem);
			convertedNode.Id = node.Id;

			if (node.HasChildren)
			{
				foreach (var childNode in node.Children)
				{
					convertedNode.Append(ConvertNode(childNode, allPictureInfos));
				}
			}			

			return convertedNode;
		}
	}

	public class CatalogSiteMapInvalidationConsumer : IConsumer<CategoryTreeChangedEvent>
	{
		private readonly ISiteMap _siteMap;
		private readonly ICommonServices _services;
		private readonly CatalogSettings _catalogSettings;

		private bool _invalidated;
		private bool _countsResetted = false;

		public CatalogSiteMapInvalidationConsumer(
			ISiteMapService siteMapService,
			ICommonServices services,
			CatalogSettings catalogSettings)
		{
			_siteMap = siteMapService.GetSiteMap("catalog");
			_services = services;
			_catalogSettings = catalogSettings;
		}

		public void HandleEvent(CategoryTreeChangedEvent eventMessage)
		{
			var reason = eventMessage.Reason;

			if (reason == CategoryTreeChangeReason.ElementCounts)
			{
				ResetElementCounts();
			}
			else
			{
				Invalidate();
			}
		}

		private void Invalidate(bool condition = true)
		{
			if (condition && !_invalidated)
			{
				_siteMap.ClearCache();
				_invalidated = true;
			}
		}

		private void ResetElementCounts()
		{
			if (!_countsResetted && _catalogSettings.ShowCategoryProductNumber)
			{
				var allCachedTrees = _siteMap.GetAllCachedTrees();
				foreach (var kvp in allCachedTrees)
				{
					bool dirty = false;
					kvp.Value.Traverse(x =>
					{
						if (x.Value.ElementsCount.HasValue)
						{
							dirty = true;
							x.Value.ElementsCount = null;
						}
					}, true);

					if (dirty)
					{
						_services.Cache.Put(kvp.Key, kvp.Value);
					}
				}

				_countsResetted = true;
			}
		}
	}
}