using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Plugin.Payments.PayInStore.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.Plugin.Payments.PayInStore.Controllers
{
    public class PaymentPayInStoreController : PaymentControllerBase
    {
		private readonly IWorkContext _workContext;
		private readonly IStoreService _storeService;
		private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

		public PaymentPayInStoreController(IWorkContext workContext,
			IStoreService storeService,
			IStoreContext storeContext, 
			ISettingService settingService, 
            ILocalizationService localizationService)
        {
			this._workContext = workContext;
			this._storeService = storeService;
			this._storeContext = storeContext;
            this._settingService = settingService;
            this._localizationService = localizationService;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var payInStorePaymentSettings = _settingService.LoadSetting<PayInStorePaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.DescriptionText = payInStorePaymentSettings.DescriptionText;
            model.AdditionalFee = payInStorePaymentSettings.AdditionalFee;
			model.AdditionalFeePercentage = payInStorePaymentSettings.AdditionalFeePercentage;

			var storeDependingSettings = new StoreDependingSettingHelper(ViewData);
			storeDependingSettings.GetOverrideKeys(payInStorePaymentSettings, model, storeScope, _settingService);
            
            return View("SmartStore.Plugin.Payments.PayInStore.Views.PaymentPayInStore.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
                return Configure();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var payInStorePaymentSettings = _settingService.LoadSetting<PayInStorePaymentSettings>(storeScope);

            //save settings
            payInStorePaymentSettings.DescriptionText = model.DescriptionText;
            payInStorePaymentSettings.AdditionalFee = model.AdditionalFee;
			payInStorePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

			var storeDependingSettings = new StoreDependingSettingHelper(ViewData);
			storeDependingSettings.UpdateSettings(payInStorePaymentSettings, form, storeScope, _settingService);

			//now clear settings cache
			_settingService.ClearCache();
            
            return View("SmartStore.Plugin.Payments.PayInStore.Views.PaymentPayInStore.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
			var payInStorePaymentSettings = _settingService.LoadSetting<PayInStorePaymentSettings>(_storeContext.CurrentStore.Id);

            var model = new PaymentInfoModel();

            string desc = payInStorePaymentSettings.DescriptionText;

            if (desc.StartsWith("@"))
            {
                model.DescriptionText = _localizationService.GetResource(desc.Substring(1));
            }
            else
            {
                model.DescriptionText = payInStorePaymentSettings.DescriptionText;
            }

            return View("SmartStore.Plugin.Payments.PayInStore.Views.PaymentPayInStore.PaymentInfo", model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }
    }
}