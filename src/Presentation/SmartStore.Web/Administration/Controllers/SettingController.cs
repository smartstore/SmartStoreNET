using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Common;
using SmartStore.Admin.Models.Settings;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.UI.Captcha;
using SmartStore.Web.Framework.Settings;
using Telerik.Web.Mvc;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Themes;
using SmartStore.Services.Stores;
using SmartStore.Admin.Models.Stores;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class SettingController : AdminControllerBase
	{
		#region Fields

        private readonly ISettingService _settingService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ICurrencyService _currencyService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IOrderService _orderService;
        private readonly IEncryptionService _encryptionService;
        private readonly IThemeRegistry _themeRegistry;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly IFulltextService _fulltextService;
        private readonly IMaintenanceService _maintenanceService;
		private readonly IStoreService _storeService;
		private readonly IWorkContext _workContext;
		private readonly IGenericAttributeService _genericAttributeService;

		private StoreDependingSettingHelper _storeDependingSettings;	// codehint: sm-add

		#endregion

		#region Constructors

        public SettingController(ISettingService settingService,
            ICountryService countryService, IStateProvinceService stateProvinceService,
            IAddressService addressService, ITaxCategoryService taxCategoryService,
            ICurrencyService currencyService, IPictureService pictureService, 
            ILocalizationService localizationService, IDateTimeHelper dateTimeHelper,
            IOrderService orderService, IEncryptionService encryptionService,
            IThemeRegistry themeRegistry, ICustomerService customerService, 
            ICustomerActivityService customerActivityService, IPermissionService permissionService,
            IWebHelper webHelper, IFulltextService fulltextService,
			IMaintenanceService maintenanceService, IStoreService storeService,
			IWorkContext workContext, IGenericAttributeService genericAttributeService)
        {
            this._settingService = settingService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._addressService = addressService;
            this._taxCategoryService = taxCategoryService;
            this._currencyService = currencyService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._orderService = orderService;
            this._encryptionService = encryptionService;
            this._themeRegistry = themeRegistry;
            this._customerService = customerService;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._webHelper = webHelper;
            this._fulltextService = fulltextService;
            this._maintenanceService = maintenanceService;
			this._storeService = storeService;
			this._workContext = workContext;
			this._genericAttributeService = genericAttributeService;
        }

		#endregion 

		/// <remarks>codehint: sm-add</remarks>
		private StoreDependingSettingHelper StoreDependingSettings
		{
			get
			{
				if (_storeDependingSettings == null)
					_storeDependingSettings = new StoreDependingSettingHelper(this.ViewData);
				return _storeDependingSettings;
			}
		}

        #region Methods

		[ChildActionOnly]
		public ActionResult StoreScopeConfiguration()
		{
			var allStores = _storeService.GetAllStores();
			if (allStores.Count < 2)
				return Content("");

			var model = new StoreScopeConfigurationModel();
			foreach (var s in allStores)
			{
				model.Stores.Add(new StoreModel()
				{
					Id = s.Id,
					Name = s.Name
				});
			}
			model.StoreId = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

			return PartialView(model);
		}

		public ActionResult ChangeStoreScopeConfiguration(int storeid, string returnUrl = "")
		{
			var store = _storeService.GetStoreById(storeid);
			if (store != null || storeid == 0)
			{
				_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
					SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration, storeid);
			}
			//url referrer
			if (String.IsNullOrEmpty(returnUrl))
				returnUrl = _webHelper.GetUrlReferrer();
			//home page
			if (String.IsNullOrEmpty(returnUrl))
				returnUrl = Url.Action("Index", "Home");
			return Redirect(returnUrl);
		}

        public ActionResult Blog()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var blogSettings = _settingService.LoadSetting<BlogSettings>(storeScope);
			var model = blogSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(blogSettings, model, storeScope, _settingService);

            return View(model);
        }
        [HttpPost]
        public ActionResult Blog(BlogSettingsModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var blogSettings = _settingService.LoadSetting<BlogSettings>(storeScope);
			blogSettings = model.ToEntity(blogSettings);

			StoreDependingSettings.UpdateSettings(blogSettings, form, storeScope, _settingService);

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Blog");
        }




        public ActionResult Forum()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var forumSettings = _settingService.LoadSetting<ForumSettings>(storeScope);
			var model = forumSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(forumSettings, model, storeScope, _settingService);

			model.ForumEditorValues = forumSettings.ForumEditor.ToSelectList();
			
			return View(model);
        }
        [HttpPost]
        public ActionResult Forum(ForumSettingsModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var forumSettings = _settingService.LoadSetting<ForumSettings>(storeScope);
			forumSettings = model.ToEntity(forumSettings);

			StoreDependingSettings.UpdateSettings(forumSettings, form, storeScope, _settingService);

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Forum");
        }




        public ActionResult News()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var newsSettings = _settingService.LoadSetting<NewsSettings>(storeScope);
			var model = newsSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(newsSettings, model, storeScope, _settingService);

			return View(model);
        }
        [HttpPost]
		public ActionResult News(NewsSettingsModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var newsSettings = _settingService.LoadSetting<NewsSettings>(storeScope);
			newsSettings = model.ToEntity(newsSettings);

			StoreDependingSettings.UpdateSettings(newsSettings, form, storeScope, _settingService);

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("News");
        }




        public ActionResult Shipping()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var shippingSettings = _settingService.LoadSetting<ShippingSettings>(storeScope);
			var model = shippingSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(shippingSettings, model, storeScope, _settingService);

			//shipping origin
			if (storeScope > 0 && _settingService.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope))
				StoreDependingSettings.AddOverrideKey(shippingSettings, "ShippingOriginAddress");

			var originAddress = shippingSettings.ShippingOriginAddressId > 0
									 ? _addressService.GetAddressById(shippingSettings.ShippingOriginAddressId)
									 : null;
			if (originAddress != null)
				model.ShippingOriginAddress = originAddress.ToModel();
			else
				model.ShippingOriginAddress = new AddressModel();

			// codehint: sm-delete
            // model.ShippingOriginAddress.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });
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
					new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" }
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var shippingSettings = _settingService.LoadSetting<ShippingSettings>(storeScope);
			shippingSettings = model.ToEntity(shippingSettings);

			StoreDependingSettings.UpdateSettings(shippingSettings, form, storeScope, _settingService);

			bool shippingOriginAddressOverride = StoreDependingSettings.IsOverrideChecked(shippingSettings, "ShippingOriginAddress", form);

			if (shippingOriginAddressOverride || storeScope == 0)
			{
				//update address
				var addressId = _settingService.SettingExists(shippingSettings, x => x.ShippingOriginAddressId, storeScope) ? shippingSettings.ShippingOriginAddressId : 0;
				
				var originAddress = _addressService.GetAddressById(addressId) ?? new Core.Domain.Common.Address() { CreatedOnUtc = DateTime.UtcNow };

				//update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
				model.ShippingOriginAddress.Id = addressId;
				originAddress = model.ShippingOriginAddress.ToEntity(originAddress);
				if (originAddress.Id > 0)
					_addressService.UpdateAddress(originAddress);
				else
					_addressService.InsertAddress(originAddress);
				shippingSettings.ShippingOriginAddressId = originAddress.Id;

				_settingService.SaveSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope, false);
			}
			else
			{
				_addressService.DeleteAddress(shippingSettings.ShippingOriginAddressId);	// codehint: sm-add
				
				_settingService.DeleteSetting(shippingSettings, x => x.ShippingOriginAddressId, storeScope);
			}

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Shipping");
        }




        public ActionResult Tax()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var taxSettings = _settingService.LoadSetting<TaxSettings>(storeScope);
			var model = taxSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(taxSettings, model, storeScope, _settingService);

            model.TaxBasedOnValues = taxSettings.TaxBasedOn.ToSelectList();
            model.TaxDisplayTypeValues = taxSettings.TaxDisplayType.ToSelectList();

            //tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            // model.ShippingTaxCategories.Add(new SelectListItem() { Text = "---", Value = "0" }); // codehint: sm-delete
			foreach (var tc in taxCategories)
			{
				model.ShippingTaxCategories.Add(
					new SelectListItem() { Text = tc.Name, Value = tc.Id.ToString(), Selected = tc.Id == taxSettings.ShippingTaxClassId }
				);
			}
            // model.PaymentMethodAdditionalFeeTaxCategories.Add(new SelectListItem() { Text = "---", Value = "0" }); // codehint: sm-delete
			foreach (var tc in taxCategories)
			{
				model.PaymentMethodAdditionalFeeTaxCategories.Add(
					new SelectListItem() { Text = tc.Name, Value = tc.Id.ToString(), Selected = tc.Id == taxSettings.PaymentMethodAdditionalFeeTaxClassId }
				);
			}

            //EU VAT countries
            // model.EuVatShopCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" }); // codehint: sm-delete
			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.EuVatShopCountries.Add(
					new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = c.Id == taxSettings.EuVatShopCountryId }
				);
			}

            //default tax address
            var defaultAddress = taxSettings.DefaultTaxAddressId > 0
                                     ? _addressService.GetAddressById(taxSettings.DefaultTaxAddressId)
                                     : null;

			if (defaultAddress != null)
				model.DefaultTaxAddress = defaultAddress.ToModel();
			else
				model.DefaultTaxAddress = new AddressModel();

			if (storeScope > 0 && _settingService.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope))
				StoreDependingSettings.AddOverrideKey(taxSettings, "DefaultTaxAddress");

            // model.DefaultTaxAddress.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" }); // codehint: sm-delete
			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.DefaultTaxAddress.AvailableCountries.Add(
					new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (defaultAddress != null && c.Id == defaultAddress.CountryId) }
				);
			}

            var states = defaultAddress != null && defaultAddress.Country != null ? 
				_stateProvinceService.GetStateProvincesByCountryId(defaultAddress.Country.Id, true).ToList() : new List<StateProvince>();
			if (states.Count > 0)
			{
				foreach (var s in states)
				{
					model.DefaultTaxAddress.AvailableStates.Add(
						new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == defaultAddress.StateProvinceId) }
					);
				}
			}
			else
			{
				model.DefaultTaxAddress.AvailableStates.Add(
					new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" }
				);
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var taxSettings = _settingService.LoadSetting<TaxSettings>(storeScope);
			taxSettings = model.ToEntity(taxSettings);

			StoreDependingSettings.UpdateSettings(taxSettings, form, storeScope, _settingService);

			bool defaultTaxAddressOverride = StoreDependingSettings.IsOverrideChecked(taxSettings, "DefaultTaxAddress", form);

			//codehint: sm-add
			taxSettings.AllowCustomersToSelectTaxDisplayType = false;
			_settingService.UpdateSetting(taxSettings, x => x.AllowCustomersToSelectTaxDisplayType, false, storeScope);

			if (defaultTaxAddressOverride || storeScope == 0)
			{
				//update address
				var addressId = _settingService.SettingExists(taxSettings, x => x.DefaultTaxAddressId, storeScope) ? taxSettings.DefaultTaxAddressId : 0;

				var originAddress = _addressService.GetAddressById(addressId) ?? new Core.Domain.Common.Address() { CreatedOnUtc = DateTime.UtcNow };

				//update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
				model.DefaultTaxAddress.Id = addressId;
				originAddress = model.DefaultTaxAddress.ToEntity(originAddress);
				if (originAddress.Id > 0)
					_addressService.UpdateAddress(originAddress);
				else
					_addressService.InsertAddress(originAddress);
				taxSettings.DefaultTaxAddressId = originAddress.Id;

				_settingService.SaveSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope, false);
			}
			else if (storeScope > 0)
			{
				_addressService.DeleteAddress(taxSettings.DefaultTaxAddressId);		// codehint: sm-add

				_settingService.DeleteSetting(taxSettings, x => x.DefaultTaxAddressId, storeScope);
			}

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Tax");
        }




        public ActionResult Catalog(string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeScope);
			var model = catalogSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(catalogSettings, model, storeScope, _settingService);

            model.AvailableDefaultViewModes.Add(
				new SelectListItem { Value = "grid", Text = _localizationService.GetResource("Common.Grid"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("grid") }
			);
            model.AvailableDefaultViewModes.Add(
				new SelectListItem { Value = "list", Text = _localizationService.GetResource("Common.List"), Selected = model.DefaultViewMode.IsCaseInsensitiveEqual("list") }
			);

            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }
        [HttpPost]
        public ActionResult Catalog(CatalogSettingsModel model, string selectedTab, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var catalogSettings = _settingService.LoadSetting<CatalogSettings>(storeScope);
			catalogSettings = model.ToEntity(catalogSettings);

			StoreDependingSettings.UpdateSettings(catalogSettings, form, storeScope, _settingService);

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Catalog", new { selectedTab = selectedTab });
        }



        public ActionResult RewardPoints()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var rewardPointsSettings = _settingService.LoadSetting<RewardPointsSettings>(storeScope);
			var model = rewardPointsSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(rewardPointsSettings, model, storeScope, _settingService);

			if (storeScope > 0 && (_settingService.SettingExists(rewardPointsSettings, x => x.PointsForPurchases_Amount, storeScope) ||
				_settingService.SettingExists(rewardPointsSettings, x => x.PointsForPurchases_Points, storeScope)))
			{
				StoreDependingSettings.AddOverrideKey(rewardPointsSettings, "PointsForPurchases_OverrideForStore");
			}

			var currencySettings = _settingService.LoadSetting<CurrencySettings>(storeScope);
			model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
			
			return View(model);
        }
        [HttpPost]
        public ActionResult RewardPoints(RewardPointsSettingsModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			if (ModelState.IsValid)
			{
				//load settings for a chosen store scope
				var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
				var rewardPointsSettings = _settingService.LoadSetting<RewardPointsSettings>(storeScope);
				rewardPointsSettings = model.ToEntity(rewardPointsSettings);

				StoreDependingSettings.UpdateSettings(rewardPointsSettings, form, storeScope, _settingService);

				bool pointsForPurchases = StoreDependingSettings.IsOverrideChecked(rewardPointsSettings, "PointsForPurchases", form);

				_settingService.UpdateSetting(rewardPointsSettings, x => x.PointsForPurchases_Amount, pointsForPurchases, storeScope);
				_settingService.UpdateSetting(rewardPointsSettings, x => x.PointsForPurchases_Points, pointsForPurchases, storeScope);

				//now clear settings cache
				_settingService.ClearCache();

				//activity log
				_customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

				NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
			}
			else
			{
				//If we got this far, something failed, redisplay form
				foreach (var modelState in ModelState.Values)
					foreach (var error in modelState.Errors)
						NotifyError(error.ErrorMessage);
			}
			return RedirectToAction("RewardPoints");
        }




        public ActionResult Order(string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var orderSettings = _settingService.LoadSetting<OrderSettings>(storeScope);
			var model = orderSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(orderSettings, model, storeScope, _settingService);

			var currencySettings = _settingService.LoadSetting<CurrencySettings>(storeScope);
			model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(currencySettings.PrimaryStoreCurrencyId).CurrencyCode;

            //gift card activation/deactivation
            model.GiftCards_Activated_OrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            //model.GiftCards_Activated_OrderStatuses.Insert(0, new SelectListItem() { Text = "---", Value = "0" }); // codehint: sm-delete
            model.GiftCards_Deactivated_OrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            //model.GiftCards_Deactivated_OrderStatuses.Insert(0, new SelectListItem() { Text = "---", Value = "0" }); // codehint: sm-delete

            //parse return request actions
			for (int i = 0; i < orderSettings.ReturnRequestActions.Count; i++)
            {
				model.ReturnRequestActionsParsed += orderSettings.ReturnRequestActions[i];
				if (i != orderSettings.ReturnRequestActions.Count - 1)
                    model.ReturnRequestActionsParsed += ",";
            }
            //parse return request reasons
			for (int i = 0; i < orderSettings.ReturnRequestReasons.Count; i++)
            {
				model.ReturnRequestReasonsParsed += orderSettings.ReturnRequestReasons[i];
				if (i != orderSettings.ReturnRequestReasons.Count - 1)
                    model.ReturnRequestReasonsParsed += ",";
            }

            //order ident
            model.OrderIdent = _maintenanceService.GetTableIdent<Order>();

            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }
        [HttpPost]
        public ActionResult Order(OrderSettingsModel model, string selectedTab, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
				//load settings for a chosen store scope
				var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
				var orderSettings = _settingService.LoadSetting<OrderSettings>(storeScope);
				orderSettings = model.ToEntity(orderSettings);

				StoreDependingSettings.UpdateSettings(orderSettings, form, storeScope, _settingService);

				//parse return request actions
				orderSettings.ReturnRequestActions.Clear();
				foreach (var returnAction in model.ReturnRequestActionsParsed.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
					orderSettings.ReturnRequestActions.Add(returnAction);
				_settingService.SaveSetting(orderSettings, x => x.ReturnRequestActions, 0, false);		// codehint: sm-edit
				
				//parse return request reasons
				orderSettings.ReturnRequestReasons.Clear();
				foreach (var returnReason in model.ReturnRequestReasonsParsed.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
					orderSettings.ReturnRequestReasons.Add(returnReason);
				_settingService.SaveSetting(orderSettings, x => x.ReturnRequestReasons, 0, false);		// codehint: sm-edit

				// codehint: sm-edit
				if (model.GiftCards_Activated_OrderStatusId.HasValue)
					_settingService.SaveSetting(orderSettings, x => x.GiftCards_Activated_OrderStatusId, 0, false);
				else
					_settingService.DeleteSetting(orderSettings, x => x.GiftCards_Activated_OrderStatusId);

				if (model.GiftCards_Deactivated_OrderStatusId.HasValue)
					_settingService.SaveSetting(orderSettings, x => x.GiftCards_Deactivated_OrderStatusId, 0, false);
				else
					_settingService.DeleteSetting(orderSettings, x => x.GiftCards_Deactivated_OrderStatusId);

				//now clear settings cache
				_settingService.ClearCache();

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
                _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

                NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
            }
            else
            {
				//If we got this far, something failed, redisplay form
                foreach (var modelState in ModelState.Values)
                    foreach (var error in modelState.Errors)
						NotifyError(error.ErrorMessage);
            }
            return RedirectToAction("Order", new { selectedTab = selectedTab });
        }




        public ActionResult ShoppingCart()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var shoppingCartSettings = _settingService.LoadSetting<ShoppingCartSettings>(storeScope);
			var model = shoppingCartSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(shoppingCartSettings, model, storeScope, _settingService);


			return View(model);
        }
        [HttpPost]
        public ActionResult ShoppingCart(ShoppingCartSettingsModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var shoppingCartSettings = _settingService.LoadSetting<ShoppingCartSettings>(storeScope);
			shoppingCartSettings = model.ToEntity(shoppingCartSettings);

			StoreDependingSettings.UpdateSettings(shoppingCartSettings, form, storeScope, _settingService);

			//now clear settings cache
			_settingService.ClearCache();
            
            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("ShoppingCart");
        }




        public ActionResult Media()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var mediaSettings = _settingService.LoadSetting<MediaSettings>(storeScope);
			var model = mediaSettings.ToModel();

			StoreDependingSettings.GetOverrideKeys(mediaSettings, model, storeScope, _settingService);

			model.PicturesStoredIntoDatabase = _pictureService.StoreInDb;

            var resKey = "Admin.Configuration.Settings.Media.PictureZoomType.";
            
            model.AvailablePictureZoomTypes.Add(new SelectListItem { 
                Text = _localizationService.GetResource(resKey + "Window"), 
                Value = "window", 
                Selected = model.PictureZoomType.Equals("window") 
            });
            model.AvailablePictureZoomTypes.Add(new SelectListItem {
                Text = _localizationService.GetResource(resKey + "Inner"),
                Value = "inner", 
                Selected = model.PictureZoomType.Equals("inner") 
            });
            model.AvailablePictureZoomTypes.Add(new SelectListItem {
                Text = _localizationService.GetResource(resKey + "Lens"),
                Value = "lens", 
                Selected = model.PictureZoomType.Equals("lens") 
            });

            return View(model);
        }
        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult Media(MediaSettingsModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var mediaSettings = _settingService.LoadSetting<MediaSettings>(storeScope);
			mediaSettings = model.ToEntity(mediaSettings);

			StoreDependingSettings.UpdateSettings(mediaSettings, form, storeScope, _settingService);

			//now clear settings cache
			_settingService.ClearCache();

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }
        [HttpPost, ActionName("Media")]
        [FormValueRequired("change-picture-storage")]
        public ActionResult ChangePictureStorage()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            _pictureService.StoreInDb = !_pictureService.StoreInDb;

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }



        public ActionResult CustomerUser(string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var customerSettings = _settingService.LoadSetting<CustomerSettings>(storeScope);
			var addressSettings = _settingService.LoadSetting<AddressSettings>(storeScope);
			var dateTimeSettings = _settingService.LoadSetting<DateTimeSettings>(storeScope);
			var externalAuthenticationSettings = _settingService.LoadSetting<ExternalAuthenticationSettings>(storeScope);

            //merge settings
            var model = new CustomerUserSettingsModel();
            model.CustomerSettings = customerSettings.ToModel();
            model.AddressSettings = addressSettings.ToModel();

            model.DateTimeSettings.AllowCustomersToSetTimeZone = dateTimeSettings.AllowCustomersToSetTimeZone;
            model.DateTimeSettings.DefaultStoreTimeZoneId = _dateTimeHelper.DefaultStoreTimeZone.Id;
            foreach (TimeZoneInfo timeZone in _dateTimeHelper.GetSystemTimeZones())
            {
                model.DateTimeSettings.AvailableTimeZones.Add(new SelectListItem()
                    {
                        Text = timeZone.DisplayName,
                        Value = timeZone.Id,
                        Selected = timeZone.Id.Equals(_dateTimeHelper.DefaultStoreTimeZone.Id, StringComparison.InvariantCultureIgnoreCase)
                    });
            }

            model.ExternalAuthenticationSettings.AutoRegisterEnabled = externalAuthenticationSettings.AutoRegisterEnabled;

            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }
        [HttpPost]
        public ActionResult CustomerUser(CustomerUserSettingsModel model, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var customerSettings = _settingService.LoadSetting<CustomerSettings>(storeScope);
			var addressSettings = _settingService.LoadSetting<AddressSettings>(storeScope);
			var dateTimeSettings = _settingService.LoadSetting<DateTimeSettings>(storeScope);
			var externalAuthenticationSettings = _settingService.LoadSetting<ExternalAuthenticationSettings>(storeScope);

            customerSettings = model.CustomerSettings.ToEntity(customerSettings);
            _settingService.SaveSetting(customerSettings);

            addressSettings = model.AddressSettings.ToEntity(addressSettings);
            _settingService.SaveSetting(addressSettings);

            dateTimeSettings.DefaultStoreTimeZoneId = model.DateTimeSettings.DefaultStoreTimeZoneId;
            dateTimeSettings.AllowCustomersToSetTimeZone = model.DateTimeSettings.AllowCustomersToSetTimeZone;
            _settingService.SaveSetting(dateTimeSettings);

            externalAuthenticationSettings.AutoRegisterEnabled = model.ExternalAuthenticationSettings.AutoRegisterEnabled;
            _settingService.SaveSetting(externalAuthenticationSettings);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("CustomerUser", new { selectedTab = selectedTab });
        }






        public ActionResult GeneralCommon(string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

			var model = new GeneralCommonSettingsModel();
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

			StoreDependingSettings.CreateViewDataObject(storeScope);

            //store information
			var storeInformationSettings = _settingService.LoadSetting<StoreInformationSettings>(storeScope);
			model.StoreInformationSettings.StoreClosed = storeInformationSettings.StoreClosed;
			model.StoreInformationSettings.StoreClosedAllowForAdmins = storeInformationSettings.StoreClosedAllowForAdmins;

			StoreDependingSettings.GetOverrideKeys(storeInformationSettings, model.StoreInformationSettings, storeScope, _settingService, false);

			//seo settings
			var seoSettings = _settingService.LoadSetting<SeoSettings>(storeScope);
            model.SeoSettings.PageTitleSeoAdjustment = seoSettings.PageTitleSeoAdjustment;
			model.SeoSettings.PageTitleSeparator = seoSettings.PageTitleSeparator;
			model.SeoSettings.DefaultTitle = seoSettings.DefaultTitle;
			model.SeoSettings.DefaultMetaKeywords = seoSettings.DefaultMetaKeywords;
			model.SeoSettings.DefaultMetaDescription = seoSettings.DefaultMetaDescription;
			model.SeoSettings.ConvertNonWesternChars = seoSettings.ConvertNonWesternChars;
			model.SeoSettings.CanonicalUrlsEnabled = seoSettings.CanonicalUrlsEnabled;

			StoreDependingSettings.GetOverrideKeys(seoSettings, model.SeoSettings, storeScope, _settingService, false);

			//security settings
			var securitySettings = _settingService.LoadSetting<SecuritySettings>(storeScope);
			var captchaSettings = _settingService.LoadSetting<CaptchaSettings>(storeScope);
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
			model.SecuritySettings.CaptchaEnabled = captchaSettings.Enabled;
			model.SecuritySettings.CaptchaShowOnLoginPage = captchaSettings.ShowOnLoginPage;
			model.SecuritySettings.CaptchaShowOnRegistrationPage = captchaSettings.ShowOnRegistrationPage;
			model.SecuritySettings.CaptchaShowOnContactUsPage = captchaSettings.ShowOnContactUsPage;
			model.SecuritySettings.CaptchaShowOnEmailWishlistToFriendPage = captchaSettings.ShowOnEmailWishlistToFriendPage;
			model.SecuritySettings.CaptchaShowOnEmailProductToFriendPage = captchaSettings.ShowOnEmailProductToFriendPage;
			model.SecuritySettings.CaptchaShowOnAskQuestionPage = captchaSettings.ShowOnAskQuestionPage;
			model.SecuritySettings.CaptchaShowOnBlogCommentPage = captchaSettings.ShowOnBlogCommentPage;
			model.SecuritySettings.CaptchaShowOnNewsCommentPage = captchaSettings.ShowOnNewsCommentPage;
			model.SecuritySettings.CaptchaShowOnProductReviewPage = captchaSettings.ShowOnProductReviewPage;
			model.SecuritySettings.ReCaptchaPublicKey = captchaSettings.ReCaptchaPublicKey;
			model.SecuritySettings.ReCaptchaPrivateKey = captchaSettings.ReCaptchaPrivateKey;

			//PDF settings
			var pdfSettings = _settingService.LoadSetting<PdfSettings>(storeScope);
			model.PdfSettings.Enabled = pdfSettings.Enabled;
			model.PdfSettings.LetterPageSizeEnabled = pdfSettings.LetterPageSizeEnabled;
			model.PdfSettings.LogoPictureId = pdfSettings.LogoPictureId;

			StoreDependingSettings.GetOverrideKeys(pdfSettings, model.PdfSettings, storeScope, _settingService, false);

			//localization
			var localizationSettings = _settingService.LoadSetting<LocalizationSettings>(storeScope);
			model.LocalizationSettings.UseImagesForLanguageSelection = localizationSettings.UseImagesForLanguageSelection;
			model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled = localizationSettings.SeoFriendlyUrlsForLanguagesEnabled;
			model.LocalizationSettings.LoadAllLocaleRecordsOnStartup = localizationSettings.LoadAllLocaleRecordsOnStartup;
            model.LocalizationSettings.DefaultLanguageRedirectBehaviour = localizationSettings.DefaultLanguageRedirectBehaviour;
            model.LocalizationSettings.InvalidLanguageRedirectBehaviour = localizationSettings.InvalidLanguageRedirectBehaviour;
            model.LocalizationSettings.DetectBrowserUserLanguage = localizationSettings.DetectBrowserUserLanguage;

			//full-text support
			var commonSettings = _settingService.LoadSetting<CommonSettings>(storeScope);
			model.FullTextSettings.Supported = _fulltextService.IsFullTextSupported();
			model.FullTextSettings.Enabled = commonSettings.UseFullTextSearch;
			model.FullTextSettings.SearchMode = commonSettings.FullTextMode;
			model.FullTextSettings.SearchModeValues = commonSettings.FullTextMode.ToSelectList();

			//codehint: sm-add begin
			//company information
			var companySettings = _settingService.LoadSetting<CompanyInformationSettings>(storeScope);
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

			StoreDependingSettings.GetOverrideKeys(companySettings, model.CompanyInformationSettings, storeScope, _settingService, false);

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
			var contactDataSettings = _settingService.LoadSetting<ContactDataSettings>(storeScope);
			model.ContactDataSettings.CompanyTelephoneNumber = contactDataSettings.CompanyTelephoneNumber;
			model.ContactDataSettings.HotlineTelephoneNumber = contactDataSettings.HotlineTelephoneNumber;
			model.ContactDataSettings.MobileTelephoneNumber = contactDataSettings.MobileTelephoneNumber;
			model.ContactDataSettings.CompanyFaxNumber = contactDataSettings.CompanyFaxNumber;
			model.ContactDataSettings.CompanyEmailAddress = contactDataSettings.CompanyEmailAddress;
			model.ContactDataSettings.WebmasterEmailAddress = contactDataSettings.WebmasterEmailAddress;
			model.ContactDataSettings.SupportEmailAddress = contactDataSettings.SupportEmailAddress;
			model.ContactDataSettings.ContactEmailAddress = contactDataSettings.ContactEmailAddress;

			StoreDependingSettings.GetOverrideKeys(contactDataSettings, model.ContactDataSettings, storeScope, _settingService, false);

			//bank connection
			var bankConnectionSettings = _settingService.LoadSetting<BankConnectionSettings>(storeScope);
			model.BankConnectionSettings.Bankname = bankConnectionSettings.Bankname;
			model.BankConnectionSettings.Bankcode = bankConnectionSettings.Bankcode;
			model.BankConnectionSettings.AccountNumber = bankConnectionSettings.AccountNumber;
			model.BankConnectionSettings.AccountHolder = bankConnectionSettings.AccountHolder;
			model.BankConnectionSettings.Iban = bankConnectionSettings.Iban;
			model.BankConnectionSettings.Bic = bankConnectionSettings.Bic;

			StoreDependingSettings.GetOverrideKeys(bankConnectionSettings, model.BankConnectionSettings, storeScope, _settingService, false);

			//social
			var socialSettings = _settingService.LoadSetting<SocialSettings>(storeScope);
			model.SocialSettings.ShowSocialLinksInFooter = socialSettings.ShowSocialLinksInFooter;
			model.SocialSettings.FacebookLink = socialSettings.FacebookLink;
			model.SocialSettings.GooglePlusLink = socialSettings.GooglePlusLink;
			model.SocialSettings.TwitterLink = socialSettings.TwitterLink;
			model.SocialSettings.PinterestLink = socialSettings.PinterestLink;
            model.SocialSettings.YoutubeLink = socialSettings.YoutubeLink;

			StoreDependingSettings.GetOverrideKeys(socialSettings, model.SocialSettings, storeScope, _settingService, false);

			//codehint: sm-add end

            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }

        private SelectListItem ResToSelectListItem(string resourceKey)
        {
            string value = _localizationService.GetResource(resourceKey).EmptyNull();
            return new SelectListItem() { Text = value, Value = value };
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult GeneralCommon(GeneralCommonSettingsModel model, string selectedTab, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

			//store information
			var storeInformationSettings = _settingService.LoadSetting<StoreInformationSettings>(storeScope);
			storeInformationSettings.StoreClosed = model.StoreInformationSettings.StoreClosed;
			storeInformationSettings.StoreClosedAllowForAdmins = model.StoreInformationSettings.StoreClosedAllowForAdmins;

			StoreDependingSettings.UpdateSettings(storeInformationSettings, form, storeScope, _settingService);

			//seo settings
			var seoSettings = _settingService.LoadSetting<SeoSettings>(storeScope);
			seoSettings.PageTitleSeparator = model.SeoSettings.PageTitleSeparator;
			seoSettings.PageTitleSeoAdjustment = model.SeoSettings.PageTitleSeoAdjustment;
			seoSettings.DefaultTitle = model.SeoSettings.DefaultTitle;
			seoSettings.DefaultMetaKeywords = model.SeoSettings.DefaultMetaKeywords;
			seoSettings.DefaultMetaDescription = model.SeoSettings.DefaultMetaDescription;
			seoSettings.ConvertNonWesternChars = model.SeoSettings.ConvertNonWesternChars;
			seoSettings.CanonicalUrlsEnabled = model.SeoSettings.CanonicalUrlsEnabled;

			StoreDependingSettings.UpdateSettings(seoSettings, form, storeScope, _settingService);

			//security settings
			var securitySettings = _settingService.LoadSetting<SecuritySettings>(storeScope);
			var captchaSettings = _settingService.LoadSetting<CaptchaSettings>(storeScope);
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
			_settingService.SaveSetting(securitySettings);

			captchaSettings.Enabled = model.SecuritySettings.CaptchaEnabled;
			captchaSettings.ShowOnLoginPage = model.SecuritySettings.CaptchaShowOnLoginPage;
			captchaSettings.ShowOnRegistrationPage = model.SecuritySettings.CaptchaShowOnRegistrationPage;
			captchaSettings.ShowOnContactUsPage = model.SecuritySettings.CaptchaShowOnContactUsPage;
			captchaSettings.ShowOnEmailWishlistToFriendPage = model.SecuritySettings.CaptchaShowOnEmailWishlistToFriendPage;
			captchaSettings.ShowOnEmailProductToFriendPage = model.SecuritySettings.CaptchaShowOnEmailProductToFriendPage;
			captchaSettings.ShowOnAskQuestionPage = model.SecuritySettings.CaptchaShowOnAskQuestionPage;
			captchaSettings.ShowOnBlogCommentPage = model.SecuritySettings.CaptchaShowOnBlogCommentPage;
			captchaSettings.ShowOnNewsCommentPage = model.SecuritySettings.CaptchaShowOnNewsCommentPage;
			captchaSettings.ShowOnProductReviewPage = model.SecuritySettings.CaptchaShowOnProductReviewPage;
			captchaSettings.ReCaptchaPublicKey = model.SecuritySettings.ReCaptchaPublicKey;
			captchaSettings.ReCaptchaPrivateKey = model.SecuritySettings.ReCaptchaPrivateKey;

			_settingService.SaveSetting(captchaSettings);

			if (captchaSettings.Enabled && (String.IsNullOrWhiteSpace(captchaSettings.ReCaptchaPublicKey) || String.IsNullOrWhiteSpace(captchaSettings.ReCaptchaPrivateKey)))
			{
				NotifyError(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.CaptchaEnabledNoKeys"));
			}

			//PDF settings
			var pdfSettings = _settingService.LoadSetting<PdfSettings>(storeScope);
			pdfSettings.Enabled = model.PdfSettings.Enabled;
			pdfSettings.LetterPageSizeEnabled = model.PdfSettings.LetterPageSizeEnabled;
			pdfSettings.LogoPictureId = model.PdfSettings.LogoPictureId;

			StoreDependingSettings.UpdateSettings(pdfSettings, form, storeScope, _settingService);

			//localization settings
			var localizationSettings = _settingService.LoadSetting<LocalizationSettings>(storeScope);
			localizationSettings.UseImagesForLanguageSelection = model.LocalizationSettings.UseImagesForLanguageSelection;
			if (localizationSettings.SeoFriendlyUrlsForLanguagesEnabled != model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
			{
				localizationSettings.SeoFriendlyUrlsForLanguagesEnabled = model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled;
				//clear cached values of routes
				System.Web.Routing.RouteTable.Routes.ClearSeoFriendlyUrlsCachedValueForRoutes();
			}
			localizationSettings.LoadAllLocaleRecordsOnStartup = model.LocalizationSettings.LoadAllLocaleRecordsOnStartup;
            localizationSettings.DefaultLanguageRedirectBehaviour = model.LocalizationSettings.DefaultLanguageRedirectBehaviour;
            localizationSettings.InvalidLanguageRedirectBehaviour = model.LocalizationSettings.InvalidLanguageRedirectBehaviour;
            localizationSettings.DetectBrowserUserLanguage = model.LocalizationSettings.DetectBrowserUserLanguage;

			_settingService.SaveSetting(localizationSettings);

			//full-text
			var commonSettings = _settingService.LoadSetting<CommonSettings>(storeScope);
			commonSettings.FullTextMode = model.FullTextSettings.SearchMode;

			_settingService.SaveSetting(commonSettings);

			//codehint: sm-add begin
			//company information
			var companySettings = _settingService.LoadSetting<CompanyInformationSettings>(storeScope);
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

			StoreDependingSettings.UpdateSettings(companySettings, form, storeScope, _settingService);

			//contact data
			var contactDataSettings = _settingService.LoadSetting<ContactDataSettings>(storeScope);
			contactDataSettings.CompanyTelephoneNumber = model.ContactDataSettings.CompanyTelephoneNumber;
			contactDataSettings.HotlineTelephoneNumber = model.ContactDataSettings.HotlineTelephoneNumber;
			contactDataSettings.MobileTelephoneNumber = model.ContactDataSettings.MobileTelephoneNumber;
			contactDataSettings.CompanyFaxNumber = model.ContactDataSettings.CompanyFaxNumber;
			contactDataSettings.CompanyEmailAddress = model.ContactDataSettings.CompanyEmailAddress;
			contactDataSettings.WebmasterEmailAddress = model.ContactDataSettings.WebmasterEmailAddress;
			contactDataSettings.SupportEmailAddress = model.ContactDataSettings.SupportEmailAddress;
			contactDataSettings.ContactEmailAddress = model.ContactDataSettings.ContactEmailAddress;

			StoreDependingSettings.UpdateSettings(contactDataSettings, form, storeScope, _settingService);

			//bank connection
			var bankConnectionSettings = _settingService.LoadSetting<BankConnectionSettings>(storeScope);
			bankConnectionSettings.Bankname = model.BankConnectionSettings.Bankname;
			bankConnectionSettings.Bankcode = model.BankConnectionSettings.Bankcode;
			bankConnectionSettings.AccountNumber = model.BankConnectionSettings.AccountNumber;
			bankConnectionSettings.AccountHolder = model.BankConnectionSettings.AccountHolder;
			bankConnectionSettings.Iban = model.BankConnectionSettings.Iban;
			bankConnectionSettings.Bic = model.BankConnectionSettings.Bic;

			StoreDependingSettings.UpdateSettings(bankConnectionSettings, form, storeScope, _settingService);

			//social
			var socialSettings = _settingService.LoadSetting<SocialSettings>(storeScope);
			socialSettings.ShowSocialLinksInFooter = model.SocialSettings.ShowSocialLinksInFooter;
			socialSettings.FacebookLink = model.SocialSettings.FacebookLink;
			socialSettings.GooglePlusLink = model.SocialSettings.GooglePlusLink;
			socialSettings.TwitterLink = model.SocialSettings.TwitterLink;
			socialSettings.PinterestLink = model.SocialSettings.PinterestLink;
            socialSettings.YoutubeLink = model.SocialSettings.YoutubeLink;

			StoreDependingSettings.UpdateSettings(socialSettings, form, storeScope, _settingService);

			//codehint: sm-add end

			//now clear settings cache
			_settingService.ClearCache();

			//activity log
			_customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("GeneralCommon", new { selectedTab = selectedTab });
        }
        [HttpPost, ActionName("GeneralCommon")]
        [FormValueRequired("changeencryptionkey")]
        public ActionResult ChangeEnryptionKey(GeneralCommonSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var securitySettings = _settingService.LoadSetting<SecuritySettings>(storeScope);

            try
            {
                if (model.SecuritySettings.EncryptionKey == null)
                    model.SecuritySettings.EncryptionKey = "";

                model.SecuritySettings.EncryptionKey = model.SecuritySettings.EncryptionKey.Trim();

                var newEncryptionPrivateKey = model.SecuritySettings.EncryptionKey;
                if (String.IsNullOrEmpty(newEncryptionPrivateKey) || newEncryptionPrivateKey.Length != 16)
                    throw new SmartException(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TooShort"));

                string oldEncryptionPrivateKey = securitySettings.EncryptionKey;
                if (oldEncryptionPrivateKey == newEncryptionPrivateKey)
                    throw new SmartException(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TheSame"));

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
                _settingService.SaveSetting(securitySettings);
                NotifySuccess(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.Changed"));
            }
            catch (Exception exc)
            {
                NotifyError(exc);
            }
			return RedirectToAction("GeneralCommon", new { selectedTab = "generalsettings-edit-3" });
        }
        [HttpPost, ActionName("GeneralCommon")]
        [FormValueRequired("togglefulltext")]
        public ActionResult ToggleFullText(GeneralCommonSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var commonSettings = _settingService.LoadSetting<CommonSettings>(storeScope);

            try
            {
                if (! _fulltextService.IsFullTextSupported())
                    throw new SmartException(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.FullTextSettings.NotSupported"));

                if (commonSettings.UseFullTextSearch)
                {
                    _fulltextService.DisableFullText();

                    commonSettings.UseFullTextSearch = false;
                    _settingService.SaveSetting(commonSettings, storeScope);

                    NotifySuccess(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Disabled"));
                }
                else
                {
                    _fulltextService.EnableFullText();

                    commonSettings.UseFullTextSearch = true;
                    _settingService.SaveSetting(commonSettings, storeScope);

                    NotifySuccess(_localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.FullTextSettings.Enabled"));
                }
            }
            catch (Exception exc)
            {
                NotifyError(exc);
            }
			return RedirectToAction("GeneralCommon", new { selectedTab = "generalsettings-edit-9" });
        }




        //all settings
        public ActionResult AllSettings()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
            
            return View();
        }
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult AllSettings(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
            
            var settings = _settingService
                .GetAllSettings()
				.Select(x =>
				{
					string storeName = "";
					if (x.StoreId == 0)
					{
						storeName = _localizationService.GetResource("Admin.Common.StoresAll");
					}
					else
					{
						var store = _storeService.GetStoreById(x.StoreId);
						storeName = store != null ? store.Name : "Unknown";
					}
					var settingModel = new SettingModel()
					{
						Id = x.Id,
						Name = x.Name,
						Value = x.Value,
						Store = storeName,
						StoreId = x.StoreId
					};
					return settingModel;
				})
                .ForCommand(command)
                .ToList();
            
            var model = new GridModel<SettingModel>
            {
                Data = settings.PagedForCommand(command),
                Total = settings.Count
            };
            return new JsonResult
            {
                Data = model
            };
        }
        [GridAction(EnableCustomBinding = true)]
        public ActionResult SettingUpdate(SettingModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
            {
                //display the first model error
                var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            var setting = _settingService.GetSettingById(model.Id);
			if (setting == null)
				return Content(_localizationService.GetResource("Admin.Configuration.Settings.NoneWithThatId"));

			var storeId = model.Store.ToInt(); //use Store property (not StoreId) because appropriate property is stored in it

			if (!setting.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase) ||
				setting.StoreId != storeId)
			{
				//setting name or store has been changed
				_settingService.DeleteSetting(setting);
			}

			_settingService.SetSetting(model.Name, model.Value, storeId);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            return AllSettings(command);
        }
        [GridAction(EnableCustomBinding = true)]
        public ActionResult SettingAdd([Bind(Exclude = "Id")] SettingModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
            {
                //display the first model error
                var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

			var storeId = model.Store.ToInt(); //use Store property (not StoreId) because appropriate property is stored in it
			_settingService.SetSetting(model.Name, model.Value, storeId);

            //activity log
            _customerActivityService.InsertActivity("AddNewSetting", _localizationService.GetResource("ActivityLog.AddNewSetting"), model.Name);

            return AllSettings(command);
        }
        [GridAction(EnableCustomBinding = true)]
        public ActionResult SettingDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var setting = _settingService.GetSettingById(id);
            if (setting == null)
                throw new ArgumentException("No setting found with the specified id");
            _settingService.DeleteSetting(setting);

            //activity log
            _customerActivityService.InsertActivity("DeleteSetting", _localizationService.GetResource("ActivityLog.DeleteSetting"), setting.Name);

            return AllSettings(command);
        }

        #endregion
    }
}
