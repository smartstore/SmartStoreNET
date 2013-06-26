using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Plugin.Payments.Prepayment.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.Plugin.Payments.Prepayment.Controllers
{
    public class PaymentPrepaymentController : PaymentControllerBase
    {
		private readonly IWorkContext _workContext;
		private readonly IStoreService _storeService;
		private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

		public PaymentPrepaymentController(IWorkContext workContext,
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
			var prepaymentPaymentSettings = _settingService.LoadSetting<PrepaymentPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.DescriptionText = prepaymentPaymentSettings.DescriptionText;
            model.AdditionalFee = prepaymentPaymentSettings.AdditionalFee;
			model.AdditionalFeePercentage = prepaymentPaymentSettings.AdditionalFeePercentage;

			var storeDependingSettings = new StoreDependingSettingHelper(ViewData);
			storeDependingSettings.GetOverrideKeys(prepaymentPaymentSettings, model, storeScope, _settingService);
            
            return View("SmartStore.Plugin.Payments.Prepayment.Views.PaymentPrepayment.Configure", model);
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
			var prepaymentPaymentSettings = _settingService.LoadSetting<PrepaymentPaymentSettings>(storeScope);

            //save settings
            prepaymentPaymentSettings.DescriptionText = model.DescriptionText;
            prepaymentPaymentSettings.AdditionalFee = model.AdditionalFee;
			prepaymentPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

			var storeDependingSettings = new StoreDependingSettingHelper(ViewData);
			storeDependingSettings.UpdateSettings(prepaymentPaymentSettings, form, storeScope, _settingService);

			//now clear settings cache
			_settingService.ClearCache();
            
            return View("SmartStore.Plugin.Payments.Prepayment.Views.PaymentPrepayment.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
			var prepaymentPaymentSettings = _settingService.LoadSetting<PrepaymentPaymentSettings>(_storeContext.CurrentStore.Id);

            var model = new PaymentInfoModel();

            string desc = prepaymentPaymentSettings.DescriptionText;

            if( desc.StartsWith("@") )
            {
                model.DescriptionText = _localizationService.GetResource(desc.Substring(1));
            } 
            else  {
                model.DescriptionText = prepaymentPaymentSettings.DescriptionText;
            }

            return View("SmartStore.Plugin.Payments.Prepayment.Views.PaymentPrepayment.PaymentInfo", model);
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