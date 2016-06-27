using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    public class BestCustomerReportLineModel : ModelBase
    {
		[SmartResourceDisplayName("Admin.Customers.Reports.BestBy.Fields.Customer")]
        public int CustomerId { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Reports.BestBy.Fields.Customer")]
        public string CustomerName { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Reports.BestBy.Fields.OrderTotal")]
        public string OrderTotal { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Reports.BestBy.Fields.OrderCount")]
        public decimal OrderCount { get; set; }
    }
}