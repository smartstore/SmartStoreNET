using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Localization;
using SmartStore.Services.Helpers;
using SmartStore.Services.Orders;

namespace SmartStore.Services.Customers
{
    public partial class CustomerReportService : ICustomerReportService
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public CustomerReportService(
            IRepository<Customer> customerRepository,
            IRepository<Order> orderRepository,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper)
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;

        public virtual IPagedList<TopCustomerReportLine> GetTopCustomersReport(
            DateTime? startTime,
            DateTime? endTime,
            OrderStatus? orderStatus,
            PaymentStatus? paymentStatus,
            ShippingStatus? shippingStatus,
            ReportSorting sorting,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var orderStatusId = orderStatus.HasValue ? (int)orderStatus.Value : (int?)null;
            var paymentStatusId = paymentStatus.HasValue ? (int)paymentStatus.Value : (int?)null;
            var shippingStatusId = shippingStatus.HasValue ? (int)shippingStatus.Value : (int?)null;

            var query =
                from c in _customerRepository.TableUntracked
                join o in _orderRepository.TableUntracked on c.Id equals o.CustomerId
                where (!startTime.HasValue || startTime.Value <= o.CreatedOnUtc) &&
                (!endTime.HasValue || endTime.Value >= o.CreatedOnUtc) &&
                (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
                (!paymentStatusId.HasValue || paymentStatusId == o.PaymentStatusId) &&
                (!shippingStatusId.HasValue || shippingStatusId == o.ShippingStatusId) &&
                (!o.Deleted) &&
                (!c.Deleted)
                select new { c, o };

            var groupedQuery =
                from co in query
                group co by co.c.Id into g
                select new TopCustomerReportLine
                {
                    CustomerId = g.Key,
                    OrderTotal = g.Sum(x => x.o.OrderTotal),
                    OrderCount = g.Count()
                };

            switch (sorting)
            {
                case ReportSorting.ByAmountAsc:
                    groupedQuery = groupedQuery.OrderBy(x => x.OrderTotal);
                    break;
                case ReportSorting.ByAmountDesc:
                    groupedQuery = groupedQuery.OrderByDescending(x => x.OrderTotal);
                    break;
                case ReportSorting.ByQuantityAsc:
                    groupedQuery = groupedQuery.OrderBy(x => x.OrderCount).ThenByDescending(x => x.OrderTotal);
                    break;
                case ReportSorting.ByQuantityDesc:
                default:
                    groupedQuery = groupedQuery.OrderByDescending(x => x.OrderCount).ThenByDescending(x => x.OrderTotal);
                    break;
            }

            var result = new PagedList<TopCustomerReportLine>(groupedQuery, pageIndex, pageSize);
            return result;
        }

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

        public virtual List<RegistredCustomersDate> GetRegisteredCustomersDate()
        {
            // Get registred customers in last 365 days, group by day and count them.
            var date = _dateTimeHelper.ConvertToUserTime(DateTime.Now).AddDays(-365);

            var registeredCustomerRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
            if (registeredCustomerRole == null)
            {
                return new List<RegistredCustomersDate>();
            }

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
            if (registeredCustomerRole == null)
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
    }
}