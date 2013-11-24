using System;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order note
    /// </summary>
	[DataContract]
	public partial class OrderNote : BaseEntity
    {
        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
		[DataMember]
		public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the note
        /// </summary>
		[DataMember]
		public string Note { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a customer can see a note
        /// </summary>
		[DataMember]
		public bool DisplayToCustomer { get; set; }

        /// <summary>
        /// Gets or sets the date and time of order note creation
        /// </summary>
		[DataMember]
		public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets the order
        /// </summary>
        public virtual Order Order { get; set; }
    }

}
