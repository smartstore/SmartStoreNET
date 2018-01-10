using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Events;

namespace SmartStore.Services.Orders
{
	/// <summary>
	/// Order service
	/// </summary>
	public partial class OrderService : IOrderService
    {
        #region Fields

        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
		private readonly IRepository<Product> _productRepository;
        private readonly IRepository<RecurringPayment> _recurringPaymentRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<ReturnRequest> _returnRequestRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="orderItemRepository">Order item repository</param>
        /// <param name="orderNoteRepository">Order note repository</param>
		/// <param name="productRepository">Product repository</param>
        /// <param name="recurringPaymentRepository">Recurring payment repository</param>
        /// <param name="customerRepository">Customer repository</param>
        /// <param name="returnRequestRepository">Return request repository</param>
        /// <param name="eventPublisher">Event published</param>
        public OrderService(IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<OrderNote> orderNoteRepository,
			IRepository<Product> productRepository,
            IRepository<RecurringPayment> recurringPaymentRepository,
            IRepository<Customer> customerRepository, 
            IRepository<ReturnRequest> returnRequestRepository,
            IEventPublisher eventPublisher)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _orderNoteRepository = orderNoteRepository;
			_productRepository = productRepository;
            _recurringPaymentRepository = recurringPaymentRepository;
            _customerRepository = customerRepository;
            _returnRequestRepository = returnRequestRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        #region Orders

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>Order</returns>
        public virtual Order GetOrderById(int orderId)
        {
            if (orderId == 0)
                return null;

            return _orderRepository.GetById(orderId);
        }

        /// <summary>
        /// Get orders by identifiers
        /// </summary>
        /// <param name="orderIds">Order identifiers</param>
        /// <returns>Order</returns>
        public virtual IList<Order> GetOrdersByIds(int[] orderIds)
        {
            if (orderIds == null || orderIds.Length == 0)
                return new List<Order>();

            var query = from o in _orderRepository.Table
                        where orderIds.Contains(o.Id)
                        select o;
            var orders = query.ToList();

			// sort by passed identifier sequence
			return orders.OrderBySequence(orderIds).ToList();
		}

        public virtual Order GetOrderByNumber(string orderNumber)
        {
            if (orderNumber.IsEmpty())
            {
                return null;
            }

            var query = from o in _orderRepository.Table
                        where o.OrderNumber == orderNumber
                        select o;
            var order = query.FirstOrDefault();

            int id = 0;
            if (order == null && int.TryParse(orderNumber, out id) && id > 0)
            {
                return this.GetOrderById(id);
            }

            return order;
        }

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderGuid">The order identifier</param>
        /// <returns>Order</returns>
        public virtual Order GetOrderByGuid(Guid orderGuid)
        {
            if (orderGuid == Guid.Empty)
                return null;

            var query = from o in _orderRepository.Table
                        where o.OrderGuid == orderGuid
                        select o;
            var order = query.FirstOrDefault();
            return order;
        }

		/// <summary>
		/// Get order by payment authorization data
		/// </summary>
		/// <param name="paymentMethodSystemName">System name of the payment method</param>
		/// <param name="authorizationTransactionId">Authorization transaction Id</param>
		/// <returns>Order entity</returns>
		public virtual Order GetOrderByPaymentAuthorization(string paymentMethodSystemName, string authorizationTransactionId)
		{
			if (paymentMethodSystemName.IsEmpty() || authorizationTransactionId.IsEmpty())
				return null;

			var query =
				from o in _orderRepository.Table
				where o.PaymentMethodSystemName == paymentMethodSystemName && o.AuthorizationTransactionId == authorizationTransactionId
				select o;

			return query.FirstOrDefault();
		}

		/// <summary>
		/// Get order by payment capture data
		/// </summary>
		/// <param name="paymentMethodSystemName">System name of the payment method</param>
		/// <param name="captureTransactionId">Capture transaction Id</param>
		/// <returns>Order entity</returns>
		public virtual Order GetOrderByPaymentCapture(string paymentMethodSystemName, string captureTransactionId)
		{
			if (paymentMethodSystemName.IsEmpty() || captureTransactionId.IsEmpty())
				return null;

			var query =
				from o in _orderRepository.Table
				where o.PaymentMethodSystemName == paymentMethodSystemName && o.CaptureTransactionId == captureTransactionId
				select o;

			return query.FirstOrDefault();
		}

