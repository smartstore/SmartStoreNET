using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Customers
{
	public class CustomerExportedEvent
	{
		public CustomerExportedEvent(Customer customer, IDictionary<string, object> result)
		{
			Guard.NotNull(customer, nameof(customer));
			Guard.NotNull(result, nameof(result));

			Customer = customer;
			Result = result;
		}

		public Customer Customer { get; private set; }
		public IDictionary<string, object> Result { get; private set; }
	}
}
