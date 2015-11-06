using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Shipping
{
    /// <summary>
    /// Represents a shipment order product variant
    /// </summary>
	[DataContract]
    public partial class ShipmentItem : BaseEntity
    {
        /// <summary>
        /// Gets or sets the shipment identifier
        /// </summary>
		[DataMember]
        public int ShipmentId { get; set; }

        /// <summary>
        /// Gets or sets the order item identifier
        /// </summary>
		[DataMember]
        public int OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
		[DataMember]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets the shipment
        /// </summary>
		[DataMember]
        public virtual Shipment Shipment { get; set; }
    }
}