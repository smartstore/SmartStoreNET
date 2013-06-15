using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Admin.Models.Common;
using SmartStore.Core.Domain.Tax;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
    public class TaxSettingsModel
    {
        public TaxSettingsModel()
        {
            PaymentMethodAdditionalFeeTaxCategories = new List<SelectListItem>();
            ShippingTaxCategories = new List<SelectListItem>();
            EuVatShopCountries = new List<SelectListItem>();
        }

		public int ActiveStoreScopeConfiguration { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.PricesIncludeTax")]
        public StoreDependingSetting<bool> PricesIncludeTax { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.AllowCustomersToSelectTaxDisplayType")]
        public StoreDependingSetting<bool> AllowCustomersToSelectTaxDisplayType { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.TaxDisplayType")]
        public StoreDependingSetting<TaxDisplayType> TaxDisplayType { get; set; }
		public SelectList TaxDisplayTypeValues { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.DisplayTaxSuffix")]
        public StoreDependingSetting<bool> DisplayTaxSuffix { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.DisplayTaxRates")]
        public StoreDependingSetting<bool> DisplayTaxRates { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.HideZeroTax")]
        public StoreDependingSetting<bool> HideZeroTax { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.HideTaxInOrderSummary")]
        public StoreDependingSetting<bool> HideTaxInOrderSummary { get; set; }


        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.ShowLegalHintsInProductList")]
        public StoreDependingSetting<bool> ShowLegalHintsInProductList { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.ShowLegalHintsInProductDetails")]
        public StoreDependingSetting<bool> ShowLegalHintsInProductDetails { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.ShowLegalHintsInFooter")]
        public StoreDependingSetting<bool> ShowLegalHintsInFooter { get; set; }

		
        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.TaxBasedOn")]
        public StoreDependingSetting<TaxBasedOn> TaxBasedOn { get; set; }
		public SelectList TaxBasedOnValues { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.DefaultTaxAddress")]
        public StoreDependingSetting<AddressModel> DefaultTaxAddress { get; set; }
       

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.ShippingIsTaxable")]
        public StoreDependingSetting<bool> ShippingIsTaxable { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.ShippingPriceIncludesTax")]
        public StoreDependingSetting<bool> ShippingPriceIncludesTax { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.ShippingTaxClass")]
        public StoreDependingSetting<int> ShippingTaxClassId { get; set; }
		public IList<SelectListItem> ShippingTaxCategories { get; set; }

		
        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.PaymentMethodAdditionalFeeIsTaxable")]
        public StoreDependingSetting<bool> PaymentMethodAdditionalFeeIsTaxable { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.PaymentMethodAdditionalFeeIncludesTax")]
        public StoreDependingSetting<bool> PaymentMethodAdditionalFeeIncludesTax { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.PaymentMethodAdditionalFeeTaxClass")]
        public StoreDependingSetting<int> PaymentMethodAdditionalFeeTaxClassId { get; set; }
		public IList<SelectListItem> PaymentMethodAdditionalFeeTaxCategories { get; set; }
		

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.EuVatEnabled")]
        public StoreDependingSetting<bool> EuVatEnabled { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.EuVatShopCountry")]
        public StoreDependingSetting<int> EuVatShopCountryId { get; set; }
		public IList<SelectListItem> EuVatShopCountries { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.EuVatAllowVatExemption")]
        public StoreDependingSetting<bool> EuVatAllowVatExemption { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.EuVatUseWebService")]
        public StoreDependingSetting<bool> EuVatUseWebService { get; set; }
        
		[SmartResourceDisplayName("Admin.Configuration.Settings.Tax.EuVatEmailAdminWhenNewVatSubmitted")]
        public StoreDependingSetting<bool> EuVatEmailAdminWhenNewVatSubmitted { get; set; }
    }
}