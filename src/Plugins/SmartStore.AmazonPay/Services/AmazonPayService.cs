using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using Autofac;
using OffAmazonPaymentsService;
using SmartStore.AmazonPay.Api;
using SmartStore.AmazonPay.Extensions;
using SmartStore.AmazonPay.Models;
using SmartStore.AmazonPay.Settings;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Tasks;

namespace SmartStore.AmazonPay.Services
{
	public class AmazonPayService : IAmazonPayService
	{
		private readonly IAmazonPayApi _api;
		private readonly HttpContextBase _httpContext;
		private readonly ICommonServices _services;
		private readonly IPaymentService _paymentService;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
		private readonly ICurrencyService _currencyService;
		private readonly CurrencySettings _currencySettings;
		private readonly ICustomerService _customerService;
		private readonly IPriceFormatter _priceFormatter;
		private readonly OrderSettings _orderSettings;
		private readonly RewardPointsSettings _rewardPointsSettings;
		private readonly IOrderService _orderService;
		private readonly IRepository<Order> _orderRepository;
		private readonly IOrderProcessingService _orderProcessingService;
		private readonly IScheduleTaskService _scheduleTaskService;
		private readonly IWorkflowMessageService _workflowMessageService;

		public AmazonPayService(
			IAmazonPayApi api,
			HttpContextBase httpContext,
			ICommonServices services,
			IPaymentService paymentService,
			IGenericAttributeService genericAttributeService,
			IOrderTotalCalculationService orderTotalCalculationService,
			ICurrencyService currencyService,
			CurrencySettings currencySettings,
			ICustomerService customerService,
			IPriceFormatter priceFormatter,
			OrderSettings orderSettings,
			RewardPointsSettings rewardPointsSettings,
			IOrderService orderService,
			IRepository<Order> orderRepository,
			IOrderProcessingService orderProcessingService,
			IScheduleTaskService scheduleTaskService,
			IWorkflowMessageService workflowMessageService)
		{
			_api = api;
			_httpContext = httpContext;
			_services = services;
			_paymentService = paymentService;
			_genericAttributeService = genericAttributeService;
			_orderTotalCalculationService = orderTotalCalculationService;
			_currencyService = currencyService;
			_currencySettings = currencySettings;
			_customerService = customerService;
			_priceFormatter = priceFormatter;
			_orderSettings = orderSettings;
			_rewardPointsSettings = rewardPointsSettings;
			_orderService = orderService;
			_orderRepository = orderRepository;
			_orderProcessingService = orderProcessingService;
			_scheduleTaskService = scheduleTaskService;
			_workflowMessageService = workflowMessageService;

			T = NullLocalizer.Instance;
			Logger = NullLogger.Instance;
		}

		public Localizer T { get; set; }
		public ILogger Logger { get; set; }

		private string GetPluginUrl(string action, bool useSsl = false)
		{
			string pluginUrl = "{0}Plugins/SmartStore.AmazonPay/AmazonPay/{1}".FormatWith(_services.WebHelper.GetStoreLocation(useSsl), action);
			return pluginUrl;
		}

		//private decimal? GetOrderTotal()
		//{
		//	decimal orderTotalDiscountAmountBase = decimal.Zero;
		//	Discount orderTotalAppliedDiscount = null;
		//	List<AppliedGiftCard> appliedGiftCards = null;
		//	int redeemedRewardPoints = 0;
		//	decimal redeemedRewardPointsAmount = decimal.Zero;

		//	var cart = _services.WorkContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _services.StoreContext.CurrentStore.Id);

		//	decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart,
		//		out orderTotalDiscountAmountBase, out orderTotalAppliedDiscount, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount);

		//	if (shoppingCartTotalBase.HasValue)		// shipping method needs to be selected here!
		//	{
		//		decimal shoppingCartTotal = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, _services.WorkContext.WorkingCurrency);

		//		return shoppingCartTotal;
		//	}
		//	return null;
		//}

		private void SerializeOrderAttribute(AmazonPayOrderAttribute attribute, Order order)
		{
			if (attribute != null)
			{
				var sb = new StringBuilder();
				using (var writer = new StringWriter(sb))
				{
					var serializer = new XmlSerializer(typeof(AmazonPayOrderAttribute));
					serializer.Serialize(writer, attribute);

					_genericAttributeService.SaveAttribute<string>(order, AmazonPayCore.AmazonPayOrderAttributeKey, sb.ToString(), order.StoreId);
				}
			}
		}
		private AmazonPayOrderAttribute DeserializeOrderAttribute(Order order)
		{
			var serialized = order.GetAttribute<string>(AmazonPayCore.AmazonPayOrderAttributeKey, _genericAttributeService, order.StoreId);

			if (!serialized.HasValue())
			{
				var attribute = new AmazonPayOrderAttribute();
				
				// legacy < v.1.14
				attribute.OrderReferenceId = order.GetAttribute<string>(AmazonPayCore.SystemName + ".OrderReferenceId", order.StoreId);

				return attribute;
			}

			using (var reader = new StringReader(serialized))
			{
				var serializer = new XmlSerializer(typeof(AmazonPayOrderAttribute));
				return (AmazonPayOrderAttribute)serializer.Deserialize(reader);
			}
		}

