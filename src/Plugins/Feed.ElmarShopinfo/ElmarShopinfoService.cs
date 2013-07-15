using System.Web;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Plugins;
using SmartStore.Plugin.Feed.ElmarShopinfo.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.Plugin.Feed.ElmarShopinfo
{
    public class ShopwahlService : BasePlugin, IMiscPlugin
    {
        private readonly ISettingService _settingService;
		private readonly IElmarShopinfoCoreService _elmarService;
		private readonly ILocalizationService _localizationService;

        public ShopwahlService(
            ISettingService settingService,
			IElmarShopinfoCoreService elmarService,
			ILocalizationService localizationService)
        {
            this._settingService = settingService;
			this._elmarService = elmarService;
			this._localizationService = localizationService;
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
            controllerName = "FeedElmarShopinfo";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Feed.ElmarShopinfo.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
			_localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

			var rnd = CommonHelper.GenerateRandomDigitCode(10);

			var settings = new ElmarShopinfoSettings() {
                ProductPictureSize = 300,
				StaticFileName = "elmar_shopinfo_{0}.csv".FormatWith(rnd),
				StaticFileNameXml = "elmar_shopinfo_{0}.xml".FormatWith(rnd),
				CurrencyId = _elmarService.Helper.CurrencyID,
				CategoryMapping = (_elmarService.Helper.IsLanguageGerman ? "Sonstiges" : "Other"),
				ExportSpecialPrice = true,
				UpdateDay = "daily"
            };
            _settingService.SaveSetting(settings);

			_elmarService.Helper.ScheduleTaskInsert();

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
			_settingService.DeleteSetting<ElmarShopinfoSettings>();

			_localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);

			_elmarService.Helper.ScheduleTaskDelete();

            base.Uninstall();
        }
    }	// class
}
