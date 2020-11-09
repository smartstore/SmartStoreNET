using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class AutoUpdateOrderItemModel : EntityModelBase
    {
        public AutoUpdateOrderItemModel()
        {
            AdjustInventory = true;
        }

        public string Caption { get; set; }
        public string PostUrl { get; set; }
        public bool ShowUpdateRewardPoints { get; set; }
        public bool ShowUpdateTotals { get; set; }

        [SmartResourceDisplayName("Admin.Orders.OrderItem.AutoUpdate.AdjustInventory")]
        public bool AdjustInventory { get; set; }

        [SmartResourceDisplayName("Admin.Orders.OrderItem.AutoUpdate.UpdateRewardPoints")]
        public bool UpdateRewardPoints { get; set; }

        [SmartResourceDisplayName("Admin.Orders.OrderItem.AutoUpdate.UpdateTotals")]
        public bool UpdateTotals { get; set; }

        public int? NewQuantity { get; set; }
        public decimal? NewUnitPriceInclTax { get; set; }
        public decimal? NewUnitPriceExclTax { get; set; }
        public decimal? NewTaxRate { get; set; }
        public decimal? NewDiscountInclTax { get; set; }
        public decimal? NewDiscountExclTax { get; set; }
        public decimal? NewPriceInclTax { get; set; }
        public decimal? NewPriceExclTax { get; set; }
    }
}