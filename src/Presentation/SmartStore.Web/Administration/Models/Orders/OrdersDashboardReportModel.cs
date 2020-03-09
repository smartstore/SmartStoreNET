using System.Collections.Generic;
using System.Linq;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class OrdersDashboardReportModel : ModelBase
    {
        // Struktur:
        // 4x Klasse mit jeweils:
        //public string[] OrderLabelsDay { get; set; }
        //public string OrdersPercentageDeltaDay { get; set; }
        //public string OrdersAmountDay { get; set; }
        //public string CompleteTotalDay { get; set; }
        //public string CompleteAmountDay { get; set; }
        //public string PendingTotalDay { get; set; }
        //public string PendingAmountDay { get; set; }
        //public string ProcessingTotalDay { get; set; }
        //public string ProcessingAmountDay { get; set; }
        //public int[] CancelledTotalDay { get; set; }
        //public int[] CancelledAmountDay { get; set; }

        // for each period time - as data types as needed by chartjs!
        // think for yourself....

        public OrdersDashboardReportLineModel Day { get; set; }
        public OrdersDashboardReportLineModel Week { get; set; }
        public OrdersDashboardReportLineModel Month { get; set; }
        public OrdersDashboardReportLineModel Year { get; set; }
    }
    public class OrdersDashboardReportLineModel : ModelBase
    {
        public string[] Labels { get; set; }
        public string PercentageDelta { get; set; }
        public int TotalAmount { get; set; }

        //public ChartDataPoint Complete { get; set; }
        //public ChartDataPoint Pending { get; set; }
        //public ChartDataPoint Processing { get; set; }
        //public ChartDataPoint Cancelled { get; set; }
        public ChartDataPoint[] Data { get; set; }

        public OrdersDashboardReportLineModel(int amountDatasets)
        {
            Data = new ChartDataPoint[4];
            Data[0] = new ChartDataPoint(amountDatasets);
            Data[1] = new ChartDataPoint(amountDatasets);
            Data[2] = new ChartDataPoint(amountDatasets);
            Data[3] = new ChartDataPoint(amountDatasets);

            //Complete = new ChartDataPoint(amountDatasets);
            //Pending = new ChartDataPoint(amountDatasets);
            //Processing = new ChartDataPoint(amountDatasets);
            //Cancelled = new ChartDataPoint(amountDatasets);
        }
    }

    public class ChartDataPoint
    {
        public int TotalAmount { get; set; } = 0;
        public int[] Quantity { get; set; }
        public int[] Amount { get; set; }
        
        public ChartDataPoint(int amountDatasets)
        {
            Quantity = new int[amountDatasets];
            Amount = new int[amountDatasets];
        }
    }
}