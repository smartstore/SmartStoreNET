using SmartStore.Web.Framework;

namespace SmartStore.Plugin.DiscountRules.HasShippingOption.Models
{
    public class RequirementModel
    {
		[SmartResourceDisplayName("Plugins.DiscountRules.HasShippingOption.Fields.ShippingOptions")]
		public string ShippingOptions { get; set; }

        public int DiscountId { get; set; }
        public int RequirementId { get; set; }
    }
}