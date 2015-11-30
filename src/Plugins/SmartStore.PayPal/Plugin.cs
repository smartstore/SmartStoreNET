using SmartStore.Core.Plugins;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.PayPal
{
	public class Plugin : BasePlugin
	{
		private readonly ISettingService _settingService;
		private readonly ILocalizationService _localizationService;

		public Plugin(
			ISettingService settingService,
			ILocalizationService localizationService)
		{
			_settingService = settingService;
			_localizationService = localizationService;
		}

		public override void Install()
		{
			_settingService.SaveSetting<PayPalExpressPaymentSettings>(new PayPalExpressPaymentSettings());
			_settingService.SaveSetting<PayPalDirectPaymentSettings>(new PayPalDirectPaymentSettings());
			_settingService.SaveSetting<PayPalStandardPaymentSettings>(new PayPalStandardPaymentSettings());

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
