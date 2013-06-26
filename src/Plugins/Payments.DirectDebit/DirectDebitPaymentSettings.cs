using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.Payments.DirectDebit
{
    public class DirectDebitPaymentSettings : ISettings
    {
        public string DescriptionText { get; set; }
        public decimal AdditionalFee { get; set; }
		public bool AdditionalFeePercentage { get; set; }
    }
}
