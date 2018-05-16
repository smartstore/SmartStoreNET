using System;
using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Core.Domain.Customers
{
	/// <summary>
	/// Represents a digital wallet history entry.
	/// </summary>
	public class WalletHistory : BaseEntity
	{
		/// <summary>
		/// Gets or sets the store identifier. Should not be zero.
		/// </summary>
		[Index("IX_StoreId_CreatedOn", 0)]
		public int StoreId { get; set; }

		/// <summary>
		/// Gets or sets the customer identifier.
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// Gets or sets the order identifier.
		/// </summary>
		public int? OrderId { get; set; }

		/// <summary>
		/// Gets or sets the amount of the entry.
		/// </summary>
		public decimal Amount { get; set; }

		/// <summary>
		/// Gets or sets the amount balance when the entry was created.
		/// </summary>
		public decimal AmountBalance { get; set; }

		/// <summary>
		/// Gets or sets the amount balance per store when the entry was created.
		/// </summary>
		public decimal AmountBalancePerStore { get; set; }

		/// <summary>
		/// Gets or sets the date ehen the entry was created (in UTC).
		/// </summary>
		[Index("IX_StoreId_CreatedOn", 1)]
		public DateTime CreatedOnUtc { get; set; }

		/// <summary>
		/// Gets or sets the reason for posting this entry.
		/// </summary>
		public WalletPostingReason? Reason { get; set; }

		/// <summary>
		/// Gets or sets the message.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Gets or sets the admin comment.
		/// </summary>
		public string AdminComment { get; set; }

		/// <summary>
		/// Gets or sets the customer.
		/// </summary>
		public virtual Customer Customer { get; set; }

		/// <summary>
		/// Gets or sets the order for which the wallet entry was used.
		/// </summary>
		public virtual Order Order { get; set; }
	}
}
