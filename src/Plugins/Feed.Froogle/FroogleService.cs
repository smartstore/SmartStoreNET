using System.Data.Entity.Migrations;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Plugin.Feed.Froogle.Data;
using SmartStore.Plugin.Feed.Froogle.Data.Migrations;
using SmartStore.Plugin.Feed.Froogle.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.Plugin.Feed.Froogle
{
    public class FroogleService : BasePlugin, IMiscPlugin
    {
        private readonly IGoogleService _googleService;
        private readonly ISettingService _settingService;
        private readonly GoogleProductObjectContext _objectContext;
		private readonly ILocalizationService _localizationService;

        public FroogleService(
			IGoogleService googleService,
            ISettingService settingService,
            GoogleProductObjectContext objectContext,
			ILocalizationService localizationService)
        {
            this._googleService = googleService;
            this._settingService = settingService;
            this._objectContext = objectContext;
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
            controllerName = "FeedFroogle";
            routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Feed.Froogle.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
			var settings = new FroogleSettings();
			settings.CurrencyId = _googleService.Helper.CurrencyID;

            _settingService.SaveSetting(settings);

			_localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

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

            _settingService.DeleteSetting<FroogleSettings>();

			_localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);

			var migrator = new DbMigrator(new Configuration());
			migrator.Update(DbMigrator.InitialDatabase);

            base.Uninstall();
        }
    }
}
