using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Menus;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Cms;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class MenuController : AdminControllerBase
    {
        private readonly IMenuManager _menuService;
        private readonly AdminAreaSettings _adminAreaSettings;

        public MenuController(
            IMenuManager menuService,
            AdminAreaSettings adminAreaSettings)
        {
            _menuService = menuService;
            _adminAreaSettings = adminAreaSettings;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                return AccessDeniedView();
            }

            var stores = Services.StoreService.GetAllStores();
            var model = new MenuRecordListModel
            {
                GridPageSize = _adminAreaSettings.GridPageSize
            };

            model.AvailableStores = stores
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToList();

            return View(model);
        }
    }
}