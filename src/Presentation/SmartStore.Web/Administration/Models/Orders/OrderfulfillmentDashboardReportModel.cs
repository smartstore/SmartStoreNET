using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class OrderFulfillmentDashboardReportModel : ModelBase
    {
        public int[] Percentages { get; set; } = new int[4];
        public int[] UnfinishedOrders { get; set; } = new int[4];
    }
}