using System.Collections.Generic;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Directory
{
    /// <summary>
    /// Currency service
    /// </summary>
    public partial interface ICurrencyService
    {
        /// <summary>
        /// Gets currency live rates
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>Exchange rates</returns>
        IList<ExchangeRate> GetCurrencyLiveRates(string exchangeRateCurrencyCode);

        /// <summary>
        /// Deletes currency
        /// </summary>
        /// <param name="currency">Currency</param>
        void DeleteCurrency(Currency currency);

        /// <summary>
        /// Gets a currency
        /// </summary>
        /// <param name="currencyId">Currency identifier</param>
        /// <returns>Currency</returns>
        Currency GetCurrencyById(int currencyId);

        /// <summary>
        /// Gets a currency by code
        /// </summary>
        /// <param name="currencyCode">Currency code</param>
        /// <returns>Currency</returns>
        Currency GetCurrencyByCode(string currencyCode);

        /// <summary>
        /// Gets all currencies
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
		/// <returns>Currencies</returns>
		IList<Currency> GetAllCurrencies(bool showHidden = false, int storeId = 0);

        /// <summary>
        /// Inserts a currency
        /// </summary>
        /// <param name="currency">Currency</param>
        void InsertCurrency(Currency currency);

        /// <summary>
        /// Updates the currency
        /// </summary>
        /// <param name="currency">Currency</param>
        void UpdateCurrency(Currency currency);



        /// <summary>
        /// Converts currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="exchangeRate">Currency exchange rate</param>
        /// <returns>Converted value</returns>
        decimal ConvertCurrency(decimal amount, decimal exchangeRate);

        /// <summary>
        /// Converts currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="sourceCurrency">Source currency code</param>
        /// <param name="targetCurrency">Target currency code</param>
		/// <param name="store">Store to get the primary currencies from</param>
        /// <returns>Converted value</returns>
		decimal ConvertCurrency(decimal amount, Currency sourceCurrency, Currency targetCurrency, Store store = null);

        /// <summary>
        /// Converts to primary exchange rate currency 
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="sourceCurrencyCode">Source currency code</param>
		/// <param name="store">Store to get the primary exchange rate currency from</param>
        /// <returns>Converted value</returns>
		decimal ConvertToPrimaryExchangeRateCurrency(decimal amount, Currency sourceCurrencyCode, Store store = null);

        /// <summary>
        /// Converts from primary exchange rate currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="targetCurrency">Target currency code</param>
		/// <param name="store">Store to get the primary exchange rate currency from</param>
        /// <returns>Converted value</returns>
		decimal ConvertFromPrimaryExchangeRateCurrency(decimal amount, Currency targetCurrency, Store store = null);

        /// <summary>
        /// Converts to primary store currency 
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="sourceCurrency">Source currency code</param>
		/// <param name="store">Store to get the primary store currency from</param>
        /// <returns>Converted value</returns>
		decimal ConvertToPrimaryStoreCurrency(decimal amount, Currency sourceCurrency, Store store = null);

        /// <summary>
        /// Converts from primary store currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="targetCurrency">Target currency code</param>
		/// <param name="store">Store to get the primary store currency from</param>
        /// <returns>Converted value</returns>
		decimal ConvertFromPrimaryStoreCurrency(decimal amount, Currency targetCurrency, Store store = null);
       

        
        /// <summary>
        /// Load active exchange rate provider
        /// </summary>
        /// <returns>Active exchange rate provider</returns>
		Provider<IExchangeRateProvider> LoadActiveExchangeRateProvider();

        /// <summary>
        /// Load exchange rate provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found exchange rate provider</returns>
		Provider<IExchangeRateProvider> LoadExchangeRateProviderBySystemName(string systemName);

        /// <summary>
        /// Load all exchange rate providers
        /// </summary>
        /// <returns>Exchange rate providers</returns>
        IEnumerable<Provider<IExchangeRateProvider>> LoadAllExchangeRateProviders();
    }
}