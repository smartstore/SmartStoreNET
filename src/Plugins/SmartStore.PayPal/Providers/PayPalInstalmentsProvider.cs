using System;
using Newtonsoft.Json;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Controllers;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal.Providers
{
    [DisplayOrder(1)]
    [SystemName("Payments.PayPalInstalments")]
    [FriendlyName("Ratenzahlung Powered by PayPal")]
    [DependentWidgets("Widgets.PayPal")]
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

        public override void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            var session = HttpContext.GetPayPalState(SystemName);

            var orderAttribute = new PayPalInstalmentsOrderAttribute
            {
                FinancingCosts = session.FinancingCosts,
                TotalInclFinancingCosts = session.TotalInclFinancingCosts
            };

            GenericAttributeService.SaveAttribute(order, PayPalInstalmentsOrderAttribute.Key, JsonConvert.SerializeObject(orderAttribute), order.StoreId);

            base.PostProcessPayment(postProcessPaymentRequest);
        }
    }


    [Serializable]
    public class PayPalInstalmentsOrderAttribute
    {
        [JsonIgnore]
        public static string Key => string.Concat(PayPalInstalmentsProvider.SystemName, ".OrderAttribute");

        public decimal FinancingCosts { get; set; }
        public decimal TotalInclFinancingCosts { get; set; }
    }
}