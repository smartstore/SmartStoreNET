using System.Collections.Generic;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Customers
{
    /// <summary>
    /// An event message, which will be published after customer has registered
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
