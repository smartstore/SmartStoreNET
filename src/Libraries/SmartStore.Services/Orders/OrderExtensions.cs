using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Html;
using SmartStore.Services.Directory;
using SmartStore.Services.Payments;

namespace SmartStore.Services.Orders
{
	public static class OrderExtensions
    {
		private static int AggregateQuantity(IEnumerable<Shipment> shipments, OrderItem orderItem)
		{
			var result = 0;

			foreach (var shipment in shipments)
			{
				var item = shipment.ShipmentItems.FirstOrDefault(x => x.OrderItemId == orderItem.Id);
				if (item != null)
				{
					result += item.Quantity;
				}
			}

			return result;
		}

        /// <summary>
        /// Formats the order note text
        /// </summary>
        /// <param name="orderNote">Order note</param>
        /// <returns>Formatted text</returns>
        public static string FormatOrderNoteText(this OrderNote orderNote)
        {
			Guard.NotNull(orderNote, nameof(orderNote));

            if (orderNote.Note.IsEmpty())
                return string.Empty;

            return HtmlUtils.FormatText(orderNote.Note, false, true, true, false, false, false);
        }

		public static List<ProductBundleItemOrderData> GetBundleData(this OrderItem orderItem)
		{
			if (orderItem != null && orderItem.BundleData.HasValue())
			{
				var data = orderItem.BundleData.Convert<List<ProductBundleItemOrderData>>();
				return data;
			}
			return new List<ProductBundleItemOrderData>();
		}

		public static void SetBundleData(this OrderItem orderItem, List<ProductBundleItemOrderData> bundleData)
		{
			string rawData = null;

			if (bundleData != null && bundleData.Count > 0)
				rawData = bundleData.Convert<string>();

			orderItem.BundleData = rawData;
		}

        /// <summary>
        /// Get the order total in the currency of the customer
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="currencyService">Currency service</param>
        /// <param name="paymentService">Payment service</param>
        /// <param name="roundingAmount">Rounding amount</param>
        /// <returns>Order total</returns>
        public static decimal GetOrderTotalInCustomerCurrency(
            this Order order,
            ICurrencyService currencyService,
            IPaymentService paymentService,
            out decimal roundingAmount)
        {
            Guard.NotNull(order, nameof(order));

            roundingAmount = order.OrderTotalRounding;
            var orderTotal = currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);

            // Avoid rounding a rounded value. It would zero roundingAmount.
            if (orderTotal != order.OrderTotal)
            {
                var currency = currencyService.GetCurrencyByCode(order.CustomerCurrencyCode);

                if (currency != null && currency.RoundOrderTotalEnabled && order.PaymentMethodSystemName.HasValue())
                {
                    var pm = paymentService.GetPaymentMethodBySystemName(order.PaymentMethodSystemName);
                    if (pm != null && pm.RoundOrderTotalEnabled)
                    {
                        orderTotal = orderTotal.RoundToNearest(currency, out roundingAmount);
                    }
                }
            }

            return orderTotal;
        }


