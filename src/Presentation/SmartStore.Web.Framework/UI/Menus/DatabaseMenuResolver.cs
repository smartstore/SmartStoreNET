using System;
using System.Collections.Generic;
using SmartStore.Core.Data;
using SmartStore.Services;
using SmartStore.Services.Cms;

namespace SmartStore.Web.Framework.UI
{
    public class DatabaseMenuResolver : IMenuResolver
	{
        protected readonly ICommonServices _services;
        protected readonly IMenuStorage _menuStorage;
        protected readonly IMenuPublisher _menuPublisher;
        protected readonly IEnumerable<Lazy<IMenuItemProvider, MenuItemMetadata>> _menuItemProviders;

        public DatabaseMenuResolver(
            ICommonServices services,
            IMenuStorage menuStorage,
            IMenuPublisher menuPublisher,
            IEnumerable<Lazy<IMenuItemProvider, MenuItemMetadata>> menuItemProviders)
        {
            _services = services;
            _menuStorage = menuStorage;
            _menuPublisher = menuPublisher;
            _menuItemProviders = menuItemProviders;

            QuerySettings = DbQuerySettings.Default;
        }

        public DbQuerySettings QuerySettings { get; set; }

        public int Order => 1;

        public bool Exists(string menuName)
		{
            return _menuStorage.MenuExists(menuName);
		}

		public IMenu Resolve(string name)
		{
            return new DatabaseMenu(name, 
				_services, 
				_menuStorage, 
				_menuPublisher, 
				QuerySettings, 
				_menuItemProviders);
		}
	}
}
