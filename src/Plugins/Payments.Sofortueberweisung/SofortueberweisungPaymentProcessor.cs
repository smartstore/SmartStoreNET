using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Services.Payments;
using SmartStore.Services.Orders;
using SmartStore.Plugin.Payments.Sofortueberweisung.Controllers;
using SmartStore.Services.Localization;
using SmartStore.Plugin.Payments.Sofortueberweisung.Core;
using SmartStore.Services.Configuration;
using System.Web;
using System.Configuration;

namespace SmartStore.Plugin.Payments.Sofortueberweisung
{
	public class SofortueberweisungPaymentProcessor : BasePlugin, IPaymentMethod
	{
		private readonly ISofortueberweisungApi _api;
		private readonly ILocalizationService _localizationService;
		private readonly ISettingService _settingService;
		private readonly SofortueberweisungPaymentSettings _paymentSettings;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
		private readonly HttpContextBase _httpContext;

		public SofortueberweisungPaymentProcessor(
			ISofortueberweisungApi api,
			ILocalizationService localizationService,
			ISettingService settingService,
			SofortueberweisungPaymentSettings paymentSettings,
			IOrderTotalCalculationService orderTotalCalculationService,
			HttpContextBase httpContext) {
			
			_api = api;
			_localizationService = localizationService;
			_settingService = settingService;
			_paymentSettings = paymentSettings;
			_orderTotalCalculationService = orderTotalCalculationService;
			_httpContext = httpContext;
		}

		public override void Install() {
			var paymentSettings = new SofortueberweisungPaymentSettings {
				ValidateOrderTotal = true,
				AccountHolder = "Max Mustermann",
				AccountNumber = "23456789",
				AccountBankCode = "00000",
				AccountCountry = "DE"
			};

			_settingService.SaveSetting<SofortueberweisungPaymentSettings>(paymentSettings);

			_localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

			base.Install();
		}
		public override void Uninstall() {
			_settingService.DeleteSetting<SofortueberweisungPaymentSettings>();

			_localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
			_localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Payments.Sofortueberweisung", false);

			base.Uninstall();
		}

		#region IPaymentMethod Member

		public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest) {
			var result = new ProcessPaymentResult();
			result.NewPaymentStatus = PaymentStatus.Pending;

			_api.PaymentProcess(result);

			return result;
		}
		public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest) {
			string transactionID, paymentUrl;

			if (_api.PaymentInitiate(postProcessPaymentRequest, out transactionID, out paymentUrl)) {
				_httpContext.Response.Redirect(paymentUrl);
			}
		}
		public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart) {
			var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart, _paymentSettings.AdditionalFee, _paymentSettings.AdditionalFeePercentage);
			return result;
		}
		public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest) {
			var result = new CapturePaymentResult();
			result.AddError("Capture method not supported");
			return result;
		}
		public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest) {
			var result = new RefundPaymentResult();
			result.AddError("Refund method not supported");
			return result;
		}
		public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest) {
			var result = new VoidPaymentResult();
			result.AddError("Void method not supported");
			return result;
		}
		public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest) {
			var result = new ProcessPaymentResult();
			result.AddError("Recurring payment not supported");
			return result;
		}
		public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest) {
			var result = new CancelRecurringPaymentResult();
			result.AddError("Recurring payment not supported");
			return result;
		}
		public bool CanRePostProcessPayment(Order order) {
			if (order == null)
				throw new ArgumentNullException("order");

			if (order.PaymentStatus == PaymentStatus.Pending) {
				if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes >= 1)
					return true;
			}
			return false;
		}
		public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues) {
			actionName = "Configure";
			controllerName = "PaymentSofortueberweisung";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Payments.Sofortueberweisung.Controllers" }, { "area", null } };
		}
		public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues) {
			actionName = "PaymentInfo";
			controllerName = "PaymentSofortueberweisung";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Payments.Sofortueberweisung.Controllers" }, { "area", null } };
		}
		public Type GetControllerType() {
			return typeof(PaymentSofortueberweisungController);
		}

		public bool SupportCapture { get { return false; } }
		public bool SupportPartiallyRefund { get { return false; } }
		public bool SupportRefund { get { return false; } }
		public bool SupportVoid { get { return false; } }
		public RecurringPaymentType RecurringPaymentType { get { return RecurringPaymentType.NotSupported; } }
		public PaymentMethodType PaymentMethodType { get { return PaymentMethodType.Redirection; } }

		#endregion
	}
}
