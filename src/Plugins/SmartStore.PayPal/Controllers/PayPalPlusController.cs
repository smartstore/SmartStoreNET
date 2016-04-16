using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Services;
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
		private readonly HttpContextBase _httpContext;
		private readonly IPayPalService _payPalService;

		public PayPalPlusController(
			HttpContextBase httpContext,
			IPaymentService paymentService,
			IOrderService orderService,
			IOrderProcessingService orderProcessingService,
			IPayPalService payPalService) : base(
				PayPalPlusProvider.SystemName,
				paymentService,
				orderService,
				orderProcessingService)
		{
			_httpContext = httpContext;
			_payPalService = payPalService;
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

		[AdminAuthorize]
		public ActionResult UpsertExperienceProfile(string profileId)
		{
			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(storeScope);

			var store = Services.StoreService.GetStoreById(storeScope == 0 ? Services.StoreContext.CurrentStore.Id : storeScope);
			var session = new PayPalSessionData();

			var result = _payPalService.EnsureAccessToken(session, settings);
			if (result.Success)
			{
				result = _payPalService.UpsertCheckoutExperience(settings, session, store, profileId);
				if (result.Success && result.Id.HasValue())
				{
					settings.ExperienceProfileId = result.Id;

					Services.Settings.SaveSetting(settings, storeScope);
				}
				else
				{
					NotifyError(result.ErrorMessage);
				}
			}
			else
			{
				NotifyError(result.ErrorMessage);
			}

			return RedirectToAction("ConfigureProvider", "Plugin", new { area = "admin", systemName = PayPalPlusProvider.SystemName });
		}

		public ActionResult PaymentInfo()
		{
			return new EmptyResult();
		}

		public ActionResult PaymentWall()
		{
			var store = Services.StoreContext.CurrentStore;
			var customer = Services.WorkContext.CurrentCustomer;
			var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(store.Id);

			var model = new PayPalPlusCheckoutModel();
			var state = _httpContext.GetCheckoutState();

			if (!state.CustomProperties.ContainsKey(PayPalPlusProvider.SystemName))
				state.CustomProperties.Add(PayPalPlusProvider.SystemName, new PayPalSessionData());

			var session = state.CustomProperties[PayPalPlusProvider.SystemName] as PayPalSessionData;

			model.UseSandbox = settings.UseSandbox;
			model.HasPaymentFee = (settings.AdditionalFee > decimal.Zero);
			model.LanguageCulture = (Services.WorkContext.WorkingLanguage.LanguageCulture ?? "de_DE").Replace("-", "_");

			if (customer.BillingAddress != null && customer.BillingAddress.Country != null)
			{
				model.BillingAddressCountryCode = customer.BillingAddress.Country.TwoLetterIsoCode;
			}

			if (session.PaymentId.IsEmpty() || session.ApprovalUrl.IsEmpty())
			{
				var result = _payPalService.EnsureAccessToken(session, settings);
				if (result.Success)
				{
					var protocol = (store.SslEnabled ? "https" : "http");
					var returnUrl = Url.Action("CheckoutReturn", "PayPalPlus", new { area = Plugin.SystemName }, protocol);
					var cancelUrl = Url.Action("CheckoutCancel", "PayPalPlus", new { area = Plugin.SystemName }, protocol);

					result = _payPalService.CreatePayment(settings, session, PayPalPlusProvider.SystemName, returnUrl, cancelUrl);
					if (result.Success && result.Json != null)
					{
						foreach (var link in result.Json.links)
						{
							if (((string)link.rel).IsCaseInsensitiveEqual("approval_url"))
							{
								session.PaymentId = result.Id;
								session.ApprovalUrl = link.href;
								break;
							}
						}
					}
					else
					{
						model.ErrorMessage = result.ErrorMessage;
					}
				}
				else
				{
					model.ErrorMessage = result.ErrorMessage;
				}
			}

			model.ApprovalUrl = session.ApprovalUrl;

			return View(model);
		}

		public ActionResult CheckoutReturn()
		{
			return RedirectToAction("Confirm", "Checkout", new { area = "" });
		}

		public ActionResult CheckoutCancel()
		{
			return RedirectToAction("Confirm", "Checkout", new { area = "" });
		}
	}
}