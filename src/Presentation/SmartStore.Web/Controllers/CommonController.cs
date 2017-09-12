using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
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
using SmartStore.Core.Search;
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
using SmartStore.Services.Search;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Localization;
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

		private readonly ICommonServices _services;
		private readonly ITopicService _topicService;
        private readonly Lazy<ILanguageService> _languageService;
        private readonly Lazy<ICurrencyService> _currencyService;
        private readonly IThemeContext _themeContext;
        private readonly Lazy<IThemeRegistry> _themeRegistry;
        private readonly Lazy<IForumService> _forumservice;
        private readonly Lazy<IGenericAttributeService> _genericAttributeService;
		private readonly Lazy<ICompareProductsService> _compareProductsService;
		private readonly Lazy<IUrlRecordService> _urlRecordService;
		private readonly Lazy<ICatalogSearchService> _catalogSearchService;

		private readonly StoreInformationSettings _storeInfoSettings;
		private readonly CustomerSettings _customerSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ThemeSettings _themeSettings;
        private readonly CommonSettings _commonSettings;
		private readonly NewsSettings _newsSettings;
        private readonly BlogSettings _blogSettings;
        private readonly ForumSettings _forumSettings;
        private readonly LocalizationSettings _localizationSettings;
		private readonly Lazy<SecuritySettings> _securitySettings;
		private readonly Lazy<SocialSettings> _socialSettings;
		private readonly Lazy<MediaSettings> _mediaSettings;
		private readonly Lazy<SearchSettings> _searchSettings;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
		
        private readonly IPriceFormatter _priceFormatter;
		private readonly IPageAssetsBuilder _pageAssetsBuilder;
		private readonly Lazy<IPictureService> _pictureService;
		private readonly Lazy<IManufacturerService> _manufacturerService;
		private readonly Lazy<ICategoryService> _categoryService;
		private readonly Lazy<IProductService> _productService;
        private readonly Lazy<IShoppingCartService> _shoppingCartService;

		private readonly IBreadcrumb _breadcrumb;

		public CommonController(
			ICommonServices services,
			ITopicService topicService,
            Lazy<ILanguageService> languageService,
            Lazy<ICurrencyService> currencyService,
			IThemeContext themeContext,
            Lazy<IThemeRegistry> themeRegistry, 
			Lazy<IForumService> forumService,
            Lazy<IGenericAttributeService> genericAttributeService, 
			Lazy<IMobileDeviceHelper> mobileDeviceHelper,
			Lazy<ICompareProductsService> compareProductsService,
			Lazy<IUrlRecordService> urlRecordService,
			Lazy<ICatalogSearchService> catalogSearchService,
			StoreInformationSettings storeInfoSettings,
            CustomerSettings customerSettings, 
            TaxSettings taxSettings, 
			CatalogSettings catalogSettings,
            ShoppingCartSettings shoppingCartSettings,
            EmailAccountSettings emailAccountSettings,
            CommonSettings commonSettings, 
			NewsSettings newsSettings,
			BlogSettings blogSettings, 
			ForumSettings forumSettings,
            LocalizationSettings localizationSettings, 
			Lazy<SecuritySettings> securitySettings,
			Lazy<SocialSettings> socialSettings,
			Lazy<MediaSettings> mediaSettings,
			Lazy<SearchSettings> searchSettings,
			IOrderTotalCalculationService orderTotalCalculationService, 
			IPriceFormatter priceFormatter,
            ThemeSettings themeSettings, 
			IPageAssetsBuilder pageAssetsBuilder,
			Lazy<IPictureService> pictureService,
			Lazy<IManufacturerService> manufacturerService,
			Lazy<ICategoryService> categoryService,
			Lazy<IProductService> productService,
            Lazy<IShoppingCartService> shoppingCartService,
			IBreadcrumb breadcrumb)
        {
			_services = services;
			_topicService = topicService;
            _languageService = languageService;
            _currencyService = currencyService;
            _themeContext = themeContext;
            _themeRegistry = themeRegistry;
            _forumservice = forumService;
            _genericAttributeService = genericAttributeService;
			_compareProductsService = compareProductsService;
			_urlRecordService = urlRecordService;
			_catalogSearchService = catalogSearchService;

			_storeInfoSettings = storeInfoSettings;
			_customerSettings = customerSettings;
            _taxSettings = taxSettings;
            _catalogSettings = catalogSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _commonSettings = commonSettings;
			_newsSettings = newsSettings;
            _blogSettings = blogSettings;
            _forumSettings = forumSettings;
            _localizationSettings = localizationSettings;
			_securitySettings = securitySettings;
			_socialSettings = socialSettings;
			_mediaSettings = mediaSettings;
			_searchSettings = searchSettings;

            _orderTotalCalculationService = orderTotalCalculationService;
            _priceFormatter = priceFormatter;

            _themeSettings = themeSettings;
			_pageAssetsBuilder = pageAssetsBuilder;
			_pictureService = pictureService;
			_manufacturerService = manufacturerService;
			_categoryService = categoryService;
			_productService = productService;
            _shoppingCartService = shoppingCartService;

			_breadcrumb = breadcrumb;
        }

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
                        NativeName = LocalizationHelper.GetLanguageNativeName(x.LanguageCulture) ?? x.Name,
                        ISOCode = x.LanguageCulture,
                        SeoCode = x.UniqueSeoCode,
                        FlagImageFileName = x.FlagImageFileName
                    })
                    .ToList();
                return result;
            });

			var workingLanguage = _services.WorkContext.WorkingLanguage;

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

				object val;
				if (!routeValues.TryGetValue(controller + "id", out val))
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
			var availableCurrencies = _services.Cache.Get(string.Format(ModelCacheEventConsumer.AVAILABLE_CURRENCIES_MODEL_KEY, _services.WorkContext.WorkingLanguage.Id, _services.StoreContext.CurrentStore.Id), () =>
            {
                var result = _currencyService.Value
					.GetAllCurrencies(storeId: _services.StoreContext.CurrentStore.Id)
                    .Select(x => new CurrencyModel()
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
				CurrentCurrencyId = _services.WorkContext.WorkingCurrency.Id,
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
				CurrentTaxType = _services.WorkContext.TaxDisplayType
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
				var host = _services.WebHelper.GetStoreLocation();
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
                Size logoSize = Size.Empty;
                if (picture != null)
                {
                    logoUrl = pictureService.GetPictureUrl(picture);
					if (picture.Width.HasValue && picture.Height.HasValue)
					{
						logoSize = new Size(picture.Width.Value, picture.Height.Value);
					}
					else
					{
						logoSize = pictureService.GetPictureSize(picture);
					} 
                }

                return new ShopHeaderModel
                {
                    LogoUploaded = picture != null && logoUrl.HasValue(),
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

            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                var helper = new LocalizedUrlHelper(HttpContext.Request.ApplicationPath, returnUrl, false);
				helper.PrependSeoCode(_services.WorkContext.WorkingLanguage.UniqueSeoCode, true);
                returnUrl = helper.GetAbsolutePath();
            }

            return RedirectToReferrer(returnUrl);
        }

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
			{
				_services.WorkContext.WorkingCurrency = currency;
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
			_services.WorkContext.TaxDisplayType = taxDisplayType;

            return RedirectToReferrer(returnUrl);
        }

        // footer
        [ChildActionOnly]
        public ActionResult JavaScriptDisabledWarning()
        {
            if (!_commonSettings.DisplayJavaScriptDisabledWarning)
                return Content("");

            return PartialView();
        }

        [ChildActionOnly]
        public ActionResult ShopBar()
        {
			var customer = _services.WorkContext.CurrentCustomer;
			var isAdmin = customer.IsAdmin();
			var isRegistered = isAdmin || customer.IsRegistered();
            
            var model = new ShopBarModel
            {
                IsAuthenticated = isRegistered,
                CustomerEmailUsername = isRegistered ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
				IsCustomerImpersonated = _services.WorkContext.OriginalCustomerIfImpersonated != null,
				DisplayAdminLink = _services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel),
				ShoppingCartEnabled = _services.Permissions.Authorize(StandardPermissionProvider.EnableShoppingCart) && _shoppingCartSettings.MiniShoppingCartEnabled,
				WishlistEnabled = _services.Permissions.Authorize(StandardPermissionProvider.EnableWishlist),
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled            
            };

			return PartialView(model);
        }

		[ChildActionOnly]
        public ActionResult Footer()
        {
			var store = _services.StoreContext.CurrentStore;
			var allTopics = _topicService.GetAllTopics(store.Id);
			var taxDisplayType = _services.WorkContext.GetTaxDisplayTypeFor(_services.WorkContext.CurrentCustomer, store.Id);

			var taxInfo = T(taxDisplayType == TaxDisplayType.IncludingTax ? "Tax.InclVAT" : "Tax.ExclVAT");

			var availableStoreThemes = !_themeSettings.AllowCustomerToSelectTheme ? new List<StoreThemeModel>() : _themeRegistry.Value.GetThemeManifests()
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
                BlogEnabled = _blogSettings.Enabled,                          
                ForumEnabled = _forumSettings.ForumsEnabled,
                HideNewsletterBlock = _customerSettings.HideNewsletterBlock,
                RecentlyAddedProductsEnabled = _catalogSettings.RecentlyAddedProductsEnabled,
                RecentlyViewedProductsEnabled = _catalogSettings.RecentlyViewedProductsEnabled,
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled,
                ManufacturerEnabled = _manufacturerService.Value.GetAllManufacturers(String.Empty, 0, 0).TotalCount > 0,
                DisplayLoginLink = _customerSettings.UserRegistrationType == UserRegistrationType.Disabled
            };

			model.TopicPageUrls = allTopics
				.Where(x => !x.RenderAsWidget)
				.GroupBy(x => x.SystemName)
				.ToDictionary(x => x.Key.EmptyNull().ToLower(), x =>
				{
					if (x.Key.IsCaseInsensitiveEqual("contactus"))
						return Url.RouteUrl("ContactUs");

					return Url.RouteUrl("Topic", new { SystemName = x.Key });
				});

			if (model.TopicPageUrls.ContainsKey("shippinginfo"))
			{
				model.LegalInfo = T("Tax.LegalInfoFooter", taxInfo, model.TopicPageUrls["shippinginfo"]);
			}
			else
			{
				model.LegalInfo = T("Tax.LegalInfoFooter2", taxInfo);
			}

			var hint = _services.Settings.GetSettingByKey<string>("Rnd_SmCopyrightHint", string.Empty, store.Id);
			if (hint.IsEmpty())
			{
				hint = s_hints[new Random().Next(s_hints.Length)];
				_services.Settings.SetSetting<string>("Rnd_SmCopyrightHint", hint, store.Id);
			}

            model.ShowSocialLinks = _socialSettings.Value.ShowSocialLinksInFooter;
            model.FacebookLink = _socialSettings.Value.FacebookLink;
            model.GooglePlusLink = _socialSettings.Value.GooglePlusLink;
            model.TwitterLink = _socialSettings.Value.TwitterLink;
            model.PinterestLink = _socialSettings.Value.PinterestLink;
            model.YoutubeLink = _socialSettings.Value.YoutubeLink;

			model.SmartStoreHint = "<a href='http://www.smartstore.com/' class='sm-hint' target='_blank'><strong>{0}</strong></a> by SmartStore AG &copy; {1}"
				.FormatCurrent(hint, DateTime.Now.Year);

            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult Menu()
        {
			var store = _services.StoreContext.CurrentStore;
			var customer = _services.WorkContext.CurrentCustomer;

            var model = new MenuModel
            {
                RecentlyAddedProductsEnabled = _catalogSettings.RecentlyAddedProductsEnabled,
				NewsEnabled = _newsSettings.Enabled,
                BlogEnabled = _blogSettings.Enabled,
                ForumEnabled = _forumSettings.ForumsEnabled,
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
				IsCustomerImpersonated = _services.WorkContext.OriginalCustomerIfImpersonated != null,
                IsAuthenticated = customer.IsRegistered(),
				DisplayAdminLink = _services.Permissions.Authorize(StandardPermissionProvider.AccessAdminPanel),
				HasContactUsPage = _topicService.GetTopicBySystemName("ContactUs", store.Id) != null,
                DisplayLoginLink = _customerSettings.UserRegistrationType != UserRegistrationType.Disabled
            };
            
            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult ServiceMenu()
        {
            var store = _services.StoreContext.CurrentStore;
            var allTopics = _topicService.GetAllTopics(store.Id);

            var model = new ServiceMenuModel
            {
                RecentlyAddedProductsEnabled = _catalogSettings.RecentlyAddedProductsEnabled,
                RecentlyViewedProductsEnabled = _catalogSettings.RecentlyViewedProductsEnabled,
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled,
                BlogEnabled = _blogSettings.Enabled,
                ForumEnabled = _forumSettings.ForumsEnabled,
                ManufacturerEnabled = _manufacturerService.Value.GetAllManufacturers(String.Empty, 0, 0).TotalCount > 0
            };

            model.TopicPageUrls = allTopics
                .Where(x => !x.RenderAsWidget)
                .GroupBy(x => x.SystemName)
                .ToDictionary(x => x.Key.EmptyNull().ToLower(), x =>
                {
                    if (x.Key.IsCaseInsensitiveEqual("contactus"))
                        return Url.RouteUrl("ContactUs");

                    return Url.RouteUrl("Topic", new { SystemName = x.Key });
                });

            return PartialView(model);
        }

		[ChildActionOnly]
		public ActionResult Breadcrumb()
		{
			if (_breadcrumb.Trail == null || _breadcrumb.Trail.Count == 0)
			{
				return Content("");
			}

			return PartialView(_breadcrumb.Trail);
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

        public ActionResult RobotsTextFile()
        {
            var disallowPaths = new List<string>()
            {
                "/bin/",
                "/Content/files/",
                "/Content/files/ExportImport/",
				"/Exchange/",
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
				"/Settings",
				"/Login",
				"/Register"
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
				//ShoppingCartItems = customer.CountProductsInCart(ShoppingCartType.ShoppingCart, _services.StoreContext.CurrentStore.Id),
				WishlistEnabled = _services.Permissions.Authorize(StandardPermissionProvider.EnableWishlist),
				//WishlistItems = customer.CountProductsInCart(ShoppingCartType.Wishlist, _services.StoreContext.CurrentStore.Id),
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
			}, TimeSpan.FromMinutes(1) /* 1 min. (just for the duration of pdf processing) */);
		}

		#endregion

		#region Entity Picker

		public ActionResult EntityPicker(EntityPickerModel model)
		{
            model.PageSize = 48; // _commonSettings.EntityPickerPageSize;
			model.AllString = T("Admin.Common.All");

			if (model.Entity.IsCaseInsensitiveEqual("product"))
			{
				var allCategories = _categoryService.Value.GetAllCategories(showHidden: true);
				var mappedCategories = allCategories.ToDictionary(x => x.Id);

				model.AvailableCategories = allCategories
					.Select(x => new SelectListItem { Text = x.GetCategoryNameWithPrefix(_categoryService.Value, mappedCategories), Value = x.Id.ToString() })
					.ToList();

				model.AvailableManufacturers = _manufacturerService.Value.GetAllManufacturers(true)
					.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
					.ToList();

				model.AvailableStores = _services.StoreService.GetAllStores()
					.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
					.ToList();

				model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
			}

			return PartialView(model);
		}

		[HttpPost]
		public ActionResult EntityPicker(EntityPickerModel model, FormCollection form)
		{
            model.PageSize = 48; // _commonSettings.EntityPickerPageSize;
			model.PublishedString = T("Common.Published");
			model.UnpublishedString = T("Common.Unpublished");

			try
			{
				var disableIf = model.DisableIf.SplitSafe(",").Select(x => x.ToLower().Trim()).ToList();
				var disableIds = model.DisableIds.SplitSafe(",").Select(x => x.ToInt()).ToList();

				using (var scope = new DbContextScope(_services.DbContext, autoDetectChanges: false, proxyCreation: true, validateOnSave: false, forceNoTracking: true))
				{
					if (model.Entity.IsCaseInsensitiveEqual("product"))
					{
						#region Product

						model.SearchTerm = model.ProductName.TrimSafe();

						var hasPermission = _services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog);
						var storeLocation = _services.WebHelper.GetStoreLocation(false);
						var disableIfNotSimpleProduct = disableIf.Contains("notsimpleproduct");
						var disableIfGroupedProduct = disableIf.Contains("groupedproduct");
						var labelTextGrouped = T("Admin.Catalog.Products.ProductType.GroupedProduct.Label").Text;
						var labelTextBundled = T("Admin.Catalog.Products.ProductType.BundledProduct.Label").Text;
						var sku = T("Products.Sku").Text;

						var fields = new List<string> { "name" };
						if (_searchSettings.Value.SearchFields.Contains("sku"))
							fields.Add("sku");
						if (_searchSettings.Value.SearchFields.Contains("shortdescription"))
							fields.Add("shortdescription");

						var searchQuery = new CatalogSearchQuery(fields.ToArray(), model.SearchTerm)
							.HasStoreId(model.StoreId);

						if (!hasPermission)
							searchQuery = searchQuery.VisibleOnly(_services.WorkContext.CurrentCustomer);

						if (model.ProductTypeId > 0)
							searchQuery = searchQuery.IsProductType((ProductType)model.ProductTypeId);

						if (model.ManufacturerId != 0)
							searchQuery = searchQuery.WithManufacturerIds(null, model.ManufacturerId);

						if (model.CategoryId != 0)
							searchQuery = searchQuery.WithCategoryIds(null, model.CategoryId);

						var query = _catalogSearchService.Value.PrepareQuery(searchQuery);

						var products = query
							.Select(x => new
							{
								x.Id,
								x.Sku,
								x.Name,
								x.Published,
								x.ProductTypeId
							})
							.OrderBy(x => x.Name)
							.Skip(model.PageIndex * model.PageSize)
							.Take(model.PageSize)
							.ToList();

						var productIds = products.Select(x => x.Id).ToArray();
						var pictures = _productService.Value.GetProductPicturesByProductIds(productIds, true);

						model.SearchResult = products
							.Select(x =>
							{
								var item = new EntityPickerModel.SearchResultModel
								{
									Id = x.Id,
									ReturnValue = (model.ReturnField.IsCaseInsensitiveEqual("sku") ? x.Sku : x.Id.ToString()),
									Title = x.Name,
									Summary = x.Sku,
									SummaryTitle = "{0}: {1}".FormatInvariant(sku, x.Sku.NaIfEmpty()),
									Published = (hasPermission ? x.Published : (bool?)null)
								};

								if (disableIfNotSimpleProduct)
								{
									item.Disable = (x.ProductTypeId != (int)ProductType.SimpleProduct);
								}
								else if (disableIfGroupedProduct)
								{
									item.Disable = (x.ProductTypeId == (int)ProductType.GroupedProduct);
								}

								if (!item.Disable && disableIds.Contains(x.Id))
								{
									item.Disable = true;
								}

								if (x.ProductTypeId == (int)ProductType.GroupedProduct)
								{
									item.LabelText = labelTextGrouped;
									item.LabelClassName = "badge-success";
								}
								else if (x.ProductTypeId == (int)ProductType.BundledProduct)
								{
									item.LabelText = labelTextBundled;
									item.LabelClassName = "badge-info";
								}

								var productPicture = pictures.FirstOrDefault(y => y.Key == x.Id);
								if (productPicture.Value != null)
								{
									var picture = productPicture.Value.FirstOrDefault();
									if (picture != null)
									{
										try
										{
											item.ImageUrl = _pictureService.Value.GetPictureUrl(
												picture.Picture,
												_mediaSettings.Value.ProductThumbPictureSizeOnProductDetailsPage,
												!_catalogSettings.HideProductDefaultPictures,
												storeLocation);
										}
										catch (Exception exception)
										{
											exception.Dump();
										}
									}
								}

								return item;
							})
							.ToList();

						#endregion
					}
				}
			}
			catch (Exception exception)
			{
				NotifyError(exception.ToAllMessages());
			}

			return PartialView("EntityPickerList", model);
		}

		#endregion
	}
}
