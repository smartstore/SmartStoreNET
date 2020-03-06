using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class OrdersDashboardReportModel : ModelBase
    {
        // Day
        public List<string> OrderLabelsDay { get; set; }
        public int OrdersSumDay { get; set; }
        public int OrdersSumDayBefore { get; set; }
        public int OrdersPercentageDeltaDay { get; set; }

        public int[] CompleteTotalDay { get; set; }
        public int[] CompleteTotalDayBefore { get; set; } // this is stupid, what should this do?????
        public int[] CompleteAmountDay { get; set; }
        public int[] CompleteAmountDayBefore { get; set; }   // is this needed? jsut calc the deklta on the fly...?

        public int[] PendingTotalDay { get; set; }
        public int[] PendingTotalDayBefore { get; set; }
        public int[] PendingAmountDay { get; set; }
        public int[] PendingAmountDayBefore { get; set; }

        public int[] ProcessingTotalDay { get; set; }
        public int[] ProcessingTotalDayBefore { get; set; }
        public int[] ProcessingAmountDay { get; set; }
        public int[] ProcessingAmountDayBefore { get; set; }

        public int[] CancelledTotalDay { get; set; }
        public int[] CancelledTotalDayBefore { get; set; }
        public int[] CancelledAmountDay { get; set; }
        public int[] CancelledAmountDayBefore { get; set; }

        // Week
        public List<string> OrderLabelsWeek { get; set; }
        public int OrdersSumWeek { get; set; }
        public int OrdersSumWeekBefore { get; set; }
        public int OrdersPercentageDeltaWeek { get; set; }

        public int[] CompleteTotalWeek { get; set; }
        public int[] CompleteTotalWeekBefore { get; set; }
        public int[] CompleteAmountWeek { get; set; }
        public int[] CompleteAmountWeekBefore { get; set; }

        public int[] PendingTotalWeek { get; set; }
        public int[] PendingTotalWeekBefore { get; set; }
        public int[] PendingAmountWeek { get; set; }
        public int[] PendingAmountWeekBefore { get; set; }
        
        public int[] ProcessingTotalWeek { get; set; }
        public int[] ProcessingTotalWeekBefore { get; set; }
        public int[] ProcessingAmountWeek { get; set; }
        public int[] ProcessingAmountWeekBefore { get; set; }
        
        public int[] CancelledTotalWeek { get; set; }
        public int[] CancelledTotalWeekBefore { get; set; }
        public int[] CancelledAmountWeek { get; set; }
        public int[] CancelledAmountWeekBefore { get; set; }
        
        // Month
        public List<string> OrderLabelsMonth { get; set; }
        public int OrdersSumMonth { get; set; }
        public int OrdersSumMonthBefore { get; set; }
        public int OrdersPercentageDeltaMonth { get; set; }

        public int[] CompleteTotalMonth { get; set; }
        public int[] CompleteTotalMonthBefore { get; set; }
        public int[] CompleteAmountMonth { get; set; }
        public int[] CompleteAmountMonthBefore { get; set; }

        public int[] PendingTotalMonth { get; set; }
        public int[] PendingTotalMonthBefore { get; set; }
        public int[] PendingAmountMonth { get; set; }
        public int[] PendingAmountMonthBefore { get; set; }

        public int[] ProcessingTotalMonth { get; set; }
        public int[] ProcessingTotalMonthBefore { get; set; }
        public int[] ProcessingAmountMonth { get; set; }
        public int[] ProcessingAmountMonthBefore { get; set; }

        public int[] CancelledTotalMonth { get; set; }
        public int[] CancelledTotalMonthBefore { get; set; }
        public int[] CancelledAmountMonth { get; set; }
        public int[] CancelledAmountMonthBefore { get; set; }

        // Year
        public List<string> OrderLabelsYear { get; set; }
        public int OrdersSumYear { get; set; }
        public int OrdersSumYearBefore { get; set; }
        public int OrdersPercentageDeltaYear { get; set; }

        public int[] CompleteTotalYear { get; set; }
        public int[] CompleteTotalYearBefore { get; set; }
        public int[] CompleteAmountYear { get; set; }
        public int[] CompleteAmountYearBefore { get; set; }

        public int[] PendingTotalYear { get; set; }
        public int[] PendingTotalYearBefore { get; set; }
        public int[] PendingAmountYear { get; set; }
        public int[] PendingAmountYearBefore { get; set; }

        public int[] ProcessingTotalYear { get; set; }
        public int[] ProcessingTotalYearBefore { get; set; }
        public int[] ProcessingAmountYear { get; set; }
        public int[] ProcessingAmountYearBefore { get; set; }

        public int[] CancelledTotalYear { get; set; }
        public int[] CancelledTotalYearBefore { get; set; }
        public int[] CancelledAmountYear { get; set; }
        public int[] CancelledAmountYearBefore { get; set; }
    }
}