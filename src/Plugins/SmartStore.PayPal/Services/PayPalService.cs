using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.PayPal.Providers;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Tax;

namespace SmartStore.PayPal.Services
{
    public partial class PayPalService : IPayPalService
    {
        private readonly Lazy<IRepository<Order>> _orderRepository;
        private readonly ICommonServices _services;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly Lazy<IMediaService> _mediaService;
        private readonly Lazy<ICountryService> _countryService;
        private readonly Lazy<CompanyInformationSettings> _companyInfoSettings;

        public PayPalService(
            Lazy<IRepository<Order>> orderRepository,
            ICommonServices services,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentService paymentService,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            Lazy<IMediaService> mediaService,
            Lazy<ICountryService> countryService,
            Lazy<CompanyInformationSettings> companyInfoSettings)
        {
            _orderRepository = orderRepository;
            _services = services;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentService = paymentService;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
            _currencyService = currencyService;
            _mediaService = mediaService;
            _countryService = countryService;
            _companyInfoSettings = companyInfoSettings;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;
        public ILogger Logger { get; set; } = NullLogger.Instance;

        private Dictionary<string, object> CreateAddress(Address addr, bool addRecipientName)
        {
            var dic = new Dictionary<string, object>();

            dic.Add("line1", addr.Address1.Truncate(100));

            if (addr.Address2.HasValue())
            {
                dic.Add("line2", addr.Address2.Truncate(100));
            }

            dic.Add("city", addr.City.Truncate(50));

            if (addr.CountryId != 0 && addr.Country != null)
            {
                dic.Add("country_code", addr.Country.TwoLetterIsoCode);
            }

            dic.Add("postal_code", addr.ZipPostalCode.Truncate(20));

            if (addr.StateProvinceId != 0 && addr.StateProvince != null)
            {
                dic.Add("state", addr.StateProvince.Abbreviation.Truncate(100));
            }

            if (addRecipientName)
            {
                dic.Add("recipient_name", addr.GetFullName().Truncate(127));
            }

            return dic;
        }

        private Dictionary<string, object> CreateAmount(
            PayPalSessionData session,
            Store store,
            Customer customer,
            List<OrganizedShoppingCartItem> cart,
            List<Dictionary<string, object>> items)
        {
            Guard.NotEmpty(session.ProviderSystemName, nameof(session.ProviderSystemName));

            var amount = new Dictionary<string, object>();
            var amountDetails = new Dictionary<string, object>();
            var language = _services.WorkContext.WorkingLanguage;
            var currency = _services.WorkContext.WorkingCurrency;
            var currencyCode = store.PrimaryStoreCurrency.CurrencyCode;
            var includingTax = (_services.WorkContext.GetTaxDisplayTypeFor(customer, store.Id) == TaxDisplayType.IncludingTax);
            var totalOrderItems = decimal.Zero;
            var taxTotal = decimal.Zero;

            var cartTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart);
            var total = Math.Round(cartTotal.TotalAmount ?? decimal.Zero, 2);

            if (total == decimal.Zero)
            {
                return amount;
            }

            var shipping = Math.Round(_orderTotalCalculationService.GetShoppingCartShippingTotal(cart) ?? decimal.Zero, 2);

            var additionalHandlingFee = _paymentService.GetAdditionalHandlingFee(cart, session.ProviderSystemName);
            var paymentFeeBase = _taxService.GetPaymentMethodAdditionalFee(additionalHandlingFee, customer);
            var paymentFee = Math.Round(_currencyService.ConvertFromPrimaryStoreCurrency(paymentFeeBase, currency), 2);

            // Line items.
            foreach (var item in cart)
            {
                decimal unitPriceTaxRate = decimal.Zero;
                decimal unitPrice = _priceCalculationService.GetUnitPrice(item, true);
                decimal productPrice = _taxService.GetProductPrice(item.Item.Product, unitPrice, includingTax, customer, out unitPriceTaxRate);

                if (items != null && productPrice != decimal.Zero)
                {
                    var line = new Dictionary<string, object>();
                    line.Add("quantity", item.Item.Quantity);
                    line.Add("name", item.Item.Product.GetLocalized(x => x.Name, language, true, false).Value.Truncate(127));
                    line.Add("price", productPrice.FormatInvariant());
                    line.Add("currency", currencyCode);
                    line.Add("sku", item.Item.Product.Sku.Truncate(50));
                    items.Add(line);
                }

                totalOrderItems += (Math.Round(productPrice, 2) * item.Item.Quantity);
            }

            // Rounding.
            if (cartTotal.RoundingAmount != decimal.Zero)
            {
                if (items != null)
                {
                    var line = new Dictionary<string, object>();
                    line.Add("quantity", "1");
                    line.Add("name", T("ShoppingCart.Totals.Rounding").Text.Truncate(127));
                    line.Add("price", cartTotal.RoundingAmount.FormatInvariant());
                    line.Add("currency", currencyCode);
                    items.Add(line);
                }

                totalOrderItems += Math.Round(cartTotal.RoundingAmount, 2);
            }

            if (items != null && paymentFee != decimal.Zero)
            {
                var line = new Dictionary<string, object>();
                line.Add("quantity", "1");
                line.Add("name", T("Order.PaymentMethodAdditionalFee").Text.Truncate(127));
                line.Add("price", paymentFee.FormatInvariant());
                line.Add("currency", currencyCode);
                items.Add(line);

                totalOrderItems += Math.Round(paymentFee, 2);
            }

            if (!includingTax)
            {
                // "To avoid rounding errors we recommend not submitting tax amounts on line item basis. 
                // Calculated tax amounts for the entire shopping basket may be submitted in the amount objects.
                // In this case the item amounts will be treated as amounts excluding tax.
                // In a B2C scenario, where taxes are included, no taxes should be submitted to PayPal."

                SortedDictionary<decimal, decimal> taxRates = null;
                taxTotal = Math.Round(_orderTotalCalculationService.GetTaxTotal(cart, out taxRates), 2);

                amountDetails.Add("tax", taxTotal.FormatInvariant());
            }

            var itemsPlusMisc = (totalOrderItems + taxTotal + shipping);

            if (total != itemsPlusMisc)
            {
                var otherAmount = Math.Round(total - itemsPlusMisc, 2);
                totalOrderItems += otherAmount;

                if (items != null && otherAmount != decimal.Zero)
                {
                    // E.g. discount applied to cart total.
                    var line = new Dictionary<string, object>();
                    line.Add("quantity", "1");
                    line.Add("name", T("Plugins.SmartStore.PayPal.Other").Text.Truncate(127));
                    line.Add("price", otherAmount.FormatInvariant());
                    line.Add("currency", currencyCode);
                    items.Add(line);
                }
            }

            // Fill amount object.
            amountDetails.Add("shipping", shipping.FormatInvariant());
            amountDetails.Add("subtotal", totalOrderItems.FormatInvariant());

            //if (paymentFee != decimal.Zero)
            //{
            //	amountDetails.Add("handling_fee", paymentFee.FormatInvariant());
            //}

            amount.Add("total", total.FormatInvariant());
            amount.Add("currency", currencyCode);
            amount.Add("details", amountDetails);

            return amount;
        }

