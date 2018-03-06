using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Customers;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Customers
{
	[TestFixture]
	class WalletServiceTests : ServiceTest
	{
		IRepository<WalletHistory> _walletHistoryRepository;
		IWalletService _walletService;
		WalletHistory _entry1;
		WalletHistory _entry2;
		WalletHistory _entry3;
		WalletHistory _entry4;

		[SetUp]
		public new void SetUp()
		{
			_entry1 = new WalletHistory { CustomerId = 1, Amount = 120M, AmountBalance = 120M, StoreId = 1, CreatedOnUtc = new DateTime(2018, 3, 1) };
			_entry2 = new WalletHistory { CustomerId = 1, Amount = 12.3M, AmountBalance = 12.3M, StoreId = 2, CreatedOnUtc = new DateTime(2018, 3, 2) };
			_entry3 = new WalletHistory { CustomerId = 1, Amount = 60.5M, AmountBalance = 180.5M, StoreId = 1, CreatedOnUtc = new DateTime(2018, 3, 3) };
			_entry4 = new WalletHistory { CustomerId = 1, Amount = -20M, AmountBalance = 160.5M, StoreId = 1, Message = "test", CreatedOnUtc = new DateTime(2018, 3, 4) };

			var entries = new List<WalletHistory>{ _entry1, _entry2, _entry3, _entry4 };

			_walletHistoryRepository = MockRepository.GenerateMock<IRepository<WalletHistory>>();
			_walletHistoryRepository.Expect(x => x.TableUntracked).Return(entries.AsQueryable());
			_walletHistoryRepository.Expect(x => x.Table).Return(entries.AsQueryable());

			_walletService = new WalletService(_walletHistoryRepository);

			_entry1 = _walletService.InsertWalletHistoryEntry(1, 1, 120M);
			_entry2 = _walletService.InsertWalletHistoryEntry(1, 2, 12.3M);
			_entry3 = _walletService.InsertWalletHistoryEntry(1, 1, 60.5M);
			_entry4 = _walletService.InsertWalletHistoryEntry(1, 1, -20M, "test");
		}

		[Test]
		public void Can_get_wallet_amount_balance()
		{
			var balance1 = _walletService.GetWalletAmountBalance(1, 1);
			balance1.ShouldEqual(160.5M);

			var balance2 = _walletService.GetWalletAmountBalance(1, 2);
			balance2.ShouldEqual(12.3M);

			var balance3 = _walletService.GetWalletAmountBalance(2, 1);
			balance3.ShouldEqual(decimal.Zero);
		}
	}
}
