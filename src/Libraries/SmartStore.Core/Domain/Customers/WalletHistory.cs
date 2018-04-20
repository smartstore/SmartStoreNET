using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Core.Domain.Customers
{
	/// <summary>
	/// Represents a digital wallet history entry.
	/// </summary>
	[DataContract]
	public class WalletHistory : BaseEntity
	{
		/// <summary>
		/// Gets or sets the store identifier. Should not be zero.
		/// </summary>
		[DataMember]
		[Index("IX_StoreId_CreatedOn", 0)]
		public int StoreId { get; set; }

		/// <summary>
		/// Gets or sets the customer identifier.
		/// </summary>
		[DataMember]
		public int CustomerId { get; set; }

		/// <summary>
		/// Gets or sets the order identifier.
		/// </summary>
		[DataMember]
		public int? OrderId { get; set; }

		/// <summary>
		/// Gets or sets the amount of the entry.
		/// </summary>
		[DataMember]
		public decimal Amount { get; set; }

		/// <summary>
		/// Gets or sets the amount balance when the entry was created.
		/// </summary>
		[DataMember]
		public decimal AmountBalance { get; set; }

		/// <summary>
		/// Gets or sets the amount balance per store when the entry was created.
		/// </summary>
		[DataMember]
		public decimal AmountBalancePerStore { get; set; }

		/// <summary>
		/// Gets or sets the date ehen the entry was created (in UTC).
		/// </summary>
		[DataMember]
		[Index("IX_StoreId_CreatedOn", 1)]
		public DateTime CreatedOnUtc { get; set; }

		/// <summary>
		/// Gets or sets the reason for posting this entry.
		/// </summary>
		[DataMember]
		public WalletPostingReason? Reason { get; set; }

		/// <summary>
		/// Gets or sets the message.
		/// </summary>
		[DataMember]
		public string Message { get; set; }

		/// <summary>
		/// Gets or sets the admin comment.
		/// </summary>
		[DataMember]
		public string AdminComment { get; set; }

		/// <summary>
		/// Gets or sets the customer.
		/// </summary>
		[DataMember]
		public virtual Customer Customer { get; set; }

		/// <summary>
		/// Gets or sets the order for which the wallet entry was used.
		/// </summary>
		[DataMember]
		public virtual Order Order { get; set; }
	}
}