		public virtual IQueryable<Order> GetOrders(
			int storeId,
			int customerId,
			DateTime? startTime,
			DateTime? endTime,
			int[] orderStatusIds,
			int[] paymentStatusIds,
			int[] shippingStatusIds,
			string billingEmail,
			string orderNumber,
			string billingName = null)
		{
			var query = _orderRepository.Table;

			if (storeId > 0)
				query = query.Where(x => x.StoreId == storeId);

			if (customerId > 0)
				query = query.Where(x => x.CustomerId == customerId);

			if (startTime.HasValue)
				query = query.Where(x => startTime.Value <= x.CreatedOnUtc);

			if (endTime.HasValue)
				query = query.Where(x => endTime.Value >= x.CreatedOnUtc);

			if (billingEmail.HasValue())
				query = query.Where(x => x.BillingAddress != null && !String.IsNullOrEmpty(x.BillingAddress.Email) && x.BillingAddress.Email.Contains(billingEmail));

			if (billingName.HasValue())
			{
				query = query.Where(x => x.BillingAddress != null && (
					(!String.IsNullOrEmpty(x.BillingAddress.LastName) && x.BillingAddress.LastName.Contains(billingName)) ||
					(!String.IsNullOrEmpty(x.BillingAddress.FirstName) && x.BillingAddress.FirstName.Contains(billingName))
				));
			}

			if (orderNumber.HasValue())
				query = query.Where(x => x.OrderNumber.ToLower().Contains(orderNumber.ToLower()));

			if (orderStatusIds != null && orderStatusIds.Count() > 0)
				query = query.Where(x => orderStatusIds.Contains(x.OrderStatusId));

			if (paymentStatusIds != null && paymentStatusIds.Count() > 0)
				query = query.Where(x => paymentStatusIds.Contains(x.PaymentStatusId));

			if (shippingStatusIds != null && shippingStatusIds.Count() > 0)
				query = query.Where(x => shippingStatusIds.Contains(x.ShippingStatusId));

			query = query.Where(x => !x.Deleted);

			return query;
		}

        /// <summary>
        /// Search orders
        /// </summary>
		/// <param name="storeId">Store identifier; 0 to load all orders</param>
		/// <param name="customerId">Customer identifier; 0 to load all orders</param>
        /// <param name="startTime">Order start time; null to load all orders</param>
        /// <param name="endTime">Order end time; null to load all orders</param>
		/// <param name="orderStatusIds">Filter by order status</param>
		/// <param name="paymentStatusIds">Filter by payment status</param>
		/// <param name="shippingStatusIds">Filter by shipping status</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="orderGuid">Search by order GUID (Global unique identifier) or part of GUID. Leave empty to load all orders.</param>
		/// <param name="orderNumber">Filter by order number</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
		/// <param name="billingName">Billing name. Leave empty to load all records.</param>
        /// <returns>Order collection</returns>
		public virtual IPagedList<Order> SearchOrders(int storeId, int customerId, DateTime? startTime, DateTime? endTime, 
			int[] orderStatusIds, int[] paymentStatusIds, int[] shippingStatusIds,
			string billingEmail, string orderGuid, string orderNumber, int pageIndex, int pageSize, string billingName = null)
        {
			var query = GetOrders(storeId, customerId, startTime, endTime, orderStatusIds, paymentStatusIds, shippingStatusIds,
				billingEmail, orderNumber, billingName);

			query = query.OrderByDescending(x => x.CreatedOnUtc);

			if (orderGuid.HasValue())
			{
				//filter by GUID. Filter in BLL because EF doesn't support casting of GUID to string
				var orders = query.ToList();
				orders = orders.FindAll(x => x.OrderGuid.ToString().ToLowerInvariant().Contains(orderGuid.ToLowerInvariant()));

				return new PagedList<Order>(orders, pageIndex, pageSize);
			}
			else
			{
				//database layer paging
				return new PagedList<Order>(query, pageIndex, pageSize);
			}  
        }

