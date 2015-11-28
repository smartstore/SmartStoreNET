using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Customers
{
    public class CustomerReportsModel : ModelBase
    {
        public BestCustomersReportModel BestCustomersByOrderTotal { get; set; }
        public BestCustomersReportModel BestCustomersByNumberOfOrders { get; set; }
    }
}