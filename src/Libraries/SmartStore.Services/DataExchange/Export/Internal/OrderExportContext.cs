using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Services.DataExchange.Export.Internal
{
    internal class OrderExportContext
    {
        protected List<int> _orderIds;
        protected List<int> _customerIds;
        protected List<int> _addressIds;

        private Func<int[], IList<Customer>> _funcCustomers;
        private Func<int[], Multimap<int, GenericAttribute>> _funcCustomerGenericAttributes;
        private Func<int[], Multimap<int, RewardPointsHistory>> _funcRewardPointsHistories;
        private Func<int[], IList<Address>> _funcAddresses;
        private Func<int[], Multimap<int, OrderItem>> _funcOrderItems;
        private Func<int[], Multimap<int, Shipment>> _funcShipments;

        private LazyMultimap<Customer> _customers;
        private LazyMultimap<GenericAttribute> _customerGenericAttributes;
        private LazyMultimap<RewardPointsHistory> _rewardPointsHistories;
        private LazyMultimap<Address> _addresses;
        private LazyMultimap<OrderItem> _orderItems;
        private LazyMultimap<Shipment> _shipments;

        public OrderExportContext(IEnumerable<Order> orders,
            Func<int[], IList<Customer>> customers,
            Func<int[], Multimap<int, GenericAttribute>> customerGenericAttributes,
            Func<int[], Multimap<int, RewardPointsHistory>> rewardPointsHistory,
            Func<int[], IList<Address>> addresses,
            Func<int[], Multimap<int, OrderItem>> orderItems,
            Func<int[], Multimap<int, Shipment>> shipments)
        {
            if (orders == null)
            {
                _orderIds = new List<int>();
                _customerIds = new List<int>();
                _addressIds = new List<int>();
            }
            else
            {
                _orderIds = new List<int>(orders.Select(x => x.Id));
                _customerIds = new List<int>(orders.Select(x => x.CustomerId));

                _addressIds = orders.Select(x => x.BillingAddressId)
                    .Union(orders.Select(x => x.ShippingAddressId ?? 0))
                    .Where(x => x != 0)
                    .Distinct()
                    .ToList();
            }

            _funcCustomers = customers;
            _funcCustomerGenericAttributes = customerGenericAttributes;
            _funcRewardPointsHistories = rewardPointsHistory;
            _funcAddresses = addresses;
            _funcOrderItems = orderItems;
            _funcShipments = shipments;
        }

        public void Clear()
        {
            if (_customers != null)
                _customers.Clear();
            if (_customerGenericAttributes != null)
                _customerGenericAttributes.Clear();
            if (_rewardPointsHistories != null)
                _rewardPointsHistories.Clear();
            if (_addresses != null)
                _addresses.Clear();
            if (_orderItems != null)
                _orderItems.Clear();
            if (_shipments != null)
                _shipments.Clear();

            _orderIds.Clear();
            _customerIds.Clear();
            _addressIds.Clear();
        }

        public LazyMultimap<Customer> Customers
        {
            get
            {
                if (_customers == null)
                {
                    _customers = new LazyMultimap<Customer>(keys => _funcCustomers(keys).ToMultimap(x => x.Id, x => x), _customerIds);
                }
                return _customers;
            }
        }

        public LazyMultimap<GenericAttribute> CustomerGenericAttributes
        {
            get
            {
                if (_customerGenericAttributes == null)
                {
                    _customerGenericAttributes = new LazyMultimap<GenericAttribute>(keys => _funcCustomerGenericAttributes(keys), _customerIds);
                }
                return _customerGenericAttributes;
            }
        }

        public LazyMultimap<RewardPointsHistory> RewardPointsHistories
        {
            get
            {
                if (_rewardPointsHistories == null)
                {
                    _rewardPointsHistories = new LazyMultimap<RewardPointsHistory>(keys => _funcRewardPointsHistories(keys), _customerIds);
                }
                return _rewardPointsHistories;
            }
        }

        public LazyMultimap<Address> Addresses
        {
            get
            {
                if (_addresses == null)
                {
                    _addresses = new LazyMultimap<Address>(keys => _funcAddresses(keys).ToMultimap(x => x.Id, x => x), _addressIds);
                }
                return _addresses;
            }
        }

        public LazyMultimap<OrderItem> OrderItems
        {
            get
            {
                if (_orderItems == null)
                {
                    _orderItems = new LazyMultimap<OrderItem>(keys => _funcOrderItems(keys), _orderIds);
                }
                return _orderItems;
            }
        }

        public LazyMultimap<Shipment> Shipments
        {
            get
            {
                if (_shipments == null)
                {
                    _shipments = new LazyMultimap<Shipment>(keys => _funcShipments(keys), _orderIds);
                }
                return _shipments;
            }
        }
    }
}
