using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.GoogleAnalytics.Services;
using SmartStore.Services.Cms;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.GoogleAnalytics
{
    /// <summary>
    /// Google Analytics Plugin
    /// </summary>
    public class GoogleAnalyticPlugin : BasePlugin, IWidget, IConfigurable, ICookiePublisher
    {
		private readonly ISettingService _settingService;
		private readonly GoogleAnalyticsSettings _googleAnalyticsSettings;
		private readonly ILocalizationService _localizationService;

		public GoogleAnalyticPlugin(ISettingService settingService,
			GoogleAnalyticsSettings googleAnalyticsSettings,
			ILocalizationService localizationService)
		{
			_settingService = settingService;
			_googleAnalyticsSettings = googleAnalyticsSettings;
			_localizationService = localizationService;
		}

		/// <summary>
		/// Gets widget zones where this widget should be rendered
		/// </summary>
		/// <returns>Widget zones</returns>
		public IList<string> GetWidgetZones()
		{
			var zones = new List<string> { "head_html_tag" };
			if (!string.IsNullOrWhiteSpace(_googleAnalyticsSettings.WidgetZone))
			{
				zones = new List<string>
				{
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
		/// Gets CookieInfos for display in CookieManager dialog.
		/// </summary>
		/// <returns>CookieInfo containing plugin name, cookie purpose description & cookie type</returns>
		public CookieInfo GetCookieInfo()
		{
			var cookieInfo = new CookieInfo
			{
				Name = _localizationService.GetResource("Plugins.FriendlyName.SmartStore.GoogleAnalytics"),
				Description = _localizationService.GetResource("Plugins.Widgets.GoogleAnalytics.CookieInfo"),
				CookieType = CookieType.Analytics
			};

			return cookieInfo;
		}

		public override void Install()
		{
			var settings = new GoogleAnalyticsSettings
			{
				GoogleId = "UA-0000000-0",
				TrackingScript = TRACKING_SCRIPT,
				EcommerceScript = ECOMMERCE_SCRIPT,
				EcommerceDetailScript = ECOMMERCE_DETAIL_SCRIPT
			};

        public override void Install()
        {
            var settings = new GoogleAnalyticsSettings
            {
                GoogleId = "UA-0000000-0",
                TrackingScript = GoogleAnalyticsScriptHelper.GetTrackingScript(),
                EcommerceScript = GoogleAnalyticsScriptHelper.GetEcommerceScript(),
                EcommerceDetailScript = GoogleAnalyticsScriptHelper.GetEcommerceDetailScript()
            };

			base.Install();
		}

		public override void Uninstall()
		{
			_localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);
			_localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Widgets.GoogleAnalytics", false);
			_settingService.DeleteSetting<GoogleAnalyticsSettings>();

			base.Uninstall();
		}
	}
}
