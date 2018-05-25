using System.Collections.Generic;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Customers
{
	/// <summary>
	/// An event message, which gets published after customer was registered
	/// </summary>
    public class CustomerRegisteredEvent
	{
        public Customer Customer
		{ 
			get; 
			set; 
		}
	}
}
