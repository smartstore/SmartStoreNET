using SmartStore.Core.Domain.Customers;

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
		/// Inserts a wallet history entry. AmountBalance and CreatedOnUtc are set internally.
		/// </summary>
		/// <param name="entity">Wallet history entry.</param>
		/// <returns>The inserted wallet history entry.</returns>
		WalletHistory InsertHistoryEntry(WalletHistory entity);

		/// <summary>
		/// Updates a wallet history entry. Some fields like AmountBalance and CreatedOnUtc cannot be changed subsequently.
		/// </summary>
		/// <param name="entity">Wallet history entry.</param>
		/// <returns>The updated wallet history entry.</returns>
		WalletHistory UpdateHistoryEntry(WalletHistory entity);

		/// <summary>
		/// Deletes a wallet history entry.
		/// </summary>
		/// <param name="entity">Wallet history entry.</param>
		void DeleteHistoryEntry(WalletHistory entity);

		/// <summary>
		/// Gets the current wallet amount balance for a customer.
		/// </summary>
		/// <param name="customerId">The customer identifier.</param>
		/// <param name="storeId">The store identifier. Must not be zero.</param>
		/// <returns>Current wallet amount balance.</returns>
		decimal GetAmountBalance(int customerId, int storeId);
	}
}
