using System.Web.Routing;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Configuration;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;
using SmartStore.Services;

namespace SmartStore.PayPal
{
	public class Plugin : BasePlugin, IConfigurable
	{
		private readonly ISettingService _settingService;
		private readonly ILocalizationService _localizationService;
        private readonly ICommonServices _services;

		public Plugin(
			ISettingService settingService,
			ILocalizationService localizationService,
            ICommonServices services)
		{
			_settingService = settingService;
			_localizationService = localizationService;
            _services = services;
		}

		public override void Install()
		{
            var paypalExpressSettings = new PayPalExpressSettings()
            {
                UseSandbox = true,
                TransactMode = TransactMode.Authorize
            };

            _settingService.SaveSetting<PayPalExpressSettings>(paypalExpressSettings);

            var paypalDirectSettings = new PayPalDirectSettings()
            {
                TransactMode = TransactMode.Authorize,
                UseSandbox = true,
            };
            _settingService.SaveSetting<PayPalDirectSettings>(paypalDirectSettings);

            var paypalStandardSettings = new PayPalStandardSettings()
            {
                UseSandbox = true,
                PdtValidateOrderTotal = true,
                EnableIpn = true,
            };
            _settingService.SaveSetting<PayPalStandardSettings>(paypalStandardSettings);

			_localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

			base.Install();
		}

		public override void Uninstall()
		{
            _settingService.DeleteSetting<PayPalExpressSettings>();
            _settingService.DeleteSetting<PayPalDirectSettings>();
            _settingService.DeleteSetting<PayPalStandardSettings>();

			_localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

			base.Uninstall();
		}


		public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "Configure";
			controllerName = "PayPalExpress";
            routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.PayPal.Controllers" }, { "area", "SmartStore.PayPal" } };
		}
	}
}
