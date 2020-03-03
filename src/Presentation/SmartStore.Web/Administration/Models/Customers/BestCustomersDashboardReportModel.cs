using System.Collections.Generic;
using SmartStore.Admin.Models.Customers;
using SmartStore.Core.Domain.Customers;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    public class BestCustomersDashboardReportModel : ModelBase
    {
        public IList<BestCustomerReportLineModel> BestCustomersByQuantity { get; set; }
        public IList<BestCustomerReportLineModel> BestCustomersByAmount { get; set; }
    }
}