using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Hooks
{
	public class CapturePaymentHook : DbSaveHook<Order>
	{
		private readonly Lazy<IOrderProcessingService> _orderProcessingService;
		private readonly Lazy<PaymentSettings> _paymentSettings;
		private readonly HashSet<Order> _toCapture = new HashSet<Order>();

		public CapturePaymentHook(
			Lazy<IOrderProcessingService> orderProcessingService,
			Lazy<PaymentSettings> paymentSettings)
		{
			_orderProcessingService = orderProcessingService;
			_paymentSettings = paymentSettings;
		}

		private bool IsStatusPropertyModifiedTo(IHookedEntity entry, string propertyName, int statusId)
		{
			var prop = entry.Entry.Property(propertyName);

			if (prop != null && prop.CurrentValue != null)
			{
				if (!prop.CurrentValue.Equals(prop.OriginalValue))
				{
					return (int)prop.CurrentValue == statusId;
				}
			}

			return false;
		}

		protected override void OnUpdating(Order entity, IHookedEntity entry)
		{
			if (entry.State == Core.Data.EntityState.Modified)
			{
				if (IsStatusPropertyModifiedTo(entry, nameof(entity.ShippingStatusId), (int)ShippingStatus.Shipped))
				{
					if (_paymentSettings.Value.CapturePaymentReason.HasValue && _paymentSettings.Value.CapturePaymentReason.Value == CapturePaymentReason.OrderShipped)
					{
						_toCapture.Add(entity);
					}
				}

				if (IsStatusPropertyModifiedTo(entry, nameof(entity.ShippingStatusId), (int)ShippingStatus.Delivered))
				{
					if (_paymentSettings.Value.CapturePaymentReason.HasValue && _paymentSettings.Value.CapturePaymentReason.Value == CapturePaymentReason.OrderDelivered)
					{
						_toCapture.Add(entity);
					}
				}

				//if (IsStatusPropertyModifiedTo(entry, nameof(entity.OrderStatusId), (int)OrderStatus.Complete))
				//{
				// That's too late. The payment is already marked as paid and the capture process would never be executed.
				//}
			}
		}

		public override void OnAfterSave(IHookedEntity entry)
		{
			// Do not remove.
		}

		public override void OnAfterSaveCompleted()
		{
			if (_toCapture.Any())
			{
				foreach (var order in _toCapture)
				{
					if (_orderProcessingService.Value.CanCapture(order))
					{
						_orderProcessingService.Value.Capture(order);
					}
				}

				_toCapture.Clear();
			}
		}
	}
}
