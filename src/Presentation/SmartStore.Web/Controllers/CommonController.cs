using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Themes;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Forums;
using SmartStore.Services.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Topics;
using SmartStore.Web.Extensions;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Themes;
using SmartStore.Web.Framework.UI.Captcha;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Topics;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Services.Configuration;

namespace SmartStore.Web.Controllers
{
    public partial class CommonController : PublicControllerBase
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ITopicService _topicService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ISitemapGenerator _sitemapGenerator;
        private readonly IThemeContext _themeContext;
        private readonly IThemeRegistry _themeRegistry;
        private readonly IForumService _forumservice;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly IMobileDeviceHelper _mobileDeviceHelper;
        private readonly ICacheManager _cacheManager;
        private readonly ICustomerActivityService _customerActivityService;

		private readonly static string[] s_hints = new string[] { "Onlineshop", "Shopsystem", "Onlineshop Software", "Shopsoftware", "Webshop", "Ecommerce", "Ecommerce Solution", "Shopping Cart", "Internetshop", "Online Commerce", "Free Shopsoftware" };

        private readonly CustomerSettings _customerSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ThemeSettings _themeSettings;
        private readonly CommonSettings _commonSettings;
        private readonly BlogSettings _blogSettings;
        private readonly ForumSettings _forumSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;

        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceFormatter _priceFormatter;
		private readonly ISettingService _settingService;

        #endregion

        #region Constructors

        public CommonController(ICategoryService categoryService, IProductService productService,
            IManufacturerService manufacturerService, ITopicService topicService,
            ILanguageService languageService,
            ICurrencyService currencyService,
            IWorkContext workContext, IStoreContext storeContext,
            IQueuedEmailService queuedEmailService, IEmailAccountService emailAccountService,
            ISitemapGenerator sitemapGenerator, IThemeContext themeContext,
            IThemeRegistry themeRegistry, IForumService forumService,
            IGenericAttributeService genericAttributeService, IWebHelper webHelper,
            IPermissionService permissionService, IMobileDeviceHelper mobileDeviceHelper,
            ICacheManager cacheManager,
            ICustomerActivityService customerActivityService, CustomerSettings customerSettings, 
            TaxSettings taxSettings, CatalogSettings catalogSettings,
            EmailAccountSettings emailAccountSettings,
            CommonSettings commonSettings, BlogSettings blogSettings, ForumSettings forumSettings,
            LocalizationSettings localizationSettings, CaptchaSettings captchaSettings,
            IOrderTotalCalculationService orderTotalCalculationService, IPriceFormatter priceFormatter,
            ThemeSettings themeSettings, ISettingService settingService)
        {
            this._categoryService = categoryService;
            this._productService = productService;
            this._manufacturerService = manufacturerService;
            this._topicService = topicService;
            this._languageService = languageService;
            this._currencyService = currencyService;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._queuedEmailService = queuedEmailService;
            this._emailAccountService = emailAccountService;
            this._sitemapGenerator = sitemapGenerator;
            this._themeContext = themeContext;
            this._themeRegistry = themeRegistry;
            this._forumservice = forumService;
            this._genericAttributeService = genericAttributeService;
            this._webHelper = webHelper;
            this._permissionService = permissionService;
            this._mobileDeviceHelper = mobileDeviceHelper;
            this._cacheManager = cacheManager;
            this._customerActivityService = customerActivityService;

            this._customerSettings = customerSettings;
            this._taxSettings = taxSettings;
            this._catalogSettings = catalogSettings;
            this._commonSettings = commonSettings;
            this._blogSettings = blogSettings;
            this._forumSettings = forumSettings;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;

            this._orderTotalCalculationService = orderTotalCalculationService;
            this._priceFormatter = priceFormatter;

            this._themeSettings = themeSettings;
			this._settingService = settingService;
			T = NullLocalizer.Instance;
        }

        #endregion

        #region Utilities

        // page not found
        public ActionResult PageNotFound()
        {
            this.Response.StatusCode = 404;
            this.Response.TrySkipIisCustomErrors = true;
            
            return View();
        }

