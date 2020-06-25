using System;

namespace SmartStore.Core.Domain.Dashboard
{
    /// <summary>
    /// Represents a order chart data point
    /// </summary>
    public partial class OrderDataPoint
    {
        /// <summary>
        /// DateTime entity was created (Utc)
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Total amount
        /// </summary>
        public decimal OrderTotal { get; set; }

        /// <summary>
        /// Order status id
        /// </summary>
        public int OrderStatusId { get; set; }

        /// <summary>
        /// Payment status
        /// </summary>
        public int PaymentStatusId { get; set; }

        /// <summary>
        /// Shipping status
        /// </summary>
        public int ShippingStatusId { get; set; }
    }
}
