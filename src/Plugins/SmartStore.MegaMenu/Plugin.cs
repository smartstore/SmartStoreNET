using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Configuration;
using SmartStore.MegaMenu.Settings;
using SmartStore.Services.Cms;

namespace SmartStore.MegaMenu
{

    public class Plugin : BasePlugin, IConfigurable
    {
        private readonly ISettingService _settingService;
        private readonly ICommonServices _services;

        public Plugin(ISettingService settingService,
            ICommonServices services)
        {
            _settingService = settingService;
            _services = services;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "MegaMenu";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.MegaMenu" } };
        }
        
        public override void Install()
        {
            _services.Settings.SaveSetting<MegaMenuSettings>(new MegaMenuSettings());
            _services.Localization.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }

        public override void Uninstall()
        {
            _services.Settings.DeleteSetting<MegaMenuSettings>();
            _services.Localization.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);

            base.Uninstall();
        }

    }
}
