using System.Collections.Generic;
using SmartStore.Admin.Models.Customers;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Admin.Models.Orders
{
    public class LatestOrdersDashboardReportModel
    {
        public IList<DashboardOrderModel> LatestOrders { get; set; } = new List<DashboardOrderModel>();
    }

    public class DashboardOrderModel
    {
        public Customer Customer { get; set; }
        public string CustomerDisplayName { get; set; }
        public int ProductsTotal { get; set; }
        public string TotalAmount { get; set; }
        public string Created { get; set; }
        public OrderStatus OrderState { get; set; }
        public int OrderId { get; set; }
        public DashboardOrderModel(
            Customer customer,
            string customerDisplayName,
            int productsTotal,
            string totalAmount,
            string created,
            OrderStatus orderState,
            int orderId)
        {
            Customer = customer;
            CustomerDisplayName = customerDisplayName;
            ProductsTotal = productsTotal;
            TotalAmount = totalAmount;
            Created = created;
            OrderState = orderState;
            OrderId = orderId;
        }
    }
}