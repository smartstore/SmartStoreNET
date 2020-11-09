using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Events;
using SmartStore.Services.Catalog;

namespace SmartStore.Web.Framework.UI.Menus.Providers
{
    /// <summary>
    /// Invalidates all menus that contain the <see cref="CatalogMenuProvider"/>
    /// </summary>
    public class CatalogMenuInvalidator : IConsumer
    {
        private readonly IMenuService _menuService;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICacheManager _cache;
        private readonly IRepository<MenuItemRecord> _rs;

        private List<string> _invalidated = new List<string>();
        private List<string> _countsResetted = new List<string>();

        public CatalogMenuInvalidator(
            IMenuService menuService,
            CatalogSettings catalogSettings,
            ICacheManager cache,
            IRepository<MenuItemRecord> rs)
        {
            _menuService = menuService;
            _catalogSettings = catalogSettings;
            _cache = cache;
            _rs = rs;
        }

        public async Task HandleAsync(CategoryTreeChangedEvent eventMessage)
        {
            var affectedMenuNames = await _rs.TableUntracked
                .Where(x => x.ProviderName == "catalog")
                .Select(x => x.Menu.SystemName)
                .Distinct()
                .ToListAsync();

            foreach (var menuName in affectedMenuNames)
            {
                var reason = eventMessage.Reason;

                if (reason == CategoryTreeChangeReason.ElementCounts)
                {
                    ResetElementCounts(menuName);
                }
                else
                {
                    Invalidate(menuName);
                }
            }
        }

        private void Invalidate(string menuName)
        {
            if (!_invalidated.Contains(menuName))
            {
                _menuService.GetMenu(menuName)?.ClearCache();
                _invalidated.Add(menuName);
            }
        }

        private void ResetElementCounts(string menuName)
        {
            if (!_countsResetted.Contains(menuName) && _catalogSettings.ShowCategoryProductNumber)
            {
                var allCachedMenus = _menuService.GetMenu(menuName)?.GetAllCachedMenus();
                if (allCachedMenus != null)
                {
                    foreach (var kvp in allCachedMenus)
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
                            _cache.Put(kvp.Key, kvp.Value);
                        }
                    }
                }

                _countsResetted.Add(menuName);
            }
        }
    }
}
