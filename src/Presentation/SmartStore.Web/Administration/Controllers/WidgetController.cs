using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Admin.Models.Cms;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Plugins;
using SmartStore.Services.Cms;
using SmartStore.Services.Configuration;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class WidgetController : AdminControllerBase
	{
		#region Fields

        private readonly IWidgetService _widgetService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly WidgetSettings _widgetSettings;
	    private readonly IPluginFinder _pluginFinder;
		private readonly PluginMediator _pluginMediator;

	    #endregion

		#region Constructors

        public WidgetController(
			IWidgetService widgetService,
            IPermissionService permissionService, 
			ISettingService settingService,
            WidgetSettings widgetSettings, 
			IPluginFinder pluginFinder,
			PluginMediator pluginMediator)
		{
            this._widgetService = widgetService;
            this._permissionService = permissionService;
            this._settingService = settingService;
            this._widgetSettings = widgetSettings;
            this._pluginFinder = pluginFinder;
			this._pluginMediator = pluginMediator;
		}

		#endregion 
        
        #region Methods
        
        public ActionResult Index()
        {
            return RedirectToAction("Providers");
        }

		public ActionResult Providers()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var widgetsModel = new List<WidgetModel>();
            var widgets = _widgetService.LoadAllWidgets();
            foreach (var widget in widgets)
            {
                var model = _pluginMediator.ToProviderModel<IWidget, WidgetModel>(widget);
                model.IsActive = widget.IsWidgetActive(_widgetSettings);
                widgetsModel.Add(model);
            }

			return View(widgetsModel);
        }

		public ActionResult ActivateProvider(string systemName, bool activate)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageWidgets))
				return AccessDeniedView();
			
			var widget = _widgetService.LoadWidgetBySystemName(systemName);
			if (widget.IsWidgetActive(_widgetSettings))
			{
				if (!activate)
				{
					// mark as disabled
					_widgetSettings.ActiveWidgetSystemNames.Remove(widget.Metadata.SystemName);
					_settingService.SaveSetting(_widgetSettings);
				}
			}
			else
			{
				if (activate)
				{
					// mark as active
					_widgetSettings.ActiveWidgetSystemNames.Add(widget.Metadata.SystemName);
					_settingService.SaveSetting(_widgetSettings);
				}
			}

			return RedirectToAction("Providers");
		}
        
        [ChildActionOnly]
        public ActionResult WidgetsByZone(string widgetZone)
        {
            var model = new List<RenderWidgetModel>();
 
            var widgets = _widgetService.LoadActiveWidgetsByWidgetZone(widgetZone);
            foreach (var widget in widgets)
            {
                var widgetModel = new RenderWidgetModel();
 
                string actionName;
                string controllerName;
                RouteValueDictionary routeValues;
                widget.Value.GetDisplayWidgetRoute(widgetZone, null, 0, out actionName, out controllerName, out routeValues);
                widgetModel.ActionName = actionName;
                widgetModel.ControllerName = controllerName;
                widgetModel.RouteValues = routeValues;

                model.Add(widgetModel);
            }
            return PartialView(model);
        }

        #endregion
    }
}
