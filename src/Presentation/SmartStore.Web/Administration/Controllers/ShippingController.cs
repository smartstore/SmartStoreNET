using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Admin.Models.Directory;
using SmartStore.Admin.Models.Shipping;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class ShippingController : AdminControllerBase
	{
		#region Fields

        private readonly IShippingService _shippingService;
        private readonly ShippingSettings _shippingSettings;
        private readonly ISettingService _settingService;
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
        private readonly IPluginFinder _pluginFinder;
		private readonly PluginMediator _pluginMediator;

		#endregion

		#region Constructors

        public ShippingController(IShippingService shippingService, ShippingSettings shippingSettings,
            ISettingService settingService, ICountryService countryService,
            ILocalizationService localizationService, IPermissionService permissionService,
            ILocalizedEntityService localizedEntityService, ILanguageService languageService,
            IPluginFinder pluginFinder,
			PluginMediator pluginMediator)
		{
            this._shippingService = shippingService;
            this._shippingSettings = shippingSettings;
            this._settingService = settingService;
            this._countryService = countryService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._localizedEntityService = localizedEntityService;
            this._languageService = languageService;
            this._pluginFinder = pluginFinder;
			this._pluginMediator = pluginMediator;
		}

		#endregion 
        
        #region Utilities

        [NonAction]
        protected void UpdateLocales(ShippingMethod shippingMethod, ShippingMethodModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(shippingMethod,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(shippingMethod,
                                                           x => x.Description,
                                                           localized.Description,
                                                           localized.LanguageId);
            }
        }

        #endregion

        #region Shipping rate computation methods

        public ActionResult Providers()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var shippingProvidersModel = new List<ShippingRateComputationMethodModel>();
            var shippingProviders = _shippingService.LoadAllShippingRateComputationMethods();
            foreach (var shippingProvider in shippingProviders)
            {
				var model = _pluginMediator.ToProviderModel<IShippingRateComputationMethod, ShippingRateComputationMethodModel>(shippingProvider);
				model.IsActive = shippingProvider.IsShippingRateComputationMethodActive(_shippingSettings);
                shippingProvidersModel.Add(model);
            }

			return View(shippingProvidersModel);
        }

		public ActionResult ActivateProvider(string systemName, bool activate)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
				return AccessDeniedView();

			var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName(systemName);
			if (srcm.IsShippingRateComputationMethodActive(_shippingSettings))
			{
				if (!activate)
				{
					// mark as disabled
					_shippingSettings.ActiveShippingRateComputationMethodSystemNames.Remove(srcm.Metadata.SystemName);
					_settingService.SaveSetting(_shippingSettings);
				}
			}
			else
			{
				if (activate)
				{
					// mark as active
					_shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(srcm.Metadata.SystemName);
					_settingService.SaveSetting(_shippingSettings);
				}
			}

			return RedirectToAction("Providers");
		}

        #endregion
        
        #region Shipping methods

        public ActionResult Methods()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var shippingMethodsModel = _shippingService.GetAllShippingMethods()
                .Select(x => x.ToModel())
                .ToList();
            var model = new GridModel<ShippingMethodModel>
            {
                Data = shippingMethodsModel,
                Total = shippingMethodsModel.Count
            };
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult Methods(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var shippingMethodsModel = _shippingService.GetAllShippingMethods()
                .Select(x => x.ToModel())
                .ForCommand(command)
                .ToList();
            var model = new GridModel<ShippingMethodModel>
            {
                Data = shippingMethodsModel,
                Total = shippingMethodsModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }


        public ActionResult CreateMethod()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var model = new ShippingMethodModel();
            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult CreateMethod(ShippingMethodModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var sm = model.ToEntity();
                _shippingService.InsertShippingMethod(sm);
                //locales
                UpdateLocales(sm, model);

                NotifySuccess(_localizationService.GetResource("Admin.Configuration.Shipping.Methods.Added"));
                return continueEditing ? RedirectToAction("EditMethod", new { id = sm.Id }) : RedirectToAction("Methods");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult EditMethod(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var sm = _shippingService.GetShippingMethodById(id);
            if (sm == null)
                //No shipping method found with the specified id
                return RedirectToAction("Methods");

            var model = sm.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = sm.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = sm.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult EditMethod(ShippingMethodModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var sm = _shippingService.GetShippingMethodById(model.Id);
            if (sm == null)
                //No shipping method found with the specified id
                return RedirectToAction("Methods");

            if (ModelState.IsValid)
            {
                sm = model.ToEntity(sm);
                _shippingService.UpdateShippingMethod(sm);
                //locales
                UpdateLocales(sm, model);
                NotifySuccess(_localizationService.GetResource("Admin.Configuration.Shipping.Methods.Updated"));
                return continueEditing ? RedirectToAction("EditMethod", sm.Id) : RedirectToAction("Methods");
            }


            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteMethod(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var sm = _shippingService.GetShippingMethodById(id);
            if (sm == null)
                //No shipping method found with the specified id
                return RedirectToAction("Methods");

            _shippingService.DeleteShippingMethod(sm);

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Shipping.Methods.Deleted"));
            return RedirectToAction("Methods");
        }
        
        #endregion
        
        #region Restrictions

        public ActionResult Restrictions()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var model = new ShippingMethodRestrictionModel();

            var countries = _countryService.GetAllCountries(true);
            var shippingMethods = _shippingService.GetAllShippingMethods();
            foreach (var country in countries)
            {
                model.AvailableCountries.Add(new CountryModel()
                    {
                        Id = country.Id,
                        Name = country.Name
                    });
            }
            foreach (var sm in shippingMethods)
            {
                model.AvailableShippingMethods.Add(new ShippingMethodModel()
                {
                    Id = sm.Id,
                    Name = sm.Name
                });
            }
            foreach (var country in countries)
                foreach (var shippingMethod in shippingMethods)
                {
                    bool restricted = shippingMethod.CountryRestrictionExists(country.Id);
                    if (!model.Restricted.ContainsKey(country.Id))
                        model.Restricted[country.Id] = new Dictionary<int, bool>();
                    model.Restricted[country.Id][shippingMethod.Id] = restricted;
                }

            return View(model);
        }

        [HttpPost, ActionName("Restrictions")]
        public ActionResult RestrictionSave(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var countries = _countryService.GetAllCountries(true);
            var shippingMethods = _shippingService.GetAllShippingMethods();


            foreach (var shippingMethod in shippingMethods)
            {
                string formKey = "restrict_" + shippingMethod.Id;
                var countryIdsToRestrict = form[formKey] != null ? form[formKey].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() : new List<int>();

                foreach (var country in countries)
                {

                    bool restrict = countryIdsToRestrict.Contains(country.Id);
                    if (restrict)
                    {
                        if (shippingMethod.RestrictedCountries.Where(c => c.Id == country.Id).FirstOrDefault() == null)
                        {
                            shippingMethod.RestrictedCountries.Add(country);
                            _shippingService.UpdateShippingMethod(shippingMethod);
                        }
                    }
                    else
                    {
                        if (shippingMethod.RestrictedCountries.Where(c => c.Id == country.Id).FirstOrDefault() != null)
                        {
                            shippingMethod.RestrictedCountries.Remove(country);
                            _shippingService.UpdateShippingMethod(shippingMethod);
                        }
                    }
                }
            }

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Shipping.Restrictions.Updated"));
            return RedirectToAction("Restrictions");
        }

        #endregion
    }
}
