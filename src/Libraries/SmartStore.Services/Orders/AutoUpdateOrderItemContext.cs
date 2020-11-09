using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Orders
{
    /// <summary>
    /// Contains data that is used to recalculated details of an order.
    /// </summary>
    public class AutoUpdateOrderItemContext
    {
        /// <summary>
        /// [IN] Whether order item is new
        /// </summary>
        public bool IsNewOrderItem { get; set; }

        /// <summary>
        /// [IN] Order item
        /// </summary>
        public OrderItem OrderItem { get; set; }

        /// <summary>
        /// [IN] Whether to adjust the inventory
        /// </summary>
        public bool AdjustInventory { get; set; }

        /// <summary>
        /// [IN] Whether to update order totals if order is in pending state
        /// </summary>
        public bool UpdateTotals { get; set; }

        /// <summary>
        /// [IN] Whether to update reward points
        /// </summary>
        public bool UpdateRewardPoints { get; set; }

        /// <summary>
        /// [IN] Quantity old
        /// </summary>
        public int QuantityOld { get; set; }

        /// <summary>
        /// [IN] Quantity new
        /// </summary>
        public int QuantityNew { get; set; }

        /// <summary>
        /// [IN] Old price incl. tax.
        /// </summary>
        public decimal? PriceInclTaxOld { get; set; }

        /// <summary>
        /// [IN] Old price excl. tax.
        /// </summary>
        public decimal? PriceExclTaxOld { get; set; }

        /// <summary>
        /// [OUT] Inventory changes
        /// </summary>
        public AdjustInventoryResult Inventory { get; set; }

        /// <summary>
        /// [OUT] Reward points old
        /// </summary>
        public int RewardPointsOld { get; set; }

        /// <summary>
        /// [OUT] Reward points new
        /// </summary>
        public int RewardPointsNew { get; set; }

        public static string InfoKey => "UpdateOrderItemContextInfo";

        /// <summary>
        /// The value to which the quantity amount has changed
        /// </summary>
        public int QuantityDelta => QuantityNew - QuantityOld;

        public decimal QuantityChangeFactor
        {
            get
            {
                if (QuantityOld != 0)
                    return (decimal)QuantityNew / (decimal)QuantityOld;

                return 1.0M;
            }
        }

        public string ToString(ILocalizationService localizationService)
        {
            if (Inventory == null && RewardPointsOld == 0 && RewardPointsNew == 0)
                return "";

            string stockOld = null;
            string stockNew = null;

            if (Inventory != null && Inventory.HasClearStockQuantityResult)
            {
                stockOld = Inventory.StockQuantityOld.ToString();
                stockNew = Inventory.StockQuantityNew.ToString();
            }

            string result = localizationService.GetResource("Admin.Orders.OrderItem.Update.Info").FormatWith(
                stockOld.NaIfEmpty(), stockNew.NaIfEmpty(), RewardPointsOld, RewardPointsNew
            );

            return result;
        }
    }
}
