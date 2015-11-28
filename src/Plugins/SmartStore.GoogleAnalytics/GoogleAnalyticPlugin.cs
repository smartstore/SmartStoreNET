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
            var zones = new List<string>() { "head_html_tag", "mobile_head_html_tag" };
            if(!string.IsNullOrWhiteSpace(_googleAnalyticsSettings.WidgetZone))
            {
                zones = new List<string>() { 
                    _googleAnalyticsSettings.WidgetZone, 
                    _googleAnalyticsSettings.WidgetZone == "head_html_tag" ? "mobile_head_html_tag" : "mobile_body_end_html_tag_after"
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
                //TrackingScript = "<script type=\"text/javascript\"> var gaJsHost = ((\"https:\" == document.location.protocol) ? \"https://ssl.\" : \"http://www.\"); document.write(unescape(\"%3Cscript src='\" + gaJsHost + \"google-analytics.com/ga.js' type='text/javascript'%3E%3C/script%3E\")); </script> <script type=\"text/javascript\"> try { var pageTracker = _gat._getTracker(\"UA-0000000-0\"); pageTracker._trackPageview(); } catch(err) {}</script>",
                TrackingScript = @"<!-- Google code for Analytics tracking -->
                    <script type=""text/javascript"">
                    var _gaq = _gaq || [];
                    _gaq.push(['_setAccount', '{GOOGLEID}']);
                    _gaq.push(['_trackPageview']);
                    {ECOMMERCE}
                    (function() {
                        var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
                        ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
                        var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
                    })();
                    </script>",
                EcommerceScript = @"_gaq.push(['_addTrans', '{ORDERID}', '{SITE}', '{TOTAL}', '{TAX}', '{SHIP}', '{CITY}', '{STATEPROVINCE}', '{COUNTRY}']);
                    {DETAILS} 
                    _gaq.push(['_trackTrans']); ",
                EcommerceDetailScript = @"_gaq.push(['_addItem', '{ORDERID}', '{PRODUCTSKU}', '{PRODUCTNAME}', '{CATEGORYNAME}', '{UNITPRICE}', '{QUANTITY}' ]); ",

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
