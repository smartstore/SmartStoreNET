
namespace SmartStore.Core.Domain.Orders
{
    public static class OrderExtensions
    {
        /// <summary>
        /// Gets the formatted ordner number
        /// </summary>
        /// <returns>Returns either a formatted order number value, or - if
        /// the database field is empty - the order id.</returns>
        public static string GetOrderNumber(this Order order)
        {
            if (order.OrderNumber.IsEmpty())
            {
                return order.Id.ToString();
            }

            return order.OrderNumber;
        }
    }
}
