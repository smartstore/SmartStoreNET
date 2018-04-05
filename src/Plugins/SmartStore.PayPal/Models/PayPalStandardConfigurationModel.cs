using SmartStore.ComponentModel;
using SmartStore.PayPal.Settings;
using SmartStore.Web.Framework;

namespace SmartStore.PayPal.Models
{
	public class PayPalStandardConfigurationModel : ApiConfigurationModel
	{
		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.BusinessEmail")]
		public string BusinessEmail { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PDTToken")]
		public string PdtToken { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PDTValidateOrderTotal")]
		public bool PdtValidateOrderTotal { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.PdtValidateOnlyWarn")]
		public bool PdtValidateOnlyWarn { get; set; }

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
				MiniMapper.Map(settings, this);
			}
            else
			{
				MiniMapper.Map(this, settings);
				settings.BusinessEmail = BusinessEmail.TrimSafe();
			}
        }
	}
}