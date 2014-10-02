using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Admin.Models.Payments;
using SmartStore.Core;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class PaymentController : AdminControllerBase
	{
		#region Fields

        private readonly IPaymentService _paymentService;
        private readonly PaymentSettings _paymentSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IPluginFinder _pluginFinder;
        private readonly ILocalizationService _localizationService;
		private readonly PluginMediator _pluginMediator;

		#endregion

		#region Constructors

        public PaymentController(
			IPaymentService paymentService, 
			PaymentSettings paymentSettings,
            ISettingService settingService, 
			IPermissionService permissionService,
            IPluginFinder pluginFinder, 
			ILocalizationService localizationService,
			PluginMediator pluginMediator)
		{
            this._paymentService = paymentService;
            this._paymentSettings = paymentSettings;
            this._settingService = settingService;
            this._permissionService = permissionService;
            this._pluginFinder = pluginFinder;
            this._localizationService = localizationService;
			this._pluginMediator = pluginMediator;
		}

		#endregion 

        #region Methods

        public ActionResult Providers()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
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
				model.RecurringPaymentType = instance.RecurringPaymentType.GetLocalizedEnum(_localizationService);
                paymentMethodsModel.Add(model);
            }

			return View(paymentMethodsModel);
        }

		public ActionResult ActivateProvider(string systemName, bool activate)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
				return AccessDeniedView();

			var pm = _paymentService.LoadPaymentMethodBySystemName(systemName);
			if (pm.IsPaymentMethodActive(_paymentSettings))
			{
				if (!activate)
				{
					// mark as disabled
					_paymentSettings.ActivePaymentMethodSystemNames.Remove(pm.Metadata.SystemName);
					_settingService.SaveSetting(_paymentSettings);
				}
			}
			else
			{
				if (activate)
				{
					// mark as active
					_paymentSettings.ActivePaymentMethodSystemNames.Add(pm.Metadata.SystemName);
					_settingService.SaveSetting(_paymentSettings);
				}
			}

			return RedirectToAction("Providers");
		}

        #endregion
    }
}
