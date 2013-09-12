using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Payments.DirectDebit.Models
{
    public class ConfigurationModel : ModelBase
    {
        [AllowHtml]
        [SmartResourceDisplayName("Plugins.Payments.DirectDebit.DescriptionText")]
        public string DescriptionText { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.DirectDebit.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.DirectDebit.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }
    }
}