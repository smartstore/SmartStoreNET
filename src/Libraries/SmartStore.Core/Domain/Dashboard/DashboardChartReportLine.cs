namespace SmartStore.Core.Domain.Dashboard
{
    public class DashboardChartReportLine
    {
        public DashboardChartReportLine(int amountDatasets, int amountDataPoints)
        {
            DataSets = new ChartDataPoint[amountDatasets];
            Labels = new string[amountDataPoints];
            for (int i = 0; i < DataSets.Length; i++)
            {
                DataSets[i] = new ChartDataPoint(amountDataPoints);
            }
        }

        public string TotalAmount { get; set; }
        public int PercentageDelta { get; set; }
        public string[] Labels { get; set; }

        public ChartDataPoint[] DataSets { get; set; }
    }

    public class ChartDataPoint
    {
        public ChartDataPoint(int amountDataPoints)
        {
            Quantity = new int[amountDataPoints];
            Amount = new decimal[amountDataPoints];
            FormattedAmount = new string[amountDataPoints];
        }

        public string TotalAmount { get; set; }
        public int[] Quantity { get; set; }
        public decimal[] Amount { get; set; }
        public string[] FormattedAmount { get; set; }
    }

    public enum PeriodState
    {
        Today,
        Yesterday,
        Week,
        Month,
        Year,
    }
}
