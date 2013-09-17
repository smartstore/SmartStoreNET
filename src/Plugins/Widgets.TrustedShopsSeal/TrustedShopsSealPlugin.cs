using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Cms;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.Plugin.Widgets.TrustedShopsSeal
{
    /// <summary>
    /// Trusted Shops seal provider
    /// </summary>
    public class TrustedShopsSealPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly TrustedShopsSealSettings _trustedShopsSealSettings;
        private readonly ILocalizationService _localizationService;
		private readonly ISettingService _settingService;

		public TrustedShopsSealPlugin(TrustedShopsSealSettings trustedShopsSealSettings, ILocalizationService localizationService, ISettingService settingService)
        {
            _trustedShopsSealSettings = trustedShopsSealSettings;
            _localizationService = localizationService;
			_settingService = settingService;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return !string.IsNullOrWhiteSpace(_trustedShopsSealSettings.WidgetZone)
                       ? new List<string>() { _trustedShopsSealSettings.WidgetZone }
                       : new List<string>() { "left_side_column_before" };
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "TrustedShopsSeal";
            routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Widgets.TrustedShopsSeal.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "TrustedShopsSeal";
            routeValues = new RouteValueDictionary()
            {
                {"Namespaces", "SmartStore.Plugin.Widgets.TrustedShopsSeal.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
        }
        
        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {            
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Widgets.TrustedShopsSeal", false);

			_settingService.DeleteSetting<TrustedShopsSealSettings>();

            base.Uninstall();
        }
    }
}
