using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Cart
{
    public class OrderTotalsEvent
    {
        public OrderTotalsEvent()
        {
        }

        public Customer Customer { get; set; }

        public int? StoreId { get; set; }
    }
}