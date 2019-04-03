using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Cms;
using SmartStore.Services;
using SmartStore.Services.Cms;

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
        private readonly IDictionary<string, Lazy<IMenuItemProvider, MenuItemProviderMetadata>> _menuItemProviders;

        public DatabaseMenu(
            string menuName,
            ICommonServices services,
            IMenuStorage menuStorage,
            IMenuPublisher menuPublisher,
            DbQuerySettings querySettings,
            IEnumerable<Lazy<IMenuItemProvider, MenuItemProviderMetadata>> menuItemProviders)
        {
            Guard.NotEmpty(menuName, nameof(menuName));

            Name = menuName;
            Services = services;
            MenuPublisher = menuPublisher;

            _menuStorage = menuStorage;
            _querySettings = querySettings;
            _menuItemProviders = menuItemProviders.ToDictionarySafe(x => x.Metadata.ProviderName, x => x);
        }

        public override string Name { get; }

        public override bool ApplyPermissions => false;

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