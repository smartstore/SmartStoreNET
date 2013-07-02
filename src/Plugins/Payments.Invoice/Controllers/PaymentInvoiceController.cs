using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Plugin.Payments.Invoice.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.Payments.Invoice.Controllers
{
    public class PaymentInvoiceController : PaymentControllerBase
    {
		private readonly ISettingService _settingService;
		private readonly InvoicePaymentSettings _invoicePaymentSettings;
		private readonly ILocalizationService _localizationService;

		public PaymentInvoiceController(ISettingService settingService,
			InvoicePaymentSettings invoicePaymentSettings,
			ILocalizationService localizationService)
		{
			this._settingService = settingService;
			this._invoicePaymentSettings = invoicePaymentSettings;
			_localizationService = localizationService;
		}
        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
			var model = new ConfigurationModel();
			model.DescriptionText = _invoicePaymentSettings.DescriptionText;
			model.AdditionalFee = _invoicePaymentSettings.AdditionalFee;
			model.AdditionalFeePercentage = _invoicePaymentSettings.AdditionalFeePercentage;
            
            return View("SmartStore.Plugin.Payments.Invoice.Views.PaymentInvoice.Configure", model);
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
			_invoicePaymentSettings.DescriptionText = model.DescriptionText;
			_invoicePaymentSettings.AdditionalFee = model.AdditionalFee;
			_invoicePaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
			_settingService.SaveSetting(_invoicePaymentSettings);

            return View("SmartStore.Plugin.Payments.Invoice.Views.PaymentInvoice.Configure", model);
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();

            string desc = _invoicePaymentSettings.DescriptionText;

            if( desc.StartsWith("@") )
            {
                model.DescriptionText = _localizationService.GetResource(desc.Substring(1));
            } 
            else  {
                model.DescriptionText = _invoicePaymentSettings.DescriptionText;
            }

            return View("SmartStore.Plugin.Payments.Invoice.Views.PaymentInvoice.PaymentInfo", model);
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