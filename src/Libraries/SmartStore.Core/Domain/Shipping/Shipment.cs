using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Core.Domain.Shipping
{
    /// <summary>
    /// Represents a shipment
    /// </summary>
	[DataContract]
    public partial class Shipment : BaseEntity
    {
        private ICollection<ShipmentItem> _shipmentItems;

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
		[DataMember]
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the tracking number of this shipment
        /// </summary>
        [DataMember]
        public string TrackingNumber { get; set; }

        /// <summary>
        /// Gets or sets the tracking URL.
        /// </summary>
		[DataMember]
        [StringLength(2000)]
        public string TrackingUrl { get; set; }

        /// <summary>
        /// Gets or sets the total weight of this shipment
        /// It's nullable for compatibility with the previous version of Smartstore where was no such property
        /// </summary>
		[DataMember]
        public decimal? TotalWeight { get; set; }

        /// <summary>
        /// Gets or sets the shipped date and time
        /// </summary>
		[DataMember]
        public DateTime? ShippedDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the delivery date and time
        /// </summary>
		[DataMember]
        public DateTime? DeliveryDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the entity creation date
        /// </summary>
		[DataMember]
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets the order
        /// </summary>
		[DataMember]
        public virtual Order Order { get; set; }

        /// <summary>
		/// Gets or sets the shipment items
        /// </summary>
		[DataMember]
        public virtual ICollection<ShipmentItem> ShipmentItems
        {
            get => _shipmentItems ?? (_shipmentItems = new HashSet<ShipmentItem>());
            protected set => _shipmentItems = value;
        }
    }
}