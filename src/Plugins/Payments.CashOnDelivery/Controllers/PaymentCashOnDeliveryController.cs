using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Plugin.Payments.CashOnDelivery.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.Payments.CashOnDelivery.Controllers
{
    public class PaymentCashOnDeliveryController : PaymentControllerBase
    {
		private readonly ISettingService _settingService;
		private readonly CashOnDeliveryPaymentSettings _cashOnDeliveryPaymentSettings;
		private readonly ILocalizationService _localizationService;

		public PaymentCashOnDeliveryController(ISettingService settingService,
			CashOnDeliveryPaymentSettings cashOnDeliveryPaymentSettings,
			ILocalizationService localizationService)
		{
			this._settingService = settingService;
			this._cashOnDeliveryPaymentSettings = cashOnDeliveryPaymentSettings;
			_localizationService = localizationService;
		}

        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
			var model = new ConfigurationModel();
			model.DescriptionText = _cashOnDeliveryPaymentSettings.DescriptionText;
			model.AdditionalFee = _cashOnDeliveryPaymentSettings.AdditionalFee;
			model.AdditionalFeePercentage = _cashOnDeliveryPaymentSettings.AdditionalFeePercentage;
            
            return View("SmartStore.Plugin.Payments.CashOnDelivery.Views.PaymentCashOnDelivery.Configure", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
		[ValidateInput(false)]
        public ActionResult Configure(ConfigurationModel model, FormCollection form)
        {
            if (!ModelState.IsValid)
                return Configure();

			//save settings
			_cashOnDeliveryPaymentSettings.DescriptionText = model.DescriptionText;
			_cashOnDeliveryPaymentSettings.AdditionalFee = model.AdditionalFee;
			_cashOnDeliveryPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
			_settingService.SaveSetting(_cashOnDeliveryPaymentSettings);
            
            return View("SmartStore.Plugin.Payments.CashOnDelivery.Views.PaymentCashOnDelivery.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();
            string desc = _cashOnDeliveryPaymentSettings.DescriptionText;

            if (desc.StartsWith("@"))
            {
                model.DescriptionText = _localizationService.GetResource(desc.Substring(1));
            } 
            else  
            {
                model.DescriptionText = _cashOnDeliveryPaymentSettings.DescriptionText;
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