using System.Collections.Generic;
using System.Linq;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Services.Catalog;
using SmartStore.Core.Domain.Dashboard;

namespace SmartStore.Admin.Models.Orders
{
    public class DashboardChartReportModel : ModelBase
    {
        public DashboardChartReportLine[] Reports { get; set; } = new DashboardChartReportLine[5];
        //}
        //public class DashboardChartReportLineModel : ModelBase
        //{
        //    public int PercentageDelta { get; set; }
        //    public string TotalAmount { get; set; }
        //    public string[] Labels { get; set; }

        //    public ChartDataPoint[] Data { get; set; }

        //    public DashboardChartReportLineModel(int amountDatasets, int amountDataPoints)
        //    {
        //        Data = new ChartDataPoint[amountDatasets];
        //        Labels = new string[amountDataPoints];
        //        for (int i = 0; i < Data.Length; i++)
        //        {
        //            Data[i] = new ChartDataPoint(amountDataPoints);
        //        }
        //    }
        //}
        //public class ChartDataPoint
        //{
        //    public string TotalAmount { get; set; }
        //    public int[] Quantity { get; set; }
        //    public decimal[] Amount { get; set; }
        //    public string[] FormattedAmount { get; set; }

        //    public ChartDataPoint(int amountDataPoints)
        //    {
        //        Quantity = new int[amountDataPoints];
        //        Amount = new decimal[amountDataPoints];
        //        FormattedAmount = new string[amountDataPoints];
        //    }
        //}
    }
}