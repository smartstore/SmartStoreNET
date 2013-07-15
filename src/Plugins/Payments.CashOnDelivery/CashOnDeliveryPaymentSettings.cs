using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.Payments.CashOnDelivery
{
    public class CashOnDeliveryPaymentSettings : ISettings
    {
        public string DescriptionText { get; set; }
        public decimal AdditionalFee { get; set; }
		public bool AdditionalFeePercentage { get; set; }
    }
}
