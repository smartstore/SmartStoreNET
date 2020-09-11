namespace SmartStore.Admin.Models.Dashboard
{
    public class DashboardChartReportModel
    {
        public DashboardChartReportModel(int amountDatasets, int amountDataPoints)
        {
            DataSets = new ChartDataPointReport[amountDatasets];
            Labels = new string[amountDataPoints];
            for (int i = 0; i < DataSets.Length; i++)
            {
                DataSets[i] = new ChartDataPointReport(amountDataPoints);
            }
        }

        public string TotalAmountFormatted { get; set; }
        public decimal TotalAmount { get; set; }
        public int PercentageDelta { get; set; }
        public string[] Labels { get; set; }

        public ChartDataPointReport[] DataSets { get; set; }
    }

    public class ChartDataPointReport
    {
        public ChartDataPointReport(int amountDataPoints)
        {
            Quantity = new int[amountDataPoints];
            Amount = new decimal[amountDataPoints];
            AmountFormatted = new string[amountDataPoints];
            QuantityFormatted = new string[amountDataPoints];
        }

        public decimal TotalAmount { get; set; }
        public string TotalAmountFormatted { get; set; }
        public int[] Quantity { get; set; }
        public string[] QuantityFormatted { get; set; }
        public decimal[] Amount { get; set; }
        public string[] AmountFormatted { get; set; }
    }
}
