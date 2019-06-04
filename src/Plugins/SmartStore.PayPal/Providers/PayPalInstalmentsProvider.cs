using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Controllers;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal.Providers
{
    [SystemName("Payments.PayPalInstalments")]
    [FriendlyName("Ratenzahlung Powered by PayPal")]
    [DisplayOrder(1)]
    public class PayPalInstalmentsProvider : PaymentMethodBase, IConfigurable
    {
        private readonly ICommonServices _services;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        public PayPalInstalmentsProvider(
            ICommonServices services,
            IOrderTotalCalculationService orderTotalCalculationService)
        {
            _services = services;
            _orderTotalCalculationService = orderTotalCalculationService;
        }

        public static string SystemName => "Payments.PayPalInstalments";

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.StandardAndRedirection;

        public override Type GetControllerType()
        {
            return typeof(PayPalInstalmentsController);
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PayPalInstalments";
            routeValues = new RouteValueDictionary { { "area", Plugin.SystemName } };
        }

        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PayPalInstalments";
            routeValues = new RouteValueDictionary { { "area", Plugin.SystemName } };
        }

        public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
        {
            var result = decimal.Zero;

            try
            {
                var settings = _services.Settings.LoadSetting<PayPalInstalmentsSettings>();

                result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart, settings.AdditionalFee, settings.AdditionalFeePercentage);
            }
            catch { }

            return result;
        }

        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }
    }
}