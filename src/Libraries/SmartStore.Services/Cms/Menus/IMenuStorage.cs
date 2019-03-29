using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Cms;

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

        /// <summary>
        /// Gets all menus.
        /// </summary>
        /// <param name="systemName">Menu system name.</param>
        /// <param name="storeId">Store identifier. 0 to get menus for all stores.</param>
        /// <param name="includeHidden">Whether to include hidden menus.</param>
        /// <param name="withItems">Whether to include menu items.</param>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Menu entities.</returns>
        IPagedList<MenuRecord> GetAllMenus(
            string systemName = null,
            int storeId = 0, 
            bool includeHidden = false,
            bool withItems = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a menu by system name.
        /// </summary>
        /// <param name="systemName">Menu system name.</param>
        /// <param name="storeId">Store identifier. 0 to get menus for all stores.</param>
        /// <param name="includeHidden">Whether to include hidden menus.</param>
        /// <param name="menu">Menu entity.</param>
        MenuRecord GetMenuBySystemName(
            string systemName,
            int storeId = 0,
            bool includeHidden = false);

        /// <summary>
        /// Gets a menu by identifier.
        /// </summary>
        /// <param name="id">Menu identifier.</param>
        /// <param name="withItems">Whether to include menu items.</param>
        /// <returns>Menu entity.</returns>
        MenuRecord GetMenuById(int id, bool withItems = false);

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
        /// <param name="withMenu">Whether to include menu and menu item entities.</param>
        /// <returns>Menu item entity.</returns>
        MenuItemRecord GetMenuItemById(int id, bool withMenu = false);

        /// <summary>
        /// Sort menu items for tree representation.
        /// </summary>
        /// <param name="items">Menu items.</param>
        /// <param name="includeItemsWithoutExistingParent">Whether to include menu items without existing parent menu item.</param>
        /// <returns></returns>
        IList<MenuItemRecord> SortForTree(IEnumerable<MenuItemRecord> items, bool includeItemsWithoutExistingParent = true);

        #endregion
    }
}
