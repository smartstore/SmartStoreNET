using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public partial class OrdersIncompleteDashboardReportModel : ModelBase
    {
        public string AmountTotal { get; set; }
        public string QuantityTotal { get; set; }

        public List<OrdersIncompleteDashboardReportLine> Reports { get; set; } = new List<OrdersIncompleteDashboardReportLine>();
    }

    public class OrdersIncompleteDashboardReportLine
    {
        //[SmartResourceDisplayName("Admin.SalesReport.Incomplete.Item")]
        //public string Item { get; set; }

        //[SmartResourceDisplayName("Admin.SalesReport.Incomplete.Total")]
        public decimal AmountTotal { get; set; }
        
        public string AmountTotalFormatted { get; set; }

        //[SmartResourceDisplayName("Admin.SalesReport.Incomplete.Count")]
        public int Quantity { get; set; }

        public string QuantityFormatted { get; set; }

        //public string Url { get; set; }
    }
}
