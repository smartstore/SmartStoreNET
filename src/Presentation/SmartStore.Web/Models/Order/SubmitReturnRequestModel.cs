using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Models.Order
{
    public partial class SubmitReturnRequestModel : ModelBase
    {
        public SubmitReturnRequestModel()
        {
            Items = new List<OrderItemModel>();
            AvailableReturnReasons = new List<SelectListItem>();
            AvailableReturnActions= new List<SelectListItem>();
        }

        public int OrderId { get; set; }
        
        public IList<OrderItemModel> Items { get; set; }
        
        [AllowHtml]
        [SmartResourceDisplayName("ReturnRequests.ReturnReason")]
        public string ReturnReason { get; set; }
        public IList<SelectListItem> AvailableReturnReasons { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("ReturnRequests.ReturnAction")]
        public string ReturnAction { get; set; }
        public IList<SelectListItem> AvailableReturnActions { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("ReturnRequests.Comments")]
        public string Comments { get; set; }

        public string Result { get; set; }
        
        #region Nested classes

        public partial class OrderItemModel : EntityModelBase
        {
            public int ProductId { get; set; }

            public LocalizedValue<string> ProductName { get; set; }

            public string ProductSeName { get; set; }

			public string ProductUrl { get; set; }

            public string AttributeInfo { get; set; }

            public string UnitPrice { get; set; }

            public int Quantity { get; set; }
        }

        #endregion
    }

}