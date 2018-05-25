using System.Collections.Generic;
using SmartStore.Core.Domain.Discounts;

namespace SmartStore.Services.Discounts
{
	public static class DiscountExtentions
    {
        /// <summary>
        /// Gets the discount amount for the specified value
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="amount">Amount</param>
        /// <returns>The discount amount</returns>
        public static decimal GetDiscountAmount(this Discount discount, decimal amount)
        {
			Guard.NotNull(discount, nameof(discount));

            var result = decimal.Zero;

			if (discount.UsePercentage)
			{
				result = (decimal)((((float)amount) * ((float)discount.DiscountPercentage)) / 100f);
			}
			else
			{
				result = discount.DiscountAmount;
			}

            return result;
        }

		/// <summary>
		/// Get the discount that achieves the highest discount amount other than zero.
		/// </summary>
		/// <param name="discounts">List of discounts</param>
		/// <param name="amount">Amount without discount (for percentage discounts)</param>
		/// <returns>Discount that achieves the highest discount amount other than zero.</returns>
		public static Discount GetPreferredDiscount(this IList<Discount> discounts, decimal amount)
        {
            Discount preferredDiscount = null;
            decimal? maximumDiscountValue = null;

            foreach (var discount in discounts)
            {
                var currentDiscountValue = discount.GetDiscountAmount(amount);
				if (currentDiscountValue != decimal.Zero)
				{
					if (!maximumDiscountValue.HasValue || currentDiscountValue > maximumDiscountValue)
					{
						maximumDiscountValue = currentDiscountValue;
						preferredDiscount = discount;
					}
				}
            }

            return preferredDiscount;
        }
    }
}