        /// <summary>
        /// Gets all orders by affiliate identifier
        /// </summary>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Orders</returns>
        public virtual IPagedList<Order> GetAllOrders(int affiliateId, int pageIndex, int pageSize)
        {
            var query = _orderRepository.Table;
            query = query.Where(o => !o.Deleted);
            query = query.Where(o => o.AffiliateId == affiliateId);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            var orders = new PagedList<Order>(query, pageIndex, pageSize);
            return orders;
        }

        /// <summary>
        /// Load all orders
        /// </summary>
        /// <returns>Order collection</returns>
        public virtual IList<Order> LoadAllOrders()
        {
            return SearchOrders(0, 0, null, null, null, null, null, null, null, null, 0, int.MaxValue);
        }

        /// <summary>
        /// Gets all orders by affiliate identifier
        /// </summary>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <returns>Order collection</returns>
        public virtual IList<Order> GetOrdersByAffiliateId(int affiliateId)
        {
            var query = from o in _orderRepository.Table
                        orderby o.CreatedOnUtc descending
                        where !o.Deleted && o.AffiliateId == affiliateId
                        select o;
            var orders = query.ToList();
            return orders;
        }

        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void InsertOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            _orderRepository.Insert(order);
        }

        /// <summary>
        /// Updates the order
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void UpdateOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            _orderRepository.Update(order);

