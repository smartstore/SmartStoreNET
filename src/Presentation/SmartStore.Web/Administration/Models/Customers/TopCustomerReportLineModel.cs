using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    public class TopCustomerReportLineModel : ModelBase
    {
        public int CustomerId { get; set; }

        public string CustomerDisplayName { get; set; }

        public string OrderTotal { get; set; }

        public string OrderCount { get; set; }
    }
}