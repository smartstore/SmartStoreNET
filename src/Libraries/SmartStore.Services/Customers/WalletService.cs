using System;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Customers
{
	public partial class WalletService : IWalletService
	{
		protected readonly IRepository<WalletHistory> _walletHistoryRepository;
		protected readonly ICustomerService _customerService;

		public WalletService(
			IRepository<WalletHistory> walletHistoryRepository,
			ICustomerService customerService)
		{
			_walletHistoryRepository = walletHistoryRepository;
			_customerService = customerService;
		}

		protected virtual void UpdateBalances(WalletHistory entity, decimal amountDifference)
		{
			if (amountDifference == decimal.Zero)
			{
				return;
			}

			var entries = _walletHistoryRepository.Table
				.Where(x => x.CustomerId == entity.CustomerId && x.Id >= entity.Id)
				.OrderByDescending(x => x.CreatedOnUtc)
				.ThenByDescending(x => x.Id)
				.ToList();

			foreach (var entry in entries)
			{
				entry.AmountBalance = entry.AmountBalance + amountDifference;

				if (entry.StoreId == entity.StoreId)
				{
					entry.AmountBalancePerStore = entry.AmountBalancePerStore + amountDifference;
				}
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

		public virtual void InsertHistoryEntry(WalletHistory entity)
		{
			Guard.NotNull(entity, nameof(entity));
			Guard.NotZero(entity.CustomerId, nameof(entity.CustomerId));
			Guard.NotZero(entity.StoreId, nameof(entity.StoreId));

			var customer = entity.Customer ?? _customerService.GetCustomerById(entity.CustomerId);

			entity.Message = entity.Message.NullEmpty();
			entity.AdminComment = entity.AdminComment.NullEmpty();

			// Always overwrite what could break the sequence.
			entity.CreatedOnUtc = DateTime.UtcNow;
			entity.AmountBalance = customer.GetWalletCreditBalance(0) + entity.Amount;
			entity.AmountBalancePerStore = customer.GetWalletCreditBalance(entity.StoreId) + entity.Amount;

			_walletHistoryRepository.Insert(entity);
		}

		public virtual void UpdateHistoryEntry(WalletHistory entity)
		{
			Guard.NotNull(entity, nameof(entity));

			// Update only what is uncritical and does not break the sequence.
			var amount = entity.Amount;
			var message = entity.Message;
			var adminComment = entity.AdminComment;
			var reason = entity.Reason;

			_walletHistoryRepository.Context.ReloadEntity(entity);

			// Entity is in now in unchanged state.
			var amountDifference = amount - entity.Amount;

			entity.Amount = amount;
			entity.Message = message.NullEmpty();
			entity.AdminComment = adminComment.NullEmpty();
			entity.Reason = reason;

			_walletHistoryRepository.Update(entity);

			UpdateBalances(entity, amountDifference);
		}

		public virtual void DeleteHistoryEntry(WalletHistory entity)
		{
			if (entity != null)
			{
				var amountDifference = -1 * entity.Amount;
				UpdateBalances(entity, amountDifference);

				_walletHistoryRepository.Delete(entity);
			}
		}

		//public virtual decimal GetCreditBalance(int customerId, int storeId = 0)
		//{
		//	Guard.NotZero(customerId, nameof(customerId));

		//	if (storeId == 0)
		//	{
		//		var result = _walletHistoryRepository.TableUntracked
		//			.Where(x => x.CustomerId == customerId)
		//			.OrderByDescending(x => x.CreatedOnUtc)
		//			.ThenByDescending(x => x.Id)
		//			.Select(x => x.AmountBalance)
		//			.FirstOrDefault();

		//		return result;
		//	}
		//	else
		//	{
		//		var result = _walletHistoryRepository.TableUntracked
		//			.Where(x => x.CustomerId == customerId && x.StoreId == storeId)
		//			.OrderByDescending(x => x.CreatedOnUtc)
		//			.ThenByDescending(x => x.Id)
		//			.Select(x => x.AmountBalancePerStore)
		//			.FirstOrDefault();

		//		return result;
		//	}
		//}
	}
}
