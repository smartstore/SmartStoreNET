using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Html;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.PayPal.Validators;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Payments;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.Theming;

namespace SmartStore.PayPal.Controllers
{
	public class PayPalPlusController : PayPalRestApiControllerBase<PayPalPlusPaymentSettings>
	{
		private readonly HttpContextBase _httpContext;
		private readonly PluginMediator _pluginMediator;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly IPaymentService _paymentService;
		private readonly ITaxService _taxService;
		private readonly ICurrencyService _currencyService;
		private readonly IPriceFormatter _priceFormatter;

		public PayPalPlusController(
			HttpContextBase httpContext,
			PluginMediator pluginMediator,
			IPayPalService payPalService,
			IGenericAttributeService genericAttributeService,
			IPaymentService paymentService,
			ITaxService taxService,
			ICurrencyService currencyService,
			IPriceFormatter priceFormatter) : base(
				PayPalPlusProvider.SystemName,
				payPalService)
		{
			_httpContext = httpContext;
			_pluginMediator = pluginMediator;
			_genericAttributeService = genericAttributeService;
			_paymentService = paymentService;
			_taxService = taxService;
			_currencyService = currencyService;
			_priceFormatter = priceFormatter;
		}

		private string GetPaymentMethodName(Provider<IPaymentMethod> provider)
		{
			if (provider != null)
			{
				var name = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata);

				if (name.IsEmpty())
					name = provider.Metadata.FriendlyName;

				if (name.IsEmpty())
					name = provider.Metadata.SystemName;

				return name;
			}
			return "";
		}

