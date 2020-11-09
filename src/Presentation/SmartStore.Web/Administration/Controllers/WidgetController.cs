using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Admin.Models.Cms;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Security;
using SmartStore.Services.Cms;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class WidgetController : AdminControllerBase
    {
        private readonly IWidgetService _widgetService;
        private readonly WidgetSettings _widgetSettings;
        private readonly PluginMediator _pluginMediator;

        public WidgetController(
            IWidgetService widgetService,
            WidgetSettings widgetSettings,
            PluginMediator pluginMediator)
        {
            _widgetService = widgetService;
            _widgetSettings = widgetSettings;
            _pluginMediator = pluginMediator;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Providers");
        }

        [Permission(Permissions.Cms.Widget.Read)]
        public ActionResult Providers()
        {
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Widget.Activate)]
        public ActionResult ActivateProvider(string systemName, bool activate)
        {
            var widget = _widgetService.LoadWidgetBySystemName(systemName);
            if (widget.IsWidgetActive(_widgetSettings))
            {
                if (!activate)
                {
                    // Mark as disabled.
                    _widgetSettings.ActiveWidgetSystemNames.Remove(widget.Metadata.SystemName);
                    Services.Settings.SaveSetting(_widgetSettings);
                }
            }
            else
            {
                if (activate)
                {
                    // Mark as active.
                    _widgetSettings.ActiveWidgetSystemNames.Add(widget.Metadata.SystemName);
                    Services.Settings.SaveSetting(_widgetSettings);
                }
            }

            return RedirectToAction("Providers");
        }
    }
}
