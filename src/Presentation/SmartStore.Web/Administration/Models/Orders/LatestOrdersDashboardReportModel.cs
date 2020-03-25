using System.Collections.Generic;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Admin.Models.Orders
{
    public class LatestOrdersDashboardReportModel
    {
        public IList<DashboardOrderModel> LatestOrders { get; set; } = new List<DashboardOrderModel>();
    }

    public class DashboardOrderModel
    {
        public int CustomerId { get; set; }
        public string CustomerDisplayName { get; set; }
        public int ProductsTotal { get; set; }
        public string TotalAmount { get; set; }
        public string Created { get; set; }
        public OrderStatus OrderState { get; set; }
        public int OrderId { get; set; }

        public DashboardOrderModel(
            int customerId,
            string customerDisplayName,
            int productsTotal,
            string totalAmount,
            string created,
            OrderStatus orderState,
            int orderId)
        {
            CustomerId = customerId;
            CustomerDisplayName = customerDisplayName;
            ProductsTotal = productsTotal;
            TotalAmount = totalAmount;
            Created = created;
            OrderState = orderState;
            OrderId = orderId;
        }
    }
}