using System.Data.Entity.Migrations;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.GoogleMerchantCenter.Data.Migrations;
using SmartStore.GoogleMerchantCenter.Services;
using SmartStore.Services;

namespace SmartStore.GoogleMerchantCenter
{
    public class FroogleFeedPlugin : BasePlugin, IConfigurable
    {
        private readonly IGoogleFeedService _googleService;
		private readonly ICommonServices _services;

        public FroogleFeedPlugin(
			IGoogleFeedService googleService,
			ICommonServices services)
        {
            _googleService = googleService;
			_services = services;
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
            controllerName = "FeedFroogle";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.GoogleMerchantCenter.Controllers" }, { "area", "SmartStore.GoogleMerchantCenter" } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
			var settings = new FroogleSettings();
			settings.CurrencyId = _services.StoreContext.CurrentStore.PrimaryStoreCurrencyId;

			_services.Settings.SaveSetting(settings);

			_services.Localization.ImportPluginResourcesFromXml(this.PluginDescriptor);

		 	_googleService.Helper.InsertScheduleTask();

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
			_googleService.Helper.DeleteFeedFiles();
			_googleService.Helper.DeleteScheduleTask();

			_services.Settings.DeleteSetting<FroogleSettings>();

			_services.Localization.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

			var migrator = new DbMigrator(new Configuration());
			migrator.Update(DbMigrator.InitialDatabase);

            base.Uninstall();
        }
    }
}
