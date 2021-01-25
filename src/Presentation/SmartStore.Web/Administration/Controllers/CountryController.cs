using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Security;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class CountryController : AdminControllerBase
    {
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICurrencyService _currencyService;

        public CountryController(
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            ILocalizedEntityService localizedEntityService,
            ILanguageService languageService,
            IStoreMappingService storeMappingService,
            ICurrencyService currencyService)
        {
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressService = addressService;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
            _storeMappingService = storeMappingService;
            _currencyService = currencyService;
        }

        #region Utilities 

        [NonAction]
        private void UpdateLocales(Country country, CountryModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(country, x => x.Name, localized.Name, localized.LanguageId);
            }
        }

        [NonAction]
        private void UpdateLocales(StateProvince stateProvince, StateProvinceModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(stateProvince, x => x.Name, localized.Name, localized.LanguageId);
            }
        }

        [NonAction]
        private void PrepareCountryModel(CountryModel model, Country country)
        {
            Guard.NotNull(model, nameof(model));

            model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(country);

            model.AllCurrencies = _currencyService.GetAllCurrencies(true)
                .Select(x => new SelectListItem { Text = x.GetLocalized(y => y.Name), Value = x.Id.ToString() })
                .ToList();
        }

        #endregion

        #region Countries

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Configuration.Country.Read)]
        public ActionResult List()
        {
            var allStores = Services.StoreService.GetAllStores();

            var model = new CountryListModel
            {
                StoreCount = allStores.Count
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Country.Read)]
        public ActionResult CountryList(GridCommand command)
        {
            var model = new GridModel<CountryModel>();

            var countries = _countryService.GetAllCountries(true);

            model.Data = countries.Select(x => x.ToModel());
            model.Total = countries.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Country.Update)]
        public ActionResult CountryUpdate(CountryModel model, GridCommand command)
        {
            if (!ModelState.IsValid)
            {
                var modelStateErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            var country = _countryService.GetCountryById(model.Id);

            country = model.ToEntity(country);
            _countryService.UpdateCountry(country);

            return CountryList(command);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Country.Delete)]
        public ActionResult CountryDelete(int id, GridCommand command)
        {
            if (_addressService.GetAddressTotalByCountryId(id) > 0)
            {
                return Content(T("Admin.Configuration.Countries.CannotDeleteDueToAssociatedAddresses"));
            }

            var country = _countryService.GetCountryById(id);

            _countryService.DeleteCountry(country);

            return CountryList(command);
        }

        [Permission(Permissions.Configuration.Country.Create)]
        public ActionResult Create()
        {
            var model = new CountryModel();

            AddLocales(_languageService, model.Locales);
            PrepareCountryModel(model, null);

            // Default values.
            model.Published = true;
            model.AllowsBilling = true;
            model.AllowsShipping = true;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Country.Create)]
        public ActionResult Create(CountryModel model, bool continueEditing, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var country = model.ToEntity();
                _countryService.InsertCountry(country);

                UpdateLocales(country, model);
                SaveStoreMappings(country, model.SelectedStoreIds);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, country, form));

                NotifySuccess(T("Admin.Configuration.Countries.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = country.Id }) : RedirectToAction("List");
            }

            PrepareCountryModel(model, null);

            return View(model);
        }

        [Permission(Permissions.Configuration.Country.Read)]
        public ActionResult Edit(int id)
        {
            var country = _countryService.GetCountryById(id);
            if (country == null)
                return RedirectToAction("List");

            var model = country.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = country.GetLocalized(x => x.Name, languageId, false, false);
            });

            PrepareCountryModel(model, country);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Country.Update)]
        public ActionResult Edit(CountryModel model, bool continueEditing, FormCollection form)
        {
            var country = _countryService.GetCountryById(model.Id);
            if (country == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                country = model.ToEntity(country);
                _countryService.UpdateCountry(country);

                UpdateLocales(country, model);
                SaveStoreMappings(country, model.SelectedStoreIds);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, country, form));

                NotifySuccess(T("Admin.Configuration.Countries.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = country.Id }) : RedirectToAction("List");
            }

            PrepareCountryModel(model, country);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Country.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var country = _countryService.GetCountryById(id);
            if (country == null)
                return RedirectToAction("List");

            try
            {
                if (_addressService.GetAddressTotalByCountryId(country.Id) > 0)
                    throw new SmartException(T("Admin.Configuration.Countries.CannotDeleteDueToAssociatedAddresses"));

                _countryService.DeleteCountry(country);

                NotifySuccess(T("Admin.Configuration.Countries.Deleted"));

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                NotifyError(exc);
                return RedirectToAction("Edit", new { id = country.Id });
            }
        }

        #endregion

        #region States / provinces

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Country.Read)]
        public ActionResult States(int countryId, GridCommand command)
        {
            var model = new GridModel<StateProvinceModel>();

            var states = _stateProvinceService.GetStateProvincesByCountryId(countryId, true)
                .Select(x => x.ToModel());

            model.Data = states;
            model.Total = states.Count();

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Configuration.Country.Update)]
        public ActionResult StateCreatePopup(int countryId)
        {
            var model = new StateProvinceModel
            {
                CountryId = countryId
            };

            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Country.Update)]
        public ActionResult StateCreatePopup(string btnId, string formId, StateProvinceModel model)
        {
            var country = _countryService.GetCountryById(model.CountryId);
            if (country == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var sp = model.ToEntity();

                _stateProvinceService.InsertStateProvince(sp);
                UpdateLocales(sp, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [Permission(Permissions.Configuration.Country.Read)]
        public ActionResult StateEditPopup(int id)
        {
            var sp = _stateProvinceService.GetStateProvinceById(id);
            if (sp == null)
                return RedirectToAction("List");

            var model = sp.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = sp.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Country.Update)]
        public ActionResult StateEditPopup(string btnId, string formId, StateProvinceModel model)
        {
            var sp = _stateProvinceService.GetStateProvinceById(model.Id);
            if (sp == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                sp = model.ToEntity(sp);
                _stateProvinceService.UpdateStateProvince(sp);

                UpdateLocales(sp, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Country.Update)]
        public ActionResult StateDelete(int id, GridCommand command)
        {
            var state = _stateProvinceService.GetStateProvinceById(id);
            var countryId = state.CountryId;

            if (_addressService.GetAddressTotalByStateProvinceId(state.Id) > 0)
            {
                return Content(T("Admin.Configuration.Countries.States.CantDeleteWithAddresses"));
            }

            _stateProvinceService.DeleteStateProvince(state);

            return States(countryId, command);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStatesByCountryId(string countryId, bool? addEmptyStateIfRequired, bool? addAsterisk)
        {
            // permission validation is not required here
            // This action method gets called via an ajax request

            var country = _countryService.GetCountryById(countryId.ToInt());

            var states = country != null ? _stateProvinceService.GetStateProvincesByCountryId(country.Id, true).ToList() : new List<StateProvince>();
            var result = (from s in states select new { id = s.Id, name = s.Name }).ToList();

            if (addEmptyStateIfRequired.HasValue && addEmptyStateIfRequired.Value && result.Count == 0)
                result.Insert(0, new { id = 0, name = T("Admin.Address.OtherNonUS").Text });

            if (addAsterisk.HasValue && addAsterisk.Value)
                result.Insert(0, new { id = 0, name = "*" });

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