		public void LogError(Exception exception, string shortMessage = null, string fullMessage = null, bool notify = false, IList<string> errors = null)
		{
			try
			{
				if (exception != null)
				{
					shortMessage = exception.Message;
					fullMessage = exception.ToString();
					exception.Dump();
				}

				if (shortMessage.HasValue())
				{
					Logger.InsertLog(LogLevel.Error, shortMessage, fullMessage.EmptyNull());

					if (notify)
						_services.Notifier.Error(new LocalizedString(shortMessage));
				}
			}
			catch (Exception) { }

			if (errors != null && shortMessage.HasValue())
				errors.Add(shortMessage);
		}
		public void LogAmazonError(OffAmazonPaymentsServiceException exception, bool notify = false, IList<string> errors = null)
		{
			try
			{
				string shortMessage, fullMessage;

				if (exception.GetErrorStrings(out shortMessage, out fullMessage))
				{
					Logger.InsertLog(LogLevel.Error, shortMessage, fullMessage);

					if (notify)
						_services.Notifier.Error(new LocalizedString(shortMessage));

					if (errors != null)
						errors.Add(shortMessage);
				}
			}
			catch (Exception) { }
		}

		private bool IsActive(int storeId, bool logInactive = false)
		{
			bool isActive = _paymentService.IsPaymentMethodActive(AmazonPayCore.SystemName, storeId);

			if (!isActive && logInactive)
			{
				LogError(null, T("Plugins.Payments.AmazonPay.PaymentMethodNotActive", _services.StoreContext.CurrentStore.Name));
			}
			return isActive;
		}

		public void AddOrderNote(AmazonPaySettings settings, Order order, AmazonPayOrderNote note, string anyString = null, bool isIpn = false)
		{
			try
			{
				if (!settings.AddOrderNotes || order == null)
					return;

				var sb = new StringBuilder();

				string[] orderNoteStrings = T("Plugins.Payments.AmazonPay.OrderNoteStrings").Text.SplitSafe(";");
				string faviconUrl = "{0}Plugins/{1}/Content/images/favicon.png".FormatWith(_services.WebHelper.GetStoreLocation(false), AmazonPayCore.SystemName);

				sb.AppendFormat("<img src=\"{0}\" style=\"float: left; width: 16px; height: 16px;\" />", faviconUrl);

				if (anyString.HasValue())
				{
					anyString = orderNoteStrings.SafeGet((int)note).FormatWith(anyString);
				}
				else
				{
					anyString = orderNoteStrings.SafeGet((int)note);
					anyString = anyString.Replace("{0}", "");
				}

				if (anyString.HasValue())
				{
					sb.AppendFormat("<span style=\"padding-left: 4px;\">{0}</span>", anyString);
				}

				if (isIpn)
					order.HasNewPaymentNotification = true;

				order.OrderNotes.Add(new OrderNote
				{
					Note = sb.ToString(),
					DisplayToCustomer = false,
					CreatedOnUtc = DateTime.UtcNow
				});

				_orderService.UpdateOrder(order);
			}
			catch (Exception exc)
			{
				LogError(exc);
			}
		}

		public void SetupConfiguration(ConfigurationModel model)
		{
			model.DataFetchings = new List<SelectListItem>()
			{
				new SelectListItem
				{
					Text = T("Common.Unspecified").Text,
					Value = ""
				},
				new SelectListItem
				{
					Selected = (model.DataFetching == AmazonPayDataFetchingType.Ipn),
					Text = T("Plugins.Payments.AmazonPay.DataFetching.Ipn"),
					Value = ((int)AmazonPayDataFetchingType.Ipn).ToString()
				},
				new SelectListItem
				{
					Selected = (model.DataFetching == AmazonPayDataFetchingType.Polling),
					Text = T("Plugins.Payments.AmazonPay.DataFetching.Polling"),
					Value = ((int)AmazonPayDataFetchingType.Polling).ToString()
				}
			};

			model.TransactionTypes = new List<SelectListItem>()
			{
				new SelectListItem
				{
					Selected = (model.TransactionType == AmazonPayTransactionType.AuthorizeAndCapture),
					Text = T("Plugins.Payments.AmazonPay.TransactionType.AuthAndCapture"),
					Value = ((int)AmazonPayTransactionType.AuthorizeAndCapture).ToString()
				},
				new SelectListItem
				{
					Selected = (model.TransactionType == AmazonPayTransactionType.Authorize),
					Text = T("Plugins.Payments.AmazonPay.TransactionType.Auth"),
					Value = ((int)AmazonPayTransactionType.Authorize).ToString()
				}
			};

			model.SaveEmailAndPhones = new List<SelectListItem>()
			{
				new SelectListItem
				{
					Text = T("Common.Unspecified").Text,
					Value = ""
				},
				new SelectListItem
				{
					Selected = (model.SaveEmailAndPhone == AmazonPaySaveDataType.OnlyIfEmpty),
					Text = T("Plugins.Payments.AmazonPay.AmazonPaySaveDataType.OnlyIfEmpty"),
					Value = ((int)AmazonPaySaveDataType.OnlyIfEmpty).ToString()
				},
				new SelectListItem
				{
					Selected = (model.SaveEmailAndPhone == AmazonPaySaveDataType.Always),
					Text = T("Plugins.Payments.AmazonPay.AmazonPaySaveDataType.Always"),
					Value = ((int)AmazonPaySaveDataType.Always).ToString()
				}
			};

			model.IpnUrl = GetPluginUrl("IPNHandler", _services.StoreContext.CurrentStore.SslEnabled);

			model.ConfigGroups = T("Plugins.Payments.AmazonPay.ConfigGroups").Text.SplitSafe(";");

			var task = _scheduleTaskService.GetTaskByType(AmazonPayCore.DataPollingTaskType);

			if (task == null)
				model.PollingTaskMinutes = 30;
			else
				model.PollingTaskMinutes = 30; // (task.Seconds / 60);
		}

