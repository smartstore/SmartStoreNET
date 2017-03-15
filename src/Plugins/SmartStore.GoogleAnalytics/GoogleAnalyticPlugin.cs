using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Cms;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.GoogleAnalytics
{
    /// <summary>
    /// Google Analytics Plugin
    /// </summary>
	public class GoogleAnalyticPlugin : BasePlugin, IWidget, IConfigurable
    {
        private readonly ISettingService _settingService;
        private readonly GoogleAnalyticsSettings _googleAnalyticsSettings;
        private readonly ILocalizationService _localizationService;

        public GoogleAnalyticPlugin(ISettingService settingService,
            GoogleAnalyticsSettings googleAnalyticsSettings,
            ILocalizationService localizationService)
        {
            this._settingService = settingService;
            this._googleAnalyticsSettings = googleAnalyticsSettings;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            var zones = new List<string>() { "head_html_tag" };
            if(!string.IsNullOrWhiteSpace(_googleAnalyticsSettings.WidgetZone))
            {
                zones = new List<string>() { 
                    _googleAnalyticsSettings.WidgetZone
                };
            }

            return zones;
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
            controllerName = "WidgetsGoogleAnalytics";
			routeValues = new RouteValueDictionary() { { "area", "SmartStore.GoogleAnalytics" } };
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
		public void GetDisplayWidgetRoute(string widgetZone, object model, int storeId, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "WidgetsGoogleAnalytics";
            routeValues = new RouteValueDictionary()
            {
                {"area", "SmartStore.GoogleAnalytics"},
                {"widgetZone", widgetZone}
            };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            var settings = new GoogleAnalyticsSettings()
            {
                GoogleId = "UA-0000000-0",
                TrackingScript = @"<!-- Google code for Analytics tracking -->
                    <script>
                        (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
                        (i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
                        m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
                        })(window,document,'script','//www.google-analytics.com/analytics.js','ga');

                        ga('create', '{GOOGLEID}', 'auto');
                        ga('send', 'pageview');

                        {ECOMMERCE}
                    </script>",
                EcommerceScript = @"
                    ga('require', 'ecommerce');

                    ga('ecommerce:addTransaction', {
                        'id': '{ORDERID}',
                        'affiliation': '{SITE}',
                        'revenue': '{TOTAL}',
                        'shipping': '{SHIP}',
                        'tax': '{TAX}',
                        'currency': '{CURRENCY}'
                    });

                    {DETAILS}

                    ga('ecommerce:send');
                    ",
                EcommerceDetailScript = @"
                    ga('ecommerce:addItem', {
                        'id': '{ORDERID}',
                        'name': '{PRODUCTNAME}',
                        'sku': '{PRODUCTSKU}',
                        'category': '{CATEGORYNAME}',
                        'price': '{UNITPRICE}',
                        'quantity': '{QUANTITY}'
                    });
                ",

            };
            _settingService.SaveSetting(settings);

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
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Widgets.GoogleAnalytics", false);

			_settingService.DeleteSetting<GoogleAnalyticsSettings>();
            
            base.Uninstall();
        }
    }
}
