using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Payments;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Security;
using SmartStore.Rules;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class PaymentController : AdminControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly PaymentSettings _paymentSettings;
        private readonly PluginMediator _pluginMediator;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IRuleStorage _ruleStorage;

        public PaymentController(
            IPaymentService paymentService,
            PaymentSettings paymentSettings,
            PluginMediator pluginMediator,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            IStoreMappingService storeMappingService,
            IRuleStorage ruleStorage)
        {
            _paymentService = paymentService;
            _paymentSettings = paymentSettings;
            _pluginMediator = pluginMediator;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _storeMappingService = storeMappingService;
            _ruleStorage = ruleStorage;
        }

        [Permission(Permissions.Configuration.PaymentMethod.Read)]
        public ActionResult Providers()
        {
            var paymentMethodsModel = new List<PaymentMethodModel>();
            var paymentMethods = _paymentService.LoadAllPaymentMethods();

            foreach (var paymentMethod in paymentMethods)
            {
                var model = _pluginMediator.ToProviderModel<IPaymentMethod, PaymentMethodModel>(paymentMethod);
                var instance = paymentMethod.Value;
                model.IsActive = paymentMethod.IsPaymentMethodActive(_paymentSettings);
                model.SupportCapture = instance.SupportCapture;
                model.SupportPartiallyRefund = instance.SupportPartiallyRefund;
                model.SupportRefund = instance.SupportRefund;
                model.SupportVoid = instance.SupportVoid;
                model.RecurringPaymentType = instance.RecurringPaymentType.GetLocalizedEnum(Services.Localization);
                paymentMethodsModel.Add(model);
            }

            return View(paymentMethodsModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.PaymentMethod.Activate)]
        public ActionResult ActivateProvider(string systemName, bool activate)
        {
            var pm = _paymentService.LoadPaymentMethodBySystemName(systemName);

            if (activate && !pm.Value.IsActive)
            {
                NotifyWarning(T("Admin.Configuration.Payment.CannotActivatePaymentMethod"));
            }
            else
            {
                if (!activate)
                {
                    _paymentSettings.ActivePaymentMethodSystemNames.Remove(pm.Metadata.SystemName);
                }
                else
                {
                    _paymentSettings.ActivePaymentMethodSystemNames.Add(pm.Metadata.SystemName);
                }

                Services.Settings.SaveSetting(_paymentSettings);
                _pluginMediator.ActivateDependentWidgets(pm.Metadata, activate);
            }

            return RedirectToAction("Providers");
        }

        [Permission(Permissions.Configuration.PaymentMethod.Read)]
        public ActionResult Edit(string systemName)
        {
            var provider = _paymentService.LoadPaymentMethodBySystemName(systemName);
            var paymentMethod = _paymentService.GetPaymentMethodBySystemName(systemName);

            var model = new PaymentMethodEditModel();
            var providerModel = _pluginMediator.ToProviderModel<IPaymentMethod, ProviderModel>(provider, true);
            var pageTitle = providerModel.FriendlyName;

            model.SystemName = providerModel.SystemName;
            model.IconUrl = providerModel.IconUrl;
            model.FriendlyName = providerModel.FriendlyName;
            model.Description = providerModel.Description;
            model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(paymentMethod);

            if (paymentMethod != null)
            {
                model.Id = paymentMethod.Id;
                model.FullDescription = paymentMethod.FullDescription;
                model.RoundOrderTotalEnabled = paymentMethod.RoundOrderTotalEnabled;
                model.LimitedToStores = paymentMethod.LimitedToStores;
                model.SelectedRuleSetIds = paymentMethod.RuleSets.Select(x => x.Id).ToArray();
            }

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.FriendlyName = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata, languageId, false);
                locale.Description = _pluginMediator.GetLocalizedDescription(provider.Metadata, languageId, false);

                if (pageTitle.IsEmpty() && languageId == Services.WorkContext.WorkingLanguage.Id)
                {
                    pageTitle = locale.FriendlyName;
                }

                if (paymentMethod != null)
                {
                    locale.FullDescription = paymentMethod.GetLocalized(x => x.FullDescription, languageId, false, false);
                }
            });

            ViewBag.Title = pageTitle;

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.PaymentMethod.Update)]
        public ActionResult Edit(string systemName, bool continueEditing, PaymentMethodEditModel model, FormCollection form)
        {
            var provider = _paymentService.LoadPaymentMethodBySystemName(systemName);
            if (provider == null)
            {
                return HttpNotFound();
            }

            _pluginMediator.SetSetting(provider.Metadata, "FriendlyName", model.FriendlyName);
            _pluginMediator.SetSetting(provider.Metadata, "Description", model.Description);

            var paymentMethod = _paymentService.GetPaymentMethodBySystemName(systemName);
            if (paymentMethod == null)
            {
                paymentMethod = new PaymentMethod { PaymentMethodSystemName = systemName };
            }

            paymentMethod.FullDescription = model.FullDescription;
            paymentMethod.RoundOrderTotalEnabled = model.RoundOrderTotalEnabled;
            paymentMethod.LimitedToStores = model.LimitedToStores;

            var updateEntity = paymentMethod.Id != 0;

            if (paymentMethod.Id == 0)
            {
                // In this case the update permission is sufficient.
                _paymentService.InsertPaymentMethod(paymentMethod);

                updateEntity = model.SelectedRuleSetIds?.Any() ?? false;
            }

            if (updateEntity)
            {
                // Add\remove assigned rule sets.
                _ruleStorage.ApplyRuleSetMappings(paymentMethod, model.SelectedRuleSetIds);

                _paymentService.UpdatePaymentMethod(paymentMethod);
            }

            SaveStoreMappings(paymentMethod, model.SelectedStoreIds);

            foreach (var localized in model.Locales)
            {
                _pluginMediator.SaveLocalizedValue(provider.Metadata, localized.LanguageId, "FriendlyName", localized.FriendlyName);
                _pluginMediator.SaveLocalizedValue(provider.Metadata, localized.LanguageId, "Description", localized.Description);

                _localizedEntityService.SaveLocalizedValue(paymentMethod, x => x.FullDescription, localized.FullDescription, localized.LanguageId);
            }

            Services.EventPublisher.Publish(new ModelBoundEvent(model, paymentMethod, form));

            NotifySuccess(T("Admin.Common.DataEditSuccess"));

            return continueEditing ?
                RedirectToAction("Edit", "Payment", new { systemName }) :
                RedirectToAction("Providers", "Payment");
        }
    }
}
