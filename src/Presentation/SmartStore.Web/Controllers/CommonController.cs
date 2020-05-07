﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Core.Security;
using SmartStore.Core.Themes;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Forums;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using SmartStore.Utilities;
using SmartStore.Utilities.ObjectPools;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Pdf;
using SmartStore.Web.Framework.Theming;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web.Controllers
{
    public partial class CommonController : PublicControllerBase
    {
		private readonly static string[] s_hints = new string[] { "Shopsystem", "Onlineshop Software", "Shopsoftware", "E-Commerce Solution" };

		private readonly Lazy<ILanguageService> _languageService;
        private readonly Lazy<ICurrencyService> _currencyService;
        private readonly IThemeContext _themeContext;
        private readonly Lazy<IThemeRegistry> _themeRegistry;
        private readonly Lazy<IForumService> _forumService;
        private readonly Lazy<IGenericAttributeService> _genericAttributeService;
		private readonly Lazy<IUrlRecordService> _urlRecordService;
		private readonly IPageAssetsBuilder _pageAssetsBuilder;
        private readonly Lazy<IMediaService> _mediaService;
        private readonly ICookieManager _cookieManager;
        private readonly IGeoCountryLookup _geoCountryLookup;
        private readonly IWebHelper _webHelper;
        private readonly ICountryService _countryService;

        private readonly CustomerSettings _customerSettings;
		private readonly PrivacySettings _privacySettings;
		private readonly TaxSettings _taxSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ThemeSettings _themeSettings;
        private readonly CommonSettings _commonSettings;
		private readonly NewsSettings _newsSettings;
        private readonly BlogSettings _blogSettings;
        private readonly ForumSettings _forumSettings;
        private readonly LocalizationSettings _localizationSettings;
		private readonly Lazy<SocialSettings> _socialSettings;

        public CommonController(
			Lazy<ILanguageService> languageService,
            Lazy<ICurrencyService> currencyService,
			IThemeContext themeContext,
            Lazy<IThemeRegistry> themeRegistry, 
			Lazy<IForumService> forumService,
            Lazy<IGenericAttributeService> genericAttributeService, 
			Lazy<IUrlRecordService> urlRecordService,
			IPageAssetsBuilder pageAssetsBuilder,
            Lazy<IMediaService> mediaService,
            CustomerSettings customerSettings,
			PrivacySettings privacySettings,
			TaxSettings taxSettings, 
			CatalogSettings catalogSettings,
            ShoppingCartSettings shoppingCartSettings,
            CommonSettings commonSettings, 
			NewsSettings newsSettings,
			BlogSettings blogSettings, 
			ForumSettings forumSettings,
            LocalizationSettings localizationSettings, 
			Lazy<SocialSettings> socialSettings,
            ThemeSettings themeSettings,
            ICookieManager cookieManager,
            IGeoCountryLookup geoCountryLookup,
            IWebHelper webHelper,
            ICountryService countryService)
        {
            _languageService = languageService;
            _currencyService = currencyService;
            _themeContext = themeContext;
            _themeRegistry = themeRegistry;
            _forumService = forumService;
            _genericAttributeService = genericAttributeService;
			_urlRecordService = urlRecordService;
			_pageAssetsBuilder = pageAssetsBuilder;
            _mediaService = mediaService;
			_customerSettings = customerSettings;
			_privacySettings = privacySettings;
			_taxSettings = taxSettings;
            _catalogSettings = catalogSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _commonSettings = commonSettings;
			_newsSettings = newsSettings;
            _blogSettings = blogSettings;
            _forumSettings = forumSettings;
            _localizationSettings = localizationSettings;
			_socialSettings = socialSettings;
            _themeSettings = themeSettings;
            _cookieManager = cookieManager;
            _geoCountryLookup = geoCountryLookup;
            _webHelper = webHelper;
            _countryService = countryService;
        }

        #region Utilities

        [NonAction]
        protected LanguageSelectorModel PrepareLanguageSelectorModel()
        {
			var availableLanguages = Services.Cache.Get(string.Format(ModelCacheEventConsumer.AVAILABLE_LANGUAGES_MODEL_KEY, Services.StoreContext.CurrentStore.Id), () =>
            {
                var result = _languageService.Value
					.GetAllLanguages(storeId: Services.StoreContext.CurrentStore.Id)
                    .Select(x => new LanguageModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        NativeName = LocalizationHelper.GetLanguageNativeName(x.LanguageCulture) ?? x.Name,
                        ISOCode = x.LanguageCulture,
                        SeoCode = x.UniqueSeoCode,
                        FlagImageFileName = x.FlagImageFileName
                    })
                    .ToList();
                return result;
            });

			var workingLanguage = Services.WorkContext.WorkingLanguage;

            var model = new LanguageSelectorModel
            {
                CurrentLanguageId = workingLanguage.Id,
                AvailableLanguages = availableLanguages,
                UseImages = _localizationSettings.UseImagesForLanguageSelection
            };

			string defaultSeoCode = _languageService.Value.GetDefaultLanguageSeoCode();

            foreach (var lang in model.AvailableLanguages)
            {
                //var helper = new LocalizedUrlHelper(HttpContext.Request, true);
				var helper = CreateUrlHelperForLanguageSelector(lang, workingLanguage.Id);

				if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    if (lang.SeoCode == defaultSeoCode && (int)(_localizationSettings.DefaultLanguageRedirectBehaviour) > 0)
                    {
                        helper.StripSeoCode();
                    }
                    else
                    {
                        helper.PrependSeoCode(lang.SeoCode, true);
                    }
                }

                model.ReturnUrls[lang.SeoCode] = helper.RelativePath;
            }

            return model;
        }

		private LocalizedUrlHelper CreateUrlHelperForLanguageSelector(LanguageModel model, int currentLanguageId)
		{
			if (currentLanguageId != model.Id)
			{
				var routeValues = this.Request.RequestContext.RouteData.Values;
				var controller = routeValues["controller"].ToString();

				if (!routeValues.TryGetValue(controller + "id", out var val))
				{
					controller = routeValues["action"].ToString();
					routeValues.TryGetValue(controller + "id", out val);
				}

				int entityId = 0;
				if (val != null)
				{
					entityId = val.Convert<int>();
				}

				if (entityId > 0)
				{
					var activeSlug = _urlRecordService.Value.GetActiveSlug(entityId, controller, model.Id);
					if (activeSlug.IsEmpty())
					{
						// Fallback to default value.
						activeSlug = _urlRecordService.Value.GetActiveSlug(entityId, controller, 0);
					}

					if (activeSlug.HasValue())
					{
						var helper = new LocalizedUrlHelper(Request.ApplicationPath, activeSlug, false);
						return helper;
					}
				}
			}

			return new LocalizedUrlHelper(HttpContext.Request, true);
		}

        [NonAction]
        protected CurrencySelectorModel PrepareCurrencySelectorModel()
        {
			var availableCurrencies = Services.Cache.Get(string.Format(ModelCacheEventConsumer.AVAILABLE_CURRENCIES_MODEL_KEY, Services.WorkContext.WorkingLanguage.Id, Services.StoreContext.CurrentStore.Id), () =>
            {
                var result = _currencyService.Value
					.GetAllCurrencies(storeId: Services.StoreContext.CurrentStore.Id)
                    .Select(x => new CurrencyModel
                    {
                        Id = x.Id,
                        Name = x.GetLocalized(y => y.Name),
                        ISOCode = x.CurrencyCode,
                        Symbol = LocalizationHelper.GetCurrencySymbol(x.DisplayLocale) ?? x.CurrencyCode
                    })
                    .ToList();
                return result;
            });

            var model = new CurrencySelectorModel()
            {
				CurrentCurrencyId = Services.WorkContext.WorkingCurrency.Id,
                AvailableCurrencies = availableCurrencies
            };
            return model;
        }

        [NonAction]
        protected TaxTypeSelectorModel PrepareTaxTypeSelectorModel()
        {
            var model = new TaxTypeSelectorModel()
            {
                Enabled = _taxSettings.AllowCustomersToSelectTaxDisplayType,
				CurrentTaxType = Services.WorkContext.TaxDisplayType
            };
            return model;
        }

        #endregion

		#region Methods

        [ChildActionOnly]
        public ActionResult LanguageSelector()
        {
            var model = PrepareLanguageSelectorModel();
            
			if (model.AvailableLanguages.Count < 2)
				return Content("");

			// register all available languages as <link hreflang="..." ... />
			if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
			{
				var host = Services.WebHelper.GetStoreLocation();
				foreach (var lang in model.AvailableLanguages)
				{
					_pageAssetsBuilder.AddLinkPart("alternate", host.EnsureEndsWith("/") + model.ReturnUrls[lang.SeoCode].TrimStart('/'), hreflang: lang.SeoCode);
				}
			}

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult Logo()
        {
            var store = Services.StoreContext.CurrentStore;
            var logo = _mediaService.Value.GetFileById(store.LogoMediaFileId);

            var model = new ShopHeaderModel
            {
                LogoUploaded = logo != null,
				LogoTitle = store.Name
			};

            if (logo != null)
            {
                model.LogoUrl = _mediaService.Value.GetUrl(logo, 0, null, false);
                model.LogoWidth = logo.Dimensions.Width;
                model.LogoHeight = logo.Dimensions.Height;
            }

            return PartialView(model);
        }

        public ActionResult SetLanguage(int langid, string returnUrl = "")
        {
			var language = _languageService.Value.GetLanguageById(langid);
            if (language != null && language.Published)
            {
                Services.WorkContext.WorkingLanguage = language;
            }

            var helper = new LocalizedUrlHelper(HttpContext.Request.ApplicationPath, returnUrl, false);

            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                helper.PrependSeoCode(Services.WorkContext.WorkingLanguage.UniqueSeoCode, true);
            }

            returnUrl = helper.GetAbsolutePath();

            return RedirectToReferrer(returnUrl);
        }

        [ChildActionOnly]
        public ActionResult CurrencySelector()
        {
            var model = PrepareCurrencySelectorModel();

            if (model.AvailableCurrencies.Count < 2)
            {
                return new EmptyResult();
            }

            return PartialView(model);
        }

        public ActionResult CurrencySelected(int customerCurrency, string returnUrl = "")
        {
			var currency = _currencyService.Value.GetCurrencyById(customerCurrency);
            if (currency != null)
			{
                Services.WorkContext.WorkingCurrency = currency;
			}		

            return RedirectToReferrer(returnUrl);
        }

        [ChildActionOnly]
        public ActionResult TaxTypeSelector()
        {
            var model = PrepareTaxTypeSelectorModel();
            return PartialView(model);
        }

        public ActionResult TaxTypeSelected(int customerTaxType, string returnUrl = "")
        {
            var taxDisplayType = (TaxDisplayType)Enum.ToObject(typeof(TaxDisplayType), customerTaxType);
            Services.WorkContext.TaxDisplayType = taxDisplayType;

            return RedirectToReferrer(returnUrl);
        }

        // footer
        [ChildActionOnly]
        public ActionResult JavaScriptDisabledWarning()
        {
            if (!_commonSettings.DisplayJavaScriptDisabledWarning)
            {
                return new EmptyResult();
            }

            return PartialView();
        }

        [ChildActionOnly]
        public ActionResult ShopBar()
        {
			var customer = Services.WorkContext.CurrentCustomer;
			var isAdmin = customer.IsAdmin();
			var isRegistered = isAdmin || customer.IsRegistered();
            
            var model = new ShopBarModel
            {
                IsAuthenticated = isRegistered,
                CustomerEmailUsername = isRegistered ? (_customerSettings.CustomerLoginType != CustomerLoginType.Email ? customer.Username : customer.Email) : "",
				IsCustomerImpersonated = Services.WorkContext.OriginalCustomerIfImpersonated != null,
				DisplayAdminLink = Services.Permissions.Authorize(Permissions.System.AccessBackend),
				ShoppingCartEnabled = Services.Permissions.Authorize(Permissions.Cart.AccessShoppingCart) && _shoppingCartSettings.MiniShoppingCartEnabled,
				WishlistEnabled = Services.Permissions.Authorize(Permissions.Cart.AccessWishlist),
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled,
				PublicStoreNavigationAllowed = Services.Permissions.Authorize(Permissions.System.AccessShop)
			};

			return PartialView(model);
        }

		[ChildActionOnly]
		[GdprConsent]
		public ActionResult Footer()
        {
			var store = Services.StoreContext.CurrentStore;
			var taxDisplayType = Services.WorkContext.GetTaxDisplayTypeFor(Services.WorkContext.CurrentCustomer, store.Id);

			var taxInfo = T(taxDisplayType == TaxDisplayType.IncludingTax ? "Tax.InclVAT" : "Tax.ExclVAT");

			var availableStoreThemes = !_themeSettings.AllowCustomerToSelectTheme 
                ? new List<StoreThemeModel>() 
                : _themeRegistry.Value.GetThemeManifests()
                    .Select(x =>
                    {
                        return new StoreThemeModel
                        {
                            Name = x.ThemeName,
                            Title = x.ThemeTitle
                        };
                    })
                    .ToList();

            var model = new FooterModel
            {
				StoreName = store.Name,
                ShowLegalInfo = _taxSettings.ShowLegalHintsInFooter,
                ShowThemeSelector = availableStoreThemes.Count > 1,          
                HideNewsletterBlock = _customerSettings.HideNewsletterBlock
            };

			var shippingInfoUrl = Url.Topic("shippinginfo").ToString();
			if (shippingInfoUrl.HasValue())
			{
				model.LegalInfo = T("Tax.LegalInfoFooter", taxInfo, shippingInfoUrl);
			}
			else
			{
				model.LegalInfo = T("Tax.LegalInfoFooter2", taxInfo);
			}

			var hint = Services.Settings.GetSettingByKey<string>("Rnd_SmCopyrightHint", string.Empty, store.Id);
			if (hint.IsEmpty())
			{
				hint = s_hints[CommonHelper.GenerateRandomInteger(0, s_hints.Length - 1)];

                Services.Settings.SetSetting<string>("Rnd_SmCopyrightHint", hint, store.Id);
			}

            model.ShowSocialLinks = _socialSettings.Value.ShowSocialLinksInFooter;
            model.FacebookLink = _socialSettings.Value.FacebookLink;
            model.TwitterLink = _socialSettings.Value.TwitterLink;
            model.PinterestLink = _socialSettings.Value.PinterestLink;
            model.YoutubeLink = _socialSettings.Value.YoutubeLink;
			model.InstagramLink = _socialSettings.Value.InstagramLink;
			
			model.SmartStoreHint = "<a href='https://www.smartstore.com/' class='sm-hint' target='_blank'><strong>{0}</strong></a> by SmartStore AG &copy; {1}"
				.FormatCurrent(hint, DateTime.Now.Year);

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult TopBar()
        {
			var customer = Services.WorkContext.CurrentCustomer;

            var model = new MenuBarModel
            {
                RecentlyAddedProductsEnabled = _catalogSettings.RecentlyAddedProductsEnabled,
				NewsEnabled = _newsSettings.Enabled,
                BlogEnabled = _blogSettings.Enabled,
                ForumEnabled = _forumSettings.ForumsEnabled,
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.CustomerLoginType != CustomerLoginType.Email ? customer.Username : customer.Email) : "",
				IsCustomerImpersonated = Services.WorkContext.OriginalCustomerIfImpersonated != null,
                IsAuthenticated = customer.IsRegistered(),
				DisplayAdminLink = Services.Permissions.Authorize(Permissions.System.AccessBackend),
				HasContactUsPage = Url.Topic("ContactUs").ToString().HasValue(),
                DisplayLoginLink = _customerSettings.UserRegistrationType != UserRegistrationType.Disabled
            };
            
            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult StoreThemeSelector()
        {
            if (!_themeSettings.AllowCustomerToSelectTheme)
                return Content("");

            var model = new StoreThemeSelectorModel();
            var currentTheme = _themeRegistry.Value.GetThemeManifest(_themeContext.WorkingThemeName);
            model.CurrentStoreTheme = new StoreThemeModel()
            {
                Name = currentTheme.ThemeName,
                Title = currentTheme.ThemeTitle
            };
			model.AvailableStoreThemes = _themeRegistry.Value
				.GetThemeManifests()
                .Select(x =>
                {
                    return new StoreThemeModel
                    {
                        Name = x.ThemeName,
                        Title = x.ThemeTitle
                    };
                })
                .ToList();

            return PartialView(model);
        }

		public ActionResult ChangeTheme(string themeName, string returnUrl = null)
        {
			if (!_themeSettings.AllowCustomerToSelectTheme || (themeName.HasValue() && !_themeRegistry.Value.ThemeManifestExists(themeName)))
			{
				return HttpNotFound();
			}

			_themeContext.WorkingThemeName = themeName;

			if (HttpContext.Request.IsAjaxRequest())
			{
				return Json(new { Success = true });
			}

			return RedirectToReferrer(returnUrl);
        }

        [ChildActionOnly]
        [OutputCache(Duration=3600, VaryByCustom="Theme_Store")]
        public ActionResult Favicon()
        {
            var icons = new string[] 
            { 
                "favicon-{0}.ico".FormatInvariant(Services.StoreContext.CurrentStore.Id), 
                "favicon.ico" 
            };

            string virtualPath = null;

            foreach (var icon in icons)
            {
                virtualPath = Url.ThemeAwareContent(icon);
                if (virtualPath.HasValue())
                {
                    break;
                }
            }

            if (virtualPath.IsEmpty())
            {
                return Content("");
            }

            var model = new FaviconModel()
            {
                Uploaded = true,
                FaviconUrl = virtualPath
            };

            return PartialView(model);
        }

        public ActionResult RobotsTextFile()
        {
            var disallowPaths = new List<string>()
            {
                "/bin/",
				"/Exchange/",
                "/Country/GetStatesByCountryId",
                "/Install$",
                "/Product/SetReviewHelpfulness",
            };
            var localizableDisallowPaths = new List<string>()
            {
                "/Boards/ForumWatch",
                "/Boards/PostEdit",
                "/Boards/PostDelete",
                "/Boards/PostCreate",
                "/Boards/TopicEdit",
                "/Boards/TopicDelete",
                "/Boards/TopicCreate",
                "/Boards/TopicMove",
                "/Boards/TopicWatch",
                "/Cart$",
                "/Checkout",
                "/Product/ClearCompareList",
                "/CompareProducts",
                "/Customer/Avatar",
                "/Customer/Activation",
                "/Customer/Addresses",
                "/Customer/BackInStockSubscriptions",
                "/Customer/ChangePassword",
                "/Customer/CheckUsernameAvailability",
                "/Customer/DownloadableProducts",
                "/Customer/ForumSubscriptions",
				"/Customer/DeleteForumSubscriptions",
                "/Customer/Info",
                "/Customer/Orders",
                "/Customer/ReturnRequests",
                "/Customer/RewardPoints",
                "/PrivateMessages",
                "/Newsletter/SubscriptionActivation",
                "/Order$",
                "/PasswordRecovery",
                "/Poll/Vote",
                "/ReturnRequest",
                "/Newsletter/Subscribe",
                "/Topic/Authenticate",
                "/Wishlist",
                "/Product/AskQuestion",
                "/Product/EmailAFriend",
				//"/Search",
				"/Config$",
				"/Settings$",
				"/Login$",
				"/Login?*",
				"/Register$",
				"/Register?*"
			};


            const string newLine = "\r\n"; //Environment.NewLine
            var sb = PooledStringBuilder.Rent();
            sb.Append("User-agent: *");
            sb.Append(newLine);
			sb.AppendFormat("Sitemap: {0}", Url.RouteUrl("XmlSitemap", (object)null, Services.StoreContext.CurrentStore.ForceSslForAllPages ? "https" : "http"));
			sb.AppendLine();

			var disallows = disallowPaths.Concat(localizableDisallowPaths);

            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                // URLs are localizable. Append SEO code
				foreach (var language in _languageService.Value.GetAllLanguages(storeId: Services.StoreContext.CurrentStore.Id))
                {
                    disallows = disallows.Concat(localizableDisallowPaths.Select(x => "/{0}{1}".FormatInvariant(language.UniqueSeoCode, x)));
                }
            }

            var seoSettings = EngineContext.Current.Resolve<SeoSettings>();

            // append extra disallows
            disallows = disallows.Concat(seoSettings.ExtraRobotsDisallows.Select(x => x.Trim()));

			// Append all lowercase variants (at least Google is case sensitive)
			disallows = disallows.Concat(GetLowerCaseVariants(disallows));

            foreach (var disallow in disallows)
            {
                sb.AppendFormat("Disallow: {0}", disallow);
                sb.Append(newLine);
            }

            Response.ContentType = "text/plain";
            Response.Write(sb.ToStringAndReturn());

            return null;
        }

		private IEnumerable<string> GetLowerCaseVariants(IEnumerable<string> disallows)
		{
			var other = new List<string>();
			foreach (var item in disallows)
			{
				var lower = item.ToLower();
				if (lower != item)
				{
					other.Add(lower);
				}
			}

			return other;
		}

        public ActionResult GenericUrl()
        {
            // Seems that no entity was found
            return HttpNotFound();
        }

        [ChildActionOnly]
        public ActionResult AccountDropdown()
        {
			var customer = Services.WorkContext.CurrentCustomer;

            var unreadMessageCount = GetUnreadPrivateMessages();
            var unreadMessage = string.Empty;
            var alertMessage = string.Empty;
            if (unreadMessageCount > 0)
            {
                unreadMessage = unreadMessageCount.ToString();

                //notifications here
                if (_forumSettings.ShowAlertForPM &&
                    !customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, Services.StoreContext.CurrentStore.Id))
                {
                    _genericAttributeService.Value.SaveAttribute(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true, Services.StoreContext.CurrentStore.Id);
                    alertMessage = T("PrivateMessages.YouHaveUnreadPM", unreadMessageCount);
                }
            }

            var model = new AccountDropdownModel
            {
                IsAuthenticated = customer.IsRegistered(),
				DisplayAdminLink = Services.Permissions.Authorize(Permissions.System.AccessBackend),
				ShoppingCartEnabled = Services.Permissions.Authorize(Permissions.Cart.AccessShoppingCart),
                //ShoppingCartItems = customer.CountProductsInCart(ShoppingCartType.ShoppingCart, Services.StoreContext.CurrentStore.Id),
                WishlistEnabled = Services.Permissions.Authorize(Permissions.Cart.AccessWishlist),
                //WishlistItems = customer.CountProductsInCart(ShoppingCartType.Wishlist, _serServicesvices.StoreContext.CurrentStore.Id),
                AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                UnreadPrivateMessages = unreadMessage,
                AlertMessage = alertMessage
            };

            return PartialView(model);
        }

        [NonAction]
        protected int GetUnreadPrivateMessages()
        {
            var result = 0;
            var customer = Services.WorkContext.CurrentCustomer;
            if (_forumSettings.AllowPrivateMessages && !customer.IsGuest())
            {
                var privateMessages = _forumService.Value.GetAllPrivateMessages(Services.StoreContext.CurrentStore.Id, 0, customer.Id, false, null, false, 0, 1);
                if (privateMessages.TotalCount > 0)
                {
                    result = privateMessages.TotalCount;
                }
            }

            return result;
        }

        [OverrideActionFilters, OverrideAuthorization]
		public ActionResult PdfReceiptHeader(PdfHeaderFooterVariables vars, int storeId = 0, bool isPartial = false)
		{
			var model = PreparePdfReceiptHeaderFooterModel(storeId);
			model.Variables = vars;

			ViewBag.IsPartial = isPartial;

			if (isPartial)
				return PartialView(model);
			return View(model);
		}

        [OverrideActionFilters, OverrideAuthorization]
		public ActionResult PdfReceiptFooter(PdfHeaderFooterVariables vars, int storeId = 0, bool isPartial = false)
		{
			var model = PreparePdfReceiptHeaderFooterModel(storeId);
			model.Variables = vars;

			ViewBag.IsPartial = isPartial;

			if (isPartial)
				return PartialView(model);
			return View(model);
		}

		protected PdfReceiptHeaderFooterModel PreparePdfReceiptHeaderFooterModel(int storeId)
		{
			return Services.Cache.Get("PdfReceiptHeaderFooterModel-{0}".FormatInvariant(storeId), () =>
			{
				var model = new PdfReceiptHeaderFooterModel { StoreId = storeId };
				var store = Services.StoreService.GetStoreById(model.StoreId) ?? Services.StoreContext.CurrentStore;

				var companyInfoSettings = Services.Settings.LoadSetting<CompanyInformationSettings>(store.Id);
				var bankSettings = Services.Settings.LoadSetting<BankConnectionSettings>(store.Id);
				var contactSettings = Services.Settings.LoadSetting<ContactDataSettings>(store.Id);
				var pdfSettings = Services.Settings.LoadSetting<PdfSettings>(store.Id);

				model.StoreName = store.Name;
				model.StoreUrl = store.Url;

                var logo = _mediaService.Value.GetFileById(pdfSettings.LogoPictureId);
                if (logo == null)
                {
                    logo = _mediaService.Value.GetFileById(store.LogoMediaFileId);
                }
                if (logo != null)
                {
                    model.LogoUrl = _mediaService.Value.GetUrl(logo, 0, null, false);
                }

				model.MerchantCompanyInfo = companyInfoSettings;
				model.MerchantBankAccount = bankSettings;
				model.MerchantContactData = contactSettings;
				model.MerchantFormattedAddress = Services.Resolve<IAddressService>().FormatAddress(companyInfoSettings, true);

				return model;			
			}, TimeSpan.FromMinutes(1) /* 1 min. (just for the duration of pdf processing) */);
		}

        #region CookieManager

        public ActionResult CookieManager()
        {
            if (!_privacySettings.EnableCookieConsent)
            {
                return new EmptyResult();
            }

            // If current country doesnt need cookie consent, don't display cookie manager.
            if (!DisplayForCountry())
            {
                return new EmptyResult();
            }

            var cookieData = _cookieManager.GetCookieData(this.ControllerContext);

            if (cookieData != null && !HttpContext.Request.IsAjaxRequest())
            {
                return new EmptyResult();
            }

            var model = new CookieManagerModel();

            PrepareCookieManagerModel(model);

            return PartialView(model);
        }

        private bool DisplayForCountry()
        {
            var ipAddress = _webHelper.GetCurrentIpAddress();
            var lookUpCountryResponse = _geoCountryLookup.LookupCountry(ipAddress);
            if (lookUpCountryResponse == null || lookUpCountryResponse.IsoCode == null)
            {
                // No country was found (e.g. localhost), so we better return true.
                return true;
            }

            var country = _countryService.GetCountryByTwoLetterIsoCode(lookUpCountryResponse.IsoCode);

            if (country != null && country.DisplayCookieManager)
            {
                // Country was configured to display cookie manager.
                return true;
            }

            return false;
        }

        private void PrepareCookieManagerModel(CookieManagerModel model)
        {
            // Get cookie infos from plugins.
            model.CookiesInfos = _cookieManager.GetAllCookieInfos();

            var cookie = _cookieManager.GetCookieData(this.ControllerContext);
            
            model.AnalyticsConsent = cookie != null ? cookie.AllowAnalytics : false;
            model.ThirdPartyConsent = cookie != null ? cookie.AllowThirdParty : false;
        }

        [HttpPost]
        public ActionResult SetCookieManagerConsent(CookieManagerModel model)
        {
            if (model.AcceptAll)
            {
                model.AnalyticsConsent = true;
                model.ThirdPartyConsent = true;
            }

            _cookieManager.SetConsentCookie(Response, model.AnalyticsConsent, model.ThirdPartyConsent);

            if (!HttpContext.Request.IsAjaxRequest() && !ControllerContext.IsChildAction)
            {
                return RedirectToReferrer();
            }

            return Json(new { Success = true });
        }

        #endregion

        [ChildActionOnly]
		public ActionResult GdprConsent(bool isSmall)
		{
			if (!_privacySettings.DisplayGdprConsentOnForms)
			{
				return new EmptyResult();
			}

			var customer = Services.WorkContext.CurrentCustomer;
			var hasConsentedToGdpr = customer.GetAttribute<bool>(SystemCustomerAttributeNames.HasConsentedToGdpr);

			if (hasConsentedToGdpr)
			{
				return new EmptyResult();
			}

			var model = new GdprConsentModel();
			model.GdprConsent = false;
			model.SmallDisplay = isSmall;

			return PartialView(model);
		}

        #endregion
    }
}
