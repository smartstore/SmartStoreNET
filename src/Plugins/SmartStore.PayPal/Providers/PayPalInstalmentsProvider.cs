using System;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Controllers;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal.Providers
{
    [SystemName("Payments.PayPalInstalments")]
    [FriendlyName("Ratenzahlung Powered by PayPal")]
    [DisplayOrder(1)]
    public class PayPalInstalmentsProvider : PayPalRestApiProviderBase<PayPalInstalmentsSettings>
    {
        public PayPalInstalmentsProvider()
            : base(SystemName)
        {
        }

        public static string SystemName => "Payments.PayPalInstalments";

        public override PaymentMethodType PaymentMethodType => PaymentMethodType.StandardAndRedirection;

        public override Type GetControllerType()
        {
            return typeof(PayPalInstalmentsController);
        }
    }
}