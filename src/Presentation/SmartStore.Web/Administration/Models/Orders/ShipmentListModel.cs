using System;
using System.ComponentModel.DataAnnotations;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Orders
{
    public class ShipmentListModel : ModelBase
    {
        [SmartResourceDisplayName("Admin.Orders.Shipments.List.StartDate")]
        public DateTime? StartDate { get; set; }

        [SmartResourceDisplayName("Admin.Orders.Shipments.List.EndDate")]
        public DateTime? EndDate { get; set; }

        public bool DisplayPdfPackagingSlip { get; set; }
    }
}