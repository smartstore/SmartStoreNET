using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services.Catalog;
using SmartStore.Services.Helpers;

namespace SmartStore.Services.Orders
{
    /// <summary>
    /// Order report service
    /// </summary>
    public partial class OrderReportService : IOrderReportService
    {
        #region Fields

        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Product> _productRepository;

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="orderItemRepository">Order item repository</param>
        /// <param name="productRepository">Product repository</param>
        /// <param name="dateTimeHelper">Datetime helper</param>
        /// <param name="productService">Product service</param>
        public OrderReportService(IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Product> productRepository,
            IDateTimeHelper dateTimeHelper, IProductService productService)
        {
            this._orderRepository = orderRepository;
            this._orderItemRepository = orderItemRepository;
            this._productRepository = productRepository;
            this._dateTimeHelper = dateTimeHelper;
            this._productService = productService;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Get order average report
        /// </summary>
		/// <param name="storeId">Store identifier</param>
		/// <param name="orderStatusIds">Filter by order status</param>
		/// <param name="paymentStatusIds">Filter by payment status</param>
		/// <param name="shippingStatusIds">Filter by shipping status</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="ignoreCancelledOrders">A value indicating whether to ignore cancelled orders</param>
        /// <returns>Result</returns>
		public virtual OrderAverageReportLine GetOrderAverageReportLine(int storeId, int[] orderStatusIds,
            int[] paymentStatusIds, int[] shippingStatusIds, DateTime? startTimeUtc, DateTime? endTimeUtc,
            string billingEmail, bool ignoreCancelledOrders = false)
        {
            var query = _orderRepository.Table;
            query = query.Where(o => !o.Deleted);
			if (storeId > 0)
				query = query.Where(o => o.StoreId == storeId);
            if (ignoreCancelledOrders)
            {
                int cancelledOrderStatusId = (int)OrderStatus.Cancelled;
                query = query.Where(o => o.OrderStatusId != cancelledOrderStatusId);
            }
            if (startTimeUtc.HasValue)
                query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
            if (endTimeUtc.HasValue)
                query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);
            if (!String.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));

			if (orderStatusIds != null && orderStatusIds.Count() > 0)
				query = query.Where(x => orderStatusIds.Contains(x.OrderStatusId));

			if (paymentStatusIds != null && paymentStatusIds.Count() > 0)
				query = query.Where(x => paymentStatusIds.Contains(x.PaymentStatusId));

			if (shippingStatusIds != null && shippingStatusIds.Count() > 0)
				query = query.Where(x => shippingStatusIds.Contains(x.ShippingStatusId));

			var item = (from oq in query
						group oq by 1 into result
						select new { OrderCount = result.Count(), OrderTaxSum = result.Sum(o => o.OrderTax), OrderTotalSum = result.Sum(o => o.OrderTotal) }
					   ).Select(r => new OrderAverageReportLine(){ SumTax = r.OrderTaxSum, CountOrders=r.OrderCount, SumOrders = r.OrderTotalSum}).FirstOrDefault();

			item = item ?? new OrderAverageReportLine() { CountOrders = 0, SumOrders = decimal.Zero, SumTax = decimal.Zero };
            return item;
        }

