using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public partial class OrdersIncompleteDashboardReportModel : ModelBase
    {
        public List<OrdersIncompleteDashboardReportLine> Reports { get; set; } = new List<OrdersIncompleteDashboardReportLine>();
    }

    public class OrdersIncompleteDashboardReportLine
    {
        //[SmartResourceDisplayName("Admin.SalesReport.Incomplete.Item")]
        //public string Item { get; set; }

        //[SmartResourceDisplayName("Admin.SalesReport.Incomplete.Total")]
        public string AmountTotal { get; set; }

        //[SmartResourceDisplayName("Admin.SalesReport.Incomplete.Count")]
        public int Quantity { get; set; }

        public string FormattedQuantity { get; set; }

        //public string Url { get; set; }
    }
}
