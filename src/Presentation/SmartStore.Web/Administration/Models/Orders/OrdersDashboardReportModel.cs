using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class OrdersDashboardReportModel : ModelBase
    {
        public string[] OrderLabelsDay { get; set; }
        public int OrdersPercentageDeltaDay { get; set; }
        public int[] OrdersTotalDay { get; set; }
        public int[] OrdersTotalDayBefore { get; set; }
        public int[] OrdersAmountDay { get; set; }
        public int[] OrdersAmountDayBefore { get; set; }
        public int OrdersSumDay { get; set; }
        public int OrdersSumDayBefore { get; set; }

        public string[] LabelsWeek { get; set; }
        public int[] OrdersTotalWeek { get; set; }
        public int[] OrdersTotalWeekBefore { get; set; }
        public int[] OrdersAmountWeek { get; set; }
        public int[] OrdersAmountWeekBefore { get; set; }
        public int OrdersSumWeek { get; set; }
        public int OrdersSumWeekBefore { get; set; }
        public int OrdersPercentageDeltaWeek { get; set; }

        public string[] LabelsMonth { get; set; }
        public int[] OrdersTotalMonth { get; set; }
        public int[] OrdersTotalMonthBefore { get; set; }
        public int[] OrdersAmountMonth { get; set; }
        public int[] OrdersAmountMonthBefore { get; set; }
        public int OrdersSumMonth { get; set; }
        public int OrdersSumMonthBefore { get; set; }
        public int OrdersPercentageDeltaMonth { get; set; }

        public string[] LabelsYear { get; set; }
        public int[] OrdersTotalYear { get; set; }
        public int[] OrdersTotalYearBefore { get; set; }
        public int[] OrdersAmountYear { get; set; }
        public int[] OrdersAmountYearBefore { get; set; }
        public int OrdersSumYear { get; set; }
        public int OrdersSumYearBefore { get; set; }
        public int OrdersPercentageDeltaYear { get; set; }



    }
}