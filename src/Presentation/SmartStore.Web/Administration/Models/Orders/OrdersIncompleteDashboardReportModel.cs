using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public partial class OrdersIncompleteDashboardReportModel : ModelBase
    {
        public OrdersIncompleteDashboardReportLine[] Reports { get; set; } = new OrdersIncompleteDashboardReportLine[5];
    }

    public class OrdersIncompleteDashboardReportLine
    {
        public OrdersIncompleteDashboardReportData[] Data { get; set; } = new OrdersIncompleteDashboardReportData[3];

        public string AmountTotal { get; set; }

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
