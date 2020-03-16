using SmartStore.Core.Domain.Dashboard;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class DashboardChartReportModel : ModelBase
    {
        public DashboardChartReportLine[] Reports { get; set; } = new DashboardChartReportLine[5];       
    }
}