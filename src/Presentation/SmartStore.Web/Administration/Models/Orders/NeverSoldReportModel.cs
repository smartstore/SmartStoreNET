using System;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class NeverSoldReportModel : ModelBase
    {
        public int GridPageSize { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.NeverSold.StartDate")]
        public DateTime? StartDate { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.NeverSold.EndDate")]
        public DateTime? EndDate { get; set; }
    }
}