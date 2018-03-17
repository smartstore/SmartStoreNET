using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Stores;
using SmartStore.Data;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Stores;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Customers
{
	[TestFixture]
	class WalletServiceTests : ServiceTest
	{
		IRepository<WalletHistory> _walletHistoryRepository;
		ICustomerService _customerService;
		ICurrencyService _currencyService;
		IStoreService _storeService;
		ISettingService _settingService;
		IWalletService _walletService;
		SmartObjectContext _context;
		List<WalletHistory> _allEntries;
		Store _store1;
		Store _store2;
		Customer _customer1;
		Customer _customer2;

		private void Insert(WalletHistory entry)
		{
			_walletService.InsertHistoryEntry(entry);
			_allEntries.Add(entry);

			// Let entry.Customer be null here for testing purposes.
			if (entry.CustomerId == 1)
			{
				_customer1.WalletHistory.Add(entry);
			}
			else if (entry.CustomerId == 2)
			{
				_customer2.WalletHistory.Add(entry);
			}

			// Let repository grow by inserted entries that also reflect preceded entries.
			_walletHistoryRepository.Expect(x => x.TableUntracked).Return(_allEntries.AsQueryable());
			_walletHistoryRepository.Expect(x => x.Table).Return(_allEntries.AsQueryable());
		}

		private void Init()
		{
			_allEntries = new List<WalletHistory>();
			_context = new SmartObjectContext();
			_store1 = new Store { Id = 1, PrimaryStoreCurrencyId = 1 };
			_store2 = new Store { Id = 2, PrimaryStoreCurrencyId = 1 };
			_customer1 = new Customer { Id = 1 };
			_customer2 = new Customer { Id = 2 };

			_customerService = MockRepository.GenerateMock<ICustomerService>();
			_customerService.Expect(x => x.GetCustomerById(1)).Return(_customer1);
			_customerService.Expect(x => x.GetCustomerById(2)).Return(_customer2);

			_currencyService = MockRepository.GenerateMock<ICurrencyService>();

			_storeService = MockRepository.GenerateMock<IStoreService>();
			_storeService.Expect(x => x.GetStoreById(1)).Return(_store1);
			_storeService.Expect(x => x.GetStoreById(2)).Return(_store2);

			_settingService = MockRepository.GenerateMock<ISettingService>();
			_settingService.Expect(x => x.LoadSetting<PaymentSettings>(0)).Return(new PaymentSettings { WalletCurrencyId = 1 });

			_walletHistoryRepository = MockRepository.GenerateMock<IRepository<WalletHistory>>();
			_walletHistoryRepository.Expect(x => x.TableUntracked).Return(_allEntries.AsQueryable());
			_walletHistoryRepository.Expect(x => x.Table).Return(_allEntries.AsQueryable());
			_walletHistoryRepository.Expect(x => x.Context).Return(_context);
			_walletService = new WalletService(_walletHistoryRepository, _customerService, _currencyService, _storeService, _settingService);

			Insert(new WalletHistory { Id = 1, CustomerId = 1, StoreId = 1, Amount = 120M, CreatedOnUtc = new DateTime(2018, 3, 1) });
			Insert(new WalletHistory { Id = 2, CustomerId = 1, StoreId = 2, Amount = 12.3M, CreatedOnUtc = new DateTime(2018, 3, 2) });
			Insert(new WalletHistory { Id = 3, CustomerId = 1, StoreId = 1, Amount = 60.5M, CreatedOnUtc = new DateTime(2018, 3, 3) });
			Insert(new WalletHistory { Id = 4, CustomerId = 1, StoreId = 1, Amount = -20M, CreatedOnUtc = new DateTime(2018, 3, 4) });
			Insert(new WalletHistory { Id = 5, CustomerId = 1, StoreId = 2, Amount = 50M, CreatedOnUtc = new DateTime(2018, 3, 5) });
		}

		[SetUp]
		public new void SetUp()
		{
			Init();
		}

		[Test]
		public void Can_get_amount_balance()
		{
			var balance1 = _customer1.GetWalletCreditBalance(0);
			balance1.ShouldEqual(222.8M);

			var balance2 = _customer1.GetWalletCreditBalance(1);
			balance2.ShouldEqual(160.5M);

			var balance3 = _customer1.GetWalletCreditBalance(2);
			balance3.ShouldEqual(62.3M);

			var balance4 = _customer2.GetWalletCreditBalance(0);
			balance4.ShouldEqual(decimal.Zero);
		}
	}
}
