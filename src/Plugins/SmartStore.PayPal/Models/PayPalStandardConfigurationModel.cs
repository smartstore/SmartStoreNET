using SmartStore.PayPal.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.PayPal.Models
{
    public class PayPalStandardConfigurationModel : ModelBase
	{
        [SmartResourceDisplayName("Plugins.Payments.PayPal.UseSandbox")]
		public bool UseSandbox { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.BusinessEmail")]
		public string BusinessEmail { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PDTToken")]
		public string PdtToken { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PDTValidateOrderTotal")]
		public bool PdtValidateOrderTotal { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PassProductNamesAndTotals")]
		public bool PassProductNamesAndTotals { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.EnableIpn")]
		public bool EnableIpn { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.IpnUrl")]
		public string IpnUrl { get; set; }

        public void Copy(PayPalStandardPaymentSettings settings, bool fromSettings)
        {
            if (fromSettings)
            {
                UseSandbox = settings.UseSandbox;
                BusinessEmail = settings.BusinessEmail;
                PdtToken = settings.PdtToken;
                PdtValidateOrderTotal = settings.PdtValidateOrderTotal;
                AdditionalFee = settings.AdditionalFee;
                AdditionalFeePercentage = settings.AdditionalFeePercentage;
                PassProductNamesAndTotals = settings.PassProductNamesAndTotals;
                EnableIpn = settings.EnableIpn;
                IpnUrl = settings.IpnUrl;
            }
            else
            {
                settings.UseSandbox = UseSandbox;
                settings.BusinessEmail = BusinessEmail;
                settings.PdtToken = PdtToken;
                settings.PdtValidateOrderTotal = PdtValidateOrderTotal;
                settings.AdditionalFee = AdditionalFee;
                settings.AdditionalFeePercentage = AdditionalFeePercentage;
                settings.PassProductNamesAndTotals = PassProductNamesAndTotals;
                settings.EnableIpn = EnableIpn;
                settings.IpnUrl = IpnUrl;
            }

        }
	}
}