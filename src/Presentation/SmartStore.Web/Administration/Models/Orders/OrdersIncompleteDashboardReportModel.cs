using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public partial class OrdersIncompleteDashboardReportModel : ModelBase
    {
        public OrdersIncompleteDashboardReportModel()
        {
            Data = new List<OrdersIncompleteDashboardReportData>()
            {
                // NotShipped = 0 
                new OrdersIncompleteDashboardReportData(),
                // NotPaid = 1
                new OrdersIncompleteDashboardReportData(),
                // NewOrders = 2 
                new OrdersIncompleteDashboardReportData()
            };
        }


        public List<OrdersIncompleteDashboardReportData> Data { get; set; }

        public decimal Amount { get; set; }

        public string AmountTotal { get; set; }

        public int Quantity { get; set; }

        public string QuantityTotal { get; set; }
    }

    public class OrdersIncompleteDashboardReportData
    {
        public decimal Amount { get; set; }

        public string AmountFormatted { get; set; }

        public int Quantity { get; set; }

        public string QuantityFormatted { get; set; }
    }
}
