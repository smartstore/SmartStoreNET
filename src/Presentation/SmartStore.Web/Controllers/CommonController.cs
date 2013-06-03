using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain;
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
using SmartStore.Services.Discounts;
using SmartStore.Services.Forums;
using SmartStore.Services.Localization;
using SmartStore.Services.Logging;
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
using SmartStore.Web.Models.ShoppingCart;
using SmartStore.Web.Models.Topics;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Web.Controllers
{
    public partial class CommonController : SmartController
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ITopicService _topicService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
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

        private readonly CustomerSettings _customerSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly ThemeSettings _themeSettings;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly CommonSettings _commonSettings;
        private readonly BlogSettings _blogSettings;
        private readonly ForumSettings _forumSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly SocialSettings _socialSettings;

        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICompareProductsService _compareProductsService;
        private readonly IPictureService _pictureService; //codehint: sm-add

        #endregion

        #region Constructors

        public CommonController(ICategoryService categoryService, IProductService productService,
            IManufacturerService manufacturerService, ITopicService topicService,
            ILanguageService languageService, ICompareProductsService compareProductsService,
            ICurrencyService currencyService, ILocalizationService localizationService,
            IWorkContext workContext, IPictureService pictureService,
            IQueuedEmailService queuedEmailService, IEmailAccountService emailAccountService,
            ISitemapGenerator sitemapGenerator, IThemeContext themeContext,
            IThemeRegistry themeRegistry, IForumService forumService,
            IGenericAttributeService genericAttributeService, IWebHelper webHelper,
            IPermissionService permissionService, IMobileDeviceHelper mobileDeviceHelper,
            ICacheManager cacheManager,
            ICustomerActivityService customerActivityService, CustomerSettings customerSettings, 
            TaxSettings taxSettings, CatalogSettings catalogSettings,
            StoreInformationSettings storeInformationSettings, EmailAccountSettings emailAccountSettings,
            CommonSettings commonSettings, BlogSettings blogSettings, ForumSettings forumSettings,
            LocalizationSettings localizationSettings, CaptchaSettings captchaSettings,
            IOrderTotalCalculationService orderTotalCalculationService, IPriceFormatter priceFormatter,
            ThemeSettings themeSettings, SocialSettings socialSettings)
        {
            this._categoryService = categoryService;
            this._productService = productService;
            this._manufacturerService = manufacturerService;
            this._topicService = topicService;
            this._languageService = languageService;
            this._currencyService = currencyService;
            this._localizationService = localizationService;
            this._workContext = workContext;
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
            this._storeInformationSettings = storeInformationSettings;
            this._emailAccountSettings = emailAccountSettings;
            this._commonSettings = commonSettings;
            this._blogSettings = blogSettings;
            this._forumSettings = forumSettings;
            this._localizationSettings = localizationSettings;
            this._captchaSettings = captchaSettings;

            this._orderTotalCalculationService = orderTotalCalculationService;
            this._priceFormatter = priceFormatter;
            this._compareProductsService = compareProductsService;

            //codehint: sm-add
            this._pictureService = pictureService;
            this._themeSettings = themeSettings;
            this._socialSettings = socialSettings;
        }

        #endregion

        #region Utilities

        //page not found
        public ActionResult PageNotFound()
        {
            this.Response.StatusCode = 404;
            this.Response.TrySkipIisCustomErrors = true;

            return View();
        }

        [NonAction]
        protected LanguageSelectorModel PrepareLanguageSelectorModel()
        {
			var availableLanguages = _cacheManager.Get(string.Format(ModelCacheEventConsumer.AVAILABLE_LANGUAGES_MODEL_KEY, _workContext.CurrentStore.Id), () =>
            {
                var result = _languageService
					.GetAllLanguages(storeId: _workContext.CurrentStore.Id)
                    .Select(x => new LanguageModel()
                    {
                        Id = x.Id,
                        Name = x.Name,
                        // codehint: sm-add
                        NativeName = GetLanguageNativeName(x.LanguageCulture) ?? x.Name,
                        ISOCode = x.LanguageCulture,
                        FlagImageFileName = x.FlagImageFileName,
                    })
                    .ToList();
                return result;
            });

            var model = new LanguageSelectorModel()
            {
                CurrentLanguageId = _workContext.WorkingLanguage.Id,
                AvailableLanguages = availableLanguages,
                UseImages = _localizationSettings.UseImagesForLanguageSelection
            };
            return model;
        }

        // codehint: sm-add
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
			var availableCurrencies = _cacheManager.Get(string.Format(ModelCacheEventConsumer.AVAILABLE_CURRENCIES_MODEL_KEY, _workContext.WorkingLanguage.Id, _workContext.CurrentStore.Id), () =>
            {
                var result = _currencyService
					.GetAllCurrencies(storeId: _workContext.CurrentStore.Id)
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
                var privateMessages = _forumservice.GetAllPrivateMessages(0, customer.Id, false, null, false, string.Empty, 0, 1);

                if (privateMessages.TotalCount > 0)
                {
                    result = privateMessages.TotalCount;
                }
            }

            return result;
        }

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
            var model = _cacheManager.Get(ModelCacheEventConsumer.SHOPHEADER_MODEL_KEY, () => {
                int logoPictureId = _storeInformationSettings.LogoPictureId;

                Picture picture = null;
                if (logoPictureId > 0)
                {
                    picture = _pictureService.GetPictureById(logoPictureId);
                }

                string logoUrl = null;
                var logoSize = new Size();
                if (picture != null)
                {
                    logoUrl = _pictureService.GetPictureUrl(picture);
                    logoSize = _pictureService.GetPictureSize(picture);
                }

                return new ShopHeaderModel()
                {
                    LogoUploaded = picture != null,
                    LogoUrl = logoUrl,
                    LogoWidth = logoSize.Width,
                    LogoHeight = logoSize.Height,
                    LogoTitle = _storeInformationSettings.StoreName
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

            //url referrer
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = _webHelper.GetUrlReferrer();
            //home page
            if (String.IsNullOrEmpty(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");
            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                string applicationPath = HttpContext.Request.ApplicationPath;
                if (returnUrl.IsLocalizedUrl(applicationPath, true))
                {
                    //already localized URL
                    returnUrl = returnUrl.RemoveLanguageSeoCodeFromRawUrl(applicationPath);
                }
                returnUrl = returnUrl.AddLanguageSeoCodeToRawUrl(applicationPath, _workContext.WorkingLanguage);
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
                unreadMessage = string.Format(_localizationService.GetResource("PrivateMessages.TotalUnread"), unreadMessageCount);

                //notifications here
                if (_forumSettings.ShowAlertForPM && 
                    !customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages))
                {
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true);
                    alertMessage = string.Format(_localizationService.GetResource("PrivateMessages.YouHaveUnreadPM"), unreadMessageCount);
                }
            }

            var model = new HeaderLinksModel()
            {
                IsAuthenticated = customer.IsRegistered(),
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.UsernamesEnabled ? customer.Username : customer.Email) : "",
                IsCustomerImpersonated = _workContext.OriginalCustomerIfImpersonated != null,
                DisplayAdminLink = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel),
                ShoppingCartEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart),
                ShoppingCartItems = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList().GetTotalProducts(),
                WishlistEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
                WishlistItems = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList().GetTotalProducts(),
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
                unreadMessage = string.Format(_localizationService.GetResource("PrivateMessages.TotalUnread"), unreadMessageCount);

                //notifications here
                if (_forumSettings.ShowAlertForPM &&
                    !customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages))
                {
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true);
                    alertMessage = string.Format(_localizationService.GetResource("PrivateMessages.YouHaveUnreadPM"), unreadMessageCount);
                }
            }

            //subtotal
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
            decimal subtotal = 0;
            if (cart.Count > 0)
            {
                decimal subtotalBase = decimal.Zero;
                decimal orderSubTotalDiscountAmountBase = decimal.Zero;
                Discount orderSubTotalAppliedDiscount = null;
                decimal subTotalWithoutDiscountBase = decimal.Zero;
                decimal subTotalWithDiscountBase = decimal.Zero;
                _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount,
                    out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);
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
                ShoppingCartItems = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList().GetTotalProducts(),
                ShoppingCartAmount = _priceFormatter.FormatPrice(subtotal, true, false),
                WishlistEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
                WishlistItems = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList().GetTotalProducts(),
                AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                UnreadPrivateMessages = unreadMessage,
                AlertMessage = alertMessage,
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled,
                CompareItems = _compareProductsService.GetComparedProducts().Count
            };

            return PartialView(model);
        }

        // footer
        // codehint: sm-edit nearly complete
        [ChildActionOnly]
        public ActionResult Footer()
        {
            string taxInfo = (_workContext.GetTaxDisplayTypeFor(_workContext.CurrentCustomer) == TaxDisplayType.IncludingTax)
                ? _localizationService.GetResource("Tax.InclVAT") 
                : _localizationService.GetResource("Tax.ExclVAT");

            string shippingInfoLink = Url.RouteUrl("Topic", new { SystemName = "shippinginfo" });

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

            //TODO
            //var topicTitles = from t in _topicService.GetAllTopics()
            //                  select new
            //                  {
            //                      Id = t.SystemName,
            //                      Title = t.Title
            //                  };

            var model = new FooterModel()
            {
                StoreName = _storeInformationSettings.StoreName,
                LegalInfo = string.Format(_localizationService.GetResource("Tax.LegalInfoFooter"), taxInfo, shippingInfoLink),
                ShowLegalInfo = _taxSettings.ShowLegalHintsInFooter,
                ShowThemeSelector = _themeSettings.AllowCustomerToSelectTheme && AvailableStoreThemes.Count > 1,          
                BlogEnabled = _blogSettings.Enabled,                          
                ForumEnabled = _forumSettings.ForumsEnabled,                  
            };

            var topics = new string[] { "paymentinfo", "imprint", "disclaimer" };
            foreach (var t in topics)
            {
                var topic = _topicService.GetTopicBySystemName(t);
                if (topic != null)
                {
                    model.Topics.Add(t, topic.Title);
                }
            }

            //var topic = _topicService.GetTopicBySystemName("paymentinfo");
            //if (topic != null)
            //{
            //    model.Topics.Add("paymentinfo", topic.Title);
            //}
            //topic = _topicService.GetTopicBySystemName("imprint");
            //if (topic != null)
            //{
            //    model.Topics.Add("imprint", topic.Title);
            //}

            //model.Topics.Add("disclaimer", _topicService.GetTopicBySystemName("disclaimer").Title);

            model.ShowSocialLinks = _socialSettings.ShowSocialLinksInFooter;
            model.FacebookLink = _socialSettings.FacebookLink;
            model.GooglePlusLink = _socialSettings.GooglePlusLink;
            model.TwitterLink = _socialSettings.TwitterLink;
            model.PinterestLink = _socialSettings.PinterestLink;

            return PartialView(model);
        }

        //menu
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
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));
            }

            if (ModelState.IsValid)
            {
                string email = model.Email.Trim();
                string fullName = model.FullName;
                string subject = string.Format(_localizationService.GetResource("ContactUs.EmailSubject"), _storeInformationSettings.StoreName);

                var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
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
                    EmailAccountId = emailAccount.Id
                });

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("ContactUs.YourEnquiryHasBeenSent");

                //activity log
                _customerActivityService.InsertActivity("PublicStore.ContactUs", _localizationService.GetResource("ActivityLog.PublicStore.ContactUs"));

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

                var products = _productService.SearchProducts(productSearchContext);

                model.Products = products.Select(product => new ProductOverviewModel()
                {
                    Id = product.Id,
                    Name = product.GetLocalized(x => x.Name),
                    ShortDescription = product.GetLocalized(x => x.ShortDescription),
                    FullDescription = product.GetLocalized(x => x.FullDescription),
                    SeName = product.GetSeName(),
                }).ToList();
            }
            if (_commonSettings.SitemapIncludeTopics)
            {
                var topics = _topicService.GetAllTopics().ToList().FindAll(t => t.IncludeInSitemap);
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
        public ActionResult Favicon()
        {
            var model = new FaviconModel()
            {
                Uploaded = System.IO.File.Exists(System.IO.Path.Combine(Request.PhysicalApplicationPath, "favicon.ico")),
                FaviconUrl = _webHelper.GetStoreLocation() + "favicon.ico"
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
                SystemCustomerAttributeNames.DontUseMobileVersion, dontUseMobileVersion);

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
                                               };


            const string newLine = "\r\n"; //Environment.NewLine
            var sb = new StringBuilder();
            sb.Append("User-agent: *");
            sb.Append(newLine);
            //usual paths
            foreach (var path in disallowPaths)
            {
                sb.AppendFormat("Disallow: {0}", path);
                sb.Append(newLine);
            }
            //localizable paths (without SEO code)
            foreach (var path in localizableDisallowPaths)
            {
                sb.AppendFormat("Disallow: {0}", path);
                sb.Append(newLine);
            }
            if (_localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                //URLs are localizable. Append SEO code
				foreach (var language in _languageService.GetAllLanguages(storeId: _workContext.CurrentStore.Id))
                {
                    foreach (var path in localizableDisallowPaths)
                    {
                        sb.AppendFormat("Disallow: {0}{1}", language.UniqueSeoCode, path);
                        sb.Append(newLine);
                    }
                }
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
                unreadMessage = string.Format(_localizationService.GetResource("PrivateMessages.TotalUnread"), unreadMessageCount);

                //notifications here
                if (_forumSettings.ShowAlertForPM &&
                    !customer.GetAttribute<bool>(SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages))
                {
                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, true);
                    alertMessage = string.Format(_localizationService.GetResource("PrivateMessages.YouHaveUnreadPM"), unreadMessageCount);
                }
            }

            var model = new AccountDropdownModel
            {
                IsAuthenticated = customer.IsRegistered(),
                DisplayAdminLink = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel),
                ShoppingCartEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart),
                ShoppingCartItems = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList().GetTotalProducts(),
                WishlistEnabled = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist),
                WishlistItems = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.Wishlist).ToList().GetTotalProducts(),
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
