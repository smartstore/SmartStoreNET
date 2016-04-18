using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal
{
	public abstract class PayPalRestApiProviderBase<TSetting> : PaymentMethodBase, IConfigurable where TSetting : PayPalApiSettingsBase, ISettings, new()
    {
        protected PayPalRestApiProviderBase()
		{
			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }
		public ICommonServices Services { get; set; }
		public IOrderService OrderService { get; set; }
        public IOrderTotalCalculationService OrderTotalCalculationService { get; set; }

		protected string GetControllerName()
		{
			return GetControllerType().Name.EmptyNull().Replace("Controller", "");
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

		public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
        {
			var result = decimal.Zero;
			try
			{
				var settings = Services.Settings.LoadSetting<TSetting>();

				result = this.CalculateAdditionalFee(OrderTotalCalculationService, cart, settings.AdditionalFee, settings.AdditionalFeePercentage);
			}
			catch (Exception)
			{
			}
			return result;
        }

        public override CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
			var result = new CapturePaymentResult
			{
				NewPaymentStatus = capturePaymentRequest.Order.PaymentStatus
			};

			var settings = Services.Settings.LoadSetting<TSetting>(capturePaymentRequest.Order.StoreId);
			var currencyCode = Services.WorkContext.WorkingCurrency.CurrencyCode ?? "EUR";

			var authorizationId = capturePaymentRequest.Order.AuthorizationTransactionId;		
			
			// TODO	

            return result;
        }

        public override RefundPaymentResult Refund(RefundPaymentRequest request)
        {
			var result = new RefundPaymentResult
			{
				NewPaymentStatus = request.Order.PaymentStatus
			};

			var settings = Services.Settings.LoadSetting<TSetting>(request.Order.StoreId);

			var transactionId = request.Order.CaptureTransactionId;

			// TODO

			return result;
        }

        public override VoidPaymentResult Void(VoidPaymentRequest request)
        {
			var result = new VoidPaymentResult
			{
				NewPaymentStatus = request.Order.PaymentStatus
			};

			var settings = Services.Settings.LoadSetting<TSetting>(request.Order.StoreId);

			var transactionId = request.Order.AuthorizationTransactionId;

            if (transactionId.IsEmpty())
                transactionId = request.Order.CaptureTransactionId;

			// TODO

			return result;
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
			actionName = "Configure";
            controllerName = GetControllerName();
            routeValues = new RouteValueDictionary { { "area", "SmartStore.PayPal" } };
        }

        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = GetControllerName();
            routeValues = new RouteValueDictionary { { "area", "SmartStore.PayPal" } };
        }
    }
}

