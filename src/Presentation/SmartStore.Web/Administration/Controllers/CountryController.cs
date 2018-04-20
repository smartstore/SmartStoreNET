using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public class CountryController : AdminControllerBase
	{
		#region Fields

        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
	    private readonly IAddressService _addressService;
	    private readonly ILocalizedEntityService _localizedEntityService;
	    private readonly ILanguageService _languageService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly ICommonServices _services;

		#endregion

		#region Constructors

		public CountryController(ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            ILocalizedEntityService localizedEntityService,
			ILanguageService languageService,
			IStoreMappingService storeMappingService,
			ICommonServices services)
		{
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressService = addressService;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
			_storeMappingService = storeMappingService;
			_services = services;
		}

		#endregion 

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
		private void PrepareCountryModel(CountryModel model, Country country, bool excludeProperties)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			if (!excludeProperties)
			{
				model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(country);
			}

			model.AvailableStores = _services.StoreService.GetAllStores().ToSelectListItems(model.SelectedStoreIds);
		}

        #endregion

        #region Countries

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

			var allStores = _services.StoreService.GetAllStores();

			var model = new CountryListModel
			{
				StoreCount = allStores.Count
			};

			return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CountryList(GridCommand command)
        {
			var model = new GridModel<CountryModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
			{
				var countries = _countryService.GetAllCountries(true);

				model.Data = countries.Select(x => x.ToModel());
				model.Total = countries.Count;
			}
			else
			{
				model.Data = Enumerable.Empty<CountryModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

		[GridAction(EnableCustomBinding = true)]
		public ActionResult CountryUpdate(CountryModel model, GridCommand command)
		{
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
			{
				if (!ModelState.IsValid)
				{
					var modelStateErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
					return Content(modelStateErrors.FirstOrDefault());
				}

				var country = _countryService.GetCountryById(model.Id);

				country = model.ToEntity(country);
				_countryService.UpdateCountry(country);
			}

			return CountryList(command);
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult CountryDelete(int id, GridCommand command)
		{
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
			{
				if (_addressService.GetAddressTotalByCountryId(id) > 0)
				{
					return Content(T("Admin.Configuration.Countries.CannotDeleteDueToAssociatedAddresses"));
				}

				var country = _countryService.GetCountryById(id);

				_countryService.DeleteCountry(country);
			}

			return CountryList(command);
		}

		public ActionResult Create()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            var model = new CountryModel();
            
            AddLocales(_languageService, model.Locales);
			PrepareCountryModel(model, null, false);
            
			//default values
            model.Published = true;
            model.AllowsBilling = true;
            model.AllowsShipping = true;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CountryModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var country = model.ToEntity();
                _countryService.InsertCountry(country);

                UpdateLocales(country, model);

				_storeMappingService.SaveStoreMappings<Country>(country, model.SelectedStoreIds);

                NotifySuccess(T("Admin.Configuration.Countries.Added"));

                return continueEditing ? RedirectToAction("Edit", new { id = country.Id }) : RedirectToAction("List");
            }

			PrepareCountryModel(model, null, true);
            
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            var country = _countryService.GetCountryById(id);
            if (country == null)
                return RedirectToAction("List");

            var model = country.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
				locale.Name = country.GetLocalized(x => x.Name, languageId, false, false);
            });

			PrepareCountryModel(model, country, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(CountryModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            var country = _countryService.GetCountryById(model.Id);
            if (country == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                country = model.ToEntity(country);
                _countryService.UpdateCountry(country);

                UpdateLocales(country, model);

				_storeMappingService.SaveStoreMappings<Country>(country, model.SelectedStoreIds);

                NotifySuccess(T("Admin.Configuration.Countries.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = country.Id }) : RedirectToAction("List");
            }

			PrepareCountryModel(model, country, true);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

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
        public ActionResult States(int countryId, GridCommand command)
        {
			var model = new GridModel<StateProvinceModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
			{
				var states = _stateProvinceService.GetStateProvincesByCountryId(countryId, true)
					.Select(x => x.ToModel());

				model.Data = states;
				model.Total = states.Count();
			}
			else
			{
				model.Data = Enumerable.Empty<StateProvinceModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

        //create
        public ActionResult StateCreatePopup(int countryId)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

			var model = new StateProvinceModel
			{
				CountryId = countryId
			};

            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost]
        public ActionResult StateCreatePopup(string btnId, string formId, StateProvinceModel model)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

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

        //edit
        public ActionResult StateEditPopup(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

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
        public ActionResult StateEditPopup(string btnId, string formId, StateProvinceModel model)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

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
        public ActionResult StateDelete(int id, GridCommand command)
        {
			var state = _stateProvinceService.GetStateProvinceById(id);
			var countryId = state.CountryId;

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageCountries))
			{
				if (_addressService.GetAddressTotalByStateProvinceId(state.Id) > 0)
				{
					return Content(T("Admin.Configuration.Countries.States.CantDeleteWithAddresses"));
				}

				_stateProvinceService.DeleteStateProvince(state);
			}

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
