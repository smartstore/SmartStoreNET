using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class OrderFulfillmentDashboardReportModel : ModelBase
    {
        //public int[] Percentages { get; set; } = new int[4];
        //public string[] UnfinishedOrders { get; set; } = new string[4];

        public OrdersIncompleteDashboardReportModel[] OrdersIncompleteReport { get; set; } = new OrdersIncompleteDashboardReportModel[4];

    }
}