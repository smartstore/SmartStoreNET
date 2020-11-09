using System;
using System.Linq;
using System.Web;
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
using SmartStore.Services.Tax;

namespace SmartStore.Web.Framework
{
    public partial class WebWorkContext : IWorkContext
    {
        private const string VisitorCookieName = "SMARTSTORE.VISITOR";

        private readonly HttpContextBase _httpContext;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _attrService;
        private readonly TaxSettings _taxSettings;
        private readonly PrivacySettings _privacySettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ICacheManager _cacheManager;
        private readonly Lazy<ITaxService> _taxService;
        private readonly IUserAgent _userAgent;
        private readonly IWebHelper _webHelper;
        private readonly IGeoCountryLookup _geoCountryLookup;
        private readonly ICountryService _countryService;

        private TaxDisplayType? _cachedTaxDisplayType;
        private Language _cachedLanguage;
        private Customer _cachedCustomer;
        private Currency _cachedCurrency;
        private Customer _originalCustomerIfImpersonated;
        private bool? _isAdmin;

        public WebWorkContext(
            ICacheManager cacheManager,
            HttpContextBase httpContext,
            ICustomerService customerService,
            IStoreContext storeContext,
            IAuthenticationService authenticationService,
            ILanguageService languageService,
            ICurrencyService currencyService,
            IGenericAttributeService attrService,
            TaxSettings taxSettings,
            PrivacySettings privacySettings,
            LocalizationSettings localizationSettings,
            Lazy<ITaxService> taxService,
            IUserAgent userAgent,
            IWebHelper webHelper,
            IGeoCountryLookup geoCountryLookup,
            ICountryService countryService)
        {
            _cacheManager = cacheManager;
            _httpContext = httpContext;
            _customerService = customerService;
            _storeContext = storeContext;
            _authenticationService = authenticationService;
            _languageService = languageService;
            _attrService = attrService;
            _currencyService = currencyService;
            _taxSettings = taxSettings;
            _privacySettings = privacySettings;
            _taxService = taxService;
            _localizationSettings = localizationSettings;
            _userAgent = userAgent;
            _webHelper = webHelper;
            _geoCountryLookup = geoCountryLookup;
            _countryService = countryService;
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

                // Is system account?
                if (TryGetSystemAccount(out var customer))
                {
                    // Get out quickly. Bots tend to overstress the shop.
                    _cachedCustomer = customer;
                    return customer;
                }

                // Registered user?
                customer = _authenticationService.GetAuthenticatedCustomer();

                // impersonate user if required (currently used for 'phone order' support)
                if (customer != null && !customer.Deleted && customer.Active)
                {
                    int? impersonatedCustomerId = customer.GetAttribute<int?>(SystemCustomerAttributeNames.ImpersonatedCustomerId);
                    if (impersonatedCustomerId.HasValue && impersonatedCustomerId.Value > 0)
                    {
                        var impersonatedCustomer = _customerService.GetCustomerById(impersonatedCustomerId.Value);
                        if (impersonatedCustomer != null && !impersonatedCustomer.Deleted && impersonatedCustomer.Active)
                        {
                            // set impersonated customer
                            _originalCustomerIfImpersonated = customer;
                            customer = impersonatedCustomer;
                        }
                    }
                }

                // Load guest customer
                if (customer == null || customer.Deleted || !customer.Active)
                {
                    customer = GetGuestCustomer();
                }

                _cachedCustomer = customer;

                return _cachedCustomer;
            }
            set => _cachedCustomer = value;
        }

        protected bool TryGetSystemAccount(out Customer customer)
        {
            // Never check whether customer is deleted/inactive in this method.
            // System accounts should neither be deletable nor activatable, they are mandatory.

            customer = null;

            // check whether request is made by a background task
            // in this case return built-in customer record for background task
            if (_httpContext == null || _httpContext.IsFakeContext())
            {
                customer = _customerService.GetCustomerBySystemName(SystemCustomerNames.BackgroundTask);
            }

            // check whether request is made by a search engine
            // in this case return built-in customer record for search engines 
            if (customer == null && _userAgent.IsBot)
            {
                customer = _customerService.GetCustomerBySystemName(SystemCustomerNames.SearchEngine);
            }

            // check whether request is made by the PDF converter
            // in this case return built-in customer record for the converter
            if (customer == null && _userAgent.IsPdfConverter)
            {
                customer = _customerService.GetCustomerBySystemName(SystemCustomerNames.PdfConverter);
            }

            return customer != null;
        }

