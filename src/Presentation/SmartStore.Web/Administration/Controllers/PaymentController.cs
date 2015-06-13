using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Admin.Models.Payments;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class PaymentController : AdminControllerBase
	{
		#region Fields

		private readonly ICommonServices _commonServices;
        private readonly IPaymentService _paymentService;
        private readonly PaymentSettings _paymentSettings;
        private readonly IPluginFinder _pluginFinder;
		private readonly PluginMediator _pluginMediator;

		#endregion

		#region Constructors

        public PaymentController(
			ICommonServices commonServices,
			IPaymentService paymentService, 
			PaymentSettings paymentSettings,
            IPluginFinder pluginFinder, 
			PluginMediator pluginMediator)
		{
			this._commonServices = commonServices;
            this._paymentService = paymentService;
            this._paymentSettings = paymentSettings;
            this._pluginFinder = pluginFinder;
			this._pluginMediator = pluginMediator;
		}

		#endregion 

        #region Methods

        public ActionResult Providers()
        {
			if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManagePaymentMethods))
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
				model.RecurringPaymentType = instance.RecurringPaymentType.GetLocalizedEnum(_commonServices.Localization);
                paymentMethodsModel.Add(model);
            }

			return View(paymentMethodsModel);
        }

		public ActionResult ActivateProvider(string systemName, bool activate)
		{
			if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManagePaymentMethods))
				return AccessDeniedView();

			var pm = _paymentService.LoadPaymentMethodBySystemName(systemName);

			if (activate && !pm.Value.IsActive)
			{
				NotifyWarning(_commonServices.Localization.GetResource("Admin.Configuration.Payment.CannotActivatePaymentMethod"));
			}
			else
			{
				if (!activate)
					_paymentSettings.ActivePaymentMethodSystemNames.Remove(pm.Metadata.SystemName);
				else
					_paymentSettings.ActivePaymentMethodSystemNames.Add(pm.Metadata.SystemName);

				_commonServices.Settings.SaveSetting(_paymentSettings);
				_pluginMediator.ActivateDependentWidgets(pm.Metadata, activate);
			}

			return RedirectToAction("Providers");
		}

		public ActionResult Edit(string systemName)
		{
			if (!_commonServices.Permissions.Authorize(StandardPermissionProvider.ManagePaymentMethods))
				return AccessDeniedView();

			var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(systemName);

			var model = _pluginMediator.ToProviderModel<IPaymentMethod, PaymentMethodEditModel>(paymentMethod);

			model.IconUrl = _pluginMediator.GetIconUrl(model.PluginDescriptor);

			return View(model);
		}

        #endregion
    }
}
