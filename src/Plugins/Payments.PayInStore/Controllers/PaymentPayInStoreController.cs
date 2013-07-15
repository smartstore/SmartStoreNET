using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Plugin.Payments.PayInStore.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.Payments.PayInStore.Controllers
{
    public class PaymentPayInStoreController : PaymentControllerBase
    {
		private readonly ISettingService _settingService;
		private readonly PayInStorePaymentSettings _payInStorePaymentSettings;
		private readonly ILocalizationService _localizationService;

		public PaymentPayInStoreController(ISettingService settingService,
			PayInStorePaymentSettings payInStorePaymentSettings,
			ILocalizationService localizationService)
		{
			this._settingService = settingService;
			this._payInStorePaymentSettings = payInStorePaymentSettings;
			this._localizationService = localizationService;
		}

        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
			var model = new ConfigurationModel();
			model.DescriptionText = _payInStorePaymentSettings.DescriptionText;
			model.AdditionalFee = _payInStorePaymentSettings.AdditionalFee;
			model.AdditionalFeePercentage = _payInStorePaymentSettings.AdditionalFeePercentage;
            
            return View("SmartStore.Plugin.Payments.PayInStore.Views.PaymentPayInStore.Configure", model);
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
			_payInStorePaymentSettings.DescriptionText = model.DescriptionText;
			_payInStorePaymentSettings.AdditionalFee = model.AdditionalFee;
			_payInStorePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
			_settingService.SaveSetting(_payInStorePaymentSettings);
            
            return View("SmartStore.Plugin.Payments.PayInStore.Views.PaymentPayInStore.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();

            string desc = _payInStorePaymentSettings.DescriptionText;

            if (desc.StartsWith("@"))
            {
                model.DescriptionText = _localizationService.GetResource(desc.Substring(1));
            }
            else
            {
                model.DescriptionText = _payInStorePaymentSettings.DescriptionText;
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