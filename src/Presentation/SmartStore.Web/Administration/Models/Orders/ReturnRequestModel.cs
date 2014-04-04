using System;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Orders;
using SmartStore.Core.Domain.Orders;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Orders
{
    [Validator(typeof(ReturnRequestValidator))]
    public class ReturnRequestModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.ID")]
        public override int Id { get; set; }

        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.Order")]
        public int OrderId { get; set; }

        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.Customer")]
        public int CustomerId { get; set; }
		public string CustomerFullName { get; set; }

        public int ProductId { get; set; }

		[SmartResourceDisplayName("Admin.ReturnRequests.Fields.Product")]
        public string ProductName { get; set; }
		public string ProductTypeName { get; set; }
		public string ProductTypeLabelHint { get; set; }

        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.Quantity")]
        public int Quantity { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.ReasonForReturn")]
        public string ReasonForReturn { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.RequestedAction")]
        public string RequestedAction { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.CustomerComments")]
        public string CustomerComments { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.StaffNotes")]
        public string StaffNotes { get; set; }

        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.Status")]
        public int ReturnRequestStatusId { get; set; }
        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.Status")]
        public string ReturnRequestStatusStr { get; set; }

        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}