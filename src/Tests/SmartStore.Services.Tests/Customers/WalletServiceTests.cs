using NUnit.Framework;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Customers;
using SmartStore.Tests;

namespace SmartStore.Services.Tests.Customers
{
	[TestFixture]
	class WalletServiceTests : ServiceTest
	{
		[SetUp]
		public new void SetUp()
		{

		}

		[Test]
		public void Can_add_wallet_history_entry()
		{
			var storeId = 1;
			var customer = new Customer();
			var order = new Order { Id = 2 };

			var balance1 = customer.GetWalletAmountBalance(storeId);
			balance1.ShouldEqual(decimal.Zero);

			var entry1 = customer.AddWalletHistoryEntry(120M, storeId);
			entry1.AmountBalance.ShouldEqual(120M);

			var entry2 = customer.AddWalletHistoryEntry(60.5M, 2);
			entry2.AmountBalance.ShouldEqual(60.5M);

			var entry3 = customer.AddWalletHistoryEntry(60.5M, storeId);
			entry3.AmountBalance.ShouldEqual(180.5M);

			var entry4 = customer.AddWalletHistoryEntry(-20M, storeId, "test", null, order);
			entry4.AmountBalance.ShouldEqual(160.5M);
			entry4.UsedWithOrder.Id.ShouldEqual(2);
		}
	}
}
