using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Logging;
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
        private readonly HttpContextBase _httpContext;
        private readonly DbQuerySettings _querySettings;
        private readonly Lazy<CatalogSettings> _catalogSettings;
        private readonly IDictionary<string, Lazy<IMenuItemProvider, MenuItemProviderMetadata>> _menuItemProviders;

        public DatabaseMenu(
            string menuName,
            ICommonServices services,
            Lazy<ICatalogSearchService> catalogSearchService,
            Lazy<ICategoryService> categoryService,
            IMenuStorage menuStorage,
            ILogger logger,
            HttpContextBase httpContext,
            IMenuPublisher menuPublisher,
            DbQuerySettings querySettings,
            Lazy<CatalogSettings> catalogSettings,
            IEnumerable<Lazy<IMenuItemProvider, MenuItemProviderMetadata>> menuItemProviders)
        {
            Guard.NotEmpty(menuName, nameof(menuName));

            Name = menuName;

            Services = services;
            _catalogSearchService = catalogSearchService;
            _categoryService = categoryService;
            _menuStorage = menuStorage;
            _logger = logger;
            _httpContext = httpContext;
            MenuPublisher = menuPublisher;
            _querySettings = querySettings;
            _catalogSettings = catalogSettings;
            _menuItemProviders = menuItemProviders.ToDictionarySafe(x => x.Metadata.ProviderName, x => x);
        }

        public override string Name { get; }

        public override bool ApplyPermissions => false;

        public override void ResolveElementCounts(TreeNode<MenuItem> curNode, bool deep = false)
        {
            if (curNode == null || Name != "Main" || !_catalogSettings.Value.ShowCategoryProductNumber)
            {
                return;
            }

            try
            {
                using (Services.Chronometer.Step($"DatabaseMenu.ResolveElementsCount() for {curNode.Value.Text.EmptyNull()}"))
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
                                        var categoryIds = new HashSet<int>();

                                        if (_catalogSettings.Value.ShowCategoryProductNumberIncludingSubcategories)
                                        {
                                            // Include subcategories.
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
                                            .BuildFacetMap(false)
                                            .BuildHits(false);

                                        node.Value.ElementsCount = _catalogSearchService.Value.Search(context).TotalHitsCount;
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

        public override IList<TreeNode<MenuItem>> GetCurrentBreadcrumb()
        {
            if (Name != "Main" || !_httpContext.TryGetRouteData(out var rd))
            {
                return base.GetCurrentBreadcrumb();
            }

            var controller = rd.Values["controller"] as string;
            var action = rd.Values["action"] as string;            
            var currentCategoryId = 0;
            var currentProductId = 0;

            if (controller.IsCaseInsensitiveEqual("catalog") && action.IsCaseInsensitiveEqual("category"))
            {
                currentCategoryId = rd.Values["categoryId"]?.ToString()?.ToInt() ?? 0;
            }
            if (controller.IsCaseInsensitiveEqual("product") && action.IsCaseInsensitiveEqual("productdetails"))
            {
                currentProductId = rd.Values["productId"]?.ToString()?.ToInt() ?? 0;
            }

            if (currentCategoryId == 0 && currentProductId == 0)
            {
                return base.GetCurrentBreadcrumb();
            }

            var cacheKey = $"sm.temp.category.breadcrumb.{currentCategoryId}-{currentProductId}";
            var breadcrumb = Services.RequestCache.Get(cacheKey, () =>
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

                if (node != null)
                {
                    var path = node.GetBreadcrumb().ToList();
                    return path;
                }

                return new List<TreeNode<MenuItem>>();
            });

            return breadcrumb;
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