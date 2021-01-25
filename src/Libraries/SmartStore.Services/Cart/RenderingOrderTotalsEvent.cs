using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Cart
{
    public class RenderingOrderTotalsEvent
    {
        public RenderingOrderTotalsEvent()
        {
        }

        public Customer Customer { get; set; }

        public int? StoreId { get; set; }
    }
}