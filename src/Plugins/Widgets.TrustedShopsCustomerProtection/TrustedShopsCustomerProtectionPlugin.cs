using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Cms;
using SmartStore.Services.Localization;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection
{
    /// <summary>
    /// Trusted Shops seal provider
    /// </summary>
    public class TrustedShopsCustomerProtectionPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly TrustedShopsCustomerProtectionSettings _TrustedShopsCustomerProtectionSettings;
        private readonly ILocalizationService _localizationService;

        public TrustedShopsCustomerProtectionPlugin(TrustedShopsCustomerProtectionSettings TrustedShopsCustomerProtectionSettings, ILocalizationService localizationService)
        {
            _TrustedShopsCustomerProtectionSettings = TrustedShopsCustomerProtectionSettings;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string>() { 
                "op_checkout_payment_method_bottom", 
                "checkout_payment_method_bottom",
                "op_checkout_confirm_bottom", 
                "order_summary_content_before"
            };
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
            controllerName = "TrustedShopsCustomerProtection";
            routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Controllers" }, { "area", null } };
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
            controllerName = "TrustedShopsCustomerProtection";
            routeValues = new RouteValueDictionary()
            {
                {"Namespaces", "SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Controllers"},
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

            //add trusted shops products 


            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Widgets.TrustedShopsCustomerProtection", false);

            base.Uninstall();
        }

    }
}
