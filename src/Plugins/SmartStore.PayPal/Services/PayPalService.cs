﻿using System;
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
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
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
	public class PayPalService : IPayPalService
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
		private readonly Lazy<IPictureService> _pictureService;
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
			Lazy<IPictureService> pictureService,
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
			_pictureService = pictureService;
			_companyInfoSettings = companyInfoSettings;

			T = NullLocalizer.Instance;
			Logger = NullLogger.Instance;
		}

		public Localizer T { get; set; }
		public ILogger Logger { get; set; }

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
				dic.Add("recipient_name", addr.GetFullName().Truncate(50));
			}

			return dic;
		}

		private Dictionary<string, object> CreateAmount(
			Store store,
			Customer customer,
			List<OrganizedShoppingCartItem> cart,
			string providerSystemName,
			List<Dictionary<string, object>> items)
		{
			var amount = new Dictionary<string, object>();
			var amountDetails = new Dictionary<string, object>();
			var language = _services.WorkContext.WorkingLanguage;
			var currency = _services.WorkContext.WorkingCurrency;
			var currencyCode = store.PrimaryStoreCurrency.CurrencyCode;
			var includingTax = (_services.WorkContext.GetTaxDisplayTypeFor(customer, store.Id) == TaxDisplayType.IncludingTax);

			Discount orderAppliedDiscount;
			List<AppliedGiftCard> appliedGiftCards;
			int redeemedRewardPoints = 0;
			decimal redeemedRewardPointsAmount;
			decimal orderDiscountInclTax;
			decimal totalOrderItems = decimal.Zero;
			var taxTotal = decimal.Zero;

			var shipping = Math.Round(_orderTotalCalculationService.GetShoppingCartShippingTotal(cart) ?? decimal.Zero, 2);

			var additionalHandlingFee = _paymentService.GetAdditionalHandlingFee(cart, providerSystemName);
			var paymentFeeBase = _taxService.GetPaymentMethodAdditionalFee(additionalHandlingFee, customer);
			var paymentFee = Math.Round(_currencyService.ConvertFromPrimaryStoreCurrency(paymentFeeBase, currency), 2);

			var total = Math.Round(_orderTotalCalculationService.GetShoppingCartTotal(cart, out orderDiscountInclTax, out orderAppliedDiscount, out appliedGiftCards,
				out redeemedRewardPoints, out redeemedRewardPointsAmount) ?? decimal.Zero, 2);

			// line items
			foreach (var item in cart)
			{
				decimal unitPriceTaxRate = decimal.Zero;
				decimal unitPrice = _priceCalculationService.GetUnitPrice(item, true);
				decimal productPrice = _taxService.GetProductPrice(item.Item.Product, unitPrice, includingTax, customer, out unitPriceTaxRate);

				if (items != null && productPrice != decimal.Zero)
				{
					var line = new Dictionary<string, object>();
					line.Add("quantity", item.Item.Quantity);
					line.Add("name", item.Item.Product.GetLocalized(x => x.Name, language.Id, true, false).Truncate(127));
					line.Add("price", productPrice.FormatInvariant());
					line.Add("currency", currencyCode);
					line.Add("sku", item.Item.Product.Sku.Truncate(50));
					items.Add(line);
				}

				totalOrderItems += (Math.Round(productPrice, 2) * item.Item.Quantity);
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
					// e.g. discount applied to cart total
					var line = new Dictionary<string, object>();
					line.Add("quantity", "1");
					line.Add("name", T("Plugins.SmartStore.PayPal.Other").Text.Truncate(127));
					line.Add("price", otherAmount.FormatInvariant());
					line.Add("currency", currencyCode);
					items.Add(line);
				}
			}

			// fill amount object
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

		public static Dictionary<SecurityProtocolType, string> GetSecurityProtocols()
		{
			var dic = new Dictionary<SecurityProtocolType, string>();

			foreach (SecurityProtocolType protocol in Enum.GetValues(typeof(SecurityProtocolType)))
			{
				string friendlyName = null;
				switch (protocol)
				{
					case SecurityProtocolType.Ssl3:
						friendlyName = "SSL 3.0";
						break;
					case SecurityProtocolType.Tls:
						friendlyName = "TLS 1.0";
						break;
					case SecurityProtocolType.Tls11:
						friendlyName = "TLS 1.1";
						break;
					case SecurityProtocolType.Tls12:
						friendlyName = "TLS 1.2";
						break;
					default:
						friendlyName = protocol.ToString().ToUpper();
						break;
				}

				dic.Add(protocol, friendlyName);
			}
			return dic;
		}

		public void AddOrderNote(PayPalSettingsBase settings, Order order, string anyString, bool isIpn = false)
		{
			try
			{
				if (order == null || anyString.IsEmpty() || (settings != null && !settings.AddOrderNotes))
					return;

				string[] orderNoteStrings = T("Plugins.SmartStore.PayPal.OrderNoteStrings").Text.SplitSafe(";");
				var faviconUrl = "{0}Plugins/{1}/Content/favicon.png".FormatInvariant(_services.WebHelper.GetStoreLocation(false), Plugin.SystemName);

				var sb = new StringBuilder();
				sb.AppendFormat("<img src=\"{0}\" style=\"float: left; width: 16px; height: 16px;\" />", faviconUrl);

				var note = orderNoteStrings.SafeGet(0).FormatInvariant(anyString);

				sb.AppendFormat("<span style=\"padding-left: 4px;\">{0}</span>", note);

				if (isIpn)
					order.HasNewPaymentNotification = true;

				_orderService.AddOrderNote(order, sb.ToString());
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

				if (instruct.Link.HasValue())
				{
					intro = "{0} <a href='{1}'>{2}</a>.".FormatInvariant(intro, instruct.Link, instructStrings.SafeGet((int)PayPalPaymentInstructionItem.Details));
				}

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

		public PayPalResponse CallApi(string method, string path, string accessToken, PayPalApiSettingsBase settings, string data)
		{
			var isJson = (data.HasValue() && (data.StartsWith("{") || data.StartsWith("[")));
			var encoding = (isJson ? Encoding.UTF8 : Encoding.ASCII);
			var result = new PayPalResponse();
			HttpWebResponse webResponse = null;

			var url = GetApiUrl(settings.UseSandbox) + path.EnsureStartsWith("/");

			if (method.IsCaseInsensitiveEqual("GET") && data.HasValue())
				url = url.EnsureEndsWith("?") + data;

			if (settings.SecurityProtocol.HasValue)
				ServicePointManager.SecurityProtocol = settings.SecurityProtocol.Value;

			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = method;
			request.Accept = "application/json";
			request.ContentType = (isJson ? "application/json" : "application/x-www-form-urlencoded");

			try
			{
				if (HttpContext.Current != null && HttpContext.Current.Request != null)
					request.UserAgent = HttpContext.Current.Request.UserAgent;
				else
					request.UserAgent = Plugin.SystemName;
			}
			catch { }

			if (path.EmptyNull().EndsWith("/token"))
			{
				// see https://github.com/paypal/sdk-core-dotnet/blob/master/Source/SDK/OAuthTokenCredential.cs
				byte[] credentials = Encoding.UTF8.GetBytes("{0}:{1}".FormatInvariant(settings.ClientId, settings.Secret));

				request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(credentials));
			}
			else
			{
				request.Headers["Authorization"] = "Bearer " + accessToken.EmptyNull();
			}

			request.Headers["PayPal-Partner-Attribution-Id"] = "SmartStoreAG_Cart_PayPalPlus";

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
			catch (WebException wexc)
			{
				result.Success = false;
				result.ErrorMessage = wexc.ToString();
				webResponse = wexc.Response as HttpWebResponse;
			}
			catch (Exception exception)
			{
				result.Success = false;
				result.ErrorMessage = exception.ToString();
				Logger.Log(LogLevel.Error, exception, null, null);
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
										var name = (string)result.Json.name;
										var message = (string)result.Json.message;

										if (name.IsEmpty())
											name = (string)result.Json.error;

										if (message.IsEmpty())
											message = (string)result.Json.error_description;

										result.ErrorMessage = "{0} ({1}).".FormatInvariant(message.NaIfEmpty(), name.NaIfEmpty());
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
						catch { }

						Logger.Log(LogLevel.Error, new Exception(sb.ToString()), result.ErrorMessage, null);
					}
				}
			}
			catch (Exception exception)
			{
				Logger.Log(LogLevel.Error, exception, null, null);
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
				var result = CallApi("POST", "/v1/oauth2/token", null, settings, "grant_type=client_credentials");

				if (result.Success)
				{
					session.AccessToken = (string)result.Json.access_token;

					var expireSeconds = ((string)result.Json.expires_in).ToInt(5 * 60);

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
			var result = CallApi("GET", "/v1/payments/payment/" + session.PaymentId, session.AccessToken, settings, null);

			if (result.Success && result.Json != null)
			{
				result.Id = (string)result.Json.id;
			}

			return result;
		}

		public PayPalResponse CreatePayment(
			PayPalApiSettingsBase settings,
			PayPalSessionData session,
			List<OrganizedShoppingCartItem> cart,
			string providerSystemName,
			string returnUrl,
			string cancelUrl)
		{
			var store = _services.StoreContext.CurrentStore;
			var customer = _services.WorkContext.CurrentCustomer;

			//var dateOfBirth = customer.GetAttribute<DateTime?>(SystemCustomerAttributeNames.DateOfBirth);

			var data = new Dictionary<string, object>();
			var redirectUrls = new Dictionary<string, object>();
			var payer = new Dictionary<string, object>();
			//var payerInfo = new Dictionary<string, object>();
			var transaction = new Dictionary<string, object>();
			var items = new List<Dictionary<string, object>>();
			var itemList = new Dictionary<string, object>();

			// "PayPal PLUS only supports transaction type “Sale” (instant settlement)"
			if (providerSystemName == PayPalPlusProvider.SystemName)
				data.Add("intent", "sale");
			else
				data.Add("intent", settings.TransactMode == TransactMode.AuthorizeAndCapture ? "sale" : "authorize");

			if (settings.ExperienceProfileId.HasValue())
				data.Add("experience_profile_id", settings.ExperienceProfileId);

			// redirect urls
			if (returnUrl.HasValue())
				redirectUrls.Add("return_url", returnUrl);

			if (cancelUrl.HasValue())
				redirectUrls.Add("cancel_url", cancelUrl);

			if (redirectUrls.Any())
				data.Add("redirect_urls", redirectUrls);

			// payer, payer_info
			// paypal review: do not transmit
			//if (dateOfBirth.HasValue)
			//{
			//	payerInfo.Add("birth_date", dateOfBirth.Value.ToString("yyyy-MM-dd"));
			//}
			//if (customer.BillingAddress != null)
			//{
			//	payerInfo.Add("billing_address", CreateAddress(customer.BillingAddress, false));
			//}

			payer.Add("payment_method", "paypal");
			//payer.Add("payer_info", payerInfo);
			data.Add("payer", payer);

			var amount = CreateAmount(store, customer, cart, providerSystemName, items);

			itemList.Add("items", items);

			transaction.Add("amount", amount);
			transaction.Add("item_list", itemList);
			transaction.Add("invoice_number", session.OrderGuid.ToString());

			data.Add("transactions", new List<Dictionary<string, object>> { transaction });

			var result = CallApi("POST", "/v1/payments/payment", session.AccessToken, settings, JsonConvert.SerializeObject(data));

			if (result.Success && result.Json != null)
			{
				result.Id = (string)result.Json.id;
			}

			//Logger.InsertLog(LogLevel.Information, "PayPal PLUS", JsonConvert.SerializeObject(data, Formatting.Indented) + "\r\n\r\n" + (result.Json != null ? result.Json.ToString() : ""));

			return result;
		}

		public PayPalResponse PatchShipping(
			PayPalApiSettingsBase settings,
			PayPalSessionData session,
			List<OrganizedShoppingCartItem> cart,
			string providerSystemName)
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

			// update of whole amount object required. patching single amount values not possible (MALFORMED_REQUEST).
			var amount = CreateAmount(store, customer, cart, providerSystemName, null);

			amountTotal.Add("op", "replace");
			amountTotal.Add("path", "/transactions/0/amount");
			amountTotal.Add("value", amount);
			data.Add(amountTotal);

			var result = CallApi("PATCH", "/v1/payments/payment/{0}".FormatInvariant(session.PaymentId), session.AccessToken, settings, JsonConvert.SerializeObject(data));

			//Logger.InsertLog(LogLevel.Information, "PayPal PLUS", JsonConvert.SerializeObject(data, Formatting.Indented) + "\r\n\r\n" + (result.Json != null ? result.Json.ToString() : ""));

			return result;
		}

		public PayPalResponse ExecutePayment(PayPalApiSettingsBase settings, PayPalSessionData session)
		{
			var data = new Dictionary<string, object>();
			data.Add("payer_id", session.PayerId);

			var result = CallApi("POST", "/v1/payments/payment/{0}/execute".FormatInvariant(session.PaymentId), session.AccessToken, settings, JsonConvert.SerializeObject(data));

			if (result.Success && result.Json != null)
			{
				result.Id = (string)result.Json.id;

				//Logger.InsertLog(LogLevel.Information, "PayPal PLUS", JsonConvert.SerializeObject(data, Formatting.Indented) + "\r\n\r\n" + result.Json.ToString());
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

			var result = CallApi("POST", path, session.AccessToken, settings, data.Any() ? JsonConvert.SerializeObject(data) : null);

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

			var result = CallApi("POST", path, session.AccessToken, settings, JsonConvert.SerializeObject(data));

			if (result.Success && result.Json != null)
			{
				result.Id = (string)result.Json.id;
			}

			return result;
		}

		public PayPalResponse Void(PayPalApiSettingsBase settings, PayPalSessionData session, VoidPaymentRequest request)
		{
			var path = "/v1/payments/authorization/{0}/void".FormatInvariant(request.Order.AuthorizationTransactionId);

			var result = CallApi("POST", path, session.AccessToken, settings, null);

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
			var logo = _pictureService.Value.GetPictureById(store.LogoPictureId);
			var path = "/v1/payment-experience/web-profiles";

			var data = new Dictionary<string, object>();
			var presentation = new Dictionary<string, object>();
			var inpuFields = new Dictionary<string, object>();

			// find existing profile id, only one profile per profile name possible
			if (settings.ExperienceProfileId.IsEmpty())
			{
				result = CallApi("GET", path, session.AccessToken, settings, null);
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
				presentation.Add("logo_image", _pictureService.Value.GetPictureUrl(logo, showDefaultPicture: false, storeLocation: store.Url));

			inpuFields.Add("allow_note", false);
			inpuFields.Add("no_shipping", 0);
			inpuFields.Add("address_override", 1);

			data.Add("name", name);
			data.Add("presentation", presentation);
			data.Add("input_fields", inpuFields);

			if (settings.ExperienceProfileId.HasValue())
				path = string.Concat(path, "/", HttpUtility.UrlPathEncode(settings.ExperienceProfileId));

			result = CallApi(settings.ExperienceProfileId.HasValue() ? "PUT" : "POST", path, session.AccessToken, settings, JsonConvert.SerializeObject(data));

			if (result.Success)
			{
				if (result.Json != null)
					result.Id = (string)result.Json.id;
				else
					result.Id = settings.ExperienceProfileId;
			}

			return result;
		}

		public PayPalResponse DeleteCheckoutExperience(PayPalApiSettingsBase settings, PayPalSessionData session)
		{
			var result = CallApi("DELETE", "/v1/payment-experience/web-profiles/" + settings.ExperienceProfileId, session.AccessToken, settings, null);

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

			var result = CallApi("POST", "/v1/notifications/webhooks", session.AccessToken, settings, JsonConvert.SerializeObject(data));

			if (result.Success && result.Json != null)
			{
				result.Id = (string)result.Json.id;
			}

			return result;
		}

		public PayPalResponse DeleteWebhook(PayPalApiSettingsBase settings, PayPalSessionData session)
		{
			var result = CallApi("DELETE", "/v1/notifications/webhooks/" + settings.WebhookId, session.AccessToken, settings, null);

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
						_orderProcessingService.MarkAsAuthorized(order);
					break;
				case PaymentStatus.Paid:
					if (_orderProcessingService.CanMarkOrderAsPaid(order))
						_orderProcessingService.MarkOrderAsPaid(order);
					break;
				case PaymentStatus.Refunded:
					if (_orderProcessingService.CanRefundOffline(order))
						_orderProcessingService.RefundOffline(order);
					break;
				case PaymentStatus.PartiallyRefunded:
					if (_orderProcessingService.CanPartiallyRefundOffline(order, Math.Abs(total)))
						_orderProcessingService.PartiallyRefundOffline(order, Math.Abs(total));
					break;
				case PaymentStatus.Voided:
					if (_orderProcessingService.CanVoidOffline(order))
						_orderProcessingService.VoidOffline(order);
					break;
			}

			AddOrderNote(settings, order, (string)ToInfoString(json), true);

			return HttpStatusCode.OK;
		}
	}


	public class PayPalResponse
	{
		public bool Success { get; set; }
		public dynamic Json { get; set; }
		public string ErrorMessage { get; set; }
		public string Id { get; set; }
	}

	[Serializable]
	public class PayPalSessionData
	{
		public PayPalSessionData()
		{
			OrderGuid = Guid.NewGuid();
		}

		public string AccessToken { get; set; }
		public DateTime TokenExpiration { get; set; }
		public string PaymentId { get; set; }
		public string PayerId { get; set; }
		public string ApprovalUrl { get; set; }
		public Guid OrderGuid { get; private set; }
		public PayPalPaymentInstruction PaymentInstruction { get; set; }
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
		public bool IsManualBankTransfer
		{
			get { return Type.IsCaseInsensitiveEqual("MANUAL_BANK_TRANSFER"); }
		}

		[JsonIgnore]
		public bool IsPayUponInvoice
		{
			get { return Type.IsCaseInsensitiveEqual("PAY_UPON_INVOICE"); }
		}

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