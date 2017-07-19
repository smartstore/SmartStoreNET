using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using SmartStore.PayPal.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.PayPal.Models
{
    public class PayPalStandardConfigurationModel : ModelBase
	{
		[SmartResourceDisplayName("Plugins.Payments.PayPal.SecurityProtocol")]
		public SecurityProtocolType? SecurityProtocol { get; set; }
		public List<SelectListItem> AvailableSecurityProtocols { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.UseSandbox")]
		public bool UseSandbox { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.IpnChangesPaymentStatus")]
		public bool IpnChangesPaymentStatus { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.BusinessEmail")]
		public string BusinessEmail { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PDTToken")]
		public string PdtToken { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PDTValidateOrderTotal")]
		public bool PdtValidateOrderTotal { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PdtValidateOnlyWarn")]
		public bool PdtValidateOnlyWarn { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.IsShippingAddressRequired")]
		public bool IsShippingAddressRequired { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.UsePayPalAddress")]
		public bool UsePayPalAddress { get; set; }

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
				SecurityProtocol = settings.SecurityProtocol;
                UseSandbox = settings.UseSandbox;
				IpnChangesPaymentStatus = settings.IpnChangesPaymentStatus;
                BusinessEmail = settings.BusinessEmail;
                PdtToken = settings.PdtToken;
                PdtValidateOrderTotal = settings.PdtValidateOrderTotal;
				PdtValidateOnlyWarn = settings.PdtValidateOnlyWarn;
                AdditionalFee = settings.AdditionalFee;
                AdditionalFeePercentage = settings.AdditionalFeePercentage;
				IsShippingAddressRequired = settings.IsShippingAddressRequired;
				UsePayPalAddress = settings.UsePayPalAddress;
                PassProductNamesAndTotals = settings.PassProductNamesAndTotals;
                EnableIpn = settings.EnableIpn;
                IpnUrl = settings.IpnUrl;
            }
            else
            {
				settings.SecurityProtocol = SecurityProtocol;
                settings.UseSandbox = UseSandbox;
				settings.IpnChangesPaymentStatus = IpnChangesPaymentStatus;
                settings.BusinessEmail = BusinessEmail;
                settings.PdtToken = PdtToken;
                settings.PdtValidateOrderTotal = PdtValidateOrderTotal;
				settings.PdtValidateOnlyWarn = PdtValidateOnlyWarn;
                settings.AdditionalFee = AdditionalFee;
                settings.AdditionalFeePercentage = AdditionalFeePercentage;
				settings.IsShippingAddressRequired = IsShippingAddressRequired;
				settings.UsePayPalAddress = UsePayPalAddress;
                settings.PassProductNamesAndTotals = PassProductNamesAndTotals;
                settings.EnableIpn = EnableIpn;
                settings.IpnUrl = IpnUrl;
            }
        }
	}
}