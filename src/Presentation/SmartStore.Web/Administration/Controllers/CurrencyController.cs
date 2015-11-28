using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services;
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
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly PluginMediator _pluginMediator;
		private readonly ICommonServices _services;

        #endregion

        #region Constructors

        public CurrencyController(ICurrencyService currencyService, 
            CurrencySettings currencySettings,
            IDateTimeHelper dateTimeHelper,
            ILocalizedEntityService localizedEntityService,
			ILanguageService languageService,
            IStoreMappingService storeMappingService,
			PluginMediator pluginMediator,
			ICommonServices services)
        {
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._dateTimeHelper = dateTimeHelper;
            this._localizedEntityService = localizedEntityService;
            this._languageService = languageService;
			this._storeMappingService = storeMappingService;
			this._pluginMediator = pluginMediator;
			this._services = services;
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

		private void PrepareCurrencyModel(CurrencyModel model, Currency currency, bool excludeProperties)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			var allStores = _services.StoreService.GetAllStores();

			model.AvailableStores = allStores.Select(s => s.ToModel()).ToList();

			if (currency != null)
			{
				model.PrimaryStoreCurrencyStores = allStores
					.Where(x => x.PrimaryStoreCurrencyId == currency.Id)
					.Select(x => new SelectListItem
					{
						Text = x.Name,
						Value = Url.Action("Edit", "Store", new { id = x.Id })
					})
					.ToList();

				model.PrimaryExchangeRateCurrencyStores = allStores
					.Where(x => x.PrimaryExchangeRateCurrencyId == currency.Id)
					.Select(x => new SelectListItem
					{
						Text = x.Name,
						Value = Url.Action("Edit", "Store", new { id = x.Id })
					})
					.ToList();
			}			

			if (!excludeProperties)
			{
				model.SelectedStoreIds = (currency == null ? new int[0] : _storeMappingService.GetStoresIdsWithAccess(currency));
			}
		}

		private bool IsAttachedToStore(Currency currency, IList<Store> stores, bool force)
		{
			var attachedStore = stores.FirstOrDefault(x => x.PrimaryStoreCurrencyId == currency.Id || x.PrimaryExchangeRateCurrencyId == currency.Id);

			if (attachedStore != null)
			{
				if (force || (!force && !currency.Published))
				{
					NotifyError(T("Admin.Configuration.Currencies.DeleteOrPublishStoreConflict", attachedStore.Name));
					return true;
				}

				// Must store limitations include the store where the currency is attached as primary or exchange rate currency?
				//if (currency.LimitedToStores)
				//{
				//	if (selectedStoreIds == null)
				//		selectedStoreIds = _storeMappingService.GetStoreMappingsFor("Currency", currency.Id).Select(x => x.StoreId).ToArray();

				//	if (!selectedStoreIds.Contains(attachedStore.Id))
				//	{
				//		NotifyError(T("Admin.Configuration.Currencies.StoreLimitationConflict", attachedStore.Name));
				//		return true;
				//	}
				//}
			}
			return false;
		}

        #endregion

        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List(bool liveRates = false)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

			var store = _services.StoreContext.CurrentStore;
            var models = _currencyService.GetAllCurrencies(true).Select(x => x.ToModel()).ToList();

			foreach (var model in models)
			{
				model.IsPrimaryStoreCurrency = (store.PrimaryStoreCurrencyId == model.Id);
				model.IsPrimaryExchangeRateCurrency = (store.PrimaryExchangeRateCurrencyId == model.Id);
			}

			//var allStores = _services.StoreService.GetAllStores();
			//foreach (var model in models)
			//{
			//	var storeNames = allStores.Where(x => x.PrimaryStoreCurrencyId == model.Id).Select(x => x.Name).ToList();

			//	model.IsPrimaryStoreCurrency = string.Join("<br />", storeNames);

			//	storeNames = allStores.Where(x => x.PrimaryExchangeRateCurrencyId == model.Id).Select(x => x.Name).ToList();

			//	model.IsPrimaryExchangeRateCurrency = string.Join("<br />", storeNames);
			//}

            if (liveRates)
            {
                try
                {
					var primaryExchangeCurrency = _services.StoreContext.CurrentStore.PrimaryExchangeRateCurrency;
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
                ViewBag.ExchangeRateProviders.Add(new SelectListItem
                {
                    Text = _pluginMediator.GetLocalizedFriendlyName(erp.Metadata),
					Value = erp.Metadata.SystemName,
					Selected = erp.Metadata.SystemName.Equals(_currencySettings.ActiveExchangeRateProviderSystemName, StringComparison.InvariantCultureIgnoreCase)
                });
            }

            ViewBag.AutoUpdateEnabled = _currencySettings.AutoUpdateEnabled;

            var gridModel = new GridModel<CurrencyModel>
            {
                Data = models,
                Total = models.Count()
            };
            return View(gridModel);
        }

        public ActionResult ApplyRate(string currencyCode, decimal rate)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
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
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            _currencySettings.ActiveExchangeRateProviderSystemName = formValues["exchangeRateProvider"];
            _currencySettings.AutoUpdateEnabled = formValues["autoUpdateEnabled"].Equals("false") ? false : true;
            _services.Settings.SaveSetting(_currencySettings);
            return RedirectToAction("List", "Currency");
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
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

        #endregion

        #region Create / Edit / Delete

        public ActionResult Create()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var model = new CurrencyModel();
            //locales
            AddLocales(_languageService, model.Locales);
			//Stores
			PrepareCurrencyModel(model, null, false);
            //default values
            model.Published = true;
            model.Rate = 1;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Create(CurrencyModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
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

                NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Currencies.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = currency.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

			//Stores
			PrepareCurrencyModel(model, null, true);

            return View(model);
        }
        
        public ActionResult Edit(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
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
			PrepareCurrencyModel(model, currency, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Edit(CurrencyModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currency = _currencyService.GetCurrencyById(model.Id);
            if (currency == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                currency = model.ToEntity(currency);

				if (!IsAttachedToStore(currency, _services.StoreService.GetAllStores(), false))
				{
					currency.UpdatedOnUtc = DateTime.UtcNow;
    
					_currencyService.UpdateCurrency(currency);
                
					//locales
					UpdateLocales(currency, model);
				
					//Stores
					_storeMappingService.SaveStoreMappings<Currency>(currency, model.SelectedStoreIds);

					NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Currencies.Updated"));
					return continueEditing ? RedirectToAction("Edit", new { id = currency.Id }) : RedirectToAction("List");
				}
            }

            //If we got this far, something failed, redisplay form
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(currency.CreatedOnUtc, DateTimeKind.Utc);

			//Stores
			PrepareCurrencyModel(model, currency, true);

            return View(model);
        }
        
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currency = _currencyService.GetCurrencyById(id);
            if (currency == null)
                return RedirectToAction("List");
            
            try
            {
				if (!IsAttachedToStore(currency, _services.StoreService.GetAllStores(), true))
				{
					_currencyService.DeleteCurrency(currency);

					NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Currencies.Deleted"));
					return RedirectToAction("List");
				}
            }
            catch (Exception exc)
            {
                NotifyError(exc);
            }

			return RedirectToAction("Edit", new { id = currency.Id });
        }

        #endregion
    }
}
