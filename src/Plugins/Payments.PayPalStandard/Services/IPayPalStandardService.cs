using System.Collections.Generic;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Services.Payments;

namespace SmartStore.Plugin.Payments.PayPalStandard.Services
{
	public partial interface IPayPalStandardService
	{
		/// <summary>
		/// Gets a payment status
		/// </summary>
		/// <param name="paymentStatus">PayPal payment status</param>
		/// <param name="pendingReason">PayPal pending reason</param>
		/// <returns>Payment status</returns>
		PaymentStatus GetPaymentStatus(string paymentStatus, string pendingReason);

		/// <summary>
		/// Get all PayPal line items
		/// </summary>
		/// <param name="postProcessPaymentRequest">Post process paymenmt request object</param>
		/// <param name="cartTotal">Receives the calculated cart total amount</param>
		/// <returns>All items for PayPal Standard API</returns>
		List<PayPalLineItem> GetLineItems(PostProcessPaymentRequest postProcessPaymentRequest, out decimal cartTotal);

		/// <summary>
		/// Adjusts the line amount for cart items to avoid rounding differences
		/// </summary>
		/// <param name="paypalItems">PayPal line items</param>
		/// <param name="postProcessPaymentRequest">Post process paymenmt request object</param>
		void AdjustLineItemAmounts(List<PayPalLineItem> paypalItems, PostProcessPaymentRequest postProcessPaymentRequest);
	}
}
