using SmartStore.Web.Framework;

namespace SmartStore.Plugin.DiscountRules.HasOneProduct.Models
{
    public class RequirementModel
    {
        [SmartResourceDisplayName("Plugins.DiscountRules.HasOneProduct.Fields.Products")]
        public string Products { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }
    }
}