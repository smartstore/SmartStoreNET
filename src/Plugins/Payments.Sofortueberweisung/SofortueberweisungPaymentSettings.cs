using SmartStore.Core.Configuration;

namespace SmartStore.Plugin.Payments.Sofortueberweisung
{
	public class SofortueberweisungPaymentSettings : ISettings
	{
		public string ApiConfigKey { get; set; }
		public decimal AdditionalFee { get; set; }
		public bool AdditionalFeePercentage { get; set; }
		public bool ValidateOrderTotal { get; set; }
		public bool SaveWarnings { get; set; }
		public bool ShowFurtherInfo { get; set; }
		public string FurtherInfoUrl { get; set; }
		public bool CustomerProtection { get; set; }
		public string CustomerProtectionInfoUrl { get; set; }
		public string PaymentReason1 { get; set; }
		public string PaymentReason2 { get; set; }

		public bool UseTestAccount { get; set; }
		public string AccountHolder { get; set; }
		public string AccountNumber { get; set; }
		public string AccountBankCode { get; set; }
		public string AccountCountry { get; set; }
	}
}