        protected virtual Customer GetGuestCustomer()
        {
            Customer customer = null;

            var visitorCookie = _httpContext?.Request?.Cookies[VisitorCookieName];
            if (visitorCookie == null)
            {
                // No anonymous visitor cookie yet. Try to identify anyway (by IP and UserAgent)
                customer = _customerService.FindGuestCustomerByClientIdent(maxAgeSeconds: 180);
            }
            else
            {
                if (visitorCookie.Value.HasValue())
                {
                    // Cookie present. Try to load guest customer by it's value.
                    if (Guid.TryParse(visitorCookie.Value, out var customerGuid))
                    {
                        customer = _customerService.GetCustomerByGuid(customerGuid);
                    }
                }
            }

            if (customer == null || customer.Deleted || !customer.Active || customer.IsRegistered())
            {
                // No record yet or account deleted/deactivated.
                // Also dont' treat registered customers as guests.
                // Create new record in these cases.
                customer = _customerService.InsertGuestCustomer();
            }

            // Set visitor cookie
            if (_httpContext?.Response != null)
            {
                var secure = _httpContext.Request.IsHttps();
                visitorCookie = new HttpCookie(VisitorCookieName)
                {
                    HttpOnly = true,
                    Secure = secure,
                    SameSite = secure ? (SameSiteMode)_privacySettings.SameSiteMode : SameSiteMode.Lax
                };

                visitorCookie.Value = customer.CustomerGuid.ToString();
                if (customer.CustomerGuid == Guid.Empty)
                {
                    visitorCookie.Expires = DateTime.Now.AddMonths(-1);
                }
                else
                {
                    int cookieExpires = 24 * 365; // TODO make configurable
                    visitorCookie.Expires = DateTime.Now.AddHours(cookieExpires);
                }

                try
                {
                    _httpContext.Response.Cookies.Remove(VisitorCookieName);
                }
                finally
                {
                    _httpContext.Response.Cookies.Add(visitorCookie);
                }
            }

            return customer;
        }

        /// <summary>
        /// Gets or sets the original customer (in case the current one is impersonated)
        /// </summary>
        public Customer OriginalCustomerIfImpersonated => _originalCustomerIfImpersonated;

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
                var customer = this.CurrentCustomer;
                int customerLangId = 0;

                if (customer != null)
                {
                    if (customer.IsSystemAccount)
                    {
                        customerLangId = _httpContext.Request.QueryString["lid"].ToInt();
                    }
                    else
                    {
                        customerLangId = customer.GetAttribute<int>(SystemCustomerAttributeNames.LanguageId, _attrService, _storeContext.CurrentStore.Id);
                    }
                }

