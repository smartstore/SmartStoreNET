using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public partial class OrderAverageReportLineSummaryModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.SalesReport.Average.OrderStatus")]
        public string OrderStatus { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.Average.SumTodayOrders")]
        public string SumTodayOrders { get; set; }

        [SmartResourceDisplayName("Common.Count")]
        public int CountTodayOrders { get; set; }
        
        [SmartResourceDisplayName("Admin.SalesReport.Average.SumThisWeekOrders")]
        public string SumThisWeekOrders { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.Average.SumThisMonthOrders")]
        public string SumThisMonthOrders { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.Average.SumThisYearOrders")]
        public string SumThisYearOrders { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.Average.SumAllTimeOrders")]
        public string SumAllTimeOrders { get; set; }

		public string Url { get; set; }
    }
}
