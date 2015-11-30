using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web.Mvc;
using Autofac;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.PayPalSvc;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.PayPal.Validators;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.PayPal.Controllers
{
	public class PayPalExpressController : PaymentControllerBase
	{
		private readonly PluginHelper _helper;
		private readonly IPaymentService _paymentService;
		private readonly IOrderService _orderService;
		private readonly IOrderProcessingService _orderProcessingService;
		private readonly ILogger _logger;
		private readonly PaymentSettings _paymentSettings;
		private readonly ILocalizationService _localizationService;
		private readonly OrderSettings _orderSettings;
		private readonly ICurrencyService _currencyService;
		private readonly CurrencySettings _currencySettings;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
		private readonly ICustomerService _customerService;
		private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICommonServices _services;
        private readonly IStoreService _storeService;

		public PayPalExpressController(
			IPaymentService paymentService, IOrderService orderService,
			IOrderProcessingService orderProcessingService,
			ILogger logger, 
			PaymentSettings paymentSettings, ILocalizationService localizationService,
			OrderSettings orderSettings,
			ICurrencyService currencyService, CurrencySettings currencySettings,
			IOrderTotalCalculationService orderTotalCalculationService, ICustomerService customerService,
			IGenericAttributeService genericAttributeService,
            IComponentContext ctx, ICommonServices services,
            IStoreService storeService)
		{
			_paymentService = paymentService;
			_orderService = orderService;
			_orderProcessingService = orderProcessingService;
			_logger = logger;
			_paymentSettings = paymentSettings;
			_localizationService = localizationService;
			_orderSettings = orderSettings;
			_currencyService = currencyService;
			_currencySettings = currencySettings;
			_orderTotalCalculationService = orderTotalCalculationService;
			_customerService = customerService;
			_genericAttributeService = genericAttributeService;
            _services = services;
            _storeService = storeService;

			_helper = new PluginHelper(ctx, "SmartStore.PayPal", "Plugins.Payments.PayPalExpress");
		}

		public SelectList TransactModeValues(TransactMode selected)
		{
			return new SelectList(new List<object>() {
				new { ID = (int)TransactMode.Authorize, Name = _helper.GetResource("ModeAuth") },
				new { ID = (int)TransactMode.AuthorizeAndCapture, Name = _helper.GetResource("ModeAuthAndCapture") }
			}, "ID", "Name", (int)selected);
		}

		[AdminAuthorize, ChildActionOnly]
		public ActionResult Configure()
		{
            var model = new PayPalExpressConfigurationModel();
            int storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _services.WorkContext);
            var settings = _services.Settings.LoadSetting<PayPalExpressPaymentSettings>(storeScope);

            model.Copy(settings, true);

            model.TransactModeValues = TransactModeValues(settings.TransactMode);

            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, _services.Settings);

			return View(model);
		}

		[HttpPost, AdminAuthorize, ChildActionOnly]
		public ActionResult Configure(PayPalExpressConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return Configure();

			ModelState.Clear();

            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            int storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _services.WorkContext);
            var settings = _services.Settings.LoadSetting<PayPalExpressPaymentSettings>(storeScope);

            model.Copy(settings, false);

            storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, _services.Settings);

			// multistore context not possible, see IPN handling
			_services.Settings.SaveSetting(settings, x => x.UseSandbox, 0, false);

            _services.Settings.ClearCache();
            NotifySuccess(_services.Localization.GetResource("Plugins.Payments.PayPal.ConfigSaveNote"));

			return Configure();
		}

		public ActionResult PaymentInfo()
		{

			var model = new PayPalExpressPaymentInfoModel();
			model.CurrentPageIsBasket = PayPalHelper.CurrentPageIsBasket(this.ControllerContext.ParentActionViewContext.RequestContext.RouteData);

			if (model.CurrentPageIsBasket)
			{
				var culture = _services.WorkContext.WorkingLanguage.LanguageCulture;
				var buttonUrl = "https://www.paypalobjects.com/{0}/i/btn/btn_xpressCheckout.gif".FormatWith(culture.Replace("-", "_"));
				model.SubmitButtonImageUrl = PayPalHelper.CheckIfButtonExists(buttonUrl);
			}

			return PartialView(model);
		}

		[ValidateInput(false)]
		public ActionResult IPNHandler()
		{
			byte[] param = Request.BinaryRead(Request.ContentLength);
			string strRequest = Encoding.ASCII.GetString(param);
			Dictionary<string, string> values;

			var provider = _paymentService.LoadPaymentMethodBySystemName("Payments.PayPalExpress", true);
			var processor = provider != null ? provider.Value as PayPalExpress : null;
			if (processor == null)
				throw new SmartException(T("PayPal Express module cannot be loaded"));

			if (processor.VerifyIPN(strRequest, out values))
			{
				#region values
				decimal total = decimal.Zero;
				try
				{
					total = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
				}
				catch { }

				string payer_status = string.Empty;
				values.TryGetValue("payer_status", out payer_status);
				string payment_status = string.Empty;
				values.TryGetValue("payment_status", out payment_status);
				string pending_reason = string.Empty;
				values.TryGetValue("pending_reason", out pending_reason);
				string mc_currency = string.Empty;
				values.TryGetValue("mc_currency", out mc_currency);
				string txn_id = string.Empty;
				values.TryGetValue("txn_id", out txn_id);
				string txn_type = string.Empty;
				values.TryGetValue("txn_type", out txn_type);
				string rp_invoice_id = string.Empty;
				values.TryGetValue("rp_invoice_id", out rp_invoice_id);
				string payment_type = string.Empty;
				values.TryGetValue("payment_type", out payment_type);
				string payer_id = string.Empty;
				values.TryGetValue("payer_id", out payer_id);
				string receiver_id = string.Empty;
				values.TryGetValue("receiver_id", out receiver_id);
				string invoice = string.Empty;
				values.TryGetValue("invoice", out invoice);
				string payment_fee = string.Empty;
				values.TryGetValue("payment_fee", out payment_fee);

				#endregion

				var sb = new StringBuilder();
				sb.AppendLine("Paypal IPN:");
				foreach (KeyValuePair<string, string> kvp in values)
				{
					sb.AppendLine(kvp.Key + ": " + kvp.Value);
				}

				var newPaymentStatus = PayPalHelper.GetPaymentStatus(payment_status, pending_reason);
				sb.AppendLine("New payment status: " + newPaymentStatus);

				switch (txn_type)
				{
					case "recurring_payment_profile_created":
						//do nothing here
						break;
					case "recurring_payment":
						#region Recurring payment
						{
							Guid orderNumberGuid = Guid.Empty;
							try
							{
								orderNumberGuid = new Guid(rp_invoice_id);
							}
							catch
							{
							}

							var initialOrder = _orderService.GetOrderByGuid(orderNumberGuid);
							if (initialOrder != null)
							{
								var recurringPayments = _orderService.SearchRecurringPayments(0, 0, initialOrder.Id, null);
								foreach (var rp in recurringPayments)
								{
									switch (newPaymentStatus)
									{
										case PaymentStatus.Authorized:
										case PaymentStatus.Paid:
											{
												var recurringPaymentHistory = rp.RecurringPaymentHistory;
												if (recurringPaymentHistory.Count == 0)
												{
													//first payment
													var rph = new RecurringPaymentHistory()
													{
														RecurringPaymentId = rp.Id,
														OrderId = initialOrder.Id,
														CreatedOnUtc = DateTime.UtcNow
													};
													rp.RecurringPaymentHistory.Add(rph);
													_orderService.UpdateRecurringPayment(rp);
												}
												else
												{
													//next payments
													_orderProcessingService.ProcessNextRecurringPayment(rp);
													//UNDONE change new order status according to newPaymentStatus
													//UNDONE refund/void is not supported
												}
											}
											break;
									}
								}

								_logger.Information("PayPal IPN. Recurring info", new SmartException(sb.ToString()));
							}
							else
							{
								_logger.Error("PayPal IPN. Order is not found", new SmartException(sb.ToString()));
							}
						}
						#endregion
						break;
					default:
						#region Standard payment
						{
							string orderNumber = string.Empty;
							values.TryGetValue("custom", out orderNumber);
							Guid orderNumberGuid = Guid.Empty;
							try
							{
								orderNumberGuid = new Guid(orderNumber);
							}
							catch
							{
							}

							var order = _orderService.GetOrderByGuid(orderNumberGuid);
							if (order != null)
							{
								//order note
								order.HasNewPaymentNotification = true;

								order.OrderNotes.Add(new OrderNote
								{
									Note = sb.ToString(),
									DisplayToCustomer = false,
									CreatedOnUtc = DateTime.UtcNow
								});
								_orderService.UpdateOrder(order);

								switch (newPaymentStatus)
								{
									case PaymentStatus.Pending:
										{
										}
										break;
									case PaymentStatus.Authorized:
										{
											if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
											{
												_orderProcessingService.MarkAsAuthorized(order);
											}
										}
										break;
									case PaymentStatus.Paid:
										{
											if (_orderProcessingService.CanMarkOrderAsPaid(order))
											{
												_orderProcessingService.MarkOrderAsPaid(order);
											}
										}
										break;
									case PaymentStatus.Refunded:
										{
											if (_orderProcessingService.CanRefundOffline(order))
											{
												_orderProcessingService.RefundOffline(order);
											}
										}
										break;
									case PaymentStatus.Voided:
										{
											if (_orderProcessingService.CanVoidOffline(order))
											{
												_orderProcessingService.VoidOffline(order);
											}
										}
										break;
									default:
										break;
								}
							}
							else
							{
								_logger.Error("PayPal IPN. Order is not found", new SmartException(sb.ToString()));
							}
						}
						#endregion
						break;
				}
			}
			else
			{
				_logger.Error("PayPal IPN failed.", new SmartException(strRequest));
			}

			//nothing should be rendered to visitor
			return Content("");
		}


		public ActionResult SubmitButton()
		{
			try
			{
				//user validation
				if ((_services.WorkContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
					return RedirectToRoute("Login");

				var store = _services.StoreContext.CurrentStore;
				var settings = _services.Settings.LoadSetting<PayPalExpressPaymentSettings>(store.Id);
				var cart = _services.WorkContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

				if (cart.Count == 0)
					return RedirectToRoute("ShoppingCart");

				var currency = store.PrimaryStoreCurrency.CurrencyCode;

                if (String.IsNullOrEmpty(settings.ApiAccountName))
					throw new ApplicationException("PayPal API Account Name is not set");
                if (String.IsNullOrEmpty(settings.ApiAccountPassword))
					throw new ApplicationException("PayPal API Password is not set");
                if (String.IsNullOrEmpty(settings.Signature))
					throw new ApplicationException("PayPal API Signature is not set");

				var provider = _paymentService.LoadPaymentMethodBySystemName("Payments.PayPalExpress", true);
				var processor = provider != null ? provider.Value as PayPalExpress : null;
				if (processor == null)
					throw new SmartException("PayPal Express Checkout module cannot be loaded");

				var processPaymentRequest = new PayPalProcessPaymentRequest();

                processPaymentRequest.StoreId = _services.StoreContext.CurrentStore.Id;

				//Get sub-total and discounts that apply to sub-total
				decimal orderSubTotalDiscountAmountBase = decimal.Zero;
				Discount orderSubTotalAppliedDiscount = null;
				decimal subTotalWithoutDiscountBase = decimal.Zero;
				decimal subTotalWithDiscountBase = decimal.Zero;

				_orderTotalCalculationService.GetShoppingCartSubTotal(cart,
					out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

				//order total
				decimal resultTemp = decimal.Zero;
				resultTemp += subTotalWithDiscountBase;

				// get customer
				int customerId = Convert.ToInt32(_services.WorkContext.CurrentCustomer.Id.ToString());
				var customer = _customerService.GetCustomerById(customerId);

				//Get discounts that apply to Total
				Discount appliedDiscount = null;
				var discountAmount = _orderTotalCalculationService.GetOrderTotalDiscount(customer, resultTemp, out appliedDiscount);

				//if the current total is less than the discount amount, we only make the discount equal to the current total        
				if (resultTemp < discountAmount)
					discountAmount = resultTemp;

				//reduce subtotal
				resultTemp -= discountAmount;

				if (resultTemp < decimal.Zero)
					resultTemp = decimal.Zero;

				decimal tempDiscount = discountAmount + orderSubTotalDiscountAmountBase;

				resultTemp = _currencyService.ConvertFromPrimaryStoreCurrency(resultTemp, _services.WorkContext.WorkingCurrency);
				if (tempDiscount > decimal.Zero)
				{
					tempDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(tempDiscount, _services.WorkContext.WorkingCurrency);
				}

				processPaymentRequest.PaymentMethodSystemName = "Payments.PayPalExpress";
				processPaymentRequest.OrderTotal = resultTemp;
				processPaymentRequest.Discount = tempDiscount;
				processPaymentRequest.IsRecurringPayment = false;

				//var selectedPaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);

				processPaymentRequest.CustomerId = _services.WorkContext.CurrentCustomer.Id;
				this.Session["OrderPaymentInfo"] = processPaymentRequest;

				var resp = processor.SetExpressCheckout(processPaymentRequest, cart);

				if (resp.Ack == AckCodeType.Success)
				{
					processPaymentRequest.PaypalToken = resp.Token;
					processPaymentRequest.OrderGuid = new Guid();
					processPaymentRequest.IsShippingMethodSet = PayPalHelper.CurrentPageIsBasket(this.RouteData);
					this.Session["OrderPaymentInfo"] = processPaymentRequest;

					_genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, "Payments.PayPalExpress",
						_services.StoreContext.CurrentStore.Id);

					var result = new RedirectResult(String.Format(
                        PayPalHelper.GetPaypalUrl(settings) + 
                        "?cmd=_express-checkout&useraction=commit&token={0}", resp.Token));

					return result;
				}
				else
				{
					var error = new StringBuilder("We apologize, but an error has occured.<br />");
					foreach (var errormsg in resp.Errors)
					{
						error.AppendLine(String.Format("{0} | {1} | {2}", errormsg.ErrorCode, errormsg.ShortMessage, errormsg.LongMessage));
					}

					_logger.InsertLog(LogLevel.Error, resp.Errors[0].ShortMessage, resp.Errors[0].LongMessage, _services.WorkContext.CurrentCustomer);
                    
                    NotifyError(error.ToString(), false);

                    return RedirectToAction("Cart", "ShoppingCart", new { area = "" });
				}
			}
			catch (Exception ex)
			{
				_logger.InsertLog(LogLevel.Error, ex.Message, ex.StackTrace, _services.WorkContext.CurrentCustomer);

                NotifyError(ex.Message, false);

                return RedirectToAction("Cart", "ShoppingCart", new { area = "" });

			}
		}

		public ActionResult GetDetails(string token)
		{
			var provider = _paymentService.LoadPaymentMethodBySystemName("Payments.PayPalExpress", true);
			var processor = provider != null ? provider.Value as PayPalExpress : null;
			if (processor == null)
				throw new SmartException("PayPal Express module cannot be loaded");

			var resp = processor.GetExpressCheckoutDetails(token);

			if (resp.Ack == AckCodeType.Success)
			{
				var paymentInfo = this.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
				paymentInfo = processor.SetCheckoutDetails(paymentInfo, resp.GetExpressCheckoutDetailsResponseDetails);
				this.Session["OrderPaymentInfo"] = paymentInfo;
				var customer = _customerService.GetCustomerById(paymentInfo.CustomerId);

				_services.WorkContext.CurrentCustomer = customer;
				_customerService.UpdateCustomer(_services.WorkContext.CurrentCustomer);

				var selectedShippingOption = _services.WorkContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption,
					_services.StoreContext.CurrentStore.Id);
				if (selectedShippingOption != null)
				{
					return RedirectToAction("Confirm", "Checkout", new { area = "" });
				}
				else
				{
					//paymentInfo.RequiresPaymentWorkflow = false;
					_genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, paymentInfo.PaymentMethodSystemName,
						_services.StoreContext.CurrentStore.Id);
					_customerService.UpdateCustomer(customer);

					return RedirectToAction("BillingAddress", "Checkout", new { area = "" });
				}
            }
            else
            {
                var error = new StringBuilder("We apologize, but an error has occured.<br />");
                foreach (var errormsg in resp.Errors)
                {
                    error.AppendLine(String.Format("{0} | {1} | {2}", errormsg.ErrorCode, errormsg.ShortMessage, errormsg.LongMessage));
                }

				_logger.InsertLog(LogLevel.Error, resp.Errors[0].ShortMessage, resp.Errors[0].LongMessage, _services.WorkContext.CurrentCustomer);

                NotifyError(error.ToString(), false);

                return RedirectToAction("Cart", "ShoppingCart", new { area = "" });
            }
		}

		[NonAction]
		public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
		{
			var paymentInfo = new ProcessPaymentRequest();
			return paymentInfo;
		}

		[NonAction]
		public override IList<string> ValidatePaymentForm(FormCollection form)
		{
			var warnings = new List<string>();

			//validate
			var validator = new PayPalExpressPaymentInfoValidator(_localizationService);
			var model = new PayPalExpressPaymentInfoModel()
			{

			};
			var validationResult = validator.Validate(model);
			if (!validationResult.IsValid)
				foreach (var error in validationResult.Errors)
					warnings.Add(error.ErrorMessage);

			return warnings;
		}
	}
}