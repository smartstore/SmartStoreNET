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
using SmartStore.Data.Caching;

namespace SmartStore.Services.Orders
{
    public partial class OrderService : IOrderService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<RecurringPayment> _recurringPaymentRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<ReturnRequest> _returnRequestRepository;
        private readonly IEventPublisher _eventPublisher;

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

        public virtual Order GetOrderById(int orderId)
        {
            if (orderId == 0)
                return null;

            return _orderRepository.GetByIdCached(orderId, "db.order.id-" + orderId);
        }

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
            string billingName = null,
            string[] paymentMethods = null)
        {
            var query = _orderRepository.Table;

            if (customerId > 0)
                query = query.Where(x => x.CustomerId == customerId);

            if (storeId > 0)
                query = query.Where(x => x.StoreId == storeId);

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

            if (paymentMethods != null && paymentMethods.Any())
            {
                query = query.Where(x => paymentMethods.Contains(x.PaymentMethodSystemName));
            }

            query = query.Where(x => !x.Deleted);

            return query;
        }

        public virtual IPagedList<Order> SearchOrders(int storeId, int customerId, DateTime? startTime, DateTime? endTime,
            int[] orderStatusIds, int[] paymentStatusIds, int[] shippingStatusIds,
            string billingEmail, string orderGuid, string orderNumber, int pageIndex, int pageSize, string billingName = null,
            string[] paymentMethods = null)
        {
            var query = GetOrders(storeId, customerId, startTime, endTime, orderStatusIds, paymentStatusIds, shippingStatusIds,
                billingEmail, orderNumber, billingName, paymentMethods);

            query = query.OrderByDescending(x => x.CreatedOnUtc);

            if (orderGuid.HasValue())
            {
                // Filter by GUID. Filter in BLL because EF doesn't support casting of GUID to string
                var orders = query.ToList();
                orders = orders.FindAll(x => x.OrderGuid.ToString().ToLowerInvariant().Contains(orderGuid.ToLowerInvariant()));

                return new PagedList<Order>(orders, pageIndex, pageSize);
            }
            else
            {
                return new PagedList<Order>(query, pageIndex, pageSize);
            }
        }

        public virtual IPagedList<Order> GetAllOrders(int affiliateId, int pageIndex, int pageSize)
        {
            var query = _orderRepository.Table;
            query = query.Where(o => !o.Deleted);
            query = query.Where(o => o.AffiliateId == affiliateId);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            var orders = new PagedList<Order>(query, pageIndex, pageSize);
            return orders;
        }

        public virtual IList<Order> GetOrdersByAffiliateId(int affiliateId)
        {
            var query = from o in _orderRepository.Table
                        orderby o.CreatedOnUtc descending
                        where !o.Deleted && o.AffiliateId == affiliateId
                        select o;
            var orders = query.ToList();
            return orders;
        }

        public virtual void InsertOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            _orderRepository.Insert(order);
        }

        public virtual void UpdateOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            _orderRepository.Update(order);

            _eventPublisher.PublishOrderUpdated(order);
        }

        public virtual void DeleteOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            order.Deleted = true;
            UpdateOrder(order);
        }

        public virtual void DeleteOrderNote(OrderNote orderNote)
        {
            if (orderNote == null)
                throw new ArgumentNullException("orderNote");

            int orderId = orderNote.OrderId;

            _orderNoteRepository.Delete(orderNote);

            var order = GetOrderById(orderId);
            _eventPublisher.PublishOrderUpdated(order);
        }

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

        #region Order items

        public virtual OrderItem GetOrderItemById(int orderItemId)
        {
            if (orderItemId == 0)
                return null;

            return _orderItemRepository.GetById(orderItemId);
        }

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

        public virtual void DeleteRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            recurringPayment.Deleted = true;
            UpdateRecurringPayment(recurringPayment);
        }

        public virtual RecurringPayment GetRecurringPaymentById(int recurringPaymentId)
        {
            if (recurringPaymentId == 0)
                return null;

            return _recurringPaymentRepository.GetById(recurringPaymentId);
        }

        public virtual void InsertRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            _recurringPaymentRepository.Insert(recurringPayment);

            _eventPublisher.PublishOrderUpdated(recurringPayment.InitialOrder);
        }

        public virtual void UpdateRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");

            _recurringPaymentRepository.Update(recurringPayment);

            _eventPublisher.PublishOrderUpdated(recurringPayment.InitialOrder);
        }

        public virtual IPagedList<RecurringPayment> SearchRecurringPayments(
            int storeId,
            int customerId,
            int initialOrderId,
            OrderStatus? initialOrderStatus,
            bool showHidden = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            int? initialOrderStatusId = null;

            if (initialOrderStatus.HasValue)
            {
                initialOrderStatusId = (int)initialOrderStatus.Value;
            }

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

            return new PagedList<RecurringPayment>(query2, pageIndex, pageSize);
        }

        #endregion

        #region Return requests

        public virtual void DeleteReturnRequest(ReturnRequest returnRequest)
        {
            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            int orderItemId = returnRequest.OrderItemId;

            _returnRequestRepository.Delete(returnRequest);

            var orderItem = GetOrderItemById(orderItemId);
            _eventPublisher.PublishOrderUpdated(orderItem.Order);
        }

        public virtual ReturnRequest GetReturnRequestById(int returnRequestId)
        {
            if (returnRequestId == 0)
                return null;

            return _returnRequestRepository.GetById(returnRequestId);
        }

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
    }
}
