using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Fakes;
using SmartStore.Services.Authentication;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework
{
    /// <summary>
	/// Work context for web application
    /// </summary>
    public partial class WebWorkContext : IWorkContext
    {
        private const string CustomerCookieName = "smartstore.customer";

        private readonly HttpContextBase _httpContext;
        private readonly ICustomerService _customerService;
		private readonly IStoreContext _storeContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
		private readonly IGenericAttributeService _genericAttributeService;
        private readonly TaxSettings _taxSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IWebHelper _webHelper;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreService _storeService;

        private Language _cachedLanguage;
        private Customer _cachedCustomer;
        private Customer _originalCustomerIfImpersonated; 

        public WebWorkContext(ICacheManager cacheManager,
            HttpContextBase httpContext,
            ICustomerService customerService,
			IStoreContext storeContext,
            IAuthenticationService authenticationService,
            ILanguageService languageService,
            ICurrencyService currencyService,
			IGenericAttributeService genericAttributeService,
            TaxSettings taxSettings, CurrencySettings currencySettings,
            LocalizationSettings localizationSettings,
            IWebHelper webHelper, IStoreService storeService)
        {
            this._cacheManager = cacheManager;
            this._httpContext = httpContext;
            this._customerService = customerService;
			this._storeContext = storeContext;
            this._authenticationService = authenticationService;
            this._languageService = languageService;
			this._genericAttributeService = genericAttributeService;
            this._currencyService = currencyService;
            this._taxSettings = taxSettings;
            this._currencySettings = currencySettings;
            this._localizationSettings = localizationSettings;
            this._webHelper = webHelper;
            this._storeService = storeService;
        }

        protected HttpCookie GetCustomerCookie()
        {
            if (_httpContext == null || _httpContext.Request == null)
                return null;

            return _httpContext.Request.Cookies[CustomerCookieName];
        }

        protected void SetCustomerCookie(Guid customerGuid)
        {
            if (_httpContext != null && _httpContext.Response != null)
            {
                var cookie = new HttpCookie(CustomerCookieName);
                cookie.HttpOnly = true;
                cookie.Value = customerGuid.ToString();
                if (customerGuid == Guid.Empty)
                {
                    cookie.Expires = DateTime.Now.AddMonths(-1);
                }
                else
                {
                    int cookieExpires = 24 * 365; //TODO make configurable
                    cookie.Expires = DateTime.Now.AddHours(cookieExpires);
                }

                _httpContext.Response.Cookies.Remove(CustomerCookieName);
                _httpContext.Response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// Gets or sets the current customer
        /// </summary>
        public Customer CurrentCustomer
        {
            get
            {
                if (_cachedCustomer != null)
                    return _cachedCustomer;

                Customer customer = null;
                if (_httpContext == null || _httpContext is FakeHttpContext)
                {
                    //check whether request is made by a background task
                    //in this case return built-in customer record for background task
                    customer = _customerService.GetCustomerBySystemName(SystemCustomerNames.BackgroundTask);
                }

                //check whether request is made by a search engine
                //in this case return built-in customer record for search engines 
                //or comment the following two lines of code in order to disable this functionality
                if (customer == null || customer.Deleted || !customer.Active)
                {
                    if (_webHelper.IsSearchEngine(_httpContext))
                        customer = _customerService.GetCustomerBySystemName(SystemCustomerNames.SearchEngine);
                }

                //registered user
                if (customer == null || customer.Deleted || !customer.Active)
                {
                    customer = _authenticationService.GetAuthenticatedCustomer();
                }

                //impersonate user if required (currently used for 'phone order' support)
                if (customer != null && !customer.Deleted && customer.Active)
                {
                    int? impersonatedCustomerId = customer.GetAttribute<int?>(SystemCustomerAttributeNames.ImpersonatedCustomerId);
                    if (impersonatedCustomerId.HasValue && impersonatedCustomerId.Value > 0)
                    {
                        var impersonatedCustomer = _customerService.GetCustomerById(impersonatedCustomerId.Value);
                        if (impersonatedCustomer != null && !impersonatedCustomer.Deleted && impersonatedCustomer.Active)
                        {
                            //set impersonated customer
                            _originalCustomerIfImpersonated = customer;
                            customer = impersonatedCustomer;
                        }
                    }
                }

                //load guest customer
                if (customer == null || customer.Deleted || !customer.Active)
                {
                    var customerCookie = GetCustomerCookie();
                    if (customerCookie != null && !String.IsNullOrEmpty(customerCookie.Value))
                    {
                        Guid customerGuid;
                        if (Guid.TryParse(customerCookie.Value, out customerGuid))
                        {
                            var customerByCookie = _customerService.GetCustomerByGuid(customerGuid);
                            if (customerByCookie != null &&
                                //this customer (from cookie) should not be registered
                                !customerByCookie.IsRegistered() &&
								//it should not be a built-in 'search engine' customer account
								!customerByCookie.IsSearchEngineAccount())
                                customer = customerByCookie;
                        }
                    }
                }

                //create guest if not exists
                if (customer == null || customer.Deleted || !customer.Active)
                {
                    customer = _customerService.InsertGuestCustomer();
                }


                //validation
                if (!customer.Deleted && customer.Active)
                {
                    SetCustomerCookie(customer.CustomerGuid);
                    _cachedCustomer = customer;
                }

                return _cachedCustomer;
            }
            set
            {
                SetCustomerCookie(value.CustomerGuid);
                _cachedCustomer = value;
            }
        }

        /// <summary>
        /// Gets or sets the original customer (in case the current one is impersonated)
        /// </summary>
        public Customer OriginalCustomerIfImpersonated
        {
            get
            {
                return _originalCustomerIfImpersonated;
            }
        }

        /// <summary>
        /// Get or set current user working language
        /// </summary>
        public Language WorkingLanguage
        {
            get
            {
                if (_cachedLanguage != null)
                    return _cachedLanguage;
                
                int storeId = _storeContext.CurrentStore.Id;
                int customerLangId = 0;

                if (this.CurrentCustomer != null)
                {
                    customerLangId = this.CurrentCustomer.GetAttribute<int>(
                        SystemCustomerAttributeNames.LanguageId, 
                        _genericAttributeService, 
                        _storeContext.CurrentStore.Id);
                }

                #region Get language from URL (if possible)
                if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled && _httpContext != null)
                {
                    var helper = new LocalizedUrlHelper(_httpContext.Request, false);
                    string seoCode;
                    if (helper.IsLocalizedUrl(out seoCode))
                    {
                        if (this.IsPublishedLanguage(seoCode, storeId))
                        {
                            // the language is found. now we need to save it
                            var langBySeoCode = _languageService.GetLanguageBySeoCode(seoCode);
                            if (this.CurrentCustomer != null && customerLangId != langBySeoCode.Id)
                            {
                                customerLangId = langBySeoCode.Id;
                                this.SetCustomerLanguage(langBySeoCode.Id, storeId);
                            }
                            _cachedLanguage = langBySeoCode;
                            return langBySeoCode;
                        }
                    }
                }
                #endregion

                if (_localizationSettings.DetectBrowserUserLanguage && (customerLangId == 0 || !this.IsPublishedLanguage(customerLangId, storeId)))
                {
                    #region Get Browser UserLanguage

                    // Fallback to browser detected language
                    Language browserLanguage = null;

                    if (_httpContext != null && _httpContext.Request != null && _httpContext.Request.UserLanguages != null)
                    {
                        var userLangs = _httpContext.Request.UserLanguages.Select(x => x.Split(new[] { ';' }, 2, StringSplitOptions.RemoveEmptyEntries)[0]);
                        if (userLangs.HasItems())
                        {
                            foreach (var culture in userLangs)
                            {
                                browserLanguage = _languageService.GetLanguageByCulture(culture);
                                if (this.IsPublishedLanguage(browserLanguage.Id, storeId))
                                {
                                    // the language is found. now we need to save it
                                    if (this.CurrentCustomer != null && customerLangId != browserLanguage.Id)
                                    {
                                        customerLangId = browserLanguage.Id;
                                        SetCustomerLanguage(customerLangId, storeId);
                                    }
                                    _cachedLanguage = browserLanguage;
                                    return browserLanguage;
                                }
                            }
                        }
                    }

                    #endregion
                }

                if (customerLangId > 0 && this.IsPublishedLanguage(customerLangId, storeId))
                {
                    _cachedLanguage = _languageService.GetLanguageById(customerLangId);
                    return _cachedLanguage;
                }
                
                // Fallback
                customerLangId = this.GetDefaultLanguageId(storeId);
                SetCustomerLanguage(customerLangId, storeId);

                _cachedLanguage = _languageService.GetLanguageById(customerLangId);
                return _cachedLanguage;
            }
            set
            {
				var languageId = value != null ? value.Id : 0;
                this.SetCustomerLanguage(languageId, _storeContext.CurrentStore.Id);
                _cachedLanguage = null;
			}
        }

        private void SetCustomerLanguage(int languageId, int storeId)
        {
            _genericAttributeService.SaveAttribute(
                this.CurrentCustomer,
                SystemCustomerAttributeNames.LanguageId,
                languageId,
                storeId);
        }

        /// <summary>
        /// Get or set current user working currency
        /// </summary>
        public Currency WorkingCurrency
        {
            get
            {
                Currency primaryStoreCurrency = null;
                // return primary store currency when we're in admin area/mode
                if (this.IsAdmin)
                {
                    primaryStoreCurrency =  _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                    if (primaryStoreCurrency != null)
                        return primaryStoreCurrency;
                }

				var allStoreCurrencies = _currencyService.GetAllCurrencies(storeId: _storeContext.CurrentStore.Id);
				if (allStoreCurrencies.Count > 0)
				{
					// find current customer language
					var customerCurrencyId = this.CurrentCustomer.GetAttribute<int>(
                        SystemCustomerAttributeNames.CurrencyId,
						_genericAttributeService, 
                        _storeContext.CurrentStore.Id);
					foreach (var currency in allStoreCurrencies)
					{
						if (customerCurrencyId == currency.Id)
						{
							return currency;
						}
					}
				}

                // codehint: sm-edit
                primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                if (primaryStoreCurrency != null && primaryStoreCurrency.Published)
                {
                    return primaryStoreCurrency;
                }

				if (allStoreCurrencies.Count > 0)
					return allStoreCurrencies.FirstOrDefault();

				// if not found in currencies filtered by the current store, then return any currency
                return _currencyService.GetAllCurrencies().FirstOrDefault();
            }
            set
            {
				var currencyId = value != null ? value.Id : 0;
				_genericAttributeService.SaveAttribute(this.CurrentCustomer,
					SystemCustomerAttributeNames.CurrencyId,
					currencyId, _storeContext.CurrentStore.Id);
			}
        }

        /// <summary>
        /// Get or set current tax display type
        /// </summary>
        public TaxDisplayType TaxDisplayType
        {
            get
            {
				return GetTaxDisplayTypeFor(this.CurrentCustomer, _storeContext.CurrentStore.Id);
            }
            set
            {
                if (!_taxSettings.AllowCustomersToSelectTaxDisplayType)
                    return;

				_genericAttributeService.SaveAttribute(this.CurrentCustomer,
					 SystemCustomerAttributeNames.TaxDisplayTypeId,
					 (int)value, _storeContext.CurrentStore.Id);
            }
        }

        public TaxDisplayType GetTaxDisplayTypeFor(Customer customer, int storeId)
        {
            if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
            {
                if (customer != null)
                {
					int? taxDisplayType = customer.GetAttribute<int?>(SystemCustomerAttributeNames.TaxDisplayTypeId, storeId);

					if (taxDisplayType.HasValue)
						return (TaxDisplayType)taxDisplayType.Value;
                }
            }

            var customerRoles = customer.CustomerRoles;
            string key = string.Format(FrameworkCacheConsumer.CUSTOMERROLES_TAX_DISPLAY_TYPES_KEY, String.Join(",", customerRoles.Select(x => x.Id)), storeId);
            return _cacheManager.Get(key, () =>
            {
                var roleTaxDisplayTypes = customerRoles
                    .Where(x => x.TaxDisplayType.HasValue)
                    .OrderByDescending(x => x.TaxDisplayType.Value)
                    .Select(x => x.TaxDisplayType.Value);

                if (roleTaxDisplayTypes.Any())
                {
                    return (TaxDisplayType)roleTaxDisplayTypes.FirstOrDefault();
                }

                return _taxSettings.TaxDisplayType;
            });
        }

		/// <summary>
		/// Get or set value indicating whether we're in admin area
		/// </summary>
		public bool IsAdmin { get; set; }

        public bool IsPublishedLanguage(string seoCode, int storeId = 0)
        {
            if (storeId <= 0)
                storeId = _storeContext.CurrentStore.Id;
            
            var map = this.GetStoreLanguageMap();
            if (map.ContainsKey(storeId)) 
            {
                return map[storeId].Any(x => x.Item2 == seoCode);
            }

            return false;
        }

        internal bool IsPublishedLanguage(int languageId, int storeId = 0)
        {
            if (languageId <= 0)
                return false;

            if (storeId <= 0)
                storeId = _storeContext.CurrentStore.Id;

            var map = this.GetStoreLanguageMap();
            if (map.ContainsKey(storeId))
            {
                return map[storeId].Any(x => x.Item1 == languageId);
            }

            return false;
        }

        public string GetDefaultLanguageSeoCode(int storeId = 0)
        {
            if (storeId <= 0)
                storeId = _storeContext.CurrentStore.Id;

            var map = this.GetStoreLanguageMap();
            if (map.ContainsKey(storeId))
            {
                return map[storeId].FirstOrDefault().Item2;
            }

            return null;
        }

        internal int GetDefaultLanguageId(int storeId = 0)
        {
            if (storeId <= 0)
                storeId = _storeContext.CurrentStore.Id;

            var map = this.GetStoreLanguageMap();
            if (map.ContainsKey(storeId))
            {
                return map[storeId].FirstOrDefault().Item1;
            }

            return 0;
        }

        /// <summary>
        /// Gets a map of active/published store languages
        /// </summary>
        /// <returns>A map of store languages where key is the store id and values are tuples of lnguage ids and seo codes</returns>
        protected virtual Multimap<int, Tuple<int, string>> GetStoreLanguageMap()
        {
            var result = _cacheManager.Get(FrameworkCacheConsumer.STORE_LANGUAGE_MAP_KEY, () => {
                var map = new Multimap<int, Tuple<int, string>>();

                var allStores = _storeService.GetAllStores();
                foreach (var store in allStores)
                {
                    var languages = _languageService.GetAllLanguages(false, store.Id);
                    if (!languages.Any())
                    {
                        // language-less stores aren't allowed but could exist accidentally. Correct this.
                        var firstStoreLang = _languageService.GetAllLanguages(true, store.Id).FirstOrDefault();
                        if (firstStoreLang == null)
                        {
                            // absolute fallback
                            firstStoreLang = _languageService.GetAllLanguages(true).FirstOrDefault();
                        }
                        map.Add(store.Id, new Tuple<int, string>(firstStoreLang.Id, firstStoreLang.UniqueSeoCode));
                    }
                    else
                    {
                        foreach (var lang in languages)
                        {
                            map.Add(store.Id, new Tuple<int, string>(lang.Id, lang.UniqueSeoCode));
                        }
                    }
                }

                return map;
            }, 1440 /* 24 hrs */);

            return result;
        }

    }
}
