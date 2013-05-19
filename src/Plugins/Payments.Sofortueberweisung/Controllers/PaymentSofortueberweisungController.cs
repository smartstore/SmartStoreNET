using System;
using System.Web;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Services.Configuration;
using SmartStore.Services.Logging;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Plugin.Payments.Sofortueberweisung.Models;
using SmartStore.Plugin.Payments.Sofortueberweisung;
using System.Reflection;
using System.IO;
using System.Web.Security;
using SmartStore.Plugin.Payments.Sofortueberweisung.Core;
using System.Xml;
using System.Xml.Linq;

namespace SmartStore.Plugin.Payments.Sofortueberweisung.Controllers
{
	public class PaymentSofortueberweisungController : PaymentControllerBase
	{
		private readonly ISofortueberweisungApi _api;
		private readonly ISettingService _settingService;
		private readonly SofortueberweisungPaymentSettings _paymentSettingsSu;

		public PaymentSofortueberweisungController(
			ISofortueberweisungApi api,
			ISettingService settingService,
			SofortueberweisungPaymentSettings paymentSettingsSu) {

			_api = api;
			_settingService = settingService;
			_paymentSettingsSu = paymentSettingsSu;
		}

		[NonAction]
		public override IList<string> ValidatePaymentForm(FormCollection form) {
			List<string> warnings = new List<string>();
			return warnings;
		}

		[NonAction]
		public override ProcessPaymentRequest GetPaymentInfo(FormCollection form) {
			var processPaymentRequest = new ProcessPaymentRequest();
			return processPaymentRequest;
		}

		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult Configure() {
			ConfigurationModel model = new ConfigurationModel();
			model.Copy(_paymentSettingsSu, true);
			
			return View("SmartStore.Plugin.Payments.Sofortueberweisung.Views.PaymentSofortueberweisung.Configure", model);
		}

		[HttpPost]
		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult Configure(ConfigurationModel model) {
			if (!ModelState.IsValid)
				return Configure();

			model.Copy(_paymentSettingsSu, false);
			_settingService.SaveSetting(_paymentSettingsSu);

			return View("SmartStore.Plugin.Payments.Sofortueberweisung.Views.PaymentSofortueberweisung.Configure", model);
		}

		[ChildActionOnly]
		public ActionResult PaymentInfo() {
			return View("SmartStore.Plugin.Payments.Sofortueberweisung.Views.PaymentSofortueberweisung.PaymentInfo");
		}

		public ActionResult Success() {
			// doesn't serve any content here
			return RedirectToRoute("CheckoutCompleted");
		}
		public ActionResult Abort() {
			return RedirectToAction("Index", "Home");
		}
		public ActionResult Notification() {
			_api.PaymentDetails(Request);

			return Content("");
		}
	}	// class
}

