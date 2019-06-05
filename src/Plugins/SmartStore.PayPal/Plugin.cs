using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Cms;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Web.Models.Order;

namespace SmartStore.PayPal
{
    [SystemName("Widgets.PayPal")]
    [FriendlyName("PayPal")]
    public class Plugin : BasePlugin, IWidget
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

		public static string SystemName => "SmartStore.PayPal";

		public override void Install()
		{
			_settingService.SaveSetting(new PayPalExpressPaymentSettings());
			_settingService.SaveSetting(new PayPalDirectPaymentSettings());
			_settingService.SaveSetting(new PayPalStandardPaymentSettings());
			_settingService.SaveSetting(new PayPalPlusPaymentSettings());
            _settingService.SaveSetting(new PayPalInstalmentsSettings());

			_localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

			base.Install();
		}

		public override void Uninstall()
		{
            DeleteWebhook(_settingService.LoadSetting<PayPalPlusPaymentSettings>(), PayPalPlusProvider.SystemName);
            DeleteWebhook(_settingService.LoadSetting<PayPalInstalmentsSettings>(), PayPalInstalmentsProvider.SystemName);

            _settingService.DeleteSetting<PayPalExpressPaymentSettings>();
            _settingService.DeleteSetting<PayPalDirectPaymentSettings>();
            _settingService.DeleteSetting<PayPalStandardPaymentSettings>();
			_settingService.DeleteSetting<PayPalPlusPaymentSettings>();
            _settingService.DeleteSetting<PayPalInstalmentsSettings>();

            _localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

			base.Uninstall();
		}

        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                "orderdetails_page_aftertotal",
                "invoice_aftertotal"
            };
        }

        public void GetDisplayWidgetRoute(string widgetZone, object model, int storeId, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = controllerName = null;
            routeValues = new RouteValueDictionary
            {
                { "Namespaces", "SmartStore.PayPal.Controllers" },
                { "area", SystemName }
            };

            if (widgetZone.IsCaseInsensitiveEqual("orderdetails_page_aftertotal") || widgetZone.IsCaseInsensitiveEqual("invoice_aftertotal"))
            {
                actionName = "OrderDetails";
                controllerName = "PayPalInstalments";

                var orderId = 0;
                var print = widgetZone.IsCaseInsensitiveEqual("invoice_aftertotal");
                var viewModel = model as OrderDetailsModel;
                if (viewModel != null)
                {
                    orderId = viewModel.Id;
                }

                routeValues.Add(nameof(orderId), orderId);
                routeValues.Add(nameof(print), print);
            }
        }

        private void DeleteWebhook(PayPalApiSettingsBase settings, string providerSystemName)
        {
            try
            {
                if (settings?.WebhookId.HasValue() ?? false)
                {
                    var session = new PayPalSessionData { ProviderSystemName = providerSystemName };
                    var result = _payPalService.Value.EnsureAccessToken(session, settings);

                    if (result.Success)
                    {
                        result = _payPalService.Value.DeleteWebhook(settings, session);
                    }

                    if (!result.Success)
                    {
                        Logger.Log(LogLevel.Error, null, result.ErrorMessage, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, null, null);
            }
        }
	}
}
