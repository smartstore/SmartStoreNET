using System;
using SmartStore.Core.Domain.Customers;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Orders
{
    /// <summary>
    /// Represents a return request
    /// </summary>
	[DataContract]
	public partial class ReturnRequest : BaseEntity, IAuditable
	{
		/// <summary>
		/// Gets or sets the store identifier
		/// </summary>
		[DataMember]
		public int StoreId { get; set; }

        /// <summary>
        /// Gets or sets the order item identifier
        /// </summary>
		[DataMember]
		public int OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
		[DataMember]
		public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the quantity
        /// </summary>
		[DataMember]
		public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the reason to return
        /// </summary>
		[DataMember]
		public string ReasonForReturn { get; set; }

        /// <summary>
        /// Gets or sets the requested action
        /// </summary>
		[DataMember]
		public string RequestedAction { get; set; }

		/// <summary>
		/// Gets or sets the date and time when requested action was last updated
		/// </summary>
		[DataMember]
		public DateTime? RequestedActionUpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the customer comments
        /// </summary>
		[DataMember]
		public string CustomerComments { get; set; }

        /// <summary>
        /// Gets or sets the staff notes
        /// </summary>
		[DataMember]
		public string StaffNotes { get; set; }

		/// <summary>
		/// Gets or sets the admin comment
		/// </summary>
		[DataMember]
		public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets the return status identifier
        /// </summary>
		[DataMember]
		public int ReturnRequestStatusId { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
		[DataMember]
		public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity update
        /// </summary>
		[DataMember]
		public DateTime UpdatedOnUtc { get; set; }
        
        /// <summary>
        /// Gets or sets the return status
        /// </summary>
		[DataMember]
		public ReturnRequestStatus ReturnRequestStatus
        {
            get
            {
                return (ReturnRequestStatus)this.ReturnRequestStatusId;
            }
            set
            {
                this.ReturnRequestStatusId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the customer
        /// </summary>
		[DataMember]
		public virtual Customer Customer { get; set; }
    }
}
