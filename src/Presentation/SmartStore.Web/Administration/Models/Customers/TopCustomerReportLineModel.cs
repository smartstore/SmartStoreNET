using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    public class TopCustomerReportLineModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.Orders.Fields.Customer")]
        public int CustomerId { get; set; }

        public string CustomerDisplayName { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Reports.BestBy.Fields.OrderTotal")]
        public string OrderTotal { get; set; }

        [SmartResourceDisplayName("Admin.Customers.Reports.BestBy.Fields.OrderCount")]
        public string OrderCount { get; set; }
    }
}