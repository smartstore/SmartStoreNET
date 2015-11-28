using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Core.Themes;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Forums;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Pdf;
using SmartStore.Web.Framework.Themes;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Common;

namespace SmartStore.Web.Controllers
{
    public partial class CommonController : PublicControllerBase
    {
        #region Fields

        private readonly ITopicService _topicService;
        private readonly Lazy<ILanguageService> _languageService;
        private readonly Lazy<ICurrencyService> _currencyService;
        private readonly IThemeContext _themeContext;
        private readonly Lazy<IThemeRegistry> _themeRegistry;
        private readonly Lazy<IForumService> _forumservice;
        private readonly Lazy<IGenericAttributeService> _genericAttributeService;
        private readonly Lazy<IMobileDeviceHelper> _mobileDeviceHelper;

		private readonly static string[] s_hints = new string[] { "Shopsystem", "Onlineshop Software", "Shopsoftware", "E-Commerce Solution" };

        private readonly CustomerSettings _customerSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ThemeSettings _themeSettings;
        private readonly CommonSettings _commonSettings;
		private readonly NewsSettings _newsSettings;
        private readonly BlogSettings _blogSettings;
        private readonly ForumSettings _forumSettings;
        private readonly LocalizationSettings _localizationSettings;
		private readonly Lazy<SecuritySettings> _securitySettings;

        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceFormatter _priceFormatter;
		private readonly IPageAssetsBuilder _pageAssetsBuilder;
		private readonly Lazy<IPictureService> _pictureService;
		private readonly ICommonServices _services;

        #endregion

        #region Constructors

        public CommonController(
			ITopicService topicService,
            Lazy<ILanguageService> languageService,
            Lazy<ICurrencyService> currencyService,
			IThemeContext themeContext,
            Lazy<IThemeRegistry> themeRegistry, 
			Lazy<IForumService> forumService,
            Lazy<IGenericAttributeService> genericAttributeService, 
			Lazy<IMobileDeviceHelper> mobileDeviceHelper,
			CustomerSettings customerSettings, 
            TaxSettings taxSettings, 
			CatalogSettings catalogSettings,
            EmailAccountSettings emailAccountSettings,
            CommonSettings commonSettings, 
			NewsSettings newsSettings,
			BlogSettings blogSettings, 
			ForumSettings forumSettings,
            LocalizationSettings localizationSettings, 
			Lazy<SecuritySettings> securitySettings,
            IOrderTotalCalculationService orderTotalCalculationService, 
			IPriceFormatter priceFormatter,
            ThemeSettings themeSettings, 
			IPageAssetsBuilder pageAssetsBuilder,
			Lazy<IPictureService> pictureService,
			ICommonServices services)
        {
            this._topicService = topicService;
            this._languageService = languageService;
            this._currencyService = currencyService;
            this._themeContext = themeContext;
            this._themeRegistry = themeRegistry;
            this._forumservice = forumService;
            this._genericAttributeService = genericAttributeService;
            this._mobileDeviceHelper = mobileDeviceHelper;
			
            this._customerSettings = customerSettings;
            this._taxSettings = taxSettings;
            this._catalogSettings = catalogSettings;
            this._commonSettings = commonSettings;
			this._newsSettings = newsSettings;
            this._blogSettings = blogSettings;
            this._forumSettings = forumSettings;
            this._localizationSettings = localizationSettings;
			this._securitySettings = securitySettings;

            this._orderTotalCalculationService = orderTotalCalculationService;
            this._priceFormatter = priceFormatter;

            this._themeSettings = themeSettings;
			this._pageAssetsBuilder = pageAssetsBuilder;
			this._pictureService = pictureService;
			this._services = services;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected LanguageSelectorModel PrepareLanguageSelectorModel()
        {
			var availableLanguages = _services.Cache.Get(string.Format(ModelCacheEventConsumer.AVAILABLE_LANGUAGES_MODEL_KEY, _services.StoreContext.CurrentStore.Id), () =>
            {
                var result = _languageService.Value
					.GetAllLanguages(storeId: _services.StoreContext.CurrentStore.Id)
                    .Select(x => new LanguageModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        NativeName = GetLanguageNativeName(x.LanguageCulture) ?? x.Name,
                        ISOCode = x.LanguageCulture,
                        SeoCode = x.UniqueSeoCode,
                        FlagImageFileName = x.FlagImageFileName
                    })
                    .ToList();
                return result;
            });

			var workingLanguage = _services.WorkContext.WorkingLanguage;

            var model = new LanguageSelectorModel()
            {
                CurrentLanguageId = workingLanguage.Id,
                AvailableLanguages = availableLanguages,
                UseImages = _localizationSettings.UseImagesForLanguageSelection
            };

			string defaultSeoCode = _languageService.Value.GetDefaultLanguageSeoCode();

            foreach (var lang in model.AvailableLanguages)
            {
                var helper = new LocalizedUrlHelper(HttpContext.Request, true);

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

                model.ReturnUrls[lang.SeoCode] = helper.GetAbsolutePath();
            }

            return model;
        }

