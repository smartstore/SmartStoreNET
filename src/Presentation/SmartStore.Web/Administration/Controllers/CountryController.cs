using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Directory;
using SmartStore.Core;
using SmartStore.Core.Domain.Directory;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public class CountryController : AdminControllerBase
	{
		#region Fields

        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;
	    private readonly IAddressService _addressService;
        private readonly IPermissionService _permissionService;
	    private readonly ILocalizedEntityService _localizedEntityService;
	    private readonly ILanguageService _languageService;
		private readonly IStoreService _storeService;
		private readonly IStoreMappingService _storeMappingService;

	    #endregion

		#region Constructors

        public CountryController(ICountryService countryService,
            IStateProvinceService stateProvinceService,
			ILocalizationService localizationService,
            IAddressService addressService,
			IPermissionService permissionService,
            ILocalizedEntityService localizedEntityService,
			ILanguageService languageService,
			IStoreService storeService,
			IStoreMappingService storeMappingService)
		{
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _localizationService = localizationService;
            _addressService = addressService;
            _permissionService = permissionService;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
			_storeService = storeService;
			_storeMappingService = storeMappingService;
		}

		#endregion 

        #region Utilities 
        
        [NonAction]
		private void UpdateLocales(Country country, CountryModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(country,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
            }
        }

        [NonAction]
		private void UpdateLocales(StateProvince stateProvince, StateProvinceModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(stateProvince,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
            }
        }

		[NonAction]
		private void PrepareCountryModel(CountryModel model, Country country, bool excludeProperties)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			var allStores = _storeService.GetAllStores();

			model.AvailableStores = allStores.Select(s => s.ToModel()).ToList();

			if (!excludeProperties)
			{
				if (country != null)
					model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(country);
				else
					model.SelectedStoreIds = new int[0];
			}

			ViewBag.StoreCount = allStores.Count;
		}

        #endregion

        #region Countries

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            var countries = _countryService.GetAllCountries(true);
            var model = new GridModel<CountryModel>
            {
                Data = countries.Select(x => x.ToModel()),
                Total = countries.Count
            };
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CountryList(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            var countries = _countryService.GetAllCountries(true);
            var model = new GridModel<CountryModel>
            {
                Data = countries.Select(x => x.ToModel()),
                Total = countries.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }
        
        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
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

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Create(CountryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var country = model.ToEntity();
                _countryService.InsertCountry(country);

                UpdateLocales(country, model);

				_storeMappingService.SaveStoreMappings<Country>(country, model.SelectedStoreIds);

                NotifySuccess(_localizationService.GetResource("Admin.Configuration.Countries.Added"));

                return continueEditing ? RedirectToAction("Edit", new { id = country.Id }) : RedirectToAction("List");
            }

			PrepareCountryModel(model, null, true);
            
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
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

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Edit(CountryModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
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

                NotifySuccess(_localizationService.GetResource("Admin.Configuration.Countries.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = country.Id }) : RedirectToAction("List");
            }

			PrepareCountryModel(model, country, true);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            var country = _countryService.GetCountryById(id);
            if (country == null)
                return RedirectToAction("List");

            try
            {
                if (_addressService.GetAddressTotalByCountryId(country.Id) > 0)
                    throw new SmartException("The country can't be deleted. It has associated addresses");

                _countryService.DeleteCountry(country);

                NotifySuccess(_localizationService.GetResource("Admin.Configuration.Countries.Deleted"));
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            var states = _stateProvinceService.GetStateProvincesByCountryId(countryId, true)
                .Select(x => x.ToModel());

            var model = new GridModel<StateProvinceModel>
            {
                Data = states,
                Total = states.Count()
            };
            return new JsonResult
            {
                Data = model
            };
        }


        //create
        public ActionResult StateCreatePopup(int countryId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            var model = new StateProvinceModel();
            model.CountryId = countryId;
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public ActionResult StateCreatePopup(string btnId, string formId, StateProvinceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            var sp = _stateProvinceService.GetStateProvinceById(id);
            if (sp == null)
                //No state found with the specified id
                return RedirectToAction("List");

            var model = sp.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = sp.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public ActionResult StateEditPopup(string btnId, string formId, StateProvinceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            var sp = _stateProvinceService.GetStateProvinceById(model.Id);
            if (sp == null)
                //No state found with the specified id
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCountries))
                return AccessDeniedView();

            var state = _stateProvinceService.GetStateProvinceById(id);
            if (state == null)
                throw new ArgumentException("No state found with the specified id");

            if (_addressService.GetAddressTotalByStateProvinceId(state.Id) > 0)
                return Content(_localizationService.GetResource("Admin.Configuration.Countries.States.CantDeleteWithAddresses"));

            int countryId = state.CountryId;
            _stateProvinceService.DeleteStateProvince(state);


            return States(countryId, command);
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetStatesByCountryId(string countryId, bool? addEmptyStateIfRequired, bool? addAsterisk)
        {
            //permission validation is not required here

            // This action method gets called via an ajax request

            int cid = 0;
            int.TryParse(countryId, out cid);
            /*if (String.IsNullOrEmpty(countryId))
                throw new ArgumentNullException("countryId");*/

            var country = _countryService.GetCountryById(cid /* Convert.ToInt32(countryId) */);
            var states = country != null ? _stateProvinceService.GetStateProvincesByCountryId(country.Id, true).ToList() : new List<StateProvince>();
            var result = (from s in states
                         select new { id = s.Id, name = s.Name }).ToList();
            if (addEmptyStateIfRequired.HasValue && addEmptyStateIfRequired.Value && result.Count == 0)
                result.Insert(0, new { id = 0, name = _localizationService.GetResource("Admin.Address.OtherNonUS") });
            if (addAsterisk.HasValue && addAsterisk.Value)
                result.Insert(0, new { id = 0, name = "*" });
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
