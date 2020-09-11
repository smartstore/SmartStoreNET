using System;
using System.Collections.Generic;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Models.Customer
{
    public partial class CustomerReturnRequestsModel : ModelBase
    {
        public CustomerReturnRequestsModel()
        {
            Items = new List<ReturnRequestModel>();
        }

        public IList<ReturnRequestModel> Items { get; set; }

        #region Nested classes

        public partial class ReturnRequestModel : EntityModelBase
        {
            public string ReturnRequestStatus { get; set; }
            public int ProductId { get; set; }
            public LocalizedValue<string> ProductName { get; set; }
            public string ProductSeName { get; set; }
            public string ProductUrl { get; set; }
            public int Quantity { get; set; }

            public string ReturnReason { get; set; }
            public string ReturnAction { get; set; }
            public string Comments { get; set; }

            public DateTime CreatedOn { get; set; }
        }

        #endregion
    }
}