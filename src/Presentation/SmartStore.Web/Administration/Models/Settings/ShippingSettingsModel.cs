using SmartStore.Admin.Models.Common;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
    public class ShippingSettingsModel
    {
		public int ActiveStoreScopeConfiguration { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.FreeShippingOverXEnabled")]
		public StoreDependingSetting<bool> FreeShippingOverXEnabled { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.FreeShippingOverXValue")]
		public StoreDependingSetting<decimal> FreeShippingOverXValue { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.FreeShippingOverXIncludingTax")]
		public StoreDependingSetting<bool> FreeShippingOverXIncludingTax { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.EstimateShippingEnabled")]
		public StoreDependingSetting<bool> EstimateShippingEnabled { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.DisplayShipmentEventsToCustomers")]
		public StoreDependingSetting<bool> DisplayShipmentEventsToCustomers { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.Shipping.ShippingOriginAddress")]
		public StoreDependingSetting<AddressModel> ShippingOriginAddress { get; set; }
    }
}