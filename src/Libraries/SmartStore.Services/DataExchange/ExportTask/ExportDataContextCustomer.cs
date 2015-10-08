using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.DataExchange.ExportTask
{
	public class ExportDataContextCustomer
	{
		protected List<int> _customerIds;

		private Func<int[], Multimap<int, GenericAttribute>> _funcGenericAttributes;

		private LazyMultimap<GenericAttribute> _genericAttributes;

		public ExportDataContextCustomer(IEnumerable<Customer> customers,
			Func<int[], Multimap<int, GenericAttribute>> genericAttributes)
		{
			if (customers == null)
			{
				_customerIds = new List<int>();
			}
			else
			{
				_customerIds = new List<int>(customers.Select(x => x.Id));
			}

			_funcGenericAttributes = genericAttributes;
		}

		public void Clear()
		{
			if (_genericAttributes != null)
				_genericAttributes.Clear();
		}

		public LazyMultimap<GenericAttribute> GenericAttributes
		{
			get
			{
				if (_genericAttributes == null)
				{
					_genericAttributes = new LazyMultimap<GenericAttribute>(keys => _funcGenericAttributes(keys), _customerIds);
				}
				return _genericAttributes;
			}
		}
	}
}
