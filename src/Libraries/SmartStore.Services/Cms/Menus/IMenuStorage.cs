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
        /// <param name="showHidden">Whether to get hidden menus.</param>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Menu entities.</returns>
        IPagedList<MenuRecord> GetAllMenus(
            string systemName = null,
            int storeId = 0, 
            bool showHidden = false, 
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a menu by identifier.
        /// </summary>
        /// <param name="id">Menu identifier.</param>
        /// <returns>Menu entity.</returns>
        MenuRecord GetMenuById(int id);

        /// <summary>
        /// Gets a menu by system name.
        /// </summary>
        /// <param name="systemName">Menu system name.</param>
        /// <param name="storeId">Store identifier.</param>
        /// <param name="checkPermission"></param>
        /// <returns>Menu entity.</returns>
        MenuRecord GetMenuBySystemName(string systemName, int storeId = 0, bool checkPermission = true);
    }
}
