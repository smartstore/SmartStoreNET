﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class CurrencyController :  AdminControllerBase
    {
        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly ISettingService _settingService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
		private readonly IStoreService _storeService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly PluginMediator _pluginMediator;

        #endregion

        #region Constructors

        public CurrencyController(ICurrencyService currencyService, 
            CurrencySettings currencySettings, ISettingService settingService,
            IDateTimeHelper dateTimeHelper, ILocalizationService localizationService,
            IPermissionService permissionService,
            ILocalizedEntityService localizedEntityService, ILanguageService languageService,
            IStoreService storeService, 
            IStoreMappingService storeMappingService,
			PluginMediator pluginMediator)
        {
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._settingService = settingService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._localizedEntityService = localizedEntityService;
            this._languageService = languageService;
			this._storeService = storeService;
			this._storeMappingService = storeMappingService;
			this._pluginMediator = pluginMediator;
        }
        
        #endregion

        #region Utilities

        [NonAction]
        public void UpdateLocales(Currency currency, CurrencyModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(currency,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
            }
        }

		[NonAction]
		private void PrepareStoresMappingModel(CurrencyModel model, Currency currency, bool excludeProperties)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			model.AvailableStores = _storeService
				.GetAllStores()
				.Select(s => s.ToModel())
				.ToList();
			if (!excludeProperties)
			{
				if (currency != null)
				{
					model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(currency);
				}
				else
				{
					model.SelectedStoreIds = new int[0];
				}
			}
		}

        #endregion

        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List(bool liveRates = false)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currenciesModel = _currencyService.GetAllCurrencies(true).Select(x => x.ToModel()).ToList();
            foreach (var currency in currenciesModel)
                currency.IsPrimaryExchangeRateCurrency = currency.Id == _currencySettings.PrimaryExchangeRateCurrencyId ? true : false;
            foreach (var currency in currenciesModel)
                currency.IsPrimaryStoreCurrency = currency.Id == _currencySettings.PrimaryStoreCurrencyId ? true : false;
            if (liveRates)
            {
                try
                {
                    var primaryExchangeCurrency = _currencyService.GetCurrencyById(_currencySettings.PrimaryExchangeRateCurrencyId);
                    if (primaryExchangeCurrency == null)
                        throw new SmartException("Primary exchange rate currency is not set");

                    ViewBag.Rates = _currencyService.GetCurrencyLiveRates(primaryExchangeCurrency.CurrencyCode);
                }
                catch (Exception exc)
                {
                    NotifyError(exc, false);
                }
            }
            ViewBag.ExchangeRateProviders = new List<SelectListItem>();
            foreach (var erp in _currencyService.LoadAllExchangeRateProviders())
            {
                ViewBag.ExchangeRateProviders.Add(new SelectListItem()
                {
                    Text = _pluginMediator.GetLocalizedFriendlyName(erp.Metadata),
					Value = erp.Metadata.SystemName,
					Selected = erp.Metadata.SystemName.Equals(_currencySettings.ActiveExchangeRateProviderSystemName, StringComparison.InvariantCultureIgnoreCase)
                });
            }
            ViewBag.AutoUpdateEnabled = _currencySettings.AutoUpdateEnabled;
            var gridModel = new GridModel<CurrencyModel>
            {
                Data = currenciesModel,
                Total = currenciesModel.Count()
            };
            return View(gridModel);
        }

        public ActionResult ApplyRate(string currencyCode, decimal rate)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            Currency currency = _currencyService.GetCurrencyByCode(currencyCode);
            if (currency != null)
            {
                currency.Rate = rate;
                currency.UpdatedOnUtc = DateTime.UtcNow;
                _currencyService.UpdateCurrency(currency);
            }
            return RedirectToAction("List","Currency", new { liveRates=true });
        }

        [HttpPost]
        public ActionResult Save(FormCollection formValues)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            _currencySettings.ActiveExchangeRateProviderSystemName = formValues["exchangeRateProvider"];
            _currencySettings.AutoUpdateEnabled = formValues["autoUpdateEnabled"].Equals("false") ? false : true;
            _settingService.SaveSetting(_currencySettings);
            return RedirectToAction("List", "Currency");
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currencies = _currencyService.GetAllCurrencies(true);
            var gridModel = new GridModel<CurrencyModel>
            {
                Data = currencies.Select(x => x.ToModel()),
                Total = currencies.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }
        
        public ActionResult MarkAsPrimaryExchangeRateCurrency(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            _currencySettings.PrimaryExchangeRateCurrencyId = id;
            _settingService.SaveSetting(_currencySettings);
            return RedirectToAction("List");
        }

        public ActionResult MarkAsPrimaryStoreCurrency(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            _currencySettings.PrimaryStoreCurrencyId = id;
            _settingService.SaveSetting(_currencySettings);
            return RedirectToAction("List");
        }

        #endregion

        #region Create / Edit / Delete

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var model = new CurrencyModel();
            //locales
            AddLocales(_languageService, model.Locales);
			//Stores
			PrepareStoresMappingModel(model, null, false);
            //default values
            model.Published = true;
            model.Rate = 1;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Create(CurrencyModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var currency = model.ToEntity();
                currency.CreatedOnUtc = DateTime.UtcNow;
                currency.UpdatedOnUtc = DateTime.UtcNow;
                
				_currencyService.InsertCurrency(currency);
                
				//locales
                UpdateLocales(currency, model);
				
				//Stores
				_storeMappingService.SaveStoreMappings<Currency>(currency, model.SelectedStoreIds);

                NotifySuccess(_localizationService.GetResource("Admin.Configuration.Currencies.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = currency.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

			//Stores
			PrepareStoresMappingModel(model, null, true);

            return View(model);
        }
        
        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currency = _currencyService.GetCurrencyById(id);
            if (currency == null)
                return RedirectToAction("List");

            var model = currency.ToModel();
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(currency.CreatedOnUtc, DateTimeKind.Utc);
            
			//locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = currency.GetLocalized(x => x.Name, languageId, false, false);
            });

			foreach (var ending in model.DomainEndings.SplitSafe(","))
			{
				var item = model.AvailableDomainEndings.FirstOrDefault(x => x.Value.IsCaseInsensitiveEqual(ending));
				if (item == null)
					model.AvailableDomainEndings.Add(new SelectListItem() { Text = ending, Value = ending, Selected = true });
				else
					item.Selected = true;
			}

			//Stores
			PrepareStoresMappingModel(model, currency, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Edit(CurrencyModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currency = _currencyService.GetCurrencyById(model.Id);
            if (currency == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                currency = model.ToEntity(currency);
                currency.UpdatedOnUtc = DateTime.UtcNow;
    
				_currencyService.UpdateCurrency(currency);
                
				//locales
                UpdateLocales(currency, model);
				
				//Stores
				_storeMappingService.SaveStoreMappings<Currency>(currency, model.SelectedStoreIds);

                NotifySuccess(_localizationService.GetResource("Admin.Configuration.Currencies.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = currency.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(currency.CreatedOnUtc, DateTimeKind.Utc);

			//Stores
			PrepareStoresMappingModel(model, currency, true);

            return View(model);
        }
        
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currency = _currencyService.GetCurrencyById(id);
            if (currency == null)
                return RedirectToAction("List");
            
            try
            {
                if (currency.Id == _currencySettings.PrimaryStoreCurrencyId)
                    throw new SmartException(_localizationService.GetResource("Admin.Configuration.Currencies.CantDeletePrimary"));

                if (currency.Id == _currencySettings.PrimaryExchangeRateCurrencyId)
                    throw new SmartException(_localizationService.GetResource("Admin.Configuration.Currencies.CantDeleteExchange"));

                _currencyService.DeleteCurrency(currency);

                NotifySuccess(_localizationService.GetResource("Admin.Configuration.Currencies.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                NotifyError(exc);
                return RedirectToAction("Edit", new { id = currency.Id });
            }
        }

        #endregion
    }
}
