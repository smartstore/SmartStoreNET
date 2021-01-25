using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Html;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
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
            IPriceFormatter priceFormatter) : base(payPalService)
        {
            _httpContext = httpContext;
            _pluginMediator = pluginMediator;
            _genericAttributeService = genericAttributeService;
            _paymentService = paymentService;
            _taxService = taxService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
        }

        protected override string ProviderSystemName => PayPalPlusProvider.SystemName;

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

            try
            {
                model.SystemName = provider.Metadata.SystemName;
                model.DisplayOrder = provider.Metadata.DisplayOrder;
                model.RedirectUrl = Url.Action("CheckoutReturn", "PayPalPlus", new { area = Plugin.SystemName, systemName = provider.Metadata.SystemName }, store.SslEnabled ? "https" : "http");

                if (provider.Metadata.SystemName == PayPalInstalmentsProvider.SystemName)
                {
                    // "The methodName contains up to 25 characters. All additional characters are truncated."
                    // https://developer.paypal.com/docs/paypal-plus/germany/how-to/integrate-third-party-payments/
                    model.MethodName = T("Plugins.Payments.PayPalInstalments.ShortMethodName");
                }
                else
                {
                    model.MethodName = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata).NullEmpty()
                        ?? provider.Metadata.FriendlyName.NullEmpty()
                        ?? provider.Metadata.SystemName;
                }

                model.MethodName = model.MethodName.EmptyNull();

                if (settings.DisplayPaymentMethodDescription)
                {
                    // Not the short description, the full description is intended for frontend.
                    var paymentMethod = _paymentService.GetPaymentMethodBySystemName(provider.Metadata.SystemName);
                    if (paymentMethod != null)
                    {
                        string description = paymentMethod.GetLocalized(x => x.FullDescription);
                        if (description.HasValue())
                        {
                            description = HtmlUtils.ConvertHtmlToPlainText(description);
                            description = HtmlUtils.StripTags(HttpUtility.HtmlDecode(description));

                            if (description.HasValue())
                            {
                                model.Description = description.EncodeJsString();
                            }
                        }
                    }
                }
            }
            catch { }

            try
            {
                if (settings.DisplayPaymentMethodLogo && provider.Metadata.PluginDescriptor != null && store.SslEnabled)
                {
                    // Special case PayPal instalments.
                    if (provider.Metadata.SystemName == PayPalInstalmentsProvider.SystemName && model.ImageUrl.IsEmpty())
                    {
                        var uri = new UriBuilder(Uri.UriSchemeHttps, Request.Url.Host, -1, "Plugins/SmartStore.PayPal/Content/instalments-sm.png");
                        model.ImageUrl = uri.ToString();
                    }
                    else
                    {
                        var brandImageUrl = _pluginMediator.GetBrandImageUrl(provider.Metadata);
                        if (brandImageUrl.HasValue())
                        {
                            if (brandImageUrl.StartsWith("~"))
                            {
                                brandImageUrl = brandImageUrl.Substring(1);
                            }

                            var uri = new UriBuilder(Uri.UriSchemeHttps, Request.Url.Host, -1, brandImageUrl);
                            model.ImageUrl = uri.ToString();
                        }
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
            // It's better to also offer inactive methods here but filter them out in frontend.
            var paymentMethods = _paymentService.LoadAllPaymentMethods(storeScope);

            var model = new PayPalPlusConfigurationModel();
            MiniMapper.Map(settings, model);
            PrepareConfigurationModel(model, storeScope);

            model.AvailableThirdPartyPaymentMethods = paymentMethods
                .Where(x =>
                {
                    if (x.Value.RequiresInteraction)
                    {
                        return false;
                    }
                    if (x.Metadata.PluginDescriptor.SystemName == Plugin.SystemName)
                    {
                        return x.Metadata.SystemName == PayPalInstalmentsProvider.SystemName;
                    }

                    return x.Metadata.PluginDescriptor.SystemName == "SmartStore.OfflinePayment" || x.Value.PaymentMethodType == PaymentMethodType.Redirection;
                })
                .ToSelectListItems(_pluginMediator, model.ThirdPartyPaymentMethods.ToArray());

            return View(model);
        }

        [HttpPost, AdminAuthorize, ChildActionOnly, AdminThemed]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(PayPalPlusConfigurationModel model, FormCollection form)
        {
            if (!SaveConfigurationModel(model, form))
            {
                var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
                var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(storeScope);

                return Configure(settings, storeScope);
            }

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

            if (!cart.Any())
            {
                return RedirectToRoute("ShoppingCart");
            }

            var pppMethod = _paymentService.GetPaymentMethodBySystemName(PayPalPlusProvider.SystemName);
            var pppProvider = _paymentService.LoadPaymentMethodBySystemName(PayPalPlusProvider.SystemName, false, store.Id);

            var methods = _paymentService.LoadActivePaymentMethods(customer, cart, store.Id, null, false);
            var session = _httpContext.GetPayPalState(PayPalPlusProvider.SystemName);
            var redirectToConfirm = false;

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

            // We must create a new paypal payment each time the payment wall is rendered because otherwise patch payment can fail
            // with "Item amount must add up to specified amount subtotal (or total if amount details not specified)".
            session.PaymentId = null;
            session.ApprovalUrl = null;

            var result = PayPalService.EnsureAccessToken(session, settings);
            if (result.Success)
            {
                var protocol = store.SslEnabled ? "https" : "http";
                var returnUrl = Url.Action("CheckoutReturn", "PayPalPlus", new { area = Plugin.SystemName }, protocol);
                var cancelUrl = Url.Action("CheckoutCancel", "PayPalPlus", new { area = Plugin.SystemName }, protocol);

                var paymentData = PayPalService.CreatePaymentData(settings, session, cart, returnUrl, cancelUrl);

                result = PayPalService.CreatePayment(settings, session, paymentData);
                if (result == null)
                {
                    // No payment required.
                    redirectToConfirm = true;
                }
                else if (result.Success && result.Json != null)
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

            // There have been cases where the token was lost for unexplained reasons, so it is additionally stored in the database.
            var sessionData = session.AccessToken.HasValue() && session.PaymentId.HasValue()
                ? JsonConvert.SerializeObject(session)
                : null;
            _genericAttributeService.SaveAttribute(customer, PayPalPlusProvider.SystemName + ".SessionData", sessionData, store.Id);

            if (redirectToConfirm)
            {
                return RedirectToAction("Confirm", "Checkout", new { area = "" });
            }

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
            var store = Services.StoreContext.CurrentStore;
            var customer = Services.WorkContext.CurrentCustomer;
            var session = _httpContext.GetPayPalState(PayPalPlusProvider.SystemName, customer, store.Id, _genericAttributeService);

            if (session.AccessToken.IsEmpty() || session.PaymentId.IsEmpty())
            {
                // Session expired. Reload payment wall and create new payment (we need the payment id).				
                session.SessionExpired = true;

                return new JsonResult { Data = new { success = false, error = string.Empty, reload = true } };
            }

            var settings = Services.Settings.LoadSetting<PayPalPlusPaymentSettings>(store.Id);
            var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

            var result = PayPalService.PatchShipping(settings, session, cart);
            var errorMessage = result.ErrorMessage;

            if (!result.Success && result.IsValidationError)
            {
                errorMessage = string.Concat(T("Plugins.SmartStore.PayPal.PayPalValidationFailure"), "\r\n", errorMessage);
            }

            return new JsonResult { Data = new { success = result.Success, error = errorMessage, reload = false } };
        }

        public ActionResult CheckoutCompleted()
        {
            var store = Services.StoreContext.CurrentStore;
            var customer = Services.WorkContext.CurrentCustomer;

            _genericAttributeService.SaveAttribute(customer, PayPalPlusProvider.SystemName + ".SessionData", (string)null, store.Id);

            var instruct = _httpContext.Session["PayPalCheckoutCompleted"] as string;
            if (instruct.HasValue())
            {
                return Content(instruct);
            }

            return new EmptyResult();
        }

        public ActionResult CheckoutReturn(string systemName, string paymentId, string PayerID)
        {
            // Request.QueryString:
            // paymentId: PAY-0TC88803RP094490KK4KM6AI, token (not the access token): EC-5P379249AL999154U, PayerID: 5L9K773HHJLPN

            var store = Services.StoreContext.CurrentStore;
            var customer = Services.WorkContext.CurrentCustomer;
            var session = _httpContext.GetPayPalState(PayPalPlusProvider.SystemName);

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