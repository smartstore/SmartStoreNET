using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Payments.Prepayment.Models
{
    public class ConfigurationModel : ModelBase
    {
        [AllowHtml]
        [SmartResourceDisplayName("Plugins.Payment.Prepayment.DescriptionText")]
        public string DescriptionText { get; set; }

        [SmartResourceDisplayName("Plugins.Payment.Prepayment.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payment.Prepayment.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }
    }
}