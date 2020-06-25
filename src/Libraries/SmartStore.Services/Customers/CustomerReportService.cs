using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Localization;
using SmartStore.Services.Catalog;
using SmartStore.Services.Helpers;

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
                        from rm in c.CustomerRoleMappings
                        where !c.Deleted &&
                        rm.CustomerRoleId == registeredCustomerRole.Id &&
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

        public virtual int GetCustomerRegistrations(DateTime? startTimeUtc, DateTime? endTimeUtc)
        {
            var registeredCustomerRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
            if(registeredCustomerRole == null)
            {
                return 0;
            }

            var query = _customerRepository.Table;
            query = query.Where(x => !x.Deleted);
            if (startTimeUtc.HasValue)
            {
                query = query.Where(x => startTimeUtc.Value <= x.CreatedOnUtc);
            }
            if (endTimeUtc.HasValue)
            {
                query = query.Where(x => endTimeUtc.Value >= x.CreatedOnUtc);
            }

            query = query.Where(x => x.CustomerRoleMappings.Any(y => y.CustomerRoleId == registeredCustomerRole.Id));

            return query.Count();
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

        #endregion
    }
}