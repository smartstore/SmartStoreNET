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
		/// Gets wallet history entry by identifier.
		/// </summary>
		/// <param name="id">The entry identifier.</param>
		/// <returns>Wallet history entity.</returns>
		WalletHistory GetHistoryEntryById(int id);

		/// <summary>
		/// Inserts a wallet history entry.
		/// </summary>
		/// <param name="customerId">The customer identifier.</param>
		/// <param name="storeId">The store identifier. Must not be zero.</param>
		/// <param name="amount">The amount.</param>
		/// <param name="message">An optional message to the customer.</param>
		/// <param name="adminComment">An optional admin comment.</param>
		/// <param name="usedWithOrder">The order the amount is used with.</param>
		/// <returns>The inserted wallet history entry.</returns>
		WalletHistory InsertHistoryEntry(
			int customerId,
			int storeId,
			decimal amount,
			string message = null,
			string adminComment = null,
			Order usedWithOrder = null);

		/// <summary>
		/// Updates a wallet history entry.
		/// </summary>
		/// <param name="id">Identifier of the wallet history entity.</param>
		/// <param name="amount">The amount.</param>
		/// <param name="message">An optional message to the customer.</param>
		/// <param name="adminComment">An optional admin comment.</param>
		void UpdateHistoryEntry(
			int id,
			decimal amount,
			string message,
			string adminComment);

		/// <summary>
		/// Deletes a wallet history entry.
		/// </summary>
		/// <param name="id">Identifier of the wallet history entity.</param>
		void DeleteHistoryEntry(int id);

		/// <summary>
		/// Gets the current wallet amount balance for the customer.
		/// </summary>
		/// <param name="customerId">The customer identifier.</param>
		/// <param name="storeId">The store identifier. Must not be zero.</param>
		/// <returns>Current wallet amount balance.</returns>
		decimal GetAmountBalance(int customerId, int storeId);
	}
}
