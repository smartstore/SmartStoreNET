using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Admin.Models.Common;
using SmartStore.Core.Domain.Tax;
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
            DefaultTaxAddress = new AddressModel();
            VatRequired = false;
        }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.PricesIncludeTax")]
        public bool PricesIncludeTax { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.AllowCustomersToSelectTaxDisplayType")]
        public bool AllowCustomersToSelectTaxDisplayType { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.TaxDisplayType")]
        public TaxDisplayType TaxDisplayType { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.DisplayTaxSuffix")]
        public bool DisplayTaxSuffix { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.DisplayTaxRates")]
        public bool DisplayTaxRates { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.HideZeroTax")]
        public bool HideZeroTax { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.HideTaxInOrderSummary")]
        public bool HideTaxInOrderSummary { get; set; }


        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.ShowLegalHintsInProductList")]
        public bool ShowLegalHintsInProductList { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.ShowLegalHintsInProductDetails")]
        public bool ShowLegalHintsInProductDetails { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.ShowLegalHintsInFooter")]
        public bool ShowLegalHintsInFooter { get; set; }


        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.TaxBasedOn")]
        public TaxBasedOn TaxBasedOn { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.DefaultTaxAddress")]
        public AddressModel DefaultTaxAddress { get; set; }


        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.ShippingIsTaxable")]
        public bool ShippingIsTaxable { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.ShippingPriceIncludesTax")]
        public bool ShippingPriceIncludesTax { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.ShippingTaxClass")]
        public int? ShippingTaxClassId { get; set; }
        public IList<SelectListItem> ShippingTaxCategories { get; set; }


        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.PaymentMethodAdditionalFeeIsTaxable")]
        public bool PaymentMethodAdditionalFeeIsTaxable { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.PaymentMethodAdditionalFeeIncludesTax")]
        public bool PaymentMethodAdditionalFeeIncludesTax { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.PaymentMethodAdditionalFeeTaxClass")]
        public int? PaymentMethodAdditionalFeeTaxClassId { get; set; }
        public IList<SelectListItem> PaymentMethodAdditionalFeeTaxCategories { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.AuxiliaryServicesTaxingType")]
        public AuxiliaryServicesTaxType AuxiliaryServicesTaxingType { get; set; }


        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.EuVatEnabled")]
        public bool EuVatEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.EuVatShopCountry")]
        public int? EuVatShopCountryId { get; set; }
        public IList<SelectListItem> EuVatShopCountries { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.EuVatAllowVatExemption")]
        public bool EuVatAllowVatExemption { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.EuVatUseWebService")]
        public bool EuVatUseWebService { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.EuVatEmailAdminWhenNewVatSubmitted")]
        public bool EuVatEmailAdminWhenNewVatSubmitted { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Tax.VatRequired")]
        public bool VatRequired { get; set; }
    }
}