        /// <summary>
        /// Gets a value indicating whether an order has items to dispatch
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether an order has items to dispatch</returns>
        public static bool HasItemsToDispatch(this Order order)
		{
			Guard.NotNull(order, nameof(order));

			foreach (var orderItem in order.OrderItems.Where(x => x.Product.IsShipEnabled))
			{
				var notDispatchedItems = orderItem.GetNotDispatchedItemsCount();
				if (notDispatchedItems <= 0)
					continue;

				// yes, we have at least one item to ship
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets a value indicating whether an order has items to deliver
		/// </summary>
		/// <param name="order">Order</param>
		/// <returns>A value indicating whether an order has items to deliver</returns>
		public static bool HasItemsToDeliver(this Order order)
		{
			Guard.NotNull(order, nameof(order));

			foreach (var orderItem in order.OrderItems.Where(x => x.Product.IsShipEnabled))
			{
				var dispatchedItems = orderItem.GetDispatchedItemsCount();
				var deliveredItems = orderItem.GetDeliveredItemsCount();

				if (dispatchedItems <= deliveredItems)
					continue;

				// yes, we have at least one item to deliver
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets a value indicating whether an order has items to be added to a shipment
		/// </summary>
		/// <param name="order">Order</param>
		/// <returns>A value indicating whether an order has items to be added to a shipment</returns>
		public static bool CanAddItemsToShipment(this Order order)
		{
			Guard.NotNull(order, nameof(order));

			foreach (var orderItem in order.OrderItems.Where(x => x.Product.IsShipEnabled))
			{
				var canBeAddedToShipment = orderItem.GetItemsCanBeAddedToShipmentCount();
				if (canBeAddedToShipment <= 0)
					continue;

				// yes, we have at least one item to create a new shipment
				return true;
			}
			return false;
		}


		/// <summary>
		/// Gets the total number of items which can be added to new shipments
		/// </summary>
		/// <param name="orderItem">Order item</param>
		/// <returns>Total number of items which can be added to new shipments</returns>
		public static int GetItemsCanBeAddedToShipmentCount(this OrderItem orderItem)
		{
			Guard.NotNull(orderItem, nameof(orderItem));

			var itemsCount = orderItem.GetShipmentItemsCount();

			return Math.Max(orderItem.Quantity - itemsCount, 0);
		}

		/// <summary>
		/// Gets the total number of items in all shipments
		/// </summary>
		/// <param name="orderItem">Order item</param>
		/// <returns>Total number of items in all shipmentss</returns>
		public static int GetShipmentItemsCount(this OrderItem orderItem)
        {
			Guard.NotNull(orderItem, nameof(orderItem));

            var shipments = orderItem.Order.Shipments.ToList();
			return AggregateQuantity(shipments, orderItem);
        }

		/// <summary>
		/// Gets the total number of dispatched items
		/// </summary>
		/// <param name="orderItem">Order item</param>
		/// <returns>Total number of dispatched items</returns>
		public static int GetDispatchedItemsCount(this OrderItem orderItem)
		{
			Guard.NotNull(orderItem, nameof(orderItem));

			var shipments = orderItem.Order.Shipments.ToList();
			return AggregateQuantity(shipments.Where(x => x.ShippedDateUtc.HasValue), orderItem);
		}

		/// <summary>
		/// Gets the total number of not dispatched items
		/// </summary>
		/// <param name="orderItem">Order item</param>
		/// <returns>Total number of not dispatched items</returns>
		public static int GetNotDispatchedItemsCount(this OrderItem orderItem)
        {
			Guard.NotNull(orderItem, nameof(orderItem));

            var shipments = orderItem.Order.Shipments.ToList();
			return AggregateQuantity(shipments.Where(x => !x.ShippedDateUtc.HasValue), orderItem);
        }

		/// <summary>
		/// Gets the total number of already delivered items
		/// </summary>
		/// <param name="orderItem">Order item</param>
		/// <returns>Total number of already delivered items</returns>
		public static int GetDeliveredItemsCount(this OrderItem orderItem)
		{
			Guard.NotNull(orderItem, nameof(orderItem));

			var shipments = orderItem.Order.Shipments.ToList();
			return AggregateQuantity(shipments.Where(x => x.DeliveryDateUtc.HasValue), orderItem);
		}

		/// <summary>
		/// Gets the total number of not delivered items
		/// </summary>
		/// <param name="orderItem">Order item</param>
		/// <returns>Total number of already delivered items</returns>
		public static int GetNotDeliveredItemsCount(this OrderItem orderItem)
		{
			Guard.NotNull(orderItem, nameof(orderItem));

			var shipments = orderItem.Order.Shipments.ToList();
			return AggregateQuantity(shipments.Where(x => !x.DeliveryDateUtc.HasValue), orderItem);
		}
	}
}
