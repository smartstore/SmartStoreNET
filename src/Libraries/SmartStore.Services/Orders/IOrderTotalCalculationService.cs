using System.Collections.Generic;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Services.Orders
{
    public partial interface IOrderTotalCalculationService
    {
        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="subTotalWithoutDiscount">Sub total (without discount)</param>
        /// <param name="subTotalWithDiscount">Sub total (with discount)</param>
		void GetShoppingCartSubTotal(IList<OrganizedShoppingCartItem> cart,
            out decimal discountAmount, out Discount appliedDiscount,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount);

        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="subTotalWithoutDiscount">Sub total (without discount)</param>
        /// <param name="subTotalWithDiscount">Sub total (with discount)</param>
		void GetShoppingCartSubTotal(IList<OrganizedShoppingCartItem> cart,
            bool includingTax,
            out decimal discountAmount, out Discount appliedDiscount,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount);

        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="subTotalWithoutDiscount">Sub total (without discount)</param>
        /// <param name="subTotalWithDiscount">Sub total (with discount)</param>
        /// <param name="taxRates">Tax rates (of order sub total)</param>
		void GetShoppingCartSubTotal(IList<OrganizedShoppingCartItem> cart,
            bool includingTax,
            out decimal discountAmount, out Discount appliedDiscount,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount,
            out SortedDictionary<decimal, decimal> taxRates);

        /// <summary>
        /// Gets an order discount (applied to order subtotal)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="orderSubTotal">Order subtotal</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Order discount</returns>
        decimal GetOrderSubtotalDiscount(Customer customer,
            decimal orderSubTotal, out Discount appliedDiscount);





        /// <summary>
        /// Adjust shipping rate (free shipping, additional charges, discounts)
        /// </summary>
        /// <param name="shippingOption">Shipping option</param>
        /// <param name="cart">Cart</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Adjusted shipping rate</returns>
		decimal AdjustShippingRate(decimal shippingRate, IList<OrganizedShoppingCartItem> cart,
			ShippingOption shippingOption, IList<ShippingMethod> shippingMethods, out Discount appliedDiscount);

        /// <summary>
        /// Gets shopping cart additional shipping charge
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>Additional shipping charge</returns>
		decimal GetShoppingCartAdditionalShippingCharge(IList<OrganizedShoppingCartItem> cart);

        /// <summary>
        /// Gets a value indicating whether shipping is free
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>A value indicating whether shipping is free</returns>
		bool IsFreeShipping(IList<OrganizedShoppingCartItem> cart);

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>Shipping total</returns>
		decimal? GetShoppingCartShippingTotal(IList<OrganizedShoppingCartItem> cart);

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <returns>Shipping total</returns>
		decimal? GetShoppingCartShippingTotal(IList<OrganizedShoppingCartItem> cart, bool includingTax);

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="taxRate">Applied tax rate</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Shipping total</returns>
		decimal? GetShoppingCartShippingTotal(IList<OrganizedShoppingCartItem> cart, bool includingTax,
            out decimal taxRate, out Discount appliedDiscount);

		/// <summary>
		/// Gets a shipping discount
		/// </summary>
		/// <param name="customer">Customer</param>
		/// <param name="shippingTotal">Shipping total</param>
		/// <param name="appliedDiscount">Applied discount</param>
		/// <returns>Shipping discount</returns>
		decimal GetShippingDiscount(Customer customer, decimal shippingTotal, out Discount appliedDiscount);






        /// <summary>
        /// Gets tax
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating tax</param>
        /// <returns>Tax total</returns>
		decimal GetTaxTotal(IList<OrganizedShoppingCartItem> cart, bool usePaymentMethodAdditionalFee = true);

        /// <summary>
        /// Gets tax
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="taxRates">Tax rates</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating tax</param>
        /// <returns>Tax total</returns>
		decimal GetTaxTotal(IList<OrganizedShoppingCartItem> cart, out SortedDictionary<decimal, decimal> taxRates,
            bool usePaymentMethodAdditionalFee = true);




        /// <summary>
        /// Gets the shopping cart total
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="ignoreRewardPoints">A value indicating whether we should ignore reward points (if enabled and a customer is going to use them)</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating order total</param>
		/// <param name="ignoreCreditBalance">A value indicating whether to ignore a credit balance.</param>
        /// <returns>Shopping cart total. TotalAmount is <c>null</c> if shopping cart total couldn't be calculated now.</returns>
        ShoppingCartTotal GetShoppingCartTotal(
            IList<OrganizedShoppingCartItem> cart,
            bool ignoreRewardPoints = false,
            bool usePaymentMethodAdditionalFee = true,
			bool ignoreCreditBalance = false);


        /// <summary>
        /// Gets an order discount (applied to order total)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="orderTotal">Order total</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Order discount</returns>
        decimal GetOrderTotalDiscount(Customer customer, decimal orderTotal, out Discount appliedDiscount);





        /// <summary>
        /// Converts reward points to amount primary store currency
        /// </summary>
        /// <param name="rewardPoints">Reward points</param>
        /// <returns>Converted value</returns>
        decimal ConvertRewardPointsToAmount(int rewardPoints);

        /// <summary>
        /// Converts an amount in primary store currency to reward points
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <returns>Converted value</returns>
        int ConvertAmountToRewardPoints(decimal amount);
    }
}
