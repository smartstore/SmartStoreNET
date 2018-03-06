using System;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Customers
{
	public partial class WalletService : IWalletService
	{
		private readonly IRepository<WalletHistory> _walletHistoryRepository;

		public WalletService(
			IRepository<WalletHistory> walletHistoryRepository)
		{
			_walletHistoryRepository = walletHistoryRepository;
		}

		public virtual WalletHistory InsertWalletHistoryEntry(
			int customerId,
			int storeId,
			decimal amount,
			string message = null,
			string adminComment = null,
			Order usedWithOrder = null)
		{
			Guard.NotZero(customerId, nameof(customerId));
			Guard.NotZero(storeId, nameof(storeId));

			var newAmountBalance = GetWalletAmountBalance(customerId, storeId) + amount;

			var entry = new WalletHistory
			{
				CustomerId = customerId,
				StoreId = storeId,
				Amount = amount,
				AmountBalance = newAmountBalance,
				CreatedOnUtc = DateTime.UtcNow,
				Message = message.NullEmpty(),
				AdminComment = adminComment.NullEmpty(),
				UsedWithOrder = usedWithOrder
			};

			_walletHistoryRepository.Insert(entry);

			return entry;
		}

		public decimal GetWalletAmountBalance(int customerId, int storeId)
		{
			Guard.NotZero(customerId, nameof(customerId));
			Guard.NotZero(storeId, nameof(storeId));

			// TODO: caching per request required?
			var result = _walletHistoryRepository.TableUntracked
				.Where(x => x.CustomerId == customerId && x.StoreId == storeId)
				.OrderByDescending(x => x.CreatedOnUtc)
				.ThenByDescending(x => x.Id)
				.Select(x => x.AmountBalance)
				.FirstOrDefault();

			return result;
		}

	}
}
