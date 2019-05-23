using System;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Controllers;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal.Providers
{
    [SystemName("Payments.PayPalInstalments")]
    [FriendlyName("Ratenzahlung Powered by PayPal")]
    [DisplayOrder(1)]
    public class PayPalInstalmentsProvider : PaymentMethodBase
    {
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

        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            throw new NotImplementedException();
        }
    }
}