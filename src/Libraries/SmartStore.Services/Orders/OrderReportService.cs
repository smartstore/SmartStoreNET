using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Dashboard;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services.Helpers;

namespace SmartStore.Services.Orders
{
    public partial class OrderReportService : IOrderReportService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IDateTimeHelper _dateTimeHelper;

        public OrderReportService(
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Product> productRepository,
            IDateTimeHelper dateTimeHelper)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _dateTimeHelper = dateTimeHelper;
        }

        public virtual OrderAverageReportLine GetOrderAverageReportLine(
            int storeId,
            int[] orderStatusIds,
            int[] paymentStatusIds,
            int[] shippingStatusIds,
            DateTime? startTimeUtc,
            DateTime? endTimeUtc,
            string billingEmail,
            bool ignoreCancelledOrders = false)
        {
            var query = _orderRepository.Table;
            query = query.Where(o => !o.Deleted);

            if (storeId > 0)
            {
                query = query.Where(o => o.StoreId == storeId);
            }
            if (ignoreCancelledOrders)
            {
                var cancelledOrderStatusId = (int)OrderStatus.Cancelled;
                query = query.Where(o => o.OrderStatusId != cancelledOrderStatusId);
            }
            if (startTimeUtc.HasValue)
            {
                query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);
            }
            if (endTimeUtc.HasValue)
            {
                query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);
            }
            if (!string.IsNullOrEmpty(billingEmail))
            {
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            }
            if (orderStatusIds != null && orderStatusIds.Any())
            {
                query = query.Where(x => orderStatusIds.Contains(x.OrderStatusId));
            }
            if (paymentStatusIds != null && paymentStatusIds.Any())
            {
                query = query.Where(x => paymentStatusIds.Contains(x.PaymentStatusId));
            }
            if (shippingStatusIds != null && shippingStatusIds.Any())
            {
                query = query.Where(x => shippingStatusIds.Contains(x.ShippingStatusId));
            }

            return GetOrderAverageReportLine(query);
        }

        public virtual OrderAverageReportLine GetOrderAverageReportLine(IQueryable<Order> orderQuery)
        {
            var item = (
                from oq in orderQuery
                group oq by 1 into result
                select new
                {
                    OrderCount = result.Count(),
                    OrderTaxSum = result.Sum(o => o.OrderTax),
                    OrderTotalSum = result.Sum(o => o.OrderTotal)
                })
                .Select(r => new OrderAverageReportLine
                {
                    SumTax = r.OrderTaxSum,
                    CountOrders = r.OrderCount,
                    SumOrders = r.OrderTotalSum
                })
                .FirstOrDefault();

            if (item == null)
            {
                return new OrderAverageReportLine
                {
                    CountOrders = 0,
                    SumOrders = decimal.Zero,
                    SumTax = decimal.Zero
                };
            }

            return item;
        }

        public virtual OrderAverageReportLineSummary OrderAverageReport(int storeId, OrderStatus os)
        {
            var item = new OrderAverageReportLineSummary
            {
                OrderStatus = os
            };

            DateTime nowDt = _dateTimeHelper.ConvertToUserTime(DateTime.Now);
            TimeZoneInfo timeZone = _dateTimeHelper.CurrentTimeZone;
            var orderStatusId = new int[] { (int)os };

            // Today.
            DateTime t1 = new DateTime(nowDt.Year, nowDt.Month, nowDt.Day);
            if (!timeZone.IsInvalidTime(t1))
            {
                DateTime? startTime1 = _dateTimeHelper.ConvertToUtcTime(t1, timeZone);
                DateTime? endTime1 = null;
                var todayResult = GetOrderAverageReportLine(storeId, orderStatusId, null, null, startTime1, endTime1, null);
                item.SumTodayOrders = todayResult.SumOrders;
                item.CountTodayOrders = todayResult.CountOrders;
            }

            // Week.
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

            // Month.
            DateTime t3 = new DateTime(nowDt.Year, nowDt.Month, 1);
            if (!timeZone.IsInvalidTime(t3))
            {
                DateTime? startTime3 = _dateTimeHelper.ConvertToUtcTime(t3, timeZone);
                DateTime? endTime3 = null;
                var monthResult = GetOrderAverageReportLine(storeId, orderStatusId, null, null, startTime3, endTime3, null);
                item.SumThisMonthOrders = monthResult.SumOrders;
                item.CountThisMonthOrders = monthResult.CountOrders;
            }

            // Year.
            DateTime t4 = new DateTime(nowDt.Year, 1, 1);
            if (!timeZone.IsInvalidTime(t4))
            {
                DateTime? startTime4 = _dateTimeHelper.ConvertToUtcTime(t4, timeZone);
                DateTime? endTime4 = null;
                var yearResult = GetOrderAverageReportLine(storeId, orderStatusId, null, null, startTime4, endTime4, null);
                item.SumThisYearOrders = yearResult.SumOrders;
                item.CountThisYearOrders = yearResult.CountOrders;
            }

            // All time.
            DateTime? startTime5 = null;
            DateTime? endTime5 = null;
            var allTimeResult = GetOrderAverageReportLine(storeId, orderStatusId, null, null, startTime5, endTime5, null);
            item.SumAllTimeOrders = allTimeResult.SumOrders;
            item.CountAllTimeOrders = allTimeResult.CountOrders;

            return item;
        }

        public virtual IPagedList<BestsellersReportLine> BestSellersReport(
            int storeId,
            DateTime? startTime,
            DateTime? endTime,
            OrderStatus? orderStatus,
            PaymentStatus? paymentStatus,
            ShippingStatus? shippingStatus,
            int billingCountryId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            ReportSorting sorting = ReportSorting.ByQuantityDesc,
            bool showHidden = false)
        {
            var orderStatusId = orderStatus.HasValue ? (int)orderStatus.Value : (int?)null;
            var paymentStatusId = paymentStatus.HasValue ? (int)paymentStatus.Value : (int?)null;
            var shippingStatusId = shippingStatus.HasValue ? (int)shippingStatus : (int?)null;

            var query =
                from orderItem in _orderItemRepository.TableUntracked
                join o in _orderRepository.TableUntracked on orderItem.OrderId equals o.Id
                join p in _productRepository.TableUntracked on orderItem.ProductId equals p.Id
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

            // Group by product ID.
            var groupedQuery =
                from orderItem in query
                group orderItem by orderItem.ProductId into g
                select new BestsellersReportLine
                {
                    ProductId = g.Key,
                    TotalAmount = g.Sum(x => x.PriceExclTax),
                    TotalQuantity = g.Sum(x => x.Quantity)
                };

            switch (sorting)
            {
                case ReportSorting.ByAmountAsc:
                    groupedQuery = groupedQuery.OrderBy(x => x.TotalAmount);
                    break;
                case ReportSorting.ByAmountDesc:
                    groupedQuery = groupedQuery.OrderByDescending(x => x.TotalAmount);
                    break;
                case ReportSorting.ByQuantityAsc:
                    groupedQuery = groupedQuery.OrderBy(x => x.TotalQuantity).ThenByDescending(x => x.TotalAmount);
                    break;
                case ReportSorting.ByQuantityDesc:
                default:
                    groupedQuery = groupedQuery.OrderByDescending(x => x.TotalQuantity).ThenByDescending(x => x.TotalAmount);
                    break;
            }

            var result = new PagedList<BestsellersReportLine>(groupedQuery, pageIndex, pageSize);
            return result;
        }

        public virtual int GetPurchaseCount(int productId)
        {
            if (productId == 0)
            {
                return 0;
            }

            var query =
                from orderItem in _orderItemRepository.Table
                where orderItem.ProductId == productId
                group orderItem by orderItem.Id into g
                select new { ProductsPurchased = g.Sum(x => x.Quantity) };

            return query.Select(x => x.ProductsPurchased).FirstOrDefault();
        }

        public virtual int[] GetAlsoPurchasedProductsIds(int storeId, int productId, int recordsToReturn = 5, bool showHidden = false)
        {
            if (productId == 0)
            {
                return new int[0];
            }

            // This inner query should retrieve all orders that have contained the productID.
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
                         select new { orderItem, p };

            var query3 = from orderItem_p in query2
                         group orderItem_p by orderItem_p.p.Id into g
                         select new
                         {
                             ProductId = g.Key,
                             ProductsPurchased = g.Sum(x => x.orderItem.Quantity),
                         };

            query3 = query3.OrderByDescending(x => x.ProductsPurchased);

            if (recordsToReturn > 0)
            {
                query3 = query3.Take(() => recordsToReturn);
            }

            var report = query3.ToList();

            var ids = report
                .Select(x => x.ProductId)
                .ToArray();

            return ids;
        }

        public virtual IPagedList<Product> ProductsNeverSold(DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize, bool showHidden = false)
        {
            var groupedProductId = (int)ProductType.GroupedProduct;

            var query1 = (from orderItem in _orderItemRepository.TableUntracked
                          join o in _orderRepository.TableUntracked on orderItem.OrderId equals o.Id
                          where !o.Deleted &&
                                (!startTime.HasValue || startTime.Value <= o.CreatedOnUtc) &&
                                (!endTime.HasValue || endTime.Value >= o.CreatedOnUtc)
                          select orderItem.ProductId).Distinct();

            var query2 = from p in _productRepository.TableUntracked
                         where !query1.Contains(p.Id) &&
                                !p.Deleted && !p.IsSystemProduct &&
                               (showHidden || p.Published) &&
                                p.ProductTypeId != groupedProductId
                         orderby p.Name
                         select p;

            var products = new PagedList<Product>(query2, pageIndex, pageSize);
            return products;
        }

        public virtual decimal GetProfit(IQueryable<Order> orderQuery)
        {
            var query =
                from orderItem in _orderItemRepository.Table
                join o in orderQuery on orderItem.OrderId equals o.Id
                select orderItem;

            var productCost = Convert.ToDecimal(query.Sum(oi => (decimal?)oi.ProductCost * oi.Quantity));
            var summary = GetOrderAverageReportLine(orderQuery);
            var profit = summary.SumOrders - summary.SumTax - productCost;

            return profit;
        }

        public virtual IPagedList<OrderDataPoint> GetIncompleteOrders(int storeId, DateTime? startTimeUtc, DateTime? endTimeUtc)
        {
            var query = _orderRepository.Table;
            query = query.Where(x => !x.Deleted && x.OrderStatusId != (int)OrderStatus.Cancelled);

            if (storeId > 0)
            {
                query = query.Where(x => x.StoreId == storeId);
            }
            if (startTimeUtc.HasValue)
            {
                query = query.Where(x => startTimeUtc.Value <= x.CreatedOnUtc);
            }
            if (endTimeUtc.HasValue)
            {
                query = query.Where(x => endTimeUtc.Value >= x.CreatedOnUtc);
            }

            query = query.Where(x =>
                x.ShippingStatusId == (int)ShippingStatus.NotYetShipped
                || x.PaymentStatusId == (int)PaymentStatus.Pending
            );
            var dataPoints = query.Select(x => new OrderDataPoint
            {
                CreatedOn = x.CreatedOnUtc,
                OrderTotal = x.OrderTotal,
                OrderStatusId = x.OrderStatusId,
                PaymentStatusId = x.PaymentStatusId,
                ShippingStatusId = x.ShippingStatusId
            });

            return new PagedList<OrderDataPoint>(dataPoints, 0, int.MaxValue);
        }

        public virtual IPagedList<OrderDataPoint> GetOrdersDashboardData(int storeId, DateTime? startTimeUtc, DateTime? endTimeUtc, int pageIndex, int pageSize)
        {
            var query = _orderRepository.Table;
            query = query.Where(x => !x.Deleted);

            if (storeId > 0)
            {
                query = query.Where(x => x.StoreId == storeId);
            }
            if (startTimeUtc.HasValue)
            {
                query = query.Where(x => startTimeUtc.Value <= x.CreatedOnUtc);
            }
            if (endTimeUtc.HasValue)
            {
                query = query.Where(x => endTimeUtc.Value >= x.CreatedOnUtc);
            }

            var dataPoints = query.Select(x => new OrderDataPoint
            {
                CreatedOn = x.CreatedOnUtc,
                OrderTotal = x.OrderTotal,
                OrderStatusId = x.OrderStatusId
            });
            return new PagedList<OrderDataPoint>(dataPoints, pageIndex, pageSize);
        }

        public virtual decimal GetOrdersTotal(int storeId, DateTime? startTimeUtc, DateTime? endTimeUtc)
        {
            var query = _orderRepository.Table;
            query = query.Where(x => !x.Deleted);

            if (storeId > 0)
            {
                query = query.Where(x => x.StoreId == storeId);
            }
            if (startTimeUtc.HasValue)
            {
                query = query.Where(x => startTimeUtc.Value <= x.CreatedOnUtc);
            }
            if (endTimeUtc.HasValue)
            {
                query = query.Where(x => endTimeUtc.Value >= x.CreatedOnUtc);
            }

            return query.Sum(x => (decimal?)x.OrderTotal) ?? decimal.Zero;
        }
    }
}
