using SmartStore.Web.Framework;

namespace SmartStore.Plugin.DiscountRules.HasAllProducts.Models
{
    public class RequirementModel
    {
        [SmartResourceDisplayName("Plugins.DiscountRules.HasAllProducts.Fields.Products")]
        public string Products { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }
    }
}