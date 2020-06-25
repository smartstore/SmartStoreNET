using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    public class TopCustomersDashboardReportModel : ModelBase
    {
        public IList<TopCustomerReportLineModel> TopCustomersByQuantity { get; set; }
        public IList<TopCustomerReportLineModel> TopCustomersByAmount { get; set; }
    }
}