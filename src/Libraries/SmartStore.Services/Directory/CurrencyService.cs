using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Directory
{
    /// <summary>
    /// Currency service
    /// </summary>
    public partial class CurrencyService : ICurrencyService
    {
        #region Constants
        private const string CURRENCIES_ALL_KEY = "SmartStore.currency.all-{0}";
        private const string CURRENCIES_PATTERN_KEY = "SmartStore.currency.";
        private const string CURRENCIES_BY_ID_KEY = "SmartStore.currency.id-{0}";
        #endregion

        #region Fields

        private readonly IRepository<Currency> _currencyRepository;
		private readonly IStoreMappingService _storeMappingService;
        private readonly ICacheManager _cacheManager;
        private readonly CurrencySettings _currencySettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly IEventPublisher _eventPublisher;
		private readonly IProviderManager _providerManager;
		private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Cache manager</param>
        /// <param name="currencyRepository">Currency repository</param>
		/// <param name="storeMappingRepository">Store mapping repository</param>
        /// <param name="currencySettings">Currency settings</param>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="eventPublisher">Event published</param>
        public CurrencyService(ICacheManager cacheManager,
            IRepository<Currency> currencyRepository,
			IStoreMappingService storeMappingService,
            CurrencySettings currencySettings,
            IPluginFinder pluginFinder,
            IEventPublisher eventPublisher,
			IProviderManager providerManager,
			IStoreContext storeContext)
        {
            this._cacheManager = cacheManager;
            this._currencyRepository = currencyRepository;
			this._storeMappingService = storeMappingService;
            this._currencySettings = currencySettings;
            this._pluginFinder = pluginFinder;
            this._eventPublisher = eventPublisher;
			this._providerManager = providerManager;
			this._storeContext = storeContext;
        }

        #endregion
        
        #region Methods

        /// <summary>
        /// Gets currency live rates
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>Exchange rates</returns>
        public virtual IList<ExchangeRate> GetCurrencyLiveRates(string exchangeRateCurrencyCode)
        {
            var exchangeRateProvider = LoadActiveExchangeRateProvider();
			if (exchangeRateProvider != null)
			{
				return exchangeRateProvider.Value.GetCurrencyLiveRates(exchangeRateCurrencyCode);
			}
			return new List<ExchangeRate>();
        }

        /// <summary>
        /// Deletes currency
        /// </summary>
        /// <param name="currency">Currency</param>
        public virtual void DeleteCurrency(Currency currency)
        {
            if (currency == null)
                throw new ArgumentNullException("currency");
            
            _currencyRepository.Delete(currency);

            _cacheManager.RemoveByPattern(CURRENCIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(currency);
        }

        /// <summary>
        /// Gets a currency
        /// </summary>
        /// <param name="currencyId">Currency identifier</param>
        /// <returns>Currency</returns>
        public virtual Currency GetCurrencyById(int currencyId)
        {
            if (currencyId == 0)
                return null;

            string key = string.Format(CURRENCIES_BY_ID_KEY, currencyId);
            return _cacheManager.Get(key, () => _currencyRepository.GetById(currencyId));
        }

        /// <summary>
        /// Gets a currency by code
        /// </summary>
        /// <param name="currencyCode">Currency code</param>
        /// <returns>Currency</returns>
        public virtual Currency GetCurrencyByCode(string currencyCode)
        {
            if (String.IsNullOrEmpty(currencyCode))
                return null;
            return GetAllCurrencies(true).FirstOrDefault(c => c.CurrencyCode.ToLower() == currencyCode.ToLower());
        }

        /// <summary>
        /// Gets all currencies
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Currency collection</returns>
		public virtual IList<Currency> GetAllCurrencies(bool showHidden = false, int storeId = 0)
        {
			string key = string.Format(CURRENCIES_ALL_KEY, showHidden);
			var currencies = _cacheManager.Get(key, () =>
			{
				var query = _currencyRepository.Table;
				if (!showHidden)
					query = query.Where(c => c.Published);
				query = query.OrderBy(c => c.DisplayOrder);
				return query.ToList();
			});

			//store mapping
			if (storeId > 0)
			{
				currencies = currencies
					.Where(c => _storeMappingService.Authorize(c, storeId))
					.ToList();
			}
			return currencies;
        }

        /// <summary>
        /// Inserts a currency
        /// </summary>
        /// <param name="currency">Currency</param>
        public virtual void InsertCurrency(Currency currency)
        {
            if (currency == null)
                throw new ArgumentNullException("currency");

            _currencyRepository.Insert(currency);

            _cacheManager.RemoveByPattern(CURRENCIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(currency);
        }

        /// <summary>
        /// Updates the currency
        /// </summary>
        /// <param name="currency">Currency</param>
        public virtual void UpdateCurrency(Currency currency)
        {
            if (currency == null)
                throw new ArgumentNullException("currency");

            _currencyRepository.Update(currency);

            _cacheManager.RemoveByPattern(CURRENCIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(currency);
        }


        /// <summary>
        /// Converts currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="exchangeRate">Currency exchange rate</param>
        /// <returns>Converted value</returns>
        public virtual decimal ConvertCurrency(decimal amount, decimal exchangeRate)
        {
            if (amount != decimal.Zero && exchangeRate != decimal.Zero)
                return amount * exchangeRate;
            return decimal.Zero;
        }

        /// <summary>
        /// Converts currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="sourceCurrencyCode">Source currency code</param>
        /// <param name="targetCurrencyCode">Target currency code</param>
		/// <param name="store">Store to get the primary currencies from</param>
        /// <returns>Converted value</returns>
		public virtual decimal ConvertCurrency(decimal amount, Currency sourceCurrencyCode, Currency targetCurrencyCode, Store store = null)
        {
            decimal result = amount;
            if (sourceCurrencyCode.Id == targetCurrencyCode.Id)
                return result;
            if (result != decimal.Zero && sourceCurrencyCode.Id != targetCurrencyCode.Id)
            {
                result = ConvertToPrimaryExchangeRateCurrency(result, sourceCurrencyCode, store);
                result = ConvertFromPrimaryExchangeRateCurrency(result, targetCurrencyCode, store);
            }
            return result;
        }

        /// <summary>
        /// Converts to primary exchange rate currency 
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="sourceCurrencyCode">Source currency code</param>
		/// <param name="store">Store to get the primary exchange rate currency from</param>
        /// <returns>Converted value</returns>
		public virtual decimal ConvertToPrimaryExchangeRateCurrency(decimal amount, Currency sourceCurrencyCode, Store store = null)
        {
            decimal result = amount;
			var primaryExchangeRateCurrency = (store == null ? _storeContext.CurrentStore.PrimaryExchangeRateCurrency : store.PrimaryExchangeRateCurrency);

            if (result != decimal.Zero && sourceCurrencyCode.Id != primaryExchangeRateCurrency.Id)
            {
                decimal exchangeRate = sourceCurrencyCode.Rate;
                if (exchangeRate == decimal.Zero)
                    throw new SmartException(string.Format("Exchange rate not found for currency [{0}]", sourceCurrencyCode.Name));
                result = result / exchangeRate;
            }
            return result;
        }

        /// <summary>
        /// Converts from primary exchange rate currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="targetCurrencyCode">Target currency code</param>
		/// <param name="store">Store to get the primary exchange rate currency from</param>
        /// <returns>Converted value</returns>
		public virtual decimal ConvertFromPrimaryExchangeRateCurrency(decimal amount, Currency targetCurrencyCode, Store store = null)
        {
            decimal result = amount;
            var primaryExchangeRateCurrency = (store == null ? _storeContext.CurrentStore.PrimaryExchangeRateCurrency : store.PrimaryExchangeRateCurrency);

            if (result != decimal.Zero && targetCurrencyCode.Id != primaryExchangeRateCurrency.Id)
            {
                decimal exchangeRate = targetCurrencyCode.Rate;
                if (exchangeRate == decimal.Zero)
                    throw new SmartException(string.Format("Exchange rate not found for currency [{0}]", targetCurrencyCode.Name));
                result = result * exchangeRate;
            }
            return result;
        }

        /// <summary>
        /// Converts to primary store currency 
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="sourceCurrencyCode">Source currency code</param>
		/// <param name="store">Store to get the primary store currency from</param>
        /// <returns>Converted value</returns>
        public virtual decimal ConvertToPrimaryStoreCurrency(decimal amount, Currency sourceCurrencyCode, Store store = null)
        {
            decimal result = amount;
			var primaryStoreCurrency = (store == null ? _storeContext.CurrentStore.PrimaryStoreCurrency : store.PrimaryStoreCurrency);

            if (result != decimal.Zero && sourceCurrencyCode.Id != primaryStoreCurrency.Id)
            {
                decimal exchangeRate = sourceCurrencyCode.Rate;
                if (exchangeRate == decimal.Zero)
                    throw new SmartException(string.Format("Exchange rate not found for currency [{0}]", sourceCurrencyCode.Name));
                result = result / exchangeRate;
            }
            return result;
        }

        /// <summary>
        /// Converts from primary store currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="targetCurrencyCode">Target currency code</param>
        /// <returns>Converted value</returns>
		public virtual decimal ConvertFromPrimaryStoreCurrency(decimal amount, Currency targetCurrencyCode, Store store = null)
        {
            decimal result = amount;
			var primaryStoreCurrency = (store == null ? _storeContext.CurrentStore.PrimaryStoreCurrency : store.PrimaryStoreCurrency);
            result = ConvertCurrency(amount, primaryStoreCurrency, targetCurrencyCode, store);
            return result;
        }
       

        /// <summary>
        /// Load active exchange rate provider
        /// </summary>
        /// <returns>Active exchange rate provider</returns>
        public virtual Provider<IExchangeRateProvider> LoadActiveExchangeRateProvider()
        {
			return LoadExchangeRateProviderBySystemName(_currencySettings.ActiveExchangeRateProviderSystemName) ?? LoadAllExchangeRateProviders().FirstOrDefault();
        }

        /// <summary>
        /// Load exchange rate provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found exchange rate provider</returns>
        public virtual Provider<IExchangeRateProvider> LoadExchangeRateProviderBySystemName(string systemName)
        {
			return _providerManager.GetProvider<IExchangeRateProvider>(systemName);
        }

        /// <summary>
        /// Load all exchange rate providers
        /// </summary>
        /// <returns>Exchange rate providers</returns>
        public virtual IEnumerable<Provider<IExchangeRateProvider>> LoadAllExchangeRateProviders()
        {
			return _providerManager.GetAllProviders<IExchangeRateProvider>();
        }
        #endregion
    }
}