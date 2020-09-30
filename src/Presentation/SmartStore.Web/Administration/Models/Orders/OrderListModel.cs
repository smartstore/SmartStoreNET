using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class OrderListModel : ModelBase
    {
        public OrderListModel()
        {
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableShippingStatuses = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailablePaymentMethods = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Admin.Orders.List.StartDate")]
        public DateTime? StartDate { get; set; }

        [SmartResourceDisplayName("Admin.Orders.List.EndDate")]
        public DateTime? EndDate { get; set; }

        [SmartResourceDisplayName("Admin.Orders.List.CustomerEmail")]
        [AllowHtml]
        public string CustomerEmail { get; set; }

        [SmartResourceDisplayName("Admin.Orders.List.CustomerName")]
        public string CustomerName { get; set; }

        [SmartResourceDisplayName("Admin.Orders.List.OrderStatus")]
        public string OrderStatusIds { get; set; }

        [SmartResourceDisplayName("Admin.Orders.List.PaymentStatus")]
        public string PaymentStatusIds { get; set; }

        [SmartResourceDisplayName("Admin.Orders.List.ShippingStatus")]
        public string ShippingStatusIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.SearchFor")]
        public int StoreId { get; set; }

        [SmartResourceDisplayName("Order.PaymentMethod")]
        public string PaymentMethods { get; set; }

        [SmartResourceDisplayName("Admin.Orders.List.OrderGuid")]
        [AllowHtml]
        public string OrderGuid { get; set; }

        [SmartResourceDisplayName("Admin.Orders.List.OrderNumber")]
        [AllowHtml]
        public string OrderNumber { get; set; }

        [SmartResourceDisplayName("Admin.Orders.List.GoDirectlyToNumber")]
        [AllowHtml]
        public string GoDirectlyToNumber { get; set; }

        public int GridPageSize { get; set; }

        // ProductId is only filled in context of product details (orders)
        // It is empty (null) in orders list
        public int? ProductId { get; set; }

        public bool HideProfitReport { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
        public IList<SelectListItem> AvailableShippingStatuses { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailablePaymentMethods { get; set; }
    }
}