using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Logging;
using SmartStore.Services;
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
        private readonly IMenuStorage _menuStorage;
        private readonly ILogger _logger;
        private readonly DbQuerySettings _querySettings;
        private readonly Lazy<CatalogSettings> _catalogSettings;
        private readonly IDictionary<string, Lazy<IMenuItemProvider, MenuItemProviderMetadata>> _menuItemProviders;

        public DatabaseMenu(
            string menuName,
            ICommonServices services,
            Lazy<ICatalogSearchService> catalogSearchService,
            IMenuStorage menuStorage,
            ILogger logger,
            IMenuPublisher menuPublisher,
            DbQuerySettings querySettings,
            Lazy<CatalogSettings> catalogSettings,
            IEnumerable<Lazy<IMenuItemProvider, MenuItemProviderMetadata>> menuItemProviders)
        {
            Guard.NotEmpty(menuName, nameof(menuName));

            Name = menuName;

            Services = services;
            _catalogSearchService = catalogSearchService;
            _menuStorage = menuStorage;
            _logger = logger;
            MenuPublisher = menuPublisher;
            _querySettings = querySettings;
            _catalogSettings = catalogSettings;
            _menuItemProviders = menuItemProviders.ToDictionarySafe(x => x.Metadata.ProviderName, x => x);
        }

        public override string Name { get; }

        public override bool ApplyPermissions => false;

        public override void ResolveElementCounts(TreeNode<MenuItem> curNode, bool deep = false)
        {
            if (curNode == null || !curNode.Value.EntityName.IsCaseInsensitiveEqual(nameof(Category)))
            {
                return;
            }

            try
            {
                Func<TreeNode<MenuItem>, bool> predicate = x => !x.Value.ElementsCount.HasValue && x.Value.EntityName.IsCaseInsensitiveEqual(nameof(Category));

                using (Services.Chronometer.Step($"DatabaseMenu.ResolveElementsCount() for {curNode.Value.Text.EmptyNull()}"))
                {
                    // Perf: only resolve counts for categories in the current path.
                    while (curNode != null)
                    {
                        if (curNode.Children.Any(predicate))
                        {
                            lock (s_lock)
                            {
                                if (curNode.Children.Any(predicate))
                                {
                                    var nodes = deep ? curNode.SelectNodes(x => true, false) : curNode.Children.AsEnumerable();
                                    nodes = nodes.Where(x => x.Value.EntityId != 0 && x.Value.EntityName.IsCaseInsensitiveEqual(nameof(Category)));

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