        private string ToInfoString(dynamic json)
        {
            var sb = new StringBuilder();

            try
            {
                string[] strings = T("Plugins.SmartStore.PayPal.MessageStrings").Text.SplitSafe(";");
                var message = (string)json.summary;
                var eventType = (string)json.event_type;
                var eventId = (string)json.id;
                string state = null;
                string amount = null;
                string paymentId = null;

                if (json.resource != null)
                {
                    state = (string)json.resource.state;
                    paymentId = (string)json.resource.parent_payment;

                    if (json.resource.amount != null)
                        amount = string.Concat((string)json.resource.amount.total, " ", (string)json.resource.amount.currency);
                }

                sb.AppendLine("{0}: {1}".FormatInvariant(strings.SafeGet((int)PayPalMessage.Message), message.NaIfEmpty()));
                sb.AppendLine("{0}: {1}".FormatInvariant(strings.SafeGet((int)PayPalMessage.Event), eventType.NaIfEmpty()));
                sb.AppendLine("{0}: {1}".FormatInvariant(strings.SafeGet((int)PayPalMessage.EventId), eventId.NaIfEmpty()));
                sb.AppendLine("{0}: {1}".FormatInvariant(strings.SafeGet((int)PayPalMessage.PaymentId), paymentId.NaIfEmpty()));
                sb.AppendLine("{0}: {1}".FormatInvariant(strings.SafeGet((int)PayPalMessage.State), state.NaIfEmpty()));
                sb.AppendLine("{0}: {1}".FormatInvariant(strings.SafeGet((int)PayPalMessage.Amount), amount.NaIfEmpty()));
            }
            catch { }

            return sb.ToString();
        }

        public static string GetApiUrl(bool sandbox)
        {
            return sandbox ? "https://api.sandbox.paypal.com" : "https://api.paypal.com";
        }

        public void AddOrderNote(PayPalSettingsBase settings, Order order, string anyString, bool isIpn = false)
        {
            try
            {
                if (order == null || anyString.IsEmpty() || (settings != null && !settings.AddOrderNotes))
                    return;

                string[] orderNoteStrings = T("Plugins.SmartStore.PayPal.OrderNoteStrings").Text.SplitSafe(";");
                var faviconUrl = "{0}Plugins/{1}/Content/favicon.png".FormatInvariant(_services.WebHelper.GetStoreLocation(), Plugin.SystemName);
                var note = $"<img src='{faviconUrl}' class='mr-1 align-text-top' />" + orderNoteStrings.SafeGet(0).FormatInvariant(anyString);

                if (isIpn)
                {
                    order.HasNewPaymentNotification = true;
                }

                _orderService.AddOrderNote(order, note);
            }
            catch { }
        }

