using System;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Core.Domain.Customers
{
    /// <summary>
    /// Represents a reward point history entry
    /// </summary>
    [DataContract]
    public partial class RewardPointsHistory : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        [DataMember]
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the points redeemed/added
        /// </summary>
        [DataMember]
        public int Points { get; set; }

        /// <summary>
        /// Gets or sets the points balance
        /// </summary>
        [DataMember]
        public int PointsBalance { get; set; }

        /// <summary>
        /// Gets or sets the used amount
        /// </summary>
        [DataMember]
        public decimal UsedAmount { get; set; }

        /// <summary>
        /// Gets or sets the message
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        [DataMember]
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the order for which points were redeemed as a payment
        /// </summary>
        [DataMember]
        public virtual Order UsedWithOrder { get; set; }

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
        [DataMember]
        public virtual Customer Customer { get; set; }
    }
}
