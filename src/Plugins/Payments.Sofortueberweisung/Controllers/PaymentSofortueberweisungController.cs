using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Services.Configuration;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Plugin.Payments.Sofortueberweisung.Models;
using SmartStore.Plugin.Payments.Sofortueberweisung.Core;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.Plugin.Payments.Sofortueberweisung.Controllers
{
	public class PaymentSofortueberweisungController : PaymentControllerBase
	{
		private readonly ISofortueberweisungApi _api;
		private readonly IWorkContext _workContext;
		private readonly IStoreService _storeService;
		private readonly ISettingService _settingService;

		public PaymentSofortueberweisungController(
			ISofortueberweisungApi api,
			IWorkContext workContext,
			IStoreService storeService,
			ISettingService settingService)
		{
			_workContext = workContext;
			_storeService = storeService;
			_api = api;
			_settingService = settingService;
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
		public ActionResult Configure() 
		{
			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var sofortueberweisungPaymentSettings = _settingService.LoadSetting<SofortueberweisungPaymentSettings>(storeScope);

			ConfigurationModel model = new ConfigurationModel();
			model.Copy(sofortueberweisungPaymentSettings, true);

			var storeDependingSettings = new StoreDependingSettingHelper(ViewData);
			storeDependingSettings.GetOverrideKeys(sofortueberweisungPaymentSettings, model, storeScope, _settingService);

			return View("SmartStore.Plugin.Payments.Sofortueberweisung.Views.PaymentSofortueberweisung.Configure", model);
		}

		[HttpPost]
		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult Configure(ConfigurationModel model, FormCollection form) {
			if (!ModelState.IsValid)
				return Configure();

			//load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var sofortueberweisungPaymentSettings = _settingService.LoadSetting<SofortueberweisungPaymentSettings>(storeScope);

			model.Copy(sofortueberweisungPaymentSettings, false);

			var storeDependingSettings = new StoreDependingSettingHelper(ViewData);
			storeDependingSettings.UpdateSettings(sofortueberweisungPaymentSettings, form, storeScope, _settingService);

			//now clear settings cache
			_settingService.ClearCache();

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

