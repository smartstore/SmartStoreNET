using System;
using System.Linq;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
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
    /// Working context for web application
    /// </summary>
    public partial class WebWorkContext : IWorkContext
    {
        private const string CustomerCookieName = "smartstore.customer";

        private readonly HttpContextBase _httpContext;
        private readonly ICustomerService _customerService;
		private readonly IStoreService _storeService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly TaxSettings _taxSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IWebHelper _webHelper;
        private readonly ICacheManager _cacheManager;

		private Store _cachedStore;
		private bool _storeIsLoaded;

        private Customer _cachedCustomer;
        private Customer _originalCustomerIfImpersonated;
        private bool _cachedIsAdmin;

        public WebWorkContext(ICacheManager cacheManager,
            HttpContextBase httpContext,
            ICustomerService customerService,
			IStoreService storeService,
            IAuthenticationService authenticationService,
            ILanguageService languageService,
            ICurrencyService currencyService,
            TaxSettings taxSettings, CurrencySettings currencySettings,
            LocalizationSettings localizationSettings,
            IWebHelper webHelper)
        {
            this._cacheManager = cacheManager;
            this._httpContext = httpContext;
            this._customerService = customerService;
			this._storeService = storeService;
            this._authenticationService = authenticationService;
            this._languageService = languageService;
            this._currencyService = currencyService;
            this._taxSettings = taxSettings;
            this._currencySettings = currencySettings;
            this._localizationSettings = localizationSettings;
            this._webHelper = webHelper;
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
		/// Gets or sets the current store
		/// </summary>
		public Store CurrentStore
		{
			get
			{
				if (_storeIsLoaded)
					return _cachedStore;

				Store store = null;
				if (_httpContext != null)
				{
					//TODO determine the current store by HTTP_HOST
				}

				if (store == null)
				{
					//load the first found store
					store = _storeService.GetAllStores().FirstOrDefault();
				}

				_storeIsLoaded = true;
				_cachedStore = store;
				return _cachedStore;
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
                                !customerByCookie.IsRegistered())
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
                //get language from URL (if possible)
                if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    if (_httpContext != null)
                    {
                        string virtualPath = _httpContext.Request.AppRelativeCurrentExecutionFilePath;
                        string applicationPath = _httpContext.Request.ApplicationPath;
                        Language langByCulture;
                        if (virtualPath.IsLocalizedUrl(applicationPath, false))
                        {
                            var seoCode = virtualPath.GetLanguageSeoCodeFromUrl(applicationPath, false);
                            if (!String.IsNullOrEmpty(seoCode))
                            {
                                //langByCulture = _languageService.GetAllLanguages()
                                //    .Where(l => seoCode.Equals(l.UniqueSeoCode, StringComparison.InvariantCultureIgnoreCase))
                                //    .FirstOrDefault();
                                langByCulture = _languageService.GetLanguageByCulture(seoCode); // codehint: sm-edit
                                if (langByCulture != null && langByCulture.Published)
                                {
                                    //the language is found. now we need to save it
                                    if (this.CurrentCustomer != null &&
                                        !langByCulture.Equals(this.CurrentCustomer.Language))
                                    {
                                        this.CurrentCustomer.Language = langByCulture;
                                        _customerService.UpdateCustomer(this.CurrentCustomer);
                                    }
                                }
                            }
                        }
                    }
                }
                if (this.CurrentCustomer != null && this.CurrentCustomer.Language != null && this.CurrentCustomer.Language.Published)
                {
                    return this.CurrentCustomer.Language;
                }

                // codehint: sm-add
                // Fallback to browser detected language
                Language lang = null;

				if (_httpContext != null && _httpContext.Request != null && _httpContext.Request.UserLanguages != null)
                {
                    var userLangs = _httpContext.Request.UserLanguages.Select(x => x.Split(new[] { ';' }, 2, StringSplitOptions.RemoveEmptyEntries)[0]);
                    if (userLangs.HasItems())
                    {
                        foreach (var culture in userLangs)
                        {
                            lang = _languageService.GetLanguageByCulture(culture);
                            if (lang != null && lang.Published)
                            {
                                //the language is found. now we need to save it
                                if (this.CurrentCustomer != null && !lang.Equals(this.CurrentCustomer.Language))
                                {
                                    this.CurrentCustomer.Language = lang;
                                    _customerService.UpdateCustomer(this.CurrentCustomer);
                                }
                                return lang;
                            }
                        }
                    }
                }

                // Absolute fallback
                lang = _languageService.GetAllLanguages().FirstOrDefault();
                return lang;
            }
            set
            {
                if (this.CurrentCustomer == null)
                    return;

                this.CurrentCustomer.Language = value;
                _customerService.UpdateCustomer(this.CurrentCustomer);
            }
        }

        /// <summary>
        /// Get or set current user working currency
        /// </summary>
        public Currency WorkingCurrency
        {
            get
            {
                Currency primaryStoreCurrency = null;
                //return primary store currency when we're in admin area/mode
                if (this.IsAdmin)
                {
                    primaryStoreCurrency =  _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                    if (primaryStoreCurrency != null)
                        return primaryStoreCurrency;
                }

                if (this.CurrentCustomer != null &&
                    this.CurrentCustomer.Currency != null &&
                    this.CurrentCustomer.Currency.Published)
                    return this.CurrentCustomer.Currency;

                // codehint: sm-edit
                primaryStoreCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId);
                if (primaryStoreCurrency != null && primaryStoreCurrency.Published)
                {
                    return primaryStoreCurrency;
                }
                return _currencyService.GetAllCurrencies().FirstOrDefault();
            }
            set
            {
                if (this.CurrentCustomer == null)
                    return;

                this.CurrentCustomer.Currency = value;
                _customerService.UpdateCustomer(this.CurrentCustomer);
            }
        }

        /// <summary>
        /// Get or set current tax display type
        /// </summary>
        public TaxDisplayType TaxDisplayType
        {
            get
            {
                return GetTaxDisplayTypeFor(this.CurrentCustomer);
            }
            set
            {
                if (!_taxSettings.AllowCustomersToSelectTaxDisplayType)
                    return;

                this.CurrentCustomer.TaxDisplayType = value;
                _customerService.UpdateCustomer(this.CurrentCustomer);
            }
        }

        public TaxDisplayType GetTaxDisplayTypeFor(Customer customer)
        {
            if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
            {
                if (customer != null)
                {
                    if (customer.TaxDisplayType.HasValue)
                    {
                        return customer.TaxDisplayType.Value;
                    }
                }
            }

            var customerRoles = customer.CustomerRoles;
            string key = string.Format(FrameworkCacheConsumer.CUSTOMERROLES_TAX_DISPLAY_TYPES_KEY, String.Join(",", customerRoles.Select(x => x.Id)));
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

        public bool IsAdmin
        {
            get
            {
                return _cachedIsAdmin;
            }
            set
            {
                _cachedIsAdmin = value;
            }
        }

        //// codehint (sm-add)
        //public bool IsPublic
        //{
        //    get
        //    {
        //        return _cachedIsPublic;
        //    }
        //    set
        //    {
        //        _cachedIsPublic = value;
        //    }
        //}
    }
}
