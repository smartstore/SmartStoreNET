using SmartStore.Core.Domain.Common;
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

		public Customer GetCustomer(int key)
		{
			return GetExpandedProperty<Customer>(key, x => x.Customer);
		}

		public Address GetBillingAddress(int key)
		{
			return GetExpandedProperty<Address>(key, x => x.BillingAddress);
		}

		public Address GetShippingAddress(int key)
		{
			return GetExpandedProperty<Address>(key, x => x.ShippingAddress);
		}

		[WebApiQueryable]
		public IQueryable<OrderNote> GetOrderNotes(int key)
		{
			var entity = GetExpandedEntity<ICollection<OrderNote>>(key, x => x.OrderNotes);
			//var entity = GetEntityByKeyNotNull(key);	// if ProxyCreationEnabled = true

			return entity.OrderNotes.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<Shipment> GetShipments(int key)
		{
			var entity = GetExpandedEntity<ICollection<Shipment>>(key, x => x.Shipments);

			return entity.Shipments.AsQueryable();
		}

		[WebApiQueryable]
		public IQueryable<OrderItem> GetOrderItems(int key)
		{
			var entity = GetExpandedEntity<ICollection<OrderItem>>(key, x => x.OrderItems);

			return entity.OrderItems.AsQueryable();
		}

		// actions

		[HttpPost]
		public Order PaymentPending(int key)
		{
			var entity = GetEntityByKeyNotNull(key);

			this.ProcessEntity(() =>
			{
				entity.PaymentStatus = PaymentStatus.Pending;
				Service.UpdateOrder(entity);
				return null;
			});

			return entity;
		}

		[HttpPost]
		public Order PaymentPaid(int key, ODataActionParameters parameters)
		{
			var entity = GetEntityByKeyNotNull(key);

			this.ProcessEntity(() =>
			{
				string paymentMethodName = parameters.GetValue<string, string>("PaymentMethodName");

				if (paymentMethodName != null)
				{
					entity.PaymentMethodSystemName = paymentMethodName;
					Service.UpdateOrder(entity);
				}

				_orderProcessingService.Value.MarkOrderAsPaid(entity);
				return null;
			});
			return entity;
		}

		[HttpPost]
		public Order PaymentRefund(int key, ODataActionParameters parameters)
		{
			var entity = GetEntityByKeyNotNull(key);

			this.ProcessEntity(() =>
			{
				bool online = parameters.GetValue<string, bool>("Online");
				
				if (online)
				{
					var errors = _orderProcessingService.Value.Refund(entity);

					if (errors.Count > 0)
						return errors[0];
				}
				else
				{
					_orderProcessingService.Value.RefundOffline(entity);
				}
				return null;
			});
			return entity;
		}

		[HttpPost]
		public Order Cancel(int key)
		{
			var entity = GetEntityByKeyNotNull(key);

			this.ProcessEntity(() =>
			{
				_orderProcessingService.Value.CancelOrder(entity, true);

				return null;
			});
			return entity;
		}
	}
}