        /// <summary>
        /// Get order average report
        /// </summary>
		/// <param name="storeId">Store identifier</param>
        /// <param name="os">Order status</param>
        /// <returns>Result</returns>
		public virtual OrderAverageReportLineSummary OrderAverageReport(int storeId, OrderStatus os)
        {
            var item = new OrderAverageReportLineSummary();
            item.OrderStatus = os;

            DateTime nowDt = _dateTimeHelper.ConvertToUserTime(DateTime.Now);
            TimeZoneInfo timeZone = _dateTimeHelper.CurrentTimeZone;
			var orderStatusId = new int[] { (int)os };

            //today
            DateTime t1 = new DateTime(nowDt.Year, nowDt.Month, nowDt.Day);
            if (!timeZone.IsInvalidTime(t1))
            {
                DateTime? startTime1 = _dateTimeHelper.ConvertToUtcTime(t1, timeZone);
                DateTime? endTime1 = null;
				var todayResult = GetOrderAverageReportLine(storeId, orderStatusId, null, null, startTime1, endTime1, null);
                item.SumTodayOrders = todayResult.SumOrders;
                item.CountTodayOrders = todayResult.CountOrders;
            }
            //week
            DayOfWeek fdow = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            DateTime today = new DateTime(nowDt.Year, nowDt.Month, nowDt.Day);
            DateTime t2 = today.AddDays(-(today.DayOfWeek - fdow));
            if (!timeZone.IsInvalidTime(t2))
            {
                DateTime? startTime2 = _dateTimeHelper.ConvertToUtcTime(t2, timeZone);
                DateTime? endTime2 = null;
				var weekResult = GetOrderAverageReportLine(storeId, orderStatusId, null, null, startTime2, endTime2, null);
                item.SumThisWeekOrders = weekResult.SumOrders;
                item.CountThisWeekOrders = weekResult.CountOrders;
            }
            //month
            DateTime t3 = new DateTime(nowDt.Year, nowDt.Month, 1);
            if (!timeZone.IsInvalidTime(t3))
            {
                DateTime? startTime3 = _dateTimeHelper.ConvertToUtcTime(t3, timeZone);
                DateTime? endTime3 = null;
				var monthResult = GetOrderAverageReportLine(storeId, orderStatusId, null, null, startTime3, endTime3, null);
                item.SumThisMonthOrders = monthResult.SumOrders;
                item.CountThisMonthOrders = monthResult.CountOrders;
            }
            //year
            DateTime t4 = new DateTime(nowDt.Year, 1, 1);
            if (!timeZone.IsInvalidTime(t4))
            {
                DateTime? startTime4 = _dateTimeHelper.ConvertToUtcTime(t4, timeZone);
                DateTime? endTime4 = null;
				var yearResult = GetOrderAverageReportLine(storeId, orderStatusId, null, null, startTime4, endTime4, null);
                item.SumThisYearOrders = yearResult.SumOrders;
                item.CountThisYearOrders = yearResult.CountOrders;
            }
            //all time
            DateTime? startTime5 = null;
            DateTime? endTime5 = null;
			var allTimeResult = GetOrderAverageReportLine(storeId, orderStatusId, null, null, startTime5, endTime5, null);
            item.SumAllTimeOrders = allTimeResult.SumOrders;
            item.CountAllTimeOrders = allTimeResult.CountOrders;

            return item;
        }

        /// <summary>
        /// Get best sellers report
        /// </summary>
		/// <param name="storeId">Store identifier</param>
        /// <param name="startTime">Order start time; null to load all</param>
        /// <param name="endTime">Order end time; null to load all</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Shipping status; null to load all records</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all records</param>
        /// <param name="recordsToReturn">Records to return</param>
        /// <param name="orderBy">1 - order by quantity, 2 - order by total amount</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Result</returns>
		public virtual IList<BestsellersReportLine> BestSellersReport(int storeId, 
			DateTime? startTime, DateTime? endTime,
			OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss,
            int billingCountryId = 0,
            int recordsToReturn = 5, int orderBy = 1, bool showHidden = false)
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


            var query1 = from orderItem in _orderItemRepository.Table
                         join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                         join p in _productRepository.Table on orderItem.ProductId equals p.Id
						 where (storeId == 0 || storeId == o.StoreId) &&
						 (!startTime.HasValue || startTime.Value <= o.CreatedOnUtc) &&
                         (!endTime.HasValue || endTime.Value >= o.CreatedOnUtc) &&
                         (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
                         (!paymentStatusId.HasValue || paymentStatusId == o.PaymentStatusId) &&
                         (!shippingStatusId.HasValue || shippingStatusId == o.ShippingStatusId) &&
                         (!o.Deleted) &&
                         (!p.Deleted) && (!p.IsSystemProduct) &&
                         (billingCountryId == 0 || o.BillingAddress.CountryId == billingCountryId) &&
                         (showHidden || p.Published)
                         select orderItem;

            var query2 = 
                //group by products
                from orderItem in query1
                group orderItem by orderItem.ProductId into g
                select new
                {
                    EntityId = g.Key,
                    TotalAmount = g.Sum(x => x.PriceExclTax),
                    TotalQuantity = g.Sum(x => x.Quantity),
                };

            switch (orderBy)
            {
                case 1:
                    {
                        query2 = query2.OrderByDescending(x => x.TotalQuantity);
                    }
                    break;
                case 2:
                    {
                        query2 = query2.OrderByDescending(x => x.TotalAmount);
                    }
                    break;
                default:
                    throw new ArgumentException("Wrong orderBy parameter", "orderBy");
            }

            if (recordsToReturn != 0 && recordsToReturn != int.MaxValue)
                query2 = query2.Take(recordsToReturn);

            var result = query2.ToList().Select(x =>
            {
                var reportLine = new BestsellersReportLine()
                {
                    ProductId = x.EntityId,
                    TotalAmount = x.TotalAmount,
                    TotalQuantity = x.TotalQuantity
                };
                return reportLine;
            }).ToList();

            return result;
        }