        [NonAction]
        protected LanguageSelectorModel PrepareLanguageSelectorModel()
        {
			var availableLanguages = _cacheManager.Get(string.Format(ModelCacheEventConsumer.AVAILABLE_LANGUAGES_MODEL_KEY, _storeContext.CurrentStore.Id), () =>
            {
                var result = _languageService
					.GetAllLanguages(storeId: _storeContext.CurrentStore.Id)
                    .Select(x => new LanguageModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        // codehint: sm-add
                        NativeName = GetLanguageNativeName(x.LanguageCulture) ?? x.Name,
                        ISOCode = x.LanguageCulture,
                        SeoCode = x.UniqueSeoCode,
                        FlagImageFileName = x.FlagImageFileName
                    })
                    .ToList();
                return result;
            });

            var workingLanguage = _workContext.WorkingLanguage;

            var model = new LanguageSelectorModel()
            {
                CurrentLanguageId = workingLanguage.Id,
                AvailableLanguages = availableLanguages,
                UseImages = _localizationSettings.UseImagesForLanguageSelection
            };
            
            string defaultSeoCode = _workContext.GetDefaultLanguageSeoCode();

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

                model.ReturnUrls[lang.SeoCode] = HttpUtility.UrlEncode(helper.GetAbsolutePath());
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
			var availableCurrencies = _cacheManager.Get(string.Format(ModelCacheEventConsumer.AVAILABLE_CURRENCIES_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id), () =>
            {
                var result = _currencyService
					.GetAllCurrencies(storeId: _storeContext.CurrentStore.Id)
                    .Select(x => new CurrencyModel()
                    {
                        Id = x.Id,
                        Name = x.GetLocalized(y => y.Name),
                        // codehint: sm-add
                        ISOCode = x.CurrencyCode,
                        Symbol = GetCurrencySymbol(x.DisplayLocale) ?? x.CurrencyCode
                    })
                    .ToList();
                return result;
            });

