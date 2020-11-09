using System.Collections.Generic;
using SmartStore.Core.Domain.Discounts;

namespace SmartStore.Services.Orders
{
    public class ShoppingCartTotal
    {
        public ShoppingCartTotal(decimal? totalAmount)
        {
            TotalAmount = totalAmount;
            ConvertedFromPrimaryStoreCurrency = new ConvertedAmounts();
        }

        public static implicit operator decimal?(ShoppingCartTotal obj)
        {
            return obj.TotalAmount;
        }

        public static implicit operator ShoppingCartTotal(decimal? obj)
        {
            return new ShoppingCartTotal(obj);
        }

        /// <summary>
        /// Total amount of the shopping cart. <c>null</c> if the cart total couldn't be calculated now.
        /// </summary>
        public decimal? TotalAmount { get; private set; }

        /// <summary>
        /// Rounding amount
        /// </summary>
        public decimal RoundingAmount { get; set; }

        /// <summary>
        /// Applied discount amount
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Applied discount
        /// </summary>
        public Discount AppliedDiscount { get; set; }

        /// <summary>
        /// Applied gift cards
        /// </summary>
        public List<AppliedGiftCard> AppliedGiftCards { get; set; }

        /// <summary>
        /// Reward points to redeem
        /// </summary>
        public int RedeemedRewardPoints { get; set; }

        /// <summary>
        /// Reward points amount to redeem (in primary store currency)
        /// </summary>
        public decimal RedeemedRewardPointsAmount { get; set; }

        /// <summary>
        /// Credit balance.
        /// </summary>
        public decimal CreditBalance { get; set; }

        public ConvertedAmounts ConvertedFromPrimaryStoreCurrency { get; set; }

        public override string ToString()
        {
            return (TotalAmount ?? decimal.Zero).FormatInvariant();
        }

        public class ConvertedAmounts
        {
            /// <summary>
            /// Converted total amount of the shopping cart. <c>null</c> if the cart total couldn't be calculated now.
            /// </summary>
            public decimal? TotalAmount { get; set; }

            /// <summary>
            /// Converted rounding amount
            /// </summary>
            public decimal RoundingAmount { get; set; }
        }
    }
}
