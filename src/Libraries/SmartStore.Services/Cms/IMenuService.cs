using SmartStore.Core;
using SmartStore.Core.Domain.Cms;

namespace SmartStore.Services.Cms
{
    public partial interface IMenuService
    {
        /// <summary>
        /// Inserts a menu.
        /// </summary>
        /// <param name="menu">Menu entity.</param>
        void InsertMenu(Menu menu);

        /// <summary>
        /// Updates a menu.
        /// </summary>
        /// <param name="menu">Menu entity.</param>
        void UpdateMenu(Menu menu);

        /// <summary>
        /// Deletes a menu.
        /// </summary>
        /// <param name="menu">Menu entity.</param>
        void DeleteMenu(Menu menu);

        /// <summary>
        /// Gets all menus.
        /// </summary>
        /// <param name="systemName">Menu system name.</param>
        /// <param name="storeId">Store identifier. 0 to get menus for all stores.</param>
        /// <param name="showHidden">Whether to get hidden menus.</param>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Menu entities.</returns>
        IPagedList<Menu> GetAllMenus(
            string systemName = null,
            int storeId = 0, 
            bool showHidden = false, 
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a menu by system name.
        /// </summary>
        /// <param name="systemName">Menu system name.</param>
        /// <param name="storeId">Store identifier.</param>
        /// <param name="checkPermission"></param>
        /// <param name="menu">Menu entity.</param>
        Menu GetMenuBySystemName(string systemName, int storeId = 0, bool checkPermission = true);
    }
}
