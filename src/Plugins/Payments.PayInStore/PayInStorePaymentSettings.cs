using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.Payments.PayInStore
{
    public class PayInStorePaymentSettings : ISettings
    {
        public string DescriptionText { get; set; }
        public decimal AdditionalFee { get; set; }
		public bool AdditionalFeePercentage { get; set; }
    }
}
