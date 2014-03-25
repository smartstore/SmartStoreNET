using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Payments.PayPalStandard.Models
{
	public class ConfigurationModel : ModelBase
	{
		[SmartResourceDisplayName("Plugins.Payments.PayPalStandard.Fields.UseSandbox")]
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
	}
}