using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.DataExchange.ExportTask
{
	internal class ExportOrderDataContext
	{
		protected List<int> _orderIds;
		protected List<int> _customerIds;
		protected List<int> _addressIds;

		private Func<int[], IList<Customer>> _funcCustomers;
		private Func<int[], IList<Address>> _funcAddresses;
		private Func<int[], Multimap<int, OrderItem>> _funcOrderItems;

		private LazyMultimap<Customer> _customers;
		private LazyMultimap<Address> _addresses;
		private LazyMultimap<OrderItem> _orderItems;

		public ExportOrderDataContext(IEnumerable<Order> orders,
			Func<int[], IList<Customer>> customers,
			Func<int[], IList<Address>> addresses,
			Func<int[], Multimap<int, OrderItem>> orderItems)
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
			_funcAddresses = addresses;
			_funcOrderItems = orderItems;
		}

		public void Clear()
		{
			if (_customers != null)
				_customers.Clear();
			if (_addresses != null)
				_addresses.Clear();
			if (_orderItems != null)
				_orderItems.Clear();
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
	}
}
