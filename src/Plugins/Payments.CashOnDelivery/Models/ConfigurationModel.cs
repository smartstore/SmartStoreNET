using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Payments.CashOnDelivery.Models
{
    public class ConfigurationModel : ModelBase
    {
        [AllowHtml]
        [SmartResourceDisplayName("Plugins.Payment.CashOnDelivery.DescriptionText")]
        public string DescriptionText { get; set; }

        [SmartResourceDisplayName("Plugins.Payment.CashOnDelivery.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payment.CashOnDelivery.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }
    }
}