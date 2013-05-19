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
		public bool CustomerProtection { get; set; }

		public bool UseTestAccount { get; set; }
		public string AccountHolder { get; set; }
		public string AccountNumber { get; set; }
		public string AccountBankCode { get; set; }
		public string AccountCountry { get; set; }
	}
}
