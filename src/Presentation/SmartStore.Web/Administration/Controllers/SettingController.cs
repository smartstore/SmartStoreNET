using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Common;
using SmartStore.Admin.Models.Settings;
using SmartStore.Admin.Validators.Settings;
using SmartStore.ComponentModel;
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
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Media.Storage;
using SmartStore.Services.Orders;
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
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Framework.UI.Captcha;
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
		private readonly IDeliveryTimeService _deliveryTimesService;
		private readonly ICommonServices _services;
		private readonly IProviderManager _providerManager;
		private readonly PluginMediator _pluginMediator;
		private readonly IPluginFinder _pluginFinder;
		private readonly Lazy<IMediaMover> _mediaMover;
		private readonly Lazy<ICatalogSearchQueryAliasMapper> _catalogSearchQueryAliasMapper;
        private readonly Lazy<ISiteMapService> _siteMapService;

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
			IDeliveryTimeService deliveryTimesService,
			ICommonServices services,
			IProviderManager providerManager,
			PluginMediator pluginMediator,
			IPluginFinder pluginFinder,
			Lazy<IMediaMover> mediaMover,
			Lazy<ICatalogSearchQueryAliasMapper> catalogSearchQueryAliasMapper,
            Lazy<ISiteMapService> siteMapService)
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
			_deliveryTimesService = deliveryTimesService;
			_services = services;
			_providerManager = providerManager;
			_pluginMediator = pluginMediator;
			_pluginFinder = pluginFinder;
			_mediaMover = mediaMover;
			_catalogSearchQueryAliasMapper = catalogSearchQueryAliasMapper;
            _siteMapService = siteMapService;
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
			var value = _services.Localization.GetResource(resourceKey).EmptyNull();
			return new SelectListItem { Text = value, Value = value };
		}

		private string CreateCommonFacetSettingKey(FacetGroupKind kind, int languageId)
		{
			return $"FacetGroupKind-{kind.ToString()}-Alias-{languageId}";
		}

		private void UpdateLocalizedFacetSetting(CommonFacetSettingsModel model, FacetGroupKind kind, ref bool clearCache)
		{
			foreach (var localized in model.Locales)
			{
				var key = CreateCommonFacetSettingKey(kind, localized.LanguageId);
				var existingAlias = _services.Settings.GetSettingByKey<string>(key);

				if (existingAlias.IsCaseInsensitiveEqual(localized.Alias))
					continue;

				if (localized.Alias.HasValue())
				{
					_services.Settings.SetSetting(key, localized.Alias, 0, false);
				}
				else
				{
					_services.Settings.DeleteSetting(key);
				}

				clearCache = true;
			}
		}

		private ActionResult NotifyAndRedirect(string actionMethod)
		{
			_customerActivityService.InsertActivity("EditSettings", T("ActivityLog.EditSettings"));
			NotifySuccess(T("Admin.Configuration.Updated"));

			return RedirectToAction(actionMethod);
		}

		#endregion

		#region Methods

		[ChildActionOnly]
		public ActionResult StoreScopeConfiguration()
		{
			var allStores = _services.StoreService.GetAllStores();
			if (allStores.Count < 2)
				return Content("");

			var model = new StoreScopeConfigurationModel
			{
				StoreId = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext)
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
				Text = _services.Localization.GetResource("Admin.Common.StoresAll"),
				Selected = (0 == model.StoreId),
				Value = Url.Action("ChangeStoreScopeConfiguration", "Setting", new { storeid = 0, returnUrl = Request.RawUrl })
			});

			return PartialView(model);
		}

		public ActionResult ChangeStoreScopeConfiguration(int storeid, string returnUrl = "")
		{
			var store = _services.StoreService.GetStoreById(storeid);
			if (store != null || storeid == 0)
			{
				_genericAttributeService.SaveAttribute(_services.WorkContext.CurrentCustomer,
					SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration, storeid);
			}

			return RedirectToReferrer(returnUrl, () => RedirectToAction("Index", "Home", new { area = "Admin" }));
		}

		[LoadSetting]
        public ActionResult Blog(BlogSettings blogSettings)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var model = blogSettings.ToModel();

            return View(model);
        }

		[HttpPost, SaveSetting]
        public ActionResult Blog(BlogSettings blogSettings, BlogSettingsModel model)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			blogSettings = model.ToEntity(blogSettings);

            return NotifyAndRedirect("Blog");
        }


		[LoadSetting]
		public ActionResult Forum(ForumSettings forumSettings)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var model = forumSettings.ToModel();
			
			return View(model);
        }

		[HttpPost, SaveSetting]
        public ActionResult Forum(ForumSettings forumSettings, ForumSettingsModel model)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			forumSettings = model.ToEntity(forumSettings);

            return NotifyAndRedirect("Forum");
        }


		[LoadSetting]
		public ActionResult News(NewsSettings newsSettings)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var model = newsSettings.ToModel();
			return View(model);
        }

		[HttpPost, SaveSetting]
		public ActionResult News(NewsSettings newsSettings, NewsSettingsModel model)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			newsSettings = model.ToEntity(newsSettings);

            return NotifyAndRedirect("News");
        }

	
        public ActionResult Shipping()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var shippingSettings = _services.Settings.LoadSetting<ShippingSettings>(storeScope);
			var store = storeScope == 0 ? _services.StoreContext.CurrentStore : _services.StoreService.GetStoreById(storeScope);

			var model = shippingSettings.ToModel();
			model.PrimaryStoreCurrencyCode = store.PrimaryStoreCurrency.CurrencyCode;

			StoreDependingSettings.GetOverrideKeys(shippingSettings, model, storeScope, _services.Settings);

			// Shipping origin
			if (storeScope > 0 && _services.Settings.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope))
			{
				StoreDependingSettings.AddOverrideKey(shippingSettings, "ShippingOriginAddress");
			}

			var originAddress = shippingSettings.ShippingOriginAddressId > 0
				? _addressService.GetAddressById(shippingSettings.ShippingOriginAddressId)
				: null;

			model.ShippingOriginAddress = originAddress != null
				? originAddress.ToModel(_addressService)
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
		public ActionResult Shipping(ShippingSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			// Note, model state is invalid here due to ShippingOriginAddress validation.
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var shippingSettings = _services.Settings.LoadSetting<ShippingSettings>(storeScope);
			shippingSettings = model.ToEntity(shippingSettings);

			using (Services.Settings.BeginScope())
			{
				StoreDependingSettings.UpdateSettings(shippingSettings, form, storeScope, _services.Settings, null, propertyName =>
				{
					// Skip to prevent the address from being recreated every time you save.
					if (propertyName.IsCaseInsensitiveEqual("ShippingOriginAddressId"))
						return null;

					return propertyName;
				});

				// Special case ShippingOriginAddressId\ShippingOriginAddress.
				if (storeScope == 0 || StoreDependingSettings.IsOverrideChecked(shippingSettings, "ShippingOriginAddress", form))
				{
					var addressId = _services.Settings.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope) ? shippingSettings.ShippingOriginAddressId : 0;
					var originAddress = _addressService.GetAddressById(addressId) ?? new Address { CreatedOnUtc = DateTime.UtcNow };

					// Update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one).
					model.ShippingOriginAddress.Id = addressId;
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
					_services.Settings.SaveSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope, false);
				}
				else
				{
					_addressService.DeleteAddress(shippingSettings.ShippingOriginAddressId);
					_services.Settings.DeleteSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope);
				}
			}

            return NotifyAndRedirect("Shipping");
        }


        public ActionResult Tax()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var taxSettings = _services.Settings.LoadSetting<TaxSettings>(storeScope);

			var model = taxSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(taxSettings, model, storeScope, _services.Settings);

			var taxCategories = _taxCategoryService.GetAllTaxCategories();
			foreach (var tc in taxCategories)
			{
				model.ShippingTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString(), Selected = tc.Id == taxSettings.ShippingTaxClassId });
			}

			foreach (var tc in taxCategories)
			{
				model.PaymentMethodAdditionalFeeTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id.ToString(),
					Selected = tc.Id == taxSettings.PaymentMethodAdditionalFeeTaxClassId });
			}

            // EU VAT countries.
			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.EuVatShopCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == taxSettings.EuVatShopCountryId });
			}

            // Default tax address.
            var defaultAddress = (taxSettings.DefaultTaxAddressId > 0 ? _addressService.GetAddressById(taxSettings.DefaultTaxAddressId) : null);
			model.DefaultTaxAddress = defaultAddress != null
				? defaultAddress.ToModel(_addressService)
				: new AddressModel();

			if (storeScope > 0 && _services.Settings.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope))
			{
				StoreDependingSettings.AddOverrideKey(taxSettings, "DefaultTaxAddress");
			}

			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.DefaultTaxAddress.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(),
					Selected = (defaultAddress != null && c.Id == defaultAddress.CountryId) });
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
        public ActionResult Tax(TaxSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			// Note, model state invalid here due to DefaultTaxAddress validation.
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var taxSettings = _services.Settings.LoadSetting<TaxSettings>(storeScope);
			taxSettings = model.ToEntity(taxSettings);

			using (Services.Settings.BeginScope())
			{
				StoreDependingSettings.UpdateSettings(taxSettings, form, storeScope, _services.Settings, null, propertyName =>
				{
					// Skip to prevent the address from being recreated every time you save.
					if (propertyName.IsCaseInsensitiveEqual("DefaultTaxAddressId"))
						return null;

					return propertyName;
				});

				taxSettings.AllowCustomersToSelectTaxDisplayType = false;
				_services.Settings.UpdateSetting(taxSettings, x => x.AllowCustomersToSelectTaxDisplayType, false, storeScope);

				// Special case DefaultTaxAddressId\DefaultTaxAddress.
				if (storeScope == 0 || StoreDependingSettings.IsOverrideChecked(taxSettings, "DefaultTaxAddress", form))
				{
					var addressId = _services.Settings.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope) ? taxSettings.DefaultTaxAddressId : 0;
					var originAddress = _addressService.GetAddressById(addressId) ?? new Address { CreatedOnUtc = DateTime.UtcNow };

					// Update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one).
					model.DefaultTaxAddress.Id = addressId;
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
					_services.Settings.SaveSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope, false);
				}
				else if (storeScope > 0)
				{
					_addressService.DeleteAddress(taxSettings.DefaultTaxAddressId);
					_services.Settings.DeleteSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope);
				}
			}

            return NotifyAndRedirect("Tax");
        }


		[LoadSetting]
        public ActionResult Catalog(CatalogSettings catalogSettings)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var model = catalogSettings.ToModel();

			model.AvailableSubCategoryDisplayTypes = catalogSettings.SubCategoryDisplayType.ToSelectList();
			model.AvailablePriceDisplayTypes = catalogSettings.PriceDisplayType.ToSelectList();

            model.AvailableDefaultViewModes.Add(
				new SelectListItem { Value = "grid", Text = _services.Localization.GetResource("Common.Grid"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("grid") }
			);
            model.AvailableDefaultViewModes.Add(
				new SelectListItem { Value = "list", Text = _services.Localization.GetResource("Common.List"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("list") }
			);

            // Default sort order modes.
            model.AvailableSortOrderModes = catalogSettings.DefaultSortOrder.ToSelectList();

			var deliveryTimes = _deliveryTimesService.GetAllDeliveryTimes();
			foreach (var dt in deliveryTimes)
			{
				model.AvailableDeliveryTimes.Add(new SelectListItem
				{
					Text = dt.Name,
					Value = dt.Id.ToString(),
					Selected = dt.Id == catalogSettings.DeliveryTimeIdForEmptyStock
				});
			}

            return View(model);
        }

        [HttpPost, ValidateInput(false), SaveSetting]
        public ActionResult Catalog(CatalogSettings catalogSettings, CatalogSettingsModel model)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Catalog(catalogSettings);

            ModelState.Clear();

			// We need to clear the sitemap cache if MaxItemsToDisplayInCatalogMenu has changed.
			if (catalogSettings.MaxItemsToDisplayInCatalogMenu != model.MaxItemsToDisplayInCatalogMenu)
            {
                // Clear cached navigation model.
                var siteMap = _siteMapService.Value.GetSiteMap("catalog");
                siteMap.ClearCache();
            }

            catalogSettings = model.ToEntity(catalogSettings);

            return NotifyAndRedirect("Catalog");
        }


		[LoadSetting]
        public ActionResult RewardPoints(RewardPointsSettings rewardPointsSettings, int storeScope)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var store = (storeScope == 0 ? _services.StoreContext.CurrentStore : _services.StoreService.GetStoreById(storeScope));

			var model = rewardPointsSettings.ToModel();
			model.PrimaryStoreCurrencyCode = store.PrimaryStoreCurrency.CurrencyCode;
			
			return View(model);
        }

        [HttpPost]
        public ActionResult RewardPoints(RewardPointsSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var rewardPointsSettings = _services.Settings.LoadSetting<RewardPointsSettings>(storeScope);

			if (!ModelState.IsValid)
				return RewardPoints(rewardPointsSettings, storeScope);

			ModelState.Clear();
			rewardPointsSettings = model.ToEntity(rewardPointsSettings);

			// Scope to avoid duplicate records.
			using (Services.Settings.BeginScope())
			{
				StoreDependingSettings.UpdateSettings(rewardPointsSettings, form, storeScope, _services.Settings);
			}

			// Scope because reward points settings are updated.
			using (Services.Settings.BeginScope())
			{
				var pointsForPurchases = StoreDependingSettings.IsOverrideChecked(rewardPointsSettings, "PointsForPurchases_Amount", form);

				_services.Settings.UpdateSetting(rewardPointsSettings, x => x.PointsForPurchases_Amount, pointsForPurchases, storeScope);
				_services.Settings.UpdateSetting(rewardPointsSettings, x => x.PointsForPurchases_Points, pointsForPurchases, storeScope);
			}

			return NotifyAndRedirect("RewardPoints");
        }


        public ActionResult Order()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var orderSettings = _services.Settings.LoadSetting<OrderSettings>(storeScope);

			var allStores = _services.StoreService.GetAllStores();
			var store = (storeScope == 0 ? _services.StoreContext.CurrentStore : allStores.FirstOrDefault(x => x.Id == storeScope));

			var model = orderSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(orderSettings, model, storeScope, _services.Settings);

			model.PrimaryStoreCurrencyCode = store.PrimaryStoreCurrency.CurrencyCode;
			model.StoreCount = allStores.Count;

            // Gift card activation/deactivation.
            model.GiftCards_Activated_OrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.GiftCards_Deactivated_OrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();

			AddLocales(_languageService, model.Locales, (locale, languageId) =>
			{
				locale.ReturnRequestActions = orderSettings.GetLocalized(x => x.ReturnRequestActions, languageId, false, false);
				locale.ReturnRequestReasons = orderSettings.GetLocalized(x => x.ReturnRequestReasons, languageId, false, false);
			});

            model.OrderIdent = _maintenanceService.GetTableIdent<Order>();

            return View(model);
        }

        [HttpPost]
        public ActionResult Order(OrderSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			if (!ModelState.IsValid)
				return Order();

			ModelState.Clear();

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var orderSettings = _services.Settings.LoadSetting<OrderSettings>(storeScope);
			orderSettings = model.ToEntity(orderSettings);

			// Scope to avoid duplicate records.
			using (Services.Settings.BeginScope())
			{
				StoreDependingSettings.UpdateSettings(orderSettings, form, storeScope, _services.Settings);
			}

			// Scope because order settings are updated.
			using (Services.Settings.BeginScope())
			{
				_services.Settings.SaveSetting(orderSettings, x => x.ReturnRequestActions, 0, false);
				_services.Settings.SaveSetting(orderSettings, x => x.ReturnRequestReasons, 0, false);

				foreach (var localized in model.Locales)
				{
					_localizedEntityService.SaveLocalizedValue(orderSettings, x => x.ReturnRequestActions, localized.ReturnRequestActions, localized.LanguageId);
					_localizedEntityService.SaveLocalizedValue(orderSettings, x => x.ReturnRequestReasons, localized.ReturnRequestReasons, localized.LanguageId);
				}

				if (model.GiftCards_Activated_OrderStatusId.HasValue)
					_services.Settings.SaveSetting(orderSettings, x => x.GiftCards_Activated_OrderStatusId, 0, false);
				else
					_services.Settings.DeleteSetting(orderSettings, x => x.GiftCards_Activated_OrderStatusId);

				if (model.GiftCards_Deactivated_OrderStatusId.HasValue)
					_services.Settings.SaveSetting(orderSettings, x => x.GiftCards_Deactivated_OrderStatusId, 0, false);
				else
					_services.Settings.DeleteSetting(orderSettings, x => x.GiftCards_Deactivated_OrderStatusId);
			}

            // Order ident.
            if (model.OrderIdent.HasValue)
            {
                try
                {
                    _maintenanceService.SetTableIdent<Order>(model.OrderIdent.Value);
                }
                catch (Exception exc)
                {
					NotifyError(exc.Message);
                }
            }

            return NotifyAndRedirect("Order");
        }


        public ActionResult ShoppingCart()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var shoppingCartSettings = _services.Settings.LoadSetting<ShoppingCartSettings>(storeScope);

			var model = shoppingCartSettings.ToModel();

			model.AvailableNewsLetterSubscriptions = shoppingCartSettings.NewsLetterSubscription.ToSelectList();
			model.AvailableThirdPartyEmailHandOver = shoppingCartSettings.ThirdPartyEmailHandOver.ToSelectList();

			StoreDependingSettings.GetOverrideKeys(shoppingCartSettings, model, storeScope, _services.Settings);

			AddLocales(_languageService, model.Locales, (locale, languageId) =>
			{
				locale.ThirdPartyEmailHandOverLabel = shoppingCartSettings.GetLocalized(x => x.ThirdPartyEmailHandOverLabel, languageId, false, false);
			});

			return View(model);
        }

        [HttpPost]
        public ActionResult ShoppingCart(ShoppingCartSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			if (!ModelState.IsValid)
				return ShoppingCart();

			ModelState.Clear();

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var shoppingCartSettings = _services.Settings.LoadSetting<ShoppingCartSettings>(storeScope);
			shoppingCartSettings = model.ToEntity(shoppingCartSettings);

			// Scope to avoid duplicate ShoppingCartSettings.ThirdPartyEmailHandOverLabel records.
			using (Services.Settings.BeginScope())
			{
				StoreDependingSettings.UpdateSettings(shoppingCartSettings, form, storeScope, _services.Settings);
			}

			// Scope because shopping cart settings are updated.
			using (Services.Settings.BeginScope())
			{
				_services.Settings.SaveSetting(shoppingCartSettings, x => x.ThirdPartyEmailHandOverLabel, 0, false);
			}

			foreach (var localized in model.Locales)
			{
				_localizedEntityService.SaveLocalizedValue(shoppingCartSettings, x => x.ThirdPartyEmailHandOverLabel, localized.ThirdPartyEmailHandOverLabel, localized.LanguageId);
			}
            
            return NotifyAndRedirect("ShoppingCart");
        }


		[LoadSetting]
		public ActionResult Payment(PaymentSettings settings)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
				return AccessDeniedView();

			var model = new PaymentSettingsModel();
			model.AvailableCapturePaymentReasons = CapturePaymentReason.OrderShipped.ToSelectList(false).ToList();
			MiniMapper.Map(settings, model);
			
			return View(model);
		}

		[HttpPost, SaveSetting]
		public ActionResult Payment(PaymentSettings settings, PaymentSettingsModel model, FormCollection form)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
				return AccessDeniedView();

			if (!ModelState.IsValid)
				return Payment(settings);

			ModelState.Clear();
			MiniMapper.Map(model, settings);

			return NotifyAndRedirect("Payment");
		}


		[LoadSetting]
        public ActionResult Media(MediaSettings mediaSettings)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var model = mediaSettings.ToModel();

            model.AvailablePictureZoomTypes.Add(new SelectListItem
			{ 
                Text = T("Admin.Configuration.Settings.Media.PictureZoomType.Window"), 
                Value = "window", 
                Selected = model.PictureZoomType.Equals("window") 
            });
            model.AvailablePictureZoomTypes.Add(new SelectListItem
			{
                Text = T("Admin.Configuration.Settings.Media.PictureZoomType.Inner"),
                Value = "inner", 
                Selected = model.PictureZoomType.Equals("inner") 
            });
            model.AvailablePictureZoomTypes.Add(new SelectListItem
			{
                Text = T("Admin.Configuration.Settings.Media.PictureZoomType.Lens"),
                Value = "lens", 
                Selected = model.PictureZoomType.Equals("lens") 
            });

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

        [HttpPost, SaveSetting, FormValueRequired("save")]
        public ActionResult Media(MediaSettings mediaSettings, MediaSettingsModel model)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			mediaSettings = model.ToEntity(mediaSettings);

            return NotifyAndRedirect("Media");
        }

        [HttpPost]
        public ActionResult ChangeMediaStorage(string targetProvider)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var currentStorageProvider = Services.Settings.GetSettingByKey<string>("Media.Storage.Provider");
			var source = _providerManager.GetProvider<IMediaStorageProvider>(currentStorageProvider);
			var target = _providerManager.GetProvider<IMediaStorageProvider>(targetProvider);

			var success = _mediaMover.Value.Move(source, target);

			if (success)
			{
				_customerActivityService.InsertActivity("EditSettings", T("ActivityLog.EditSettings"));

				NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
			}

			return RedirectToAction("Media");
        }


        public ActionResult CustomerUser()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			StoreDependingSettings.CreateViewDataObject(storeScope);

			var allCustomerRoles = _customerService.GetAllCustomerRoles(true);

			var customerSettings = _services.Settings.LoadSetting<CustomerSettings>(storeScope);
			var addressSettings = _services.Settings.LoadSetting<AddressSettings>(storeScope);
			var dateTimeSettings = _services.Settings.LoadSetting<DateTimeSettings>(storeScope);
			var externalAuthenticationSettings = _services.Settings.LoadSetting<ExternalAuthenticationSettings>(storeScope);

			//merge settings
			var model = new CustomerUserSettingsModel();
            model.CustomerSettings = customerSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(customerSettings, model.CustomerSettings, storeScope, _services.Settings, false);

            model.AddressSettings = addressSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(addressSettings, model.AddressSettings, storeScope, _services.Settings, false);

            model.DateTimeSettings.AllowCustomersToSetTimeZone = dateTimeSettings.AllowCustomersToSetTimeZone;
            model.DateTimeSettings.DefaultStoreTimeZoneId = _dateTimeHelper.DefaultStoreTimeZone.Id;

            foreach (TimeZoneInfo timeZone in _dateTimeHelper.GetSystemTimeZones())
            {
                model.DateTimeSettings.AvailableTimeZones.Add(new SelectListItem
                {
                    Text = timeZone.DisplayName,
                    Value = timeZone.Id,
                    Selected = timeZone.Id.Equals(_dateTimeHelper.DefaultStoreTimeZone.Id, StringComparison.InvariantCultureIgnoreCase)
                });
            }

			StoreDependingSettings.GetOverrideKeys(dateTimeSettings, model.DateTimeSettings, storeScope, _services.Settings, false);

            model.ExternalAuthenticationSettings.AutoRegisterEnabled = externalAuthenticationSettings.AutoRegisterEnabled;

			StoreDependingSettings.GetOverrideKeys(externalAuthenticationSettings, model.ExternalAuthenticationSettings, storeScope, _services.Settings, false);

			model.CustomerSettings.AvailableRegisterCustomerRoles = allCustomerRoles
				.Where(x => x.SystemName != SystemCustomerRoleNames.Registered && x.SystemName != SystemCustomerRoleNames.Guests)
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
				.ToList();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Salutations = addressSettings.GetLocalized(x => x.Salutations, languageId, false, false);
            });

			return View(model);
        }

        [HttpPost]
		public ActionResult CustomerUser(CustomerUserSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);

			var customerSettings = _services.Settings.LoadSetting<CustomerSettings>(storeScope);
            customerSettings = model.CustomerSettings.ToEntity(customerSettings);

			var addressSettings = _services.Settings.LoadSetting<AddressSettings>(storeScope);
			addressSettings = model.AddressSettings.ToEntity(addressSettings);

			var dateTimeSettings = _services.Settings.LoadSetting<DateTimeSettings>(storeScope);
			dateTimeSettings.DefaultStoreTimeZoneId = model.DateTimeSettings.DefaultStoreTimeZoneId;
			dateTimeSettings.AllowCustomersToSetTimeZone = model.DateTimeSettings.AllowCustomersToSetTimeZone;

			var authSettings = _services.Settings.LoadSetting<ExternalAuthenticationSettings>(storeScope);
			authSettings.AutoRegisterEnabled = model.ExternalAuthenticationSettings.AutoRegisterEnabled;

			// Scope to avoid duplicate CustomerSettings.DefaultPasswordFormat records.
			using (Services.Settings.BeginScope())
			{
				StoreDependingSettings.UpdateSettings(customerSettings, form, storeScope, _services.Settings);
				StoreDependingSettings.UpdateSettings(addressSettings, form, storeScope, _services.Settings);
				StoreDependingSettings.UpdateSettings(dateTimeSettings, form, storeScope, _services.Settings);
				StoreDependingSettings.UpdateSettings(authSettings, form, storeScope, _services.Settings);
			}

			// Scope because customer settings are updated.
			using (Services.Settings.BeginScope())
			{
				_services.Settings.SaveSetting(customerSettings, x => x.DefaultPasswordFormat, 0, false);
			}

			foreach (var localized in model.Locales)
			{
				_localizedEntityService.SaveLocalizedValue(addressSettings, x => x.Salutations, localized.Salutations, localized.LanguageId);
			}			

            return NotifyAndRedirect("CustomerUser");
        }


        public ActionResult GeneralCommon()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            // Set page timeout to 5 minutes.
            Server.ScriptTimeout = 300;

			var model = new GeneralCommonSettingsModel();
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			StoreDependingSettings.CreateViewDataObject(storeScope);

            // Store information.
			var storeInformationSettings = _services.Settings.LoadSetting<StoreInformationSettings>(storeScope);
			MiniMapper.Map(storeInformationSettings, model.StoreInformationSettings);

			StoreDependingSettings.GetOverrideKeys(storeInformationSettings, model.StoreInformationSettings, storeScope, _services.Settings, false);

			// SEO.
			var seoSettings = _services.Settings.LoadSetting<SeoSettings>(storeScope);
			MiniMapper.Map(seoSettings, model.SeoSettings);

			StoreDependingSettings.GetOverrideKeys(seoSettings, model.SeoSettings, storeScope, _services.Settings, false);

			// Security.
			var securitySettings = _services.Settings.LoadSetting<SecuritySettings>(storeScope);
			MiniMapper.Map(securitySettings, model.SecuritySettings);

			var captchaSettings = _services.Settings.LoadSetting<CaptchaSettings>(storeScope);
			MiniMapper.Map(captchaSettings, model.CaptchaSettings);

			StoreDependingSettings.GetOverrideKeys(captchaSettings, model.CaptchaSettings, storeScope, _services.Settings, false);

			// PDF.
			var pdfSettings = _services.Settings.LoadSetting<PdfSettings>(storeScope);
			MiniMapper.Map(pdfSettings, model.PdfSettings);

			StoreDependingSettings.GetOverrideKeys(pdfSettings, model.PdfSettings, storeScope, _services.Settings, false);

			// Localization.
			var localizationSettings = _services.Settings.LoadSetting<LocalizationSettings>(storeScope);
			MiniMapper.Map(localizationSettings, model.LocalizationSettings);

			StoreDependingSettings.GetOverrideKeys(localizationSettings, model.LocalizationSettings, storeScope, _services.Settings, false);

			// Company information.
			var companySettings = _services.Settings.LoadSetting<CompanyInformationSettings>(storeScope);
			MiniMapper.Map(companySettings, model.CompanyInformationSettings);

			StoreDependingSettings.GetOverrideKeys(companySettings, model.CompanyInformationSettings, storeScope, _services.Settings, false);

			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.CompanyInformationSettings.AvailableCountries.Add(
					new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.CompanyInformationSettings.CountryId)
				});
			}

            model.CompanyInformationSettings.Salutations.Add(ResToSelectListItem("Admin.Address.Salutation.Mr"));
            model.CompanyInformationSettings.Salutations.Add(ResToSelectListItem("Admin.Address.Salutation.Mrs"));

			model.CompanyInformationSettings.ManagementDescriptions.Add(
                ResToSelectListItem("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Manager"));
			model.CompanyInformationSettings.ManagementDescriptions.Add(
                ResToSelectListItem("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Shopkeeper"));
			model.CompanyInformationSettings.ManagementDescriptions.Add(
                ResToSelectListItem("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Procurator"));
			model.CompanyInformationSettings.ManagementDescriptions.Add(
                ResToSelectListItem("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Shareholder"));
			model.CompanyInformationSettings.ManagementDescriptions.Add(
                ResToSelectListItem("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.AuthorizedPartner"));
			model.CompanyInformationSettings.ManagementDescriptions.Add(
                ResToSelectListItem("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.Director"));
			model.CompanyInformationSettings.ManagementDescriptions.Add(
                ResToSelectListItem("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ManagementDescriptions.ManagingPartner"));

			// Contact data.
			var contactDataSettings = _services.Settings.LoadSetting<ContactDataSettings>(storeScope);
			MiniMapper.Map(contactDataSettings, model.ContactDataSettings);

			StoreDependingSettings.GetOverrideKeys(contactDataSettings, model.ContactDataSettings, storeScope, _services.Settings, false);

			// Bank connection.
			var bankConnectionSettings = _services.Settings.LoadSetting<BankConnectionSettings>(storeScope);
			MiniMapper.Map(bankConnectionSettings, model.BankConnectionSettings);

			StoreDependingSettings.GetOverrideKeys(bankConnectionSettings, model.BankConnectionSettings, storeScope, _services.Settings, false);

			// Social.
			var socialSettings = _services.Settings.LoadSetting<SocialSettings>(storeScope);
			MiniMapper.Map(socialSettings, model.SocialSettings);

			StoreDependingSettings.GetOverrideKeys(socialSettings, model.SocialSettings, storeScope, _services.Settings, false);

            return View(model);
        }

        [HttpPost, FormValueRequired("save")]
        public ActionResult GeneralCommon(GeneralCommonSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);

			// Store information.
			var storeInformationSettings = _services.Settings.LoadSetting<StoreInformationSettings>(storeScope);
			MiniMapper.Map(model.StoreInformationSettings, storeInformationSettings);

			// SEO.
			var seoSettings = _services.Settings.LoadSetting<SeoSettings>(storeScope);
			var resetUserSeoCharacterTable = (seoSettings.SeoNameCharConversion != model.SeoSettings.SeoNameCharConversion);
			MiniMapper.Map(model.SeoSettings, seoSettings);

			// Security.
			var securitySettings = _services.Settings.LoadSetting<SecuritySettings>(storeScope);
			MiniMapper.Map(model.SecuritySettings, securitySettings);

			// Captcha.
			var captchaSettings = _services.Settings.LoadSetting<CaptchaSettings>(storeScope);
			MiniMapper.Map(model.CaptchaSettings, captchaSettings);

			// PDF.
			var pdfSettings = _services.Settings.LoadSetting<PdfSettings>(storeScope);
			MediaHelper.UpdatePictureTransientState(pdfSettings.LogoPictureId, model.PdfSettings.LogoPictureId, true);
			MiniMapper.Map(model.PdfSettings, pdfSettings);

			// Localization.
			var localizationSettings = _services.Settings.LoadSetting<LocalizationSettings>(storeScope);
			var clearSeoFriendlyUrls = localizationSettings.SeoFriendlyUrlsForLanguagesEnabled != model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled;
			MiniMapper.Map(model.LocalizationSettings, localizationSettings);

			// Company information.
			var companySettings = _services.Settings.LoadSetting<CompanyInformationSettings>(storeScope);
			MiniMapper.Map(model.CompanyInformationSettings, companySettings);
			companySettings.CountryName = _countryService.GetCountryById(model.CompanyInformationSettings.CountryId)?.Name;

			// Contact data.
			var contactDataSettings = _services.Settings.LoadSetting<ContactDataSettings>(storeScope);
			MiniMapper.Map(model.ContactDataSettings, contactDataSettings);

			// Bank connection.
			var bankConnectionSettings = _services.Settings.LoadSetting<BankConnectionSettings>(storeScope);
			MiniMapper.Map(model.BankConnectionSettings, bankConnectionSettings);

			// Social.
			var socialSettings = _services.Settings.LoadSetting<SocialSettings>(storeScope);
			MiniMapper.Map(model.SocialSettings, socialSettings);
			
			using (Services.Settings.BeginScope())
			{
				StoreDependingSettings.UpdateSettings(storeInformationSettings, form, storeScope, _services.Settings);
				StoreDependingSettings.UpdateSettings(seoSettings, form, storeScope, _services.Settings);
				StoreDependingSettings.UpdateSettings(captchaSettings, form, storeScope, _services.Settings);
				StoreDependingSettings.UpdateSettings(pdfSettings, form, storeScope, _services.Settings);
				StoreDependingSettings.UpdateSettings(localizationSettings, form, storeScope, _services.Settings);
				StoreDependingSettings.UpdateSettings(companySettings, form, storeScope, _services.Settings);
				StoreDependingSettings.UpdateSettings(contactDataSettings, form, storeScope, _services.Settings);
				StoreDependingSettings.UpdateSettings(bankConnectionSettings, form, storeScope, _services.Settings);
				StoreDependingSettings.UpdateSettings(socialSettings, form, storeScope, _services.Settings);

				_services.Settings.SaveSetting(securitySettings);
			}

			if (resetUserSeoCharacterTable)
			{
				SeoHelper.ResetUserSeoCharacterTable();
			}

			if (clearSeoFriendlyUrls)
			{
				LocalizedRoute.ClearSeoFriendlyUrlsCachedValue();
			}

			if (captchaSettings.Enabled && (captchaSettings.ReCaptchaPublicKey.IsEmpty() || captchaSettings.ReCaptchaPrivateKey.IsEmpty()))
			{
				NotifyError(T("Admin.Configuration.Settings.GeneralCommon.CaptchaEnabledNoKeys"));
			}

			return NotifyAndRedirect("GeneralCommon");
        }

        [HttpPost, ActionName("GeneralCommon"), FormValueRequired("changeencryptionkey")]
        public ActionResult ChangeEnryptionKey(GeneralCommonSettingsModel model)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            // Set page timeout to 5 minutes.
            Server.ScriptTimeout = 300;

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var securitySettings = _services.Settings.LoadSetting<SecuritySettings>(storeScope);

            try
            {
                if (model.SecuritySettings.EncryptionKey == null)
                    model.SecuritySettings.EncryptionKey = "";

                model.SecuritySettings.EncryptionKey = model.SecuritySettings.EncryptionKey.Trim();

                var newEncryptionPrivateKey = model.SecuritySettings.EncryptionKey;
                if (newEncryptionPrivateKey.IsEmpty() || newEncryptionPrivateKey.Length != 16)
                    throw new SmartException(T("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TooShort"));

                var oldEncryptionPrivateKey = securitySettings.EncryptionKey;
                if (oldEncryptionPrivateKey == newEncryptionPrivateKey)
                    throw new SmartException(T("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TheSame"));

                // Update encrypted order info.
                var orders = _orderService.LoadAllOrders();
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

                // Update user information.
                // Optimization - load only users with PasswordFormat.Encrypted.
                var customers = _customerService.GetAllCustomersByPasswordFormat(PasswordFormat.Encrypted);
                foreach (var customer in customers)
                {
                    string decryptedPassword = _encryptionService.DecryptText(customer.Password, oldEncryptionPrivateKey);
                    string encryptedPassword = _encryptionService.EncryptText(decryptedPassword, newEncryptionPrivateKey);

                    customer.Password = encryptedPassword;
                    _customerService.UpdateCustomer(customer);
                }

                securitySettings.EncryptionKey = newEncryptionPrivateKey;
                _services.Settings.SaveSetting(securitySettings);

                NotifySuccess(T("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.Changed"));
            }
            catch (Exception exc)
            {
                NotifyError(exc);
            }

			return RedirectToAction("GeneralCommon");
        }

		[HttpPost]
		public ActionResult TestSeoNameCreation(GeneralCommonSettingsModel model)
		{
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var seoSettings = _services.Settings.LoadSetting<SeoSettings>(storeScope);

			// we always test against persisted settings

			var result = SeoHelper.GetSeName(model.SeoSettings.TestSeoNameCreation,
				seoSettings.ConvertNonWesternChars,
				seoSettings.AllowUnicodeCharsInUrls,
				seoSettings.SeoNameCharConversion);

			return Content(result);
		}


		[LoadSetting]
		public ActionResult DataExchange(DataExchangeSettings settings)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
				return AccessDeniedView();

			var model = new DataExchangeSettingsModel();
			MiniMapper.Map(settings, model);

			return View(model);
		}

		[HttpPost, SaveSetting]
		public ActionResult DataExchange(DataExchangeSettings settings, DataExchangeSettingsModel model, FormCollection form)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
				return AccessDeniedView();

			if (!ModelState.IsValid)
				return DataExchange(settings);

			ModelState.Clear();
			MiniMapper.Map(model, settings);

			return NotifyAndRedirect("DataExchange");
		}


		public ActionResult Search()
		{
			if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
				return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<SearchSettings>(storeScope);
			var megaSearchDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.MegaSearch");
			var megaSearchPlusDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.MegaSearchPlus");

			var model = new SearchSettingsModel();
			MiniMapper.Map(settings, model);
			model.IsMegaSearchInstalled = megaSearchDescriptor != null;
			model.AvailableSortOrderModes = settings.DefaultSortOrder.ToSelectList();

			if (megaSearchDescriptor == null)
			{
				model.SearchFieldsNote = T("Admin.Configuration.Settings.Search.SearchFieldsNote");

				model.AvailableSearchFields = new List<SelectListItem>
				{
					new SelectListItem { Text = T("Admin.Catalog.Products.Fields.ShortDescription"), Value = "shortdescription" },
					new SelectListItem { Text = T("Admin.Catalog.Products.Fields.Sku"), Value = "sku" },
				};

				model.AvailableSearchModes = settings.SearchMode.ToSelectList().Where(x => x.Value.ToInt() != (int)SearchMode.ExactMatch).ToList();
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
			}

			// Common facets
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

			// Localized facet settings (CommonFacetSettingsLocalizedModel).
			foreach (var language in _languageService.GetAllLanguages(true))
			{
				model.CategoryFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
				{
					LanguageId = language.Id,
					Alias = _services.Settings.GetSettingByKey<string>(CreateCommonFacetSettingKey(FacetGroupKind.Category, language.Id))
				});
				model.BrandFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
				{
					LanguageId = language.Id,
					Alias = _services.Settings.GetSettingByKey<string>(CreateCommonFacetSettingKey(FacetGroupKind.Brand, language.Id))
				});
				model.PriceFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
				{
					LanguageId = language.Id,
					Alias = _services.Settings.GetSettingByKey<string>(CreateCommonFacetSettingKey(FacetGroupKind.Price, language.Id))
				});
				model.RatingFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
				{
					LanguageId = language.Id,
					Alias = _services.Settings.GetSettingByKey<string>(CreateCommonFacetSettingKey(FacetGroupKind.Rating, language.Id))
				});
				model.DeliveryTimeFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
				{
					LanguageId = language.Id,
					Alias = _services.Settings.GetSettingByKey<string>(CreateCommonFacetSettingKey(FacetGroupKind.DeliveryTime, language.Id))
				});
				model.AvailabilityFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
				{
					LanguageId = language.Id,
					Alias = _services.Settings.GetSettingByKey<string>(CreateCommonFacetSettingKey(FacetGroupKind.Availability, language.Id))
				});
				model.NewArrivalsFacet.Locales.Add(new CommonFacetSettingsLocalizedModel
				{
					LanguageId = language.Id,
					Alias = _services.Settings.GetSettingByKey<string>(CreateCommonFacetSettingKey(FacetGroupKind.NewArrivals, language.Id))
				});
			}

			// Facet settings (CommonFacetSettingsModel).
			StoreDependingSettings.GetOverrideKeys(settings, model, storeScope, Services.Settings);

			var keyPrefixes = new string[] { "Brand", "Price", "Rating", "DeliveryTime", "Availability", "NewArrivals" };
			foreach (var prefix in keyPrefixes)
			{
				StoreDependingSettings.GetOverrideKey(prefix + "Facet.Disabled", prefix + "Disabled", settings, storeScope, Services.Settings);
				StoreDependingSettings.GetOverrideKey(prefix + "Facet.DisplayOrder", prefix + "DisplayOrder", settings, storeScope, Services.Settings);
			}

			// Facet settings with a non-prefixed name.
			StoreDependingSettings.GetOverrideKey("AvailabilityFacet.IncludeNotAvailable", "IncludeNotAvailable", settings, storeScope, Services.Settings);

			return View(model);
		}

		[HttpPost]
		public ActionResult Search(SearchSettingsModel model, FormCollection form)
		{
			if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
				return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<SearchSettings>(storeScope);

			var validator = new SearchSettingValidator(Services.Localization, x =>
			{
				return storeScope == 0 || StoreDependingSettings.IsOverrideChecked(settings, x, form);
			});

			validator.Validate(model, ModelState);

			if (!ModelState.IsValid)
				return Search();

			ModelState.Clear();
			MiniMapper.Map(model, settings);

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

			// Scope to avoid duplicate SearchSettings.SearchFields records.
			using (Services.Settings.BeginScope())
			{
				StoreDependingSettings.UpdateSettings(settings, form, storeScope, Services.Settings);
			}

			var clearFacetCache = false;
			using (Services.Settings.BeginScope())
			{
				_services.Settings.SaveSetting(settings, x => x.SearchFields, 0, false);

				// Facet settings (CommonFacetSettingsModel).
				var keyPrefixes = new string[] { "Brand", "Price", "Rating", "DeliveryTime", "Availability", "NewArrivals" };
				foreach (var prefix in keyPrefixes)
				{
					StoreDependingSettings.UpdateSetting(prefix + "Facet.Disabled", prefix + "Disabled", settings, form, storeScope, Services.Settings);
					StoreDependingSettings.UpdateSetting(prefix + "Facet.DisplayOrder", prefix + "DisplayOrder", settings, form, storeScope, Services.Settings);
				}

				// Facet settings with a non-prefixed name.
				StoreDependingSettings.UpdateSetting("AvailabilityFacet.IncludeNotAvailable", "IncludeNotAvailable", settings, form, storeScope, Services.Settings);

				// Localized facet settings (CommonFacetSettingsLocalizedModel).
				UpdateLocalizedFacetSetting(model.CategoryFacet, FacetGroupKind.Category, ref clearFacetCache);
				UpdateLocalizedFacetSetting(model.BrandFacet, FacetGroupKind.Brand, ref clearFacetCache);
				UpdateLocalizedFacetSetting(model.PriceFacet, FacetGroupKind.Price, ref clearFacetCache);
				UpdateLocalizedFacetSetting(model.RatingFacet, FacetGroupKind.Rating, ref clearFacetCache);
				UpdateLocalizedFacetSetting(model.DeliveryTimeFacet, FacetGroupKind.DeliveryTime, ref clearFacetCache);
				UpdateLocalizedFacetSetting(model.AvailabilityFacet, FacetGroupKind.Availability, ref clearFacetCache);
				UpdateLocalizedFacetSetting(model.NewArrivalsFacet, FacetGroupKind.NewArrivals, ref clearFacetCache);
			}

			if (clearFacetCache)
			{
				_catalogSearchQueryAliasMapper.Value.ClearCommonFacetCache();
			}

			return NotifyAndRedirect("Search");
		}


		public ActionResult AllSettings()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
            
            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult AllSettings(GridCommand command)
        {
			var model = new GridModel<SettingModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
			{
				var stores = _services.StoreService.GetAllStores();
				string allStoresString = T("Admin.Common.StoresAll");

				var settings = _services.Settings
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
			}
			else
			{
				model.Data = Enumerable.Empty<SettingModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult SettingUpdate(SettingModel model, GridCommand command)
        {
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
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

				var setting = _services.Settings.GetSettingById(model.Id);
				if (setting == null)
				{
					return Content(T("Admin.Configuration.Settings.NoneWithThatId"));
				}

				// Use Store property (not StoreId) because appropriate property is stored in it.
				var storeId = model.Store.ToInt();

				if (!setting.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) || setting.StoreId != storeId)
				{
					// Setting name or store has been changed.
					_services.Settings.DeleteSetting(setting);
				}

				_services.Settings.SetSetting(model.Name, model.Value ?? "", storeId);

				_customerActivityService.InsertActivity("EditSettings", T("ActivityLog.EditSettings"));
			}

            return AllSettings(command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult SettingAdd([Bind(Exclude = "Id")] SettingModel model, GridCommand command)
        {
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
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
				_services.Settings.SetSetting(model.Name, model.Value, storeId);

				_customerActivityService.InsertActivity("AddNewSetting", T("ActivityLog.AddNewSetting", model.Name));
			}

            return AllSettings(command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult SettingDelete(int id, GridCommand command)
        {
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
			{
				var setting = _services.Settings.GetSettingById(id);

				_services.Settings.DeleteSetting(setting);
				_customerActivityService.InsertActivity("DeleteSetting", T("ActivityLog.DeleteSetting", setting.Name));
			}

            return AllSettings(command);
        }

        #endregion
    }
}
