using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using SmartStore.Core.Logging;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Services.Payments;
using SmartStore.Services.Localization;
using SmartStore.Services.Tax;
using SmartStore.Core.Localization;

namespace SmartStore.Plugin.Payments.PayPalStandard.Services
{
	public class PayPalStandardService : IPayPalStandardService
	{
		private readonly ITaxService _taxService;

		public PayPalStandardService(
			ITaxService taxService)
		{
			_taxService = taxService;

			T = NullLocalizer.Instance;
			Logger = NullLogger.Instance;
		}

		public Localizer T { get; set; }
		public ILogger Logger { get; set; }

		/// <summary>
		/// Splits the difference of two value into a portion value (for each item) and a rest value
		/// </summary>
		/// <param name="difference">The difference value</param>
		/// <param name="numberOfLines">Number of lines\items to split the difference</param>
		/// <param name="portion">Portion value</param>
		/// <param name="rest">Rest value</param>
		private void SplitDifference(decimal difference, int numberOfLines, out decimal portion, out decimal rest)
		{
			portion = rest = decimal.Zero;

			if (numberOfLines == 0)
				numberOfLines = 1;

			int intDifference = (int)(difference * 100);
			int intPortion = (int)Math.Truncate((double)intDifference / (double)numberOfLines);
			int intRest = intDifference % numberOfLines;

			portion = Math.Round(((decimal)intPortion) / 100, 2);
			rest = Math.Round(((decimal)intRest) / 100, 2);

			Debug.Assert(difference == ((numberOfLines * portion) + rest));
		}

		/// <summary>
		/// Gets a payment status
		/// </summary>
		/// <param name="paymentStatus">PayPal payment status</param>
		/// <param name="pendingReason">PayPal pending reason</param>
		/// <returns>Payment status</returns>
		public PaymentStatus GetPaymentStatus(string paymentStatus, string pendingReason)
		{
			var result = PaymentStatus.Pending;

			if (paymentStatus == null)
				paymentStatus = string.Empty;

			if (pendingReason == null)
				pendingReason = string.Empty;

			switch (paymentStatus.ToLowerInvariant())
			{
				case "pending":
					switch (pendingReason.ToLowerInvariant())
					{
						case "authorization":
							result = PaymentStatus.Authorized;
							break;
						default:
							result = PaymentStatus.Pending;
							break;
					}
					break;
				case "processed":
				case "completed":
				case "canceled_reversal":
					result = PaymentStatus.Paid;
					break;
				case "denied":
				case "expired":
				case "failed":
				case "voided":
					result = PaymentStatus.Voided;
					break;
				case "refunded":
				case "reversed":
					result = PaymentStatus.Refunded;
					break;
				default:
					break;
			}
			return result;
		}

		/// <summary>
		/// Get all PayPal line items
		/// </summary>
		/// <param name="postProcessPaymentRequest">Post process paymenmt request object</param>
		/// <param name="checkoutAttributeValues">List with checkout attribute values</param>
		/// <param name="cartTotal">Receives the calculated cart total amount</param>
		/// <returns>All items for PayPal Standard API</returns>
		public List<PayPalLineItem> GetLineItems(PostProcessPaymentRequest postProcessPaymentRequest, out decimal cartTotal)
		{
			cartTotal = decimal.Zero;

			var order = postProcessPaymentRequest.Order;
			var lst = new List<PayPalLineItem>();

			// order items
			foreach (var orderItem in order.OrderItems)
			{
				var item = new PayPalLineItem()
				{
					Type = PayPalItemType.CartItem,
					Name = orderItem.Product.GetLocalized(x => x.Name),
					Quantity = orderItem.Quantity,
					Amount = orderItem.UnitPriceExclTax
				};
				lst.Add(item);

				cartTotal += orderItem.PriceExclTax;
			}

			// checkout attributes.... are included in order total
			//foreach (var caValue in checkoutAttributeValues)
			//{
			//	var attributePrice = _taxService.GetCheckoutAttributePrice(caValue, false, order.Customer);

			//	if (attributePrice > decimal.Zero && caValue.CheckoutAttribute != null)
			//	{
			//		var item = new PayPalLineItem()
			//		{
			//			Type = PayPalItemType.CheckoutAttribute,
			//			Name = caValue.CheckoutAttribute.GetLocalized(x => x.Name),
			//			Quantity = 1,
			//			Amount = attributePrice
			//		};
			//		lst.Add(item);

			//		cartTotal += attributePrice;
			//	}
			//}

			// shipping
			if (order.OrderShippingExclTax > decimal.Zero)
			{
				var item = new PayPalLineItem()
				{
					Type = PayPalItemType.Shipping,
					Name = T("Plugins.Payments.PayPalStandard.ShippingFee").Text,
					Quantity = 1,
					Amount = order.OrderShippingExclTax
				};
				lst.Add(item);

				cartTotal += order.OrderShippingExclTax;
			}

			// payment fee
			if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
			{
				var item = new PayPalLineItem()
				{
					Type = PayPalItemType.PaymentFee,
					Name = T("Plugins.Payments.PayPalStandard.PaymentMethodFee").Text,
					Quantity = 1,
					Amount = order.PaymentMethodAdditionalFeeExclTax
				};
				lst.Add(item);

				cartTotal += order.PaymentMethodAdditionalFeeExclTax;
			}

			// tax
			if (order.OrderTax > decimal.Zero)
			{
				var item = new PayPalLineItem()
				{
					Type = PayPalItemType.Tax,
					Name = T("Plugins.Payments.PayPalStandard.SalesTax").Text,
					Quantity = 1,
					Amount = order.OrderTax
				};
				lst.Add(item);

				cartTotal += order.OrderTax;
			}

			return lst;
		}

