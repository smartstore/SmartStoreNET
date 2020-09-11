using NUnit.Framework;
using SmartStore.Core.Domain.Orders;
using SmartStore.Tests;

namespace SmartStore.Core.Tests.Domain.Orders
{
    [TestFixture]
    public class GiftCardTests
    {
        [Test]
        public void Can_validate_giftCard()
        {
            var gc = new GiftCard
            {
                Amount = 100,
                IsGiftCardActivated = true,
                PurchasedWithOrderItemId = 2,
                PurchasedWithOrderItem = new OrderItem
                {
                    Id = 2,
                    OrderId = 1,
                    Order = new Order
                    {
                        StoreId = 1
                    }
                }
            };

            gc.GiftCardUsageHistory.Add(new GiftCardUsageHistory
            {
                UsedValue = 30
            });
            gc.GiftCardUsageHistory.Add(new GiftCardUsageHistory
            {
                UsedValue = 20
            });
            gc.GiftCardUsageHistory.Add(new GiftCardUsageHistory
            {
                UsedValue = 5
            });

            //valid
            gc.IsGiftCardValid(1).ShouldEqual(true);

            //wrong store
            gc.IsGiftCardValid(2).ShouldEqual(false);

            //mark as not active
            gc.IsGiftCardActivated = false;
            gc.IsGiftCardValid(1).ShouldEqual(false);

            //again active
            gc.IsGiftCardActivated = true;
            gc.IsGiftCardValid(1).ShouldEqual(true);

            //add usage history record
            gc.GiftCardUsageHistory.Add(new GiftCardUsageHistory
            {
                UsedValue = 1000
            });
            gc.IsGiftCardValid(1).ShouldEqual(false);
        }

        [Test]
        public void Can_calculate_giftCard_remainingAmount()
        {
            var gc = new GiftCard()
            {
                Amount = 100
            };
            gc.GiftCardUsageHistory.Add
                (
                    new GiftCardUsageHistory()
                    {
                        UsedValue = 30
                    }

                );
            gc.GiftCardUsageHistory.Add
                (
                    new GiftCardUsageHistory()
                    {
                        UsedValue = 20
                    }

                );
            gc.GiftCardUsageHistory.Add
                (
                    new GiftCardUsageHistory()
                    {
                        UsedValue = 5
                    }

                );

            gc.GetGiftCardRemainingAmount().ShouldEqual(45);
        }
    }
}
