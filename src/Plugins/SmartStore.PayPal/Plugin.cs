using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Cms;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Orders;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Order;
using SmartStore.Web.Models.ShoppingCart;

namespace SmartStore.PayPal
{
    [SystemName("Widgets.PayPal")]
    [FriendlyName("PayPal")]
    public class Plugin : BasePlugin, IWidget
    {
        private readonly ICommonServices _services;
		private readonly Lazy<IPayPalService> _payPalService;
        private readonly Lazy<IOrderTotalCalculationService> _orderTotalCalculationService;
        private readonly Lazy<ICurrencyService> _currencyService;

        public Plugin(
            ICommonServices services,
			Lazy<IPayPalService> payPalService,
            Lazy<IOrderTotalCalculationService> orderTotalCalculationService,
            Lazy<ICurrencyService> currencyService)
		{
            _services = services;
			_payPalService = payPalService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _currencyService = currencyService;

			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		public static string SystemName => "SmartStore.PayPal";

		public override void Install()
		{
            _services.Settings.SaveSetting(new PayPalExpressPaymentSettings());
            _services.Settings.SaveSetting(new PayPalDirectPaymentSettings());
            _services.Settings.SaveSetting(new PayPalStandardPaymentSettings());
            _services.Settings.SaveSetting(new PayPalPlusPaymentSettings());
            _services.Settings.SaveSetting(new PayPalInstalmentsSettings());

            _services.Localization.ImportPluginResourcesFromXml(this.PluginDescriptor);

			base.Install();
		}

		public override void Uninstall()
		{
            DeleteWebhook(_services.Settings.LoadSetting<PayPalPlusPaymentSettings>(), PayPalPlusProvider.SystemName);
            DeleteWebhook(_services.Settings.LoadSetting<PayPalInstalmentsSettings>(), PayPalInstalmentsProvider.SystemName);

            _services.Settings.DeleteSetting<PayPalExpressPaymentSettings>();
            _services.Settings.DeleteSetting<PayPalDirectPaymentSettings>();
            _services.Settings.DeleteSetting<PayPalStandardPaymentSettings>();
            _services.Settings.DeleteSetting<PayPalPlusPaymentSettings>();
            _services.Settings.DeleteSetting<PayPalInstalmentsSettings>();

            _services.Localization.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

			base.Uninstall();
		}

        public IList<string> GetWidgetZones()
        {
            return new List<string>
            {
                "productdetails_add_info",
                "order_summary_totals_after",
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

            if (widgetZone == "productdetails_add_info")
            {
                var viewModel = model as ProductDetailsModel;
                if (viewModel != null)
                {
                    actionName = "Promotion";
                    controllerName = "PayPalInstalments";

                    var price = viewModel.ProductPrice.PriceWithDiscountValue > decimal.Zero
                        ? viewModel.ProductPrice.PriceWithDiscountValue
                        : viewModel.ProductPrice.PriceValue;

                    // Convert price because it is in working currency.
                    price = _currencyService.Value.ConvertToPrimaryStoreCurrency(price, _services.WorkContext.WorkingCurrency);

                    routeValues.Add("isProductPage", true);
                    routeValues.Add("amount", price);
                }
            }
            else if (widgetZone == "order_summary_totals_after")
            {
                var viewModel = model as ShoppingCartModel;
                if (viewModel != null && viewModel.IsEditable)
                {
                    actionName = "Promotion";
                    controllerName = "PayPalInstalments";

                    var cart = _services.WorkContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _services.StoreContext.CurrentStore.Id);

                    _orderTotalCalculationService.Value.GetShoppingCartSubTotal(cart, out _, out _, out _, out var subTotalWithDiscountBase);

                    routeValues.Add("isProductPage", false);
                    routeValues.Add("amount", subTotalWithDiscountBase);
                }
            }
            else if (widgetZone == "orderdetails_page_aftertotal" || widgetZone == "invoice_aftertotal")
            {
                var viewModel = model as OrderDetailsModel;
                if (viewModel != null)
                {
                    actionName = "OrderDetailsTotal";
                    controllerName = "PayPalInstalments";

                    routeValues.Add("orderId", viewModel.Id);
                    routeValues.Add("print", widgetZone.IsCaseInsensitiveEqual("invoice_aftertotal"));
                }
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
