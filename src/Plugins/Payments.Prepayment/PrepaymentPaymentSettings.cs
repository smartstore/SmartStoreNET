using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.Payments.Prepayment
{
    public class PrepaymentPaymentSettings : ISettings
    {
        public string DescriptionText { get; set; }
        public decimal AdditionalFee { get; set; }
		public bool AdditionalFeePercentage { get; set; }
    }
}