		public virtual int[] GetAlsoPurchasedProductsIds(int storeId, int productId, int recordsToReturn = 5, bool showHidden = false)
        {
            if (productId == 0)
                throw new ArgumentException("Product ID is not specified");

            //this inner query should retrieve all orders that have contained the productID
			var query1 = from orderItem in _orderItemRepository.Table
						 where orderItem.ProductId == productId
						 select orderItem.OrderId;

            var query2 = from orderItem in _orderItemRepository.Table
                         join p in _productRepository.Table on orderItem.ProductId equals p.Id
                         where (query1.Contains(orderItem.OrderId)) &&
                         (p.Id != productId) &&
                         (showHidden || p.Published) &&
						 (!orderItem.Order.Deleted) &&
						 (storeId == 0 || orderItem.Order.StoreId == storeId) &&
                         (!p.Deleted) && (!p.IsSystemProduct) &&
						 (showHidden || p.Published)
                         select new { orderItem = orderItem, p };

            var query3 = from orderItem_p in query2
                         group orderItem_p by orderItem_p.p.Id into g
                         select new
                         {
                             ProductId = g.Key,
                             ProductsPurchased = g.Sum(x => x.orderItem.Quantity),
                         };
            query3 = query3.OrderByDescending(x => x.ProductsPurchased);

			if (recordsToReturn > 0)
				query3 = query3.Take(recordsToReturn);

			var report = query3.ToList();

			var ids = new List<int>();
			foreach (var line in report)
				ids.Add(line.ProductId);

			return ids.ToArray();
        }

        /// <summary>
        /// Gets a list of products that were never sold
        /// </summary>
        /// <param name="startTime">Order start time; null to load all</param>
        /// <param name="endTime">Order end time; null to load all</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>Products</returns>
        public virtual IPagedList<Product> ProductsNeverSold(DateTime? startTime,
            DateTime? endTime, int pageIndex, int pageSize, bool showHidden = false)
        {
			var groupedProductId = (int)ProductType.GroupedProduct;

			// This inner query should retrieve all purchased order product varint identifiers.
			var query1 = (from orderItem in _orderItemRepository.Table
                          join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                          where (!startTime.HasValue || startTime.Value <= o.CreatedOnUtc) &&
                                (!endTime.HasValue || endTime.Value >= o.CreatedOnUtc) &&
                                (!o.Deleted)
                          select orderItem.ProductId).Distinct();

            var query2 = from p in _productRepository.Table
                         where !query1.Contains(p.Id) &&
								p.ProductTypeId != groupedProductId &&
							   !p.Deleted &&
                               (showHidden || p.Published)
						 orderby p.Name
						 select p;

			var products = new PagedList<Product>(query2, pageIndex, pageSize);
			return products;
        }

        /// <summary>
        /// Get profit report
        /// </summary>
		/// <param name="storeId">Store identifier</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
		/// <param name="orderStatusIds">Filter by order status</param>
		/// <param name="paymentStatusIds">Filter by payment status</param>
		/// <param name="shippingStatusIds">Filter by shipping status</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <returns>Result</returns>
		public virtual decimal ProfitReport(int storeId, int[] orderStatusIds, int[] paymentStatusIds, int[] shippingStatusIds,
			DateTime? startTimeUtc, DateTime? endTimeUtc, string billingEmail)
        {
            //We cannot use String.IsNullOrEmpty(billingEmail) in SQL Compact
            bool dontSearchEmail = String.IsNullOrEmpty(billingEmail);

			var orders =
				from o in _orderRepository.Table
				where (storeId == 0 || storeId == o.StoreId) &&
					(!startTimeUtc.HasValue || startTimeUtc.Value <= o.CreatedOnUtc) &&
					(!endTimeUtc.HasValue || endTimeUtc.Value >= o.CreatedOnUtc) &&
					(!o.Deleted) &&
					(dontSearchEmail || (o.BillingAddress != null && !String.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail)))
				select o;

			if (orderStatusIds != null && orderStatusIds.Count() > 0)
				orders = orders.Where(x => orderStatusIds.Contains(x.OrderStatusId));

			if (paymentStatusIds != null && paymentStatusIds.Count() > 0)
				orders = orders.Where(x => paymentStatusIds.Contains(x.PaymentStatusId));

			if (shippingStatusIds != null && shippingStatusIds.Count() > 0)
				orders = orders.Where(x => shippingStatusIds.Contains(x.ShippingStatusId));

			var query =
				from orderItem in _orderItemRepository.Table
				join o in orders on orderItem.OrderId equals o.Id
				select orderItem;

			var productCost = Convert.ToDecimal(query.Sum(orderItem => (decimal?)orderItem.ProductCost * orderItem.Quantity));

			var reportSummary = GetOrderAverageReportLine(storeId, orderStatusIds, paymentStatusIds, shippingStatusIds, startTimeUtc, endTimeUtc, billingEmail);
			var profit = reportSummary.SumOrders - reportSummary.SumTax - productCost;

            return profit;
        }

        #endregion
    }
}