            var model = new CurrencySelectorModel()
            {
                CurrentCurrencyId = _workContext.WorkingCurrency.Id,
                AvailableCurrencies = availableCurrencies
            };
            return model;
        }

        // codehint: sm-add
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
                CurrentTaxType = _workContext.TaxDisplayType
            };
            return model;
        }

        [NonAction]
        protected int GetUnreadPrivateMessages()
        {
            var result = 0;
            var customer = _workContext.CurrentCustomer;
            if (_forumSettings.AllowPrivateMessages && !customer.IsGuest())
            {
				var privateMessages = _forumservice.GetAllPrivateMessages(_storeContext.CurrentStore.Id,
					 0, customer.Id, false, null, false, string.Empty, 0, 1);

                if (privateMessages.TotalCount > 0)
                {
                    result = privateMessages.TotalCount;
                }
            }

            return result;
        }

        #endregion

		#region Properties

		public Localizer T { get; set; }

		#endregion

		#region Methods

		//language
        [ChildActionOnly]
        public ActionResult LanguageSelector()
        {
            var model = PrepareLanguageSelectorModel();
            return PartialView(model);
        }

        [ChildActionOnly]
        public ActionResult Header()
        {
			var model = _cacheManager.Get(ModelCacheEventConsumer.SHOPHEADER_MODEL_KEY.FormatWith(_storeContext.CurrentStore.Id), () =>
			{
                var pictureService = EngineContext.Current.Resolve<IPictureService>();
                int logoPictureId = _storeContext.CurrentStore.LogoPictureId;

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
                    LogoTitle = _storeContext.CurrentStore.Name
                };
            });
            

            return PartialView(model);
        }

        public ActionResult SetLanguage(int langid, string returnUrl = "")
        {
            var language = _languageService.GetLanguageById(langid);
            if (language != null && language.Published)
            {
                _workContext.WorkingLanguage = language;
            }
            
            // url referrer
            if (String.IsNullOrEmpty(returnUrl))
            {
                returnUrl = _webHelper.GetUrlReferrer();
            }

            // home page
            if (String.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.RouteUrl("HomePage");
            }

            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                var helper = new LocalizedUrlHelper(HttpContext.Request.ApplicationPath, returnUrl, true);
                helper.PrependSeoCode(_workContext.WorkingLanguage.UniqueSeoCode, true);
                returnUrl = helper.GetAbsolutePath();
            }

            return Redirect(returnUrl);
        }

        //currency
        [ChildActionOnly]
        public ActionResult CurrencySelector()
        {
            var model = PrepareCurrencySelectorModel();
            return PartialView(model);
        }
        public ActionResult CurrencySelected(int customerCurrency, string returnUrl = "")
        {
            var currency = _currencyService.GetCurrencyById(customerCurrency);
            if (currency != null)
                _workContext.WorkingCurrency = currency;

            //url referrer
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = _webHelper.GetUrlReferrer();
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
            _workContext.TaxDisplayType = taxDisplayType;

            //url referrer
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = _webHelper.GetUrlReferrer();
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
        public ActionResult Config()
        {
            return View();
        }

        //footer
        [ChildActionOnly]
        public ActionResult JavaScriptDisabledWarning()
        {
            if (!_commonSettings.DisplayJavaScriptDisabledWarning)
                return Content("");

            return PartialView();
        }

        //header links
        [ChildActionOnly]
        public ActionResult HeaderLinks()
        {
            var customer = _workContext.CurrentCustomer;

            var unreadMessageCount = GetUnreadPrivateMessages();
            var unreadMessage = string.Empty;
            var alertMessage = string.Empty;
            if (unreadMessageCount > 0)
            {
                unreadMessage = T("PrivateMessages.TotalUnread", unreadMessageCount);

                //notifications here
				var notifiedAboutNewPrivateMessagesAttributeKey = string.Format(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, _storeContext.CurrentStore.Id);
				if (_forumSettings.ShowAlertForPM &&
					!customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, _storeContext.CurrentStore.Id))
                {
					_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true, _storeContext.CurrentStore.Id);
					alertMessage = T("PrivateMessages.YouHaveUnreadPM", unreadMessageCount);
                }
            }

            var model = new HeaderLinksModel()
            {
                IsAuthenticated = customer.IsRegistered(),
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
                IsCustomerImpersonated = _workContext.OriginalCustomerIfImpersonated != null,
                DisplayAdminLink = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel),
                ShoppingCartEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart),
				ShoppingCartItems = customer.CountProductsInCart(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id),
                WishlistEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
				WishlistItems = customer.CountProductsInCart(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id),
                AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                UnreadPrivateMessages = unreadMessage,
                AlertMessage = alertMessage,
            };

            return PartialView(model);
        }


        //shopbar
        [ChildActionOnly]
        public ActionResult ShopBar()
        {
            var customer = _workContext.CurrentCustomer;

            var unreadMessageCount = GetUnreadPrivateMessages();
            var unreadMessage = string.Empty;
            var alertMessage = string.Empty;
            if (unreadMessageCount > 0)
            {
                unreadMessage = T("PrivateMessages.TotalUnread");

                //notifications here
                if (_forumSettings.ShowAlertForPM &&
					!customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, _storeContext.CurrentStore.Id))
                {
					_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true, _storeContext.CurrentStore.Id);
                    alertMessage = T("PrivateMessages.YouHaveUnreadPM", unreadMessageCount);
                }
            }

            //subtotal
			decimal subtotal = 0;
            var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

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
                subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
            }
            var model = new ShopBarModel()
            {
                IsAuthenticated = customer.IsRegistered(),
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
                IsCustomerImpersonated = _workContext.OriginalCustomerIfImpersonated != null,
                DisplayAdminLink = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel),
                ShoppingCartEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart),
                ShoppingCartAmount = _priceFormatter.FormatPrice(subtotal, true, false),
                WishlistEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
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
					model.WishlistItems = customer.CountProductsInCart(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);
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
            string taxInfo = (_workContext.GetTaxDisplayTypeFor(_workContext.CurrentCustomer, _storeContext.CurrentStore.Id) == TaxDisplayType.IncludingTax)
                ? T("Tax.InclVAT") 
                : T("Tax.ExclVAT");

            string shippingInfoLink = Url.RouteUrl("Topic", new { SystemName = "shippinginfo" });
			var store = _storeContext.CurrentStore;

            var AvailableStoreThemes = _themeRegistry.GetThemeManifests()
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

            var model = new FooterModel()
            {
				StoreName = store.Name,
				LegalInfo = T("Tax.LegalInfoFooter", taxInfo, shippingInfoLink),
                ShowLegalInfo = _taxSettings.ShowLegalHintsInFooter,
                ShowThemeSelector = _themeSettings.AllowCustomerToSelectTheme && AvailableStoreThemes.Count > 1,          
                BlogEnabled = _blogSettings.Enabled,                          
                ForumEnabled = _forumSettings.ForumsEnabled,
                HideNewsletterBlock = _customerSettings.HideNewsletterBlock,
            };

			var hint = _settingService.GetSettingByKey<string>("Rnd_SmCopyrightHint", string.Empty, store.Id);
			if (hint.IsEmpty())
			{
				hint = s_hints[new Random().Next(s_hints.Length)];
				_settingService.SetSetting<string>("Rnd_SmCopyrightHint", hint, store.Id);
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
            var customer = _workContext.CurrentCustomer;

            var model = new MenuModel()
            {
                RecentlyAddedProductsEnabled = _catalogSettings.RecentlyAddedProductsEnabled,
                BlogEnabled = _blogSettings.Enabled,
                ForumEnabled = _forumSettings.ForumsEnabled,
                AllowPrivateMessages = _forumSettings.AllowPrivateMessages && _workContext.CurrentCustomer.IsRegistered(),
                UnreadPrivateMessages = GetUnreadPrivateMessages(),
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
                IsCustomerImpersonated = _workContext.OriginalCustomerIfImpersonated != null,
                IsAuthenticated = customer.IsRegistered(),
                DisplayAdminLink = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel),
            };

            return PartialView(model);
        }

        //info block
        [ChildActionOnly]
        public ActionResult InfoBlock()
        {
            var model = new InfoBlockModel()
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

        //contact us page
        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult ContactUs()
        {
            var model = new ContactUsModel()
            {
                Email = _workContext.CurrentCustomer.Email,
                FullName = _workContext.CurrentCustomer.GetFullName(),
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage
            };
            return View(model);
        }
        [HttpPost, ActionName("ContactUs")]
        [CaptchaValidator]
        public ActionResult ContactUsSend(ContactUsModel model, bool captchaValid)
        {
            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage && !captchaValid)
            {
                ModelState.AddModelError("", T("Common.WrongCaptcha"));
            }

            if (ModelState.IsValid)
            {
                string email = model.Email.Trim();
                string fullName = model.FullName;
				string subject = T("ContactUs.EmailSubject", _storeContext.CurrentStore.Name);

                var emailAccount = _emailAccountService.GetEmailAccountById(EngineContext.Current.Resolve<EmailAccountSettings>().DefaultEmailAccountId);
                if (emailAccount == null)
                    emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();

                string from = null;
                string fromName = null;
                string body = Core.Html.HtmlUtils.FormatText(model.Enquiry, false, true, false, false, false, false);
                //required for some SMTP servers
                if (_commonSettings.UseSystemEmailForContactUsForm)
                {
                    from = emailAccount.Email;
                    fromName = emailAccount.DisplayName;
                    body = string.Format("<strong>From</strong>: {0} - {1}<br /><br />{2}", 
                        Server.HtmlEncode(fullName), 
                        Server.HtmlEncode(email), body);
                }
                else
                {
                    from = email;
                    fromName = fullName;
                }
                _queuedEmailService.InsertQueuedEmail(new QueuedEmail()
                {
                    From = from,
                    FromName = fromName,
                    To = emailAccount.Email,
                    ToName = emailAccount.DisplayName,
                    Priority = 5,
                    Subject = subject,
                    Body = body,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id,
					ReplyTo = email,
					ReplyToName = fullName
                });

                model.SuccessfullySent = true;
                model.Result = T("ContactUs.YourEnquiryHasBeenSent");

                //activity log
                _customerActivityService.InsertActivity("PublicStore.ContactUs", T("ActivityLog.PublicStore.ContactUs"));

                return View(model);
            }

            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage;
            return View(model);
        }

        //sitemap page
        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult Sitemap()
        {
            if (!_commonSettings.SitemapEnabled)
                return RedirectToRoute("HomePage");

            var model = new SitemapModel();
            if (_commonSettings.SitemapIncludeCategories)
            {
                var categories = _categoryService.GetAllCategories();
                model.Categories = categories.Select(x => x.ToModel()).ToList();
            }
            if (_commonSettings.SitemapIncludeManufacturers)
            {
                var manufacturers = _manufacturerService.GetAllManufacturers();
                model.Manufacturers = manufacturers.Select(x => x.ToModel()).ToList();
            }
            if (_commonSettings.SitemapIncludeProducts)
            {
                //limit product to 200 until paging is supported on this page
                IList<int> filterableSpecificationAttributeOptionIds = null;

                var productSearchContext = new ProductSearchContext();

                productSearchContext.OrderBy = ProductSortingEnum.Position;
                productSearchContext.PageSize = 200;
                productSearchContext.FilterableSpecificationAttributeOptionIds = filterableSpecificationAttributeOptionIds;
				productSearchContext.StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode;
				productSearchContext.VisibleIndividuallyOnly = true;

                var products = _productService.SearchProducts(productSearchContext);

                model.Products = products.Select(product => new ProductOverviewModel()
                {
                    Id = product.Id,
                    Name = product.GetLocalized(x => x.Name).EmptyNull(),
                    ShortDescription = product.GetLocalized(x => x.ShortDescription),
                    FullDescription = product.GetLocalized(x => x.FullDescription),
                    SeName = product.GetSeName(),
                }).ToList();
            }
            if (_commonSettings.SitemapIncludeTopics)
            {
				var topics = _topicService.GetAllTopics(_storeContext.CurrentStore.Id)
					 .ToList()
					 .FindAll(t => t.IncludeInSitemap);
                model.Topics = topics.Select(topic => new TopicModel()
                {
                    Id = topic.Id,
                    SystemName = topic.SystemName,
                    IncludeInSitemap = topic.IncludeInSitemap,
                    IsPasswordProtected = topic.IsPasswordProtected,
                    Title = topic.GetLocalized(x => x.Title),
                })
                .ToList();
            }
            return View(model);
        }

        //SEO sitemap page
        [RequireHttpsByConfigAttribute(SslRequirement.No)]
        public ActionResult SitemapSeo()
        {
            if (!_commonSettings.SitemapEnabled)
                return RedirectToRoute("HomePage");

            string siteMap = _sitemapGenerator.Generate();
            return Content(siteMap, "text/xml");
        }

        //store theme
        [ChildActionOnly]
        public ActionResult StoreThemeSelector()
        {
            if (!_themeSettings.AllowCustomerToSelectTheme)
                return Content("");

            var model = new StoreThemeSelectorModel();
            var currentTheme = _themeRegistry.GetThemeManifest(_themeContext.WorkingDesktopTheme);
            model.CurrentStoreTheme = new StoreThemeModel()
            {
                Name = currentTheme.ThemeName,
                Title = currentTheme.ThemeTitle
            };
            model.AvailableStoreThemes = _themeRegistry.GetThemeManifests()
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
        public ActionResult StoreThemeSelected(string themeName)
        {
            _themeContext.WorkingDesktopTheme = themeName;

            var model = new StoreThemeSelectorModel();
            var currentTheme = _themeRegistry.GetThemeManifest(_themeContext.WorkingDesktopTheme);
            model.CurrentStoreTheme = new StoreThemeModel()
            {
                Name = currentTheme.ThemeName,
                Title = currentTheme.ThemeTitle
            };
            model.AvailableStoreThemes = _themeRegistry.GetThemeManifests()
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
            return PartialView("StoreThemeSelector", model);
        }

        //favicon
        [ChildActionOnly]
        [OutputCache(Duration=3600, VaryByCustom="Theme_Store")]
        public ActionResult Favicon()
        {
            var icons = new string[] 
            { 
                "favicon-{0}.ico".FormatInvariant(_storeContext.CurrentStore.Id), 
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
        public ActionResult ChangeDevice(bool dontUseMobileVersion)
        {
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
				SystemCustomerAttributeNames.DontUseMobileVersion, dontUseMobileVersion, _storeContext.CurrentStore.Id);

            string returnurl = _webHelper.GetUrlReferrer();
            if (String.IsNullOrEmpty(returnurl))
                returnurl = Url.RouteUrl("HomePage");
            return Redirect(returnurl);
        }
        [ChildActionOnly]
        public ActionResult ChangeDeviceBlock()
        {
            if (!_mobileDeviceHelper.MobileDevicesSupported())
                //mobile devices support is disabled
                return Content("");

            if (!_mobileDeviceHelper.IsMobileDevice())
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
                "/Content/files/exportimport/",
                "/country/getstatesbycountryid",
                "/install",
                "/setproductreviewhelpfulness",
            };
            var localizableDisallowPaths = new List<string>()
            {
				"/addproducttocart/catalog/",
				"/addproducttocart/details/",
                "/boards/forumwatch",
                "/boards/postedit",
                "/boards/postdelete",
                "/boards/postcreate",
                "/boards/topicedit",
                "/boards/topicdelete",
                "/boards/topiccreate",
                "/boards/topicmove",
                "/boards/topicwatch",
                "/cart",
                "/checkout",
                "/checkout/billingaddress",
                "/checkout/completed",
                "/checkout/confirm",
                "/checkout/shippingaddress",
                "/checkout/shippingmethod",
                "/checkout/paymentinfo",
                "/checkout/paymentmethod",
                "/clearcomparelist",
                "/compareproducts",
                "/customer/avatar",
                "/customer/activation",
                "/customer/addresses",
                "/customer/backinstocksubscriptions",
                "/customer/changepassword",
                "/customer/checkusernameavailability",
                "/customer/downloadableproducts",
                "/customer/forumsubscriptions",
                "/customer/info",
                "/customer/orders",
                "/customer/returnrequests",
                "/customer/rewardpoints",
                "/deletepm",
                "/emailwishlist",
                "/inboxupdate",
                "/newsletter/subscriptionactivation",
                "/onepagecheckout",
                "/orderdetails",
                "/passwordrecovery/confirm",
                "/poll/vote",
                "/privatemessages",
                "/returnrequest",
                "/sendpm",
                "/sentupdate",
                "/subscribenewsletter",
                "/topic/authenticate",
                "/viewpm",
                "/wishlist",
                "/productaskquestion",
                "/productemailafriend"
            };


            const string newLine = "\r\n"; //Environment.NewLine
            var sb = new StringBuilder();
            sb.Append("User-agent: *");
            sb.Append(newLine);

            var disallows = disallowPaths.Concat(localizableDisallowPaths);
            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                // URLs are localizable. Append SEO code
                foreach (var language in _languageService.GetAllLanguages(storeId: _storeContext.CurrentStore.Id))
                {
                    disallows = disallows.Concat(localizableDisallowPaths.Select(x => "/{0}{1}".FormatInvariant(language.UniqueSeoCode, x)));
                }
            }

            var seoSettings = EngineContext.Current.Resolve<SeoSettings>();

            // append extra disallows
            disallows = disallows.Concat(seoSettings.ExtraRobotsDisallows.Select(x => x.Trim()));

            foreach (var disallow in disallows)
            {
                sb.AppendFormat("Disallow: {0}", disallow);
                sb.Append(newLine);
            }

            Response.ContentType = "text/plain";
            Response.Write(sb.ToString());
            return null;
        }

        public ActionResult GenericUrl()
        {
            //seems that no entity was found
            return RedirectToRoute("HomePage");
        }

        /// <summary>
        /// <remarks>codehint: sm-add</remarks>
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult AccountDropdown()
        {
            var customer = _workContext.CurrentCustomer;

            var unreadMessageCount = GetUnreadPrivateMessages();
            var unreadMessage = string.Empty;
            var alertMessage = string.Empty;
            if (unreadMessageCount > 0)
            {
                unreadMessage = T("PrivateMessages.TotalUnread", unreadMessageCount);

                //notifications here
                if (_forumSettings.ShowAlertForPM &&
					!customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, _storeContext.CurrentStore.Id))
                {
					_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true, _storeContext.CurrentStore.Id);
					alertMessage = T("PrivateMessages.YouHaveUnreadPM", unreadMessageCount);
                }
            }

            var model = new AccountDropdownModel
            {
                IsAuthenticated = customer.IsRegistered(),
                DisplayAdminLink = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel),
                ShoppingCartEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart),
				ShoppingCartItems = customer.CountProductsInCart(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id),
                WishlistEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
				WishlistItems = customer.CountProductsInCart(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id),
                AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                UnreadPrivateMessages = unreadMessage,
                AlertMessage = alertMessage
            };

            return PartialView(model);
        }

        //store is closed
        public ActionResult StoreClosed()
        {
            return View();
        }

        #endregion
    }
}
