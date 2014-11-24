using System.Web.Routing;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Configuration;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;
using SmartStore.Services;

namespace SmartStore.PayPal
{
	public class Plugin : BasePlugin
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
            var paypalExpressSettings = new PayPalExpressPaymentSettings()
            {
                UseSandbox = true,
                TransactMode = TransactMode.Authorize
            };
            _settingService.SaveSetting<PayPalExpressPaymentSettings>(paypalExpressSettings);

            var paypalDirectSettings = new PayPalDirectPaymentSettings()
            {
                TransactMode = TransactMode.Authorize,
                UseSandbox = true,
            };
            _settingService.SaveSetting<PayPalDirectPaymentSettings>(paypalDirectSettings);

            var paypalStandardSettings = new PayPalStandardPaymentSettings()
            {
                UseSandbox = true,
                PdtValidateOrderTotal = true,
                EnableIpn = true,
            };
            _settingService.SaveSetting<PayPalStandardPaymentSettings>(paypalStandardSettings);

			_localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

			base.Install();
		}

		public override void Uninstall()
		{
            _settingService.DeleteSetting<PayPalExpressPaymentSettings>();
            _settingService.DeleteSetting<PayPalDirectPaymentSettings>();
            _settingService.DeleteSetting<PayPalStandardPaymentSettings>();

			_localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

			base.Uninstall();
		}
	}
}
