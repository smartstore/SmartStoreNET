using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Core.Domain.Customers
{
    public class RewardPointsSettings : ISettings
    {
        public RewardPointsSettings()
        {
            ExchangeRate = 1;
            PointsForPurchases_Amount = 10;
            PointsForPurchases_Points = 1;
            PointsForPurchases_Awarded = OrderStatus.Complete;
            PointsForPurchases_Canceled = OrderStatus.Cancelled;
        }

        /// <summary>
        /// Gets or sets a value indicating whether Reward Points Program is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value of Reward Points exchange rate
        /// </summary>
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// Gets or sets a value whether to round down reward points
        /// </summary>
        public bool RoundDownRewardPoints { get; set; }

        /// <summary>
        /// Gets or sets a number of points awarded for registration
        /// </summary>
        public int PointsForRegistration { get; set; }

        /// <summary>
        /// Gets or sets a number of points awarded for a product review
        /// </summary>
        public int PointsForProductReview { get; set; }

        /// <summary>
        /// Gets or sets a number of points awarded for purchases (amount in primary store currency)
        /// </summary>
        public decimal PointsForPurchases_Amount { get; set; }

        /// <summary>
        /// Gets or sets a number of points awarded for purchases
        /// </summary>
        public int PointsForPurchases_Points { get; set; }

        /// <summary>
        /// Points are awarded when the order status is
        /// </summary>
        public OrderStatus PointsForPurchases_Awarded { get; set; }

        /// <summary>
        /// Points are canceled when the order is
        /// </summary>
        public OrderStatus PointsForPurchases_Canceled { get; set; }
    }
}