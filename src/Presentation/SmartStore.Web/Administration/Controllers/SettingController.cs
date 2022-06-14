using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Admin.Models.Common;
using SmartStore.Admin.Models.Settings;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Media.Storage;
using SmartStore.Services.Orders;
using SmartStore.Services.Search.Extensions;
using SmartStore.Services.Search.Modelling;
using SmartStore.Services.Security;
using SmartStore.Services.Tax;
using SmartStore.Utilities;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Seo;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.UI;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class SettingController : AdminControllerBase
    {
        #region Fields

        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IOrderService _orderService;
        private readonly IEncryptionService _encryptionService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
        private readonly IProviderManager _providerManager;
        private readonly PluginMediator _pluginMediator;
        private readonly IPluginFinder _pluginFinder;
        private readonly Lazy<IMediaMover> _mediaMover;
        private readonly Lazy<IMediaTracker> _mediaTracker;
        private readonly Lazy<ICatalogSearchQueryAliasMapper> _catalogSearchQueryAliasMapper;
        private readonly Lazy<IForumSearchQueryAliasMapper> _forumSearchQueryAliasMapper;
        private readonly Lazy<IMenuService> _menuService;
        private readonly ICookieManager _cookieManager;

        private StoreDependingSettingHelper _storeDependingSettings;

        #endregion

        #region Constructors

        public SettingController(
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            ITaxCategoryService taxCategoryService,
            IDateTimeHelper dateTimeHelper,
            IOrderService orderService,
            IEncryptionService encryptionService,
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            IMaintenanceService maintenanceService,
            IGenericAttributeService genericAttributeService,
            ILocalizedEntityService localizedEntityService,
            ILanguageService languageService,
            IProviderManager providerManager,
            PluginMediator pluginMediator,
            IPluginFinder pluginFinder,
            Lazy<IMediaMover> mediaMover,
            Lazy<IMediaTracker> mediaTracker,
            Lazy<ICatalogSearchQueryAliasMapper> catalogSearchQueryAliasMapper,
            Lazy<IForumSearchQueryAliasMapper> forumSearchQueryAliasMapper,
            Lazy<IMenuService> menuService,
            ICookieManager cookieManager)
        {
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressService = addressService;
            _taxCategoryService = taxCategoryService;
            _dateTimeHelper = dateTimeHelper;
            _orderService = orderService;
            _encryptionService = encryptionService;
            _customerService = customerService;
            _customerActivityService = customerActivityService;
            _maintenanceService = maintenanceService;
            _genericAttributeService = genericAttributeService;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
            _providerManager = providerManager;
            _pluginMediator = pluginMediator;
            _pluginFinder = pluginFinder;
            _mediaMover = mediaMover;
            _mediaTracker = mediaTracker;
            _catalogSearchQueryAliasMapper = catalogSearchQueryAliasMapper;
            _forumSearchQueryAliasMapper = forumSearchQueryAliasMapper;
            _menuService = menuService;
            _cookieManager = cookieManager;
        }

        #endregion

        #region Utilities

        private StoreDependingSettingHelper StoreDependingSettings
        {
            get
            {
                if (_storeDependingSettings == null)
                {
                    _storeDependingSettings = new StoreDependingSettingHelper(ViewData);
                }

                return _storeDependingSettings;
            }
        }

        private SelectListItem ResToSelectListItem(string resourceKey)
        {
            var value = Services.Localization.GetResource(resourceKey).EmptyNull();
            return new SelectListItem { Text = value, Value = value };
        }

        private void UpdateLocalizedFacetSetting(CommonFacetSettingsModel model, FacetGroupKind kind, ref bool clearCache, string scope = null)
        {
            foreach (var localized in model.Locales)
            {
                var key = FacetUtility.GetFacetAliasSettingKey(kind, localized.LanguageId, scope);
                var existingAlias = Services.Settings.GetSettingByKey<string>(key);

                if (existingAlias.IsCaseInsensitiveEqual(localized.Alias))
                    continue;

                if (localized.Alias.HasValue())
                {
                    Services.Settings.SetSetting(key, localized.Alias, 0, false);
                }
                else
                {
                    Services.Settings.DeleteSetting(key);
                }

                clearCache = true;
            }
        }

        private ActionResult NotifyAndRedirect(string actionMethod)
        {
            NotifySuccess(T("Admin.Configuration.Updated"));
            return RedirectToAction(actionMethod);
        }

        #endregion

        #region Methods

        [ChildActionOnly]
        public ActionResult StoreScopeConfiguration()
        {
            var allStores = Services.StoreService.GetAllStores();
            if (allStores.Count < 2)
            {
                return new EmptyResult();
            }

            var model = new StoreScopeConfigurationModel
            {
                StoreId = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext)
            };

            foreach (var store in allStores)
            {
                model.AllStores.Add(new SelectListItem
                {
                    Text = store.Name,
                    Selected = (store.Id == model.StoreId),
                    Value = Url.Action("ChangeStoreScopeConfiguration", "Setting", new { storeid = store.Id, returnUrl = Request.RawUrl })
                });
            }

            model.AllStores.Insert(0, new SelectListItem
            {
                Text = Services.Localization.GetResource("Admin.Common.StoresAll"),
                Selected = 0 == model.StoreId,
                Value = Url.Action("ChangeStoreScopeConfiguration", "Setting", new { storeid = 0, returnUrl = Request.RawUrl })
            });

            return PartialView(model);
        }

        public ActionResult ChangeStoreScopeConfiguration(int storeid, string returnUrl = "")
        {
            var store = Services.StoreService.GetStoreById(storeid);
            if (store != null || storeid == 0)
            {
                _genericAttributeService.SaveAttribute(Services.WorkContext.CurrentCustomer, SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration, storeid);
            }

            return RedirectToReferrer(returnUrl, () => RedirectToAction("Index", "Home", new { area = "Admin" }));
        }

        [Permission(Permissions.Configuration.Setting.Read)]
        [LoadSetting]
        public ActionResult Blog(BlogSettings blogSettings, int storeId)
        {
            var model = blogSettings.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.MetaTitle = blogSettings.GetLocalizedSetting(x => x.MetaTitle, languageId, storeId, false, false);
                locale.MetaDescription = blogSettings.GetLocalizedSetting(x => x.MetaDescription, languageId, storeId, false, false);
                locale.MetaKeywords = blogSettings.GetLocalizedSetting(x => x.MetaKeywords, languageId, storeId, false, false);
            });

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost, SaveSetting]
        public ActionResult Blog(BlogSettings blogSettings, BlogSettingsModel model, int storeId)
        {
            model.ToEntity(blogSettings);

            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedSetting(blogSettings, x => x.MetaTitle, localized.MetaTitle, localized.LanguageId, storeId);
                _localizedEntityService.SaveLocalizedSetting(blogSettings, x => x.MetaDescription, localized.MetaDescription, localized.LanguageId, storeId);
                _localizedEntityService.SaveLocalizedSetting(blogSettings, x => x.MetaKeywords, localized.MetaKeywords, localized.LanguageId, storeId);
            }

            return NotifyAndRedirect("Blog");
        }

        [Permission(Permissions.Configuration.Setting.Read)]
        [LoadSetting]
        public ActionResult Forum(ForumSettings forumSettings, int storeId)
        {
            var model = forumSettings.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.MetaTitle = forumSettings.GetLocalizedSetting(x => x.MetaTitle, languageId, storeId, false, false);
                locale.MetaDescription = forumSettings.GetLocalizedSetting(x => x.MetaDescription, languageId, storeId, false, false);
                locale.MetaKeywords = forumSettings.GetLocalizedSetting(x => x.MetaKeywords, languageId, storeId, false, false);
            });

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost, SaveSetting]
        public ActionResult Forum(ForumSettings forumSettings, ForumSettingsModel model, int storeId)
        {
            model.ToEntity(forumSettings);

            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedSetting(forumSettings, x => x.MetaTitle, localized.MetaTitle, localized.LanguageId, storeId);
                _localizedEntityService.SaveLocalizedSetting(forumSettings, x => x.MetaDescription, localized.MetaDescription, localized.LanguageId, storeId);
                _localizedEntityService.SaveLocalizedSetting(forumSettings, x => x.MetaKeywords, localized.MetaKeywords, localized.LanguageId, storeId);
            }

            return NotifyAndRedirect("Forum");
        }


        [Permission(Permissions.Configuration.Setting.Read)]
        [LoadSetting]
        public ActionResult News(NewsSettings newsSettings, int storeId)
        {
            var model = newsSettings.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.MetaTitle = newsSettings.GetLocalizedSetting(x => x.MetaTitle, languageId, storeId, false, false);
                locale.MetaDescription = newsSettings.GetLocalizedSetting(x => x.MetaDescription, languageId, storeId, false, false);
                locale.MetaKeywords = newsSettings.GetLocalizedSetting(x => x.MetaKeywords, languageId, storeId, false, false);
            });

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost, SaveSetting]
        public ActionResult News(NewsSettings newsSettings, NewsSettingsModel model, int storeId)
        {
            model.ToEntity(newsSettings);

            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedSetting(newsSettings, x => x.MetaTitle, localized.MetaTitle, localized.LanguageId, storeId);
                _localizedEntityService.SaveLocalizedSetting(newsSettings, x => x.MetaDescription, localized.MetaDescription, localized.LanguageId, storeId);
                _localizedEntityService.SaveLocalizedSetting(newsSettings, x => x.MetaKeywords, localized.MetaKeywords, localized.LanguageId, storeId);
            }

            return NotifyAndRedirect("News");
        }

        [Permission(Permissions.Configuration.Setting.Read)]
        public ActionResult Shipping()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var shippingSettings = Services.Settings.LoadSetting<ShippingSettings>(storeScope);
            var store = storeScope == 0 ? Services.StoreContext.CurrentStore : Services.StoreService.GetStoreById(storeScope);

            var model = shippingSettings.ToModel();
            model.PrimaryStoreCurrencyCode = store.PrimaryStoreCurrency.CurrencyCode;
            model.TodayShipmentHours = new List<SelectListItem>();

            for (var i = 1; i <= 24; ++i)
            {
                var hourStr = i.ToString();
                model.TodayShipmentHours.Add(new SelectListItem
                {
                    Text = hourStr,
                    Value = hourStr,
                    Selected = shippingSettings.TodayShipmentHour == i
                });
            }

            StoreDependingSettings.GetOverrideKeys(shippingSettings, model, storeScope, Services.Settings);

            // Shipping origin
            if (storeScope > 0 && Services.Settings.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope))
            {
                StoreDependingSettings.AddOverrideKey(shippingSettings, "ShippingOriginAddress");
            }

            var originAddress = shippingSettings.ShippingOriginAddressId > 0
                ? _addressService.GetAddressById(shippingSettings.ShippingOriginAddressId)
                : null;

            model.ShippingOriginAddress = originAddress != null
                ? originAddress.ToModel()
                : new AddressModel();

            foreach (var c in _countryService.GetAllCountries(true))
            {
                model.ShippingOriginAddress.AvailableCountries.Add(
                    new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (originAddress != null && c.Id == originAddress.CountryId) }
                );
            }

            var states = originAddress != null && originAddress.Country != null
                ? _stateProvinceService.GetStateProvincesByCountryId(originAddress.Country.Id, true).ToList()
                : new List<StateProvince>();

            if (states.Count > 0)
            {
                foreach (var s in states)
                {
                    model.ShippingOriginAddress.AvailableStates.Add(
                        new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == originAddress.StateProvinceId) }
                    );
                }
            }
            else
            {
                model.ShippingOriginAddress.AvailableStates.Add(new SelectListItem { Text = T("Admin.Address.OtherNonUS"), Value = "0" });
            }

            model.ShippingOriginAddress.CountryEnabled = true;
            model.ShippingOriginAddress.StateProvinceEnabled = true;
            model.ShippingOriginAddress.ZipPostalCodeEnabled = true;
            model.ShippingOriginAddress.ZipPostalCodeRequired = true;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Setting.Update)]
        public ActionResult Shipping(ShippingSettingsModel model, FormCollection form)
        {
            // Note, model state is invalid here due to ShippingOriginAddress validation.
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var shippingSettings = Services.Settings.LoadSetting<ShippingSettings>(storeScope);
            shippingSettings = model.ToEntity(shippingSettings);

            using (Services.Settings.BeginScope())
            {
                StoreDependingSettings.UpdateSettings(shippingSettings, form, storeScope, Services.Settings, propertyName =>
                {
                    // Skip to prevent the address from being recreated every time you save.
                    if (propertyName.IsCaseInsensitiveEqual("ShippingOriginAddressId"))
                        return null;

                    return propertyName;
                });

                // Special case ShippingOriginAddressId\ShippingOriginAddress.
                if (storeScope == 0 || StoreDependingSettings.IsOverrideChecked(shippingSettings, "ShippingOriginAddress", form))
                {
                    var addressId = Services.Settings.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope) ? shippingSettings.ShippingOriginAddressId : 0;
                    var originAddress = _addressService.GetAddressById(addressId) ?? new Address { CreatedOnUtc = DateTime.UtcNow };

                    // Update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one).
                    model.ShippingOriginAddress.Id = originAddress.Id == 0 ? 0 : addressId;
                    originAddress = model.ShippingOriginAddress.ToEntity(originAddress);

                    if (originAddress.Id > 0)
                    {
                        _addressService.UpdateAddress(originAddress);
                    }
                    else
                    {
                        _addressService.InsertAddress(originAddress);
                    }

                    shippingSettings.ShippingOriginAddressId = originAddress.Id;
                    Services.Settings.SaveSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope, false);
                }
                else
                {
                    _addressService.DeleteAddress(shippingSettings.ShippingOriginAddressId);
                    Services.Settings.DeleteSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope);
                }
            }

            return NotifyAndRedirect("Shipping");
        }

        [Permission(Permissions.Configuration.Setting.Read)]
        public ActionResult Tax()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var taxSettings = Services.Settings.LoadSetting<TaxSettings>(storeScope);

            var model = taxSettings.ToModel();

            StoreDependingSettings.GetOverrideKeys(taxSettings, model, storeScope, Services.Settings);

            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            foreach (var tc in taxCategories)
            {
                model.ShippingTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString(), Selected = tc.Id == taxSettings.ShippingTaxClassId });
            }

            foreach (var tc in taxCategories)
            {
                model.PaymentMethodAdditionalFeeTaxCategories.Add(new SelectListItem
                {
                    Text = tc.Name,
                    Value = tc.Id.ToString(),
                    Selected = tc.Id == taxSettings.PaymentMethodAdditionalFeeTaxClassId
                });
            }

            // EU VAT countries.
            foreach (var c in _countryService.GetAllCountries(true))
            {
                model.EuVatShopCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == taxSettings.EuVatShopCountryId });
            }

            // Default tax address.
            var defaultAddress = (taxSettings.DefaultTaxAddressId > 0 ? _addressService.GetAddressById(taxSettings.DefaultTaxAddressId) : null);
            model.DefaultTaxAddress = defaultAddress != null
                ? defaultAddress.ToModel()
                : new AddressModel();

            if (storeScope > 0 && Services.Settings.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope))
            {
                StoreDependingSettings.AddOverrideKey(taxSettings, "DefaultTaxAddress");
            }

            foreach (var c in _countryService.GetAllCountries(true))
            {
                model.DefaultTaxAddress.AvailableCountries.Add(new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                    Selected = (defaultAddress != null && c.Id == defaultAddress.CountryId)
                });
            }

            var states = defaultAddress != null && defaultAddress.Country != null
                ? _stateProvinceService.GetStateProvincesByCountryId(defaultAddress.Country.Id, true).ToList()
                : new List<StateProvince>();

            if (states.Any())
            {
                foreach (var s in states)
                {
                    model.DefaultTaxAddress.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == defaultAddress.StateProvinceId) });
                }
            }
            else
            {
                model.DefaultTaxAddress.AvailableStates.Add(new SelectListItem { Text = T("Admin.Address.OtherNonUS"), Value = "0" });
            }

            model.DefaultTaxAddress.CountryEnabled = true;
            model.DefaultTaxAddress.StateProvinceEnabled = true;
            model.DefaultTaxAddress.ZipPostalCodeEnabled = true;
            model.DefaultTaxAddress.ZipPostalCodeRequired = true;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Setting.Update)]
        public ActionResult Tax(TaxSettingsModel model, FormCollection form)
        {
            // Note, model state invalid here due to DefaultTaxAddress validation.
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var taxSettings = Services.Settings.LoadSetting<TaxSettings>(storeScope);
            taxSettings = model.ToEntity(taxSettings);

            using (Services.Settings.BeginScope())
            {
                StoreDependingSettings.UpdateSettings(taxSettings, form, storeScope, Services.Settings, propertyName =>
                {
                    // Skip to prevent the address from being recreated every time you save.
                    if (propertyName.IsCaseInsensitiveEqual("DefaultTaxAddressId"))
                        return null;

                    return propertyName;
                });

                taxSettings.AllowCustomersToSelectTaxDisplayType = false;
                Services.Settings.UpdateSetting(taxSettings, x => x.AllowCustomersToSelectTaxDisplayType, false, storeScope);

                // Special case DefaultTaxAddressId\DefaultTaxAddress.
                if (storeScope == 0 || StoreDependingSettings.IsOverrideChecked(taxSettings, "DefaultTaxAddress", form))
                {
                    var addressId = Services.Settings.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope) ? taxSettings.DefaultTaxAddressId : 0;
                    var originAddress = _addressService.GetAddressById(addressId) ?? new Address { CreatedOnUtc = DateTime.UtcNow };

                    // Update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one).
                    model.DefaultTaxAddress.Id = originAddress.Id == 0 ? 0 : addressId;
                    originAddress = model.DefaultTaxAddress.ToEntity(originAddress);

                    if (originAddress.Id > 0)
                    {
                        _addressService.UpdateAddress(originAddress);
                    }
                    else
                    {
                        _addressService.InsertAddress(originAddress);
                    }

                    taxSettings.DefaultTaxAddressId = originAddress.Id;
                    Services.Settings.SaveSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope, false);
                }
                else if (storeScope > 0)
                {
                    _addressService.DeleteAddress(taxSettings.DefaultTaxAddressId);
                    Services.Settings.DeleteSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope);
                }
            }

            return NotifyAndRedirect("Tax");
        }


        [Permission(Permissions.Configuration.Setting.Read)]
        [LoadSetting]
        public ActionResult Catalog(CatalogSettings catalogSettings)
        {
            var model = catalogSettings.ToModel();

            model.AvailableSubCategoryDisplayTypes = catalogSettings.SubCategoryDisplayType.ToSelectList();
            model.AvailablePriceDisplayTypes = catalogSettings.PriceDisplayType.ToSelectList();
            model.AvailableSortOrderModes = catalogSettings.DefaultSortOrder.ToSelectList();

            model.AvailableDefaultViewModes.Add(
                new SelectListItem { Value = "grid", Text = T("Common.Grid"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("grid") }
            );
            model.AvailableDefaultViewModes.Add(
                new SelectListItem { Value = "list", Text = T("Common.List"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("list") }
            );

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost, ValidateInput(false), SaveSetting]
        public ActionResult Catalog(CatalogSettings catalogSettings, CatalogSettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return Catalog(catalogSettings);
            }

            ModelState.Clear();

            // We need to clear the sitemap cache if MaxItemsToDisplayInCatalogMenu has changed.
            if (catalogSettings.MaxItemsToDisplayInCatalogMenu != model.MaxItemsToDisplayInCatalogMenu)
            {
                // Clear cached navigation model.
                _menuService.Value.ClearCache("Main");
            }

            catalogSettings = model.ToEntity(catalogSettings);

            return NotifyAndRedirect("Catalog");
        }


        [Permission(Permissions.Configuration.Setting.Read)]
        [LoadSetting]
        public ActionResult RewardPoints(RewardPointsSettings rewardPointsSettings, int storeScope)
        {
            var store = storeScope == 0 ? Services.StoreContext.CurrentStore : Services.StoreService.GetStoreById(storeScope);

            var model = rewardPointsSettings.ToModel();
            model.PrimaryStoreCurrencyCode = store.PrimaryStoreCurrency.CurrencyCode;

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult RewardPoints(RewardPointsSettingsModel model, FormCollection form)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var rewardPointsSettings = Services.Settings.LoadSetting<RewardPointsSettings>(storeScope);

            if (!ModelState.IsValid)
            {
                return RewardPoints(rewardPointsSettings, storeScope);
            }

            ModelState.Clear();
            rewardPointsSettings = model.ToEntity(rewardPointsSettings);

            // Scope to avoid duplicate records.
            using (Services.Settings.BeginScope())
            {
                StoreDependingSettings.UpdateSettings(rewardPointsSettings, form, storeScope, Services.Settings);
            }

            // Scope because reward points settings are updated.
            using (Services.Settings.BeginScope())
            {
                var pointsForPurchases = StoreDependingSettings.IsOverrideChecked(rewardPointsSettings, "PointsForPurchases_Amount", form);

                Services.Settings.UpdateSetting(rewardPointsSettings, x => x.PointsForPurchases_Amount, pointsForPurchases, storeScope);
                Services.Settings.UpdateSetting(rewardPointsSettings, x => x.PointsForPurchases_Points, pointsForPurchases, storeScope);
            }

            return NotifyAndRedirect("RewardPoints");
        }


        [Permission(Permissions.Configuration.Setting.Read)]
        public ActionResult Order()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var orderSettings = Services.Settings.LoadSetting<OrderSettings>(storeScope);

            var allStores = Services.StoreService.GetAllStores();
            var store = storeScope == 0 ? Services.StoreContext.CurrentStore : allStores.FirstOrDefault(x => x.Id == storeScope);

            var model = orderSettings.ToModel();

            model.PrimaryStoreCurrencyCode = store.PrimaryStoreCurrency.CurrencyCode;
            model.StoreCount = allStores.Count;

            // Gift card activation/deactivation.
            model.GiftCards_Activated_OrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.GiftCards_Deactivated_OrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.ReturnRequestActions = orderSettings.GetLocalizedSetting(x => x.ReturnRequestActions, languageId, storeScope, false, false);
                locale.ReturnRequestReasons = orderSettings.GetLocalizedSetting(x => x.ReturnRequestReasons, languageId, storeScope, false, false);
            });

            model.OrderIdent = _maintenanceService.GetTableIdent<Order>();

            StoreDependingSettings.GetOverrideKeys(orderSettings, model, storeScope, Services.Settings);

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Order(OrderSettingsModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return Order();
            }

            ModelState.Clear();

            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var orderSettings = Services.Settings.LoadSetting<OrderSettings>(storeScope);
            orderSettings = model.ToEntity(orderSettings);

            // Scope to avoid duplicate records.
            using (Services.Settings.BeginScope())
            {
                StoreDependingSettings.UpdateSettings(orderSettings, form, storeScope, Services.Settings);
            }

            // Scope because order settings are updated.
            using (Services.Settings.BeginScope())
            {
                foreach (var localized in model.Locales)
                {
                    _localizedEntityService.SaveLocalizedSetting(orderSettings, x => x.ReturnRequestActions, localized.ReturnRequestActions, localized.LanguageId, storeScope);
                    _localizedEntityService.SaveLocalizedSetting(orderSettings, x => x.ReturnRequestReasons, localized.ReturnRequestReasons, localized.LanguageId, storeScope);
                }

                if (model.GiftCards_Activated_OrderStatusId.HasValue)
                {
                    Services.Settings.SaveSetting(orderSettings, x => x.GiftCards_Activated_OrderStatusId, 0, false);
                }
                else
                {
                    Services.Settings.DeleteSetting(orderSettings, x => x.GiftCards_Activated_OrderStatusId);
                }

                if (model.GiftCards_Deactivated_OrderStatusId.HasValue)
                {
                    Services.Settings.SaveSetting(orderSettings, x => x.GiftCards_Deactivated_OrderStatusId, 0, false);
                }
                else
                {
                    Services.Settings.DeleteSetting(orderSettings, x => x.GiftCards_Deactivated_OrderStatusId);
                }
            }

            // Order ident.
            if (model.OrderIdent.HasValue)
            {
                try
                {
                    _maintenanceService.SetTableIdent<Order>(model.OrderIdent.Value);
                }
                catch (Exception ex)
                {
                    NotifyError(ex.Message);
                }
            }

            return NotifyAndRedirect("Order");
        }


        [Permission(Permissions.Configuration.Setting.Read)]
        public ActionResult ShoppingCart()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var shoppingCartSettings = Services.Settings.LoadSetting<ShoppingCartSettings>(storeScope);

            var model = shoppingCartSettings.ToModel();

            model.AvailableNewsLetterSubscriptions = shoppingCartSettings.NewsLetterSubscription.ToSelectList();
            model.AvailableThirdPartyEmailHandOver = shoppingCartSettings.ThirdPartyEmailHandOver.ToSelectList();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.ThirdPartyEmailHandOverLabel = shoppingCartSettings.GetLocalizedSetting(x => x.ThirdPartyEmailHandOverLabel, languageId, storeScope, false, false);
            });

            StoreDependingSettings.GetOverrideKeys(shoppingCartSettings, model, storeScope, Services.Settings);

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ShoppingCart(ShoppingCartSettingsModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return ShoppingCart();
            }

            ModelState.Clear();

            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var shoppingCartSettings = Services.Settings.LoadSetting<ShoppingCartSettings>(storeScope);
            shoppingCartSettings = model.ToEntity(shoppingCartSettings);

            // Scope to avoid duplicate ShoppingCartSettings.ThirdPartyEmailHandOverLabel records.
            using (Services.Settings.BeginScope())
            {
                StoreDependingSettings.UpdateSettings(shoppingCartSettings, form, storeScope, Services.Settings);
            }

            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedSetting(shoppingCartSettings, x => x.ThirdPartyEmailHandOverLabel, localized.ThirdPartyEmailHandOverLabel, localized.LanguageId, storeScope);
            }

            return NotifyAndRedirect("ShoppingCart");
        }


        [Permission(Permissions.Configuration.Setting.Read)]
        [LoadSetting]
        public ActionResult Payment(PaymentSettings settings)
        {
            var model = new PaymentSettingsModel();
            model.AvailableCapturePaymentReasons = CapturePaymentReason.OrderShipped.ToSelectList(false).ToList();
            MiniMapper.Map(settings, model);

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost, SaveSetting]
        public ActionResult Payment(PaymentSettings settings, PaymentSettingsModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return Payment(settings);
            }

            ModelState.Clear();
            MiniMapper.Map(model, settings);

            return NotifyAndRedirect("Payment");
        }


        [Permission(Permissions.Configuration.Setting.Read)]
        [LoadSetting]
        public ActionResult Media(MediaSettings mediaSettings)
        {
            var model = mediaSettings.ToModel();

            model.CurrentlyAllowedThumbnailSizes = mediaSettings.GetAllowedThumbnailSizes();

            #region Obsolete
            //model.AvailablePictureZoomTypes.Add(new SelectListItem
            //{
            //    Text = T("Admin.Configuration.Settings.Media.PictureZoomType.Window"),
            //    Value = "window",
            //    Selected = model.PictureZoomType == "window"
            //});
            //model.AvailablePictureZoomTypes.Add(new SelectListItem
            //{
            //    Text = T("Admin.Configuration.Settings.Media.PictureZoomType.Inner"),
            //    Value = "inner",
            //    Selected = model.PictureZoomType == "inner"
            //});
            //model.AvailablePictureZoomTypes.Add(new SelectListItem
            //{
            //    Text = T("Admin.Configuration.Settings.Media.PictureZoomType.Lens"),
            //    Value = "lens",
            //    Selected = model.PictureZoomType == "lens"
            //});
            #endregion

            // Media storage provider.
            var currentStorageProvider = Services.Settings.GetSettingByKey<string>("Media.Storage.Provider");
            var provider = _providerManager.GetProvider<IMediaStorageProvider>(currentStorageProvider);

            model.StorageProvider = (provider != null ? _pluginMediator.GetLocalizedFriendlyName(provider.Metadata) : null);

            model.AvailableStorageProvider = _providerManager.GetAllProviders<IMediaStorageProvider>()
                .Where(x => !x.Metadata.SystemName.IsCaseInsensitiveEqual(currentStorageProvider))
                .Select(x => new SelectListItem { Text = _pluginMediator.GetLocalizedFriendlyName(x.Metadata), Value = x.Metadata.SystemName })
                .ToList();

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost, FormValueRequired("save")]
        [SaveSetting]
        public ActionResult Media(MediaSettings settings, MediaSettingsModel model)
        {
            if (!ModelState.IsValid)
            {
                return Media(settings);
            }

            ModelState.Clear();
            model.ToEntity(settings);

            return NotifyAndRedirect("Media");
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ChangeMediaStorage(string targetProvider)
        {
            var currentStorageProvider = Services.Settings.GetSettingByKey<string>("Media.Storage.Provider");
            var source = _providerManager.GetProvider<IMediaStorageProvider>(currentStorageProvider);
            var target = _providerManager.GetProvider<IMediaStorageProvider>(targetProvider);

            var success = _mediaMover.Value.Move(source, target);

            if (success)
                NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

            return RedirectToAction("Media");
        }


        [Permission(Permissions.Configuration.Setting.Read)]
        public ActionResult CustomerUser()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            StoreDependingSettings.CreateViewDataObject(storeScope);

            var customerSettings = Services.Settings.LoadSetting<CustomerSettings>(storeScope);
            var addressSettings = Services.Settings.LoadSetting<AddressSettings>(storeScope);
            var externalAuthenticationSettings = Services.Settings.LoadSetting<ExternalAuthenticationSettings>(storeScope);
            var privacySettings = Services.Settings.LoadSetting<PrivacySettings>(storeScope);

            var model = new CustomerUserSettingsModel();
            model.CustomerSettings = customerSettings.ToModel();
            model.AddressSettings = addressSettings.ToModel();

            model.ExternalAuthenticationSettings.AutoRegisterEnabled = externalAuthenticationSettings.AutoRegisterEnabled;
            model.PrivacySettings = privacySettings.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Salutations = addressSettings.GetLocalizedSetting(x => x.Salutations, languageId, storeScope, false, false);
            });

            StoreDependingSettings.GetOverrideKeys(addressSettings, model.AddressSettings, storeScope, Services.Settings, false);
            StoreDependingSettings.GetOverrideKeys(privacySettings, model.PrivacySettings, storeScope, Services.Settings, false);
            StoreDependingSettings.GetOverrideKeys(externalAuthenticationSettings, model.ExternalAuthenticationSettings, storeScope, Services.Settings, false);
            StoreDependingSettings.GetOverrideKeys(customerSettings, model.CustomerSettings, storeScope, Services.Settings, false);

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost, ValidateInput(false)]
        public ActionResult CustomerUser(CustomerUserSettingsModel model, FormCollection form)
        {
            var ignoreKey = $"{nameof(model.CustomerSettings)}.{nameof(model.CustomerSettings.RegisterCustomerRoleId)}";

            foreach (var key in ModelState.Keys.Where(x => x.IsCaseInsensitiveEqual(ignoreKey)))
            {
                ModelState[key].Errors.Clear();
            }

            if (!ModelState.IsValid)
            {
                return CustomerUser();
            }

            ModelState.Clear();

            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);

            var customerSettings = Services.Settings.LoadSetting<CustomerSettings>(storeScope);
            customerSettings = model.CustomerSettings.ToEntity(customerSettings);

            var addressSettings = Services.Settings.LoadSetting<AddressSettings>(storeScope);
            addressSettings = model.AddressSettings.ToEntity(addressSettings);

            var authSettings = Services.Settings.LoadSetting<ExternalAuthenticationSettings>(storeScope);
            authSettings.AutoRegisterEnabled = model.ExternalAuthenticationSettings.AutoRegisterEnabled;

            var privacySettings = Services.Settings.LoadSetting<PrivacySettings>(storeScope);
            privacySettings = model.PrivacySettings.ToEntity(privacySettings);

            // Scope to avoid duplicate CustomerSettings.DefaultPasswordFormat records.
            using (Services.Settings.BeginScope())
            {
                StoreDependingSettings.UpdateSettings(customerSettings, form, storeScope, Services.Settings);
                StoreDependingSettings.UpdateSettings(addressSettings, form, storeScope, Services.Settings);
                StoreDependingSettings.UpdateSettings(authSettings, form, storeScope, Services.Settings);
                StoreDependingSettings.UpdateSettings(privacySettings, form, storeScope, Services.Settings);
            }

            // Scope because customer settings are updated.
            using (Services.Settings.BeginScope())
            {
                Services.Settings.SaveSetting(customerSettings, x => x.DefaultPasswordFormat, storeScope, false);
            }

            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedSetting(addressSettings, x => x.Salutations, localized.Salutations, localized.LanguageId, storeScope);
            }

            return NotifyAndRedirect("CustomerUser");
        }


        #region CookieInfos

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CookieInfoList(GridCommand command)
        {
            var data = _cookieManager.GetAllCookieInfos();
            var systemCookies = string.Join(",", data.Select(x => x.Name).ToArray());
            var privacySettings = Services.Settings.LoadSetting<PrivacySettings>();

            if (privacySettings.CookieInfos.HasValue())
            {
                data.AddRange(JsonConvert.DeserializeObject<List<CookieInfo>>(privacySettings.CookieInfos)
                    .OrderBy(x => x.CookieType)
                    .ThenBy(x => x.Name));
            }

            var model = new GridModel<CookieInfoModel>
            {
                Data = data
                    .Select(x =>
                    {
                        return new CookieInfoModel
                        {
                            CookieType = x.CookieType,
                            Name = x.Name,
                            Description = x.Description,
                            IsPluginInfo = systemCookies.Contains(x.Name),
                            CookieTypeName = x.CookieType.ToString()
                        };
                    })
                    .ToList(),
                Total = data.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult CookieInfoDelete(string name, GridCommand command)
        {
            // First deserialize setting.
            var privacySettings = Services.Settings.LoadSetting<PrivacySettings>();

            var ciList = JsonConvert.DeserializeObject<List<CookieInfo>>(privacySettings.CookieInfos);
            ciList.Remove(x => x.Name.IsCaseInsensitiveEqual(name));

            // Now serialize again.
            privacySettings.CookieInfos = JsonConvert.SerializeObject(ciList, Formatting.None);

            // Save setting.
            Services.Settings.SaveSetting(privacySettings, x => x.CookieInfos, 0, true);

            return CookieInfoList(command);
        }

        public ActionResult CookieInfoCreatePopup()
        {
            var model = new CookieInfoModel();

            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public ActionResult CookieInfoCreatePopup(string btnId, string formId, CookieInfoModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Deserialize
            var privacySettings = Services.Settings.LoadSetting<PrivacySettings>();
            var ciList = JsonConvert.DeserializeObject<List<CookieInfo>>(privacySettings.CookieInfos);

            if (ciList == null)
                ciList = new List<CookieInfo>();

            var cookieInfo = ciList
                .Select(x => x)
                .Where(x => x.Name.IsCaseInsensitiveEqual(model.Name))
                .FirstOrDefault();

            if (cookieInfo != null)
            {
                // Remove item if it's already there.
                ciList.Remove(x => x.Name.IsCaseInsensitiveEqual(cookieInfo.Name));
            }

            cookieInfo = new CookieInfo
            {
                // TODO: Use MiniMapper
                CookieType = model.CookieType,
                Name = model.Name,
                Description = model.Description,
                SelectedStoreIds = model.SelectedStoreIds
            };

            ciList.Add(cookieInfo);

            // Serialize
            privacySettings.CookieInfos = JsonConvert.SerializeObject(ciList, Formatting.None);

            // Now save again.
            Services.Settings.SaveSetting(privacySettings, x => x.CookieInfos, 0, true);

            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(cookieInfo, x => x.Name, localized.Name, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(cookieInfo, x => x.Description, localized.Description, localized.LanguageId);
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            return View(model);
        }

        [AdminAuthorize]
        public ActionResult CookieInfoEditPopup(string name)
        {
            var privacySettings = Services.Settings.LoadSetting<PrivacySettings>();
            var ciList = JsonConvert.DeserializeObject<List<CookieInfo>>(privacySettings.CookieInfos);
            var cookieInfo = ciList
                .Select(x => x)
                .Where(x => x.Name.IsCaseInsensitiveEqual(name))
                .FirstOrDefault();

            if (cookieInfo == null)
            {
                NotifyError(T("Admin.Configuration.Settings.CustomerUser.Privacy.Cookies.CookieInfoNotFound"));
                return View(new CookieInfoModel());
            }

            var model = new CookieInfoModel
            {
                CookieType = cookieInfo.CookieType,
                Name = cookieInfo.Name,
                Description = cookieInfo.Description,
                SelectedStoreIds = cookieInfo.SelectedStoreIds
            };

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = cookieInfo.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = cookieInfo.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminAuthorize]
        public ActionResult CookieInfoEditPopup(string btnId, string formId, CookieInfoModel model)
        {
            var privacySettings = Services.Settings.LoadSetting<PrivacySettings>();
            var ciList = JsonConvert.DeserializeObject<List<CookieInfo>>(privacySettings.CookieInfos);
            var cookieInfo = ciList
                .Select(x => x)
                .Where(x => x.Name.IsCaseInsensitiveEqual(model.Name))
                .FirstOrDefault();

            if (cookieInfo == null)
            {
                NotifyError(T("Admin.Configuration.Settings.CustomerUser.Privacy.Cookies.CookieInfoNotFound"));
                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(new CookieInfoModel());
            }

            if (ModelState.IsValid)
            {
                cookieInfo.Name = model.Name;
                cookieInfo.Description = model.Description;
                cookieInfo.CookieType = model.CookieType;
                cookieInfo.SelectedStoreIds = model.SelectedStoreIds;

                ciList.Remove(x => x.Name.IsCaseInsensitiveEqual(cookieInfo.Name));
                ciList.Add(cookieInfo);

                privacySettings.CookieInfos = JsonConvert.SerializeObject(ciList, Formatting.None);

                Services.Settings.SaveSetting(privacySettings, x => x.CookieInfos, 0, true);

                foreach (var localized in model.Locales)
                {
                    _localizedEntityService.SaveLocalizedValue(cookieInfo, x => x.Name, localized.Name, localized.LanguageId);
                    _localizedEntityService.SaveLocalizedValue(cookieInfo, x => x.Description, localized.Description, localized.LanguageId);
                }

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
            }

            return View(model);
        }

        #endregion


        [Permission(Permissions.Configuration.Setting.Read)]
        [LoadSetting(IsRootedModel = true)]
        public ActionResult GeneralCommon(int storeScope,
            StoreInformationSettings storeInformationSettings,
            SeoSettings seoSettings,
            DateTimeSettings dateTimeSettings,
            SecuritySettings securitySettings,
            CaptchaSettings captchaSettings,
            PdfSettings pdfSettings,
            LocalizationSettings localizationSettings,
            CompanyInformationSettings companySettings,
            ContactDataSettings contactDataSettings,
            BankConnectionSettings bankConnectionSettings,
            SocialSettings socialSettings,
            HomePageSettings homePageSettings)
        {
            // Set page timeout to 5 minutes.
            Server.ScriptTimeout = 300;

            var model = new GeneralCommonSettingsModel();

            // Map entities to model
            MiniMapper.Map(storeInformationSettings, model.StoreInformationSettings);
            MiniMapper.Map(seoSettings, model.SeoSettings);
            MiniMapper.Map(dateTimeSettings, model.DateTimeSettings);
            MiniMapper.Map(securitySettings, model.SecuritySettings);
            MiniMapper.Map(captchaSettings, model.CaptchaSettings);
            MiniMapper.Map(pdfSettings, model.PdfSettings);
            MiniMapper.Map(localizationSettings, model.LocalizationSettings);
            MiniMapper.Map(companySettings, model.CompanyInformationSettings);
            MiniMapper.Map(contactDataSettings, model.ContactDataSettings);
            MiniMapper.Map(bankConnectionSettings, model.BankConnectionSettings);
            MiniMapper.Map(socialSettings, model.SocialSettings);
            MiniMapper.Map(homePageSettings, model.HomepageSettings);

            #region SEO custom mapping

            // Fix for Disallows & Allows joined with comma in MiniMapper (we need NewLine).
            model.SeoSettings.ExtraRobotsDisallows = string.Join(Environment.NewLine, seoSettings.ExtraRobotsDisallows);
            model.SeoSettings.ExtraRobotsAllows = string.Join(Environment.NewLine, seoSettings.ExtraRobotsAllows);

            model.SeoSettings.MetaTitle = seoSettings.MetaTitle;
            model.SeoSettings.MetaDescription = seoSettings.MetaDescription;
            model.SeoSettings.MetaKeywords = seoSettings.MetaKeywords;

            AddLocales(_languageService, model.SeoSettings.Locales, (locale, languageId) =>
            {
                locale.MetaTitle = seoSettings.GetLocalizedSetting(x => x.MetaTitle, languageId, storeScope, false, false);
                locale.MetaDescription = seoSettings.GetLocalizedSetting(x => x.MetaDescription, languageId, storeScope, false, false);
                locale.MetaKeywords = seoSettings.GetLocalizedSetting(x => x.MetaKeywords, languageId, storeScope, false, false);
            });

            model.HomepageSettings.MetaTitle = homePageSettings.MetaTitle;
            model.HomepageSettings.MetaDescription = homePageSettings.MetaDescription;
            model.HomepageSettings.MetaKeywords = homePageSettings.MetaKeywords;

            AddLocales(_languageService, model.HomepageSettings.Locales, (locale, languageId) =>
            {
                locale.MetaTitle = homePageSettings.GetLocalizedSetting(x => x.MetaTitle, languageId, storeScope, false, false);
                locale.MetaDescription = homePageSettings.GetLocalizedSetting(x => x.MetaDescription, languageId, storeScope, false, false);
                locale.MetaKeywords = homePageSettings.GetLocalizedSetting(x => x.MetaKeywords, languageId, storeScope, false, false);
            });

            #endregion

            PrepareConfigurationModel(model);

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost, SaveSetting(IsRootedModel = true), FormValueRequired("save")]
        public ActionResult GeneralCommon(
            GeneralCommonSettingsModel model,
            int storeScope,
            StoreInformationSettings storeInformationSettings,
            SeoSettings seoSettings,
            DateTimeSettings dateTimeSettings,
            SecuritySettings securitySettings,
            CaptchaSettings captchaSettings,
            PdfSettings pdfSettings,
            LocalizationSettings localizationSettings,
            CompanyInformationSettings companySettings,
            ContactDataSettings contactDataSettings,
            BankConnectionSettings bankConnectionSettings,
            SocialSettings socialSettings,
            HomePageSettings homePageSeoSettings)
        {
            if (!ModelState.IsValid)
            {
                PrepareConfigurationModel(model);
                return View(model);
            }

            ModelState.Clear();

            // Necessary before mapping
            var resetUserSeoCharacterTable = (seoSettings.SeoNameCharConversion != model.SeoSettings.SeoNameCharConversion);
            var clearSeoFriendlyUrls = localizationSettings.SeoFriendlyUrlsForLanguagesEnabled != model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled;
            var prevPdfLogoId = pdfSettings.LogoPictureId;

            // Map model to entities
            MiniMapper.Map(model.StoreInformationSettings, storeInformationSettings);
            MiniMapper.Map(model.SeoSettings, seoSettings);
            MiniMapper.Map(model.DateTimeSettings, dateTimeSettings);
            MiniMapper.Map(model.SecuritySettings, securitySettings);
            MiniMapper.Map(model.CaptchaSettings, captchaSettings);
            MiniMapper.Map(model.PdfSettings, pdfSettings);
            MiniMapper.Map(model.LocalizationSettings, localizationSettings);
            MiniMapper.Map(model.CompanyInformationSettings, companySettings);
            MiniMapper.Map(model.ContactDataSettings, contactDataSettings);
            MiniMapper.Map(model.BankConnectionSettings, bankConnectionSettings);
            MiniMapper.Map(model.SocialSettings, socialSettings);
            MiniMapper.Map(model.HomepageSettings, homePageSeoSettings);

            #region POST mapping

            // Set CountryId explicitly else it can't be resetted.
            companySettings.CountryId = model.CompanyInformationSettings.CountryId ?? 0; 

            // (Un)track PDF logo id
            _mediaTracker.Value.Track(pdfSettings, prevPdfLogoId, x => x.LogoPictureId);

            seoSettings.MetaTitle = model.SeoSettings.MetaTitle;
            seoSettings.MetaDescription = model.SeoSettings.MetaDescription;
            seoSettings.MetaKeywords = model.SeoSettings.MetaKeywords;

            foreach (var localized in model.SeoSettings.Locales)
            {
                _localizedEntityService.SaveLocalizedSetting(seoSettings, x => x.MetaTitle, localized.MetaTitle, localized.LanguageId, storeScope);
                _localizedEntityService.SaveLocalizedSetting(seoSettings, x => x.MetaDescription, localized.MetaDescription, localized.LanguageId, storeScope);
                _localizedEntityService.SaveLocalizedSetting(seoSettings, x => x.MetaKeywords, localized.MetaKeywords, localized.LanguageId, storeScope);
            }

            homePageSeoSettings.MetaTitle = model.HomepageSettings.MetaTitle;
            homePageSeoSettings.MetaDescription = model.HomepageSettings.MetaDescription;
            homePageSeoSettings.MetaKeywords = model.HomepageSettings.MetaKeywords;

            foreach (var localized in model.HomepageSettings.Locales)
            {
                _localizedEntityService.SaveLocalizedSetting(homePageSeoSettings, x => x.MetaTitle, localized.MetaTitle, localized.LanguageId, storeScope);
                _localizedEntityService.SaveLocalizedSetting(homePageSeoSettings, x => x.MetaDescription, localized.MetaDescription, localized.LanguageId, storeScope);
                _localizedEntityService.SaveLocalizedSetting(homePageSeoSettings, x => x.MetaKeywords, localized.MetaKeywords, localized.LanguageId, storeScope);
            }

            if (resetUserSeoCharacterTable)
            {
                SeoHelper.ResetUserSeoCharacterTable();
            }

            if (clearSeoFriendlyUrls)
            {
                LocalizedRoute.ClearSeoFriendlyUrlsCachedValue();
            }

            #endregion

            // Does not contain any store specific settings
            Services.Settings.SaveSetting(securitySettings);

            return NotifyAndRedirect("GeneralCommon");
        }

        private void PrepareConfigurationModel(GeneralCommonSettingsModel model)
        {
            foreach (var timeZone in _dateTimeHelper.GetSystemTimeZones())
            {
                model.DateTimeSettings.AvailableTimeZones.Add(new SelectListItem
                {
                    Text = timeZone.DisplayName,
                    Value = timeZone.Id,
                    Selected = timeZone.Id.Equals(_dateTimeHelper.DefaultStoreTimeZone.Id, StringComparison.InvariantCultureIgnoreCase)
                });
            }

            #region CompanyInfo custom mapping

            foreach (var c in _countryService.GetAllCountries(true))
            {
                model.CompanyInformationSettings.AvailableCountries.Add(new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString(),
                    Selected = (c.Id == model.CompanyInformationSettings.CountryId)
                });
            }

            model.CompanyInformationSettings.Salutations.AddRange(new[]
            {
                ResToSelectListItem("Admin.Address.Salutation.Mr"),
                ResToSelectListItem("Admin.Address.Salutation.Mrs")
            });

            var resRoot = "Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.";
            model.CompanyInformationSettings.ManagementDescriptions.AddRange(new[]
            {
                ResToSelectListItem(resRoot + "Manager"),
                ResToSelectListItem(resRoot + "Shopkeeper"),
                ResToSelectListItem(resRoot + "Procurator"),
                ResToSelectListItem(resRoot + "Shareholder"),
                ResToSelectListItem(resRoot + "AuthorizedPartner"),
                ResToSelectListItem(resRoot + "Director"),
                ResToSelectListItem(resRoot + "ManagingPartner")
            });

            #endregion
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost, ActionName("GeneralCommon"), FormValueRequired("changeencryptionkey")]
        public ActionResult ChangeEnryptionKey(GeneralCommonSettingsModel model)
        {
            // Set page timeout to 5 minutes.
            Server.ScriptTimeout = 300;

            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var securitySettings = Services.Settings.LoadSetting<SecuritySettings>(storeScope);
            var oldEncryptionPrivateKey = securitySettings.EncryptionKey;
            var newEncryptionPrivateKey = model.SecuritySettings.EncryptionKey.EmptyNull();

            if (newEncryptionPrivateKey.IsEmpty() || newEncryptionPrivateKey.Length != 16)
            {
                NotifyError(T("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TooShort"));
                return RedirectToAction("GeneralCommon");
            }

            if (oldEncryptionPrivateKey == newEncryptionPrivateKey)
            {
                NotifyError(T("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TheSame"));
                return RedirectToAction("GeneralCommon");
            }

            try
            {
                // Update encrypted order info.
                var orderQuery = _orderService.GetOrders(0, 0, null, null, null, null, null, null, null);
                orderQuery = orderQuery.OrderByDescending(x => x.CreatedOnUtc);
                IPagedList<Order> orders = null;
                var pageIndex = 0;

                do
                {
                    orders = new PagedList<Order>(orderQuery, pageIndex++, 1000);

                    foreach (var order in orders)
                    {
                        // New credit card encryption.
                        string decryptedCardType = _encryptionService.DecryptText(order.CardType, oldEncryptionPrivateKey);
                        string decryptedCardName = _encryptionService.DecryptText(order.CardName, oldEncryptionPrivateKey);
                        string decryptedCardNumber = _encryptionService.DecryptText(order.CardNumber, oldEncryptionPrivateKey);
                        string decryptedMaskedCreditCardNumber = _encryptionService.DecryptText(order.MaskedCreditCardNumber, oldEncryptionPrivateKey);
                        string decryptedCardCvv2 = _encryptionService.DecryptText(order.CardCvv2, oldEncryptionPrivateKey);
                        string decryptedCardExpirationMonth = _encryptionService.DecryptText(order.CardExpirationMonth, oldEncryptionPrivateKey);
                        string decryptedCardExpirationYear = _encryptionService.DecryptText(order.CardExpirationYear, oldEncryptionPrivateKey);

                        string encryptedCardType = _encryptionService.EncryptText(decryptedCardType, newEncryptionPrivateKey);
                        string encryptedCardName = _encryptionService.EncryptText(decryptedCardName, newEncryptionPrivateKey);
                        string encryptedCardNumber = _encryptionService.EncryptText(decryptedCardNumber, newEncryptionPrivateKey);
                        string encryptedMaskedCreditCardNumber = _encryptionService.EncryptText(decryptedMaskedCreditCardNumber, newEncryptionPrivateKey);
                        string encryptedCardCvv2 = _encryptionService.EncryptText(decryptedCardCvv2, newEncryptionPrivateKey);
                        string encryptedCardExpirationMonth = _encryptionService.EncryptText(decryptedCardExpirationMonth, newEncryptionPrivateKey);
                        string encryptedCardExpirationYear = _encryptionService.EncryptText(decryptedCardExpirationYear, newEncryptionPrivateKey);

                        order.CardType = encryptedCardType;
                        order.CardName = encryptedCardName;
                        order.CardNumber = encryptedCardNumber;
                        order.MaskedCreditCardNumber = encryptedMaskedCreditCardNumber;
                        order.CardCvv2 = encryptedCardCvv2;
                        order.CardExpirationMonth = encryptedCardExpirationMonth;
                        order.CardExpirationYear = encryptedCardExpirationYear;

                        // New direct debit encryption.
                        string decryptedAccountHolder = _encryptionService.DecryptText(order.DirectDebitAccountHolder, oldEncryptionPrivateKey);
                        string decryptedAccountNumber = _encryptionService.DecryptText(order.DirectDebitAccountNumber, oldEncryptionPrivateKey);
                        string decryptedBankCode = _encryptionService.DecryptText(order.DirectDebitBankCode, oldEncryptionPrivateKey);
                        string decryptedBankName = _encryptionService.DecryptText(order.DirectDebitBankName, oldEncryptionPrivateKey);
                        string decryptedBic = _encryptionService.DecryptText(order.DirectDebitBIC, oldEncryptionPrivateKey);
                        string decryptedCountry = _encryptionService.DecryptText(order.DirectDebitCountry, oldEncryptionPrivateKey);
                        string decryptedIban = _encryptionService.DecryptText(order.DirectDebitIban, oldEncryptionPrivateKey);

                        string encryptedAccountHolder = _encryptionService.EncryptText(decryptedAccountHolder, newEncryptionPrivateKey);
                        string encryptedAccountNumber = _encryptionService.EncryptText(decryptedAccountNumber, newEncryptionPrivateKey);
                        string encryptedBankCode = _encryptionService.EncryptText(decryptedBankCode, newEncryptionPrivateKey);
                        string encryptedBankName = _encryptionService.EncryptText(decryptedBankName, newEncryptionPrivateKey);
                        string encryptedBic = _encryptionService.EncryptText(decryptedBic, newEncryptionPrivateKey);
                        string encryptedCountry = _encryptionService.EncryptText(decryptedCountry, newEncryptionPrivateKey);
                        string encryptedIban = _encryptionService.EncryptText(decryptedIban, newEncryptionPrivateKey);

                        order.DirectDebitAccountHolder = encryptedAccountHolder;
                        order.DirectDebitAccountNumber = encryptedAccountNumber;
                        order.DirectDebitBankCode = encryptedBankCode;
                        order.DirectDebitBankName = encryptedBankName;
                        order.DirectDebitBIC = encryptedBic;
                        order.DirectDebitCountry = encryptedCountry;
                        order.DirectDebitIban = encryptedIban;

                        _orderService.UpdateOrder(order);
                    }
                }
                while (orders.HasNextPage);

                // Update user information.
                var customerQuery = _customerService.GetAllCustomersByPasswordFormat(PasswordFormat.Encrypted).SourceQuery;
                PagedList<Customer> customers = null;
                pageIndex = 0;

                do
                {
                    customers = new PagedList<Customer>(customerQuery, pageIndex++, 1000);

                    if (customers.Any())
                    {
                        foreach (var customer in customers)
                        {
                            var decryptedPassword = _encryptionService.DecryptText(customer.Password, oldEncryptionPrivateKey);
                            var encryptedPassword = _encryptionService.EncryptText(decryptedPassword, newEncryptionPrivateKey);

                            customer.Password = encryptedPassword;
                        }

                        Services.DbContext.SaveChanges();
                    }
                }
                while (customers.HasNextPage);

                securitySettings.EncryptionKey = newEncryptionPrivateKey;
                Services.Settings.SaveSetting(securitySettings);

                NotifySuccess(T("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.Changed"));
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                Logger.Error(ex);
            }

            return RedirectToAction("GeneralCommon");
        }

        [HttpPost]
        [Permission(Permissions.Configuration.Setting.Read)]
        public ActionResult TestSeoNameCreation(GeneralCommonSettingsModel model)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var seoSettings = Services.Settings.LoadSetting<SeoSettings>(storeScope);

            // We always test against persisted settings.
            var result = SeoHelper.GetSeName(model.SeoSettings.TestSeoNameCreation,
                seoSettings.ConvertNonWesternChars,
                seoSettings.AllowUnicodeCharsInUrls,
                seoSettings.SeoNameCharConversion);

            return Content(result);
        }


        [Permission(Permissions.Configuration.Setting.Read)]
        [LoadSetting]
        public ActionResult DataExchange(DataExchangeSettings settings)
        {
            var model = new DataExchangeSettingsModel();
            MiniMapper.Map(settings, model);

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost, SaveSetting]
        public ActionResult DataExchange(DataExchangeSettings settings, DataExchangeSettingsModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
            {
                return DataExchange(settings);
            }

            ModelState.Clear();
            MiniMapper.Map(model, settings);

            return NotifyAndRedirect("DataExchange");
        }


        [Permission(Permissions.Configuration.Setting.Read)]
        public ActionResult Search()
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<SearchSettings>(storeScope);
            var fsettings = Services.Settings.LoadSetting<ForumSearchSettings>(storeScope);
            var megaSearchDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.MegaSearch");
            var megaSearchPlusDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.MegaSearchPlus");

            var model = new SearchSettingsModel();
            MiniMapper.Map(settings, model);
            MiniMapper.Map(fsettings, model.ForumSearchSettings);

            model.IsMegaSearchInstalled = megaSearchDescriptor != null;
            model.AvailableSortOrderModes = settings.DefaultSortOrder.ToSelectList();
            model.ForumSearchSettings.AvailableDefaultSortOrders = fsettings.DefaultSortOrder.ToSelectList();

            if (megaSearchDescriptor == null)
            {
                model.SearchFieldsNote = T("Admin.Configuration.Settings.Search.SearchFieldsNote");

                model.AvailableSearchFields = new List<SelectListItem>
                {
                    new SelectListItem { Text = T("Admin.Catalog.Products.Fields.ShortDescription"), Value = "shortdescription" },
                    new SelectListItem { Text = T("Admin.Catalog.Products.Fields.Sku"), Value = "sku" },
                };

                model.AvailableSearchModes = settings.SearchMode.ToSelectList().Where(x => x.Value.ToInt() != (int)SearchMode.ExactMatch).ToList();
                model.ForumSearchSettings.AvailableSearchModes = fsettings.SearchMode.ToSelectList().Where(x => x.Value.ToInt() != (int)SearchMode.ExactMatch).ToList();
            }
            else
            {
                model.AvailableSearchFields = new List<SelectListItem>
                {
                    new SelectListItem { Text = T("Admin.Catalog.Products.Fields.ShortDescription"), Value = "shortdescription" },
                    new SelectListItem { Text = T("Admin.Catalog.Products.Fields.FullDescription"), Value = "fulldescription" },
                    new SelectListItem { Text = T("Admin.Catalog.Products.Fields.ProductTags"), Value = "tagname" },
                    new SelectListItem { Text = T("Admin.Catalog.Manufacturers"), Value = "manufacturer" },
                    new SelectListItem { Text = T("Admin.Catalog.Categories"), Value = "category" },
                    new SelectListItem { Text = T("Admin.Catalog.Products.Fields.Sku"), Value = "sku" },
                    new SelectListItem { Text = T("Admin.Catalog.Products.Fields.GTIN"), Value = "gtin" },
                    new SelectListItem { Text = T("Admin.Catalog.Products.Fields.ManufacturerPartNumber"), Value = "mpn" }
                };

                if (megaSearchPlusDescriptor != null)
                {
                    model.AvailableSearchFields.Add(new SelectListItem { Text = T("Search.Fields.SpecificationAttributeOptionName"), Value = "attrname" });
                    model.AvailableSearchFields.Add(new SelectListItem { Text = T("Search.Fields.ProductAttributeOptionName"), Value = "variantname" });
                }

                model.AvailableSearchModes = settings.SearchMode.ToSelectList().ToList();
                model.ForumSearchSettings.AvailableSearchModes = fsettings.SearchMode.ToSelectList().ToList();
            }

            model.ForumSearchSettings.AvailableSearchFields = new List<SelectListItem>
            {
                new SelectListItem { Text = T("Admin.Customers.Customers.Fields.Username"), Value = "username" },
                new SelectListItem { Text = T("Forum.PostText"), Value = "text" },
            };

            // Common facets.
            model.BrandFacet.Disabled = settings.BrandDisabled;
            model.BrandFacet.DisplayOrder = settings.BrandDisplayOrder;
            model.PriceFacet.Disabled = settings.PriceDisabled;
            model.PriceFacet.DisplayOrder = settings.PriceDisplayOrder;
            model.RatingFacet.Disabled = settings.RatingDisabled;
            model.RatingFacet.DisplayOrder = settings.RatingDisplayOrder;
            model.DeliveryTimeFacet.Disabled = settings.DeliveryTimeDisabled;
            model.DeliveryTimeFacet.DisplayOrder = settings.DeliveryTimeDisplayOrder;
            model.AvailabilityFacet.Disabled = settings.AvailabilityDisabled;
            model.AvailabilityFacet.DisplayOrder = settings.AvailabilityDisplayOrder;
            model.AvailabilityFacet.IncludeNotAvailable = settings.IncludeNotAvailable;
            model.NewArrivalsFacet.Disabled = settings.NewArrivalsDisabled;
            model.NewArrivalsFacet.DisplayOrder = settings.NewArrivalsDisplayOrder;

            model.ForumSearchSettings.ForumFacet.Disabled = fsettings.ForumDisabled;
            model.ForumSearchSettings.ForumFacet.DisplayOrder = fsettings.ForumDisplayOrder;
            model.ForumSearchSettings.CustomerFacet.Disabled = fsettings.CustomerDisabled;
            model.ForumSearchSettings.CustomerFacet.DisplayOrder = fsettings.CustomerDisplayOrder;
            model.ForumSearchSettings.DateFacet.Disabled = fsettings.DateDisabled;
            model.ForumSearchSettings.DateFacet.DisplayOrder = fsettings.DateDisplayOrder;

            // Localized facet settings (CommonFacetSettingsLocalizedModel).
            foreach (var language in _languageService.GetAllLanguages(true))
            {
                model.CategoryFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
                {
                    LanguageId = language.Id,
                    Alias = Services.Settings.GetSettingByKey<string>(FacetUtility.GetFacetAliasSettingKey(FacetGroupKind.Category, language.Id))
                });
                model.BrandFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
                {
                    LanguageId = language.Id,
                    Alias = Services.Settings.GetSettingByKey<string>(FacetUtility.GetFacetAliasSettingKey(FacetGroupKind.Brand, language.Id))
                });
                model.PriceFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
                {
                    LanguageId = language.Id,
                    Alias = Services.Settings.GetSettingByKey<string>(FacetUtility.GetFacetAliasSettingKey(FacetGroupKind.Price, language.Id))
                });
                model.RatingFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
                {
                    LanguageId = language.Id,
                    Alias = Services.Settings.GetSettingByKey<string>(FacetUtility.GetFacetAliasSettingKey(FacetGroupKind.Rating, language.Id))
                });
                model.DeliveryTimeFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
                {
                    LanguageId = language.Id,
                    Alias = Services.Settings.GetSettingByKey<string>(FacetUtility.GetFacetAliasSettingKey(FacetGroupKind.DeliveryTime, language.Id))
                });
                model.AvailabilityFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
                {
                    LanguageId = language.Id,
                    Alias = Services.Settings.GetSettingByKey<string>(FacetUtility.GetFacetAliasSettingKey(FacetGroupKind.Availability, language.Id))
                });
                model.NewArrivalsFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
                {
                    LanguageId = language.Id,
                    Alias = Services.Settings.GetSettingByKey<string>(FacetUtility.GetFacetAliasSettingKey(FacetGroupKind.NewArrivals, language.Id))
                });

                model.ForumSearchSettings.ForumFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
                {
                    LanguageId = language.Id,
                    Alias = Services.Settings.GetSettingByKey<string>(FacetUtility.GetFacetAliasSettingKey(FacetGroupKind.Forum, language.Id, "Forum"))
                });
                model.ForumSearchSettings.CustomerFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
                {
                    LanguageId = language.Id,
                    Alias = Services.Settings.GetSettingByKey<string>(FacetUtility.GetFacetAliasSettingKey(FacetGroupKind.Customer, language.Id, "Forum"))
                });
                model.ForumSearchSettings.DateFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
                {
                    LanguageId = language.Id,
                    Alias = Services.Settings.GetSettingByKey<string>(FacetUtility.GetFacetAliasSettingKey(FacetGroupKind.Date, language.Id, "Forum"))
                });
            }

            StoreDependingSettings.GetOverrideKeys(settings, model, storeScope, Services.Settings);
            StoreDependingSettings.GetOverrideKeys(fsettings, model.ForumSearchSettings, storeScope, Services.Settings, false);

            // Facet settings (CommonFacetSettingsModel).
            foreach (var prefix in new string[] { "Brand", "Price", "Rating", "DeliveryTime", "Availability", "NewArrivals" })
            {
                StoreDependingSettings.GetOverrideKey(prefix + "Facet.Disabled", prefix + "Disabled", settings, storeScope, Services.Settings);
                StoreDependingSettings.GetOverrideKey(prefix + "Facet.DisplayOrder", prefix + "DisplayOrder", settings, storeScope, Services.Settings);
            }

            foreach (var prefix in new string[] { "ForumSearchSettings.Forum", "ForumSearchSettings.Customer", "ForumSearchSettings.Date" })
            {
                StoreDependingSettings.GetOverrideKey(prefix + "Facet.Disabled", prefix + "Disabled", fsettings, storeScope, Services.Settings);
                StoreDependingSettings.GetOverrideKey(prefix + "Facet.DisplayOrder", prefix + "DisplayOrder", fsettings, storeScope, Services.Settings);
            }

            // Facet settings with a non-prefixed name.
            StoreDependingSettings.GetOverrideKey("AvailabilityFacet.IncludeNotAvailable", "IncludeNotAvailable", settings, storeScope, Services.Settings);

            return View(model);
        }

        [Permission(Permissions.Configuration.Setting.Update)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult Search(SearchSettingsModel model, FormCollection form)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<SearchSettings>(storeScope);
            var fsettings = Services.Settings.LoadSetting<ForumSearchSettings>(storeScope);

            var validator = new SearchSettingValidator(T, x =>
            {
                return storeScope == 0 || StoreDependingSettings.IsOverrideChecked(settings, x, form);
            });
            validator.Validate(model, ModelState);

            var fvalidator = new ForumSearchSettingValidator(T, x =>
            {
                return storeScope == 0 || StoreDependingSettings.IsOverrideChecked(fsettings, x, form);
            });
            var fvResult = fvalidator.Validate(model.ForumSearchSettings);
            if (!fvResult.IsValid)
            {
                fvResult.Errors.Each(x => ModelState.AddModelError(string.Concat(nameof(model.ForumSearchSettings), ".", x.PropertyName), x.ErrorMessage));
            }

            if (!ModelState.IsValid)
            {
                return Search();
            }

            CategoryTreeChangeReason? categoriesChange = model.AvailabilityFacet.IncludeNotAvailable != settings.IncludeNotAvailable
                ? CategoryTreeChangeReason.ElementCounts
                : (CategoryTreeChangeReason?)null;

            ModelState.Clear();
            MiniMapper.Map(model, settings);
            MiniMapper.Map(model.ForumSearchSettings, fsettings);

            // Common facets.
            settings.BrandDisabled = model.BrandFacet.Disabled;
            settings.BrandDisplayOrder = model.BrandFacet.DisplayOrder;
            settings.PriceDisabled = model.PriceFacet.Disabled;
            settings.PriceDisplayOrder = model.PriceFacet.DisplayOrder;
            settings.RatingDisabled = model.RatingFacet.Disabled;
            settings.RatingDisplayOrder = model.RatingFacet.DisplayOrder;
            settings.DeliveryTimeDisabled = model.DeliveryTimeFacet.Disabled;
            settings.DeliveryTimeDisplayOrder = model.DeliveryTimeFacet.DisplayOrder;
            settings.AvailabilityDisabled = model.AvailabilityFacet.Disabled;
            settings.AvailabilityDisplayOrder = model.AvailabilityFacet.DisplayOrder;
            settings.IncludeNotAvailable = model.AvailabilityFacet.IncludeNotAvailable;
            settings.NewArrivalsDisabled = model.NewArrivalsFacet.Disabled;
            settings.NewArrivalsDisplayOrder = model.NewArrivalsFacet.DisplayOrder;

            fsettings.ForumDisabled = model.ForumSearchSettings.ForumFacet.Disabled;
            fsettings.ForumDisplayOrder = model.ForumSearchSettings.ForumFacet.DisplayOrder;
            fsettings.CustomerDisabled = model.ForumSearchSettings.CustomerFacet.Disabled;
            fsettings.CustomerDisplayOrder = model.ForumSearchSettings.CustomerFacet.DisplayOrder;
            fsettings.DateDisabled = model.ForumSearchSettings.DateFacet.Disabled;
            fsettings.DateDisplayOrder = model.ForumSearchSettings.DateFacet.DisplayOrder;

            // Scope to avoid duplicate SearchSettings.SearchFields records.
            using (Services.Settings.BeginScope())
            {
                StoreDependingSettings.UpdateSettings(settings, form, storeScope, Services.Settings);
                StoreDependingSettings.UpdateSettings(fsettings, form, storeScope, Services.Settings);
            }

            var clearCatalogFacetCache = false;
            var clearForumFacetCache = false;
            using (Services.Settings.BeginScope())
            {
                Services.Settings.SaveSetting(settings, x => x.SearchFields, 0, false);
                Services.Settings.SaveSetting(fsettings, x => x.SearchFields, 0, false);

                // Facet settings (CommonFacetSettingsModel).
                foreach (var prefix in new string[] { "Brand", "Price", "Rating", "DeliveryTime", "Availability", "NewArrivals" })
                {
                    StoreDependingSettings.UpdateSetting(prefix + "Facet.Disabled", prefix + "Disabled", settings, form, storeScope, Services.Settings);
                    StoreDependingSettings.UpdateSetting(prefix + "Facet.DisplayOrder", prefix + "DisplayOrder", settings, form, storeScope, Services.Settings);
                }

                foreach (var prefix in new string[] { "ForumSearchSettings.Forum", "ForumSearchSettings.Customer", "ForumSearchSettings.Date" })
                {
                    StoreDependingSettings.UpdateSetting(prefix + "Facet.Disabled", prefix + "Disabled", settings, form, storeScope, Services.Settings);
                    StoreDependingSettings.UpdateSetting(prefix + "Facet.DisplayOrder", prefix + "DisplayOrder", fsettings, form, storeScope, Services.Settings);
                }

                // Facet settings with a non-prefixed name.
                StoreDependingSettings.UpdateSetting("AvailabilityFacet.IncludeNotAvailable", "IncludeNotAvailable", settings, form, storeScope, Services.Settings);

                // Localized facet settings (CommonFacetSettingsLocalizedModel).
                UpdateLocalizedFacetSetting(model.CategoryFacet, FacetGroupKind.Category, ref clearCatalogFacetCache);
                UpdateLocalizedFacetSetting(model.BrandFacet, FacetGroupKind.Brand, ref clearCatalogFacetCache);
                UpdateLocalizedFacetSetting(model.PriceFacet, FacetGroupKind.Price, ref clearCatalogFacetCache);
                UpdateLocalizedFacetSetting(model.RatingFacet, FacetGroupKind.Rating, ref clearCatalogFacetCache);
                UpdateLocalizedFacetSetting(model.DeliveryTimeFacet, FacetGroupKind.DeliveryTime, ref clearCatalogFacetCache);
                UpdateLocalizedFacetSetting(model.AvailabilityFacet, FacetGroupKind.Availability, ref clearCatalogFacetCache);
                UpdateLocalizedFacetSetting(model.NewArrivalsFacet, FacetGroupKind.NewArrivals, ref clearCatalogFacetCache);

                UpdateLocalizedFacetSetting(model.ForumSearchSettings.ForumFacet, FacetGroupKind.Forum, ref clearForumFacetCache, "Forum");
                UpdateLocalizedFacetSetting(model.ForumSearchSettings.CustomerFacet, FacetGroupKind.Customer, ref clearForumFacetCache, "Forum");
                UpdateLocalizedFacetSetting(model.ForumSearchSettings.DateFacet, FacetGroupKind.Date, ref clearForumFacetCache, "Forum");
            }

            if (clearCatalogFacetCache)
            {
                _catalogSearchQueryAliasMapper.Value.ClearCommonFacetCache();
            }

            if (clearForumFacetCache)
            {
                _forumSearchQueryAliasMapper.Value.ClearCommonFacetCache();
            }

            if (categoriesChange.HasValue)
            {
                Services.EventPublisher.Publish(new CategoryTreeChangedEvent(categoriesChange.Value));
            }

            return NotifyAndRedirect("Search");
        }


        [Permission(Permissions.Configuration.Setting.Read)]
        public ActionResult AllSettings()
        {
            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Setting.Read)]
        public ActionResult AllSettings(GridCommand command)
        {
            var model = new GridModel<SettingModel>();
            var stores = Services.StoreService.GetAllStores();
            string allStoresString = T("Admin.Common.StoresAll");

            var settings = Services.Settings
                .GetAllSettings()
                .Select(x =>
                {
                    var settingModel = new SettingModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Value = x.Value,
                        StoreId = x.StoreId
                    };

                    if (x.StoreId == 0)
                    {
                        settingModel.Store = allStoresString;
                    }
                    else
                    {
                        var store = stores.FirstOrDefault(s => s.Id == x.StoreId);
                        settingModel.Store = store != null ? store.Name : "".NaIfEmpty();
                    }

                    return settingModel;
                })
                .ForCommand(command)
                .ToList();

            model.Data = settings.PagedForCommand(command);
            model.Total = settings.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Setting.Update)]
        public ActionResult SettingUpdate(SettingModel model, GridCommand command)
        {
            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
            {
                var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            var setting = Services.Settings.GetSettingById(model.Id);
            if (setting == null)
            {
                return Content(T("Admin.Configuration.Settings.NoneWithThatId"));
            }

            // Use Store property (not StoreId) because appropriate property is stored in it.
            var storeId = model.Store.ToInt();

            if (!setting.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) || setting.StoreId != storeId)
            {
                // Setting name or store has been changed.
                Services.Settings.DeleteSetting(setting);
            }

            Services.Settings.SetSetting(model.Name, model.Value ?? "", storeId);

            return AllSettings(command);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Setting.Create)]
        public ActionResult SettingAdd([Bind(Exclude = "Id")] SettingModel model, GridCommand command)
        {
            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
            {
                var modelStateErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            // Use Store property (not StoreId) because appropriate property is stored in it.
            var storeId = model.Store.ToInt();
            Services.Settings.SetSetting(model.Name, model.Value, storeId);

            _customerActivityService.InsertActivity("AddNewSetting", T("ActivityLog.AddNewSetting", model.Name));

            return AllSettings(command);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Setting.Delete)]
        public ActionResult SettingDelete(int id, GridCommand command)
        {
            var setting = Services.Settings.GetSettingById(id);

            Services.Settings.DeleteSetting(setting);
            _customerActivityService.InsertActivity("DeleteSetting", T("ActivityLog.DeleteSetting", setting.Name));

            return AllSettings(command);
        }

        #endregion
    }
}