using System;
using System.Collections.Generic;
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

        public OrderReportService(IRepository<Order> orderRepository,
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

		public virtual IList<BestsellersReportLine> BestSellersReport(
            int storeId,
            DateTime? startTime,
            DateTime? endTime,
            OrderStatus? os,
            PaymentStatus? ps,
            ShippingStatus? ss,
            int billingCountryId = 0,
            int recordsToReturn = 5,
            int orderBy = 1,
            bool showHidden = false)
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
                        query2 = query2.OrderByDescending(x => x.TotalQuantity).ThenByDescending(x => x.TotalAmount);
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
                query2 = query2.Take(() => recordsToReturn);

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

        public virtual DashboardChartReportLine GetOrdersDashboardReport(IPagedList<Order> allOrders, PeriodState state)
        {
            var startTime = DateTime.UtcNow;
            var endTime = startTime;
            var startTimeBefore = startTime;
            var endTimeBefore = startTime;
            var period = 0;

            if (state == PeriodState.Today)
            {
                period = 24;
                endTime = endTime.AddDays(1);
                startTimeBefore = startTime.Date.AddDays(-1);
                endTimeBefore = startTime.Date;
            }
            else if (state == PeriodState.Yesterday)
            {
                period = 24;
                startTime = startTime.AddDays(-1);
                startTimeBefore = startTime.Date.AddDays(-1);
                endTimeBefore = startTime.Date;
            }
            else if (state == PeriodState.Week)
            {
                period = 7;
                startTime = startTime.AddDays(-6);
                endTime = endTime.AddDays(1);
                startTimeBefore = startTime.Date.AddDays(-6);
                endTimeBefore = startTime.Date;
            }
            else if (state == PeriodState.Month)
            {
                period = 4;
                startTime = startTime.AddDays(-27);
                endTime = endTime.AddDays(1);
                startTimeBefore = startTime.Date.AddDays(-28);
                endTimeBefore = startTime;
            }
            else if (state == PeriodState.Year)
            {
                period = 12;
                startTime = new DateTime(startTime.Year, 1, 1);
                endTime = endTime.AddDays(1);
                startTimeBefore = startTime.Date.AddYears(-1);
                endTimeBefore = startTime;
            }

            var report = new DashboardChartReportLine(4, period);
            var orders = allOrders.Where(x => x.CreatedOnUtc < endTime.Date && x.CreatedOnUtc >= startTime.Date).Select(x => x).ToList();
            var ordersReports = GetOrderReports(orders);

            for (int i = 0; i < period; i++)
            {
                var startDate = startTime.Date;
                var endDate = startDate;
                if (state == PeriodState.Today || state == PeriodState.Yesterday)
                {
                    startDate = startDate.AddHours(i - _dateTimeHelper.CurrentTimeZone.BaseUtcOffset.Hours);
                    endDate = startDate.AddHours(1);
                    report.Labels[i] = startDate.AddHours(_dateTimeHelper.CurrentTimeZone.BaseUtcOffset.Hours).ToString("t");
                }
                else if (state == PeriodState.Week)
                {
                    startDate = startDate.AddDays(i);
                    endDate = startDate.AddDays(1);
                    report.Labels[i] = startDate.AddHours(_dateTimeHelper.CurrentTimeZone.BaseUtcOffset.Hours).ToString("m");
                }
                else if (state == PeriodState.Month)
                {
                    endDate = startDate.AddDays((i + 1) * 7);
                    startDate = startDate.AddDays(i * 7);
                    report.Labels[i] = startDate.AddHours(_dateTimeHelper.CurrentTimeZone.BaseUtcOffset.Hours).ToString("m") + " - " + endDate.AddDays(-1).ToString("m");
                }
                else if (state == PeriodState.Year)
                {
                    startDate = new DateTime(startTime.Year, i + 1, 1);
                    endDate = startDate.AddMonths(1);
                    report.Labels[i] = startDate.ToString("Y");
                }

                GetReportPointData(report, ordersReports, startDate, endDate, i);
            }

            CalculateOrdersAmount(report, allOrders, orders, startTimeBefore, endTimeBefore);


            return report;
        }

        private List<Order>[] GetOrderReports(List<Order> orders)
        {
            var orderReports = new List<Order>[4];

            orderReports[0] = orders.Where(x => x.OrderStatus == OrderStatus.Cancelled).Select(x => x).ToList();
            orderReports[1] = orders.Where(x => x.OrderStatus == OrderStatus.Pending).Select(x => x).ToList();
            orderReports[2] = orders.Where(x => x.OrderStatus == OrderStatus.Processing).Select(x => x).ToList();
            orderReports[3] = orders.Where(x => x.OrderStatus == OrderStatus.Complete).Select(x => x).ToList();

            return orderReports;
        }

        private void GetReportPointData(DashboardChartReportLine report, List<Order>[] reports, DateTime startDate, DateTime endDate, int index)
        {
            for (int j = 0; j < reports.Length; j++)
            {
                var point = reports[j].Where(x => x.CreatedOnUtc < endDate && x.CreatedOnUtc >= startDate).ToList();
                report.DataSets[j].Amount[index] = point.Sum(x => x.OrderTotal);
                report.DataSets[j].FormattedAmount[index] = ((int)Math.Round(report.DataSets[j].Amount[index])).ToString("C0");
                //report.DataSets[j].FormattedAmount[index] = _priceFormatter.FormatPrice((int)report.DataSets[j].Amount[index], true, false);
                report.DataSets[j].Quantity[index] = point.Count;
            }
        }

        private void CalculateOrdersAmount(DashboardChartReportLine report, IList<Order> allOrders, List<Order> orders, DateTime fromDate, DateTime toDate)
        {
            foreach (var item in report.DataSets)
            {
                item.TotalAmount = ((int)Math.Round(item.Amount.Sum())).ToString("C0");
            }

            var totalAmount = orders.Where(x => x.OrderStatus != OrderStatus.Cancelled).Sum(x => x.OrderTotal);
            report.TotalAmount = ((int)Math.Round(totalAmount)).ToString("C0");
            var sumBefore = Math.Round(allOrders
                .Where(x => x.CreatedOnUtc < toDate && x.CreatedOnUtc >= fromDate)
                .Select(x => x)
                .Sum(x => x.OrderTotal)
                );

            report.PercentageDelta = sumBefore <= 0 ? 0 : (int)Math.Round(totalAmount / sumBefore * 100 - 100);
        }
    }
}
