using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Orders;
using SmartStore.Core.Domain.Orders;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    [Validator(typeof(ReturnRequestValidator))]
    public class ReturnRequestModel : EntityModelBase
    {
		public ReturnRequestModel()
		{
			AvailableReasonForReturn = new List<SelectListItem>();
			AvailableRequestedAction = new List<SelectListItem>();
			AutoUpdateOrderItem = new AutoUpdateOrderItemModel();
		}

        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.ID")]
        public override int Id { get; set; }

        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.Order")]
        public int OrderId { get; set; }
		public string OrderNumber { get; set; }

        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.Customer")]
        public int CustomerId { get; set; }
		public string CustomerFullName { get; set; }
		public bool CanSendEmailToCustomer { get; set; }

        public int ProductId { get; set; }
		public string ProductSku { get; set; }

		[SmartResourceDisplayName("Admin.ReturnRequests.Fields.Product")]
        public string ProductName { get; set; }
		public string ProductTypeName { get; set; }
		public string ProductTypeLabelHint { get; set; }

		[SmartResourceDisplayName("Admin.Customers.Customers.Orders.Store")]
		public string StoreName { get; set; }

        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.Quantity")]
        public int Quantity { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.ReasonForReturn")]
        public string ReasonForReturn { get; set; }
		public IList<SelectListItem> AvailableReasonForReturn { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.RequestedAction")]
        public string RequestedAction { get; set; }
		public IList<SelectListItem> AvailableRequestedAction { get; set; }

		[SmartResourceDisplayName("Admin.ReturnRequests.Fields.RequestedActionUpdatedOnUtc")]
		public DateTime? RequestedActionUpdated { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.CustomerComments")]
        public string CustomerComments { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.StaffNotes")]
        public string StaffNotes { get; set; }

		[SmartResourceDisplayName("Admin.Common.AdminComment")]
		public string AdminComment { get; set; }

        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.Status")]
        public int ReturnRequestStatusId { get; set; }
        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.Status")]
        public string ReturnRequestStatusStr { get; set; }

        [SmartResourceDisplayName("Admin.ReturnRequests.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

		[SmartResourceDisplayName("Common.UpdatedOn")]
		public DateTime UpdatedOn { get; set; }

		public bool CanAccept
		{
			get
			{
				if (Id != 0 && (ReturnRequestStatus)ReturnRequestStatusId < ReturnRequestStatus.ReturnAuthorized)
					return true;

				return false;
			}
		}

		public string ReturnRequestInfo { get; set; }

		public AutoUpdateOrderItemModel AutoUpdateOrderItem { get; set; }
    }
}