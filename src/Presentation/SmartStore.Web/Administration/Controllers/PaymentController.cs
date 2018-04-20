using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Payments;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;
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
		private readonly ICommonServices _services;
        private readonly IPaymentService _paymentService;
        private readonly PaymentSettings _paymentSettings;
        private readonly IPluginFinder _pluginFinder;
		private readonly PluginMediator _pluginMediator;
		private readonly ILanguageService _languageService;
		private readonly ICustomerService _customerService;
		private readonly IShippingService _shippingService;
		private readonly ICountryService _countryService;
		private readonly ILocalizedEntityService _localizedEntityService;
		private readonly IStoreMappingService _storeMappingService;

		public PaymentController(
			ICommonServices services,
			IPaymentService paymentService, 
			PaymentSettings paymentSettings,
            IPluginFinder pluginFinder, 
			PluginMediator pluginMediator,
			ILanguageService languageService,
			ICustomerService customerService,
			IShippingService shippingService,
			ICountryService countryService,
			ILocalizedEntityService localizedEntityService,
			IStoreMappingService storeMappingService)
		{
			_services = services;
            _paymentService = paymentService;
            _paymentSettings = paymentSettings;
            _pluginFinder = pluginFinder;
			_pluginMediator = pluginMediator;
			_languageService = languageService;
			_customerService = customerService;
			_shippingService = shippingService;
			_countryService = countryService;
			_localizedEntityService = localizedEntityService;
			_storeMappingService = storeMappingService;
		}

		#region Utilities

		private void PreparePaymentMethodEditModel(PaymentMethodEditModel model, PaymentMethod paymentMethod)
		{
			var allFilters = _paymentService.GetAllPaymentMethodFilters();
			var configUrls = allFilters
				.Select(x => x.GetConfigurationUrl(model.SystemName))
				.Where(x => x.HasValue())
				.ToList();

			model.FilterConfigurationUrls = configUrls
				.Select(x => string.Concat("'", x, "'"))
				.OrderBy(x => x)
				.ToList();

			if (paymentMethod != null)
			{
				model.Id = paymentMethod.Id;
				model.FullDescription = paymentMethod.FullDescription;
                model.RoundOrderTotalEnabled = paymentMethod.RoundOrderTotalEnabled;
				model.LimitedToStores = paymentMethod.LimitedToStores;
				model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(paymentMethod);
			}

			model.AvailableStores = _services.StoreService.GetAllStores().ToSelectListItems(model.SelectedStoreIds);
		}

		#endregion

		#region Methods

		public ActionResult Providers()
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

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
				model.RecurringPaymentType = instance.RecurringPaymentType.GetLocalizedEnum(_services.Localization);
                paymentMethodsModel.Add(model);
            }

			return View(paymentMethodsModel);
        }

		public ActionResult ActivateProvider(string systemName, bool activate)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManagePaymentMethods))
				return AccessDeniedView();

			var pm = _paymentService.LoadPaymentMethodBySystemName(systemName);

			if (activate && !pm.Value.IsActive)
			{
				NotifyWarning(_services.Localization.GetResource("Admin.Configuration.Payment.CannotActivatePaymentMethod"));
			}
			else
			{
				if (!activate)
					_paymentSettings.ActivePaymentMethodSystemNames.Remove(pm.Metadata.SystemName);
				else
					_paymentSettings.ActivePaymentMethodSystemNames.Add(pm.Metadata.SystemName);

				_services.Settings.SaveSetting(_paymentSettings);
				_pluginMediator.ActivateDependentWidgets(pm.Metadata, activate);
			}

			return RedirectToAction("Providers");
		}

		public ActionResult Edit(string systemName)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManagePaymentMethods))
				return AccessDeniedView();

			var provider = _paymentService.LoadPaymentMethodBySystemName(systemName);
			var paymentMethod = _paymentService.GetPaymentMethodBySystemName(systemName);

			var model = new PaymentMethodEditModel();
			var providerModel = _pluginMediator.ToProviderModel<IPaymentMethod, ProviderModel>(provider, true);
			var pageTitle = providerModel.FriendlyName;

			model.SystemName = providerModel.SystemName;
			model.IconUrl = providerModel.IconUrl;
			model.FriendlyName = providerModel.FriendlyName;
			model.Description = providerModel.Description;

			AddLocales(_languageService, model.Locales, (locale, languageId) =>
			{
				locale.FriendlyName = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata, languageId, false);
				locale.Description = _pluginMediator.GetLocalizedDescription(provider.Metadata, languageId, false);

				if (pageTitle.IsEmpty() && languageId == _services.WorkContext.WorkingLanguage.Id)
				{
					pageTitle = locale.FriendlyName;
				}

				if (paymentMethod != null)
				{
					locale.FullDescription = paymentMethod.GetLocalized(x => x.FullDescription, languageId, false, false);
				}
			});

			PreparePaymentMethodEditModel(model, paymentMethod);

			ViewBag.Title = pageTitle;

			return View(model);
		}

		[HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
		public ActionResult Edit(string systemName, bool continueEditing, PaymentMethodEditModel model, FormCollection form)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManagePaymentMethods))
				return AccessDeniedView();

			var provider = _paymentService.LoadPaymentMethodBySystemName(systemName);
			if (provider == null)
				return HttpNotFound();

			_pluginMediator.SetSetting(provider.Metadata, "FriendlyName", model.FriendlyName);
			_pluginMediator.SetSetting(provider.Metadata, "Description", model.Description);

			var paymentMethod = _paymentService.GetPaymentMethodBySystemName(systemName);

			if (paymentMethod == null)
				paymentMethod = new PaymentMethod { PaymentMethodSystemName = systemName };

			paymentMethod.FullDescription = model.FullDescription;
            paymentMethod.RoundOrderTotalEnabled = model.RoundOrderTotalEnabled;
			paymentMethod.LimitedToStores = model.LimitedToStores;

			if (paymentMethod.Id == 0)
				_paymentService.InsertPaymentMethod(paymentMethod);
			else
				_paymentService.UpdatePaymentMethod(paymentMethod);

			_storeMappingService.SaveStoreMappings(paymentMethod, model.SelectedStoreIds);

			foreach (var localized in model.Locales)
			{
				_pluginMediator.SaveLocalizedValue(provider.Metadata, localized.LanguageId, "FriendlyName", localized.FriendlyName);
				_pluginMediator.SaveLocalizedValue(provider.Metadata, localized.LanguageId, "Description", localized.Description);

				_localizedEntityService.SaveLocalizedValue(paymentMethod, x => x.FullDescription, localized.FullDescription, localized.LanguageId);
			}

			_services.EventPublisher.Publish(new ModelBoundEvent(model, paymentMethod, form));

			NotifySuccess(T("Admin.Common.DataEditSuccess"));

			return (continueEditing ?
				RedirectToAction("Edit", "Payment", new { systemName = systemName }) :
				RedirectToAction("Providers", "Payment"));
		}

        #endregion
    }
}
