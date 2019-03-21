using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Menus;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Cms;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

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

            var model = new MenuRecordListModel
            {
                GridPageSize = _adminAreaSettings.GridPageSize,
                AvailableStores = Services.StoreService.GetAllStores().ToSelectListItems()
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command, MenuRecordListModel model)
        {
            var gridModel = new GridModel<MenuRecordModel>();

            if (Services.Permissions.Authorize(StandardPermissionProvider.ManageMenus))
            {
                var items = _menuService.GetAllMenus(model.SystemName, model.StoreId, true, command.Page - 1, command.PageSize);

                gridModel.Data = items.Select(x =>
                {
                    var itemModel = new MenuRecordModel();
                    MiniMapper.Map(x, itemModel);

                    return itemModel;
                });

                gridModel.Total = items.TotalCount;
            }
            else
            {
                gridModel.Data = Enumerable.Empty<MenuRecordModel>();
                NotifyAccessDenied();
            }

            return new JsonResult
            {
                MaxJsonLength = int.MaxValue,
                Data = gridModel
            };
        }
    }
}