		private string GetPaymentFee(Provider<IPaymentMethod> provider, List<OrganizedShoppingCartItem> cart)
		{
			var paymentMethodAdditionalFee = provider.Value.GetAdditionalHandlingFee(cart);
			var rateBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, Services.WorkContext.CurrentCustomer);
			var rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, Services.WorkContext.WorkingCurrency);

			if (rate != decimal.Zero)
			{
				return _priceFormatter.FormatPaymentMethodAdditionalFee(rate, true);
			}
			return "";
		}

		private PayPalPlusCheckoutModel.ThirdPartyPaymentMethod GetThirdPartyPaymentMethodModel(
			Provider<IPaymentMethod> provider,
			PayPalPlusPaymentSettings settings,
			Store store)
		{
			var model = new PayPalPlusCheckoutModel.ThirdPartyPaymentMethod();
			model.MethodName = GetPaymentMethodName(provider);
			model.RedirectUrl = Url.Action("CheckoutReturn", "PayPalPlus", new { area = Plugin.SystemName, systemName = provider.Metadata.SystemName }, store.SslEnabled ? "https" : "http");

			try
			{
				if (settings.DisplayPaymentMethodDescription)
				{
					// not the short description, the full description is intended for frontend
					var paymentMethod = _paymentService.GetPaymentMethodBySystemName(provider.Metadata.SystemName);
					if (paymentMethod != null)
					{
						string description = paymentMethod.GetLocalized(x => x.FullDescription);
						if (description.HasValue())
						{
							description = HtmlUtils.ConvertHtmlToPlainText(description);
							description = HtmlUtils.StripTags(HttpUtility.HtmlDecode(description));

							if (description.HasValue())
								model.Description = description.EncodeJsString();
						}
					}
				}
			}
			catch { }

			try
			{
				if (settings.DisplayPaymentMethodLogo && provider.Metadata.PluginDescriptor != null && store.SslEnabled)
				{
					var brandImageUrl = _pluginMediator.GetBrandImageUrl(provider.Metadata);
					if (brandImageUrl.HasValue())
					{
						if (brandImageUrl.StartsWith("~"))
							brandImageUrl = brandImageUrl.Substring(1);

						var uri = new UriBuilder(Uri.UriSchemeHttps, Request.Url.Host, -1, brandImageUrl);
						model.ImageUrl = uri.ToString();
					}
				}
			}
			catch { }

			return model;
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

		[LoadSetting, AdminAuthorize, ChildActionOnly, AdminThemed]
		public ActionResult Configure(PayPalPlusPaymentSettings settings, int storeScope)
		{
			var model = new PayPalPlusConfigurationModel
			{
				ConfigGroups = T("Plugins.SmartStore.PayPal.ConfigGroups").Text.SplitSafe(";")
			};

			// It's better to also offer inactive methods here but filter them out in frontend.
			var paymentMethods = _paymentService.LoadAllPaymentMethods(storeScope);

			model.Copy(settings, true);
			PrepareConfigurationModel(model, storeScope);

			model.AvailableThirdPartyPaymentMethods = paymentMethods
				.Where(x =>
					x.Metadata.PluginDescriptor.SystemName != Plugin.SystemName &&
					!x.Value.RequiresInteraction &&
					(x.Metadata.PluginDescriptor.SystemName == "SmartStore.OfflinePayment" || x.Value.PaymentMethodType == PaymentMethodType.Redirection))
				.ToSelectListItems(_pluginMediator, model.ThirdPartyPaymentMethods.ToArray());

			return View(model);
		}

		[HttpPost, AdminAuthorize, ChildActionOnly, AdminThemed]
		public ActionResult Configure(PayPalPlusConfigurationModel model, FormCollection form)
		{
			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(storeScope);

			var oldClientId = settings.ClientId;
			var oldSecret = settings.Secret;
			var oldProfileId = settings.ExperienceProfileId;

			var validator = new PayPalPlusConfigValidator(Services.Localization, x =>
			{
				return storeScope == 0 || storeDependingSettingHelper.IsOverrideChecked(settings, x, form);
			});

			validator.Validate(model, ModelState);

			if (!ModelState.IsValid)
			{
				return Configure(settings, storeScope);
			}

			ModelState.Clear();
			model.Copy(settings, false);

			// Credentials changed: reset profile and webhook id to avoid errors.
			if (!oldClientId.IsCaseInsensitiveEqual(settings.ClientId) || !oldSecret.IsCaseInsensitiveEqual(settings.Secret))
			{
				if (oldProfileId.IsCaseInsensitiveEqual(settings.ExperienceProfileId))
					settings.ExperienceProfileId = null;

				settings.WebhookId = null;
			}

			using (Services.Settings.BeginScope())
			{
				storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);
			}

			using (Services.Settings.BeginScope())
			{
				// Multistore context not possible, see IPN handling.
				Services.Settings.SaveSetting(settings, x => x.UseSandbox, 0, false);
			}

			NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

			return RedirectToConfiguration(PayPalPlusProvider.SystemName, false);
		}

		public ActionResult PaymentInfo()
		{
			return new EmptyResult();
		}

		public ActionResult PaymentWall()
		{
			var sb = new StringBuilder();
			var store = Services.StoreContext.CurrentStore;
			var customer = Services.WorkContext.CurrentCustomer;
			var language = Services.WorkContext.WorkingLanguage;
			var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(store.Id);
			var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

			var pppMethod = _paymentService.GetPaymentMethodBySystemName(PayPalPlusProvider.SystemName);
			var pppProvider = _paymentService.LoadPaymentMethodBySystemName(PayPalPlusProvider.SystemName, false, store.Id);

			var methods = _paymentService.LoadActivePaymentMethods(customer, cart, store.Id, null, false);
			var session = _httpContext.GetPayPalSessionData();

			var model = new PayPalPlusCheckoutModel();
			model.ThirdPartyPaymentMethods = new List<PayPalPlusCheckoutModel.ThirdPartyPaymentMethod>();
			model.UseSandbox = settings.UseSandbox;
			model.LanguageCulture = (language.LanguageCulture ?? "de_DE").Replace("-", "_");
			model.PayPalPlusPseudoMessageFlag = TempData["PayPalPlusPseudoMessageFlag"] as string;
			model.PayPalFee = GetPaymentFee(pppProvider, cart);
			model.HasAnyFees = model.PayPalFee.HasValue();

			if (pppMethod != null)
			{
				model.FullDescription = pppMethod.GetLocalized(x => x.FullDescription, language);
			}

			if (customer.BillingAddress != null && customer.BillingAddress.Country != null)
			{
				model.BillingAddressCountryCode = customer.BillingAddress.Country.TwoLetterIsoCode;
			}

			foreach (var systemName in settings.ThirdPartyPaymentMethods)
			{
				var provider = methods.FirstOrDefault(x => x.Metadata.SystemName == systemName);
				if (provider != null)
				{
					var methodModel = GetThirdPartyPaymentMethodModel(provider, settings, store);
					model.ThirdPartyPaymentMethods.Add(methodModel);

					var fee = GetPaymentFee(provider, cart);
					if (fee.HasValue())
						model.HasAnyFees = true;
					if (sb.Length > 0)
						sb.Append(", ");
					sb.AppendFormat("['{0}','{1}']", methodModel.MethodName.Replace("'", ""), fee);
				}
			}

			model.ThirdPartyFees = sb.ToString();

			// we must create a new paypal payment each time the payment wall is rendered because otherwise patch payment can fail
			// with "Item amount must add up to specified amount subtotal (or total if amount details not specified)".
			session.PaymentId = null;
			session.ApprovalUrl = null;

			var result = PayPalService.EnsureAccessToken(session, settings);
			if (result.Success)
			{
				var protocol = (store.SslEnabled ? "https" : "http");
				var returnUrl = Url.Action("CheckoutReturn", "PayPalPlus", new { area = Plugin.SystemName }, protocol);
				var cancelUrl = Url.Action("CheckoutCancel", "PayPalPlus", new { area = Plugin.SystemName }, protocol);

				result = PayPalService.CreatePayment(settings, session, cart, PayPalPlusProvider.SystemName, returnUrl, cancelUrl);
				if (result == null)
				{
					return RedirectToAction("Confirm", "Checkout", new { area = "" });
				}

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

			model.ApprovalUrl = session.ApprovalUrl;

			if (session.SessionExpired)
			{
				// Customer has been redirected because the session expired.
				session.SessionExpired = false;
				NotifyInfo(T("Plugins.SmartStore.PayPal.SessionExpired"));
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult PatchShipping()
		{
			var session = HttpContext.GetPayPalSessionData();
			if (session.AccessToken.IsEmpty() || session.PaymentId.IsEmpty())
			{
				// Session expired. Reload payment wall and create new payment (we need the payment id).				
				session.SessionExpired = true;

				return new JsonResult { Data = new { success = false, error = string.Empty, reload = true } };
			}

			var store = Services.StoreContext.CurrentStore;
			var customer = Services.WorkContext.CurrentCustomer;
			var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(store.Id);
			var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

			var result = PayPalService.PatchShipping(settings, session, cart, PayPalPlusProvider.SystemName);
			var errorMessage = result.ErrorMessage;

			if (!result.Success && result.IsValidationError)
			{
				errorMessage = string.Concat(T("Plugins.SmartStore.PayPal.PayPalValidationFailure"), "\r\n", errorMessage);
			}

			return new JsonResult { Data = new { success = result.Success, error = errorMessage, reload = false } };
		}

		public ActionResult CheckoutCompleted()
		{
			var instruct = _httpContext.Session[PayPalPlusProvider.CheckoutCompletedKey] as string;

			if (instruct.HasValue())
			{
				return Content(instruct);
			}

			return new EmptyResult();
		}

		[ValidateInput(false)]
		public ActionResult CheckoutReturn(string systemName, string paymentId, string PayerID)
		{
			// Request.QueryString:
			// paymentId: PAY-0TC88803RP094490KK4KM6AI, token (not the access token): EC-5P379249AL999154U, PayerID: 5L9K773HHJLPN

			var customer = Services.WorkContext.CurrentCustomer;
			var store = Services.StoreContext.CurrentStore;
			var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(store.Id);
			var session = _httpContext.GetPayPalSessionData();

			if (systemName.IsEmpty())
				systemName = PayPalPlusProvider.SystemName;

			if (paymentId.HasValue() && session.PaymentId.IsEmpty())
				session.PaymentId = paymentId;

			session.PayerId = PayerID;

			_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, systemName, store.Id);

			var paymentRequest = _httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
			if (paymentRequest == null)
			{
				_httpContext.Session["OrderPaymentInfo"] = new ProcessPaymentRequest
				{
					PaymentMethodSystemName = systemName
				};
			}

			return RedirectToAction("Confirm", "Checkout", new { area = "" });
		}

		[ValidateInput(false)]
		public ActionResult CheckoutCancel()
		{
			// Request.QueryString:
			// token: EC-6JM38216F6718012L, ppp_msg: 1

			// undocumented
			var pseudoMessageFlag = Request.QueryString["ppp_msg"] as string;

			if (pseudoMessageFlag.HasValue())
			{
				TempData["PayPalPlusPseudoMessageFlag"] = pseudoMessageFlag;
			}

			// back to where he came from
			return RedirectToAction("PaymentMethod", "Checkout", new { area = "" });
		}
	}
}