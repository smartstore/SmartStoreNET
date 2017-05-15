using System;
using System.Data.Entity.Migrations;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Plugins;
using SmartStore.Shipping.Data;
using SmartStore.Shipping.Data.Migrations;
using SmartStore.Shipping.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Shipping;
using SmartStore.Services.Shipping.Tracking;

namespace SmartStore.Shipping
{
	public class Plugin : BasePlugin
    {
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ByTotalObjectContext _objectContext;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="logger">Logger</param>
        /// /// <param name="settingService">Settings service</param>
        /// /// <param name="settingService">Localization service</param>
        public Plugin(ILogger logger,
            ISettingService settingService,
            ILocalizationService localizationService,
            ByTotalObjectContext objectContext)
        {
            this._logger = logger;
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._objectContext = objectContext;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {            
            var shippingByTotalSettings = new ShippingByTotalSettings()
            {
                LimitMethodsToCreated = false,
                SmallQuantityThreshold = 0,
                SmallQuantitySurcharge = 0
            };
            _settingService.SaveSetting(shippingByTotalSettings);

            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();

            _logger.Info(string.Format("Plugin installed: SystemName: {0}, Version: {1}, Description: '{2}'", PluginDescriptor.SystemName, PluginDescriptor.Version, PluginDescriptor.FriendlyName));
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            _settingService.DeleteSetting<ShippingByTotalSettings>();

			_localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

			var migrator = new DbMigrator(new Configuration());
			migrator.Update(DbMigrator.InitialDatabase);

            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Shipping.FixedRateShipping", false);

            base.Uninstall();
        }
    }
}
