using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

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

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.ShowFurtherInfo")]
		public bool ShowFurtherInfo { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.FurtherInfoUrl")]
		public string FurtherInfoUrl { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.CustomerProtection")]
		public bool CustomerProtection { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.CustomerProtectionInfoUrl")]
		public string CustomerProtectionInfoUrl { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.PaymentReason1")]
		public string PaymentReason1 { get; set; }
		[SmartResourceDisplayName("Plugins.Payments.Sofortueberweisung.PaymentReason2")]
		public string PaymentReason2 { get; set; }
		public List<SelectListItem> AvailablePaymentReason { get; set; }

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



		public void Copy(SofortueberweisungPaymentSettings settings, bool fromSettings)
		{
			if (fromSettings)
			{
				ApiConfigKey = settings.ApiConfigKey;
				AdditionalFee = settings.AdditionalFee;
				AdditionalFeePercentage = settings.AdditionalFeePercentage;
				ValidateOrderTotal = settings.ValidateOrderTotal;
				SaveWarnings = settings.SaveWarnings;
				ShowFurtherInfo = settings.ShowFurtherInfo;
				FurtherInfoUrl = settings.FurtherInfoUrl;
				CustomerProtection = settings.CustomerProtection;
				CustomerProtectionInfoUrl = settings.CustomerProtectionInfoUrl;
				PaymentReason1 = settings.PaymentReason1;
				PaymentReason2 = settings.PaymentReason2;
				UseTestAccount = settings.UseTestAccount;
				AccountHolder = settings.AccountHolder;
				AccountNumber = settings.AccountNumber;
				AccountBankCode = settings.AccountBankCode;
				AccountCountry = settings.AccountCountry;
			}
			else
			{
				settings.ApiConfigKey = ApiConfigKey;
				settings.AdditionalFee = AdditionalFee;
				settings.AdditionalFeePercentage = AdditionalFeePercentage;
				settings.ValidateOrderTotal = ValidateOrderTotal;
				settings.SaveWarnings = SaveWarnings;
				settings.ShowFurtherInfo = ShowFurtherInfo;
				settings.FurtherInfoUrl = FurtherInfoUrl;
				settings.CustomerProtection = CustomerProtection;
				settings.CustomerProtectionInfoUrl = CustomerProtectionInfoUrl;
				settings.PaymentReason1 = PaymentReason1;
				settings.PaymentReason2 = PaymentReason2;
				settings.UseTestAccount = UseTestAccount;
				settings.AccountHolder = AccountHolder;
				settings.AccountNumber = AccountNumber;
				settings.AccountBankCode = AccountBankCode;
				settings.AccountCountry = AccountCountry;
			}
		}
	}
}
