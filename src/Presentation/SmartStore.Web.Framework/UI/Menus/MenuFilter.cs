using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Collections;
using SmartStore.Core.Themes;
using SmartStore.Services.Cms;
using SmartStore.Web.Framework.Theming;

namespace SmartStore.Web.Framework.UI
{
    public class MenuFilter : IActionFilter, IResultFilter
    {
		private readonly IWidgetProvider _widgetProvider;
		private readonly IMenuStorage _menuStorage;
        private readonly IMenuService _menuService;
        private readonly WebViewPageHelper _pageHelper;
        private readonly IPageAssetsBuilder _pageAssetsBuilder;

        public MenuFilter(
            IWidgetProvider widgetProvider, 
            IMenuStorage menuStorage, 
            IMenuService menuService,
            WebViewPageHelper pageHelper,
            IPageAssetsBuilder pageAssetsBuilder)
        {
            _widgetProvider = widgetProvider;
			_menuStorage = menuStorage;
            _menuService = menuService;
            _pageHelper = pageHelper;
            _pageAssetsBuilder = pageAssetsBuilder;
        }


        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Noop
        }

        /// <summary>
        /// Find the selected node in any registered menu
        /// </summary>
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            if (filterContext.HttpContext.Request.IsAjaxRequest())
                return;

            //if (filterContext.HttpContext.Request.HttpMethod != "GET")
            //	return;

            if (!filterContext.Result.IsHtmlViewResult())
                return;

            if (filterContext.RouteData.GetAreaName().IsCaseInsensitiveEqual("admin"))
                return;

            var selectedNode = ResolveCurrentNode(filterContext);

            if (selectedNode != null)
            {
                var httpContext = filterContext.HttpContext;

                // So that other actions/partials can access this.
                httpContext.Items["SelectedNode"] = selectedNode;

                // Add custom meta head part (mainly for client scripts)
                var nodeType = (selectedNode.Value.EntityName ?? _pageHelper.CurrentPageType).ToLowerInvariant();
                object nodeId = selectedNode.Id;
                if (_pageHelper.IsHomePage)
                {
                    nodeId = "home";
                }
                else if (nodeType == "system")
                {
                    nodeId = _pageHelper.CurrentPageId;
                }

                var nodeData = new
                {
                    type = nodeType,
                    id = nodeId,
                    menuItemId = selectedNode.Value.MenuItemId,
                    entityId = selectedNode.Value.EntityId
                };

                // Add it as JSON
                _pageAssetsBuilder.AddCustomHeadParts("<meta property='sm:currentpage' content='{0}' />".FormatInvariant(JsonConvert.SerializeObject(nodeData)));
            }
        }

        private TreeNode<MenuItem> ResolveCurrentNode(ActionExecutedContext filterContext)
        {
            // Ensure page helper is initialized
            _pageHelper.Initialize(filterContext);

            if (_pageHelper.IsHomePage)
            {
                return _menuService.GetRootNode("Main");
            }

            foreach (var menuName in _menuStorage.GetAllMenuSystemNames())
            {
                var selectedNode = _menuService.GetMenu(menuName)?.ResolveCurrentNode(filterContext);
                if (selectedNode != null)
                {
                    return selectedNode;
                }
            }

            return null;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
			if (filterContext.IsChildAction || !filterContext.Result.IsHtmlViewResult())
			{
				return;
			}

            ProcessUserMenus();
		}

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
            // Noop
        }

		/// <summary>
		/// Registers actions to render user menus in widget zones.
		/// </summary>
		private void ProcessUserMenus()
		{
			var menusInfo = _menuStorage.GetUserMenuInfos();

			foreach (var info in menusInfo)
			{
				_widgetProvider.RegisterAction(
					info.WidgetZones,
					"Menu",
					"Common",
					new { area = "", name = info.SystemName, template = info.Template },
					info.DisplayOrder);
			}
		}
    }
}
