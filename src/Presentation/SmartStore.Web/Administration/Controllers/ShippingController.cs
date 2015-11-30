using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Shipping;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services;
using SmartStore.Services.Customers;
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
        private readonly ICountryService _countryService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
		private readonly PluginMediator _pluginMediator;
		private readonly ICommonServices _services;
		private readonly ICustomerService _customerService;

		#endregion

		#region Constructors

        public ShippingController(IShippingService shippingService,
			ShippingSettings shippingSettings,
            ICountryService countryService,
            ILocalizedEntityService localizedEntityService,
			ILanguageService languageService,
			PluginMediator pluginMediator,
			ICommonServices services,
			ICustomerService customerService)
		{
            this._shippingService = shippingService;
            this._shippingSettings = shippingSettings;
            this._countryService = countryService;
            this._localizedEntityService = localizedEntityService;
            this._languageService = languageService;
			this._pluginMediator = pluginMediator;
			this._services = services;
			this._customerService = customerService;
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

		private void PrepareShippingMethodModel(ShippingMethodModel model, ShippingMethod shippingMethod)
		{
			var customerRoles = _customerService.GetAllCustomerRoles(true);
			var countries = _countryService.GetAllCountries(true);

			model.AvailableCustomerRoles = new List<SelectListItem>();
			model.AvailableCountries = new List<SelectListItem>();

			model.AvailableCountryExclusionContextTypes = CountryRestrictionContextType.BillingAddress.ToSelectList(false).ToList();

			foreach (var role in customerRoles.OrderBy(x => x.Name))
			{
				model.AvailableCustomerRoles.Add(new SelectListItem { Text = role.Name, Value = role.Id.ToString() });
			}

			foreach (var country in countries.OrderBy(x => x.Name))
			{
				model.AvailableCountries.Add(new SelectListItem { Text = country.GetLocalized(x => x.Name), Value = country.Id.ToString() });
			}

			if (shippingMethod != null)
			{
				model.ExcludedCustomerRoleIds = shippingMethod.ExcludedCustomerRoleIds.SplitSafe(",");
				model.ExcludedCountryIds = shippingMethod.RestrictedCountries.Select(x => x.Id.ToString()).ToArray();

				model.CountryExclusionContext = shippingMethod.CountryExclusionContext;
			}
		}

		private void ApplyRestrictions(ShippingMethod shippingMethod, ShippingMethodModel model)
		{
			var countries = _countryService.GetAllCountries(true);

			shippingMethod.ExcludedCustomerRoleIds = Request.Form["ExcludedCustomerRoleIds"];
			shippingMethod.CountryExclusionContext = model.CountryExclusionContext;

			string[] excludedCountryIds = Request.Form["ExcludedCountryIds"].SplitSafe(",");

			foreach (var country in countries)
			{
				if (excludedCountryIds.Contains(country.Id.ToString()))
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

        #endregion

        #region Shipping rate computation methods

        public ActionResult Providers()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageShippingSettings))
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
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageShippingSettings))
				return AccessDeniedView();

			var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName(systemName);

			if (activate && !srcm.Value.IsActive)
			{
				NotifyWarning(_services.Localization.GetResource("Admin.Configuration.Payment.CannotActivateShippingRateComputationMethod"));
			}
			else
			{
				if (!activate)
					_shippingSettings.ActiveShippingRateComputationMethodSystemNames.Remove(srcm.Metadata.SystemName);
				else
					_shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(srcm.Metadata.SystemName);

				_services.Settings.SaveSetting(_shippingSettings);
				_pluginMediator.ActivateDependentWidgets(srcm.Metadata, activate);
			}

			return RedirectToAction("Providers");
		}

        #endregion
        
        #region Shipping methods

        public ActionResult Methods()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageShippingSettings))
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
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageShippingSettings))
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
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var model = new ShippingMethodModel();
			PrepareShippingMethodModel(model, null);

            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult CreateMethod(ShippingMethodModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var sm = model.ToEntity();
				ApplyRestrictions(sm, model);

                _shippingService.InsertShippingMethod(sm);
                
				//locales
                UpdateLocales(sm, model);

                NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Shipping.Methods.Added"));

                return continueEditing ? RedirectToAction("EditMethod", new { id = sm.Id }) : RedirectToAction("Methods");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult EditMethod(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var sm = _shippingService.GetShippingMethodById(id);
            if (sm == null)
                return RedirectToAction("Methods");

            var model = sm.ToModel();
			PrepareShippingMethodModel(model, sm);

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
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var sm = _shippingService.GetShippingMethodById(model.Id);
            if (sm == null)
                return RedirectToAction("Methods");

            if (ModelState.IsValid)
            {
                sm = model.ToEntity(sm);
				ApplyRestrictions(sm, model);

                _shippingService.UpdateShippingMethod(sm);

                //locales
                UpdateLocales(sm, model);

				NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Shipping.Methods.Updated"));

                return continueEditing ? RedirectToAction("EditMethod", sm.Id) : RedirectToAction("Methods");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteMethod(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var sm = _shippingService.GetShippingMethodById(id);
            if (sm == null)
                return RedirectToAction("Methods");

            _shippingService.DeleteShippingMethod(sm);

			NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Shipping.Methods.Deleted"));
            return RedirectToAction("Methods");
        }
        
        #endregion        
    }
}
