﻿using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services.Orders;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.OData;
using SmartStore.Web.Framework.WebApi.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;
using System;

namespace SmartStore.WebApi.Controllers.OData
{
	[WebApiAuthenticate(Permission = "ManageOrders")]
	public class OrdersController : WebApiEntityController<Order, IOrderService>
	{
		private readonly Lazy<IOrderProcessingService> _orderProcessingService;

		public OrdersController(Lazy<IOrderProcessingService> orderProcessingService)
		{
			_orderProcessingService = orderProcessingService;
		}

		protected override IQueryable<Order> GetEntitySet()
		{
			var query =
				from x in this.Repository.Table
				where !x.Deleted
				select x;

			return query;
		}
		protected override void Insert(Order entity)
		{
			Service.InsertOrder(entity);
		}
		protected override void Update(Order entity)
		{
			Service.UpdateOrder(entity);
		}
		protected override void Delete(Order entity)
		{
			Service.DeleteOrder(entity);
		}

		[WebApiQueryable]
		public SingleResult<Order> GetOrder(int key)
		{
			return GetSingleResult(key);
		}

		// navigation properties

		[WebApiQueryable]
		public SingleResult<Customer> GetCustomer(int key)
		{
			return GetRelatedEntity(key, x => x.Customer);
		}

		[WebApiQueryable]
		public SingleResult<Address> GetBillingAddress(int key)
		{
			return GetRelatedEntity(key, x => x.BillingAddress);
		}

		[WebApiQueryable]
		public SingleResult<Address> GetShippingAddress(int key)
		{
			return GetRelatedEntity(key, x => x.ShippingAddress);
		}

		[WebApiQueryable]
		public IQueryable<OrderNote> GetOrderNotes(int key)
		{
			//var entity = GetEntityByKeyNotNull(key);	// if ProxyCreationEnabled = true
			return GetRelatedCollection(key, x => x.OrderNotes);
		}

		[WebApiQueryable]
		public IQueryable<Shipment> GetShipments(int key)
		{
			return GetRelatedCollection(key, x => x.Shipments);
		}

		[WebApiQueryable]
		public IQueryable<OrderItem> GetOrderItems(int key)
		{
			return GetRelatedCollection(key, x => x.OrderItems);
		}

		// actions

		[HttpPost]
		public SingleResult<Order> PaymentPending(int key)
		{
			var result = GetSingleResult(key);
			var order = GetExpandedEntity(key, result, null);

			this.ProcessEntity(() =>
			{
				order.PaymentStatus = PaymentStatus.Pending;
				Service.UpdateOrder(order);
				return null;
			});

			return result;
		}

		[HttpPost]
		public SingleResult<Order> PaymentPaid(int key, ODataActionParameters parameters)
		{
			var result = GetSingleResult(key);
			var order = GetExpandedEntity(key, result, null);

			this.ProcessEntity(() =>
			{
				string paymentMethodName = parameters.GetValue<string, string>("PaymentMethodName");

				if (paymentMethodName != null)
				{
					order.PaymentMethodSystemName = paymentMethodName;
					Service.UpdateOrder(order);
				}

				_orderProcessingService.Value.MarkOrderAsPaid(order);

				return null;
			});

			return result;
		}

		[HttpPost]
		public SingleResult<Order> PaymentRefund(int key, ODataActionParameters parameters)
		{
			var result = GetSingleResult(key);
			var order = GetExpandedEntity(key, result, null);

			this.ProcessEntity(() =>
			{
				bool online = parameters.GetValue<string, bool>("Online");
				
				if (online)
				{
					var errors = _orderProcessingService.Value.Refund(order);

					if (errors.Count > 0)
						return errors[0];
				}
				else
				{
					_orderProcessingService.Value.RefundOffline(order);
				}
				return null;
			});

			return result;
		}

		[HttpPost]
		public SingleResult<Order> Cancel(int key)
		{
			var result = GetSingleResult(key);
			var order = GetExpandedEntity(key, result, "OrderItems, OrderItems.Product");

			this.ProcessEntity(() =>
			{
				_orderProcessingService.Value.CancelOrder(order, true);

				return null;
			});

			return result;
		}

		[HttpPost]
		public SingleResult<Order> AddShipment(int key, ODataActionParameters parameters)
		{
			var result = GetSingleResult(key);
			var order = GetExpandedEntity(key, result, "OrderItems, OrderItems.Product, Shipments, Shipments.ShipmentItems");

			this.ProcessEntity(() =>
			{
				if (order.HasItemsToAddToShipment())
				{
					var trackingNumber = parameters.GetValue<string, string>("TrackingNumber");

					var shipment = _orderProcessingService.Value.AddShipment(order, trackingNumber, null);

					if (shipment != null)
					{
						if (parameters.ContainsKey("SetAsShipped") && parameters.GetValue<string, bool>("SetAsShipped"))
							_orderProcessingService.Value.Ship(shipment, true);
					}
				}
				return null;
			});

			return result;
		}
	}
}
