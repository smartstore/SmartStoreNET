using System;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public class ShipmentListModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.Orders.Shipments.List.StartDate")]
        public DateTime? StartDate { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Shipments.List.EndDate")]
        public DateTime? EndDate { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Shipments.List.TrackingNumber")]
        [AllowHtml]
        public string TrackingNumber { get; set; }

        public bool DisplayPdfPackagingSlip { get; set; }
    }
}