		public string GetWidgetUrl()
		{
			try
			{
				var store = _services.StoreContext.CurrentStore;

				if (IsActive(store.Id))
				{
					var settings = _services.Settings.LoadSetting<AmazonPaySettings>(store.Id);
					if (settings.SellerId.HasValue())
						return settings.GetWidgetUrl();
				}
			}
			catch (Exception exc)
			{
				LogError(exc);
			}
			return "";
		}

		public AmazonPayViewModel ProcessPluginRequest(AmazonPayRequestType type, TempDataDictionary tempData, string orderReferenceId = null)
		{
			var model = new AmazonPayViewModel();
			model.Type = type;

			try
			{
				var store = _services.StoreContext.CurrentStore;
				var customer = _services.WorkContext.CurrentCustomer;
				var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

				if (type == AmazonPayRequestType.LoginHandler)
				{
					if (string.IsNullOrWhiteSpace(orderReferenceId))
					{
						LogError(null, T("Plugins.Payments.AmazonPay.MissingOrderReferenceId"), null, true);
						model.Result = AmazonPayResultType.Redirect;
						return model;
					}

					if (cart.Count <= 0 || !IsActive(store.Id))
					{
						model.Result = AmazonPayResultType.Redirect;
						return model;
					}

					if (customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
					{
						model.Result = AmazonPayResultType.Unauthorized;
						return model;
					}

					var checkoutState = _httpContext.GetCheckoutState();

					if (checkoutState == null)
					{
						Logger.InsertLog(LogLevel.Warning, "Checkout state is null in AmazonPayService.ValidateAndInitiateCheckout!");
						model.Result = AmazonPayResultType.Redirect;
						return model;
					}

					var state = new AmazonPayCheckoutState()
					{
						OrderReferenceId = orderReferenceId
					};

					if (checkoutState.CustomProperties.ContainsKey(AmazonPayCore.AmazonPayCheckoutStateKey))
						checkoutState.CustomProperties[AmazonPayCore.AmazonPayCheckoutStateKey] = state;
					else
						checkoutState.CustomProperties.Add(AmazonPayCore.AmazonPayCheckoutStateKey, state);

					//_httpContext.Session.SafeSet(AmazonPayCore.AmazonPayCheckoutStateKey, state);

					model.RedirectAction = "Index";
					model.RedirectController = "Checkout";
					model.Result = AmazonPayResultType.Redirect;
					return model;
				}
				else if (type == AmazonPayRequestType.ShoppingCart || type == AmazonPayRequestType.MiniShoppingCart)
				{
					if (cart.Count <= 0 || !IsActive(store.Id))
					{
						model.Result = AmazonPayResultType.None;
						return model;
					}

					string storeLocation = _services.WebHelper.GetStoreLocation(store.SslEnabled);
					model.LoginHandlerUrl = "{0}Plugins/SmartStore.AmazonPay/AmazonPayShoppingCart/LoginHandler".FormatWith(storeLocation);
				}
				else
				{
					if (!_httpContext.HasAmazonPayState() || cart.Count <= 0)
					{
						model.Result = AmazonPayResultType.Redirect;
						return model;
					}

					if (customer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed)
					{
						model.Result = AmazonPayResultType.Unauthorized;
						return model;
					}

					var state = _httpContext.GetAmazonPayState(_services.Localization);

					model.OrderReferenceId = state.OrderReferenceId;
					//model.IsOrderConfirmed = state.IsOrderConfirmed;
				}

				var currency = store.PrimaryStoreCurrency;
				var settings = _services.Settings.LoadSetting<AmazonPaySettings>(store.Id);

				model.SellerId = settings.SellerId;
				model.ClientId = settings.AccessKey;
				model.IsShippable = cart.RequiresShipping();
				model.IsRecurring = cart.IsRecurring();
				model.WidgetUrl = settings.GetWidgetUrl();
				model.ButtonUrl = settings.GetButtonUrl(type);
				model.AddressWidgetWidth = Math.Max(settings.AddressWidgetWidth, 200);
				model.AddressWidgetHeight = Math.Max(settings.AddressWidgetHeight, 228);
				model.PaymentWidgetWidth = Math.Max(settings.PaymentWidgetWidth, 200);
				model.PaymentWidgetHeight = Math.Max(settings.PaymentWidgetHeight, 228);

				if (type == AmazonPayRequestType.MiniShoppingCart)
				{
					if (!settings.ShowButtonInMiniShoppingCart)
					{
						model.Result = AmazonPayResultType.None;
						return model;
					}
				}
				else if (type == AmazonPayRequestType.Address)
				{
					if (!model.IsShippable)
					{
						model.RedirectAction = "ShippingMethod";
						model.RedirectController = "Checkout";
						model.Result = AmazonPayResultType.Redirect;
						return model;
					}

					var shippingToCountryNotAllowed = tempData[AmazonPayCore.SystemName + "ShippingToCountryNotAllowed"];

					if (shippingToCountryNotAllowed != null && true == (bool)shippingToCountryNotAllowed)
						model.Warning = T("Plugins.Payments.AmazonPay.ShippingToCountryNotAllowed");
				}
				else if (type == AmazonPayRequestType.ShippingMethod)
				{
					model.RedirectAction = model.RedirectController = "";

					if (model.IsShippable)
					{
						var client = new AmazonPayClient(settings);
						var details = _api.GetOrderReferenceDetails(client, model.OrderReferenceId);

						if (_api.FindAndApplyAddress(details, customer, model.IsShippable, true))
						{
							_customerService.UpdateCustomer(customer);
							model.Result = AmazonPayResultType.None;
							return model;
						}
						else
						{
							tempData[AmazonPayCore.SystemName + "ShippingToCountryNotAllowed"] = true;
							model.RedirectAction = "ShippingAddress";
							model.RedirectController = "Checkout";
							model.Result = AmazonPayResultType.Redirect;
							return model;
						}
					}
				}
				else if (type == AmazonPayRequestType.Payment)
				{
					if (_rewardPointsSettings.Enabled && !model.IsRecurring)
					{
						int rewardPointsBalance = customer.GetRewardPointsBalance();
						decimal rewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
						decimal rewardPointsAmount = _currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, currency);

						if (rewardPointsAmount > decimal.Zero)
						{
							model.DisplayRewardPoints = true;
							model.RewardPointsAmount = _priceFormatter.FormatPrice(rewardPointsAmount, true, false);
							model.RewardPointsBalance = rewardPointsBalance;
						}
					}

					_genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, AmazonPayCore.SystemName, store.Id);

					var client = new AmazonPayClient(settings);
					var unused = _api.SetOrderReferenceDetails(client, model.OrderReferenceId, store.PrimaryStoreCurrency.CurrencyCode, cart);

					// this is ugly...
					var paymentRequest = _httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
					if (paymentRequest == null)
					{
						_httpContext.Session["OrderPaymentInfo"] = new ProcessPaymentRequest();
					}
				}
				else if (type == AmazonPayRequestType.OrderReviewData)
				{
					if (model.IsShippable)
					{
						var shippingOption = customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, store.Id);
						if (shippingOption != null)
							model.ShippingMethod = shippingOption.Name;
					}
				}
			}
			catch (OffAmazonPaymentsServiceException exc)
			{
				LogAmazonError(exc, notify: true);
			}
			catch (Exception exc)
			{
				LogError(exc, notify: true);
			}
			return model;
		}

		public void ApplyRewardPoints(bool useRewardPoints)
		{
			if (_rewardPointsSettings.Enabled)
			{
				_genericAttributeService.SaveAttribute(_services.WorkContext.CurrentCustomer,
					SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, useRewardPoints, _services.StoreContext.CurrentStore.Id);
			}
		}

		private string GetAuthorizationState(AmazonPayClient client, string authorizationId)
		{
			try
			{
				if (authorizationId.HasValue())
				{
					AmazonPayApiData data;

					if (_api.GetAuthorizationDetails(client, authorizationId, out data) != null)
						return data.State;
				}
			}
			catch (OffAmazonPaymentsServiceException exc)
			{
				LogAmazonError(exc);
			}
			catch (Exception exc)
			{
				LogError(exc);
			}
			return null;
		}

		private void CloseOrderReference(AmazonPaySettings settings, Order order)
		{
			// You can still perform captures against any open authorizations, but you cannot create any new authorizations on the
			// Order Reference object. You can still execute refunds against the Order Reference object.

			try
			{
				var client = new AmazonPayClient(settings);

				var orderAttribute = DeserializeOrderAttribute(order);

				_api.CloseOrderReference(client, orderAttribute.OrderReferenceId);
			}
			catch (OffAmazonPaymentsServiceException exc)
			{
				LogAmazonError(exc);
			}
			catch (Exception exc)
			{
				LogError(exc);
			}
		}

		private void ProcessAuthorizationResult(AmazonPayClient client, Order order, AmazonPayApiData data, OffAmazonPaymentsService.Model.AuthorizationDetails details)
		{
			string formattedAddress;
			var orderAttribute = DeserializeOrderAttribute(order);

			if (!orderAttribute.IsBillingAddressApplied)
			{
				if (_api.FulfillBillingAddress(client.Settings, order, details, out formattedAddress))
				{
					AddOrderNote(client.Settings, order, AmazonPayOrderNote.BillingAddressApplied, formattedAddress);

					orderAttribute.IsBillingAddressApplied = true;
					SerializeOrderAttribute(orderAttribute, order);
				}
				else if (formattedAddress.HasValue())
				{
					AddOrderNote(client.Settings, order, AmazonPayOrderNote.BillingAddressCountryNotAllowed, formattedAddress);

					orderAttribute.IsBillingAddressApplied = true;
					SerializeOrderAttribute(orderAttribute, order);
				}
			}

			if (data.State.IsCaseInsensitiveEqual("Pending"))
				return;

			string newResult = data.State.Grow(data.ReasonCode, " ");

			if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
			{
				_orderProcessingService.MarkAsAuthorized(order);
			}

			if (data.State.IsCaseInsensitiveEqual("Closed") && data.ReasonCode.IsCaseInsensitiveEqual("OrderReferenceCanceled") && _orderProcessingService.CanVoidOffline(order))
			{
				_orderProcessingService.VoidOffline(order);		// cancelation at amazon seller central
			}
			else if (data.State.IsCaseInsensitiveEqual("Declined") && _orderProcessingService.CanVoidOffline(order))
			{
				_orderProcessingService.VoidOffline(order);
			}

			if (!newResult.IsCaseInsensitiveEqual(order.AuthorizationTransactionResult))
			{
				order.AuthorizationTransactionResult = newResult;

				if (order.CaptureTransactionId.IsEmpty() && data.CaptureId.HasValue())
					order.CaptureTransactionId = data.CaptureId;	// captured at amazon seller central

				_orderService.UpdateOrder(order);

				AddOrderNote(client.Settings, order, AmazonPayOrderNote.AmazonMessageProcessed, _api.ToInfoString(data), true);
			}
		}
		private void ProcessCaptureResult(AmazonPayClient client, Order order, AmazonPayApiData data)
		{
			if (data.State.IsCaseInsensitiveEqual("Pending"))
				return;

			string newResult = data.State.Grow(data.ReasonCode, " ");

			if (data.State.IsCaseInsensitiveEqual("Completed") && _orderProcessingService.CanMarkOrderAsPaid(order))
			{
				_orderProcessingService.MarkOrderAsPaid(order);

				CloseOrderReference(client.Settings, order);
			}
			else if (data.State.IsCaseInsensitiveEqual("Declined") && _orderProcessingService.CanVoidOffline(order))
			{
				if (!GetAuthorizationState(client, order.AuthorizationTransactionId).IsCaseInsensitiveEqual("Open"))
				{
					_orderProcessingService.VoidOffline(order);
				}
			}

			if (!newResult.IsCaseInsensitiveEqual(order.CaptureTransactionResult))
			{
				order.CaptureTransactionResult = newResult;
				_orderService.UpdateOrder(order);

				AddOrderNote(client.Settings, order, AmazonPayOrderNote.AmazonMessageProcessed, _api.ToInfoString(data), true);
			}
		}
		private void ProcessRefundResult(AmazonPayClient client, Order order, AmazonPayApiData data)
		{
			if (data.State.IsCaseInsensitiveEqual("Pending"))
				return;

			if (data.RefundedAmount != null && data.RefundedAmount.Amount != 0.0)	// totally refunded amount
			{
				// we could only process it once cause otherwise order.RefundedAmount would getting wrong.
				if (order.RefundedAmount == decimal.Zero)
				{
					decimal refundAmount = Convert.ToDecimal(data.RefundedAmount.Amount);
					decimal receivable = order.OrderTotal - refundAmount;

					if (receivable <= decimal.Zero)
					{
						if (_orderProcessingService.CanRefundOffline(order))
						{
							_orderProcessingService.RefundOffline(order);

							if (client.Settings.DataFetching == AmazonPayDataFetchingType.Polling)
								AddOrderNote(client.Settings, order, AmazonPayOrderNote.AmazonMessageProcessed, _api.ToInfoString(data), true);
						}
					}
					else
					{
						if (_orderProcessingService.CanPartiallyRefundOffline(order, refundAmount))
						{
							_orderProcessingService.PartiallyRefundOffline(order, refundAmount);

							if (client.Settings.DataFetching == AmazonPayDataFetchingType.Polling)
								AddOrderNote(client.Settings, order, AmazonPayOrderNote.AmazonMessageProcessed, _api.ToInfoString(data), true);
						}
					}
				}
			}

			if (client.Settings.DataFetching == AmazonPayDataFetchingType.Ipn)
				AddOrderNote(client.Settings, order, AmazonPayOrderNote.AmazonMessageProcessed, _api.ToInfoString(data), true);
		}

		private void PollingLoop(PollingLoopData data, Func<bool> poll)
		{
			try
			{
				var sleepMillSec = 8000;
				var loopMillSec = 90000;
				var startTime = DateTime.Now.TimeOfDay;

				for (int i = 0; i < 99 && (DateTime.Now.TimeOfDay.Milliseconds - startTime.Milliseconds) <= loopMillSec; ++i)
				{
					// inside the loop cause other instances are also updating the order
					data.Order = _orderService.GetOrderById(data.OrderId);

					if (data.Settings == null)
						data.Settings = _services.Settings.LoadSetting<AmazonPaySettings>(data.Order.StoreId);

					if (data.Client == null)
						data.Client = new AmazonPayClient(data.Settings);

					if (!poll())
						break;

					Thread.Sleep(sleepMillSec);
				}
			}
			catch (OffAmazonPaymentsServiceException exc)
			{
				LogAmazonError(exc);
			}
			catch (Exception exc)
			{
				LogError(exc);
			}
		}
		private void EarlyPolling(int orderId, AmazonPaySettings settings)
		{
			// the Authorization object moves to the Open state after remaining in the Pending state for 30 seconds.

			AmazonPayApiData data;
			var d = new PollingLoopData(orderId);
			d.Settings = settings;

			PollingLoop(d, () =>
			{
				if (d.Order.AuthorizationTransactionId.IsEmpty())
					return false;

				var details = _api.GetAuthorizationDetails(d.Client, d.Order.AuthorizationTransactionId, out data);

				if (!data.State.IsCaseInsensitiveEqual("pending"))
				{
					ProcessAuthorizationResult(d.Client, d.Order, data, details);
					return false;
				}
				return true;
			});


			PollingLoop(d, () =>
			{
				if (d.Order.CaptureTransactionId.IsEmpty())
					return false;

				_api.GetCaptureDetails(d.Client, d.Order.CaptureTransactionId, out data);

				ProcessCaptureResult(d.Client, d.Order, data);

				return data.State.IsCaseInsensitiveEqual("pending");
			});
		}

		private Order FindOrder(AmazonPayApiData data)
		{
			Order order = null;
			string errorId = null;

			if (data.MessageType.IsCaseInsensitiveEqual("AuthorizationNotification"))
			{
				if ((order = _orderService.GetOrderByPaymentAuthorization(AmazonPayCore.SystemName, data.AuthorizationId)) == null)
					errorId = "AuthorizationId {0}".FormatWith(data.AuthorizationId);
			}
			else if (data.MessageType.IsCaseInsensitiveEqual("CaptureNotification"))
			{
				if ((order = _orderService.GetOrderByPaymentCapture(AmazonPayCore.SystemName, data.CaptureId)) == null)
					order = _orderRepository.GetOrderByAmazonId(data.AnyAmazonId);

				if (order == null)
					errorId = "CaptureId {0}".FormatWith(data.CaptureId);
			}
			else if (data.MessageType.IsCaseInsensitiveEqual("RefundNotification"))
			{
				var attribute = _genericAttributeService.GetAttributes(AmazonPayCore.AmazonPayRefundIdKey, "Order")
					.Where(x => x.Value == data.RefundId)
					.FirstOrDefault();

				if (attribute == null || (order = _orderService.GetOrderById(attribute.EntityId)) == null)
					order = _orderRepository.GetOrderByAmazonId(data.AnyAmazonId);

				if (order == null)
					errorId = "RefundId {0}".FormatWith(data.RefundId);
			}

			if (errorId.HasValue())
				Logger.InsertLog(LogLevel.Warning, T("Plugins.Payments.AmazonPay.OrderNotFound", errorId), "");

			return order;
		}
		public void AddCustomerOrderNoteLoop(AmazonPayActionState state)
		{
			try
			{
				var sleepMillSec = 4000;
				var loopMillSec = 40000;
				var startTime = DateTime.Now.TimeOfDay;

				for (int i = 0; i < 99 && (DateTime.Now.TimeOfDay.Milliseconds - startTime.Milliseconds) <= loopMillSec; ++i)
				{
					var order = _orderService.GetOrderByGuid(state.OrderGuid);
					if (order != null)
					{
						var sb = new StringBuilder(T("Plugins.Payments.AmazonPay.AuthorizationHardDeclineMessage"));

						if (state.Errors != null)
						{
							foreach (var error in state.Errors)
								sb.AppendFormat("<p>{0}</p>", error);
						}

						var orderNote = new OrderNote()
						{
							DisplayToCustomer = true,
							Note = sb.ToString(),
							CreatedOnUtc = DateTime.UtcNow,
						};

						order.OrderNotes.Add(orderNote);
						_orderService.UpdateOrder(order);

						_workflowMessageService.SendNewOrderNoteAddedCustomerNotification(orderNote, _services.WorkContext.WorkingLanguage.Id);
						break;
					}

					Thread.Sleep(sleepMillSec);
				}
			}
			catch (Exception exc)
			{
				LogError(exc);
			}
		}

		public PreProcessPaymentResult PreProcessPayment(ProcessPaymentRequest request)
		{
			// fulfill the Amazon checkout
			var result = new PreProcessPaymentResult();

			try
			{
				var orderGuid = request.OrderGuid.ToString();
				var store = _services.StoreService.GetStoreById(request.StoreId);
				var customer = _customerService.GetCustomerById(request.CustomerId);
				var currency = store.PrimaryStoreCurrency;
				var settings = _services.Settings.LoadSetting<AmazonPaySettings>(store.Id);
				var state = _httpContext.GetAmazonPayState(_services.Localization);
				var client = new AmazonPayClient(settings);

				if (!IsActive(store.Id, true))
				{
					//_httpContext.ResetCheckoutState();

					result.AddError(T("Plugins.Payments.AmazonPay.PaymentMethodNotActive", store.Name));
					return result;
				}

				var preConfirmDetails = _api.SetOrderReferenceDetails(client, state.OrderReferenceId, request.OrderTotal, currency.CurrencyCode, orderGuid, store.Name);

				_api.GetConstraints(preConfirmDetails, result.Errors);

				if (!result.Success)
					return result;

				_api.ConfirmOrderReference(client, state.OrderReferenceId);

				// address and payment cannot be changed if order is in open state, amazon widgets then might show an error.
				//state.IsOrderConfirmed = true;

				var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);
				var isShippable = cart.RequiresShipping();

				// note: billing address is only available after authorization is in a non-pending and non-declined state.
				var details = _api.GetOrderReferenceDetails(client, state.OrderReferenceId);

				_api.FindAndApplyAddress(details, customer, isShippable, false);

				if (details.IsSetBuyer() && details.Buyer.IsSetEmail() && settings.CanSaveEmailAndPhone(customer.Email))
				{
					customer.Email = details.Buyer.Email;
				}

				_customerService.UpdateCustomer(customer);

				if (details.IsSetBuyer() && details.Buyer.IsSetPhone() && settings.CanSaveEmailAndPhone(customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone, store.Id)))
				{
					_genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.Phone, details.Buyer.Phone);
				}
			}
			catch (OffAmazonPaymentsServiceException exc)
			{
				LogAmazonError(exc, errors: result.Errors);
			}
			catch (Exception exc)
			{
				LogError(exc, errors: result.Errors);
			}

			return result;
		}

		public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest request)
		{
			// initiate Amazon payment. We do not add errors to request.Errors cause of asynchronous processing.
			var result = new ProcessPaymentResult();
			var errors = new List<string>();
			bool informCustomerAboutErrors = false;
			bool informCustomerAddErrors = false;

			try
			{
				var orderGuid = request.OrderGuid.ToString();
				var store = _services.StoreService.GetStoreById(request.StoreId);
				var currency = store.PrimaryStoreCurrency;
				var settings = _services.Settings.LoadSetting<AmazonPaySettings>(store.Id);
				var state = _httpContext.GetAmazonPayState(_services.Localization);
				var client = new AmazonPayClient(settings);

				informCustomerAboutErrors = settings.InformCustomerAboutErrors;
				informCustomerAddErrors = settings.InformCustomerAddErrors;

				_api.Authorize(client, result, errors, state.OrderReferenceId, request.OrderTotal, currency.CurrencyCode, orderGuid);
			}
			catch (OffAmazonPaymentsServiceException exc)
			{
				LogAmazonError(exc, errors: errors);
			}
			catch (Exception exc)
			{
				LogError(exc, errors: errors);
			}

			if (informCustomerAboutErrors && errors != null && errors.Count > 0)
			{
				// customer needs to be informed of an amazon error here. hooking OrderPlaced.CustomerNotification won't work
				// cause of asynchronous processing. solution: we add a customer order note that is also send as an email.

				var state = new AmazonPayActionState() { OrderGuid = request.OrderGuid };

				if (informCustomerAddErrors)
				{
					state.Errors = new List<string>();
					state.Errors.AddRange(errors);
				}

				AsyncRunner.Run((container, ct, o) =>
				{
					var obj = o as AmazonPayActionState;
					container.Resolve<IAmazonPayService>().AddCustomerOrderNoteLoop(obj);
				},
				state, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
			}

			return result;
		}

		public void PostProcessPayment(PostProcessPaymentRequest request)
		{
			// early polling... note we do not have the amazon billing address yet
			//try
			//{
			//	int orderId = request.Order.Id;
			//	var settings = _services.Settings.LoadSetting<AmazonPaySettings>(request.Order.StoreId);

			//	if (orderId != 0 && settings.StatusFetching == AmazonPayStatusFetchingType.Polling)
			//	{
			//		AsyncRunner.Run((container, obj) =>
			//		{
			//			var amazonService = container.Resolve<IAmazonPayService>();
			//			amazonService.EarlyPolling(orderId, obj as AmazonPaySettings);
			//		},
			//		settings, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
			//	}
			//}
			//catch (OffAmazonPaymentsServiceException exc)
			//{
			//	LogAmazonError(exc);
			//}
			//catch (Exception exc)
			//{
			//	LogError(exc);
			//}

			try
			{
				var state = _httpContext.GetAmazonPayState(_services.Localization);

				var orderAttribute = new AmazonPayOrderAttribute()
				{
					OrderReferenceId = state.OrderReferenceId
				};

				SerializeOrderAttribute(orderAttribute, request.Order);
			}
			catch (Exception exc)
			{
				LogError(exc);
			}
		}

		public CapturePaymentResult Capture(CapturePaymentRequest request)
		{
			var result = new CapturePaymentResult()
			{
				NewPaymentStatus = request.Order.PaymentStatus
			};

			try
			{
				var settings = _services.Settings.LoadSetting<AmazonPaySettings>(request.Order.StoreId);
				var client = new AmazonPayClient(settings);

				_api.Capture(client, request, result);
			}
			catch (OffAmazonPaymentsServiceException exc)
			{
				LogAmazonError(exc, errors: result.Errors);
			}
			catch (Exception exc)
			{
				LogError(exc, errors: result.Errors);
			}
			return result;
		}

		public RefundPaymentResult Refund(RefundPaymentRequest request)
		{
			var result = new RefundPaymentResult()
			{
				NewPaymentStatus = request.Order.PaymentStatus
			};

			try
			{
				var settings = _services.Settings.LoadSetting<AmazonPaySettings>(request.Order.StoreId);
				var client = new AmazonPayClient(settings);

				string amazonRefundId = _api.Refund(client, request, result);

				if (amazonRefundId.HasValue() && request.Order.Id != 0)
				{
					_genericAttributeService.InsertAttribute(new GenericAttribute()
					{
						EntityId = request.Order.Id,
						KeyGroup = "Order",
						Key = AmazonPayCore.AmazonPayRefundIdKey,
						Value = amazonRefundId,
						StoreId = request.Order.StoreId
					});
				}
			}
			catch (OffAmazonPaymentsServiceException exc)
			{
				LogAmazonError(exc, errors: result.Errors);
			}
			catch (Exception exc)
			{
				LogError(exc, errors: result.Errors);
			}
			return result;
		}

		public VoidPaymentResult Void(VoidPaymentRequest request)
		{
			var result = new VoidPaymentResult()
			{
				NewPaymentStatus = request.Order.PaymentStatus
			};

			// redundant... cause payment infrastructure hides "void" and displays "refund" instead.
			//if (request.Order.PaymentStatus == PaymentStatus.Paid)
			//{
			//	var refundRequest = new RefundPaymentRequest()
			//	{
			//		Order = request.Order,
			//		IsPartialRefund = false,
			//		AmountToRefund = request.Order.OrderTotal
			//	};

			//	var refundResult = Refund(refundRequest);

			//	result.Errors.AddRange(refundResult.Errors);
			//	return result;
			//}

			try
			{
				if (request.Order.PaymentStatus == PaymentStatus.Pending || request.Order.PaymentStatus == PaymentStatus.Authorized)
				{
					var settings = _services.Settings.LoadSetting<AmazonPaySettings>(request.Order.StoreId);
					var client = new AmazonPayClient(settings);

					var orderAttribute = DeserializeOrderAttribute(request.Order);

					_api.CancelOrderReference(client, orderAttribute.OrderReferenceId);
				}
			}
			catch (OffAmazonPaymentsServiceException exc)
			{
				LogAmazonError(exc, errors: result.Errors);
			}
			catch (Exception exc)
			{
				LogError(exc, errors: result.Errors);
			}
			return result;
		}

		public void ProcessIpn(HttpRequestBase request)
		{
			try
			{
				var data = _api.ParseNotification(request);
				var order = FindOrder(data);

				if (order == null || !IsActive(order.StoreId))
					return;

				var client = new AmazonPayClient(_services.Settings.LoadSetting<AmazonPaySettings>(order.StoreId));

				if (client.Settings.DataFetching != AmazonPayDataFetchingType.Ipn)
					return;

				if (data.MessageType.IsCaseInsensitiveEqual("AuthorizationNotification"))
				{
					ProcessAuthorizationResult(client, order, data, null);
					return;
				}
				else if (data.MessageType.IsCaseInsensitiveEqual("CaptureNotification"))
				{
					ProcessCaptureResult(client, order, data);
					return;
				}
				else if (data.MessageType.IsCaseInsensitiveEqual("RefundNotification"))
				{
					ProcessRefundResult(client, order, data);
					return;
				}
			}
			catch (OffAmazonPaymentsServiceException exc)
			{
				LogAmazonError(exc);
			}
			catch (Exception exc)
			{
				LogError(exc);
			}
		}

		public void DataPollingTaskProcess()
		{
			try
			{
				// ignore cancelled and completed (paid and shipped) orders. ignore old orders too.

				var data = new AmazonPayApiData();
				int pollingMaxOrderCreationDays = _services.Settings.GetSettingByKey<int>("AmazonPaySettings.PollingMaxOrderCreationDays", 31);
				var isTooOld = DateTime.UtcNow.AddDays(-(pollingMaxOrderCreationDays));

				var query =
					from x in _orderRepository.Table
					where x.PaymentMethodSystemName == AmazonPayCore.SystemName && x.CreatedOnUtc > isTooOld &&
						!x.Deleted && x.OrderStatusId < (int)OrderStatus.Complete && x.PaymentStatusId != (int)PaymentStatus.Voided
					orderby x.Id descending
					select x;

				var orders = query.ToList();

				//"- start polling {0} orders".FormatWith(orders.Count).Dump();

				foreach (var order in orders)
				{
					try
					{
						var client = new AmazonPayClient(_services.Settings.LoadSetting<AmazonPaySettings>(order.StoreId));

						if (client.Settings.DataFetching == AmazonPayDataFetchingType.Polling)
						{
							if (order.AuthorizationTransactionId.HasValue())
							{
								var details = _api.GetAuthorizationDetails(client, order.AuthorizationTransactionId, out data);

								ProcessAuthorizationResult(client, order, data, details);
							}

							if (order.CaptureTransactionId.HasValue())
							{
								if (_orderProcessingService.CanMarkOrderAsPaid(order) || _orderProcessingService.CanVoidOffline(order) || 
									_orderProcessingService.CanRefundOffline(order) || _orderProcessingService.CanPartiallyRefundOffline(order, 0.01M))
								{
									var details = _api.GetCaptureDetails(client, order.CaptureTransactionId, out data);

									ProcessCaptureResult(client, order, data);

									if (_orderProcessingService.CanRefundOffline(order) || _orderProcessingService.CanPartiallyRefundOffline(order, 0.01M))
									{
										// note status polling: we cannot use GetRefundDetails to reflect refund(s) made at Amazon seller central cause we 
										// do not have any refund-id and there is no api endpoint that serves them. so we only can process CaptureDetails.RefundedAmount.

										ProcessRefundResult(client, order, data);
									}
								}
							}
						}
					}
					catch (OffAmazonPaymentsServiceException exc)
					{
						LogAmazonError(exc);
					}
					catch (Exception exc)
					{
						LogError(exc);
					}
				}
			}
			catch (OffAmazonPaymentsServiceException exc)
			{
				LogAmazonError(exc);
			}
			catch (Exception exc)
			{
				LogError(exc);
			}
		}

		public void DataPollingTaskInit()
		{
			var task = _scheduleTaskService.GetTaskByType(AmazonPayCore.DataPollingTaskType);
			if (task == null)
			{
				_scheduleTaskService.InsertTask(new ScheduleTask
				{
					Name = "{0} data polling".FormatWith(AmazonPayCore.SystemName),
					CronExpression = "*/30 * * * *", // Every 30 minutes
					Type = AmazonPayCore.DataPollingTaskType,
					Enabled = false,
					StopOnError = false,
				});
			}
		}

		public void DataPollingTaskUpdate(bool enabled, int seconds)
		{
			var task = _scheduleTaskService.GetTaskByType(AmazonPayCore.DataPollingTaskType);
			if (task != null)
			{
				task.Enabled = enabled;
				//task.Seconds = seconds;

				_scheduleTaskService.UpdateTask(task);
			}
		}

		public void DataPollingTaskDelete()
		{
			var task = _scheduleTaskService.GetTaskByType(AmazonPayCore.DataPollingTaskType);
			if (task != null)
				_scheduleTaskService.DeleteTask(task);
		}
	}
}
