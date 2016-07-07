﻿using SmartStore.Admin.Models.Common;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
	public class ShippingSettingsModel
    {
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
    }
}