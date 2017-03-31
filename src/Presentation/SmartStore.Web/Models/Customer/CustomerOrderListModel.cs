using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
	public partial class CustomerOrderListModel : ModelBase
    {
        public CustomerOrderListModel()
        {
			RecurringOrders = new List<RecurringOrderModel>();
			CancelRecurringPaymentErrors = new List<string>();
        }

        public PagedList<OrderDetailsModel> Orders { get; set; }
        public IList<RecurringOrderModel> RecurringOrders { get; set; }
        public IList<string> CancelRecurringPaymentErrors { get; set; }

        #region Nested classes

        public partial class OrderDetailsModel : EntityModelBase
        {
            public string OrderNumber { get; set; }
            public string OrderTotal { get; set; }
            public bool IsReturnRequestAllowed { get; set; }
            public string OrderStatus { get; set; }
            public DateTime CreatedOn { get; set; }
        }

        public partial class RecurringOrderModel : EntityModelBase
        {
            public string StartDate { get; set; }
            public string CycleInfo { get; set; }
            public string NextPayment { get; set; }
            public int TotalCycles { get; set; }
            public int CyclesRemaining { get; set; }
            public int InitialOrderId { get; set; }
            public bool CanCancel { get; set; }
        }

        #endregion
    }
}