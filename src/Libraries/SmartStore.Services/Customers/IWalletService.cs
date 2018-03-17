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
		/// Count wallet history entries.
		/// </summary>
		/// <param name="customer">Customer.</param>
		/// <param name="order">Order.</param>
		/// <param name="storeId">Store identifier.</param>
		/// <returns>Number of wallet history entries.</returns>
		int CountEntries(Customer customer = null, Order order = null, int? storeId = null);

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
		void InsertHistoryEntry(WalletHistory entity);

		/// <summary>
		/// Updates a wallet history entry. Some fields like AmountBalance and CreatedOnUtc cannot be changed subsequently.
		/// </summary>
		/// <param name="entity">Wallet history entry.</param>
		void UpdateHistoryEntry(WalletHistory entity);

		/// <summary>
		/// Deletes a wallet history entry.
		/// </summary>
		/// <param name="entity">Wallet history entry.</param>
		void DeleteHistoryEntry(WalletHistory entity);
	}
}
