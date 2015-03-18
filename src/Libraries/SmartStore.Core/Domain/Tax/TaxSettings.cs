﻿
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Tax
{
    public class TaxSettings : ISettings
    {
		public TaxSettings()
		{
			TaxBasedOn = TaxBasedOn.BillingAddress;
			TaxDisplayType = TaxDisplayType.ExcludingTax;
			ActiveTaxProviderSystemName = "Tax.FixedRate";
			DisplayTaxSuffix = true;
			PricesIncludeTax = true;
			EuVatAllowVatExemption = true;
			ShowLegalHintsInFooter = true; 
			ShowLegalHintsInProductDetails = true;
			ShowLegalHintsInProductList = true;
		}
		
		/// <summary>
        /// Tax based on
        /// </summary>
        public TaxBasedOn TaxBasedOn { get; set; }

        /// <summary>
        /// Tax display type
        /// </summary>
        public TaxDisplayType TaxDisplayType { get; set; }

        /// <summary>
        /// Gets or sets an system name of active tax provider
        /// </summary>
        public string ActiveTaxProviderSystemName { get; set; }

        /// <summary>
        /// Gets or sets default address used for tax calculation
        /// </summary>
        public int DefaultTaxAddressId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display tax suffix
        /// </summary>
        public bool DisplayTaxSuffix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether each tax rate should be displayed on separate line (shopping cart page)
        /// </summary>
        public bool DisplayTaxRates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether prices incude tax
        /// </summary>
        public bool PricesIncludeTax { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to select tax display type
        /// </summary>
        public bool AllowCustomersToSelectTaxDisplayType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide zero tax
        /// </summary>
        public bool HideZeroTax { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide tax in order summary when prices are shown tax inclusive
        /// </summary>
        public bool HideTaxInOrderSummary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show legal info in product list
        /// </summary>
        public bool ShowLegalHintsInProductList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show legal info in product detail
        /// </summary>
        public bool ShowLegalHintsInProductDetails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show legal info in footer
        /// </summary>
        public bool ShowLegalHintsInFooter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether shipping price is taxable
        /// </summary>
        public bool ShippingIsTaxable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether shipping price incudes tax
        /// </summary>
        public bool ShippingPriceIncludesTax { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the shipping tax class identifier
        /// </summary>
        public int ShippingTaxClassId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether payment method additional fee is taxable
        /// </summary>
        public bool PaymentMethodAdditionalFeeIsTaxable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether payment method additional fee incudes tax
        /// </summary>
        public bool PaymentMethodAdditionalFeeIncludesTax { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the payment method additional fee tax class identifier
        /// </summary>
        public int PaymentMethodAdditionalFeeTaxClassId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether EU VAT (Eupore Union Value Added Tax) is enabled
        /// </summary>
        public bool EuVatEnabled { get; set; }

        /// <summary>
        /// Gets or sets a shop country identifier
        /// </summary>
        public int EuVatShopCountryId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this store will exempt eligible VAT-registered customers from VAT
        /// </summary>
        public bool EuVatAllowVatExemption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should use the EU web service to validate VAT numbers
        /// </summary>
        public bool EuVatUseWebService { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should notify a store owner when a new VAT number is submitted
        /// </summary>
        public bool EuVatEmailAdminWhenNewVatSubmitted { get; set; }
    }
}