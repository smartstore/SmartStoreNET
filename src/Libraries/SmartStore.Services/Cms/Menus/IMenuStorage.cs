using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Cms
{
    public partial interface IMenuStorage
    {
        /// <summary>
        /// Inserts a menu.
        /// </summary>
        /// <param name="menu">Menu entity.</param>
        void InsertMenu(MenuRecord menu);

        /// <summary>
        /// Updates a menu.
        /// </summary>
        /// <param name="menu">Menu entity.</param>
        void UpdateMenu(MenuRecord menu);

        /// <summary>
        /// Deletes a menu.
        /// </summary>
        /// <param name="menu">Menu entity.</param>
        void DeleteMenu(MenuRecord menu);

        // Gets the system names of all published menus
        IEnumerable<string> GetAllMenuSystemNames();

        /// <summary>
        /// Gets all menus.
        /// </summary>
        /// <param name="systemName">Menu system name.</param>
        /// <param name="storeId">Store identifier. 0 to get menus for all stores.</param>
        /// <param name="includeHidden">Whether to include hidden menus.</param>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Menu entities.</returns>
        IPagedList<MenuRecord> GetAllMenus(
            string systemName = null,
            int storeId = 0,
            bool includeHidden = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        /// <summary>
        /// Gets cached infos about all user menus.
        /// </summary>
		/// <param name="roles">Customer roles to check access for. <c>null</c> to use current customer's roles.</param>
		/// <param name="storeId">Store identifier. 0 to use current store.</param>
        /// <returns>Menu infos.</returns>
        IEnumerable<MenuInfo> GetUserMenuInfos(IEnumerable<CustomerRole> roles = null, int storeId = 0);

        /// <summary>
        /// Gets a menu by identifier.
        /// </summary>
        /// <param name="id">Menu identifier.</param>
        /// <returns>Menu item entity.</returns>
        MenuRecord GetMenuById(int id);

        /// <summary>
        /// Checks whether the menu exists.
        /// </summary>
        /// <param name="systemName">Menu system name.</param>
        /// <returns><c>true</c> the menu exists, <c>false</c> the menu doesn't exist.</returns>
        bool MenuExists(string systemName);

        #region Menu items

        /// <summary>
        /// Inserts a menu item.
        /// </summary>
        /// <param name="item">Menu item entity.</param>
        void InsertMenuItem(MenuItemRecord item);

        /// <summary>
        /// Updates a menu item.
        /// </summary>
        /// <param name="item">Menu item entity.</param>
        void UpdateMenuItem(MenuItemRecord item);

        /// <summary>
        /// Deletes a menu item.
        /// </summary>
        /// <param name="item">Menu item entity.</param>
        /// <param name="deleteChilds">Whether to delete all child items too.</param>
        void DeleteMenuItem(MenuItemRecord item, bool deleteChilds = true);

        /// <summary>
        /// Gets a menu item by identifier.
        /// </summary>
        /// <param name="id">Menu item identifier.</param>
        /// <returns>Menu item entity.</returns>
        MenuItemRecord GetMenuItemById(int id);

        /// <summary>
        /// Gets menu items.
        /// </summary>
        /// <param name="menuId">Menu item identifier.</param>
        /// <param name="storeId">Store identifier. 0 to get menus for all stores.</param>
        /// <param name="includeHidden">Whether to include hidden menus.</param>
        /// <returns>Menu item entities.</returns>
        IList<MenuItemRecord> GetMenuItems(int menuId, int storeId = 0, bool includeHidden = false);

        /// <summary>
        /// Gets menu items.
        /// </summary>
        /// <param name="systemName">Menu system name.</param>
        /// <param name="storeId">Store identifier. 0 to get menus for all stores.</param>
        /// <param name="includeHidden">Whether to include hidden menus.</param>
        /// <returns>Menu item entities.</returns>
        IList<MenuItemRecord> GetMenuItems(string systemName, int storeId = 0, bool includeHidden = false);

        #endregion
    }
}
