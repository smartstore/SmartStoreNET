using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.AmazonPay.Controllers;
using SmartStore.AmazonPay.Services;
using SmartStore.AmazonPay.Settings;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.AmazonPay
{
	[DependentWidgets("Widgets.AmazonPay")]
	public class Plugin : PaymentPluginBase, IConfigurable
	{
		private readonly IAmazonPayService _apiService;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
		private readonly ICommonServices _services;

		public Plugin(
			IAmazonPayService apiService,
			IOrderTotalCalculationService orderTotalCalculationService,
			ICommonServices services)
		{
			_apiService = apiService;
			_orderTotalCalculationService = orderTotalCalculationService;
			_services = services;
		}

		public override void Install()
		{
			_services.Settings.SaveSetting<AmazonPaySettings>(new AmazonPaySettings());

			_services.Localization.ImportPluginResourcesFromXml(this.PluginDescriptor);

			_apiService.DataPollingTaskInit();

			base.Install();
		}

		public override void Uninstall()
		{
			_apiService.DataPollingTaskDelete();

			_services.Settings.DeleteSetting<AmazonPaySettings>();

			_services.Localization.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);

			base.Uninstall();
		}

		public override PreProcessPaymentResult PreProcessPayment(ProcessPaymentRequest processPaymentRequest)
		{
			var result = _apiService.PreProcessPayment(processPaymentRequest);
			return result;
		}

		public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
		{
			var result = _apiService.ProcessPayment(processPaymentRequest);
			return result;
		}

		public override void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
		{
			_apiService.PostProcessPayment(postProcessPaymentRequest);
		}

		public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
		{
			var result = decimal.Zero;
			try
			{
				var settings = _services.Settings.LoadSetting<AmazonPaySettings>(_services.StoreContext.CurrentStore.Id);

				result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart, settings.AdditionalFee, settings.AdditionalFeePercentage);
			}
			catch (Exception exc)
			{
				_apiService.LogError(exc);
			}
			return result;

		}

		public override CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
		{
			var result = _apiService.Capture(capturePaymentRequest);
			return result;
		}

		public override RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
		{
			var result = _apiService.Refund(refundPaymentRequest);
			return result;
		}

		public override VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
		{
			var result = _apiService.Void(voidPaymentRequest);
			return result;
		}


		public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "Configure";
			controllerName = "AmazonPay";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.AmazonPay.Controllers" }, { "area", AmazonPayCore.SystemName } };
		}

		public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "ShoppingCart";
			controllerName = "AmazonPayShoppingCart";
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.AmazonPay.Controllers" }, { "area", AmazonPayCore.SystemName } };
		}

		public override Type GetControllerType()
		{
			return typeof(AmazonPayController);
		}

		public override bool SupportCapture
		{
			get { return true; }
		}

		public override bool SupportPartiallyRefund
		{
			get { return true; }
		}

		public override bool SupportRefund
		{
			get { return true; }
		}

		public override bool SupportVoid
		{
			get { return true; }
		}

		public override PaymentMethodType PaymentMethodType
		{
			get { return PaymentMethodType.StandardAndButton; }
		}
	}
}