        // TODO: (MC) zentral auslagern
        private string GetLanguageNativeName(string locale)
        {
            try
            {
                if (!string.IsNullOrEmpty(locale))
                {
                    var info = CultureInfo.GetCultureInfoByIetfLanguageTag(locale);
                    if (info == null)
                    {
                        return null;
                    }
                    return info.NativeName;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        [NonAction]
        protected CurrencySelectorModel PrepareCurrencySelectorModel()
        {
			var availableCurrencies = _services.Cache.Get(string.Format(ModelCacheEventConsumer.AVAILABLE_CURRENCIES_MODEL_KEY, _services.WorkContext.WorkingLanguage.Id, _services.StoreContext.CurrentStore.Id), () =>
            {
                var result = _currencyService.Value
					.GetAllCurrencies(storeId: _services.StoreContext.CurrentStore.Id)
                    .Select(x => new CurrencyModel()
                    {
                        Id = x.Id,
                        Name = x.GetLocalized(y => y.Name),
                        ISOCode = x.CurrencyCode,
                        Symbol = GetCurrencySymbol(x.DisplayLocale) ?? x.CurrencyCode
                    })
                    .ToList();
                return result;
            });

            var model = new CurrencySelectorModel()
            {
				CurrentCurrencyId = _services.WorkContext.WorkingCurrency.Id,
                AvailableCurrencies = availableCurrencies
            };
            return model;
        }

        // TODO: Zentral auslagern
        private static string GetCurrencySymbol(string locale)
        {
            try
            {
                if (!string.IsNullOrEmpty(locale))
                {
                    var info = new RegionInfo(locale);
                    if (info == null)
                    {
                        return null;
                    }
                    return info.CurrencySymbol;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        [NonAction]
        protected TaxTypeSelectorModel PrepareTaxTypeSelectorModel()
        {
            var model = new TaxTypeSelectorModel()
            {
                Enabled = _taxSettings.AllowCustomersToSelectTaxDisplayType,
				CurrentTaxType = _services.WorkContext.TaxDisplayType
            };
            return model;
        }

        [NonAction]
        protected int GetUnreadPrivateMessages()
        {
            var result = 0;
			var customer = _services.WorkContext.CurrentCustomer;
            if (_forumSettings.AllowPrivateMessages && !customer.IsGuest())
            {
				var privateMessages = _forumservice.Value.GetAllPrivateMessages(_services.StoreContext.CurrentStore.Id, 0, customer.Id, false, null, false, string.Empty, 0, 1);

                if (privateMessages.TotalCount > 0)
                {
                    result = privateMessages.TotalCount;
                }
            }

            return result;
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
				var host = _services.WebHelper.GetStoreLocation();
				foreach (var lang in model.AvailableLanguages)
				{
					_pageAssetsBuilder.AddLinkPart("alternate", host + model.ReturnUrls[lang.SeoCode].TrimStart('/'), hreflang: lang.SeoCode);
				}
			}

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult Header()
        {
			var model = _services.Cache.Get(ModelCacheEventConsumer.SHOPHEADER_MODEL_KEY.FormatWith(_services.StoreContext.CurrentStore.Id), () =>
			{
                var pictureService = _pictureService.Value;
				int logoPictureId = _services.StoreContext.CurrentStore.LogoPictureId;

                Picture picture = null;
                if (logoPictureId > 0)
                {
                    picture = pictureService.GetPictureById(logoPictureId);
                }

                string logoUrl = null;
                var logoSize = new Size();
                if (picture != null)
                {
                    logoUrl = pictureService.GetPictureUrl(picture);
                    logoSize = pictureService.GetPictureSize(picture);
                }

                return new ShopHeaderModel()
                {
                    LogoUploaded = picture != null,
                    LogoUrl = logoUrl,
                    LogoWidth = logoSize.Width,
                    LogoHeight = logoSize.Height,
					LogoTitle = _services.StoreContext.CurrentStore.Name
                };
            });
            

            return PartialView(model);
        }

        public ActionResult SetLanguage(int langid, string returnUrl = "")
        {
			var language = _languageService.Value.GetLanguageById(langid);
            if (language != null && language.Published)
            {
				_services.WorkContext.WorkingLanguage = language;
            }
            
            // url referrer
            if (String.IsNullOrEmpty(returnUrl))
            {
				returnUrl = _services.WebHelper.GetUrlReferrer();
            }

            // home page
            if (String.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.RouteUrl("HomePage");
            }

            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                var helper = new LocalizedUrlHelper(HttpContext.Request.ApplicationPath, returnUrl, true);
				helper.PrependSeoCode(_services.WorkContext.WorkingLanguage.UniqueSeoCode, true);
                returnUrl = helper.GetAbsolutePath();
            }

            return Redirect(returnUrl);
        }

        //currency
        [ChildActionOnly]
        public ActionResult CurrencySelector()
        {
            var model = PrepareCurrencySelectorModel();

			if (model.AvailableCurrencies.Count < 2)
				return Content("");

            return PartialView(model);
        }
        public ActionResult CurrencySelected(int customerCurrency, string returnUrl = "")
        {
			var currency = _currencyService.Value.GetCurrencyById(customerCurrency);
            if (currency != null)
				_services.WorkContext.WorkingCurrency = currency;

            //url referrer
            if (String.IsNullOrEmpty(returnUrl))
				returnUrl = _services.WebHelper.GetUrlReferrer();
            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");
            return Redirect(returnUrl);
        }

        //tax type
        [ChildActionOnly]
        public ActionResult TaxTypeSelector()
        {
            var model = PrepareTaxTypeSelectorModel();
            return PartialView(model);
        }
        public ActionResult TaxTypeSelected(int customerTaxType, string returnUrl = "")
        {
            var taxDisplayType = (TaxDisplayType)Enum.ToObject(typeof(TaxDisplayType), customerTaxType);
			_services.WorkContext.TaxDisplayType = taxDisplayType;

            //url referrer
            if (String.IsNullOrEmpty(returnUrl))
				returnUrl = _services.WebHelper.GetUrlReferrer();
            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");
            return Redirect(returnUrl);
        }

        //Configuration page (used on mobile devices)
        [ChildActionOnly]
        public ActionResult ConfigButton()
        {
            var langModel = PrepareLanguageSelectorModel();
            var currModel = PrepareCurrencySelectorModel();
            var taxModel = PrepareTaxTypeSelectorModel();
            //should we display the button?
            if (langModel.AvailableLanguages.Count > 1 ||
                currModel.AvailableCurrencies.Count > 1 ||
                taxModel.Enabled)
                return PartialView();
            else
                return Content("");
        }

        public ActionResult Settings()
        {
            return View();
        }

        // footer
        [ChildActionOnly]
        public ActionResult JavaScriptDisabledWarning()
        {
            if (!_commonSettings.DisplayJavaScriptDisabledWarning)
                return Content("");

            return PartialView();
        }

        // header links
        [ChildActionOnly]
        public ActionResult HeaderLinks()
        {
			var customer = _services.WorkContext.CurrentCustomer;

            var unreadMessageCount = GetUnreadPrivateMessages();
            var unreadMessage = string.Empty;
            var alertMessage = string.Empty;
            if (unreadMessageCount > 0)
            {
                unreadMessage = T("PrivateMessages.TotalUnread", unreadMessageCount);

                //notifications here
				var notifiedAboutNewPrivateMessagesAttributeKey = string.Format(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, _services.StoreContext.CurrentStore.Id);
				if (_forumSettings.ShowAlertForPM &&
					!customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, _services.StoreContext.CurrentStore.Id))
                {
					_genericAttributeService.Value.SaveAttribute(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true, _services.StoreContext.CurrentStore.Id);
					alertMessage = T("PrivateMessages.YouHaveUnreadPM", unreadMessageCount);
                }
            }

            var model = new HeaderLinksModel
            {
                IsAuthenticated = customer.IsRegistered(),
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
				IsCustomerImpersonated = _services.WorkContext.OriginalCustomerIfImpersonated != null,
                DisplayAdminLink = _services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel),
				ShoppingCartEnabled = _services.Permissions.Authorize(StandardPermissionProvider.EnableShoppingCart),
				ShoppingCartItems = customer.CountProductsInCart(ShoppingCartType.ShoppingCart, _services.StoreContext.CurrentStore.Id),
				WishlistEnabled = _services.Permissions.Authorize(StandardPermissionProvider.EnableWishlist),
				WishlistItems = customer.CountProductsInCart(ShoppingCartType.Wishlist, _services.StoreContext.CurrentStore.Id),
                AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                UnreadPrivateMessages = unreadMessage,
                AlertMessage = alertMessage,
            };

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult ShopBar()
        {
			var customer = _services.WorkContext.CurrentCustomer;

            var unreadMessageCount = GetUnreadPrivateMessages();
            var unreadMessage = string.Empty;
            var alertMessage = string.Empty;
            if (unreadMessageCount > 0)
            {
                unreadMessage = T("PrivateMessages.TotalUnread");

                //notifications here
                if (_forumSettings.ShowAlertForPM &&
					!customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, _services.StoreContext.CurrentStore.Id))
                {
					_genericAttributeService.Value.SaveAttribute(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true, _services.StoreContext.CurrentStore.Id);
                    alertMessage = T("PrivateMessages.YouHaveUnreadPM", unreadMessageCount);
                }
            }

            //subtotal
			decimal subtotal = 0;
			var cart = _services.WorkContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _services.StoreContext.CurrentStore.Id);

            if (cart.Count > 0)
            {
                decimal subtotalBase = decimal.Zero;
                decimal orderSubTotalDiscountAmountBase = decimal.Zero;
                Discount orderSubTotalAppliedDiscount = null;
                decimal subTotalWithoutDiscountBase = decimal.Zero;
                decimal subTotalWithDiscountBase = decimal.Zero;

                _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

                subtotalBase = subTotalWithoutDiscountBase;
				subtotal = _currencyService.Value.ConvertFromPrimaryStoreCurrency(subtotalBase, _services.WorkContext.WorkingCurrency);
            }
            var model = new ShopBarModel
            {
                IsAuthenticated = customer.IsRegistered(),
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
				IsCustomerImpersonated = _services.WorkContext.OriginalCustomerIfImpersonated != null,
				DisplayAdminLink = _services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel),
				ShoppingCartEnabled = _services.Permissions.Authorize(StandardPermissionProvider.EnableShoppingCart),
                ShoppingCartAmount = _priceFormatter.FormatPrice(subtotal, true, false),
				WishlistEnabled = _services.Permissions.Authorize(StandardPermissionProvider.EnableWishlist),
                AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                UnreadPrivateMessages = unreadMessage,
                AlertMessage = alertMessage,
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled            
            };

            if (model.ShoppingCartEnabled || model.WishlistEnabled)
            {
				if (model.ShoppingCartEnabled)
					model.ShoppingCartItems = cart.GetTotalProducts();

                if (model.WishlistEnabled)
					model.WishlistItems = customer.CountProductsInCart(ShoppingCartType.Wishlist, _services.StoreContext.CurrentStore.Id);
            }

            if (_catalogSettings.CompareProductsEnabled)
            {
                model.CompareItems = EngineContext.Current.Resolve<ICompareProductsService>().GetComparedProductsCount();
            }

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult Footer()
        {
			string taxInfo = (_services.WorkContext.GetTaxDisplayTypeFor(_services.WorkContext.CurrentCustomer, _services.StoreContext.CurrentStore.Id) == TaxDisplayType.IncludingTax)
                ? T("Tax.InclVAT") 
                : T("Tax.ExclVAT");

            string shippingInfoLink = Url.RouteUrl("Topic", new { SystemName = "shippinginfo" });
			var store = _services.StoreContext.CurrentStore;

			var availableStoreThemes = !_themeSettings.AllowCustomerToSelectTheme ? new List<StoreThemeModel>() : _themeRegistry.Value.GetThemeManifests()
                .Where(x => !x.MobileTheme)
                .Select(x =>
                {
                    return new StoreThemeModel()
                    {
                        Name = x.ThemeName,
                        Title = x.ThemeTitle
                    };
                })
                .ToList();

            var model = new FooterModel
            {
				StoreName = store.Name,
				LegalInfo = T("Tax.LegalInfoFooter", taxInfo, shippingInfoLink),
                ShowLegalInfo = _taxSettings.ShowLegalHintsInFooter,
                ShowThemeSelector = availableStoreThemes.Count > 1,          
                BlogEnabled = _blogSettings.Enabled,                          
                ForumEnabled = _forumSettings.ForumsEnabled,
                HideNewsletterBlock = _customerSettings.HideNewsletterBlock,
            };

			var hint = _services.Settings.GetSettingByKey<string>("Rnd_SmCopyrightHint", string.Empty, store.Id);
			if (hint.IsEmpty())
			{
				hint = s_hints[new Random().Next(s_hints.Length)];
				_services.Settings.SetSetting<string>("Rnd_SmCopyrightHint", hint, store.Id);
			}

            var topics = new string[] { "paymentinfo", "imprint", "disclaimer" };
            foreach (var t in topics)
            {
				//load by store
				var topic = _topicService.GetTopicBySystemName(t, store.Id);
				if (topic == null)
					//not found. let's find topic assigned to all stores
					topic = _topicService.GetTopicBySystemName(t, 0);

                if (topic != null)
                {
                    model.Topics.Add(t, topic.Title);
                }
            }

            var socialSettings = EngineContext.Current.Resolve<SocialSettings>();

            model.ShowSocialLinks = socialSettings.ShowSocialLinksInFooter;
            model.FacebookLink = socialSettings.FacebookLink;
            model.GooglePlusLink = socialSettings.GooglePlusLink;
            model.TwitterLink = socialSettings.TwitterLink;
            model.PinterestLink = socialSettings.PinterestLink;
            model.YoutubeLink = socialSettings.YoutubeLink;
			model.SmartStoreHint = "<a href='http://www.smartstore.com/net' class='sm-hint' target='_blank'><strong>{0}</strong></a> by SmartStore AG &copy; {1}".FormatCurrent(hint, DateTime.Now.Year);

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult Menu()
        {
			var customer = _services.WorkContext.CurrentCustomer;

            var model = new MenuModel
            {
                RecentlyAddedProductsEnabled = _catalogSettings.RecentlyAddedProductsEnabled,
				NewsEnabled = _newsSettings.Enabled,
                BlogEnabled = _blogSettings.Enabled,
                ForumEnabled = _forumSettings.ForumsEnabled,
				AllowPrivateMessages = _forumSettings.AllowPrivateMessages && customer.IsRegistered(),
                UnreadPrivateMessages = GetUnreadPrivateMessages(),
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
				IsCustomerImpersonated = _services.WorkContext.OriginalCustomerIfImpersonated != null,
                IsAuthenticated = customer.IsRegistered(),
				DisplayAdminLink = _services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel),
            };

            return PartialView(model);
        }

        //info block
        [ChildActionOnly]
        public ActionResult InfoBlock()
        {
            var model = new InfoBlockModel
            {
                RecentlyAddedProductsEnabled = _catalogSettings.RecentlyAddedProductsEnabled,
                RecentlyViewedProductsEnabled = _catalogSettings.RecentlyViewedProductsEnabled,
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled,
                BlogEnabled = _blogSettings.Enabled,
                SitemapEnabled = _commonSettings.SitemapEnabled,
                ForumEnabled = _forumSettings.ForumsEnabled,
                AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
            };

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult StoreThemeSelector()
        {
            if (!_themeSettings.AllowCustomerToSelectTheme)
                return Content("");

            var model = new StoreThemeSelectorModel();
            var currentTheme = _themeRegistry.Value.GetThemeManifest(_themeContext.WorkingDesktopTheme);
            model.CurrentStoreTheme = new StoreThemeModel()
            {
                Name = currentTheme.ThemeName,
                Title = currentTheme.ThemeTitle
            };
			model.AvailableStoreThemes = _themeRegistry.Value.GetThemeManifests()
                //do not display themes for mobile devices
                .Where(x => !x.MobileTheme)
                .Select(x =>
                {
                    return new StoreThemeModel()
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

			_themeContext.WorkingDesktopTheme = themeName;

			if (HttpContext.Request.IsAjaxRequest())
			{
				return Json(new { Success = true });
			}

			if (returnUrl.IsEmpty())
			{
				return RedirectToRoute("HomePage");
			}

			return Redirect(returnUrl);
        }

        [ChildActionOnly]
        [OutputCache(Duration=3600, VaryByCustom="Theme_Store")]
        public ActionResult Favicon()
        {
            var icons = new string[] 
            { 
                "favicon-{0}.ico".FormatInvariant(_services.StoreContext.CurrentStore.Id), 
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

        /// <summary>
        /// Change presentation layer (desktop or mobile version)
        /// </summary>
        /// <param name="dontUseMobileVersion">True - use desktop version; false - use version for mobile devices</param>
        /// <returns>Action result</returns>
        [HttpPost]
        public ActionResult ChangeDevice(bool dontUseMobileVersion)
        {
			_genericAttributeService.Value.SaveAttribute(_services.WorkContext.CurrentCustomer,
				SystemCustomerAttributeNames.DontUseMobileVersion, dontUseMobileVersion, _services.StoreContext.CurrentStore.Id);

			string returnurl = _services.WebHelper.GetUrlReferrer();
            if (String.IsNullOrEmpty(returnurl))
                returnurl = Url.RouteUrl("HomePage");
            return Redirect(returnurl);
        }

        [ChildActionOnly]
        public ActionResult ChangeDeviceBlock()
        {
			if (!_mobileDeviceHelper.Value.MobileDevicesSupported())
                //mobile devices support is disabled
                return Content("");

			if (!_mobileDeviceHelper.Value.IsMobileDevice())
                //request is made by a desktop computer
                return Content("");

            return View();
        }

        public ActionResult RobotsTextFile()
        {
            var disallowPaths = new List<string>()
            {
                "/bin/",
                "/Content/files/",
                "/Content/files/ExportImport/",
                "/Country/GetStatesByCountryId",
                "/Install",
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
                "/Cart",
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
                "/Order",
                "/PasswordRecovery",
                "/Poll/Vote",
                "/ReturnRequest",
                "/Newsletter/Subscribe",
                "/Topic/Authenticate",
                "/Wishlist",
                "/Product/AskQuestion",
                "/Product/EmailAFriend",
				"/Search",
				"/Config",
				"/Settings"
            };


            const string newLine = "\r\n"; //Environment.NewLine
            var sb = new StringBuilder();
            sb.Append("User-agent: *");
            sb.Append(newLine);
			sb.AppendFormat("Sitemap: {0}", Url.RouteUrl("SitemapSEO", (object)null, _securitySettings.Value.ForceSslForAllPages ? "https" : "http"));
			sb.AppendLine();

			var disallows = disallowPaths.Concat(localizableDisallowPaths);

            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                // URLs are localizable. Append SEO code
				foreach (var language in _languageService.Value.GetAllLanguages(storeId: _services.StoreContext.CurrentStore.Id))
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
            Response.Write(sb.ToString());
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
            // seems that no entity was found
            return HttpNotFound();
        }

        [ChildActionOnly]
        public ActionResult AccountDropdown()
        {
			var customer = _services.WorkContext.CurrentCustomer;

            var unreadMessageCount = GetUnreadPrivateMessages();
            var unreadMessage = string.Empty;
            var alertMessage = string.Empty;
            if (unreadMessageCount > 0)
            {
                unreadMessage = unreadMessageCount.ToString();

                //notifications here
                if (_forumSettings.ShowAlertForPM &&
					!customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, _services.StoreContext.CurrentStore.Id))
                {
					_genericAttributeService.Value.SaveAttribute(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true, _services.StoreContext.CurrentStore.Id);
					alertMessage = T("PrivateMessages.YouHaveUnreadPM", unreadMessageCount);
                }
            }

            var model = new AccountDropdownModel
            {
                IsAuthenticated = customer.IsRegistered(),
				DisplayAdminLink = _services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel),
				ShoppingCartEnabled = _services.Permissions.Authorize(StandardPermissionProvider.EnableShoppingCart),
				ShoppingCartItems = customer.CountProductsInCart(ShoppingCartType.ShoppingCart, _services.StoreContext.CurrentStore.Id),
				WishlistEnabled = _services.Permissions.Authorize(StandardPermissionProvider.EnableWishlist),
				WishlistItems = customer.CountProductsInCart(ShoppingCartType.Wishlist, _services.StoreContext.CurrentStore.Id),
                AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                UnreadPrivateMessages = unreadMessage,
                AlertMessage = alertMessage
            };

            return PartialView(model);
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
			return _services.Cache.Get("PdfReceiptHeaderFooterModel-{0}".FormatInvariant(storeId), () =>
			{
				var model = new PdfReceiptHeaderFooterModel { StoreId = storeId };
				var store = _services.StoreService.GetStoreById(model.StoreId) ?? _services.StoreContext.CurrentStore;

				var companyInfoSettings = _services.Settings.LoadSetting<CompanyInformationSettings>(store.Id);
				var bankSettings = _services.Settings.LoadSetting<BankConnectionSettings>(store.Id);
				var contactSettings = _services.Settings.LoadSetting<ContactDataSettings>(store.Id);
				var pdfSettings = _services.Settings.LoadSetting<PdfSettings>(store.Id);

				model.StoreName = store.Name;
				model.StoreUrl = store.Url;

				var logoPicture = _pictureService.Value.GetPictureById(pdfSettings.LogoPictureId);
				if (logoPicture == null)
				{
					logoPicture = _pictureService.Value.GetPictureById(store.LogoPictureId);
				}

				if (logoPicture != null)
				{
					model.LogoUrl = _pictureService.Value.GetPictureUrl(logoPicture, showDefaultPicture: false);
				}

				model.MerchantCompanyInfo = companyInfoSettings;
				model.MerchantBankAccount = bankSettings;
				model.MerchantContactData = contactSettings;

				return model;			
			}, 1 /* 1 min. (just for the duration of pdf processing) */);
		}

        #endregion
    }
}
