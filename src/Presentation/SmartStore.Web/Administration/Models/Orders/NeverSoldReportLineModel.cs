using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class NeverSoldReportLineModel : ModelBase
    {
        public int ProductId { get; set; }
        public string ProductTypeName { get; set; }
        public string ProductTypeLabelHint { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.NeverSold.Fields.Name")]
        public string ProductName { get; set; }
    }
}