using System;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Customers
{
	public partial class WalletService : IWalletService
	{
		protected readonly IRepository<WalletHistory> _walletHistoryRepository;

		public WalletService(
			IRepository<WalletHistory> walletHistoryRepository)
		{
			_walletHistoryRepository = walletHistoryRepository;
		}

		protected virtual void UpdateBalances(WalletHistory entity, decimal amountDifference)
		{
			if (amountDifference == decimal.Zero)
			{
				return;
			}

			var entries = _walletHistoryRepository.Table
				.Where(x => x.CustomerId == entity.CustomerId && x.StoreId == entity.StoreId && x.Id >= entity.Id)
				.OrderByDescending(x => x.CreatedOnUtc)
				.ThenByDescending(x => x.Id)
				.ToList();

			foreach (var entry in entries)
			{
				entry.AmountBalance = entry.AmountBalance + amountDifference;
			}

			_walletHistoryRepository.UpdateRange(entries);
		}

		public virtual WalletHistory GetHistoryEntryById(int id)
		{
			if (id == 0)
			{
				return null;
			}

			return _walletHistoryRepository.GetById(id);
		}

		public virtual WalletHistory InsertHistoryEntry(
			int customerId,
			int storeId,
			decimal amount,
			string message = null,
			string adminComment = null,
			Order usedWithOrder = null,
			WalletPostingReason? reason = null)
		{
			Guard.NotZero(customerId, nameof(customerId));
			Guard.NotZero(storeId, nameof(storeId));

			var newAmountBalance = GetAmountBalance(customerId, storeId) + amount;

			var entry = new WalletHistory
			{
				CustomerId = customerId,
				StoreId = storeId,
				Amount = amount,
				AmountBalance = newAmountBalance,
				CreatedOnUtc = DateTime.UtcNow,
				Message = message.NullEmpty(),
				AdminComment = adminComment.NullEmpty(),
				UsedWithOrder = usedWithOrder,
				Reason = reason
			};

			_walletHistoryRepository.Insert(entry);

			return entry;
		}

		public virtual void UpdateHistoryEntry(
			int id,
			decimal amount,
			string message,
			string adminComment,
			WalletPostingReason? reason = null)
		{
			var entity = GetHistoryEntryById(id);
			if (entity == null)
			{
				return;
			}

			var amountDifference = amount - entity.Amount;

			entity.Amount = amount;
			entity.Message = message.NullEmpty();
			entity.AdminComment = adminComment.NullEmpty();
			entity.Reason = reason;

			_walletHistoryRepository.Update(entity);

			UpdateBalances(entity, amountDifference);
		}

		public virtual void DeleteHistoryEntry(int id)
		{
			var entity = GetHistoryEntryById(id);
			if (entity == null)
			{
				return;
			}

			var amountDifference = -1 * entity.Amount;
			UpdateBalances(entity, amountDifference);

			_walletHistoryRepository.Delete(entity);
		}

		public decimal GetAmountBalance(int customerId, int storeId)
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
