using System;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.PayPal
{
	public class Plugin : BasePlugin
	{
		private readonly ISettingService _settingService;
		private readonly ILocalizationService _localizationService;
		private readonly Lazy<IPayPalService> _payPalService;

		public Plugin(
			ISettingService settingService,
			ILocalizationService localizationService,
			Lazy<IPayPalService> payPalService)
		{
			_settingService = settingService;
			_localizationService = localizationService;
			_payPalService = payPalService;

			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		public static string SystemName
		{
			get { return "SmartStore.PayPal"; }
		}

		public override void Install()
		{
			_settingService.SaveSetting<PayPalExpressPaymentSettings>(new PayPalExpressPaymentSettings());
			_settingService.SaveSetting<PayPalDirectPaymentSettings>(new PayPalDirectPaymentSettings());
			_settingService.SaveSetting<PayPalStandardPaymentSettings>(new PayPalStandardPaymentSettings());
			_settingService.SaveSetting<PayPalPlusPaymentSettings>(new PayPalPlusPaymentSettings());

			_localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

			base.Install();
		}

		public override void Uninstall()
		{
			try
			{
				var settings = _settingService.LoadSetting<PayPalPlusPaymentSettings>();
				if (settings.WebhookId.HasValue())
				{
					var session = new PayPalSessionData();
					var result = _payPalService.Value.EnsureAccessToken(session, settings);

					if (result.Success)
						result = _payPalService.Value.DeleteWebhook(settings, session);

					if (!result.Success)
						Logger.Log(LogLevel.Error, null, result.ErrorMessage, null);
				}
			}
			catch (Exception exception)
			{
				Logger.Log(LogLevel.Error, exception, null, null);
			}

            _settingService.DeleteSetting<PayPalExpressPaymentSettings>();
            _settingService.DeleteSetting<PayPalDirectPaymentSettings>();
            _settingService.DeleteSetting<PayPalStandardPaymentSettings>();
			_settingService.DeleteSetting<PayPalPlusPaymentSettings>();

			_localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

			base.Uninstall();
		}
	}
}
