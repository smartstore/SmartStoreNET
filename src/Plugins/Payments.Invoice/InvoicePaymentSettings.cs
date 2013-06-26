using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.Payments.Invoice
{
    public class InvoicePaymentSettings : ISettings
    {
        public string DescriptionText { get; set; }
        public decimal AdditionalFee { get; set; }
		public bool AdditionalFeePercentage { get; set; }
    }
}
