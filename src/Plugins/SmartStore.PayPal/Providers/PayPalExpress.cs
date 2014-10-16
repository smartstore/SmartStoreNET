using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using SmartStore.PayPal.Controllers;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Directory;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Payments;
using System.Globalization;
using SmartStore.PayPal.PayPalSvc;

namespace SmartStore.PayPal
{
    [SystemName("Payments.PayPalExpress")]
    [FriendlyName("PayPal Express")]
    [DisplayOrder(0)]
    public partial class PayPalExpress : PaymentMethodBase, IConfigurable
    {
        private readonly ISettingService _settingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICommonServices _services;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IPayPalExpressApiService _apiService;
        
        public PayPalExpress(
            ISettingService settingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ICommonServices services,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            IPayPalExpressApiService apiService)
        {
            _settingService = settingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _services = services;
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _apiService = apiService;
        }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = _apiService.ProcessPayment(processPaymentRequest);
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public override void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //TODO:
            //handle Giropay

            //if(!String.IsNullOrEmpty(postProcessPaymentRequest.GiroPayUrl))
            //    return re

        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
        {
            var settings = _settingService.LoadSetting<PayPalExpressSettings>(_services.StoreContext.CurrentStore.Id);

            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                settings.AdditionalFee, settings.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public override CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = _apiService.Capture(capturePaymentRequest);
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public override RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = _apiService.Refund(refundPaymentRequest);
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public override VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = _apiService.Void(voidPaymentRequest);
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public override ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public override CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = _apiService.CancelRecurringPayment(cancelPaymentRequest);
            return result;
        }

        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PayPalExpress";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.PayPal" } };
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PayPalExpress";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.PayPal" } };
        }

        public override Type GetControllerType()
        {
            return typeof(PayPalExpressController);
        }

        public override bool SupportCapture
        {
            get { return true; }
        }

        public override bool SupportPartiallyRefund
        {
            get { return false; }
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
            get
            {
                return PaymentMethodType.StandardAndButton;
            }
        }

        public SetExpressCheckoutResponseType SetExpressCheckout(PayPalProcessPaymentRequest processPaymentRequest, IList<Core.Domain.Orders.OrganizedShoppingCartItem> cart) 
        {
            var result = _apiService.SetExpressCheckout(processPaymentRequest, cart);
            return result;
        }

        public GetExpressCheckoutDetailsResponseType GetExpressCheckoutDetails(string token)
        {
            var result = _apiService.GetExpressCheckoutDetails(token);
            return result;
        }

        public ProcessPaymentRequest SetCheckoutDetails(ProcessPaymentRequest processPaymentRequest, GetExpressCheckoutDetailsResponseDetailsType checkoutDetails)
        {
            var result = _apiService.SetCheckoutDetails(processPaymentRequest, checkoutDetails);
            return result;
        }

        public bool VerifyIPN(string formString, out Dictionary<string, string> values) 
        {
            var result = _apiService.VerifyIPN(formString, out values);
            return result;
        }
    }
}