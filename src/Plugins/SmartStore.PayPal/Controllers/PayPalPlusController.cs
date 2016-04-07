using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.PayPal.Controllers
{
	public class PayPalPlusController : PayPalControllerBase<PayPalPlusPaymentSettings>
	{
		public PayPalPlusController(
			IPaymentService paymentService,
			IOrderService orderService,
			IOrderProcessingService orderProcessingService) : base(
				PayPalPlusProvider.SystemName,
				paymentService,
				orderService,
				orderProcessingService)
		{
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

		[AdminAuthorize, ChildActionOnly]
		public ActionResult Configure()
		{
			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(storeScope);

			var model = new PayPalPlusConfigurationModel
			{
				ConfigGroups = T("Plugins.SmartStore.PayPal.ConfigGroups").Text.SplitSafe(";")
			};

			model.TransactModeValues = new SelectList(new List<object>
			{
				new { ID = (int)TransactMode.Authorize, Name = T("Plugins.SmartStore.PayPal.ModeAuth") },
				new { ID = (int)TransactMode.AuthorizeAndCapture, Name = T("Plugins.SmartStore.PayPal.ModeAuthAndCapture") }
			},
			"ID", "Name", (int)settings.TransactMode);

			model.AvailableSecurityProtocols = GetSecurityProtocols()
				.Select(x => new SelectListItem { Value = ((int)x.Key).ToString(), Text = x.Value })
				.ToList();

			model.Copy(settings, true);

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, Services.Settings);

			return View(model);
		}

		[HttpPost, AdminAuthorize, ChildActionOnly]
		public ActionResult Configure(PayPalPlusConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return Configure();

			ModelState.Clear();

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(storeScope);

			model.Copy(settings, false);

			storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);

			// multistore context not possible, see IPN handling
			Services.Settings.SaveSetting(settings, x => x.UseSandbox, 0, false);

			Services.Settings.ClearCache();
			NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

			return Configure();
		}

	}
}