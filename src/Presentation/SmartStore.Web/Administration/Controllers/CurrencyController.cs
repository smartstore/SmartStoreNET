using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;
using SmartStore.Web.Framework;

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
        private readonly IPaymentService _paymentService;

        #endregion

        #region Constructors

        public CurrencyController(
            ICurrencyService currencyService, 
            CurrencySettings currencySettings,
            IDateTimeHelper dateTimeHelper,
            ILocalizedEntityService localizedEntityService,
			ILanguageService languageService,
            IStoreMappingService storeMappingService,
			PluginMediator pluginMediator,
			ICommonServices services,
            IPaymentService paymentService)
        {
            _currencyService = currencyService;
            _currencySettings = currencySettings;
            _dateTimeHelper = dateTimeHelper;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
			_storeMappingService = storeMappingService;
			_pluginMediator = pluginMediator;
			_services = services;
            _paymentService = paymentService;
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
			Guard.NotNull(model, nameof(model));

			var allStores = _services.StoreService.GetAllStores();
            var paymentMethods = _paymentService.GetAllPaymentMethods();
            var paymentProviders = _paymentService.LoadAllPaymentMethods().ToDictionarySafe(x => x.Metadata.SystemName);

			foreach (var paymentMethod in paymentMethods.Where(x => x.RoundOrderTotalEnabled))
            {
                string friendlyName = null;
                Provider<IPaymentMethod> provider;
                if (paymentProviders.TryGetValue(paymentMethod.PaymentMethodSystemName, out provider))
                {
                    friendlyName = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata);
                }

                model.RoundOrderTotalPaymentMethods[paymentMethod.PaymentMethodSystemName] = friendlyName ?? paymentMethod.PaymentMethodSystemName;
            }

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
				model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(currency);
			}

			model.AvailableStores = allStores.ToSelectListItems(model.SelectedStoreIds);
		}

		private CurrencyModel CreateCurrencyListModel(Currency currency)
		{
			var store = _services.StoreContext.CurrentStore;
			var model = currency.ToModel();

			model.IsPrimaryStoreCurrency = store.PrimaryStoreCurrencyId == model.Id;
			model.IsPrimaryExchangeRateCurrency = store.PrimaryExchangeRateCurrencyId == model.Id;

			return model;
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

			var language = _services.WorkContext.WorkingLanguage;
			var allCurrencies = _currencyService.GetAllCurrencies(true)
				.ToDictionarySafe(x => x.CurrencyCode.EmptyNull().ToUpper(), x => x);

			var models = allCurrencies.Select(x => CreateCurrencyListModel(x.Value)).ToList();

			if (liveRates)
            {
                try
                {
					var primaryExchangeCurrency = _services.StoreContext.CurrentStore.PrimaryExchangeRateCurrency;
                    if (primaryExchangeCurrency == null)
                        throw new SmartException(T("Admin.System.Warnings.ExchangeCurrency.NotSet"));

					var rates = _currencyService.GetCurrencyLiveRates(primaryExchangeCurrency.CurrencyCode);

					// get localized name of currencies
					var currencyNames = allCurrencies.ToDictionarySafe(
						x => x.Key,
						x => x.Value.GetLocalized(y => y.Name, language, true, false).Value
					);

					// fallback to english name where no localized currency name exists
					foreach (var info in CultureInfo.GetCultures(CultureTypes.AllCultures).Where(x => !x.IsNeutralCulture))
					{
						try
						{
							var region = new RegionInfo(info.LCID);

							if (!currencyNames.ContainsKey(region.ISOCurrencySymbol))
								currencyNames.Add(region.ISOCurrencySymbol, region.CurrencyEnglishName);
						}
						catch { }
					}

					// provide rate with currency name and whether it is available in store
					rates.Each(x =>
					{
						x.IsStoreCurrency = allCurrencies.ContainsKey(x.CurrencyCode);

						if (x.Name.IsEmpty() && currencyNames.ContainsKey(x.CurrencyCode))
							x.Name = currencyNames[x.CurrencyCode];
					});

					ViewBag.Rates = rates;
                }
                catch (Exception exception)
                {
                    NotifyError(exception, false);
                }
            }

			ViewBag.AutoUpdateEnabled = _currencySettings.AutoUpdateEnabled;
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

            return View(new GridModel<CurrencyModel>
			{
				Data = models,
				Total = models.Count()
			});
        }

        public ActionResult ApplyRate(string currencyCode, decimal rate)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var currency = _currencyService.GetCurrencyByCode(currencyCode);
            if (currency != null)
            {
                currency.Rate = rate;

                _currencyService.UpdateCurrency(currency);

				NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
            }

            return RedirectToAction("List", "Currency", new { liveRates = true });
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
			var model = new GridModel<CurrencyModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
			{
				var currencies = _currencyService.GetAllCurrencies(true);

				model.Data = currencies.Select(x => CreateCurrencyListModel(x));
				model.Total = currencies.Count();
			}
			else
			{
				model.Data = Enumerable.Empty<CurrencyModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

        #endregion

        #region Create / Edit / Delete

        public ActionResult Create()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            var model = new CurrencyModel();
            AddLocales(_languageService, model.Locales);
			PrepareCurrencyModel(model, null, false);

            // Default values
            model.Published = true;
            model.Rate = 1;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CurrencyModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCurrencies))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var currency = model.ToEntity();
                
				_currencyService.InsertCurrency(currency);
                
                UpdateLocales(currency, model);
				
				_storeMappingService.SaveStoreMappings(currency, model.SelectedStoreIds);

                NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Currencies.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = currency.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form
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
            
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = currency.GetLocalized(x => x.Name, languageId, false, false);
            });

			foreach (var ending in model.DomainEndings.SplitSafe(","))
			{
				var item = model.AvailableDomainEndings.FirstOrDefault(x => x.Value.IsCaseInsensitiveEqual(ending));
                if (item == null)
                {
                    model.AvailableDomainEndings.Add(new SelectListItem { Text = ending, Value = ending, Selected = true });
                }
                else
                {
                    item.Selected = true;
                }
			}

			PrepareCurrencyModel(model, currency, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
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
					_currencyService.UpdateCurrency(currency);
                
					UpdateLocales(currency, model);
				
					_storeMappingService.SaveStoreMappings<Currency>(currency, model.SelectedStoreIds);

					NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Currencies.Updated"));
					return continueEditing ? RedirectToAction("Edit", new { id = currency.Id }) : RedirectToAction("List");
				}
            }

            // If we got this far, something failed, redisplay form
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(currency.CreatedOnUtc, DateTimeKind.Utc);

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
