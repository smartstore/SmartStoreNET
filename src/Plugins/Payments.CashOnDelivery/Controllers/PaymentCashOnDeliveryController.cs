using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Plugin.Payments.CashOnDelivery.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.Plugin.Payments.CashOnDelivery.Controllers
{
    public class PaymentCashOnDeliveryController : PaymentControllerBase
    {
		private readonly IWorkContext _workContext;
		private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
		private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;

		public PaymentCashOnDeliveryController(IWorkContext workContext,
			IStoreService storeService, 
			ISettingService settingService,
			IStoreContext storeContext,
            ILocalizationService localizationService)
        {
			this._workContext = workContext;
			this._storeService = storeService;
            this._settingService = settingService;
			this._storeContext = storeContext;
            this._localizationService = localizationService;
        }
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var cashOnDeliveryPaymentSettings = _settingService.LoadSetting<CashOnDeliveryPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.DescriptionText = cashOnDeliveryPaymentSettings.DescriptionText;
            model.AdditionalFee = cashOnDeliveryPaymentSettings.AdditionalFee;
			model.AdditionalFeePercentage = cashOnDeliveryPaymentSettings.AdditionalFeePercentage;

			var storeDependingSettings = new StoreDependingSettingHelper(ViewData);
			storeDependingSettings.GetOverrideKeys(cashOnDeliveryPaymentSettings, model, storeScope, _settingService);
            
            return View("SmartStore.Plugin.Payments.CashOnDelivery.Views.PaymentCashOnDelivery.Configure", model);
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
			var cashOnDeliveryPaymentSettings = _settingService.LoadSetting<CashOnDeliveryPaymentSettings>(storeScope);
            
            //save settings
            cashOnDeliveryPaymentSettings.DescriptionText = model.DescriptionText;
            cashOnDeliveryPaymentSettings.AdditionalFee = model.AdditionalFee;
			cashOnDeliveryPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

			var storeDependingSettings = new StoreDependingSettingHelper(ViewData);
			storeDependingSettings.UpdateSettings(cashOnDeliveryPaymentSettings, form, storeScope, _settingService);

			//now clear settings cache
			_settingService.ClearCache();
            
            return View("SmartStore.Plugin.Payments.CashOnDelivery.Views.PaymentCashOnDelivery.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
			var cashOnDeliveryPaymentSettings = _settingService.LoadSetting<CashOnDeliveryPaymentSettings>(_storeContext.CurrentStore.Id);

            var model = new PaymentInfoModel();
            string desc = cashOnDeliveryPaymentSettings.DescriptionText;

            if (desc.StartsWith("@"))
            {
                model.DescriptionText = _localizationService.GetResource(desc.Substring(1));
            } 
            else  
            {
                model.DescriptionText = cashOnDeliveryPaymentSettings.DescriptionText;
            }

            return View("SmartStore.Plugin.Payments.CashOnDelivery.Views.PaymentCashOnDelivery.PaymentInfo", model);
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