using System;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Controllers
{
    public partial class MenuController : PublicControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly IBreadcrumb _breadcrumb;
        private readonly CatalogSettings _catalogSettings;
        private readonly CatalogHelper _catalogHelper;

        public MenuController(
            IMenuService menuService,
            IBreadcrumb breadcrumb,
            CatalogSettings catalogSettings,
            CatalogHelper catalogHelper)
        {
            _menuService = menuService;
            _breadcrumb = breadcrumb;
            _catalogSettings = catalogSettings;
            _catalogHelper = catalogHelper;
        }

        [ChildActionOnly]
        public ActionResult Menu(string name, string template = null)
        {
            var menu = _menuService.GetMenu(name);

            if (menu == null)
                return new EmptyResult();

            var model = menu.CreateModel(template, ControllerContext);

            return Menu(model);
        }

        [ChildActionOnly, ActionName("MenuFromModel")]
        public ActionResult Menu(MenuModel model)
        {
            Guard.NotNull(model, nameof(model));

            var viewName = (model.Template ?? model.Name);
            if (viewName[0] != '~' && !viewName.StartsWith("Menus/", StringComparison.OrdinalIgnoreCase))
            {
                viewName = "Menus/" + viewName;
            }

            return this.RootActionPartialView(viewName, model);
        }

        [ChildActionOnly]
        public ActionResult Breadcrumb()
        {
            if (_breadcrumb.Trail == null || _breadcrumb.Trail.Count == 0)
            {
                return new EmptyResult();
            }

            return PartialView(_breadcrumb.Trail);
        }

        #region OffCanvasMenu 

        /// <summary>
        /// Called by AJAX to get the an OffCanvas layer (either the root "home" or a "submenu" layer)
        /// </summary>
        /// <param name="currentNodeId">Id of currently selected node/page from sm:pagedata meta tag</param>
        /// <param name="targetNodeId">Id of the parent node to which should be navigated in the OffCanvasMenu (actually the node which was clicked)</param>
        [HttpPost]
        public ActionResult OffCanvas(string currentNodeId, string targetNodeId)
        {
            bool allowNavigation = Services.Permissions.Authorize(Permissions.System.AccessShop);

            ViewBag.AllowNavigation = allowNavigation;
            ViewBag.ShowNodes = allowNavigation;
            ViewBag.ShowBrands = allowNavigation
                && _catalogSettings.ShowManufacturersInOffCanvas == true
                && _catalogSettings.ManufacturerItemsToDisplayInOffcanvasMenu > 0;

            if (!allowNavigation)
            {
                return PartialView("OffCanvas.Home", null);
            }

            var model = PrepareMenuModel(currentNodeId, targetNodeId);
            if (model == null)
            {
                return new EmptyResult();
            }

            var selectedNode = model.SelectedNode;
            var isHomeLayer = selectedNode.IsRoot || (selectedNode.Depth == 1 && selectedNode.IsLeaf);

            var templateName = isHomeLayer
                // Render home layer, if parent node is either home or a direct child of home
                ? "OffCanvas.Home"
                // Render a submenu
                : "OffCanvas.Menu";

            return PartialView(templateName, model);
        }

        /// <summary>
        /// Prepares the menu for given parent node 'targetNodeId' with current node 'currentNodeId'
        /// </summary>
        /// <param name="currentNodeId">Id of currently selected node/page from sm:pagedata meta tag.</param>
        /// <param name="targetNodeId">Id of the parent node to which should be navigated in the OffCanvasMenu (actually the node which was clicked).</param>
        [NonAction]
        protected MenuModel PrepareMenuModel(string currentNodeId, string targetNodeId)
        {
            var menu = _menuService.GetMenu("Main");
            if (menu == null)
            {
                return null;
            }

            object nodeId = ConvertNodeId(targetNodeId);

            var model = new MenuModel
            {
                Name = "offcanvas",
                Root = menu.Root,
                SelectedNode = IsNullNode(nodeId)
                    ? menu.Root
                    : menu.Root.SelectNodeById(nodeId)
            };
            menu.ResolveElementCounts(model.SelectedNode, false);

            if (currentNodeId == targetNodeId)
            {
                ViewBag.CurrentNode = model.SelectedNode;
            }
            else
            {
                nodeId = ConvertNodeId(currentNodeId);
                if (!IsNullNode(nodeId))
                {
                    ViewBag.CurrentNode = menu.Root.SelectNodeById(nodeId);
                }
            }

            return model;
        }

        [HttpPost]
        public ActionResult OffCanvasBrands()
        {
            var model = _catalogHelper.PrepareManufacturerNavigationModel(_catalogSettings.ManufacturerItemsToDisplayInOffcanvasMenu);
            return PartialView("OffCanvas.Brands", model);
        }

        #endregion

        #region Utils

        private static object ConvertNodeId(string source)
        {
            int? intId = int.TryParse(source, out var id) ? id : (int?)null;
            return (intId.HasValue ? (object)intId.Value : (object)source);
        }

        private static bool IsNullNode(object source)
        {
            return source == null || object.Equals(0, source) || object.Equals(string.Empty, source);
        }

        #endregion
    }
}