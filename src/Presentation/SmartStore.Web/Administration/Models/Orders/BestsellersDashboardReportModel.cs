using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class BestsellersDashboardReportModel : ModelBase
    {
        public IList<BestsellersReportLineModel> BestsellersByQuantity { get; set; }
        public IList<BestsellersReportLineModel> BestsellersByAmount { get; set; }
    }
}