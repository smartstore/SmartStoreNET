using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData
{
    /// <summary>
    /// Extra information about an order item returned by the API.
    /// </summary>
    [DataContract]
    public partial class OrderItemInfo
    {
        /// <summary>
        /// Gets the total number of items which can be added to new shipments
        /// </summary>
        [DataMember]
        public int ItemsCanBeAddedToShipmentCount { get; set; }

        /// <summary>
        /// Gets the total number of items in all shipments
        /// </summary>
        [DataMember]
        public int ShipmentItemsCount { get; set; }

        /// <summary>
        /// Gets the total number of dispatched items
        /// </summary>
        [DataMember]
        public int DispatchedItemsCount { get; set; }

        /// <summary>
        /// Gets the total number of not dispatched items
        /// </summary>
        [DataMember]
        public int NotDispatchedItemsCount { get; set; }

        /// <summary>
        /// Gets the total number of already delivered items
        /// </summary>
        [DataMember]
        public int DeliveredItemsCount { get; set; }

        /// <summary>
        /// Gets the total number of not delivered items
        /// </summary>
        [DataMember]
        public int NotDeliveredItemsCount { get; set; }
    }
}