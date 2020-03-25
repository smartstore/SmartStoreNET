using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services.Helpers;
using SmartStore.Services.Catalog;
using SmartStore.Core.Localization;
using SmartStore.Core.Domain.Dashboard;
using SmartStore.Core;

namespace SmartStore.Services.Customers
{
    /// <summary>
    /// Customer report service
    /// </summary>
    public partial class CustomerReportService : ICustomerReportService
    {
        #region Fields

        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="customerRepository">Customer repository</param>
        /// <param name="orderRepository">Order repository</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="dateTimeHelper">Date time helper</param>
        public CustomerReportService(IRepository<Customer> customerRepository,
            IRepository<Order> orderRepository, ICustomerService customerService,
            IDateTimeHelper dateTimeHelper, IPriceFormatter priceFormatter)
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
        }

        #endregion

        public Localizer T
        {
            get;
            set;
        } = NullLocalizer.Instance;


        #region Methods

        /// <summary>
        /// Get best customers
        /// </summary>
        /// <param name="startTime">Order start time; null to load all</param>
        /// <param name="endTime">Order end time; null to load all</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Order shippment status; null to load all records</param>
        /// <param name="orderBy">1 - order by order total, 2 - order by number of orders</param>
        /// <returns>Report</returns>
        public virtual IList<TopCustomerReportLine> GetTopCustomersReport(DateTime? startTime,
            DateTime? endTime, OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss, int orderBy, int count)
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
            var query1 = from c in _customerRepository.Table
                         join o in _orderRepository.Table on c.Id equals o.CustomerId
                         where (!startTime.HasValue || startTime.Value <= o.CreatedOnUtc) &&
                         (!endTime.HasValue || endTime.Value >= o.CreatedOnUtc) &&
                         (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
                         (!paymentStatusId.HasValue || paymentStatusId == o.PaymentStatusId) &&
                         (!shippingStatusId.HasValue || shippingStatusId == o.ShippingStatusId) &&
                         (!o.Deleted) &&
                         (!c.Deleted)
                         select new { c, o };

            var query2 = from co in query1
                         group co by co.c.Id into g
                         select new
                         {
                             CustomerId = g.Key,
                             OrderTotal = g.Sum(x => x.o.OrderTotal),
                             OrderCount = g.Count()
                         };
            switch (orderBy)
            {
                case 1:
                    {
                        query2 = query2.OrderByDescending(x => x.OrderTotal);
                    }
                    break;
                case 2:
                    {
                        query2 = query2.OrderByDescending(x => x.OrderCount).ThenByDescending(x => x.OrderTotal);
                    }
                    break;
                default:
                    throw new ArgumentException("Wrong orderBy parameter", "orderBy");
            }

            query2 = query2.Take(() => count);

            var result = query2.ToList().Select(x =>
            {
                return new TopCustomerReportLine()
                {
                    CustomerId = x.CustomerId,
                    OrderTotal = x.OrderTotal,
                    OrderCount = x.OrderCount,
                };

            }).ToList();

            return result;
        }

        /// <summary>
        /// Gets a report of customers registered in the last days
        /// </summary>
        /// <param name="days">Customers registered in the last days</param>
        /// <returns>Number of registered customers</returns>
        public virtual int GetRegisteredCustomersReport(int days)
        {
            DateTime date = _dateTimeHelper.ConvertToUserTime(DateTime.Now).AddDays(-days);

            var registeredCustomerRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
            if (registeredCustomerRole == null)
            {
                return 0;
            }

            var query = 
                from c in _customerRepository.Table
                from rm in c.CustomerRoleMappings
                where !c.Deleted && rm.CustomerRoleId == registeredCustomerRole.Id && c.CreatedOnUtc >= date
                select c;

            var count = query.Count();
            return count;
        }

        public List<RegistredCustomersDate> GetRegisteredCustomersDate()
        {
            //Get registred customers in last 365 days, group by day and count them 
            var date = _dateTimeHelper.ConvertToUserTime(DateTime.Now).AddDays(-365);

            var registeredCustomerRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
            if (registeredCustomerRole == null)
                return new List<RegistredCustomersDate>();

            var query = from c in _customerRepository.Table
                        from cr in c.CustomerRoles
                        where !c.Deleted &&
                        cr.Id == registeredCustomerRole.Id &&
                        c.CreatedOnUtc >= date
                        group c.CreatedOnUtc by DbFunctions.TruncateTime(c.CreatedOnUtc) into dg
                        select new { Date = (DateTime)dg.Key, Count = dg.Count() };

            var data = new List<RegistredCustomersDate>();
            data.Add(new RegistredCustomersDate(DateTime.Now.AddDays(-760), 0));
            foreach (var item in query.ToList())
            {
                data.Add(new RegistredCustomersDate(item.Date, item.Count));
            }

            return data;
        }

        public virtual DashboardChartReportLine GetCustomersDashboardReportLine(IPagedList<Customer> allCustomers, PeriodState state)
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

            var report = new DashboardChartReportLine(1, period);
            var customers = allCustomers.Where(x => x.CreatedOnUtc < endTime.Date && x.CreatedOnUtc >= startTime.Date).Select(x => x).ToList();

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

                var point = customers.Where(x => x.CreatedOnUtc < endDate && x.CreatedOnUtc >= startDate).ToList();
                report.DataSets[0].Quantity[i] = point.Count;
                report.DataSets[0].FormattedAmount[i] = point.Count.ToString("D");
            }

            foreach (var item in report.DataSets)
            {
                item.TotalAmount = item.Quantity.Sum().ToString("D");
            }

            report.TotalAmount = customers.Count.ToString("D");
            var sumBefore = allCustomers
                .Where(x => x.CreatedOnUtc < endTimeBefore && x.CreatedOnUtc >= startTimeBefore)
                .Select(x => x)
                .Count();

            report.PercentageDelta = sumBefore <= 0 ? 0 : (int)Math.Round(customers.Count / (double)sumBefore * 100 - 100);
            return report;
        }

        public struct RegistredCustomersDate
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }

            public RegistredCustomersDate(DateTime date, int count)
            {
                Date = date;
                Count = count;
            }
        }

        public struct customerCountDay
        {
            public int Key { get; set; }
            public int Value { get; set; }

            public customerCountDay(int key, int value)
            {
                Key = key;
                Value = value;
            }
        };
        #endregion
    }
}