using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Plugin.Payments.DirectDebit.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.Plugin.Payments.DirectDebit.Controllers
{
    public class PaymentDirectDebitController : PaymentControllerBase
    {
		private readonly IWorkContext _workContext;
		private readonly IStoreService _storeService;
		private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;

		public PaymentDirectDebitController(IWorkContext workContext,
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
			var directDebitPaymentSettings = _settingService.LoadSetting<DirectDebitPaymentSettings>(storeScope);

            var model = new ConfigurationModel();
            model.DescriptionText = directDebitPaymentSettings.DescriptionText;
            model.AdditionalFee = directDebitPaymentSettings.AdditionalFee;
			model.AdditionalFeePercentage = directDebitPaymentSettings.AdditionalFeePercentage;

			var storeDependingSettings = new StoreDependingSettingHelper(ViewData);
			storeDependingSettings.GetOverrideKeys(directDebitPaymentSettings, model, storeScope, _settingService);
            
            return View("SmartStore.Plugin.Payments.DirectDebit.Views.PaymentDirectDebit.Configure", model);
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
			var directDebitPaymentSettings = _settingService.LoadSetting<DirectDebitPaymentSettings>(storeScope);

            //save settings
            directDebitPaymentSettings.DescriptionText = model.DescriptionText;
            directDebitPaymentSettings.AdditionalFee = model.AdditionalFee;
			directDebitPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

			var storeDependingSettings = new StoreDependingSettingHelper(ViewData);
			storeDependingSettings.UpdateSettings(directDebitPaymentSettings, form, storeScope, _settingService);

			//now clear settings cache
			_settingService.ClearCache();
            
            return View("SmartStore.Plugin.Payments.DirectDebit.Views.PaymentDirectDebit.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
			var directDebitPaymentSettings = _settingService.LoadSetting<DirectDebitPaymentSettings>(_storeContext.CurrentStore.Id);

            var model = new PaymentInfoModel();
            string desc = directDebitPaymentSettings.DescriptionText;

            if( desc.StartsWith("@") )
            {
                model.DescriptionText = _localizationService.GetResource(desc.Substring(1));
            } 
            else  {
                model.DescriptionText = directDebitPaymentSettings.DescriptionText;
            }

            return View("SmartStore.Plugin.Payments.DirectDebit.Views.PaymentDirectDebit.PaymentInfo", model);
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
            paymentInfo.DirectDebitAccountHolder = form["DirectDebitAccountHolder"];
            paymentInfo.DirectDebitAccountNumber = form["DirectDebitAccountNumber"];
            paymentInfo.DirectDebitBankCode = form["DirectDebitBankCode"];
            paymentInfo.DirectDebitBankName = form["DirectDebitBankName"];
            paymentInfo.DirectDebitBic = form["DirectDebitBic"];
            paymentInfo.DirectDebitCountry = form["DirectDebitCountry"];
            paymentInfo.DirectDebitIban = form["DirectDebitIban"];
            return paymentInfo;
        }

    }
}