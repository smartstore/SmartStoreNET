using System;
using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class RecurringPaymentModel : EntityModelBase
    {
        public RecurringPaymentModel()
        {
            History = new List<RecurringPaymentHistoryModel>();
        }

        [SmartResourceDisplayName("Admin.RecurringPayments.Fields.CycleLength")]
        public int CycleLength { get; set; }

        [SmartResourceDisplayName("Admin.RecurringPayments.Fields.CyclePeriod")]
        public int CyclePeriodId { get; set; }

        [SmartResourceDisplayName("Admin.RecurringPayments.Fields.CyclePeriod")]
        public string CyclePeriodStr { get; set; }

        [SmartResourceDisplayName("Admin.RecurringPayments.Fields.TotalCycles")]
        public int TotalCycles { get; set; }

        [SmartResourceDisplayName("Admin.RecurringPayments.Fields.StartDate")]
        public string StartDate { get; set; }

        [SmartResourceDisplayName("Admin.RecurringPayments.Fields.IsActive")]
        public bool IsActive { get; set; }

        [SmartResourceDisplayName("Admin.RecurringPayments.Fields.NextPaymentDate")]
        public string NextPaymentDate { get; set; }

        [SmartResourceDisplayName("Admin.RecurringPayments.Fields.CyclesRemaining")]
        public int CyclesRemaining { get; set; }

        [SmartResourceDisplayName("Admin.RecurringPayments.Fields.InitialOrder")]
        public int InitialOrderId { get; set; }

        [SmartResourceDisplayName("Admin.RecurringPayments.Fields.Customer")]
        public int CustomerId { get; set; }
        [SmartResourceDisplayName("Admin.RecurringPayments.Fields.Customer")]
        public string CustomerEmail { get; set; }

        [SmartResourceDisplayName("Admin.RecurringPayments.Fields.PaymentType")]
        public string PaymentType { get; set; }

        public bool CanCancelRecurringPayment { get; set; }


        public IList<RecurringPaymentHistoryModel> History { get; set; }

        #region Nested classes


        public class RecurringPaymentHistoryModel : EntityModelBase
        {
            [SmartResourceDisplayName("Admin.RecurringPayments.History.Order")]
            public int OrderId { get; set; }

            public int RecurringPaymentId { get; set; }

            [SmartResourceDisplayName("Admin.RecurringPayments.History.OrderStatus")]
            public string OrderStatus { get; set; }

            [SmartResourceDisplayName("Admin.RecurringPayments.History.PaymentStatus")]
            public string PaymentStatus { get; set; }

            [SmartResourceDisplayName("Admin.RecurringPayments.History.ShippingStatus")]
            public string ShippingStatus { get; set; }

            [SmartResourceDisplayName("Common.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        #endregion
    }
}