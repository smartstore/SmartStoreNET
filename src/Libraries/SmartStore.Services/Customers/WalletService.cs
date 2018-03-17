using System;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Customers
{
	public partial class WalletService : IWalletService
	{
		protected readonly IRepository<WalletHistory> _walletHistoryRepository;
		protected readonly ICustomerService _customerService;
		protected readonly ICurrencyService _currencyService;
		protected readonly IStoreService _storeService;
		protected readonly ISettingService _settingService;

		public WalletService(
			IRepository<WalletHistory> walletHistoryRepository,
			ICustomerService customerService,
			ICurrencyService currencyService,
			IStoreService storeService,
			ISettingService settingService)
		{
			_walletHistoryRepository = walletHistoryRepository;
			_customerService = customerService;
			_currencyService = currencyService;
			_storeService = storeService;
			_settingService = settingService;
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

		protected virtual decimal ConvertToWalletCurrency(decimal amount, int storeId)
		{
			Guard.NotZero(storeId, nameof(storeId));

			if (amount == decimal.Zero)
			{
				return amount;
			}

			var store = _storeService.GetStoreById(storeId);
			var paymentSettings = _settingService.LoadSetting<PaymentSettings>(0);

			// Nothing to convert if aource and target currency are equal.
			if (store.PrimaryStoreCurrencyId == paymentSettings.WalletCurrencyId)
			{
				return amount;
			}
			
			var walletCurrency = _currencyService.GetCurrencyById(paymentSettings.WalletCurrencyId);
			var result = _currencyService.ConvertCurrency(amount, store.PrimaryStoreCurrency, walletCurrency, store);

			return result;
		}

		public virtual int CountEntries(
			Customer customer = null,
			Order order = null,
			int? storeId = null)
		{
			var query = _walletHistoryRepository.Table;

			if (customer != null)
			{
				query = query.Where(x => x.CustomerId == customer.Id);
			}

			if (order != null)
			{
				query = query.Where(x => x.OrderId == order.Id);
			}

			if (storeId.HasValue)
			{
				query = query.Where(x => x.StoreId == storeId.Value);
			}

			return query.Count();
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

			var convertedAmount = ConvertToWalletCurrency(entity.Amount, entity.StoreId);
			var customer = entity.Customer ?? _customerService.GetCustomerById(entity.CustomerId);

			entity.Message = entity.Message.NullEmpty();
			entity.AdminComment = entity.AdminComment.NullEmpty();

			// Always overwrite what could break the sequence.
			entity.CreatedOnUtc = DateTime.UtcNow;
			entity.AmountBalance = customer.GetWalletCreditBalance(0) + convertedAmount;
			entity.AmountBalancePerStore = customer.GetWalletCreditBalance(entity.StoreId) + convertedAmount;

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

			// Entity is in unchanged state now.
			var convertedAmount = ConvertToWalletCurrency(amount, entity.StoreId);
			var amountDifference = convertedAmount - entity.Amount;

			entity.Amount = convertedAmount;
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
	}
}
