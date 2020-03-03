using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Store
{
    public class StoreDashboardReportModel : ModelBase
    {
        public Dictionary<string, int> StoreStatisticsReport { get; set; } = new Dictionary<string, int>();        

    }
}