		/// <summary>
		/// Manually adjusts the net prices for cart items to avoid rounding differences with the PayPal API.
		/// </summary>
		/// <param name="paypalItems">PayPal line items</param>
		/// <param name="postProcessPaymentRequest">Post process paymenmt request object</param>
		/// <remarks>
		/// In detail: We add what we have thrown away in the checkout when we rounded prices to two decimal places.
		/// It's a workaround. Better solution would be to store the thrown away decimal places for each OrderItem in the database.
		/// More details: http://magento.xonu.de/magento-extensions/empfehlungen/magento-paypal-rounding-error-fix/
		/// </remarks>
		public void AdjustLineItemAmounts(List<PayPalLineItem> paypalItems, PostProcessPaymentRequest postProcessPaymentRequest)
		{
			try
			{
				var cartItems = paypalItems.Where(x => x.Type == PayPalItemType.CartItem);

				if (cartItems.Count() <= 0)
					return;

				decimal totalSmartStore = Math.Round(postProcessPaymentRequest.Order.OrderSubtotalExclTax, 2);
				decimal totalPayPal = decimal.Zero;
				decimal delta, portion, rest;

				// calculate what PayPal calculates
				cartItems.Each(x => totalPayPal += (x.AmountRounded * x.Quantity));
				totalPayPal = Math.Round(totalPayPal, 2, MidpointRounding.AwayFromZero);

				// calculate difference
				delta = Math.Round(totalSmartStore - totalPayPal, 2);
				if (delta == decimal.Zero)
					return;

				// prepare lines... only lines with quantity = 1 are adjustable. if there is no one, create one.
				if (!cartItems.Any(x => x.Quantity == 1))
				{
					var item = cartItems.First(x => x.Quantity > 1);
					item.Quantity -= 1;
					var newItem = item.Clone();
					newItem.Quantity = 1;
					paypalItems.Insert(paypalItems.IndexOf(item) + 1, newItem);
				}

				var cartItemsOneQuantity = paypalItems.Where(x => x.Type == PayPalItemType.CartItem && x.Quantity == 1);
				Debug.Assert(cartItemsOneQuantity.Count() > 0);

				SplitDifference(delta, cartItemsOneQuantity.Count(), out portion, out rest);

				if (portion != decimal.Zero)
				{
					cartItems
						.Where(x => x.Quantity == 1)
						.Each(x => x.Amount = x.Amount + portion);
				}

				if (rest != decimal.Zero)
				{
					var restItem = cartItems.First(x => x.Quantity == 1);
					restItem.Amount = restItem.Amount + rest;
				}

				//"SM: {0}, PP: {1}, delta: {2} (portion: {3}, rest: {4})".FormatWith(totalSmartStore, totalPayPal, delta, portion, rest).Dump();
			}
			catch (Exception exc)
			{
				Logger.Error(exc.Message, exc);
			}
		}
	}
}
