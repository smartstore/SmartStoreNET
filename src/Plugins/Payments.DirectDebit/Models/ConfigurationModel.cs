using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Payments.DirectDebit.Models
{
    public class ConfigurationModel : ModelBase
    {
        [AllowHtml]
        [SmartResourceDisplayName("Plugins.Payment.DirectDebit.DescriptionText")]
        public string DescriptionText { get; set; }

        [SmartResourceDisplayName("Plugins.Payment.DirectDebit.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payment.DirectDebit.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }
    }
}