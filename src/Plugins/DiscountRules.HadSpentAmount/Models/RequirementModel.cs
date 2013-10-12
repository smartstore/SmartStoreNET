using SmartStore.Web.Framework;

namespace SmartStore.Plugin.DiscountRules.HadSpentAmount.Models
{
    public class RequirementModel
    {
        [SmartResourceDisplayName("Plugins.DiscountRules.HadSpentAmount.Fields.Amount")]
        public decimal SpentAmount { get; set; }

        [SmartResourceDisplayName("Plugins.DiscountRules.HadSpentAmount.Fields.LimitToCurrentBasketSubTotal")]
        public bool LimitToCurrentBasketSubTotal { get; set; }

        [SmartResourceDisplayName("Plugins.DiscountRules.HadSpentAmount.Fields.BasketSubTotalIncludesDiscounts")]
        public bool BasketSubTotalIncludesDiscounts { get; set; }

        public int DiscountId { get; set; }

        public int RequirementId { get; set; }
    }
}