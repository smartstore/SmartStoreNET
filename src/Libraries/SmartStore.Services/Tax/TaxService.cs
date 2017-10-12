using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Plugins;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Services.Tax
{
    /// <summary>
    /// Tax service
    /// </summary>
    public partial class TaxService : ITaxService
	{
		#region Nested classes

		private class TaxAddressKey : Tuple<int, bool> // <CustomerId, IsEsd>
		{
			public TaxAddressKey(int customerId, bool productIsEsd)
				: base(customerId, productIsEsd)
			{
			}
		}

		#endregion

		#region Fields

		private static readonly DateTime _euEsdRegulationStart = new DateTime(2015, 01, 01);

		private readonly IAddressService _addressService;
        private readonly IWorkContext _workContext;
        private readonly TaxSettings _taxSettings;
		private readonly ShoppingCartSettings _cartSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IDictionary<TaxRateCacheKey, decimal> _cachedTaxRates;
		private readonly IDictionary<TaxAddressKey, Address> _cachedTaxAddresses;
		private readonly IProviderManager _providerManager;
		private readonly IGeoCountryLookup _geoCountryLookup;

        #endregion

        #region Ctor

        public TaxService(
			IAddressService addressService,
            IWorkContext workContext,
            TaxSettings taxSettings,
			ShoppingCartSettings cartSettings,
            IPluginFinder pluginFinder,
			IGeoCountryLookup geoCountryLookup,
			IProviderManager providerManager)
        {
            this._addressService = addressService;
			this._workContext = workContext;
			this._taxSettings = taxSettings;
			this._cartSettings = cartSettings;
			this._pluginFinder = pluginFinder;
			this._cachedTaxRates = new Dictionary<TaxRateCacheKey, decimal>();
			this._cachedTaxAddresses = new Dictionary<TaxAddressKey, Address>();
			this._providerManager = providerManager;
			this._geoCountryLookup = geoCountryLookup;
        }

        #endregion

        #region Nested class

        internal class TaxRateCacheKey : Tuple<int, int, int>
        {
            public TaxRateCacheKey(int variantId, int taxCategoryId, int customerId)
                : base(variantId, taxCategoryId, customerId)
            {
            }
        }

        #endregion

        #region Utilities

        internal TaxRateCacheKey CreateTaxRateCacheKey(Product product, int taxCategoryId, Customer customer)
        {
            return new TaxRateCacheKey(
                product == null ? 0 : product.Id,
                taxCategoryId,
                customer == null ? 0 : customer.Id);
        }

        /// <summary>
        /// Create request for tax calculation
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>Package for tax calculation</returns>
        protected CalculateTaxRequest CreateCalculateTaxRequest(Product product, int taxCategoryId, Customer customer)
        {
            var calculateTaxRequest = new CalculateTaxRequest();
            calculateTaxRequest.Customer = customer;
            if (taxCategoryId > 0)
            {
                calculateTaxRequest.TaxCategoryId = taxCategoryId;
            }
            else
            {
                if (product != null)
                    calculateTaxRequest.TaxCategoryId = product.TaxCategoryId;
            }

            calculateTaxRequest.Address = this.GetTaxAddress(customer, product);
            return calculateTaxRequest;
        }

		/// <summary>
		/// Gets a value indicating whether the given customer is a consumer (NOT a business/company) within the EU
		/// </summary>
		/// <param name="customer">Customer</param>
		/// <returns><c>true</c> if the customer is a consumer, <c>false</c> otherwise</returns>
		/// <remarks>
		/// A customer is assumed to be a consumer if the default tax address doesn't include a company name,
		/// OR if a company name was specified but the EU VAT number for this record is invalid.
		/// </remarks>
		protected virtual bool IsEuConsumer(Customer customer)
		{
			if (customer == null)
				return false;

			var address = customer.BillingAddress;
			if (address != null && address.Company.IsEmpty())
			{
				// BillingAddress is explicitly set, but no CompanyName in there: so we assume a consumer 
				return true;
			}

			var country = address == null ? null : address.Country;

			if (country == null)
			{
				// No Country or BillingAddress set: try to resolve country from IP address
				_geoCountryLookup.IsEuIpAddress(customer.LastIpAddress, out country);
			}

			if (country == null || !country.SubjectToVat)
			{
				return false;
			}

			// It's EU: check VAT number status
			var vatStatus = (VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId);
			// companies with invalid VAT numbers are assumed to be consumers
			return vatStatus != VatNumberStatus.Valid;
		}

        protected virtual Address GetTaxAddress(Customer customer, Product product = null)
        {
			int customerId = customer != null ? customer.Id : 0;
			Address address = null;

			bool productIsEsd = product != null ? product.IsEsd : false;

			var cacheKey = new TaxAddressKey(customerId, productIsEsd);
			if (_cachedTaxAddresses.TryGetValue(cacheKey, out address))
			{
				return address;
			}

			var basedOn = _taxSettings.TaxBasedOn;

			// According to the new EU VAT regulations for electronic services from 2015 on,
			// VAT must be charged in the EU country the customer originates from (BILLING address).
			// In addition to this, the IP addresses' origin should also be checked for verification.
			if (DateTime.UtcNow > _euEsdRegulationStart)
			{
				if (_taxSettings.EuVatEnabled && productIsEsd)
				{
					if (IsEuConsumer(customer))
					{
						basedOn = TaxBasedOn.BillingAddress;
					}
				}
			}

			if (basedOn == TaxBasedOn.BillingAddress && (customer == null || customer.BillingAddress == null))
			{
				basedOn = TaxBasedOn.DefaultAddress;
			}
			if (basedOn == TaxBasedOn.ShippingAddress && (customer == null || customer.ShippingAddress == null))
			{
				basedOn = TaxBasedOn.DefaultAddress;
			}

			switch (basedOn)
			{
				case TaxBasedOn.BillingAddress:
					address = customer.BillingAddress;
					break;
				case TaxBasedOn.ShippingAddress:
					address = customer.ShippingAddress;
					break;
				case TaxBasedOn.DefaultAddress:
				default:
					address = _addressService.GetAddressById(_taxSettings.DefaultTaxAddressId);
					break;
			}

			_cachedTaxAddresses[cacheKey] = address;

			return address;
        }

        /// <summary>
        /// Calculated price
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="percent">Percent</param>
        /// <param name="increase">Increase</param>
        /// <returns>New price</returns>
        protected decimal CalculatePrice(decimal price, decimal percent, bool increase, Currency currency)
        {
            decimal result = decimal.Zero;
            if (percent == decimal.Zero)
                return price;

            if (increase)
            {
                result = price * (1 + percent / 100);
            }
            else
			{
				var decreaseValue = (price) / (100 + percent) * percent;
                result = price - decreaseValue;
			}

            // Gross > Net RoundFix
            result = result.RoundIfEnabledFor(currency);
            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load active tax provider
        /// </summary>
        /// <returns>Active tax provider</returns>
        public virtual Provider<ITaxProvider> LoadActiveTaxProvider()
        {
            var taxProvider = LoadTaxProviderBySystemName(_taxSettings.ActiveTaxProviderSystemName);
            if (taxProvider == null)
            {
                taxProvider = LoadAllTaxProviders().FirstOrDefault();
			}
            return taxProvider;
        }

        /// <summary>
        /// Load tax provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found tax provider</returns>
        public virtual Provider<ITaxProvider> LoadTaxProviderBySystemName(string systemName)
        {
			return _providerManager.GetProvider<ITaxProvider>(systemName);
        }

        /// <summary>
        /// Load all tax providers
        /// </summary>
        /// <returns>Tax providers</returns>
        public virtual IEnumerable<Provider<ITaxProvider>> LoadAllTaxProviders()
        {
			return _providerManager.GetAllProviders<ITaxProvider>();
        }


        private decimal GetOriginTaxRate(Product product)
        {
            return GetTaxRate(product, 0, null);
        }

        /// <summary>
        /// Gets tax rate
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Tax rate</returns>
        public virtual decimal GetTaxRate(Product product, Customer customer)
        {
            return GetTaxRate(product, product.TaxCategoryId, customer);
        }

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>Tax rate</returns>
        public virtual decimal GetTaxRate(int taxCategoryId, Customer customer)
        {
            return GetTaxRate(null, taxCategoryId, customer);
        }

        /// <summary>
        /// Gets tax rate
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>Tax rate</returns>
        public virtual decimal GetTaxRate(Product product, int taxCategoryId, Customer customer)
        {
            var cacheKey = this.CreateTaxRateCacheKey(product, taxCategoryId, customer);
            decimal result;
            if (!_cachedTaxRates.TryGetValue(cacheKey, out result))
            {
                result = GetTaxRateCore(product, taxCategoryId, customer);
                _cachedTaxRates[cacheKey] = result;
            }

            return result;
        }

        protected virtual decimal GetTaxRateCore(Product product, int taxCategoryId, Customer customer)
        {
			// active tax provider
			var activeTaxProvider = LoadActiveTaxProvider();
			if (activeTaxProvider == null)
			{
				return decimal.Zero;
			}

            if (IsTaxExempt(product, customer))
            {
                return decimal.Zero;
            }

			// tax request
            var calculateTaxRequest = CreateCalculateTaxRequest(product, taxCategoryId, customer);

			#region Legacy
			////make EU VAT exempt validation (the European Union Value Added Tax) (VATFIX)
            //if (_taxSettings.EuVatEnabled && IsVatExempt(calculateTaxRequest.Address, calculateTaxRequest.Customer))
            //{
            //    //return zero if VAT is not chargeable
            //    return decimal.Zero;
			//}
			#endregion

			//get tax rate
            var calculateTaxResult = activeTaxProvider.Value.GetTaxRate(calculateTaxRequest);
            if (calculateTaxResult.Success)
            {
                // ensure that tax is equal or greater than zero
                return Math.Max(0, calculateTaxResult.TaxRate);
            }
            else
            {
                return decimal.Zero;
            }
        }


        /// <summary>
        /// Gets price
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual decimal GetProductPrice(Product product, decimal price, out decimal taxRate)
        {
            return GetProductPrice(product, price, _workContext.CurrentCustomer, out taxRate);
        }

        /// <summary>
        /// Gets price
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual decimal GetProductPrice(Product product, decimal price, Customer customer, out decimal taxRate)
        {
            var includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;

            return GetProductPrice(product, price, includingTax, customer, out taxRate);
        }

		public virtual decimal GetProductPrice(Product product, decimal price, Customer customer, Currency currency, out decimal taxRate)
		{
			var includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
			var priceIncludesTax = _taxSettings.PricesIncludeTax;
			var taxCategoryId = product.TaxCategoryId; // 0; // (VATFIX)

			return GetProductPrice(product, taxCategoryId, price, includingTax, customer, currency, priceIncludesTax, out taxRate);
		}

		/// <summary>
		/// Gets price
		/// </summary>
		/// <param name="product">Product</param>
		/// <param name="price">Price</param>
		/// <param name="includingTax">A value indicating whether calculated price should include tax</param>
		/// <param name="customer">Customer</param>
		/// <param name="taxRate">Tax rate</param>
		/// <returns>Price</returns>
		public virtual decimal GetProductPrice(Product product, decimal price, bool includingTax, Customer customer, out decimal taxRate)
        {
            var priceIncludesTax = _taxSettings.PricesIncludeTax;
            var taxCategoryId = product.TaxCategoryId; // 0; // (VATFIX)

            return GetProductPrice(product, taxCategoryId, price, includingTax, customer, _workContext.WorkingCurrency, priceIncludesTax, out taxRate);
		}

		/// <summary>
		/// Gets price
		/// </summary>
		/// <param name="product">Product</param>
		/// <param name="taxCategoryId">Tax category identifier</param>
		/// <param name="price">Price</param>
		/// <param name="includingTax">A value indicating whether calculated price should include tax</param>
		/// <param name="customer">Customer</param>
		/// <param name="priceIncludesTax">A value indicating whether price already includes tax</param>
		/// <param name="taxRate">Tax rate</param>
		/// <returns>Price</returns>
		public virtual decimal GetProductPrice(
			Product product, 
			int taxCategoryId,
            decimal price, 
			bool includingTax, 
			Customer customer,
			Currency currency,
			bool priceIncludesTax, 
			out decimal taxRate)
        {
			// don't calculate if price is 0
			if (price == decimal.Zero)
			{
				taxRate = decimal.Zero;
				return decimal.Zero;
			}
			
			taxRate = GetTaxRate(product, taxCategoryId, customer);

            // Admin: GROSS prices
            if (priceIncludesTax)
            {
                if (!includingTax)
                {
                    price = CalculatePrice(price, taxRate, false, currency);
                }
            }
            // Admin: NET prices
            else
            {
                if (includingTax)
                {
                    price = CalculatePrice(price, taxRate, true, currency);
                }
            }

            //allowed to support negative price adjustments
            //if (price < decimal.Zero)
            //    price = decimal.Zero;

            return price;
        }




		public virtual decimal GetShippingPrice(decimal price, Customer customer)
        {
            var includingTax = (_workContext.TaxDisplayType == TaxDisplayType.IncludingTax);
            return GetShippingPrice(price, includingTax, customer);
        }

        public virtual decimal GetShippingPrice(decimal price, bool includingTax, Customer customer)
        {
            var taxRate = decimal.Zero;
            return GetShippingPrice(price, includingTax, customer, out taxRate);
        }

        public virtual decimal GetShippingPrice(decimal price, bool includingTax, Customer customer, out decimal taxRate)
        {
			return GetShippingPrice(price, includingTax, customer, _taxSettings.ShippingTaxClassId, out taxRate);
		}

		public virtual decimal GetShippingPrice(decimal price, bool includingTax, Customer customer, int taxCategoryId, out decimal taxRate)
		{
			taxRate = decimal.Zero;

			if (!_taxSettings.ShippingIsTaxable)
				return price;

			var result = GetProductPrice(
				null,
				taxCategoryId,
				price,
				includingTax,
				customer,
				_workContext.WorkingCurrency,
				_taxSettings.ShippingPriceIncludesTax,
				out taxRate);

			return result;
		}



		public virtual decimal GetPaymentMethodAdditionalFee(decimal price, Customer customer)
        {
            var includingTax = (_workContext.TaxDisplayType == TaxDisplayType.IncludingTax);
            return GetPaymentMethodAdditionalFee(price, includingTax, customer);
        }

        public virtual decimal GetPaymentMethodAdditionalFee(decimal price, bool includingTax, Customer customer)
        {
            var taxRate = decimal.Zero;
            return GetPaymentMethodAdditionalFee(price, includingTax, customer, out taxRate);
        }

		public virtual decimal GetPaymentMethodAdditionalFee(decimal price, bool includingTax, Customer customer, out decimal taxRate)
		{
			return GetPaymentMethodAdditionalFee(price, includingTax, customer, _taxSettings.PaymentMethodAdditionalFeeTaxClassId, out taxRate);
		}

		public virtual decimal GetPaymentMethodAdditionalFee(decimal price, bool includingTax, Customer customer, int taxCategoryId, out decimal taxRate)
        {
            taxRate = decimal.Zero;

            if (!_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
                return price;

            var result = GetProductPrice(
				null,
				taxCategoryId,
				price,
				includingTax,
				customer,
				_workContext.WorkingCurrency,
				_taxSettings.PaymentMethodAdditionalFeeIncludesTax,
				out taxRate);

			return result;
        }



        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <returns>Price</returns>
        public virtual decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav)
        {
            var customer = _workContext.CurrentCustomer;
            return GetCheckoutAttributePrice(cav, customer);
        }

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        public virtual decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav, Customer customer)
        {
            bool includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;
            return GetCheckoutAttributePrice(cav, includingTax, customer);
        }

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        public virtual decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav,
            bool includingTax, Customer customer)
        {
            decimal taxRate = decimal.Zero;
            return GetCheckoutAttributePrice(cav, includingTax, customer, out taxRate);
        }

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        public virtual decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav,
            bool includingTax, Customer customer, out decimal taxRate)
        {
            if (cav == null)
                throw new ArgumentNullException("cav");

            taxRate = decimal.Zero;

            var priceIncludesTax = _taxSettings.PricesIncludeTax;
			var taxClassId = cav.CheckoutAttribute.TaxCategoryId;
			var price = cav.PriceAdjustment;

            if (cav.CheckoutAttribute.IsTaxExempt)
            {
                return price;
            }

            return GetProductPrice(null, taxClassId, price, includingTax, customer, _workContext.WorkingCurrency, priceIncludesTax, out taxRate);
        }





        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="fullVatNumber">Two letter ISO code of a country and VAT number (e.g. GB 111 1111 111)</param>
        /// <returns>VAT Number status</returns>
        public virtual VatNumberStatus GetVatNumberStatus(string fullVatNumber)
        {
            string name, address;
            return GetVatNumberStatus(fullVatNumber, out name, out address);
        }

        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="fullVatNumber">Two letter ISO code of a country and VAT number (e.g. GB 111 1111 111)</param>
        /// <param name="name">Name (if received)</param>
        /// <param name="address">Address (if received)</param>
        /// <returns>VAT Number status</returns>
        public virtual VatNumberStatus GetVatNumberStatus(string fullVatNumber, out string name, out string address)
        {
            name = string.Empty;
            address = string.Empty;

            if (String.IsNullOrWhiteSpace(fullVatNumber))
                return VatNumberStatus.Empty;
            fullVatNumber = fullVatNumber.Trim();

            //GB 111 1111 111 or GB 1111111111
            //more advanced regex - http://codeigniter.com/wiki/European_Vat_Checker
            var r = new Regex(@"^(\w{2})(.*)");
            var match = r.Match(fullVatNumber);
            if (!match.Success)
                return VatNumberStatus.Invalid;
            var twoLetterIsoCode = match.Groups[1].Value;
            var vatNumber = match.Groups[2].Value;

            return GetVatNumberStatus(twoLetterIsoCode, vatNumber, out name, out address);
        }

        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <returns>VAT Number status</returns>
        public virtual VatNumberStatus GetVatNumberStatus(string twoLetterIsoCode, string vatNumber)
        {
            string name, address;
            return GetVatNumberStatus(twoLetterIsoCode, vatNumber, out name, out address);
        }

        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <param name="name">Name (if received)</param>
        /// <param name="address">Address (if received)</param>
        /// <returns>VAT Number status</returns>
        public virtual VatNumberStatus GetVatNumberStatus(string twoLetterIsoCode, string vatNumber,
            out string name, out string address)
        {
            name = string.Empty;
            address = string.Empty;

            if (String.IsNullOrEmpty(twoLetterIsoCode) || String.IsNullOrEmpty(vatNumber))
                return VatNumberStatus.Empty;

            if (!_taxSettings.EuVatUseWebService)
                return VatNumberStatus.Unknown;

            Exception exception = null;
            return DoVatCheck(twoLetterIsoCode, vatNumber, out name, out address, out exception);
        }

        /// <summary>
        /// Performs a basic check of a VAT number for validity
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <param name="name">Company name</param>
        /// <param name="address">Address</param>
        /// <param name="exception">Exception</param>
        /// <returns>VAT number status</returns>
        public virtual VatNumberStatus DoVatCheck(string twoLetterIsoCode, string vatNumber,
            out string name, out string address, out Exception exception)
        {
            name = string.Empty;
            address = string.Empty;

            if (vatNumber == null)
                vatNumber = string.Empty;
            vatNumber = vatNumber.Trim().Replace(" ", "");

            if (twoLetterIsoCode == null)
                twoLetterIsoCode = string.Empty;
            if (!String.IsNullOrEmpty(twoLetterIsoCode))
                //The service returns INVALID_INPUT for country codes that are not uppercase.
                twoLetterIsoCode = twoLetterIsoCode.ToUpper();

            EuropaCheckVatService.checkVatService s = null;

            try
            {
                bool valid;

                s = new EuropaCheckVatService.checkVatService();
                s.checkVat(ref twoLetterIsoCode, ref vatNumber, out valid, out name, out address);
                exception = null;
                return valid ? VatNumberStatus.Valid : VatNumberStatus.Invalid;
            }
            catch (Exception ex)
            {
                name = address = string.Empty;
                exception = ex;
                return VatNumberStatus.Unknown;
            }
            finally
            {
                if (name == null)
                    name = string.Empty;

                if (address == null)
                    address = string.Empty;

                if (s != null)
                    s.Dispose();
            }
        }


        /// <summary>
        /// Gets a value indicating whether tax exempt
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>A value indicating whether a product is tax exempt</returns>
        public virtual bool IsTaxExempt(Product product, Customer customer)
        {
            if (customer != null)
            {
                if (customer.IsTaxExempt)
                    return true;

                if (customer.CustomerRoles.Where(cr => cr.Active).Any(cr => cr.TaxExempt))
                    return true;
            }

            if (product == null)
            {
                return false;
            }

            if (product.IsTaxExempt)
            {
                return true;
            }

            return false;
        }

        public virtual bool IsVatExempt(Address address, Customer customer)
        {
            if (!_taxSettings.EuVatEnabled)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (address == null)
            {
                address = GetTaxAddress(customer);
            }

            if (address == null || address.Country == null)
            {
                return false;
            }

            if (!address.Country.SubjectToVat)
            {
                // VAT not chargeable if shipping outside VAT zone:
                return true;
            }
            else
            {
                // VAT not chargeable if address, customer and config meet our VAT exemption requirements:
                // returns true if this customer is VAT exempt because they are shipping within the EU but outside our shop country, 
                // they have supplied a validated VAT number, and the shop is configured to allow VAT exemption
                if (address.CountryId == _taxSettings.EuVatShopCountryId)
                    return false;

                var customerVatStatus = (VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId);
                return customerVatStatus == VatNumberStatus.Valid && _taxSettings.EuVatAllowVatExemption;
            }
        }

        #endregion
    }
}
