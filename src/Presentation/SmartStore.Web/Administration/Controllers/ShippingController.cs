using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Shipping;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class ShippingController : AdminControllerBase
	{
        private readonly IShippingService _shippingService;
        private readonly ShippingSettings _shippingSettings;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILanguageService _languageService;
		private readonly PluginMediator _pluginMediator;
		private readonly ICommonServices _services;
		private readonly IStoreMappingService _storeMappingService;

		public ShippingController(
			IShippingService shippingService,
			ShippingSettings shippingSettings,
            ILocalizedEntityService localizedEntityService,
			ILanguageService languageService,
			PluginMediator pluginMediator,
			ICommonServices services,
			IStoreMappingService storeMappingService)
		{
            _shippingService = shippingService;
            _shippingSettings = shippingSettings;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
			_pluginMediator = pluginMediator;
			_services = services;
			_storeMappingService = storeMappingService;
		}
        
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
			if (shippingMethod != null)
			{
				var allFilters = _shippingService.GetAllShippingMethodFilters();
				var configUrls = allFilters
					.Select(x => x.GetConfigurationUrl(shippingMethod.Id))
					.Where(x => x.HasValue())
					.ToList();

				model.FilterConfigurationUrls = configUrls
					.Select(x => string.Concat("'", x, "'"))
					.OrderBy(x => x)
					.ToList();

				model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(shippingMethod);
			}
			else
			{
				model.FilterConfigurationUrls = new List<string>();
				model.SelectedStoreIds = new int[0];
			}

			model.AvailableStores = _services.StoreService.GetAllStores().ToSelectListItems(model.SelectedStoreIds);
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
			var model = new GridModel<ShippingMethodModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageShippingSettings))
			{
				var shippingMethodsModel = _shippingService.GetAllShippingMethods()
					.Select(x => x.ToModel())
					.ForCommand(command)
					.ToList();

				model.Data = shippingMethodsModel;
				model.Total = shippingMethodsModel.Count;
			}
			else
			{
				model.Data = Enumerable.Empty<ShippingMethodModel>();

				NotifyAccessDenied();
			}

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

            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult CreateMethod(ShippingMethodModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var sm = model.ToEntity();
                _shippingService.InsertShippingMethod(sm);

				_storeMappingService.SaveStoreMappings(sm, model.SelectedStoreIds);

				UpdateLocales(sm, model);

                NotifySuccess(_services.Localization.GetResource("Admin.Configuration.Shipping.Methods.Added"));
                return continueEditing ? RedirectToAction("EditMethod", new { id = sm.Id }) : RedirectToAction("Methods");
            }

            // If we got this far, something failed, redisplay form.
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

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = sm.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = sm.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult EditMethod(ShippingMethodModel model, bool continueEditing, FormCollection form)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var sm = _shippingService.GetShippingMethodById(model.Id);
            if (sm == null)
                return RedirectToAction("Methods");

            if (ModelState.IsValid)
            {
                sm = model.ToEntity(sm);
                _shippingService.UpdateShippingMethod(sm);

				_storeMappingService.SaveStoreMappings(sm, model.SelectedStoreIds);

				UpdateLocales(sm, model);

				_services.EventPublisher.Publish(new ModelBoundEvent(model, sm, form));

				NotifySuccess(T("Admin.Configuration.Shipping.Methods.Updated"));
                return continueEditing ? RedirectToAction("EditMethod", sm.Id) : RedirectToAction("Methods");
            }

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
