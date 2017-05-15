using System;
using System.Linq;
using System.Net;
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
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework
{
    /// <summary>
	/// Work context for web application
    /// </summary>
    public partial class WebWorkContext : IWorkContext
    {
        private readonly HttpContextBase _httpContext;
        private readonly ICustomerService _customerService;
		private readonly IStoreContext _storeContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
		private readonly IGenericAttributeService _attrService;
        private readonly TaxSettings _taxSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ICacheManager _cacheManager;
        private readonly IStoreService _storeService;
		private readonly Lazy<ITaxService> _taxService;
		private readonly IUserAgent _userAgent;

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
            LocalizationSettings localizationSettings,
			Lazy<ITaxService> taxService,
            IStoreService storeService,
			IUserAgent userAgent)
        {
			this._cacheManager = cacheManager;
            this._httpContext = httpContext;
            this._customerService = customerService;
			this._storeContext = storeContext;
            this._authenticationService = authenticationService;
            this._languageService = languageService;
			this._attrService = attrService;
            this._currencyService = currencyService;
            this._taxSettings = taxSettings;
			this._taxService = taxService;
            this._localizationSettings = localizationSettings;
            this._storeService = storeService;
			this._userAgent = userAgent;
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

				// Is system account?
				if (TryGetSystemAccount(out customer))
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
            set
            {
                _cachedCustomer = value;
            }
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
			Guid customerGuid = Guid.Empty;

			var anonymousId = _httpContext.Request.AnonymousID;

			if (anonymousId != null && anonymousId.HasValue())
			{
				Guid.TryParse(anonymousId, out customerGuid);
			}

			if (customerGuid == Guid.Empty)
			{
				_httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				_httpContext.Response.End();
			}

			// Try to load an existing record...
			customer = _customerService.GetCustomerByGuid(customerGuid);

			if (customer == null || customer.Deleted || !customer.Active || customer.IsRegistered())
			{
				// ...but no record yet. Create one.
				customer = _customerService.InsertGuestCustomer(customerGuid);
			}

			return customer;
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
                        _attrService, 
                        _storeContext.CurrentStore.Id);
                }

                #region Get language from URL (if possible)

				if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled && _httpContext != null && _httpContext.Request != null)
                {
                    var helper = new LocalizedUrlHelper(_httpContext.Request, true);
                    string seoCode;
                    if (helper.IsLocalizedUrl(out seoCode))
                    {
                        if (_languageService.IsPublishedLanguage(seoCode, storeId))
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

				if (_localizationSettings.DetectBrowserUserLanguage && (customerLangId == 0 || !_languageService.IsPublishedLanguage(customerLangId, storeId)))
                {
                    #region Get Browser UserLanguage

                    // Fallback to browser detected language
                    Language browserLanguage = null;

                    if (_httpContext != null && _httpContext.Request != null && _httpContext.Request.UserLanguages != null)
                    {
                        var userLangs = _httpContext.Request.UserLanguages.Select(x => x.Split(new[] { ';' }, 2, StringSplitOptions.RemoveEmptyEntries)[0]);
                        if (userLangs.Any())
                        {
                            foreach (var culture in userLangs)
                            {
                                browserLanguage = _languageService.GetLanguageByCulture(culture);
								if (browserLanguage != null && _languageService.IsPublishedLanguage(browserLanguage.Id, storeId))
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

				if (customerLangId > 0 && _languageService.IsPublishedLanguage(customerLangId, storeId))
                {
                    _cachedLanguage = _languageService.GetLanguageById(customerLangId);
                    return _cachedLanguage;
                }
                
                // Fallback
				customerLangId = _languageService.GetDefaultLanguageId(storeId);
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
            _attrService.SaveAttribute(
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
                if (_cachedCurrency != null)
                    return _cachedCurrency;

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
							if (storeCurrenciesMap.TryGetValue(customerCurrencyId.Value, out currency))
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

					// find currency by domain ending
					if (currency == null && _httpContext != null && _httpContext.Request != null && _httpContext.Request.Url != null)
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
                int? id = value != null ? value.Id : (int?)null;
				_attrService.SaveAttribute<int?>(this.CurrentCustomer, SystemCustomerAttributeNames.CurrencyId, id, _storeContext.CurrentStore.Id);
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
            get
            {
				return GetTaxDisplayTypeFor(this.CurrentCustomer, _storeContext.CurrentStore.Id);
            }
            set
            {
                if (!_taxSettings.AllowCustomersToSelectTaxDisplayType)
                    return;

				_attrService.SaveAttribute(this.CurrentCustomer,
					 SystemCustomerAttributeNames.TaxDisplayTypeId,
					 (int)value, _storeContext.CurrentStore.Id);
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
		        taxDisplayType = customer.GetAttribute<int?>(SystemCustomerAttributeNames.TaxDisplayTypeId, storeId);
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
                var customerRoles = customer.CustomerRoles;
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
			set
			{
				_isAdmin = value;
			}
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
