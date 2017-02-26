using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Admin.Models.Common;
using SmartStore.Admin.Models.Settings;
using SmartStore.Admin.Validators.Settings;
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
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;
using SmartStore.Core.Search.Filter;
using SmartStore.Core.Themes;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Media.Storage;
using SmartStore.Services.Orders;
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
		private readonly IPictureService _pictureService;
		private readonly IDateTimeHelper _dateTimeHelper;
		private readonly IOrderService _orderService;
		private readonly IEncryptionService _encryptionService;
		private readonly IThemeRegistry _themeRegistry;
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
		private readonly IMediaMover _mediaMover;

		private StoreDependingSettingHelper _storeDependingSettings;
		private IDisposable _settingsWriteBatch;

		#endregion

		#region Constructors

        public SettingController(
            ICountryService countryService,
			IStateProvinceService stateProvinceService,
            IAddressService addressService,
			ITaxCategoryService taxCategoryService,
			IPictureService pictureService, 
            IDateTimeHelper dateTimeHelper,
            IOrderService orderService,
			IEncryptionService encryptionService,
            IThemeRegistry themeRegistry,
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
			IMediaMover mediaMover)
        {
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressService = addressService;
            _taxCategoryService = taxCategoryService;
            _pictureService = pictureService;
            _dateTimeHelper = dateTimeHelper;
            _orderService = orderService;
            _encryptionService = encryptionService;
            _themeRegistry = themeRegistry;
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
        }

		#endregion

		#region Utilities

		private StoreDependingSettingHelper StoreDependingSettings
		{
			get
			{
				if (_storeDependingSettings == null)
					_storeDependingSettings = new StoreDependingSettingHelper(this.ViewData);
				return _storeDependingSettings;
			}
		}

		private SelectListItem ResToSelectListItem(string resourceKey)
		{
			string value = _services.Localization.GetResource(resourceKey).EmptyNull();
			return new SelectListItem() { Text = value, Value = value };
		}

		#endregion

		#region Methods

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (filterContext.HttpContext.Request.HttpMethod.IsCaseInsensitiveEqual("POST"))
			{
				_settingsWriteBatch = _services.Settings.BeginScope(true);
			}

			base.OnActionExecuting(filterContext);
		}

		protected override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (_settingsWriteBatch != null)
			{
				_settingsWriteBatch.Dispose();
				_settingsWriteBatch = null;
			}

			base.OnActionExecuted(filterContext);
		}

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

        public ActionResult Blog()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var blogSettings = _services.Settings.LoadSetting<BlogSettings>(storeScope);
			var model = blogSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(blogSettings, model, storeScope, _services.Settings);

            return View(model);
        }
        [HttpPost]
        public ActionResult Blog(BlogSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var blogSettings = _services.Settings.LoadSetting<BlogSettings>(storeScope);
			blogSettings = model.ToEntity(blogSettings);

			StoreDependingSettings.UpdateSettings(blogSettings, form, storeScope, _services.Settings);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _services.Localization.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Blog");
        }




        public ActionResult Forum()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var forumSettings = _services.Settings.LoadSetting<ForumSettings>(storeScope);
			var model = forumSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(forumSettings, model, storeScope, _services.Settings);

			model.ForumEditorValues = forumSettings.ForumEditor.ToSelectList();
			
			return View(model);
        }
        [HttpPost]
        public ActionResult Forum(ForumSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var forumSettings = _services.Settings.LoadSetting<ForumSettings>(storeScope);
			forumSettings = model.ToEntity(forumSettings);

			StoreDependingSettings.UpdateSettings(forumSettings, form, storeScope, _services.Settings);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _services.Localization.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Forum");
        }




        public ActionResult News()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var newsSettings = _services.Settings.LoadSetting<NewsSettings>(storeScope);
			var model = newsSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(newsSettings, model, storeScope, _services.Settings);

			return View(model);
        }
        [HttpPost]
		public ActionResult News(NewsSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var newsSettings = _services.Settings.LoadSetting<NewsSettings>(storeScope);
			newsSettings = model.ToEntity(newsSettings);

			StoreDependingSettings.UpdateSettings(newsSettings, form, storeScope, _services.Settings);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _services.Localization.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("News");
        }

        public ActionResult Shipping()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var shippingSettings = _services.Settings.LoadSetting<ShippingSettings>(storeScope);
			var model = shippingSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(shippingSettings, model, storeScope, _services.Settings);

			//shipping origin
			if (storeScope > 0 && _services.Settings.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope))
				StoreDependingSettings.AddOverrideKey(shippingSettings, "ShippingOriginAddress");

			var originAddress = shippingSettings.ShippingOriginAddressId > 0
									 ? _addressService.GetAddressById(shippingSettings.ShippingOriginAddressId)
									 : null;
			if (originAddress != null)
				model.ShippingOriginAddress = originAddress.ToModel();
			else
				model.ShippingOriginAddress = new AddressModel();

			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.ShippingOriginAddress.AvailableCountries.Add(
					new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (originAddress != null && c.Id == originAddress.CountryId) }
				);
			}

            var states = originAddress != null && originAddress.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(originAddress.Country.Id, true).ToList() : new List<StateProvince>();
			if (states.Count > 0)
			{
				foreach (var s in states)
				{
					model.ShippingOriginAddress.AvailableStates.Add(
						new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == originAddress.StateProvinceId) }
					);
				}
			}
			else
			{
				model.ShippingOriginAddress.AvailableStates.Add(
					new SelectListItem() { Text = _services.Localization.GetResource("Admin.Address.OtherNonUS"), Value = "0" }
				);
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

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var shippingSettings = _services.Settings.LoadSetting<ShippingSettings>(storeScope);
			shippingSettings = model.ToEntity(shippingSettings);

			StoreDependingSettings.UpdateSettings(shippingSettings, form, storeScope, _services.Settings);

			bool shippingOriginAddressOverride = StoreDependingSettings.IsOverrideChecked(shippingSettings, "ShippingOriginAddress", form);

			if (shippingOriginAddressOverride || storeScope == 0)
			{
				//update address
				var addressId = _services.Settings.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope) ? shippingSettings.ShippingOriginAddressId : 0;
				
				var originAddress = _addressService.GetAddressById(addressId) ?? new Core.Domain.Common.Address() { CreatedOnUtc = DateTime.UtcNow };

				//update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
				model.ShippingOriginAddress.Id = addressId;
				originAddress = model.ShippingOriginAddress.ToEntity(originAddress);
				if (originAddress.Id > 0)
					_addressService.UpdateAddress(originAddress);
				else
					_addressService.InsertAddress(originAddress);
				shippingSettings.ShippingOriginAddressId = originAddress.Id;

				_services.Settings.SaveSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope, false);
			}
			else
			{
				_addressService.DeleteAddress(shippingSettings.ShippingOriginAddressId);
				
				_services.Settings.DeleteSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope);
			}

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _services.Localization.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Shipping");
        }

        public ActionResult Tax()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
            var taxSettings = _services.Settings.LoadSetting<TaxSettings>(storeScope);
			var model = taxSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(taxSettings, model, storeScope, _services.Settings);

            model.TaxBasedOnValues = taxSettings.TaxBasedOn.ToSelectList();
            model.TaxDisplayTypeValues = taxSettings.TaxDisplayType.ToSelectList();
			model.AvailableAuxiliaryServicesTaxTypes = taxSettings.AuxiliaryServicesTaxingType.ToSelectList();

            //tax categories
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

            //EU VAT countries
			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.EuVatShopCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == taxSettings.EuVatShopCountryId });
			}

            //default tax address
            var defaultAddress = (taxSettings.DefaultTaxAddressId > 0 ? _addressService.GetAddressById(taxSettings.DefaultTaxAddressId) : null);

			if (defaultAddress != null)
				model.DefaultTaxAddress = defaultAddress.ToModel();
			else
				model.DefaultTaxAddress = new AddressModel();

			if (storeScope > 0 && _services.Settings.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope))
				StoreDependingSettings.AddOverrideKey(taxSettings, "DefaultTaxAddress");

			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.DefaultTaxAddress.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(),
					Selected = (defaultAddress != null && c.Id == defaultAddress.CountryId) });
			}

			var states = (defaultAddress != null && defaultAddress.Country != null ?
				_stateProvinceService.GetStateProvincesByCountryId(defaultAddress.Country.Id, true).ToList() : new List<StateProvince>());

			if (states.Count > 0)
			{
				foreach (var s in states)
				{
					model.DefaultTaxAddress.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == defaultAddress.StateProvinceId) });
				}
			}
			else
			{
				model.DefaultTaxAddress.AvailableStates.Add(new SelectListItem { Text = _services.Localization.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
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

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var taxSettings = _services.Settings.LoadSetting<TaxSettings>(storeScope);
			taxSettings = model.ToEntity(taxSettings);

			StoreDependingSettings.UpdateSettings(taxSettings, form, storeScope, _services.Settings);

			bool defaultTaxAddressOverride = StoreDependingSettings.IsOverrideChecked(taxSettings, "DefaultTaxAddress", form);

			taxSettings.AllowCustomersToSelectTaxDisplayType = false;
			_services.Settings.UpdateSetting(taxSettings, x => x.AllowCustomersToSelectTaxDisplayType, false, storeScope);

			if (defaultTaxAddressOverride || storeScope == 0)
			{
				//update address
				var addressId = _services.Settings.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope) ? taxSettings.DefaultTaxAddressId : 0;

				var originAddress = _addressService.GetAddressById(addressId) ?? new Core.Domain.Common.Address() { CreatedOnUtc = DateTime.UtcNow };

				//update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
				model.DefaultTaxAddress.Id = addressId;
				originAddress = model.DefaultTaxAddress.ToEntity(originAddress);
				if (originAddress.Id > 0)
					_addressService.UpdateAddress(originAddress);
				else
					_addressService.InsertAddress(originAddress);
				taxSettings.DefaultTaxAddressId = originAddress.Id;

				_services.Settings.SaveSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope, false);
			}
			else if (storeScope > 0)
			{
				_addressService.DeleteAddress(taxSettings.DefaultTaxAddressId);

				_services.Settings.DeleteSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope);
			}

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _services.Localization.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Tax");
        }

        public ActionResult Catalog()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var catalogSettings = _services.Settings.LoadSetting<CatalogSettings>(storeScope);
			var model = catalogSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(catalogSettings, model, storeScope, _services.Settings);

			model.AvailableSubCategoryDisplayTypes = catalogSettings.SubCategoryDisplayType.ToSelectList();
			model.AvailablePriceDisplayTypes = catalogSettings.PriceDisplayType.ToSelectList();

            model.AvailableDefaultViewModes.Add(
				new SelectListItem { Value = "grid", Text = _services.Localization.GetResource("Common.Grid"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("grid") }
			);
            model.AvailableDefaultViewModes.Add(
				new SelectListItem { Value = "list", Text = _services.Localization.GetResource("Common.List"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("list") }
			);

            // default sort order modes
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

        [HttpPost]
        public ActionResult Catalog(CatalogSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			// Load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var catalogSettings = _services.Settings.LoadSetting<CatalogSettings>(storeScope);
			catalogSettings = model.ToEntity(catalogSettings);

			StoreDependingSettings.UpdateSettings(catalogSettings, form, storeScope, _services.Settings);

            // Activity log
            _customerActivityService.InsertActivity("EditSettings", _services.Localization.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Catalog");
        }

        public ActionResult RewardPoints()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var rewardPointsSettings = _services.Settings.LoadSetting<RewardPointsSettings>(storeScope);
			var store = (storeScope == 0 ? _services.StoreContext.CurrentStore : _services.StoreService.GetStoreById(storeScope));

			var model = rewardPointsSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(rewardPointsSettings, model, storeScope, _services.Settings);

			if (storeScope > 0 && (_services.Settings.SettingExists(rewardPointsSettings, x => x.PointsForPurchases_Amount, storeScope) ||
				_services.Settings.SettingExists(rewardPointsSettings, x => x.PointsForPurchases_Points, storeScope)))
			{
				StoreDependingSettings.AddOverrideKey(rewardPointsSettings, "PointsForPurchases_OverrideForStore");
			}

			model.PrimaryStoreCurrencyCode = store.PrimaryStoreCurrency.CurrencyCode;
			
			return View(model);
        }

        [HttpPost]
        public ActionResult RewardPoints(RewardPointsSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			if (!ModelState.IsValid)
				return RewardPoints();

			ModelState.Clear();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var rewardPointsSettings = _services.Settings.LoadSetting<RewardPointsSettings>(storeScope);
			rewardPointsSettings = model.ToEntity(rewardPointsSettings);

			StoreDependingSettings.UpdateSettings(rewardPointsSettings, form, storeScope, _services.Settings);

			bool pointsForPurchases = StoreDependingSettings.IsOverrideChecked(rewardPointsSettings, "PointsForPurchases", form);

			_services.Settings.UpdateSetting(rewardPointsSettings, x => x.PointsForPurchases_Amount, pointsForPurchases, storeScope);
			_services.Settings.UpdateSetting(rewardPointsSettings, x => x.PointsForPurchases_Points, pointsForPurchases, storeScope);

			//activity log
			_customerActivityService.InsertActivity("EditSettings", _services.Localization.GetResource("ActivityLog.EditSettings"));

			NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Updated"));

			return RedirectToAction("RewardPoints");
        }

        public ActionResult Order()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var orderSettings = _services.Settings.LoadSetting<OrderSettings>(storeScope);

			var allStores = _services.StoreService.GetAllStores();
			var store = (storeScope == 0 ? _services.StoreContext.CurrentStore : allStores.FirstOrDefault(x => x.Id == storeScope));

			var model = orderSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(orderSettings, model, storeScope, _services.Settings);

			model.PrimaryStoreCurrencyCode = store.PrimaryStoreCurrency.CurrencyCode;
			model.StoreCount = allStores.Count;

            //gift card activation/deactivation
            model.GiftCards_Activated_OrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.GiftCards_Deactivated_OrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();

			AddLocales(_languageService, model.Locales, (locale, languageId) =>
			{
				locale.ReturnRequestActions = orderSettings.GetLocalized(x => x.ReturnRequestActions, languageId, false, false);
				locale.ReturnRequestReasons = orderSettings.GetLocalized(x => x.ReturnRequestReasons, languageId, false, false);
			});

            //order ident
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

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var orderSettings = _services.Settings.LoadSetting<OrderSettings>(storeScope);
			orderSettings = model.ToEntity(orderSettings);

			StoreDependingSettings.UpdateSettings(orderSettings, form, storeScope, _services.Settings);

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

            //order ident
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

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _services.Localization.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Updated"));

            return RedirectToAction("Order");
        }

        public ActionResult ShoppingCart()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var allLanguages = _languageService.GetAllLanguages(true);
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

			StoreDependingSettings.UpdateSettings(shoppingCartSettings, form, storeScope, _services.Settings);

			_services.Settings.SaveSetting(shoppingCartSettings, x => x.ThirdPartyEmailHandOverLabel, 0, false);

			foreach (var localized in model.Locales)
			{
				_localizedEntityService.SaveLocalizedValue(shoppingCartSettings, x => x.ThirdPartyEmailHandOverLabel, localized.ThirdPartyEmailHandOverLabel, localized.LanguageId);
			}
            
            _customerActivityService.InsertActivity("EditSettings", T("ActivityLog.EditSettings"));

            NotifySuccess(T("Admin.Configuration.Updated"));
            return RedirectToAction("ShoppingCart");
        }

        public ActionResult Media()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var mediaSettings = Services.Settings.LoadSetting<MediaSettings>(storeScope);
			var model = mediaSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(mediaSettings, model, storeScope, Services.Settings);
           
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

			// media storage provider
			var currentStorageProvider = Services.Settings.GetSettingByKey<string>("Media.Storage.Provider");
			var provider = _providerManager.GetProvider<IMediaStorageProvider>(currentStorageProvider);

			model.StorageProvider = (provider != null ? _pluginMediator.GetLocalizedFriendlyName(provider.Metadata) : null);

			model.AvailableStorageProvider = _providerManager.GetAllProviders<IMediaStorageProvider>()
				.Where(x => !x.Metadata.SystemName.IsCaseInsensitiveEqual(currentStorageProvider))
				.Select(x => new SelectListItem { Text = _pluginMediator.GetLocalizedFriendlyName(x.Metadata), Value = x.Metadata.SystemName })
				.ToList();

			return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult Media(MediaSettingsModel model, FormCollection form)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var mediaSettings = Services.Settings.LoadSetting<MediaSettings>(storeScope);
			mediaSettings = model.ToEntity(mediaSettings);

			StoreDependingSettings.UpdateSettings(mediaSettings, form, storeScope, Services.Settings);

            _customerActivityService.InsertActivity("EditSettings", T("ActivityLog.EditSettings"));

            NotifySuccess(T("Admin.Configuration.Updated"));

            return RedirectToAction("Media");
        }

        [HttpPost]
        public ActionResult ChangeMediaStorage(string targetProvider)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var currentStorageProvider = Services.Settings.GetSettingByKey<string>("Media.Storage.Provider");
			var source = _providerManager.GetProvider<IMediaStorageProvider>(currentStorageProvider);
			var target = _providerManager.GetProvider<IMediaStorageProvider>(targetProvider);

			var success = _mediaMover.Move(source, target);

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

            model.CustomerSettings.AvailableCustomerNumberMethods = customerSettings.CustomerNumberMethod.ToSelectList(false);
            model.CustomerSettings.AvailableCustomerNumberVisibilities = customerSettings.CustomerNumberVisibility.ToSelectList(false);

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

			StoreDependingSettings.UpdateSettings(customerSettings, form, storeScope, _services.Settings);

			_services.Settings.SaveSetting(customerSettings, x => x.DefaultPasswordFormat, 0, false);

			var addressSettings = _services.Settings.LoadSetting<AddressSettings>(storeScope);
            addressSettings = model.AddressSettings.ToEntity(addressSettings);

			StoreDependingSettings.UpdateSettings(addressSettings, form, storeScope, _services.Settings);

			var dateTimeSettings = _services.Settings.LoadSetting<DateTimeSettings>(storeScope);
            dateTimeSettings.DefaultStoreTimeZoneId = model.DateTimeSettings.DefaultStoreTimeZoneId;
            dateTimeSettings.AllowCustomersToSetTimeZone = model.DateTimeSettings.AllowCustomersToSetTimeZone;

			StoreDependingSettings.UpdateSettings(dateTimeSettings, form, storeScope, _services.Settings);

			var externalAuthenticationSettings = _services.Settings.LoadSetting<ExternalAuthenticationSettings>(storeScope);
            externalAuthenticationSettings.AutoRegisterEnabled = model.ExternalAuthenticationSettings.AutoRegisterEnabled;

            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(addressSettings, x => x.Salutations, localized.Salutations, localized.LanguageId);
            }

			StoreDependingSettings.UpdateSettings(externalAuthenticationSettings, form, storeScope, _services.Settings);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _services.Localization.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("CustomerUser");
        }

        public ActionResult GeneralCommon()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

			var model = new GeneralCommonSettingsModel();
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);

			StoreDependingSettings.CreateViewDataObject(storeScope);

            //store information
			var storeInformationSettings = _services.Settings.LoadSetting<StoreInformationSettings>(storeScope);
			model.StoreInformationSettings.StoreClosed = storeInformationSettings.StoreClosed;
			model.StoreInformationSettings.StoreClosedAllowForAdmins = storeInformationSettings.StoreClosedAllowForAdmins;

			StoreDependingSettings.GetOverrideKeys(storeInformationSettings, model.StoreInformationSettings, storeScope, _services.Settings, false);

			//seo settings
			var seoSettings = _services.Settings.LoadSetting<SeoSettings>(storeScope);
            model.SeoSettings.PageTitleSeoAdjustment = seoSettings.PageTitleSeoAdjustment;
			model.SeoSettings.PageTitleSeparator = seoSettings.PageTitleSeparator;
			model.SeoSettings.DefaultTitle = seoSettings.DefaultTitle;
			model.SeoSettings.DefaultMetaKeywords = seoSettings.DefaultMetaKeywords;
			model.SeoSettings.DefaultMetaDescription = seoSettings.DefaultMetaDescription;
			model.SeoSettings.MetaRobotsContent = seoSettings.MetaRobotsContent;
			model.SeoSettings.ConvertNonWesternChars = seoSettings.ConvertNonWesternChars;
			model.SeoSettings.AllowUnicodeCharsInUrls = seoSettings.AllowUnicodeCharsInUrls;
			model.SeoSettings.SeoNameCharConversion = seoSettings.SeoNameCharConversion;
			model.SeoSettings.CanonicalUrlsEnabled = seoSettings.CanonicalUrlsEnabled;
			model.SeoSettings.CanonicalHostNameRule = seoSettings.CanonicalHostNameRule;
            model.SeoSettings.ExtraRobotsDisallows = String.Join(Environment.NewLine, seoSettings.ExtraRobotsDisallows);

			StoreDependingSettings.GetOverrideKeys(seoSettings, model.SeoSettings, storeScope, _services.Settings, false);

			//security settings
			var securitySettings = _services.Settings.LoadSetting<SecuritySettings>(storeScope);
			model.SecuritySettings.EncryptionKey = securitySettings.EncryptionKey;
			if (securitySettings.AdminAreaAllowedIpAddresses != null)
			{
				for (int i = 0; i < securitySettings.AdminAreaAllowedIpAddresses.Count; i++)
				{
					model.SecuritySettings.AdminAreaAllowedIpAddresses += securitySettings.AdminAreaAllowedIpAddresses[i];
					if (i != securitySettings.AdminAreaAllowedIpAddresses.Count - 1)
						model.SecuritySettings.AdminAreaAllowedIpAddresses += ",";
				}
			}
			model.SecuritySettings.HideAdminMenuItemsBasedOnPermissions = securitySettings.HideAdminMenuItemsBasedOnPermissions;

			var captchaSettings = _services.Settings.LoadSetting<CaptchaSettings>(storeScope);
			model.CaptchaSettings.Enabled = captchaSettings.Enabled;
			model.CaptchaSettings.ShowOnLoginPage = captchaSettings.ShowOnLoginPage;
			model.CaptchaSettings.ShowOnRegistrationPage = captchaSettings.ShowOnRegistrationPage;
			model.CaptchaSettings.ShowOnContactUsPage = captchaSettings.ShowOnContactUsPage;
			model.CaptchaSettings.ShowOnEmailWishlistToFriendPage = captchaSettings.ShowOnEmailWishlistToFriendPage;
			model.CaptchaSettings.ShowOnEmailProductToFriendPage = captchaSettings.ShowOnEmailProductToFriendPage;
			model.CaptchaSettings.ShowOnAskQuestionPage = captchaSettings.ShowOnAskQuestionPage;
			model.CaptchaSettings.ShowOnBlogCommentPage = captchaSettings.ShowOnBlogCommentPage;
			model.CaptchaSettings.ShowOnNewsCommentPage = captchaSettings.ShowOnNewsCommentPage;
			model.CaptchaSettings.ShowOnProductReviewPage = captchaSettings.ShowOnProductReviewPage;
			model.CaptchaSettings.ReCaptchaPublicKey = captchaSettings.ReCaptchaPublicKey;
			model.CaptchaSettings.ReCaptchaPrivateKey = captchaSettings.ReCaptchaPrivateKey;

			StoreDependingSettings.GetOverrideKeys(captchaSettings, model.CaptchaSettings, storeScope, _services.Settings, false);

			//PDF settings
			var pdfSettings = _services.Settings.LoadSetting<PdfSettings>(storeScope);
			model.PdfSettings.Enabled = pdfSettings.Enabled;
			model.PdfSettings.LetterPageSizeEnabled = pdfSettings.LetterPageSizeEnabled;
			model.PdfSettings.LogoPictureId = pdfSettings.LogoPictureId;
			model.PdfSettings.AttachOrderPdfToOrderPlacedEmail = pdfSettings.AttachOrderPdfToOrderPlacedEmail;
			model.PdfSettings.AttachOrderPdfToOrderCompletedEmail = pdfSettings.AttachOrderPdfToOrderCompletedEmail;

			StoreDependingSettings.GetOverrideKeys(pdfSettings, model.PdfSettings, storeScope, _services.Settings, false);

			//localization
			var localizationSettings = _services.Settings.LoadSetting<LocalizationSettings>(storeScope);
			model.LocalizationSettings.UseImagesForLanguageSelection = localizationSettings.UseImagesForLanguageSelection;
			model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled = localizationSettings.SeoFriendlyUrlsForLanguagesEnabled;
            model.LocalizationSettings.DefaultLanguageRedirectBehaviour = localizationSettings.DefaultLanguageRedirectBehaviour;
            model.LocalizationSettings.InvalidLanguageRedirectBehaviour = localizationSettings.InvalidLanguageRedirectBehaviour;
            model.LocalizationSettings.DetectBrowserUserLanguage = localizationSettings.DetectBrowserUserLanguage;

			StoreDependingSettings.GetOverrideKeys(localizationSettings, model.LocalizationSettings, storeScope, _services.Settings, false);

			//company information
			var companySettings = _services.Settings.LoadSetting<CompanyInformationSettings>(storeScope);
			model.CompanyInformationSettings.CompanyName = companySettings.CompanyName;
			model.CompanyInformationSettings.Salutation = companySettings.Salutation;
			model.CompanyInformationSettings.Title = companySettings.Title;
			model.CompanyInformationSettings.Firstname = companySettings.Firstname;
			model.CompanyInformationSettings.Lastname = companySettings.Lastname;
			model.CompanyInformationSettings.CompanyManagementDescription = companySettings.CompanyManagementDescription;
			model.CompanyInformationSettings.CompanyManagement = companySettings.CompanyManagement;
			model.CompanyInformationSettings.Street = companySettings.Street;
			model.CompanyInformationSettings.Street2 = companySettings.Street2;
			model.CompanyInformationSettings.ZipCode = companySettings.ZipCode;
			model.CompanyInformationSettings.City = companySettings.City;
			model.CompanyInformationSettings.CountryId = companySettings.CountryId;
			model.CompanyInformationSettings.Region = companySettings.Region;
			model.CompanyInformationSettings.VatId = companySettings.VatId;
			model.CompanyInformationSettings.CommercialRegister = companySettings.CommercialRegister;
			model.CompanyInformationSettings.TaxNumber = companySettings.TaxNumber;

			StoreDependingSettings.GetOverrideKeys(companySettings, model.CompanyInformationSettings, storeScope, _services.Settings, false);

			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.CompanyInformationSettings.AvailableCountries.Add(
					new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == model.CompanyInformationSettings.CountryId)
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

			//contact data
			var contactDataSettings = _services.Settings.LoadSetting<ContactDataSettings>(storeScope);
			model.ContactDataSettings.CompanyTelephoneNumber = contactDataSettings.CompanyTelephoneNumber;
			model.ContactDataSettings.HotlineTelephoneNumber = contactDataSettings.HotlineTelephoneNumber;
			model.ContactDataSettings.MobileTelephoneNumber = contactDataSettings.MobileTelephoneNumber;
			model.ContactDataSettings.CompanyFaxNumber = contactDataSettings.CompanyFaxNumber;
			model.ContactDataSettings.CompanyEmailAddress = contactDataSettings.CompanyEmailAddress;
			model.ContactDataSettings.WebmasterEmailAddress = contactDataSettings.WebmasterEmailAddress;
			model.ContactDataSettings.SupportEmailAddress = contactDataSettings.SupportEmailAddress;
			model.ContactDataSettings.ContactEmailAddress = contactDataSettings.ContactEmailAddress;

			StoreDependingSettings.GetOverrideKeys(contactDataSettings, model.ContactDataSettings, storeScope, _services.Settings, false);

			//bank connection
			var bankConnectionSettings = _services.Settings.LoadSetting<BankConnectionSettings>(storeScope);
			model.BankConnectionSettings.Bankname = bankConnectionSettings.Bankname;
			model.BankConnectionSettings.Bankcode = bankConnectionSettings.Bankcode;
			model.BankConnectionSettings.AccountNumber = bankConnectionSettings.AccountNumber;
			model.BankConnectionSettings.AccountHolder = bankConnectionSettings.AccountHolder;
			model.BankConnectionSettings.Iban = bankConnectionSettings.Iban;
			model.BankConnectionSettings.Bic = bankConnectionSettings.Bic;

			StoreDependingSettings.GetOverrideKeys(bankConnectionSettings, model.BankConnectionSettings, storeScope, _services.Settings, false);

			//social
			var socialSettings = _services.Settings.LoadSetting<SocialSettings>(storeScope);
			model.SocialSettings.ShowSocialLinksInFooter = socialSettings.ShowSocialLinksInFooter;
			model.SocialSettings.FacebookLink = socialSettings.FacebookLink;
			model.SocialSettings.GooglePlusLink = socialSettings.GooglePlusLink;
			model.SocialSettings.TwitterLink = socialSettings.TwitterLink;
			model.SocialSettings.PinterestLink = socialSettings.PinterestLink;
            model.SocialSettings.YoutubeLink = socialSettings.YoutubeLink;

			StoreDependingSettings.GetOverrideKeys(socialSettings, model.SocialSettings, storeScope, _services.Settings, false);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult GeneralCommon(GeneralCommonSettingsModel model, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);

			//store information
			var storeInformationSettings = _services.Settings.LoadSetting<StoreInformationSettings>(storeScope);
			storeInformationSettings.StoreClosed = model.StoreInformationSettings.StoreClosed;
			storeInformationSettings.StoreClosedAllowForAdmins = model.StoreInformationSettings.StoreClosedAllowForAdmins;

			StoreDependingSettings.UpdateSettings(storeInformationSettings, form, storeScope, _services.Settings);

			//seo settings
			var seoSettings = _services.Settings.LoadSetting<SeoSettings>(storeScope);
			var resetUserSeoCharacterTable = (seoSettings.SeoNameCharConversion != model.SeoSettings.SeoNameCharConversion);

			seoSettings.PageTitleSeparator = model.SeoSettings.PageTitleSeparator;
			seoSettings.PageTitleSeoAdjustment = model.SeoSettings.PageTitleSeoAdjustment;
			seoSettings.DefaultTitle = model.SeoSettings.DefaultTitle;
			seoSettings.DefaultMetaKeywords = model.SeoSettings.DefaultMetaKeywords;
			seoSettings.DefaultMetaDescription = model.SeoSettings.DefaultMetaDescription;
			seoSettings.MetaRobotsContent = model.SeoSettings.MetaRobotsContent;
			seoSettings.AllowUnicodeCharsInUrls = model.SeoSettings.AllowUnicodeCharsInUrls;
			seoSettings.SeoNameCharConversion = model.SeoSettings.SeoNameCharConversion;
			seoSettings.ConvertNonWesternChars = model.SeoSettings.ConvertNonWesternChars;
			seoSettings.CanonicalUrlsEnabled = model.SeoSettings.CanonicalUrlsEnabled;
			seoSettings.CanonicalHostNameRule = model.SeoSettings.CanonicalHostNameRule;
            seoSettings.ExtraRobotsDisallows = new List<string>(model.SeoSettings.ExtraRobotsDisallows.EmptyNull().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

			StoreDependingSettings.UpdateSettings(seoSettings, form, storeScope, _services.Settings);

			if (resetUserSeoCharacterTable)
			{
				SeoHelper.ResetUserSeoCharacterTable();
			}

			// security settings
			var securitySettings = _services.Settings.LoadSetting<SecuritySettings>(storeScope);
			if (securitySettings.AdminAreaAllowedIpAddresses == null)
				securitySettings.AdminAreaAllowedIpAddresses = new List<string>();
			securitySettings.AdminAreaAllowedIpAddresses.Clear();
			if (model.SecuritySettings.AdminAreaAllowedIpAddresses.HasValue())
			{
				foreach (string s in model.SecuritySettings.AdminAreaAllowedIpAddresses.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					if (!String.IsNullOrWhiteSpace(s))
						securitySettings.AdminAreaAllowedIpAddresses.Add(s.Trim());
				}
			}
			securitySettings.HideAdminMenuItemsBasedOnPermissions = model.SecuritySettings.HideAdminMenuItemsBasedOnPermissions;
			_services.Settings.SaveSetting(securitySettings);

			var captchaSettings = _services.Settings.LoadSetting<CaptchaSettings>(storeScope);
			captchaSettings.Enabled = model.CaptchaSettings.Enabled;
			captchaSettings.ShowOnLoginPage = model.CaptchaSettings.ShowOnLoginPage;
			captchaSettings.ShowOnRegistrationPage = model.CaptchaSettings.ShowOnRegistrationPage;
			captchaSettings.ShowOnContactUsPage = model.CaptchaSettings.ShowOnContactUsPage;
			captchaSettings.ShowOnEmailWishlistToFriendPage = model.CaptchaSettings.ShowOnEmailWishlistToFriendPage;
			captchaSettings.ShowOnEmailProductToFriendPage = model.CaptchaSettings.ShowOnEmailProductToFriendPage;
			captchaSettings.ShowOnAskQuestionPage = model.CaptchaSettings.ShowOnAskQuestionPage;
			captchaSettings.ShowOnBlogCommentPage = model.CaptchaSettings.ShowOnBlogCommentPage;
			captchaSettings.ShowOnNewsCommentPage = model.CaptchaSettings.ShowOnNewsCommentPage;
			captchaSettings.ShowOnProductReviewPage = model.CaptchaSettings.ShowOnProductReviewPage;
			captchaSettings.ReCaptchaPublicKey = model.CaptchaSettings.ReCaptchaPublicKey;
			captchaSettings.ReCaptchaPrivateKey = model.CaptchaSettings.ReCaptchaPrivateKey;

			StoreDependingSettings.UpdateSettings(captchaSettings, form, storeScope, _services.Settings);

			if (captchaSettings.Enabled && (String.IsNullOrWhiteSpace(captchaSettings.ReCaptchaPublicKey) || String.IsNullOrWhiteSpace(captchaSettings.ReCaptchaPrivateKey)))
			{
				NotifyError(_services.Localization.GetResource("Admin.Configuration.Settings.GeneralCommon.CaptchaEnabledNoKeys"));
			}

			// PDF settings
			var pdfSettings = _services.Settings.LoadSetting<PdfSettings>(storeScope);
			pdfSettings.Enabled = model.PdfSettings.Enabled;
			pdfSettings.LetterPageSizeEnabled = model.PdfSettings.LetterPageSizeEnabled;
			MediaHelper.UpdatePictureTransientState(pdfSettings.LogoPictureId, model.PdfSettings.LogoPictureId, true);
			pdfSettings.LogoPictureId = model.PdfSettings.LogoPictureId;
			pdfSettings.AttachOrderPdfToOrderPlacedEmail = model.PdfSettings.AttachOrderPdfToOrderPlacedEmail;
			pdfSettings.AttachOrderPdfToOrderCompletedEmail = model.PdfSettings.AttachOrderPdfToOrderCompletedEmail;

			StoreDependingSettings.UpdateSettings(pdfSettings, form, storeScope, _services.Settings);

			//localization settings
			var localizationSettings = _services.Settings.LoadSetting<LocalizationSettings>(storeScope);
            localizationSettings.DefaultLanguageRedirectBehaviour = model.LocalizationSettings.DefaultLanguageRedirectBehaviour;
            localizationSettings.InvalidLanguageRedirectBehaviour = model.LocalizationSettings.InvalidLanguageRedirectBehaviour;
			localizationSettings.UseImagesForLanguageSelection = model.LocalizationSettings.UseImagesForLanguageSelection;
            localizationSettings.DetectBrowserUserLanguage = model.LocalizationSettings.DetectBrowserUserLanguage;

			StoreDependingSettings.UpdateSettings(localizationSettings, form, storeScope, _services.Settings);

			_services.Settings.SaveSetting(localizationSettings, x => x.DefaultLanguageRedirectBehaviour, 0, false);
			_services.Settings.SaveSetting(localizationSettings, x => x.InvalidLanguageRedirectBehaviour, 0, false);

			if (localizationSettings.SeoFriendlyUrlsForLanguagesEnabled != model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
			{
				localizationSettings.SeoFriendlyUrlsForLanguagesEnabled = model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled;
				_services.Settings.SaveSetting(localizationSettings, x => x.SeoFriendlyUrlsForLanguagesEnabled, 0, false);

				System.Web.Routing.RouteTable.Routes.ClearSeoFriendlyUrlsCachedValueForRoutes();	// clear cached values of routes
			}

			//company information
			var companySettings = _services.Settings.LoadSetting<CompanyInformationSettings>(storeScope);
			companySettings.CompanyName = model.CompanyInformationSettings.CompanyName;
			companySettings.Salutation = model.CompanyInformationSettings.Salutation;
			companySettings.Title = model.CompanyInformationSettings.Title;
			companySettings.Firstname = model.CompanyInformationSettings.Firstname;
			companySettings.Lastname = model.CompanyInformationSettings.Lastname;
			companySettings.CompanyManagementDescription = model.CompanyInformationSettings.CompanyManagementDescription;
			companySettings.CompanyManagement = model.CompanyInformationSettings.CompanyManagement;
			companySettings.Street = model.CompanyInformationSettings.Street;
			companySettings.Street2 = model.CompanyInformationSettings.Street2;
			companySettings.ZipCode = model.CompanyInformationSettings.ZipCode;
			companySettings.City = model.CompanyInformationSettings.City;
			companySettings.CountryId = model.CompanyInformationSettings.CountryId;
			companySettings.Region = model.CompanyInformationSettings.Region;
			if (model.CompanyInformationSettings.CountryId != 0)
			{
				companySettings.CountryName = _countryService.GetCountryById(model.CompanyInformationSettings.CountryId).Name;
			}
			companySettings.VatId = model.CompanyInformationSettings.VatId;
			companySettings.CommercialRegister = model.CompanyInformationSettings.CommercialRegister;
			companySettings.TaxNumber = model.CompanyInformationSettings.TaxNumber;

			StoreDependingSettings.UpdateSettings(companySettings, form, storeScope, _services.Settings);

			//contact data
			var contactDataSettings = _services.Settings.LoadSetting<ContactDataSettings>(storeScope);
			contactDataSettings.CompanyTelephoneNumber = model.ContactDataSettings.CompanyTelephoneNumber;
			contactDataSettings.HotlineTelephoneNumber = model.ContactDataSettings.HotlineTelephoneNumber;
			contactDataSettings.MobileTelephoneNumber = model.ContactDataSettings.MobileTelephoneNumber;
			contactDataSettings.CompanyFaxNumber = model.ContactDataSettings.CompanyFaxNumber;
			contactDataSettings.CompanyEmailAddress = model.ContactDataSettings.CompanyEmailAddress;
			contactDataSettings.WebmasterEmailAddress = model.ContactDataSettings.WebmasterEmailAddress;
			contactDataSettings.SupportEmailAddress = model.ContactDataSettings.SupportEmailAddress;
			contactDataSettings.ContactEmailAddress = model.ContactDataSettings.ContactEmailAddress;

			StoreDependingSettings.UpdateSettings(contactDataSettings, form, storeScope, _services.Settings);

			//bank connection
			var bankConnectionSettings = _services.Settings.LoadSetting<BankConnectionSettings>(storeScope);
			bankConnectionSettings.Bankname = model.BankConnectionSettings.Bankname;
			bankConnectionSettings.Bankcode = model.BankConnectionSettings.Bankcode;
			bankConnectionSettings.AccountNumber = model.BankConnectionSettings.AccountNumber;
			bankConnectionSettings.AccountHolder = model.BankConnectionSettings.AccountHolder;
			bankConnectionSettings.Iban = model.BankConnectionSettings.Iban;
			bankConnectionSettings.Bic = model.BankConnectionSettings.Bic;

			StoreDependingSettings.UpdateSettings(bankConnectionSettings, form, storeScope, _services.Settings);

			//social
			var socialSettings = _services.Settings.LoadSetting<SocialSettings>(storeScope);
			socialSettings.ShowSocialLinksInFooter = model.SocialSettings.ShowSocialLinksInFooter;
			socialSettings.FacebookLink = model.SocialSettings.FacebookLink;
			socialSettings.GooglePlusLink = model.SocialSettings.GooglePlusLink;
			socialSettings.TwitterLink = model.SocialSettings.TwitterLink;
			socialSettings.PinterestLink = model.SocialSettings.PinterestLink;
            socialSettings.YoutubeLink = model.SocialSettings.YoutubeLink;

			StoreDependingSettings.UpdateSettings(socialSettings, form, storeScope, _services.Settings);

			//activity log
			_customerActivityService.InsertActivity("EditSettings", _services.Localization.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("GeneralCommon");
        }

        [HttpPost, ActionName("GeneralCommon")]
        [FormValueRequired("changeencryptionkey")]
        public ActionResult ChangeEnryptionKey(GeneralCommonSettingsModel model)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var securitySettings = _services.Settings.LoadSetting<SecuritySettings>(storeScope);

            try
            {
                if (model.SecuritySettings.EncryptionKey == null)
                    model.SecuritySettings.EncryptionKey = "";

                model.SecuritySettings.EncryptionKey = model.SecuritySettings.EncryptionKey.Trim();

                var newEncryptionPrivateKey = model.SecuritySettings.EncryptionKey;
                if (String.IsNullOrEmpty(newEncryptionPrivateKey) || newEncryptionPrivateKey.Length != 16)
                    throw new SmartException(_services.Localization.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TooShort"));

                string oldEncryptionPrivateKey = securitySettings.EncryptionKey;
                if (oldEncryptionPrivateKey == newEncryptionPrivateKey)
                    throw new SmartException(_services.Localization.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TheSame"));

                //update encrypted order info
                var orders = _orderService.LoadAllOrders();
                foreach (var order in orders)
                {
                    // new credit card encryption
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

                    // new direct debit encryption
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

                //update user information
                //optimization - load only users with PasswordFormat.Encrypted
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
                NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.Changed"));
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

		public ActionResult DataExchange()
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
				return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var settings = _services.Settings.LoadSetting<DataExchangeSettings>(storeScope);

			var model = new DataExchangeSettingsModel
			{
				MaxFileNameLength = settings.MaxFileNameLength,
				ImageImportFolder = settings.ImageImportFolder,
				ImageDownloadTimeout = settings.ImageDownloadTimeout
			};

			StoreDependingSettings.GetOverrideKeys(settings, model, storeScope, _services.Settings);

			return View(model);
		}

		[HttpPost]
		public ActionResult DataExchange(DataExchangeSettingsModel model, FormCollection form)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
				return AccessDeniedView();

			if (!ModelState.IsValid)
				return DataExchange();

			ModelState.Clear();

			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var settings = _services.Settings.LoadSetting<DataExchangeSettings>(storeScope);

			settings.MaxFileNameLength = model.MaxFileNameLength;
			settings.ImageImportFolder = model.ImageImportFolder;
			settings.ImageDownloadTimeout = model.ImageDownloadTimeout;

			StoreDependingSettings.UpdateSettings(settings, form, storeScope, _services.Settings);

			_customerActivityService.InsertActivity("EditSettings", T("ActivityLog.EditSettings"));

			NotifySuccess(T("Admin.Configuration.Updated"));

			return RedirectToAction("DataExchange");
		}

		public ActionResult Search()
		{
			if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
				return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<SearchSettings>(storeScope);
			var megaSearchDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.MegaSearch");

			var model = new SearchSettingsModel();
			model.SearchMode = settings.SearchMode;
			model.SearchFields = settings.SearchFields;
			model.InstantSearchEnabled = settings.InstantSearchEnabled;
			model.InstantSearchNumberOfProducts = settings.InstantSearchNumberOfProducts;
			model.InstantSearchTermMinLength = settings.InstantSearchTermMinLength;
			model.ShowProductImagesInInstantSearch = settings.ShowProductImagesInInstantSearch;
			model.FilterMinHitCount = settings.FilterMinHitCount;
			model.FilterMaxChoicesCount = settings.FilterMaxChoicesCount;
			
			if (megaSearchDescriptor == null)
			{
				model.SearchFieldsNote = T("Admin.Configuration.Settings.Search.SearchFieldsNote");
			}

			model.AvailableSearchModes = settings.SearchMode.ToSelectList().ToList();

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

			// global filters
			var globalFilters = settings.GlobalFilters.HasValue()
				? JsonConvert.DeserializeObject<List<GlobalSearchFilterDescriptor>>(settings.GlobalFilters)
				: new List<GlobalSearchFilterDescriptor>();

			var displayOrder = (globalFilters.Any() ? globalFilters.Max(x => x.DisplayOrder) : 0);

			foreach (var fieldName in new string[] { "manufacturerid", "price", "rate", "deliveryid" })
			{
				var filter = globalFilters.FirstOrDefault(x => x.FieldName.IsCaseInsensitiveEqual(fieldName));

				model.GlobalFilters.Add(new GlobalSearchFilterDescriptor
				{
					FieldName = fieldName,
					Disabled = filter != null ? filter.Disabled : false,
					DisplayOrder = filter != null ? filter.DisplayOrder : ++displayOrder,
					FriendlyName = T(FacetDescriptor.GetLabelResourceKey(fieldName))
				});
			}

			StoreDependingSettings.GetOverrideKeys(settings, model, storeScope, Services.Settings);

			return View(model);
		}

		[HttpPost]
		public ActionResult Search(SearchSettingsModel model, FormCollection form)
		{
			if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageSettings))
				return AccessDeniedView();

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<SearchSettings>(storeScope);

			var validator = new SearchSettingValidator(Services.Localization, x =>
			{
				return storeScope == 0 || storeDependingSettingHelper.IsOverrideChecked(settings, x, form);
			});

			validator.Validate(model, ModelState);

			if (!ModelState.IsValid)
				return Search();

			ModelState.Clear();

			settings.SearchMode = model.SearchMode;
			settings.SearchFields = model.SearchFields;
			settings.InstantSearchEnabled = model.InstantSearchEnabled;
			settings.InstantSearchNumberOfProducts = model.InstantSearchNumberOfProducts;
			settings.InstantSearchTermMinLength = model.InstantSearchTermMinLength;
			settings.ShowProductImagesInInstantSearch = model.ShowProductImagesInInstantSearch;
			settings.GlobalFilters = JsonConvert.SerializeObject(model.GlobalFilters);
			settings.FilterMinHitCount = model.FilterMinHitCount;
			settings.FilterMaxChoicesCount = model.FilterMaxChoicesCount;

			StoreDependingSettings.UpdateSettings(settings, form, storeScope, Services.Settings);

			_customerActivityService.InsertActivity("EditSettings", T("ActivityLog.EditSettings"));

			NotifySuccess(T("Admin.Configuration.Updated"));

			return RedirectToAction("Search");
		}


		//all settings
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
				string allStoresString = _services.Localization.GetResource("Admin.Common.StoresAll");

				var settings = _services.Settings
					.GetAllSettings()
					.Select(x =>
					{
						var settingModel = new SettingModel()
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
					return Content(T("Admin.Configuration.Settings.NoneWithThatId"));

				var storeId = model.Store.ToInt(); //use Store property (not StoreId) because appropriate property is stored in it

				if (!setting.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) || setting.StoreId != storeId)
				{
					//setting name or store has been changed
					_services.Settings.DeleteSetting(setting);
				}

				_services.Settings.SetSetting(model.Name, model.Value, storeId);

				//activity log
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
					var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
					return Content(modelStateErrors.FirstOrDefault());
				}

				var storeId = model.Store.ToInt(); //use Store property (not StoreId) because appropriate property is stored in it
				_services.Settings.SetSetting(model.Name, model.Value, storeId);

				//activity log
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

				//activity log
				_customerActivityService.InsertActivity("DeleteSetting", T("ActivityLog.DeleteSetting", setting.Name));
			}

            return AllSettings(command);
        }

        #endregion
    }
}