        public PayPalPaymentInstruction ParsePaymentInstruction(dynamic json)
        {
            if (json == null)
                return null;

            DateTime dt;
            var result = new PayPalPaymentInstruction();

            try
            {
                result.ReferenceNumber = (string)json.reference_number;
                result.Type = (string)json.instruction_type;
                result.Note = (string)json.note;

                if (DateTime.TryParse((string)json.payment_due_date, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    result.DueDate = dt;
                }

                if (json.amount != null)
                {
                    result.AmountCurrencyCode = (string)json.amount.currency;
                    result.Amount = decimal.Parse((string)json.amount.value, CultureInfo.InvariantCulture);
                }

                var rbi = json.recipient_banking_instruction;

                if (rbi != null)
                {
                    result.RecipientBanking = new PayPalPaymentInstruction.RecipientBankingInstruction();
                    result.RecipientBanking.BankName = (string)rbi.bank_name;
                    result.RecipientBanking.AccountHolderName = (string)rbi.account_holder_name;
                    result.RecipientBanking.AccountNumber = (string)rbi.account_number;
                    result.RecipientBanking.RoutingNumber = (string)rbi.routing_number;
                    result.RecipientBanking.Iban = (string)rbi.international_bank_account_number;
                    result.RecipientBanking.Bic = (string)rbi.bank_identifier_code;
                }

                if (json.links != null)
                {
                    result.Link = (string)json.links[0].href;
                }
            }
            catch { }

            return result;
        }

        public string CreatePaymentInstruction(PayPalPaymentInstruction instruct)
        {
            if (instruct == null || instruct.RecipientBanking == null)
                return null;

            if (!instruct.IsManualBankTransfer && !instruct.IsPayUponInvoice)
                return null;

            var sb = new StringBuilder("<div style='text-align:left;'>");
            var paragraphTemplate = "<div style='margin-bottom:10px;'>{0}</div>";
            var rowTemplate = "<span style='min-width:120px;'>{0}</span>: {1}<br />";
            var instructStrings = T("Plugins.SmartStore.PayPal.PaymentInstructionStrings").Text.SplitSafe(";");

            if (instruct.IsManualBankTransfer)
            {
                sb.AppendFormat(paragraphTemplate, T("Plugins.SmartStore.PayPal.ManualBankTransferNote"));

                sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.Reference), instruct.ReferenceNumber);
                sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.AccountNumber), instruct.RecipientBanking.AccountNumber);
                sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.AccountHolder), instruct.RecipientBanking.AccountHolderName);

                sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.Bank), instruct.RecipientBanking.BankName);
                sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.Iban), instruct.RecipientBanking.Iban);
                sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.Bic), instruct.RecipientBanking.Bic);
            }
            else if (instruct.IsPayUponInvoice)
            {
                string amount = null;
                var culture = new CultureInfo(_services.WorkContext.WorkingLanguage.LanguageCulture ?? "de-DE");

                try
                {
                    var currency = _currencyService.GetCurrencyByCode(instruct.AmountCurrencyCode);
                    var format = (currency != null && currency.CustomFormatting.HasValue() ? currency.CustomFormatting : "C");

                    amount = instruct.Amount.ToString(format, culture);
                }
                catch { }

                if (amount.IsEmpty())
                {
                    amount = string.Concat(instruct.Amount.ToString("N"), " ", instruct.AmountCurrencyCode);
                }

                var intro = T("Plugins.SmartStore.PayPal.PayUponInvoiceLegalNote", _companyInfoSettings.Value.CompanyName.NaIfEmpty());

                // /v1/payments/payment/<id>/payment-instruction not working anymore. Serves 401 unauthorized.
                //if (instruct.Link.HasValue())
                //{
                //	intro = "{0} <a href='{1}'>{2}</a>.".FormatInvariant(intro, instruct.Link, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.Details));
                //}

                sb.AppendFormat(paragraphTemplate, intro);

                sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.Bank), instruct.RecipientBanking.BankName);
                sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.AccountHolder), instruct.RecipientBanking.AccountHolderName);
                sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.Iban), instruct.RecipientBanking.Iban);
                sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.Bic), instruct.RecipientBanking.Bic);
                sb.Append("<br />");
                sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.Amount), amount);
                if (instruct.DueDate.HasValue)
                {
                    sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.PaymentDueDate), instruct.DueDate.Value.ToString("d", culture));
                }
                sb.AppendFormat(rowTemplate, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.Reference), instruct.ReferenceNumber);
            }

            sb.Append("</div>");

            return sb.ToString();
        }

        public PaymentStatus GetPaymentStatus(string state, string reasonCode, PaymentStatus defaultStatus)
        {
            var result = defaultStatus;

            if (state == null)
                state = string.Empty;

            if (reasonCode == null)
                reasonCode = string.Empty;

            switch (state.ToLowerInvariant())
            {
                case "authorized":
                    result = PaymentStatus.Authorized;
                    break;
                case "pending":
                    switch (reasonCode.ToLowerInvariant())
                    {
                        case "authorization":
                            result = PaymentStatus.Authorized;
                            break;
                        default:
                            result = PaymentStatus.Pending;
                            break;
                    }
                    break;
                case "completed":
                case "captured":
                case "partially_captured":
                case "canceled_reversal":
                    result = PaymentStatus.Paid;
                    break;
                case "denied":
                case "expired":
                case "failed":
                case "voided":
                    result = PaymentStatus.Voided;
                    break;
                case "reversed":
                case "refunded":
                    result = PaymentStatus.Refunded;
                    break;
                case "partially_refunded":
                    result = PaymentStatus.PartiallyRefunded;
                    break;
            }

            return result;
        }

        public PayPalResponse CallApi(
            string method,
            string path,
            PayPalApiSettingsBase settings,
            PayPalSessionData session,
            string data)
        {
            var isJson = data.HasValue() && (data.StartsWith("{") || data.StartsWith("["));
            var encoding = isJson ? Encoding.UTF8 : Encoding.ASCII;
            var result = new PayPalResponse();
            HttpWebResponse webResponse = null;

            var url = GetApiUrl(settings.UseSandbox) + path.EnsureStartsWith("/");

            if (method.IsCaseInsensitiveEqual("GET") && data.HasValue())
            {
                url = url.EnsureEndsWith("?") + data;
            }

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.Accept = "application/json";
            request.ContentType = isJson ? "application/json" : "application/x-www-form-urlencoded";

            try
            {
                request.UserAgent = HttpContext.Current != null && HttpContext.Current.Request != null
                    ? HttpContext.Current.Request.UserAgent
                    : Plugin.SystemName;
            }
            catch { }

            if (path.EmptyNull().EndsWith("/token"))
            {
                // see https://github.com/paypal/sdk-core-dotnet/blob/master/Source/SDK/OAuthTokenCredential.cs
                byte[] credentials = Encoding.UTF8.GetBytes("{0}:{1}".FormatInvariant(settings.ClientId, settings.Secret));

                request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(credentials));
            }
            else if (session != null)
            {
                request.Headers["Authorization"] = "Bearer " + session.AccessToken.EmptyNull();

                if (session.AccessToken.IsEmpty())
                {
                    Logger.Error(T("Plugins.SmartStore.PayPal.MissingAccessToken", method.NaIfEmpty(), path.NaIfEmpty()));
                }
            }

            if (session != null)
            {
                if (session.ProviderSystemName.IsCaseInsensitiveEqual(PayPalPlusProvider.SystemName))
                {
                    request.Headers["PayPal-Partner-Attribution-Id"] = "SmartStoreAG_Cart_PayPalPlus";
                }
                else if (session.ProviderSystemName.IsCaseInsensitiveEqual(PayPalInstalmentsProvider.SystemName))
                {
                    request.Headers["PayPal-Partner-Attribution-Id"] = "SmartStoreAG_Cart_Ratenzahlung";
                }
            }

            if (data.HasValue() && (method.IsCaseInsensitiveEqual("POST") || method.IsCaseInsensitiveEqual("PUT") || method.IsCaseInsensitiveEqual("PATCH")))
            {
                byte[] bytes = encoding.GetBytes(data);

                request.ContentLength = bytes.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            else
            {
                request.ContentLength = 0;
            }

            try
            {
                webResponse = request.GetResponse() as HttpWebResponse;
                result.Success = ((int)webResponse.StatusCode < 400);
            }
            catch (WebException wex)
            {
                result.Success = false;
                result.ErrorMessage = wex.ToString();
                webResponse = wex.Response as HttpWebResponse;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.ToString();
                Logger.Log(LogLevel.Error, ex, null, null);
            }

            try
            {
                if (webResponse != null)
                {
                    string rawResponse = null;
                    using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                    {
                        rawResponse = reader.ReadToEnd();
                        if (rawResponse.HasValue())
                        {
                            try
                            {
                                if (rawResponse.StartsWith("["))
                                    result.Json = JArray.Parse(rawResponse);
                                else
                                    result.Json = JObject.Parse(rawResponse);

                                if (result.Json != null)
                                {
                                    if (!result.Success)
                                    {
                                        // Parse error details.
                                        string message = null;
                                        var name = (string)result.Json.name;

                                        if (name.IsEmpty())
                                        {
                                            name = (string)result.Json.error;
                                        }

                                        if (name.IsCaseInsensitiveEqual("VALIDATION_ERROR"))
                                        {
                                            result.IsValidationError = true;

                                            JArray details = result.Json.details;
                                            if (details != null)
                                            {
                                                foreach (dynamic detail in details)
                                                {
                                                    message = message.Grow((string)detail.issue, ". ");
                                                }
                                            }
                                        }

                                        if (message.IsEmpty())
                                        {
                                            message = (string)result.Json.message;
                                        }

                                        if (message.IsEmpty())
                                        {
                                            message = (string)result.Json.error_description;
                                        }

                                        result.ErrorMessage = "{0}: {1}.".FormatInvariant(name.NaIfEmpty(), message.NaIfEmpty());
                                    }
                                }
                            }
                            catch
                            {
                                if (!result.Success)
                                    result.ErrorMessage = rawResponse;
                            }
                        }
                    }

                    if (!result.Success)
                    {
                        // Log all headers and raw response.
                        if (result.ErrorMessage.IsEmpty())
                            result.ErrorMessage = webResponse.StatusDescription;

                        var sb = new StringBuilder();

                        try
                        {
                            sb.AppendLine();
                            request.Headers.AllKeys.Each(x => sb.AppendLine($"{x}: {request.Headers[x]}"));
                            if (data.HasValue())
                            {
                                sb.AppendLine();
                                if (data.StartsWith("["))
                                    sb.AppendLine(JArray.Parse(data).ToString(Formatting.Indented));
                                else
                                    sb.AppendLine(JObject.Parse(data).ToString(Formatting.Indented));
                            }
                            sb.AppendLine();
                            webResponse.Headers.AllKeys.Each(x => sb.AppendLine($"{x}: {webResponse.Headers[x]}"));
                            sb.AppendLine();
                            if (result.Json != null)
                            {
                                sb.AppendLine(result.Json.ToString());
                            }
                            else if (rawResponse.HasValue())
                            {
                                sb.AppendLine(rawResponse);
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Dump();
                        }

                        var logLevel = webResponse.StatusCode == HttpStatusCode.InternalServerError ? LogLevel.Warning : LogLevel.Error;
                        Logger.Log(logLevel, new Exception(sb.ToString()), result.ErrorMessage, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex, null, null);
            }
            finally
            {
                if (webResponse != null)
                {
                    webResponse.Close();
                    webResponse.Dispose();
                }
            }

            return result;
        }

        public PayPalResponse EnsureAccessToken(PayPalSessionData session, PayPalApiSettingsBase settings)
        {
            if (session.AccessToken.IsEmpty() || DateTime.UtcNow >= session.TokenExpiration)
            {
                var result = CallApi("POST", "/v1/oauth2/token", settings, null, "grant_type=client_credentials");
                if (result.Success)
                {
                    session.AccessToken = (string)result.Json.access_token;

                    var expireSeconds = ((string)result.Json.expires_in).ToInt(30 * 60);
                    session.TokenExpiration = DateTime.UtcNow.AddSeconds(expireSeconds);
                }
                else
                {
                    return result;
                }
            }

            return new PayPalResponse
            {
                Success = session.AccessToken.HasValue()
            };
        }

        public PayPalResponse GetPayment(PayPalApiSettingsBase settings, PayPalSessionData session)
        {
            var result = CallApi("GET", "/v1/payments/payment/" + session.PaymentId, settings, session, null);

            if (result.Success && result.Json != null)
            {
                result.Id = (string)result.Json.id;
            }

            return result;
        }

        public Dictionary<string, object> CreatePaymentData(
            PayPalApiSettingsBase settings,
            PayPalSessionData session,
            List<OrganizedShoppingCartItem> cart,
            string returnUrl,
            string cancelUrl)
        {
            var store = _services.StoreContext.CurrentStore;
            var customer = _services.WorkContext.CurrentCustomer;

            var data = new Dictionary<string, object>();
            var redirectUrls = new Dictionary<string, object>();
            var payer = new Dictionary<string, object>();
            var transaction = new Dictionary<string, object>();
            var items = new List<Dictionary<string, object>>();
            var itemList = new Dictionary<string, object>();

            // "PayPal PLUS only supports transaction type “Sale” (instant settlement)".
            if (session.ProviderSystemName == PayPalPlusProvider.SystemName || session.ProviderSystemName == PayPalInstalmentsProvider.SystemName)
            {
                data.Add("intent", "sale");
            }
            else
            {
                data.Add("intent", settings.TransactMode == TransactMode.AuthorizeAndCapture ? "sale" : "authorize");
            }

            if (settings.ExperienceProfileId.HasValue())
            {
                data.Add("experience_profile_id", settings.ExperienceProfileId);
            }

            // Redirect URLs.
            if (returnUrl.HasValue())
            {
                redirectUrls.Add("return_url", returnUrl);
            }
            if (cancelUrl.HasValue())
            {
                redirectUrls.Add("cancel_url", cancelUrl);
            }
            if (redirectUrls.Any())
            {
                data.Add("redirect_urls", redirectUrls);
            }

            // payer, payer_info
            // PayPal review: do not transmit payer_info for PP PLUS.
            if (session.ProviderSystemName == PayPalInstalmentsProvider.SystemName)
            {
                GetPayerInfo(customer, out var firstName, out var lastName, out var email);

                var payerInfo = new Dictionary<string, object>();
                payerInfo.Add("email", email.EmptyNull());
                payerInfo.Add("first_name", firstName.EmptyNull());
                payerInfo.Add("last_name", lastName.EmptyNull());

                //var dateOfBirth = customer.GetAttribute<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);
                //if (dateOfBirth.HasValue)
                //{
                //	payerInfo.Add("birth_date", dateOfBirth.Value.ToString("yyyy-MM-dd"));
                //}

                if (customer.BillingAddress != null)
                {
                    payerInfo.Add("billing_address", CreateAddress(customer.BillingAddress, false));
                }

                payer.Add("external_selected_funding_instrument_type", "CREDIT");
                payer.Add("payer_info", payerInfo);

                if (customer.ShippingAddress != null)
                {
                    // PayPal review: always pass recipient_name.
                    itemList.Add("shipping_address", CreateAddress(customer.ShippingAddress, true));
                }
            }

            payer.Add("payment_method", "paypal");
            data.Add("payer", payer);

            var amount = CreateAmount(session, store, customer, cart, items);
            if (!amount.Any())
            {
                return null;
            }

            itemList.Add("items", items);

            transaction.Add("amount", amount);
            transaction.Add("item_list", itemList);
            transaction.Add("invoice_number", session.OrderGuid.ToString());

            data.Add("transactions", new List<Dictionary<string, object>> { transaction });

            return data;
        }

        public PayPalResponse CreatePayment(
            PayPalApiSettingsBase settings,
            PayPalSessionData session,
            Dictionary<string, object> data)
        {
            if (data == null || !data.Any())
            {
                return null;
            }

            var serializeData = JsonConvert.SerializeObject(data);
            var result = CallApi("POST", "/v1/payments/payment", settings, session, serializeData);

            if (result.Success && result.Json != null)
            {
                result.Id = (string)result.Json.id;
            }

            //Logger.Log(LogLevel.Information, new Exception(JsonConvert.SerializeObject(data, Formatting.Indented) + "\r\n\r\n" + (result.Json != null ? result.Json.ToString() : "")), "PayPal API", null);

            return result;
        }

        public PayPalResponse PatchShipping(
            PayPalApiSettingsBase settings,
            PayPalSessionData session,
            List<OrganizedShoppingCartItem> cart)
        {
            var data = new List<Dictionary<string, object>>();
            var amountTotal = new Dictionary<string, object>();

            var store = _services.StoreContext.CurrentStore;
            var customer = _services.WorkContext.CurrentCustomer;

            if (customer.ShippingAddress != null)
            {
                var shippingAddress = new Dictionary<string, object>();
                shippingAddress.Add("op", "add");
                shippingAddress.Add("path", "/transactions/0/item_list/shipping_address");
                shippingAddress.Add("value", CreateAddress(customer.ShippingAddress, true));
                data.Add(shippingAddress);
            }

            if (customer.BillingAddress != null)
            {
                GetPayerInfo(customer, out var firstName, out var lastName, out var email);

                var payerInfo = new Dictionary<string, object>();
                payerInfo.Add("email", email.EmptyNull());
                payerInfo.Add("first_name", firstName.EmptyNull());
                payerInfo.Add("last_name", lastName.EmptyNull());
                payerInfo.Add("billing_address", CreateAddress(customer.BillingAddress, false));

                var payer = new Dictionary<string, object>();
                payer.Add("op", "add");
                payer.Add("path", "/payer/payer_info");
                payer.Add("value", payerInfo);
                data.Add(payer);
            }

            // Update of whole amount object required. patching single amount values not possible (MALFORMED_REQUEST).
            var amount = CreateAmount(session, store, customer, cart, null);

            amountTotal.Add("op", "replace");
            amountTotal.Add("path", "/transactions/0/amount");
            amountTotal.Add("value", amount);
            data.Add(amountTotal);

            var result = CallApi("PATCH", "/v1/payments/payment/{0}".FormatInvariant(session.PaymentId), settings, session, JsonConvert.SerializeObject(data));

            //Logger.Log(LogLevel.Information, new Exception(JsonConvert.SerializeObject(data, Formatting.Indented) + "\r\n\r\n" + (result.Json != null ? result.Json.ToString() : "")), "PayPal API", null);

            return result;
        }

        public PayPalResponse ExecutePayment(PayPalApiSettingsBase settings, PayPalSessionData session)
        {
            var data = new Dictionary<string, object>();
            data.Add("payer_id", session.PayerId);

            var result = CallApi("POST", "/v1/payments/payment/{0}/execute".FormatInvariant(session.PaymentId), settings, session, JsonConvert.SerializeObject(data));

            if (result.Success && result.Json != null)
            {
                result.Id = (string)result.Json.id;

                //Logger.Log(LogLevel.Information, new Exception(JsonConvert.SerializeObject(data, Formatting.Indented) + "\r\n\r\n" + (result.Json != null ? result.Json.ToString() : "")), "PayPal API", null);
            }

            return result;
        }

        public PayPalResponse Refund(PayPalApiSettingsBase settings, PayPalSessionData session, RefundPaymentRequest request)
        {
            var data = new Dictionary<string, object>();
            var store = _services.StoreService.GetStoreById(request.Order.StoreId);
            var isSale = request.Order.AuthorizationTransactionResult.Contains("(sale)");

            var path = "/v1/payments/{0}/{1}/refund".FormatInvariant(isSale ? "sale" : "capture", request.Order.CaptureTransactionId);

            var amount = new Dictionary<string, object>();
            amount.Add("total", request.AmountToRefund.FormatInvariant());
            amount.Add("currency", store.PrimaryStoreCurrency.CurrencyCode);

            data.Add("amount", amount);

            var result = CallApi("POST", path, settings, session, data.Any() ? JsonConvert.SerializeObject(data) : null);

            if (result.Success && result.Json != null)
            {
                result.Id = (string)result.Json.id;
            }

            //Logger.InsertLog(LogLevel.Information, "PayPal Refund", JsonConvert.SerializeObject(data, Formatting.Indented) + "\r\n\r\n" + (result.Json != null ? result.Json.ToString() : ""));

            return result;
        }

        public PayPalResponse Capture(PayPalApiSettingsBase settings, PayPalSessionData session, CapturePaymentRequest request)
        {
            var data = new Dictionary<string, object>();
            //var isAuthorize = request.Order.AuthorizationTransactionCode.IsCaseInsensitiveEqual("authorize");

            var path = "/v1/payments/authorization/{0}/capture".FormatInvariant(request.Order.AuthorizationTransactionId);

            var store = _services.StoreService.GetStoreById(request.Order.StoreId);

            var amount = new Dictionary<string, object>();
            amount.Add("total", request.Order.OrderTotal.FormatInvariant());
            amount.Add("currency", store.PrimaryStoreCurrency.CurrencyCode);

            data.Add("amount", amount);
            data.Add("is_final_capture", "true");

            var result = CallApi("POST", path, settings, session, JsonConvert.SerializeObject(data));

            if (result.Success && result.Json != null)
            {
                result.Id = (string)result.Json.id;
            }

            return result;
        }

        public PayPalResponse Void(PayPalApiSettingsBase settings, PayPalSessionData session, VoidPaymentRequest request)
        {
            var path = "/v1/payments/authorization/{0}/void".FormatInvariant(request.Order.AuthorizationTransactionId);

            var result = CallApi("POST", path, settings, session, null);

            if (result.Success && result.Json != null)
            {
                result.Id = (string)result.Json.id;
            }

            return result;
        }

        public PayPalResponse UpsertCheckoutExperience(PayPalApiSettingsBase settings, PayPalSessionData session, Store store)
        {
            PayPalResponse result;
            var name = store.Name;
            var logo = _mediaService.Value.GetFileById(store.LogoMediaFileId, MediaLoadFlags.AsNoTracking);
            var path = "/v1/payment-experience/web-profiles";

            var data = new Dictionary<string, object>();
            var presentation = new Dictionary<string, object>();
            var inpuFields = new Dictionary<string, object>();

            // Find existing profile id, only one profile per profile name possible.
            if (settings.ExperienceProfileId.IsEmpty())
            {
                result = CallApi("GET", path, settings, session, null);
                if (result.Success && result.Json != null)
                {
                    foreach (var profile in result.Json)
                    {
                        var profileName = (string)profile.name;
                        if (profileName.IsCaseInsensitiveEqual(name))
                        {
                            settings.ExperienceProfileId = (string)profile.id;
                            break;
                        }
                    }
                }
            }

            presentation.Add("brand_name", name);
            presentation.Add("locale_code", _services.WorkContext.WorkingLanguage.UniqueSeoCode.EmptyNull().ToUpper());

            if (logo != null)
            {
                presentation.Add("logo_image", _mediaService.Value.GetUrl(logo, 0, _services.StoreService.GetHost(store), false));
            }

            inpuFields.Add("allow_note", false);
            
            // Settings for no_shipping and address_override would not be suitable. The merchant always delivers to the address selected in the shop.
            // He will not be aware of a different address selected at PP.
            // https://developer.paypal.com/docs/api/payment-experience/v1#definition-input_fields

            // "0" means display the shipping address on PP pages. If no shipping is required, no address will be transmitted.
            inpuFields.Add("no_shipping", 0);

            // "1" means display the shipping address specified in patch shipping call. The customer cannot edit it at PP.
            inpuFields.Add("address_override", 1);

            data.Add("name", name);
            data.Add("presentation", presentation);
            data.Add("input_fields", inpuFields);

            if (settings.ExperienceProfileId.HasValue())
            {
                path = string.Concat(path, "/", HttpUtility.UrlPathEncode(settings.ExperienceProfileId));
            }

            result = CallApi(settings.ExperienceProfileId.HasValue() ? "PUT" : "POST", path, settings, session, JsonConvert.SerializeObject(data));

            if (result.Success)
            {
                if (result.Json != null)
                {
                    result.Id = (string)result.Json.id;
                }
                else
                {
                    result.Id = settings.ExperienceProfileId;
                }
            }

            return result;
        }

        public PayPalResponse DeleteCheckoutExperience(PayPalApiSettingsBase settings, PayPalSessionData session)
        {
            var result = CallApi("DELETE", "/v1/payment-experience/web-profiles/" + settings.ExperienceProfileId, settings, session, null);

            if (result.Success && result.Json != null)
            {
                result.Id = (string)result.Json.id;
            }

            return result;
        }

        public PayPalResponse CreateWebhook(PayPalApiSettingsBase settings, PayPalSessionData session, string url)
        {
            var data = new Dictionary<string, object>();
            var events = new List<Dictionary<string, object>>();

            events.Add(new Dictionary<string, object> { { "name", "PAYMENT.AUTHORIZATION.VOIDED" } });
            events.Add(new Dictionary<string, object> { { "name", "PAYMENT.CAPTURE.COMPLETED" } });
            events.Add(new Dictionary<string, object> { { "name", "PAYMENT.CAPTURE.DENIED" } });
            events.Add(new Dictionary<string, object> { { "name", "PAYMENT.CAPTURE.PENDING" } });
            events.Add(new Dictionary<string, object> { { "name", "PAYMENT.CAPTURE.REFUNDED" } });
            events.Add(new Dictionary<string, object> { { "name", "PAYMENT.CAPTURE.REVERSED" } });
            events.Add(new Dictionary<string, object> { { "name", "PAYMENT.SALE.COMPLETED" } });
            events.Add(new Dictionary<string, object> { { "name", "PAYMENT.SALE.DENIED" } });
            events.Add(new Dictionary<string, object> { { "name", "PAYMENT.SALE.PENDING" } });
            events.Add(new Dictionary<string, object> { { "name", "PAYMENT.SALE.REFUNDED" } });
            events.Add(new Dictionary<string, object> { { "name", "PAYMENT.SALE.REVERSED" } });

            data.Add("url", url);
            data.Add("event_types", events);

            var result = CallApi("POST", "/v1/notifications/webhooks", settings, session, JsonConvert.SerializeObject(data));

            if (result.Success && result.Json != null)
            {
                result.Id = (string)result.Json.id;
            }

            return result;
        }

        public PayPalResponse DeleteWebhook(PayPalApiSettingsBase settings, PayPalSessionData session)
        {
            var result = CallApi("DELETE", "/v1/notifications/webhooks/" + settings.WebhookId, settings, session, null);

            if (result.Success && result.Json != null)
            {
                result.Id = (string)result.Json.id;
            }

            return result;
        }

        /// <remarks>return 503 (HttpStatusCode.ServiceUnavailable) to ask paypal to resend it at later time again</remarks>
        public HttpStatusCode ProcessWebhook(
            PayPalApiSettingsBase settings,
            NameValueCollection headers,
            string rawJson,
            string providerSystemName)
        {
            if (rawJson.IsEmpty())
                return HttpStatusCode.OK;

            dynamic json = JObject.Parse(rawJson);
            var eventType = (string)json.event_type;

            //foreach (var key in headers.AllKeys)"{0}: {1}".FormatInvariant(key, headers[key]).Dump();
            //string data = JsonConvert.SerializeObject(json, Formatting.Indented);data.Dump();


            // validating against PayPal SDK failing using sandbox, so better we do not use it:
            //var apiContext = new global::PayPal.Api.APIContext
            //{
            //	AccessToken = "I do not have one here",
            //	Config = new Dictionary<string, string>
            //		{
            //			{ "mode", settings.UseSandbox ? "sandbox" : "live" },
            //			{ "clientId", settings.ClientId },
            //			{ "clientSecret", settings.Secret },
            //			{ "webhook.id", setting.WebhookId },
            //		}
            //};
            //var result = global::PayPal.Api.WebhookEvent.ValidateReceivedEvent(apiContext, headers, rawJson, webhookId);
            //}

            var paymentId = (string)json.resource.parent_payment;
            if (paymentId.IsEmpty())
            {
                Logger.Log(
                    LogLevel.Warning,
                    new Exception(JsonConvert.SerializeObject(json, Formatting.Indented)),
                    T("Plugins.SmartStore.PayPal.FoundOrderForPayment", 0, "".NaIfEmpty()),
                    null);

                return HttpStatusCode.OK;
            }

            var orders = _orderRepository.Value.Table
                .Where(x => x.PaymentMethodSystemName == providerSystemName && x.AuthorizationTransactionCode == paymentId)
                .ToList();

            if (orders.Count != 1)
            {
                Logger.Log(
                    LogLevel.Warning,
                    new Exception(JsonConvert.SerializeObject(json, Formatting.Indented)),
                    T("Plugins.SmartStore.PayPal.FoundOrderForPayment", orders.Count, paymentId),
                    null);

                return HttpStatusCode.OK;
            }

            var order = orders.First();
            var store = _services.StoreService.GetStoreById(order.StoreId);

            var total = decimal.Zero;
            var currency = (string)json.resource.amount.currency;
            var primaryCurrency = store.PrimaryStoreCurrency.CurrencyCode;

            if (!primaryCurrency.IsCaseInsensitiveEqual(currency))
            {
                Logger.Log(
                    LogLevel.Warning,
                    new Exception(JsonConvert.SerializeObject(json, Formatting.Indented)),
                    T("Plugins.SmartStore.PayPal.CurrencyNotEqual", currency.NaIfEmpty(), primaryCurrency),
                    null);

                return HttpStatusCode.OK;
            }

            eventType = eventType.Substring(eventType.LastIndexOf('.') + 1);

            var newPaymentStatus = GetPaymentStatus(eventType, "authorization", order.PaymentStatus);

            var isValidTotal = decimal.TryParse((string)json.resource.amount.total, NumberStyles.Currency, CultureInfo.InvariantCulture, out total);

            if (newPaymentStatus == PaymentStatus.Refunded && (Math.Abs(order.OrderTotal) - Math.Abs(total)) > decimal.Zero)
            {
                newPaymentStatus = PaymentStatus.PartiallyRefunded;
            }

            switch (newPaymentStatus)
            {
                case PaymentStatus.Pending:
                    break;
                case PaymentStatus.Authorized:
                    if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
                    {
                        _orderProcessingService.MarkAsAuthorized(order);
                    }
                    break;
                case PaymentStatus.Paid:
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        _orderProcessingService.MarkOrderAsPaid(order);
                    }
                    break;
                case PaymentStatus.Refunded:
                    if (_orderProcessingService.CanRefundOffline(order))
                    {
                        _orderProcessingService.RefundOffline(order);
                    }
                    break;
                case PaymentStatus.PartiallyRefunded:
                    // We could only process it once cause otherwise order.RefundedAmount would getting wrong.
                    if (order.RefundedAmount == decimal.Zero && _orderProcessingService.CanPartiallyRefundOffline(order, Math.Abs(total)))
                    {
                        _orderProcessingService.PartiallyRefundOffline(order, Math.Abs(total));
                    }
                    break;
                case PaymentStatus.Voided:
                    if (_orderProcessingService.CanVoidOffline(order))
                    {
                        _orderProcessingService.VoidOffline(order);
                    }
                    break;
            }

            AddOrderNote(settings, order, (string)ToInfoString(json), true);

            return HttpStatusCode.OK;
        }

        #region Utilities

        private Money Parse(string amount, Currency sourceCurrency, Currency targetCurrency, Store store)
        {
            Guard.NotNull(sourceCurrency, nameof(sourceCurrency));
            Guard.NotNull(targetCurrency, nameof(targetCurrency));

            if (amount.HasValue() && decimal.TryParse(amount, NumberStyles.Currency, CultureInfo.InvariantCulture, out var value))
            {
                value = _currencyService.ConvertCurrency(value, sourceCurrency, targetCurrency, store);
                return new Money(value, targetCurrency);
            }

            return new Money(decimal.Zero, targetCurrency);
        }

        private void GetPayerInfo(Customer customer, out string firstName, out string lastName, out string email)
        {
            firstName = lastName = email = string.Empty;

            // PayPal review: do take name and email from account data.
            if (customer.ShippingAddress != null)
            {
                email = customer.ShippingAddress.Email;
                firstName = customer.ShippingAddress.FirstName;
                lastName = customer.ShippingAddress.LastName;
            }
            if (lastName.IsEmpty() && customer.BillingAddress != null)
            {
                email = customer.BillingAddress.Email;
                firstName = customer.BillingAddress.FirstName;
                lastName = customer.BillingAddress.LastName;
            }
            if (email.IsEmpty())
            {
                email = customer.Email;
            }
        }

        #endregion
    }


    public class PayPalResponse
    {
        public bool Success { get; set; }
        public dynamic Json { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsValidationError { get; set; }
        public string Id { get; set; }
    }

    [Serializable]
    public class PayPalSessionData
    {
        public PayPalSessionData()
        {
            OrderGuid = Guid.NewGuid();
        }

        public string ProviderSystemName { get; set; }
        public bool SessionExpired { get; set; }
        public string AccessToken { get; set; }
        public DateTime TokenExpiration { get; set; }
        public string PaymentId { get; set; }
        public string PaymentDataHash { get; set; }
        public string PayerId { get; set; }
        public string ApprovalUrl { get; set; }
        public Guid OrderGuid { get; private set; }
        public PayPalPaymentInstruction PaymentInstruction { get; set; }

        public decimal FinancingCosts { get; set; }
        public decimal TotalInclFinancingCosts { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SessionExpired: " + SessionExpired.ToString());
            sb.AppendLine("AccessToken: " + AccessToken.EmptyNull());
            sb.AppendLine("TokenExpiration: " + TokenExpiration.ToString());
            sb.AppendLine("PaymentId: " + PaymentId.EmptyNull());
            sb.AppendLine("PayerId: " + PayerId.EmptyNull());
            sb.AppendLine("ApprovalUrl: " + ApprovalUrl.EmptyNull());
            sb.AppendLine("OrderGuid: " + OrderGuid.ToString());
            sb.AppendLine("PaymentInstruction: " + (PaymentInstruction != null).ToString());
            return sb.ToString();
        }
    }

    [Serializable]
    public class PayPalPaymentInstruction
    {
        public string ReferenceNumber { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string AmountCurrencyCode { get; set; }
        public DateTime? DueDate { get; set; }
        public string Note { get; set; }
        public string Link { get; set; }

        public RecipientBankingInstruction RecipientBanking { get; set; }

        [JsonIgnore]
        public bool IsManualBankTransfer => Type.IsCaseInsensitiveEqual("MANUAL_BANK_TRANSFER");

        [JsonIgnore]
        public bool IsPayUponInvoice => Type.IsCaseInsensitiveEqual("PAY_UPON_INVOICE");

        [Serializable]
        public class RecipientBankingInstruction
        {
            public string BankName { get; set; }
            public string AccountHolderName { get; set; }
            public string AccountNumber { get; set; }
            public string RoutingNumber { get; set; }
            public string Iban { get; set; }
            public string Bic { get; set; }
        }
    }
}