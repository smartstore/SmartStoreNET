using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Customers
{
	/// <summary>
	/// Digital wallet interface.
	/// </summary>
	public partial interface IWalletService
	{
		/// <summary>
		/// Insert a wallet history entry.
		/// </summary>
		/// <param name="customerId">The customer identifier.</param>
		/// <param name="storeId">The store identifier. Must not be zero.</param>
		/// <param name="amount">The amount.</param>
		/// <param name="message">An optional message to the customer.</param>
		/// <param name="adminComment">An optional admin comment.</param>
		/// <param name="usedWithOrder">The order the amount is used with.</param>
		/// <returns>The inserted wallet history entry.</returns>
		WalletHistory InsertWalletHistoryEntry(
			int customerId,
			int storeId,
			decimal amount,
			string message = null,
			string adminComment = null,
			Order usedWithOrder = null);

		/// <summary>
		/// Gets the current wallet amount balance for the customer.
		/// </summary>
		/// <param name="customerId">The customer identifier.</param>
		/// <param name="storeId">The store identifier. Must not be zero.</param>
		/// <returns>Current wallet amount balance.</returns>
		decimal GetWalletAmountBalance(int customerId, int storeId);
	}
}
