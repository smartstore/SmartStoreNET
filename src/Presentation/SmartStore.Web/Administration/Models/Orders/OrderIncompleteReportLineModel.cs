using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public partial class OrderIncompleteReportLineModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.SalesReport.Incomplete.Item")]
        public string Item { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.Incomplete.Total")]
        public string Total { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.Incomplete.Count")]
        public int Count { get; set; }

		public string Url { get; set; }
    }
}
