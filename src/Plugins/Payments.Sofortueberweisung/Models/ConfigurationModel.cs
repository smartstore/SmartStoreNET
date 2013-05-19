using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using System.ComponentModel;

namespace SmartStore.Plugin.Payments.Sofortueberweisung.Models
{
	public class ConfigurationModel : ModelBase
	{
		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.ApiConfigKey")]
		public string ApiConfigKey { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.ValidateOrderTotal")]
		public bool ValidateOrderTotal { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.SaveWarnings")]
		public bool SaveWarnings { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.CustomerProtection")]
		public bool CustomerProtection { get; set; }


		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.UseTestAccount")]
		public bool UseTestAccount { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.AccountHolder")]
		public string AccountHolder { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.AccountNumber")]
		public string AccountNumber { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.AccountBankCode")]
		public string AccountBankCode { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.AccountCountry")]
		public string AccountCountry { get; set; }



		public void Copy(SofortueberweisungPaymentSettings settings, bool fromSettings) {
			if (fromSettings) {
				ApiConfigKey = settings.ApiConfigKey;
				AdditionalFee = settings.AdditionalFee;
				AdditionalFeePercentage = settings.AdditionalFeePercentage;
				ValidateOrderTotal = settings.ValidateOrderTotal;
				SaveWarnings = settings.SaveWarnings;
				CustomerProtection = settings.CustomerProtection;
				UseTestAccount = settings.UseTestAccount;
				AccountHolder = settings.AccountHolder;
				AccountNumber = settings.AccountNumber;
				AccountBankCode = settings.AccountBankCode;
				AccountCountry = settings.AccountCountry;
			}
			else {
				settings.ApiConfigKey = ApiConfigKey;
				settings.AdditionalFee = AdditionalFee;
				settings.AdditionalFeePercentage = AdditionalFeePercentage;
				settings.ValidateOrderTotal = ValidateOrderTotal;
				settings.SaveWarnings = SaveWarnings;
				settings.CustomerProtection = CustomerProtection;
				settings.UseTestAccount = UseTestAccount;
				settings.AccountHolder = AccountHolder;
				settings.AccountNumber = AccountNumber;
				settings.AccountBankCode = AccountBankCode;
				settings.AccountCountry = AccountCountry;
			}
		}
	}
}
