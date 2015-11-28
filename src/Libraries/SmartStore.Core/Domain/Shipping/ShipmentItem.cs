
namespace SmartStore.Core.Domain.Shipping
{
    /// <summary>
    /// Represents a shipment order product variant
    /// </summary>
    public partial class ShipmentItem : BaseEntity
    {
        /// <summary>
        /// Gets or sets the shipment identifier
        /// </summary>
        public int ShipmentId { get; set; }

        /// <summary>
        /// Gets or sets the order item identifier
        /// </summary>
        public int OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Gets the shipment
        /// </summary>
        public virtual Shipment Shipment { get; set; }
    }
}