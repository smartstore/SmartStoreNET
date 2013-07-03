using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Plugin.Payments.Prepayment.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.Payments.Prepayment.Controllers
{
    public class PaymentPrepaymentController : PaymentControllerBase
    {
		private readonly ISettingService _settingService;
		private readonly PrepaymentPaymentSettings _prepaymentPaymentSettings;
		private readonly ILocalizationService _localizationService;

		public PaymentPrepaymentController(ISettingService settingService,
			PrepaymentPaymentSettings prepaymentPaymentSettings,
			ILocalizationService localizationService)
		{
			this._settingService = settingService;
			this._prepaymentPaymentSettings = prepaymentPaymentSettings;
			this._localizationService = localizationService;
		}
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
			var model = new ConfigurationModel();
			model.DescriptionText = _prepaymentPaymentSettings.DescriptionText;
			model.AdditionalFee = _prepaymentPaymentSettings.AdditionalFee;
			model.AdditionalFeePercentage = _prepaymentPaymentSettings.AdditionalFeePercentage;
            
            return View("SmartStore.Plugin.Payments.Prepayment.Views.PaymentPrepayment.Configure", model);
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
			_prepaymentPaymentSettings.DescriptionText = model.DescriptionText;
			_prepaymentPaymentSettings.AdditionalFee = model.AdditionalFee;
			_prepaymentPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
			_settingService.SaveSetting(_prepaymentPaymentSettings);
            
            return View("SmartStore.Plugin.Payments.Prepayment.Views.PaymentPrepayment.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();

			string desc = _prepaymentPaymentSettings.DescriptionText;

            if( desc.StartsWith("@") )
            {
                model.DescriptionText = _localizationService.GetResource(desc.Substring(1));
            } 
            else  
			{
				model.DescriptionText = _prepaymentPaymentSettings.DescriptionText;
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