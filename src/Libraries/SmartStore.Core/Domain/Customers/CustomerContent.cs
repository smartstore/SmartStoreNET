using System;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Customers
{
	/// <summary>
	/// Represents a customer generated content
	/// </summary>
	[DataContract]
	public partial class CustomerContent : BaseEntity, IAuditable
	{
		/// <summary>
		/// Gets or sets the customer identifier
		/// </summary>
		[DataMember]
		public int CustomerId { get; set; }

		/// <summary>
		/// Gets or sets the IP address
		/// </summary>
		[DataMember]
		public string IpAddress { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the content is approved
		/// </summary>
		[DataMember]
		public bool IsApproved { get; set; }

		/// <summary>
		/// Gets or sets the date and time of instance creation
		/// </summary>
		[DataMember]
		public DateTime CreatedOnUtc { get; set; }

		/// <summary>
		/// Gets or sets the date and time of instance update
		/// </summary>
		[DataMember]
		public DateTime UpdatedOnUtc { get; set; }

		/// <summary>
		/// Gets or sets the customer
		/// </summary>
		[DataMember]
		public virtual Customer Customer { get; set; }
    }
}
