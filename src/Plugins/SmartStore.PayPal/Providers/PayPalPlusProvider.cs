using System;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Controllers;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal
{
	[SystemName("Payments.PayPalPlus")]
    [FriendlyName("PayPal Plus")]
    [DisplayOrder(1)]
    public partial class PayPalPlusProvider : PayPalProviderBase<PayPalPlusPaymentSettings>
    {
        public PayPalPlusProvider()
        {
        }

		public static string SystemName
		{
			get { return "Payments.PayPalPlus"; }
		}

		public override PaymentMethodType PaymentMethodType
		{
			get
			{
				return PaymentMethodType.StandardAndRedirection;
			}
		}

		protected override string GetResourceRootKey()
		{
			return "Plugins.Payments.PayPalPlus";
		}

		protected override string GetControllerName()
		{
			return "PayPalPlus";
		}

		/// <summary>
		/// Process a payment
		/// </summary>
		/// <param name="processPaymentRequest">Payment info required for an order processing</param>
		/// <returns>Process payment result</returns>
		public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
			var result = new ProcessPaymentResult
			{
				NewPaymentStatus = PaymentStatus.Pending
			};

            return result;
        }

        public override Type GetControllerType()
        {
            return typeof(PayPalPlusController);
        }
    }
}