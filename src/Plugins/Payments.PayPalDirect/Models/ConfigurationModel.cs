using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Payments.PayPalDirect.Models
{
	public class ConfigurationModel : ModelBase
	{
		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.UseSandbox")]
		public bool UseSandbox { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.TransactMode")]
		public int TransactMode { get; set; }
		public SelectList TransactModeValues { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.ApiAccountName")]
		public string ApiAccountName { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.ApiAccountPassword")]
		public string ApiAccountPassword { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.Signature")]
		public string Signature { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalDirect.Fields.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }
	}
}