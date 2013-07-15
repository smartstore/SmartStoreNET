using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Plugin.Payments.Manual.Models
{
    public class ConfigurationModel : ModelBase
    {
        [SmartResourceDisplayName("Plugins.Payments.Manual.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.Manual.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.Manual.Fields.TransactMode")]
        public int TransactMode { get; set; }
        public SelectList TransactModeValues { get; set; }
    }
}