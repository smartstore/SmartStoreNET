using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    public class CustomerReportsModel : ModelBase
    {
        public TopCustomersReportModel TopCustomersByOrderTotal { get; set; }
        public TopCustomersReportModel TopCustomersByNumberOfOrders { get; set; }
    }
}