			_eventPublisher.PublishOrderUpdated(order);
        }

		/// <summary>
		/// Deletes an order
		/// </summary>
		/// <param name="order">The order</param>
		public virtual void DeleteOrder(Order order)
		{
			if (order == null)
				throw new ArgumentNullException("order");

			order.Deleted = true;
			UpdateOrder(order);
		}

        /// <summary>
        /// Deletes an order note
        /// </summary>
        /// <param name="orderNote">The order note</param>
        public virtual void DeleteOrderNote(OrderNote orderNote)
        {
            if (orderNote == null)
                throw new ArgumentNullException("orderNote");

			int orderId = orderNote.OrderId;

            _orderNoteRepository.Delete(orderNote);

			var order = GetOrderById(orderId);
			_eventPublisher.PublishOrderUpdated(order);
        }

        /// <summary>
        /// Get an order by authorization transaction ID and payment method system name
        /// </summary>
        /// <param name="authorizationTransactionId">Authorization transaction ID</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>Order</returns>
        public virtual Order GetOrderByAuthorizationTransactionIdAndPaymentMethod(string authorizationTransactionId, string paymentMethodSystemName)
        { 
            var query = _orderRepository.Table;
            if (!String.IsNullOrWhiteSpace(authorizationTransactionId))
                query = query.Where(o => o.AuthorizationTransactionId == authorizationTransactionId);
            
            if (!String.IsNullOrWhiteSpace(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);
            
            query = query.OrderByDescending(o => o.CreatedOnUtc);
            var order = query.FirstOrDefault();
            return order;
        }

		public virtual void AddOrderNote(Order order, string note, bool displayToCustomer = false)
		{
			if (order != null && note.HasValue())
			{
				order.OrderNotes.Add(new OrderNote
				{
					Note = note,
					DisplayToCustomer = displayToCustomer,
					CreatedOnUtc = DateTime.UtcNow
				});

				UpdateOrder(order);
			}
		}

		#endregion

		#region Order items

		/// <summary>
		/// Gets an Order item
		/// </summary>
		/// <param name="orderItemId">Order item identifier</param>
		/// <returns>Order item</returns>
		public virtual OrderItem GetOrderItemById(int orderItemId)
        {
            if (orderItemId == 0)
                return null;

            return _orderItemRepository.GetById(orderItemId);
        }

        /// <summary>
        /// Gets an Order item
        /// </summary>
        /// <param name="orderItemGuid">Order item identifier</param>
        /// <returns>Order item</returns>
        public virtual OrderItem GetOrderItemByGuid(Guid orderItemGuid)
        {
            if (orderItemGuid == Guid.Empty)
                return null;
            
            var query = from orderItem in _orderItemRepository.Table
                        where orderItem.OrderItemGuid == orderItemGuid
                        select orderItem;
            var item = query.FirstOrDefault();
            return item;
        }
        
        /// <summary>
        /// Gets all Order items
        /// </summary>
        /// <param name="orderId">Order identifier; null to load all records</param>
        /// <param name="customerId">Customer identifier; null to load all records</param>
        /// <param name="startTime">Order start time; null to load all records</param>
        /// <param name="endTime">Order end time; null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Order shippment status; null to load all records</param>
        /// <param name="loadDownloableProductsOnly">Value indicating whether to load downloadable products only</param>
        /// <returns>Order collection</returns>
        public virtual IList<OrderItem> GetAllOrderItems(int? orderId,
            int? customerId, DateTime? startTime, DateTime? endTime,
            OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss,
            bool loadDownloableProductsOnly)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;
            

            var query = from orderItem in _orderItemRepository.Table
                        join o in _orderRepository.Table on orderItem.OrderId equals o.Id
						join p in _productRepository.Table on orderItem.ProductId equals p.Id
                        where (!orderId.HasValue || orderId.Value == 0 || orderId == o.Id) &&
                        (!customerId.HasValue || customerId.Value == 0 || customerId == o.CustomerId) &&
                        (!startTime.HasValue || startTime.Value <= o.CreatedOnUtc) &&
                        (!endTime.HasValue || endTime.Value >= o.CreatedOnUtc) &&
                        (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
                        (!paymentStatusId.HasValue || paymentStatusId.Value == o.PaymentStatusId) &&
                        (!shippingStatusId.HasValue || shippingStatusId.Value == o.ShippingStatusId) &&
                        (!loadDownloableProductsOnly || p.IsDownload) &&
                        !o.Deleted
                        orderby o.CreatedOnUtc descending, orderItem.Id
                        select orderItem;

            var orderItems = query.ToList();
            return orderItems;
        }

		public virtual Multimap<int, OrderItem> GetOrderItemsByOrderIds(int[] orderIds)
		{
			Guard.NotNull(orderIds, nameof(orderIds));

			var query =
				from x in _orderItemRepository.TableUntracked.Expand(x => x.Product)
				where orderIds.Contains(x.OrderId)
				select x;

			var map = query
				.OrderBy(x => x.OrderId)
				.ToList()
				.ToMultimap(x => x.OrderId, x => x);

			return map;
		}

        /// <summary>
        /// Delete an Order item
        /// </summary>
        /// <param name="orderItem">The Order item</param>
        public virtual void DeleteOrderItem(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException("orderItem");

			int orderId = orderItem.OrderId;

            _orderItemRepository.Delete(orderItem);

			var order = GetOrderById(orderId);
			_eventPublisher.PublishOrderUpdated(order);
        }

        #endregion
        
        #region Recurring payments

        /// <summary>
        /// Deletes a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual void DeleteRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            recurringPayment.Deleted = true;
            UpdateRecurringPayment(recurringPayment);
        }

        /// <summary>
        /// Gets a recurring payment
        /// </summary>
        /// <param name="recurringPaymentId">The recurring payment identifier</param>
        /// <returns>Recurring payment</returns>
        public virtual RecurringPayment GetRecurringPaymentById(int recurringPaymentId)
        {
            if (recurringPaymentId == 0)
                return null;

           return _recurringPaymentRepository.GetById(recurringPaymentId);
        }

        /// <summary>
        /// Inserts a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual void InsertRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            _recurringPaymentRepository.Insert(recurringPayment);

			_eventPublisher.PublishOrderUpdated(recurringPayment.InitialOrder);
        }

        /// <summary>
        /// Updates the recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual void UpdateRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            _recurringPaymentRepository.Update(recurringPayment);

			_eventPublisher.PublishOrderUpdated(recurringPayment.InitialOrder);
        }

        /// <summary>
        /// Search recurring payments
        /// </summary>
		/// <param name="storeId">The store identifier; 0 to load all records</param>
        /// <param name="customerId">The customer identifier; 0 to load all records</param>
        /// <param name="initialOrderId">The initial order identifier; 0 to load all records</param>
        /// <param name="initialOrderStatus">Initial order status identifier; null to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Recurring payment collection</returns>
		public virtual IList<RecurringPayment> SearchRecurringPayments(int storeId, 
			int customerId, int initialOrderId, OrderStatus? initialOrderStatus,
			bool showHidden = false)
        {
            int? initialOrderStatusId = null;
            if (initialOrderStatus.HasValue)
                initialOrderStatusId = (int)initialOrderStatus.Value;

            var query1 = from rp in _recurringPaymentRepository.Table
                         join c in _customerRepository.Table on rp.InitialOrder.CustomerId equals c.Id
                         where
                         (!rp.Deleted) &&
                         (showHidden || !rp.InitialOrder.Deleted) &&
                         (showHidden || !c.Deleted) &&
                         (showHidden || rp.IsActive) &&
                         (customerId == 0 || rp.InitialOrder.CustomerId == customerId) &&
						 (storeId == 0 || rp.InitialOrder.StoreId == storeId) &&
                         (initialOrderId == 0 || rp.InitialOrder.Id == initialOrderId) &&
                         (!initialOrderStatusId.HasValue || initialOrderStatusId.Value == 0 || rp.InitialOrder.OrderStatusId == initialOrderStatusId.Value)
                         select rp.Id;

            var query2 = from rp in _recurringPaymentRepository.Table
                         where query1.Contains(rp.Id)
                         orderby rp.StartDateUtc, rp.Id
                         select rp;
            
            var recurringPayments = query2.ToList();
            return recurringPayments;
        }

        #endregion

        #region Return requests

        /// <summary>
        /// Deletes a return request
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        public virtual void DeleteReturnRequest(ReturnRequest returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

			int orderItemId = returnRequest.OrderItemId;

            _returnRequestRepository.Delete(returnRequest);

			var orderItem = GetOrderItemById(orderItemId);
			_eventPublisher.PublishOrderUpdated(orderItem.Order);
        }

        /// <summary>
        /// Gets a return request
        /// </summary>
        /// <param name="returnRequestId">Return request identifier</param>
        /// <returns>Return request</returns>
        public virtual ReturnRequest GetReturnRequestById(int returnRequestId)
        {
            if (returnRequestId == 0)
                return null;

            return _returnRequestRepository.GetById(returnRequestId);
        }

        /// <summary>
        /// Search return requests
        /// </summary>
		/// <param name="storeId">Store identifier; 0 to load all entries</param>
        /// <param name="customerId">Customer identifier; null to load all entries</param>
        /// <param name="orderItemId">Order item identifier; null to load all entries</param>
        /// <param name="rs">Return request status; null to load all entries</param>
		/// <param name="pageIndex">Page index</param>
		/// <param name="pageSize">Page size</param>
		/// <param name="id">Return request Id</param>
        /// <returns>Return requests</returns>
		public virtual IPagedList<ReturnRequest> SearchReturnRequests(int storeId, int customerId, int orderItemId, ReturnRequestStatus? rs, int pageIndex, int pageSize, int id = 0)
        {
            var query = _returnRequestRepository.Table;

			if (storeId > 0)
				query = query.Where(rr => storeId == rr.StoreId);
    
			if (customerId > 0)
                query = query.Where(rr => customerId == rr.CustomerId);

			if (orderItemId > 0)
				query = query.Where(rr => rr.OrderItemId == orderItemId);

			if (id != 0)
				query = query.Where(rr => rr.Id == id);

            if (rs.HasValue)
            {
                int returnStatusId = (int)rs.Value;
                query = query.Where(rr => rr.ReturnRequestStatusId == returnStatusId);
            }

            query = query.OrderByDescending(rr => rr.CreatedOnUtc).ThenByDescending(rr => rr.Id);

			var returnRequests = new PagedList<ReturnRequest>(query, pageIndex, pageSize);
            return returnRequests;
        }

        #endregion
        
        #endregion
    }
}
