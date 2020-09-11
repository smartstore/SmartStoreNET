using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData
{
    /// <summary>
    /// Extra information about an order returned by the API.
    /// </summary>
    [DataContract]
    public partial class OrderInfo
    {
        /// <summary>
        /// Gets a value indicating whether an order has items to dispatch
        /// </summary>
        [DataMember]
        public bool HasItemsToDispatch { get; set; }

        /// <summary>
        /// Gets a value indicating whether an order has items to deliver
        /// </summary>
        [DataMember]
        public bool HasItemsToDeliver { get; set; }

        /// <summary>
        /// Gets a value indicating whether an order has items to be added to a shipment
        /// </summary>
        [DataMember]
        public bool CanAddItemsToShipment { get; set; }
    }
}