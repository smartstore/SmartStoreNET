using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Plugin.Payments.DirectDebit.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.Payments.DirectDebit.Controllers
{
    public class PaymentDirectDebitController : PaymentControllerBase
    {
		private readonly ISettingService _settingService;
		private readonly DirectDebitPaymentSettings _directDebitPaymentSettings;
		private readonly ILocalizationService _localizationService;

		public PaymentDirectDebitController(ISettingService settingService,
			DirectDebitPaymentSettings directDebitPaymentSettings,
			ILocalizationService localizationService)
		{
			this._settingService = settingService;
			this._directDebitPaymentSettings = directDebitPaymentSettings;
			this._localizationService = localizationService;
		}

        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
			var model = new ConfigurationModel();
			model.DescriptionText = _directDebitPaymentSettings.DescriptionText;
			model.AdditionalFee = _directDebitPaymentSettings.AdditionalFee;
			model.AdditionalFeePercentage = _directDebitPaymentSettings.AdditionalFeePercentage;
            
            return View("SmartStore.Plugin.Payments.DirectDebit.Views.PaymentDirectDebit.Configure", model);
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
			_directDebitPaymentSettings.DescriptionText = model.DescriptionText;
			_directDebitPaymentSettings.AdditionalFee = model.AdditionalFee;
			_directDebitPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
			_settingService.SaveSetting(_directDebitPaymentSettings);
            
            return View("SmartStore.Plugin.Payments.DirectDebit.Views.PaymentDirectDebit.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
			var model = new PaymentInfoModel();
            string desc = _directDebitPaymentSettings.DescriptionText;

            if( desc.StartsWith("@") )
            {
                model.DescriptionText = _localizationService.GetResource(desc.Substring(1));
            } 
            else  {
                model.DescriptionText = _directDebitPaymentSettings.DescriptionText;
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