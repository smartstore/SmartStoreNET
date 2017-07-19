using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AmazonPay.StandardPaymentRequests;
using Autofac;
using Newtonsoft.Json.Linq;
using OffAmazonPaymentsService;
using SmartStore.AmazonPay.Api;
using SmartStore.AmazonPay.Models;
using SmartStore.AmazonPay.Settings;
using SmartStore.Core;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Authentication.External;
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
	public partial class AmazonPayService : IAmazonPayService
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
		private readonly ICountryService _countryService;
		private readonly IStateProvinceService _stateProvinceService;
		private readonly IAddressService _addressService;
		private readonly IPriceFormatter _priceFormatter;
		private readonly OrderSettings _orderSettings;
		private readonly RewardPointsSettings _rewardPointsSettings;
		private readonly IOrderService _orderService;
		private readonly IRepository<Order> _orderRepository;
		private readonly IOrderProcessingService _orderProcessingService;
		private readonly IScheduleTaskService _scheduleTaskService;
		private readonly IWorkflowMessageService _workflowMessageService;
		private readonly IPluginFinder _pluginFinder;

		private readonly Lazy<ExternalAuthenticationSettings> _externalAuthenticationSettings;
		private readonly Lazy<IExternalAuthorizer> _authorizer;

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
			ICountryService countryService,
			IStateProvinceService stateProvinceService,
			IAddressService addressService,
			IPriceFormatter priceFormatter,
			OrderSettings orderSettings,
			RewardPointsSettings rewardPointsSettings,
			IOrderService orderService,
			IRepository<Order> orderRepository,
			IOrderProcessingService orderProcessingService,
			IScheduleTaskService scheduleTaskService,
			IWorkflowMessageService workflowMessageService,
			IPluginFinder pluginFinder,
			Lazy<ExternalAuthenticationSettings> externalAuthenticationSettings,
			Lazy<IExternalAuthorizer> authorizer)
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
			_countryService = countryService;
			_stateProvinceService = stateProvinceService;
			_addressService = addressService;
			_priceFormatter = priceFormatter;
			_orderSettings = orderSettings;
			_rewardPointsSettings = rewardPointsSettings;
			_orderService = orderService;
			_orderRepository = orderRepository;
			_orderProcessingService = orderProcessingService;
			_scheduleTaskService = scheduleTaskService;
			_workflowMessageService = workflowMessageService;
			_pluginFinder = pluginFinder;
			_externalAuthenticationSettings = externalAuthenticationSettings;
			_authorizer = authorizer;

			T = NullLocalizer.Instance;
			Logger = NullLogger.Instance;
		}

		public Localizer T { get; set; }
		public ILogger Logger { get; set; }

		public void LogError(Exception exception, string shortMessage = null, string fullMessage = null, bool notify = false, IList<string> errors = null)
		{
			try
			{
				if (exception != null)
				{
					shortMessage = exception.Message;
					exception.Dump();
				}

				if (shortMessage.HasValue())
				{
					Logger.Error(exception, shortMessage);

					if (notify)
					{
						_services.Notifier.Error(new LocalizedString(shortMessage));
					}
				}
			}
			catch { }

			if (errors != null && shortMessage.HasValue())
			{
				errors.Add(shortMessage);
			}
		}

		private void LogAmazonError(OffAmazonPaymentsServiceException exception, bool notify = false, IList<string> errors = null)
		{
			try
			{
				string shortMessage, fullMessage;

				if (exception.GetErrorStrings(out shortMessage, out fullMessage))
				{
					Logger.Error(exception, shortMessage);

					if (notify)
					{
						_services.Notifier.Error(new LocalizedString(shortMessage));
					}

					if (errors != null)
					{
						errors.Add(shortMessage);
					}
				}
			}
			catch { }
		}

		public void SetupConfiguration(ConfigurationModel model)
		{
			var store = _services.StoreContext.CurrentStore;
			var language = _services.WorkContext.WorkingLanguage;
			var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(AmazonPayPlugin.SystemName);
			var allStores = _services.StoreService.GetAllStores();

			model.PollingTaskMinutes = 30;
			model.IpnUrl = GetPluginUrl("IPNHandler", store.SslEnabled);
			model.ConfigGroups = T("Plugins.Payments.AmazonPay.ConfigGroups").Text.SplitSafe(";");

			model.RegisterUrl = "https://payments-eu.amazon.com/register";
			model.SoftwareVersion = SmartStoreVersion.CurrentFullVersion;
			if (descriptor != null)
			{
				model.PluginVersion = descriptor.Version.ToString();
			}
			model.LeadCode = AmazonPayApi.LeadCode;
			model.PlatformId = AmazonPayApi.PlatformId;
			model.LanguageLocale = language.UniqueSeoCode.ToAmazonLanguageCode('_');
			model.MerchantLoginDomains = allStores.Select(x => x.SslEnabled ? x.SecureUrl.EmptyNull().TrimEnd('/') : x.Url.EmptyNull().TrimEnd('/')).ToArray();
			model.MerchantLoginRedirectURLs = new string[0];
			model.MerchantStoreDescription = store.Name.Truncate(2048);

			model.DataFetchings = new List<SelectListItem>
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

			model.TransactionTypes = new List<SelectListItem>
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

			model.SaveEmailAndPhones = new List<SelectListItem>
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
		}

		public AmazonPayViewModel CreateViewModel(
			AmazonPayRequestType type,
			TempDataDictionary tempData,
			string orderReferenceId = null,
			string accessToken = null)
		{
			var model = new AmazonPayViewModel();
			model.Type = type;

			try
			{
				var store = _services.StoreContext.CurrentStore;
				var customer = _services.WorkContext.CurrentCustomer;
				var language = _services.WorkContext.WorkingLanguage;
				var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);
				var storeLocation = _services.WebHelper.GetStoreLocation(store.SslEnabled);

				model.ButtonHandlerUrl = $"{storeLocation}Plugins/SmartStore.AmazonPay/AmazonPayShoppingCart/PayButtonHandler";
				model.LanguageCode = language.UniqueSeoCode.ToAmazonLanguageCode();

				if (type == AmazonPayRequestType.PayButtonHandler)
				{
					if (orderReferenceId.IsEmpty() || accessToken.IsEmpty())
					{
						var msg = orderReferenceId.IsEmpty() ? T("Plugins.Payments.AmazonPay.MissingOrderReferenceId") : T("Plugins.Payments.AmazonPay.MissingAddressConsentToken");
						LogError(null, msg, null, true);
						model.Result = AmazonPayResultType.Redirect;
						return model;
					}

					if (cart.Count <= 0 || !IsPaymentMethodActive(store.Id))
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
					var checkoutStateKey = AmazonPayPlugin.SystemName + ".CheckoutState";

					if (checkoutState == null)
					{
						Logger.Warn("Checkout state is null in AmazonPayService.ValidateAndInitiateCheckout!");
						model.Result = AmazonPayResultType.Redirect;
						return model;
					}

					var state = new AmazonPayCheckoutState
					{
						OrderReferenceId = orderReferenceId,
						AddressConsentToken = accessToken
					};

					if (checkoutState.CustomProperties.ContainsKey(checkoutStateKey))
						checkoutState.CustomProperties[checkoutStateKey] = state;
					else
						checkoutState.CustomProperties.Add(checkoutStateKey, state);

					//_httpContext.Session.SafeSet(checkoutStateKey, state);

					model.RedirectAction = "Index";
					model.RedirectController = "Checkout";
					model.Result = AmazonPayResultType.Redirect;
					return model;
				}
				else if (type == AmazonPayRequestType.ShoppingCart || type == AmazonPayRequestType.MiniShoppingCart)
				{
					if (cart.Count <= 0 || !IsPaymentMethodActive(store.Id))
					{
						model.Result = AmazonPayResultType.None;
						return model;
					}
				}
				else if (type == AmazonPayRequestType.AuthenticationPublicInfo)
				{
					model.ButtonHandlerUrl = string.Concat(
						storeLocation,
						"Plugins/SmartStore.AmazonPay/AmazonPay/AuthenticationButtonHandler?returnUrl=",
						_httpContext.Request.QueryString["returnUrl"]);
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
					model.AddressConsentToken = state.AddressConsentToken;
					//model.IsOrderConfirmed = state.IsOrderConfirmed;
				}

				var currency = store.PrimaryStoreCurrency;
				var settings = _services.Settings.LoadSetting<AmazonPaySettings>(store.Id);

				model.SellerId = settings.SellerId;
				model.ClientId = settings.ClientId;
				model.IsShippable = cart.RequiresShipping();
				model.IsRecurring = cart.IsRecurring();
				model.WidgetUrl = settings.WidgetUrl;
				model.AddressWidgetWidth = Math.Max(settings.AddressWidgetWidth, 200);
				model.AddressWidgetHeight = Math.Max(settings.AddressWidgetHeight, 228);
				model.PaymentWidgetWidth = Math.Max(settings.PaymentWidgetWidth, 200);
				model.PaymentWidgetHeight = Math.Max(settings.PaymentWidgetHeight, 228);

				if (type == AmazonPayRequestType.MiniShoppingCart && !settings.ShowButtonInMiniShoppingCart)
				{
					model.Result = AmazonPayResultType.None;
					return model;
				}

				if (type == AmazonPayRequestType.MiniShoppingCart || type == AmazonPayRequestType.ShoppingCart)
				{
					model.ButtonType = settings.PayButtonType;
					model.ButtonColor = settings.PayButtonColor;
					model.ButtonSize = settings.PayButtonSize;
				}
				else if (type == AmazonPayRequestType.AuthenticationPublicInfo)
				{
					model.ButtonType = settings.AuthButtonType;
					model.ButtonColor = settings.AuthButtonColor;
					model.ButtonSize = settings.AuthButtonSize;
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

					var shippingToCountryNotAllowed = tempData[AmazonPayPlugin.SystemName + "ShippingToCountryNotAllowed"];

					if (shippingToCountryNotAllowed != null && true == (bool)shippingToCountryNotAllowed)
						model.Warning = T("Plugins.Payments.AmazonPay.ShippingToCountryNotAllowed");
				}
				else if (type == AmazonPayRequestType.ShippingMethod)
				{
					model.RedirectAction = model.RedirectController = "";

					if (model.IsShippable)
					{
						var client = CreateClient(settings);
						var getOrderRequest = new GetOrderReferenceDetailsRequest()
							.WithMerchantId(settings.SellerId)
							.WithAmazonOrderReferenceId(model.OrderReferenceId)
							.WithaddressConsentToken(model.AddressConsentToken);

						var getOrderResponse = client.GetOrderReferenceDetails(getOrderRequest);
						if (getOrderResponse.GetSuccess())
						{
							if (FindAndApplyAddress(getOrderResponse, customer, model.IsShippable, true))
							{
								_customerService.UpdateCustomer(customer);
								model.Result = AmazonPayResultType.None;
								return model;
							}
							else
							{
								tempData[AmazonPayPlugin.SystemName + "ShippingToCountryNotAllowed"] = true;
								model.RedirectAction = "ShippingAddress";
								model.RedirectController = "Checkout";
								model.Result = AmazonPayResultType.Redirect;
								return model;
							}
						}
						else
						{
							LogError(getOrderResponse);
						}
					}
				}
				else if (type == AmazonPayRequestType.PaymentMethod)
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

					_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, AmazonPayPlugin.SystemName, store.Id);

					decimal orderTotalDiscountAmountBase = decimal.Zero;
					Discount orderTotalAppliedDiscount = null;
					List<AppliedGiftCard> appliedGiftCards = null;
					int redeemedRewardPoints = 0;
					decimal redeemedRewardPointsAmount = decimal.Zero;

					decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart,
						out orderTotalDiscountAmountBase, out orderTotalAppliedDiscount, out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount);
					if (shoppingCartTotalBase.HasValue)
					{
						var client = CreateClient(settings, store.PrimaryStoreCurrency.CurrencyCode);
						var setOrderRequest = new SetOrderReferenceDetailsRequest()
							.WithMerchantId(settings.SellerId)
							.WithAmazonOrderReferenceId(model.OrderReferenceId)
							.WithPlatformId(PlatformId)
							.WithAmount(shoppingCartTotalBase.Value)
							.WithCurrencyCode(ConvertCurrency(store.PrimaryStoreCurrency.CurrencyCode))
							.WithStoreName(store.Name);

						var setOrderResponse = client.SetOrderReferenceDetails(setOrderRequest);
						if (!setOrderResponse.GetSuccess())
						{
							LogError(setOrderResponse);
						}
					}

					// This is ugly...
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
			catch (Exception exception)
			{
				LogError(exception, notify: true);
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

		private string GetAuthorizationState(AmazonPayApiClient client, string authorizationId)
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
				var orderAttribute = DeserializeOrderAttribute(order);
				var client = CreateClient(settings);

				var closeRequest = new CloseOrderReferenceRequest()
					.WithMerchantId(settings.SellerId)
					.WithAmazonOrderReferenceId(orderAttribute.OrderReferenceId);

				var closeResponse = client.CloseOrderReference(closeRequest);
				if (!closeResponse.GetSuccess())
				{
					LogError(closeResponse, null, true);
				}
			}
			catch (Exception exception)
			{
				LogError(exception);
			}
		}

		private void ProcessAuthorizationResult(AmazonPayApiClient client, Order order, AmazonPayApiData data, OffAmazonPaymentsService.Model.AuthorizationDetails details)
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
		private void ProcessCaptureResult(AmazonPayApiClient client, Order order, AmazonPayApiData data)
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
		private void ProcessRefundResult(AmazonPayApiClient client, Order order, AmazonPayApiData data)
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
						data.Client = _api.CreateClient(data.Settings);

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
							{
								sb.AppendFormat("<p>{0}</p>", error);
							}
						}

						var orderNote = new OrderNote
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
			// Fulfill the Amazon checkout.
			var result = new PreProcessPaymentResult();

			try
			{
				var store = _services.StoreService.GetStoreById(request.StoreId);

				if (!IsPaymentMethodActive(store.Id, true))
				{
					//_httpContext.ResetCheckoutState();

					result.AddError(T("Plugins.Payments.AmazonPay.PaymentMethodNotActive", store.Name));
					return result;
				}

				var orderGuid = request.OrderGuid.ToString();
				var customer = _customerService.GetCustomerById(request.CustomerId);
				var settings = _services.Settings.LoadSetting<AmazonPaySettings>(store.Id);
				var state = _httpContext.GetAmazonPayState(_services.Localization);
				var client = CreateClient(settings, store.PrimaryStoreCurrency.CurrencyCode);

				var setOrderRequest = new SetOrderReferenceDetailsRequest()
					.WithMerchantId(settings.SellerId)
					.WithAmazonOrderReferenceId(state.OrderReferenceId)
					.WithPlatformId(PlatformId)
					.WithAmount(request.OrderTotal)
					.WithCurrencyCode(ConvertCurrency(store.PrimaryStoreCurrency.CurrencyCode))
					.WithSellerOrderId(orderGuid)
					.WithStoreName(store.Name);

				var setOrderResponse = client.SetOrderReferenceDetails(setOrderRequest);
				if (setOrderResponse.GetSuccess())
				{
					if (setOrderResponse.GetHasConstraint())
					{
						var ids = setOrderResponse.GetConstraintIdList();
						var descriptions = setOrderResponse.GetDescriptionList();

						foreach (var id in ids)
						{
							var idx = ids.IndexOf(id);
							if (idx < descriptions.Count)
							{
								result.Errors.Add($"{descriptions[idx]} ({id})");
							}
						}
					}
				}
				else
				{
					LogError(setOrderResponse, result.Errors);
				}

				if (!result.Success)
				{
					return result;
				}

				// Inform Amazon that the buyer has placed the order.
				var confirmRequest = new ConfirmOrderReferenceRequest()
					.WithMerchantId(settings.SellerId)
					.WithAmazonOrderReferenceId(state.OrderReferenceId);

				client.ConfirmOrderReference(confirmRequest);

				// Address and payment cannot be changed if order is in open state, amazon widgets then might show an error.
				//state.IsOrderConfirmed = true;

				var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);
				var isShippable = cart.RequiresShipping();

				var getOrderRequest = new GetOrderReferenceDetailsRequest()
					.WithMerchantId(settings.SellerId)
					.WithAmazonOrderReferenceId(state.OrderReferenceId)
					.WithaddressConsentToken(state.AddressConsentToken);

				var getOrderResponse = client.GetOrderReferenceDetails(getOrderRequest);

				FindAndApplyAddress(getOrderResponse, customer, isShippable, false);

				if (settings.CanSaveEmailAndPhone(customer.Email))
				{
					customer.Email = getOrderResponse.GetEmail();
				}
				_customerService.UpdateCustomer(customer);

				if (settings.CanSaveEmailAndPhone(customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone, store.Id)))
				{
					_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.Phone, getOrderResponse.GetPhone());
				}
			}
			catch (Exception exception)
			{
				LogError(exception, errors: result.Errors);
			}

			return result;
		}

		public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest request)
		{
			// Initiate Amazon payment. We do not add errors to request.Errors cause of asynchronous processing.
			var result = new ProcessPaymentResult();
			var errors = new List<string>();
			bool informCustomerAboutErrors = false;
			bool informCustomerAddErrors = false;

			try
			{
				var store = _services.StoreService.GetStoreById(request.StoreId);
				var settings = _services.Settings.LoadSetting<AmazonPaySettings>(store.Id);
				var state = _httpContext.GetAmazonPayState(_services.Localization);
				var client = CreateClient(settings, store.PrimaryStoreCurrency.CurrencyCode);

				informCustomerAboutErrors = settings.InformCustomerAboutErrors;
				informCustomerAddErrors = settings.InformCustomerAddErrors;

				// Asynchronous as long as we do not set TransactionTimeout to 0. So transaction is always in pending state after return.
				var authorizeRequest = new AuthorizeRequest()
					.WithMerchantId(settings.SellerId)
					.WithAmazonOrderReferenceId(state.OrderReferenceId)
					.WithAuthorizationReferenceId(GetRandomId("Authorize"))
					.WithCaptureNow(settings.TransactionType == AmazonPayTransactionType.AuthorizeAndCapture)
					.WithCurrencyCode(ConvertCurrency(store.PrimaryStoreCurrency.CurrencyCode))
					.WithAmount(request.OrderTotal);

				var authorizeResponse = client.Authorize(authorizeRequest);
				if (authorizeResponse.GetSuccess())
				{
					result.AuthorizationTransactionId = authorizeResponse.GetAuthorizationId();
					result.AuthorizationTransactionCode = authorizeResponse.GetAuthorizationReferenceId();
					result.AuthorizationTransactionResult = authorizeResponse.GetAuthorizationState();

					if (settings.TransactionType == AmazonPayTransactionType.AuthorizeAndCapture)
					{
						var idList = authorizeResponse.GetCaptureIdList();
						if (idList.Any())
						{
							result.CaptureTransactionId = idList.First();
						}
					}

					var reason = authorizeResponse.GetReasonCode();
					if (reason.IsCaseInsensitiveEqual("InvalidPaymentMethod") || reason.IsCaseInsensitiveEqual("AmazonRejected") ||
						reason.IsCaseInsensitiveEqual("ProcessingFailure") || reason.IsCaseInsensitiveEqual("TransactionTimedOut") ||
						reason.IsCaseInsensitiveEqual("TransactionTimeout"))
					{
						var reasonDescription = authorizeResponse.GetReasonDescription();
						errors.Add(reasonDescription.HasValue() ? $"{reason}: {reasonDescription}" : reason);
					}
				}
				else
				{
					LogError(authorizeResponse, errors);
				}

				// The response to the Authorize call includes the AuthorizationStatus response element, which will be always be
				// set to Pending if you have selected the asynchronous mode of operation.
				result.NewPaymentStatus = PaymentStatus.Pending;
			}
			catch (Exception exception)
			{
				LogError(exception, errors: errors);
			}

			if (informCustomerAboutErrors && errors != null && errors.Count > 0)
			{
				// Customer needs to be informed of an amazon error here. hooking OrderPlaced.CustomerNotification won't work
				// cause of asynchronous processing. Solution: we add a customer order note that is also send as an email.
				var state = new AmazonPayActionState { OrderGuid = request.OrderGuid };

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
				var store = _services.StoreService.GetStoreById(request.Order.StoreId);
				var client = CreateClient(settings, store.PrimaryStoreCurrency.CurrencyCode);

				var captureRequest = new CaptureRequest()
					.WithMerchantId(settings.SellerId)
					.WithAmazonAuthorizationId(request.Order.AuthorizationTransactionId)
					.WithCaptureReferenceId(GetRandomId("Capture"))
					.WithCurrencyCode(ConvertCurrency(store.PrimaryStoreCurrency.CurrencyCode))
					.WithAmount(request.Order.OrderTotal);

				var captureResponse = client.Capture(captureRequest);
				if (captureResponse.GetSuccess())
				{
					var state = captureResponse.GetCaptureState();

					result.CaptureTransactionId = captureResponse.GetCaptureId();
					result.CaptureTransactionResult = state.Grow(captureResponse.GetReasonCode(), " ");

					if (state.IsCaseInsensitiveEqual("completed"))
					{
						result.NewPaymentStatus = PaymentStatus.Paid;
					}
				}
				else
				{
					LogError(captureResponse, result.Errors);
				}
			}
			catch (Exception exception)
			{
				LogError(exception, errors: result.Errors);
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
				var store = _services.StoreService.GetStoreById(request.Order.StoreId);
				var client = CreateClient(settings, store.PrimaryStoreCurrency.CurrencyCode);

				var refundRequest = new RefundRequest()
					.WithMerchantId(settings.SellerId)
					.WithAmazonCaptureId(request.Order.CaptureTransactionId)
					.WithRefundReferenceId(GetRandomId("Refund"))
					.WithCurrencyCode(ConvertCurrency(store.PrimaryStoreCurrency.CurrencyCode))
					.WithAmount(request.AmountToRefund);

				var refundResponse = client.Refund(refundRequest);
				if (refundResponse.GetSuccess())
				{
					var refundId = refundResponse.GetRefundId();
					if (refundId.HasValue() && request.Order.Id != 0)
					{
						_genericAttributeService.InsertAttribute(new GenericAttribute
						{
							EntityId = request.Order.Id,
							KeyGroup = "Order",
							Key = AmazonPayPlugin.SystemName + ".RefundId",
							Value = refundId,
							StoreId = request.Order.StoreId
						});
					}
				}
				else
				{
					LogError(refundResponse, result.Errors);
				}
			}
			catch (Exception exception)
			{
				LogError(exception, errors: result.Errors);
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
					var orderAttribute = DeserializeOrderAttribute(request.Order);
					var client = CreateClient(settings);

					var cancelRequest = new CancelOrderReferenceRequest()
						.WithMerchantId(settings.SellerId)
						.WithAmazonOrderReferenceId(orderAttribute.OrderReferenceId);

					var cancelResponse = client.CancelOrderReference(cancelRequest);
					if (!cancelResponse.GetSuccess())
					{
						LogError(cancelResponse, result.Errors);
					}
				}
			}
			catch (Exception exception)
			{
				LogError(exception, errors: result.Errors);
			}

			return result;
		}

		public void ProcessIpn(HttpRequestBase request)
		{
			try
			{
				var data = _api.ParseNotification(request);
				var order = FindOrder(data);

				if (order == null || !IsPaymentMethodActive(order.StoreId))
					return;

				var client = _api.CreateClient(_services.Settings.LoadSetting<AmazonPaySettings>(order.StoreId));

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
					where x.PaymentMethodSystemName == AmazonPayPlugin.SystemName && x.CreatedOnUtc > isTooOld &&
						!x.Deleted && x.OrderStatusId < (int)OrderStatus.Complete && x.PaymentStatusId != (int)PaymentStatus.Voided
					orderby x.Id descending
					select x;

				var orders = query.ToList();

				//"- start polling {0} orders".FormatWith(orders.Count).Dump();

				foreach (var order in orders)
				{
					try
					{
						var client = _api.CreateClient(_services.Settings.LoadSetting<AmazonPaySettings>(order.StoreId));

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
			_scheduleTaskService.GetOrAddTask<DataPollingTask>(x => 
			{
				x.Name = "{0} data polling".FormatWith(AmazonPayPlugin.SystemName);
				x.CronExpression = "*/30 * * * *"; // Every 30 minutes
			});
		}

		public void DataPollingTaskUpdate(bool enabled, int seconds)
		{
			var task = _scheduleTaskService.GetTaskByType<DataPollingTask>();
			if (task != null)
			{
				task.Enabled = enabled;
				//task.Seconds = seconds;

				_scheduleTaskService.UpdateTask(task);
			}
		}

		public void DataPollingTaskDelete()
		{
			_scheduleTaskService.TryDeleteTask<DataPollingTask>();
		}

		#region IExternalProviderAuthorizer

		public AuthorizeState Authorize(string returnUrl, bool? verifyResponse = null)
		{
			string error = null;
			string email = null;
			string name = null;
			string userId = null;
			var accessToken = _httpContext.Request.QueryString["accessToken"];

			if (accessToken.HasValue())
			{
				var settings = _services.Settings.LoadSetting<AmazonPaySettings>();
				var client = CreateClient(settings);
				var jsonString = client.GetUserInfo(accessToken);
				if (jsonString.HasValue())
				{
					var json = JObject.Parse(jsonString);

					email = json.GetValue("email").ToString();
					name = json.GetValue("name").ToString();
					userId = json.GetValue("user_id").ToString();

					if (email.IsEmpty() || name.IsEmpty() || userId.IsEmpty())
					{
						error = T("Plugins.Payments.AmazonPay.IncompleteProfileDetails") +
							$" Email: {email.NaIfEmpty()}, name: {name.NaIfEmpty()}, userId: {userId.NaIfEmpty()}.";
					}
				}
				else
				{
					error = T("Plugins.Payments.AmazonPay.IncompleteProfileDetails") + " Email: , name: , userId: .";
				}
			}
			else
			{
				error = T("Plugins.Payments.AmazonPayMissingAccessToken");
			}

			if (error.HasValue())
			{
				var state = new AuthorizeState("", OpenAuthenticationStatus.Error);
				state.AddError(error);
				Logger.Error(error);
				return state;
			}

			string firstName, lastName;
			name.ToFirstAndLastName(out firstName, out lastName);

			var claims = new UserClaims();
			claims.Name = new NameClaims();
			claims.Contact = new ContactClaims();
			claims.Contact.Email = email;
			claims.Name.FullName = name;
			claims.Name.First = firstName;
			claims.Name.Last = lastName;

			var parameters = new AmazonAuthenticationParameters();
			parameters.ExternalIdentifier = userId;
			parameters.AddClaim(claims);

			var result = _authorizer.Value.Authorize(parameters);

			return new AuthorizeState(returnUrl, result);
		}

		#endregion
	}
}
