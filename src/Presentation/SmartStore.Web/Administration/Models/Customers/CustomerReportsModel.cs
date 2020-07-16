using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    public class CustomerReportsModel : ModelBase
    {
        public int GridPageSize { get; set; }
        public bool UsernamesEnabled { get; set; }

        public TopCustomersReportModel TopCustomers { get; set; }
    }
}