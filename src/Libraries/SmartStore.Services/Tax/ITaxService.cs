using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Plugins;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Services.Tax
{
    /// <summary>
    /// Tax service
    /// </summary>
    public partial interface ITaxService
    {
        /// <summary>
        /// Load active tax provider
        /// </summary>
        /// <returns>Active tax provider</returns>
        Provider<ITaxProvider> LoadActiveTaxProvider();

        /// <summary>
        /// Load tax provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found tax provider</returns>
        Provider<ITaxProvider> LoadTaxProviderBySystemName(string systemName);

        /// <summary>
        /// Load all tax providers
        /// </summary>
        /// <returns>Tax providers</returns>
        IEnumerable<Provider<ITaxProvider>> LoadAllTaxProviders();
        



        /// <summary>
        /// Gets tax rate
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Tax rate</returns>
        decimal GetTaxRate(Product product, Customer customer);

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>Tax rate</returns>
        decimal GetTaxRate(int taxCategoryId, Customer customer);
        
        /// <summary>
        /// Gets tax rate
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="taxCategoryId">Tax category identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>Tax rate</returns>
        decimal GetTaxRate(Product product, int taxCategoryId,  Customer customer);
        



        /// <summary>
        /// Gets price
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        decimal GetProductPrice(Product product, decimal price, out decimal taxRate);

        /// <summary>
        /// Gets price
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="price">Price</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        decimal GetProductPrice(Product product, decimal price, Customer customer, out decimal taxRate);

		/// <summary>
		/// Gets price
		/// </summary>
		/// <param name="product">Product</param>
		/// <param name="price">Price</param>
		/// <param name="customer">Customer</param>
		/// <param name="currency">Currency</param>
		/// <param name="taxRate">Tax rate</param>
		/// <returns>Price</returns>
		decimal GetProductPrice(Product product, decimal price, Customer customer, Currency currency, out decimal taxRate);

		/// <summary>
		/// Gets price
		/// </summary>
		/// <param name="product">Product</param>
		/// <param name="price">Price</param>
		/// <param name="includingTax">A value indicating whether calculated price should include tax</param>
		/// <param name="customer">Customer</param>
		/// <param name="taxRate">Tax rate</param>
		/// <returns>Price</returns>
		decimal GetProductPrice(Product product, decimal price, bool includingTax, Customer customer, out decimal taxRate);

		/// <summary>
		/// Gets price
		/// </summary>
		/// <param name="product">Product</param>
		/// <param name="taxCategoryId">Tax category identifier</param>
		/// <param name="price">Price</param>
		/// <param name="includingTax">A value indicating whether calculated price should include tax</param>
		/// <param name="customer">Customer</param>
		/// <param name="currency">Currency</param>
		/// <param name="priceIncludesTax">A value indicating whether price already includes tax</param>
		/// <param name="taxRate">Tax rate</param>
		/// <returns>Price</returns>
		decimal GetProductPrice(Product product,
			int taxCategoryId,
			decimal price,
            bool includingTax,
			Customer customer,
			Currency currency,
			bool priceIncludesTax,
			out decimal taxRate);




		/// <summary>
		/// Gets the shipping price
		/// </summary>
		/// <param name="price">Price</param>
		/// <param name="customer">Customer</param>
		/// <returns>Shipping price</returns>
		decimal GetShippingPrice(decimal price, Customer customer);

		/// <summary>
		/// Gets the shipping price
		/// </summary>
		/// <param name="price">Price</param>
		/// <param name="includingTax">A value indicating whether calculated price should include tax</param>
		/// <param name="customer">Customer</param>
		/// <returns>Shipping price</returns>
		decimal GetShippingPrice(decimal price, bool includingTax, Customer customer);

		/// <summary>
		/// Gets the shipping price
		/// </summary>
		/// <param name="price">Price</param>
		/// <param name="includingTax">A value indicating whether calculated price should include tax</param>
		/// <param name="customer">Customer</param>
		/// <param name="taxRate">Tax rate</param>
		/// <returns>Shipping price</returns>
		decimal GetShippingPrice(decimal price, bool includingTax, Customer customer, out decimal taxRate);

		/// <summary>
		/// Gets the shipping price
		/// </summary>
		/// <param name="price">Price</param>
		/// <param name="includingTax">A value indicating whether calculated price should include tax</param>
		/// <param name="customer">Customer</param>
		/// <param name="taxCategoryId">Tax category id</param>
		/// <param name="taxRate">Tax rate</param>
		/// <returns>Shipping price</returns>
		decimal GetShippingPrice(decimal price, bool includingTax, Customer customer, int taxCategoryId, out decimal taxRate);




		/// <summary>
		/// Gets payment method additional handling fee
		/// </summary>
		/// <param name="price">Price</param>
		/// <param name="customer">Customer</param>
		/// <returns>Payment fee</returns>
		decimal GetPaymentMethodAdditionalFee(decimal price, Customer customer);

		/// <summary>
		/// Gets payment method additional handling fee
		/// </summary>
		/// <param name="price">Price</param>
		/// <param name="includingTax">A value indicating whether calculated price should include tax</param>
		/// <param name="customer">Customer</param>
		/// <returns>Payment fee</returns>
		decimal GetPaymentMethodAdditionalFee(decimal price, bool includingTax, Customer customer);

		/// <summary>
		/// Gets payment method additional handling fee
		/// </summary>
		/// <param name="price">Price</param>
		/// <param name="includingTax">A value indicating whether calculated price should include tax</param>
		/// <param name="customer">Customer</param>
		/// <param name="taxRate">Tax rate</param>
		/// <returns>Payment fee</returns>
		decimal GetPaymentMethodAdditionalFee(decimal price, bool includingTax, Customer customer, out decimal taxRate);

		/// <summary>
		/// Gets payment method additional handling fee
		/// </summary>
		/// <param name="price">Price</param>
		/// <param name="includingTax">A value indicating whether calculated price should include tax</param>
		/// <param name="customer">Customer</param>
		/// <param name="taxCategoryId">Tax category id</param>
		/// <param name="taxRate">Tax rate</param>
		/// <returns>Payment fee</returns>
		decimal GetPaymentMethodAdditionalFee(decimal price, bool includingTax, Customer customer, int taxCategoryId, out decimal taxRate);




		/// <summary>
		/// Gets checkout attribute value price
		/// </summary>
		/// <param name="cav">Checkout attribute value</param>
		/// <returns>Price</returns>
		decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav);

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav, Customer customer);

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <returns>Price</returns>
        decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav,
            bool includingTax, Customer customer);

        /// <summary>
        /// Gets checkout attribute value price
        /// </summary>
        /// <param name="cav">Checkout attribute value</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="customer">Customer</param>
        /// <param name="taxRate">Tax rate</param>
        /// <returns>Price</returns>
        decimal GetCheckoutAttributePrice(CheckoutAttributeValue cav,
            bool includingTax, Customer customer, out decimal taxRate);




        

        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="fullVatNumber">Two letter ISO code of a country and VAT number (e.g. GB 111 1111 111)</param>
        /// <returns>VAT Number status</returns>
        VatNumberStatus GetVatNumberStatus(string fullVatNumber);

        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="fullVatNumber">Two letter ISO code of a country and VAT number (e.g. GB 111 1111 111)</param>
        /// <param name="name">Name (if received)</param>
        /// <param name="address">Address (if received)</param>
        /// <returns>VAT Number status</returns>
        VatNumberStatus GetVatNumberStatus(string fullVatNumber,
            out string name, out string address);
        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <returns>VAT Number status</returns>
        VatNumberStatus GetVatNumberStatus(string twoLetterIsoCode, string vatNumber);
        
        /// <summary>
        /// Gets VAT Number status
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <param name="name">Name (if received)</param>
        /// <param name="address">Address (if received)</param>
        /// <returns>VAT Number status</returns>
        VatNumberStatus GetVatNumberStatus(string twoLetterIsoCode, string vatNumber, 
            out string name, out string address);

        /// <summary>
        /// Performs a basic check of a VAT number for validity
        /// </summary>
        /// <param name="twoLetterIsoCode">Two letter ISO code of a country</param>
        /// <param name="vatNumber">VAT number</param>
        /// <param name="name">Company name</param>
        /// <param name="address">Address</param>
        /// <param name="exception">Exception</param>
        /// <returns>VAT number status</returns>
        VatNumberStatus DoVatCheck(string twoLetterIsoCode, string vatNumber, 
            out string name, out string address, out Exception exception);



        /// <summary>
        /// Gets a value indicating whether tax exempt
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
		/// <returns>A value indicating whether a product is tax exempt</returns>
        bool IsTaxExempt(Product product, Customer customer);

        /// <summary>
        /// Gets a value indicating whether EU VAT exempt (the European Union Value Added Tax)
        /// </summary>
        /// <param name="address">Address</param>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        bool IsVatExempt(Address address, Customer customer);
    }
}
