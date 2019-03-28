using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Cms;
using SmartStore.Services;
using SmartStore.Services.Cms;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.UI
{
    /// <summary>
    /// A generic implementation of <see cref="IMenu" /> which represents a
    /// <see cref="MenuRecord"/> entity retrieved by <see cref="IMenuStorage"/>.
    /// </summary>
    internal class DatabaseMenu : MenuBase
    {
        private readonly IMenuStorage _menuStorage;
        private readonly DbQuerySettings _querySettings;
        private readonly IDictionary<string, Lazy<IMenuItemProvider, MenuItemMetadata>> _menuItemProviders;

        public DatabaseMenu(
            string menuName,
            ICommonServices services,
            IMenuStorage menuStorage,
            IMenuPublisher menuPublisher,
            DbQuerySettings querySettings,
            IEnumerable<Lazy<IMenuItemProvider, MenuItemMetadata>> menuItemProviders)
        {
            Guard.NotEmpty(menuName, nameof(menuName));

            Name = menuName;
            Services = services;
            MenuPublisher = menuPublisher;

            _menuStorage = menuStorage;
            _querySettings = querySettings;
            _menuItemProviders = menuItemProviders.ToDictionarySafe(x => x.Metadata.SystemName, x => x);
        }

        public override string Name { get; }

        public override bool ApplyPermissions => false;

        protected override TreeNode<MenuItem> Build()
        {
            var storeId = _querySettings.IgnoreMultiStore ? 0 : Services.StoreContext.CurrentStore.Id;
            var menu = _menuStorage.GetAllMenus(Name, storeId, false, true, 0, 1).FirstOrDefault();

            if (menu == null)
            {
                return new TreeNode<MenuItem>(new MenuItem { Text = Name });
            }

            var entities = _menuStorage.SortForTree(menu.Items, false);
            var root = new TreeNode<MenuItem>(new MenuItem { Text = menu.GetLocalized(x => x.Title) });
            var parent = root;
            MenuItemRecord prevItem = null;

            foreach (var entity in entities)
            {
                // Get parent.
                if (prevItem != null)
                {
                    if (entity.ParentItemId != parent.Value.EntityId)
                    {
                        if (entity.ParentItemId == prevItem.Id)
                        {
                            // Level +1.
                            parent = parent.LastChild;
                        }
                        else
                        {
                            // Level -x.
                            while (!parent.IsRoot)
                            {
                                if (parent.Value.EntityId == entity.ParentItemId)
                                {
                                    break;
                                }
                                parent = parent.Parent;
                            }
                        }
                    }
                }

                // Add to parent.
                if (entity.SystemName.HasValue() && _menuItemProviders.TryGetValue(entity.SystemName, out var provider))
                {
                    var providerBase = provider.Value as MenuItemProviderBase;
                    providerBase.Append(parent, entity);
                }
                else
                {
                    // TODO: Convert and append native items too (divider etc.)...
                    //menuItem.Attributes.Add("IsDivider", entity.IsDivider);
                }

                prevItem = entity;
            }

            return root;
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