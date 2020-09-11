using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Logging;
using SmartStore.Core.Search;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Cms;
using SmartStore.Services.Search;

namespace SmartStore.Web.Framework.UI
{
    /// <summary>
    /// A generic implementation of <see cref="IMenu" /> which represents a
    /// <see cref="MenuRecord"/> entity retrieved by <see cref="IMenuStorage"/>.
    /// </summary>
    internal class DatabaseMenu : MenuBase
    {
        private static object s_lock = new object();

        private readonly Lazy<ICatalogSearchService> _catalogSearchService;
        private readonly Lazy<ICategoryService> _categoryService;
        private readonly IMenuStorage _menuStorage;
        private readonly ILogger _logger;
        private readonly DbQuerySettings _querySettings;
        private readonly Lazy<CatalogSettings> _catalogSettings;
        private readonly Lazy<SearchSettings> _searchSettings;
        private readonly IDictionary<string, Lazy<IMenuItemProvider, MenuItemProviderMetadata>> _menuItemProviders;

        public DatabaseMenu(
            string menuName,
            ICommonServices services,
            Lazy<ICatalogSearchService> catalogSearchService,
            Lazy<ICategoryService> categoryService,
            IMenuStorage menuStorage,
            ILogger logger,
            IMenuPublisher menuPublisher,
            DbQuerySettings querySettings,
            Lazy<CatalogSettings> catalogSettings,
            Lazy<SearchSettings> searchSettings,
            IEnumerable<Lazy<IMenuItemProvider, MenuItemProviderMetadata>> menuItemProviders)
        {
            Guard.NotEmpty(menuName, nameof(menuName));

            Name = menuName;

            Services = services;
            _catalogSearchService = catalogSearchService;
            _categoryService = categoryService;
            _menuStorage = menuStorage;
            _logger = logger;
            MenuPublisher = menuPublisher;
            _querySettings = querySettings;
            _catalogSettings = catalogSettings;
            _searchSettings = searchSettings;
            _menuItemProviders = menuItemProviders.ToDictionarySafe(x => x.Metadata.ProviderName, x => x);
        }

        public override string Name { get; }

        public override bool ApplyPermissions => true;

        public override void ResolveElementCounts(TreeNode<MenuItem> curNode, bool deep = false)
        {
            if (curNode == null || !ContainsProvider("catalog") || !_catalogSettings.Value.ShowCategoryProductNumber)
            {
                return;
            }

            try
            {
                using (Services.Chronometer.Step($"DatabaseMenu.ResolveElementsCount() for {curNode.Value.Text.NaIfEmpty()}"))
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
                                    nodes = nodes.Where(x => x.Value.EntityId != 0);

                                    foreach (var node in nodes)
                                    {
                                        var isCategory = node.Value.EntityName.IsCaseInsensitiveEqual(nameof(Category));
                                        var isManufacturer = node.Value.EntityName.IsCaseInsensitiveEqual(nameof(Manufacturer));

                                        if (isCategory || isManufacturer)
                                        {
                                            var entityIds = new HashSet<int>();
                                            if (isCategory && _catalogSettings.Value.ShowCategoryProductNumberIncludingSubcategories)
                                            {
                                                // Include sub-categories.
                                                node.Traverse(x =>
                                                {
                                                    entityIds.Add(x.Value.EntityId);
                                                }, true);
                                            }
                                            else
                                            {
                                                entityIds.Add(node.Value.EntityId);
                                            }

                                            var context = new CatalogSearchQuery()
                                                .VisibleOnly()
                                                .WithVisibility(ProductVisibility.Full)
                                                .HasStoreId(Services.StoreContext.CurrentStoreIdIfMultiStoreMode)
                                                .BuildFacetMap(false)
                                                .BuildHits(false);

                                            if (isCategory)
                                            {
                                                context = context.WithCategoryIds(null, entityIds.ToArray());
                                            }
                                            else
                                            {
                                                context = context.WithManufacturerIds(null, entityIds.ToArray());
                                            }

                                            if (!_searchSettings.Value.IncludeNotAvailable)
                                            {
                                                context = context.AvailableOnly(true);
                                            }

                                            node.Value.ElementsCount = _catalogSearchService.Value.Search(context).TotalHitsCount;
                                        }
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
                _logger.Error(ex);
            }
        }

        public override TreeNode<MenuItem> ResolveCurrentNode(ControllerContext context)
        {
            if (context == null || !ContainsProvider("catalog"))
            {
                return base.ResolveCurrentNode(context);
            }

            TreeNode<MenuItem> currentNode = null;

            try
            {
                var rootContext = context.GetRootControllerContext();

                int currentCategoryId = GetRequestValue<int?>(rootContext, "currentCategoryId") ?? GetRequestValue<int>(rootContext, "categoryId");
                int currentProductId = 0;

                if (currentCategoryId == 0)
                {
                    currentProductId = GetRequestValue<int?>(rootContext, "currentProductId") ?? GetRequestValue<int>(rootContext, "productId");
                }

                if (currentCategoryId == 0 && currentProductId == 0)
                {
                    // Possibly not a category node of a menu where the category tree is attached to.
                    return base.ResolveCurrentNode(rootContext);
                }

                var cacheKey = $"sm.temp.category.breadcrumb.{currentCategoryId}-{currentProductId}";
                currentNode = Services.RequestCache.Get(cacheKey, () =>
                {
                    var root = Root;
                    TreeNode<MenuItem> node = null;

                    if (currentCategoryId > 0)
                    {
                        node = root.SelectNodeById(currentCategoryId) ?? root.SelectNode(x => x.Value.EntityId == currentCategoryId);
                    }

                    if (node == null && currentProductId > 0)
                    {
                        var productCategories = _categoryService.Value.GetProductCategoriesByProductId(currentProductId);
                        if (productCategories.Any())
                        {
                            currentCategoryId = productCategories[0].Category.Id;
                            node = root.SelectNodeById(currentCategoryId) ?? root.SelectNode(x => x.Value.EntityId == currentCategoryId);
                        }
                    }

                    return node;
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return currentNode;
        }

        protected override TreeNode<MenuItem> Build()
        {
            var entities = _menuStorage.GetMenuItems(Name, Services.StoreContext.CurrentStore.Id);
            var tree = entities.GetTree("DatabaseMenu", _menuItemProviders);

            return tree;
        }

        protected override string GetCacheKey()
        {
            var cacheKey = "{0}-{1}-{2}".FormatInvariant(
                Services.WorkContext.WorkingLanguage.Id,
                _querySettings.IgnoreMultiStore ? 0 : Services.StoreContext.CurrentStore.Id,
                _querySettings.IgnoreAcl ? "0" : Services.WorkContext.CurrentCustomer.GetRolesIdent());

            return cacheKey;
        }
    }
}