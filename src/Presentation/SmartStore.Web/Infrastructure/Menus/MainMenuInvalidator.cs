using System;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Events;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Infrastructure
{
    public class MainMenuInvalidator : IConsumer
    {
        private readonly ICommonServices _services;
        private readonly Lazy<IMenuService> _menuService;
        private readonly CatalogSettings _catalogSettings;

        private bool _invalidated;
        private bool _countsResetted = false;

        public MainMenuInvalidator(
            ICommonServices services,
            Lazy<IMenuService> menuService,
            CatalogSettings catalogSettings)
        {
            _services = services;
            _menuService = menuService;
            _catalogSettings = catalogSettings;
        }

        public void HandleEvent(CategoryTreeChangedEvent eventMessage)
        {
            if (eventMessage.Reason == CategoryTreeChangeReason.ElementCounts)
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
                _menuService.Value.ClearCache("Main");
                _invalidated = true;
            }
        }

        private void ResetElementCounts()
        {
            if (!_countsResetted && _catalogSettings.ShowCategoryProductNumber)
            {
                var menu = _menuService.Value.GetMenu("Main");
                if (menu != null)
                {
                    var allCachedMenus = menu.GetAllCachedMenus();
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
                            _services.Cache.Put(kvp.Key, kvp.Value);
                        }
                    }
                }

                _countsResetted = true;
            }
        }
    }
}