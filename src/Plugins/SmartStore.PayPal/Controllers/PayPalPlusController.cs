using System;
using System.Collections.Generic;
using System.Linq;
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
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.PayPal.Controllers
{
	public class PayPalPlusController : PaymentControllerBase
	{
		private readonly HttpContextBase _httpContext;
		private readonly PluginMediator _pluginMediator;
		private readonly IPayPalService _payPalService;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly IPaymentService _paymentService;

		public PayPalPlusController(
			HttpContextBase httpContext,
			IPaymentService paymentService,
			IOrderService orderService,
			IOrderProcessingService orderProcessingService,
			PluginMediator pluginMediator,
			IPayPalService payPalService,
			IGenericAttributeService genericAttributeService)
		{
			_httpContext = httpContext;
			_pluginMediator = pluginMediator;
			_payPalService = payPalService;
			_genericAttributeService = genericAttributeService;
			_paymentService = paymentService;
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

		private PayPalPlusCheckoutModel.ThirdPartyPaymentMethod GetThirdPartyPaymentMethodModel(
			Provider<IPaymentMethod> provider,
			PayPalPlusPaymentSettings settings,
			Store store)
		{
			var model = new PayPalPlusCheckoutModel.ThirdPartyPaymentMethod();
			model.MethodName = GetPaymentMethodName(provider).EncodeJsString();
			model.RedirectUrl = Url.Action("CheckoutReturn", "PayPalPlus", new { area = Plugin.SystemName, systemName = provider.Metadata.SystemName }, store.SslEnabled ? "https" : "http");

			try
			{
				if (settings.DisplayPaymentMethodDescription)
				{
					// not the short description, the full description is intended for frontend
					var paymentMethod = _paymentService.GetPaymentMethodBySystemName(provider.Metadata.SystemName);
					if (paymentMethod != null)
					{
						var description = paymentMethod.GetLocalized(x => x.FullDescription);
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

		[AdminAuthorize, ChildActionOnly]
		public ActionResult Configure()
		{
			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(storeScope);

			var model = new PayPalPlusConfigurationModel
			{
				ConfigGroups = T("Plugins.SmartStore.PayPal.ConfigGroups").Text.SplitSafe(";")
			};

			model.AvailableSecurityProtocols = PayPalService.GetSecurityProtocols()
				.Select(x => new SelectListItem { Value = ((int)x.Key).ToString(), Text = x.Value })
				.ToList();

			// it's better to also offer inactive methods here but filter them out in frontend
			var methods = _paymentService.LoadAllPaymentMethods(storeScope);

			model.AvailableThirdPartyPaymentMethods = methods
				.Where(x => 
					x.Metadata.PluginDescriptor.SystemName != Plugin.SystemName &&
					!x.Value.RequiresInteraction &&
					(x.Metadata.PluginDescriptor.SystemName == "SmartStore.OfflinePayment" || x.Value.PaymentMethodType == PaymentMethodType.Redirection))
				.Select(x => new SelectListItem { Value = x.Metadata.SystemName, Text = GetPaymentMethodName(x) })
				.ToList();


			model.Copy(settings, true);

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, Services.Settings);

			return View(model);
		}

		[HttpPost, AdminAuthorize, ChildActionOnly]
		public ActionResult Configure(PayPalPlusConfigurationModel model, FormCollection form)
		{
			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
			var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(storeScope);

			var validator = new PayPalPlusConfigValidator(Services.Localization, x =>
			{
				return storeScope == 0 || storeDependingSettingHelper.IsOverrideChecked(settings, x, form);
			});

			validator.Validate(model, ModelState);

			if (!ModelState.IsValid)
				return Configure();

			ModelState.Clear();

			model.Copy(settings, false);

			storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);

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
			var language = Services.WorkContext.WorkingLanguage;
			var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(store.Id);
			var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

			var methods = _paymentService.LoadActivePaymentMethods(customer, cart, store.Id, null, false);
			var session = _httpContext.GetPayPalSessionData();

			var model = new PayPalPlusCheckoutModel();
			model.ThirdPartyPaymentMethods = new List<PayPalPlusCheckoutModel.ThirdPartyPaymentMethod>();
			model.UseSandbox = settings.UseSandbox;
			model.HasPaymentFee = (settings.AdditionalFee > decimal.Zero);
			model.LanguageCulture = (language.LanguageCulture ?? "de_DE").Replace("-", "_");

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
				}
			}


			if (session.PaymentId.IsEmpty() || session.ApprovalUrl.IsEmpty())
			{
				var result = _payPalService.EnsureAccessToken(session, settings);
				if (result.Success)
				{
					var protocol = (store.SslEnabled ? "https" : "http");
					var returnUrl = Url.Action("CheckoutReturn", "PayPalPlus", new { area = Plugin.SystemName }, protocol);
					var cancelUrl = Url.Action("CheckoutCancel", "PayPalPlus", new { area = Plugin.SystemName }, protocol);

					result = _payPalService.CreatePayment(settings, session, cart, PayPalPlusProvider.SystemName, returnUrl, cancelUrl);
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

		[ValidateInput(false)]
		public ActionResult CheckoutReturn(string systemName, string paymentId, string PayerID)
		{
			// Request.QueryString:
			// paymentId: PAY-0TC88803RP094490KK4KM6AI, token: EC-5P379249AL999154U, PayerID: 5L9K773HHJLPN

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
			// back to where he came from
			return RedirectToAction("PaymentMethod", "Checkout", new { area = "" });
		}
	}
}