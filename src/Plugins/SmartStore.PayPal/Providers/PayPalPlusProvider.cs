using System;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Controllers;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal
{
	[SystemName("Payments.PayPalPlus")]
    [FriendlyName("PayPal PLUS")]
    [DisplayOrder(1)]
    public partial class PayPalPlusProvider : PayPalRestApiProviderBase<PayPalPlusPaymentSettings>
    {
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

		public override Type GetControllerType()
		{
			return typeof(PayPalPlusController);
		}
	}
}