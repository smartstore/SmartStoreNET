using System.Data.Entity.Migrations;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.GoogleMerchantCenter.Data.Migrations;
using SmartStore.GoogleMerchantCenter.Services;
using SmartStore.Services;

namespace SmartStore.GoogleMerchantCenter
{
    public class GoogleMerchantCenterFeedPlugin : BasePlugin, IConfigurable
    {
        private readonly IGoogleFeedService _googleFeedService;
		private readonly ICommonServices _services;

        public GoogleMerchantCenterFeedPlugin(
			IGoogleFeedService googleFeedService,
			ICommonServices services)
        {
            _googleFeedService = googleFeedService;
			_services = services;
        }

		public static string SystemName
		{
			get { return "SmartStore.GoogleMerchantCenter"; }
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
			controllerName = "FeedGoogleMerchantCenter";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.GoogleMerchantCenter.Controllers" }, { "area", SystemName } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
			_services.Localization.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
			_services.Localization.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

			var migrator = new DbMigrator(new Configuration());
			migrator.Update(DbMigrator.InitialDatabase);

            base.Uninstall();
        }
    }
}
