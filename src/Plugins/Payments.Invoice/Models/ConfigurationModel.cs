using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Payments.Invoice.Models
{
    public class ConfigurationModel : ModelBase
    {
        [AllowHtml]
        [SmartResourceDisplayName("Plugins.Payment.Invoice.DescriptionText")]
        public string DescriptionText { get; set; }

        [SmartResourceDisplayName("Plugins.Payment.Invoice.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payment.Invoice.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }
    }
}