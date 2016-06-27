using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    public partial class BestsellersReportModel : ModelBase
    {
        public BestsellersReportModel()
        {
            AvailableOrderStatuses = new List<SelectListItem>();
            AvailablePaymentStatuses = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Admin.SalesReport.Bestsellers.StartDate")]
        public DateTime? StartDate { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.Bestsellers.EndDate")]
        public DateTime? EndDate { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.Bestsellers.BillingCountry")]
        public int BillingCountryId { get; set; }

        [SmartResourceDisplayName("Admin.SalesReport.Bestsellers.OrderStatus")]
        public int OrderStatusId { get; set; }
        [SmartResourceDisplayName("Admin.SalesReport.Bestsellers.PaymentStatus")]
        public int PaymentStatusId { get; set; }

        public IList<SelectListItem> AvailableOrderStatuses { get; set; }
        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
    }
}