                if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled && _httpContext.Request != null)
                {
                    #region Get language from URL (if possible)

                    var helper = new LocalizedUrlHelper(_httpContext.Request, true);
                    string seoCode;
                    if (helper.IsLocalizedUrl(out seoCode))
                    {
                        if (_languageService.IsPublishedLanguage(seoCode, storeId))
                        {
                            // The language is found. now we need to save it
                            var langBySeoCode = _languageService.GetLanguageBySeoCode(seoCode);

                            if (customer != null && customerLangId != langBySeoCode.Id)
                            {
                                customerLangId = langBySeoCode.Id;
                                SetCustomerLanguage(langBySeoCode.Id, storeId);
                            }
                            _cachedLanguage = langBySeoCode;
                            return langBySeoCode;
                        }
                    }

                    #endregion
                }

                if (_localizationSettings.DetectBrowserUserLanguage && !customer.IsSystemAccount && (customerLangId == 0 || !_languageService.IsPublishedLanguage(customerLangId, storeId)))
                {
                    #region Get Browser UserLanguage

                    // Fallback to browser detected language
                    Language browserLanguage = null;

                    if (_httpContext.Request?.UserLanguages != null)
                    {
                        var userLangs = _httpContext.Request.UserLanguages.Select(x => x.Split(new[] { ';' }, 2, StringSplitOptions.RemoveEmptyEntries)[0]);
                        if (userLangs.Any())
                        {
                            foreach (var culture in userLangs)
                            {
                                browserLanguage = _languageService.GetLanguageByCulture(culture) ?? _languageService.GetLanguageBySeoCode(culture);
                                if (browserLanguage != null && _languageService.IsPublishedLanguage(browserLanguage.Id, storeId))
                                {
                                    // The language is found. Now we need to save it
                                    if (customer != null && customerLangId != browserLanguage.Id)
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

                if (customerLangId > 0 && _languageService.IsPublishedLanguage(customerLangId, storeId))
                {
                    _cachedLanguage = _languageService.GetLanguageById(customerLangId);
                    return _cachedLanguage;
                }

                // Fallback
                customerLangId = _languageService.GetDefaultLanguageId(storeId);

                if (customer != null)
                {
                    SetCustomerLanguage(customerLangId, storeId);
                }

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
            if (this.CurrentCustomer.IsSystemAccount)
                return;

            _attrService.SaveAttribute(this.CurrentCustomer, SystemCustomerAttributeNames.LanguageId, languageId, storeId);
            _customerService.UpdateCustomer(this.CurrentCustomer);
        }

        /// <summary>
        /// Get or set current user working currency
        /// </summary>
        public Currency WorkingCurrency
        {
            get
            {
                if (_cachedCurrency != null)
                {
                    return _cachedCurrency;
                }

                Currency currency = null;

                // return primary store currency when we're in admin area/mode
                if (this.IsAdmin)
                {
                    currency = _storeContext.CurrentStore.PrimaryStoreCurrency;
                }

                if (currency == null)
                {
                    // find current customer language
                    var customer = this.CurrentCustomer;
                    var storeCurrenciesMap = _currencyService.GetAllCurrencies(storeId: _storeContext.CurrentStore.Id).ToDictionary(x => x.Id);

                    if (customer != null && !customer.IsSearchEngineAccount())
                    {
                        // search engines should always crawl by primary store currency
                        var customerCurrencyId = customer.GetAttribute<int?>(SystemCustomerAttributeNames.CurrencyId, _attrService, _storeContext.CurrentStore.Id);
                        if (customerCurrencyId.GetValueOrDefault() > 0)
                        {
                            if (storeCurrenciesMap.TryGetValue((int)customerCurrencyId, out currency))
                            {
                                currency = VerifyCurrency(currency);
                                if (currency == null)
                                {
                                    _attrService.SaveAttribute<int?>(customer, SystemCustomerAttributeNames.CurrencyId, null, _storeContext.CurrentStore.Id);
                                }
                            }
                        }
                    }

                    // if there's only one currency for current store it dominates the primary currency
                    if (storeCurrenciesMap.Count == 1)
                    {
                        currency = storeCurrenciesMap[storeCurrenciesMap.Keys.First()];
                    }

                    // Default currency of country to which the current IP address belongs.
                    if (currency == null)
                    {
                        var ipAddress = _webHelper.GetCurrentIpAddress();
                        var lookupCountry = _geoCountryLookup.LookupCountry(ipAddress);
                        if (lookupCountry != null)
                        {
                            var country = _countryService.GetCountryByTwoLetterIsoCode(lookupCountry.IsoCode);
                            if (country?.DefaultCurrency?.Published ?? false)
                            {
                                currency = country.DefaultCurrency;
                            }
                        }
                    }

                    // find currency by domain ending
                    if (currency == null && _httpContext?.Request?.Url != null)
                    {
                        currency = storeCurrenciesMap.Values.GetByDomainEnding(_httpContext.Request.Url.Authority);
                    }

                    // get PrimaryStoreCurrency
                    if (currency == null)
                    {
                        currency = VerifyCurrency(_storeContext.CurrentStore.PrimaryStoreCurrency);
                    }

                    // get the first published currency for current store
                    if (currency == null)
                    {
                        currency = storeCurrenciesMap.Values.FirstOrDefault();
                    }
                }

                // if not found in currencies filtered by the current store, then return any currency
                if (currency == null)
                {
                    currency = _currencyService.GetAllCurrencies().FirstOrDefault();
                }

                // no published currency available (fix it)
                if (currency == null)
                {
                    currency = _currencyService.GetAllCurrencies(true).FirstOrDefault();
                    if (currency != null)
                    {
                        currency.Published = true;
                        _currencyService.UpdateCurrency(currency);
                    }
                }

                _cachedCurrency = currency;
                return _cachedCurrency;
            }
            set
            {
                _attrService.SaveAttribute<int?>(this.CurrentCustomer, SystemCustomerAttributeNames.CurrencyId, value?.Id, _storeContext.CurrentStore.Id);
                _customerService.UpdateCustomer(this.CurrentCustomer);
                _cachedCurrency = null;
            }
        }

        private Currency VerifyCurrency(Currency currency)
        {
            if (currency != null && !currency.Published)
            {
                return null;
            }
            return currency;
        }

        /// <summary>
        /// Get or set current tax display type
        /// </summary>
        public TaxDisplayType TaxDisplayType
        {
            get => GetTaxDisplayTypeFor(this.CurrentCustomer, _storeContext.CurrentStore.Id);
            set
            {
                if (!_taxSettings.AllowCustomersToSelectTaxDisplayType)
                    return;

                this.CurrentCustomer.TaxDisplayTypeId = (int)value;
                _customerService.UpdateCustomer(this.CurrentCustomer);
            }
        }

        public TaxDisplayType GetTaxDisplayTypeFor(Customer customer, int storeId)
        {
            if (_cachedTaxDisplayType.HasValue)
            {
                return _cachedTaxDisplayType.Value;
            }

            int? taxDisplayType = null;

            if (_taxSettings.AllowCustomersToSelectTaxDisplayType && customer != null)
            {
                taxDisplayType = customer.TaxDisplayTypeId;
            }

            if (!taxDisplayType.HasValue && _taxSettings.EuVatEnabled)
            {
                if (customer != null && _taxService.Value.IsVatExempt(null, customer))
                {
                    taxDisplayType = (int)TaxDisplayType.ExcludingTax;
                }
            }

            if (!taxDisplayType.HasValue)
            {
                var customerRoles = customer.CustomerRoleMappings.Select(x => x.CustomerRole).ToList();
                string key = string.Format(FrameworkCacheConsumer.CUSTOMERROLES_TAX_DISPLAY_TYPES_KEY, String.Join(",", customerRoles.Select(x => x.Id)), storeId);
                var cacheResult = _cacheManager.Get(key, () =>
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

                taxDisplayType = (int)cacheResult;
            }

            _cachedTaxDisplayType = (TaxDisplayType)taxDisplayType.Value;
            return _cachedTaxDisplayType.Value;
        }


        public bool IsAdmin
        {
            get
            {
                if (!_isAdmin.HasValue)
                {
                    _isAdmin = _httpContext.Request.IsAdminArea();
                }

                return _isAdmin.Value;
            }
            set => _isAdmin = value;
        }

        [Obsolete("Use ILanguageService.IsPublishedLanguage() instead")]
        public bool IsPublishedLanguage(string seoCode, int storeId = 0)
        {
            return _languageService.IsPublishedLanguage(seoCode, storeId);
        }

        [Obsolete("Use ILanguageService.GetDefaultLanguageSeoCode() instead")]
        public string GetDefaultLanguageSeoCode(int storeId = 0)
        {
            return _languageService.GetDefaultLanguageSeoCode(storeId);
        }

    }
}
