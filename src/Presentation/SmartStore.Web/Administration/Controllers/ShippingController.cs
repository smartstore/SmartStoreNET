using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Shipping;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Security;
using SmartStore.Rules;
using SmartStore.Services.Localization;
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
        private readonly IStoreMappingService _storeMappingService;
        private readonly IRuleStorage _ruleStorage;

        public ShippingController(
            IShippingService shippingService,
            ShippingSettings shippingSettings,
            ILocalizedEntityService localizedEntityService,
            ILanguageService languageService,
            PluginMediator pluginMediator,
            IStoreMappingService storeMappingService,
            IRuleStorage ruleStorage)
        {
            _shippingService = shippingService;
            _shippingSettings = shippingSettings;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
            _pluginMediator = pluginMediator;
            _storeMappingService = storeMappingService;
            _ruleStorage = ruleStorage;
        }

        #region Utilities

        private void UpdateLocales(ShippingMethod shippingMethod, ShippingMethodModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(shippingMethod, x => x.Name, localized.Name, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(shippingMethod, x => x.Description, localized.Description, localized.LanguageId);
            }
        }

        private void PrepareShippingMethodModel(ShippingMethodModel model, ShippingMethod shippingMethod)
        {
            if (shippingMethod != null)
            {
                model.SelectedRuleSetIds = shippingMethod.RuleSets.Select(x => x.Id).ToArray();
            }

            model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(shippingMethod);
        }

        #endregion

        #region Shipping rate computation methods

        [Permission(Permissions.Configuration.Shipping.Read)]
        public ActionResult Providers()
        {
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Shipping.Activate)]
        public ActionResult ActivateProvider(string systemName, bool activate)
        {
            var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName(systemName);

            if (activate && !srcm.Value.IsActive)
            {
                NotifyWarning(T("Admin.Configuration.Payment.CannotActivateShippingRateComputationMethod"));
            }
            else
            {
                if (!activate)
                {
                    _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Remove(srcm.Metadata.SystemName);
                }
                else
                {
                    _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(srcm.Metadata.SystemName);
                }

                Services.Settings.SaveSetting(_shippingSettings);
                _pluginMediator.ActivateDependentWidgets(srcm.Metadata, activate);
            }

            return RedirectToAction("Providers");
        }

        #endregion

        #region Shipping methods

        [Permission(Permissions.Configuration.Shipping.Read)]
        public ActionResult Methods()
        {
            var shippingMethodsModel = _shippingService.GetAllShippingMethods()
                .Select(x =>
                {
                    var smm = x.ToModel();
                    smm.NumberOfRules = x.RuleSets.Count;
                    return smm;
                })
                .ToList();

            var model = new GridModel<ShippingMethodModel>
            {
                Data = shippingMethodsModel,
                Total = shippingMethodsModel.Count
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Shipping.Read)]
        public ActionResult Methods(GridCommand command)
        {
            var model = new GridModel<ShippingMethodModel>();

            var shippingMethodsModel = _shippingService.GetAllShippingMethods()
                .Select(x =>
                {
                    var smm = x.ToModel();
                    smm.NumberOfRules = x.RuleSets.Count;
                    return smm;
                })
                .ForCommand(command)
                .ToList();

            model.Data = shippingMethodsModel;
            model.Total = shippingMethodsModel.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Configuration.Shipping.Create)]
        public ActionResult CreateMethod()
        {
            var model = new ShippingMethodModel();
            PrepareShippingMethodModel(model, null);

            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Shipping.Create)]
        public ActionResult CreateMethod(ShippingMethodModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var sm = model.ToEntity();
                _shippingService.InsertShippingMethod(sm);

                if (model.SelectedRuleSetIds?.Any() ?? false)
                {
                    _ruleStorage.ApplyRuleSetMappings(sm, model.SelectedRuleSetIds);

                    _shippingService.UpdateShippingMethod(sm);
                }

                SaveStoreMappings(sm, model.SelectedStoreIds);
                UpdateLocales(sm, model);

                NotifySuccess(T("Admin.Configuration.Shipping.Methods.Added"));
                return continueEditing ? RedirectToAction("EditMethod", new { id = sm.Id }) : RedirectToAction("Methods");
            }

            return View(model);
        }

        [Permission(Permissions.Configuration.Shipping.Read)]
        public ActionResult EditMethod(int id)
        {
            var sm = _shippingService.GetShippingMethodById(id);
            if (sm == null)
            {
                return RedirectToAction("Methods");
            }

            var model = sm.ToModel();
            PrepareShippingMethodModel(model, sm);

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = sm.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = sm.GetLocalized(x => x.Description, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Shipping.Update)]
        public ActionResult EditMethod(ShippingMethodModel model, bool continueEditing, FormCollection form)
        {
            var sm = _shippingService.GetShippingMethodById(model.Id);
            if (sm == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                sm = model.ToEntity(sm);

                // Add\remove assigned rule sets.
                _ruleStorage.ApplyRuleSetMappings(sm, model.SelectedRuleSetIds);

                _shippingService.UpdateShippingMethod(sm);

                SaveStoreMappings(sm, model.SelectedStoreIds);
                UpdateLocales(sm, model);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, sm, form));

                NotifySuccess(T("Admin.Configuration.Shipping.Methods.Updated"));
                return continueEditing ? RedirectToAction("EditMethod", sm.Id) : RedirectToAction("Methods");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Shipping.Delete)]
        public ActionResult DeleteMethod(int id)
        {
            var sm = _shippingService.GetShippingMethodById(id);
            if (sm == null)
            {
                return RedirectToAction("Methods");
            }

            _shippingService.DeleteShippingMethod(sm);

            NotifySuccess(T("Admin.Configuration.Shipping.Methods.Deleted"));
            return RedirectToAction("Methods");
        }

        #endregion
    }
}
