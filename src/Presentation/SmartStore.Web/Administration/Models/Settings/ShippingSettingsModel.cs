using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Admin.Models.Common;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
    public class ShippingSettingsModel
    {
        public string PrimaryStoreCurrencyCode { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.FreeShippingOverXEnabled")]
        public bool FreeShippingOverXEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.FreeShippingOverXValue")]
        public decimal FreeShippingOverXValue { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.FreeShippingOverXIncludingTax")]
        public bool FreeShippingOverXIncludingTax { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.EstimateShippingEnabled")]
        public bool EstimateShippingEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.DisplayShipmentEventsToCustomers")]
        public bool DisplayShipmentEventsToCustomers { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.ShippingOriginAddress")]
        public AddressModel ShippingOriginAddress { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.SkipShippingIfSingleOption")]
        public bool SkipShippingIfSingleOption { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.ChargeOnlyHighestProductShippingSurcharge")]
        public bool ChargeOnlyHighestProductShippingSurcharge { get; set; }

        #region Delivery Time

        [SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.DeliveryOnWorkweekDaysOnly")]
        public bool DeliveryOnWorkweekDaysOnly { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.TodayShipmentHour")]
        public int? TodayShipmentHour { get; set; }
        public List<SelectListItem> TodayShipmentHours { get; set; }

        #endregion
    }
}