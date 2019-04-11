using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Cms;
using SmartStore.Services.Cms;

namespace SmartStore.Web.Framework.UI
{
    public partial class MenuInvalidator : DbSaveHook<BaseEntity>
    {
        private readonly Lazy<IMenuService> _menuService;
        private readonly Lazy<IMenuStorage> _menuStorage;

        private static readonly HashSet<Type> _candidateTypes = new HashSet<Type>(new Type[]
        {
            typeof(MenuRecord),
            typeof(MenuItemRecord)
        });

        public MenuInvalidator(
            Lazy<IMenuService> menuService,
            Lazy<IMenuStorage> menuStorage)
        {
            _menuService = menuService;
            _menuStorage = menuStorage;
        }

        protected override void OnDeleted(BaseEntity entity, IHookedEntity entry)
        {
            HookObject(entity, entry);
        }

        protected override void OnInserted(BaseEntity entity, IHookedEntity entry)
        {
            HookObject(entity, entry);
        }

        protected override void OnUpdated(BaseEntity entity, IHookedEntity entry)
        {
            HookObject(entity, entry);
        }

        private void HookObject(BaseEntity entity, IHookedEntity entry)
        {
            var type = entry.EntityType;

            if (!_candidateTypes.Contains(type))
            {
                throw new NotSupportedException();
            }

            if (entry.Entity is MenuRecord mr)
            {
                _menuService.Value.ClearCache(mr.SystemName);
            }
            else if (entry.Entity is MenuItemRecord mir)
            {
                var menu = mir.Menu ?? _menuStorage.Value.GetMenuById(mir.MenuId);
                _menuService.Value.ClearCache(menu.SystemName);
            }
        }
    }
}
