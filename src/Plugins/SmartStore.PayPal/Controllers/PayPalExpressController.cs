﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Logging;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.PayPalSvc;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.PayPal.Validators;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.PayPal.Controllers
{
	public class PayPalExpressController : PayPalControllerBase<PayPalExpressPaymentSettings>
	{
		private readonly OrderSettings _orderSettings;
		private readonly ICurrencyService _currencyService;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
		private readonly ICustomerService _customerService;
		private readonly IGenericAttributeService _genericAttributeService;

		public PayPalExpressController(
			IPaymentService paymentService,
			IOrderService orderService,
			IOrderProcessingService orderProcessingService,
			OrderSettings orderSettings,
			ICurrencyService currencyService,
			IOrderTotalCalculationService orderTotalCalculationService,
			ICustomerService customerService,
			IGenericAttributeService genericAttributeService) : base(
				PayPalExpressProvider.SystemName,
				paymentService,
				orderService,
				orderProcessingService)
		{
			_orderSettings = orderSettings;
			_currencyService = currencyService;
			_orderTotalCalculationService = orderTotalCalculationService;
			_customerService = customerService;
			_genericAttributeService = genericAttributeService;
		}

		private SelectList TransactModeValues(TransactMode selected)
		{
			return new SelectList(new List<object>
			{
				new { ID = (int)TransactMode.Authorize, Name = T("Plugins.Payments.PayPalExpress.ModeAuth") },
				new { ID = (int)TransactMode.AuthorizeAndCapture, Name = T("Plugins.Payments.PayPalExpress.ModeAuthAndCapture") }
			},
			"ID", "Name", (int)selected);
		}

		private string GetCheckoutButtonUrl(PayPalExpressPaymentSettings settings)
		{
			var expressCheckoutButton = "~/Plugins/SmartStore.PayPal/Content/checkout-button-default.png";
            var cultureString = Services.WorkContext.WorkingLanguage.LanguageCulture;

            if (cultureString.StartsWith("en-"))
            {
                expressCheckoutButton = "~/Plugins/SmartStore.PayPal/Content/checkout-button-en.png";
            }
            else if (cultureString.StartsWith("de-"))
            {
                expressCheckoutButton = "~/Plugins/SmartStore.PayPal/Content/checkout-button-de.png";
            }

            return expressCheckoutButton;

        }

		[AdminAuthorize, ChildActionOnly]
		public ActionResult Configure()
		{
            var model = new PayPalExpressConfigurationModel();
            int storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<PayPalExpressPaymentSettings>(storeScope);

            model.Copy(settings, true);

            model.TransactModeValues = TransactModeValues(settings.TransactMode);

			model.AvailableSecurityProtocols = PayPalService.GetSecurityProtocols()
				.Select(x => new SelectListItem { Value = ((int)x.Key).ToString(), Text = x.Value })
				.ToList();

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, Services.Settings);

			return View(model);
		}

		[HttpPost, AdminAuthorize, ChildActionOnly]
		public ActionResult Configure(PayPalExpressConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return Configure();

			ModelState.Clear();

            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            int storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<PayPalExpressPaymentSettings>(storeScope);

            model.Copy(settings, false);

			using (Services.Settings.BeginScope())
			{
				storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);

				// multistore context not possible, see IPN handling
				Services.Settings.SaveSetting(settings, x => x.UseSandbox, 0, false);
			}

            NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

			return Configure();
		}

		public ActionResult PaymentInfo()
		{
			var model = new PayPalExpressPaymentInfoModel();
			model.CurrentPageIsBasket = ControllerContext.ParentActionViewContext.RequestContext.RouteData.IsRouteEqual("ShoppingCart", "Cart");

			if (model.CurrentPageIsBasket)
			{
				var settings = Services.Settings.LoadSetting<PayPalExpressPaymentSettings>(Services.StoreContext.CurrentStore.Id);

				model.SubmitButtonImageUrl = GetCheckoutButtonUrl(settings);
			}

			return PartialView(model);
		}

		public ActionResult MiniShoppingCart()
		{
			var settings = Services.Settings.LoadSetting<PayPalExpressPaymentSettings>(Services.StoreContext.CurrentStore.Id);

			if (settings.ShowButtonInMiniShoppingCart)
			{
				var model = new PayPalExpressPaymentInfoModel();
				model.SubmitButtonImageUrl = GetCheckoutButtonUrl(settings);

				return PartialView(model);
			}

			return new EmptyResult();
		}

		public ActionResult SubmitButton()
		{
			try
			{
				//user validation
				if ((Services.WorkContext.CurrentCustomer.IsGuest() && !_orderSettings.AnonymousCheckoutAllowed))
					return RedirectToRoute("Login");

				var store = Services.StoreContext.CurrentStore;
				var customer = Services.WorkContext.CurrentCustomer;
				var settings = Services.Settings.LoadSetting<PayPalExpressPaymentSettings>(store.Id);
				var cart = Services.WorkContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

				if (cart.Count == 0)
					return RedirectToRoute("ShoppingCart");

                if (String.IsNullOrEmpty(settings.ApiAccountName))
					throw new ApplicationException("PayPal API Account Name is not set");
                if (String.IsNullOrEmpty(settings.ApiAccountPassword))
					throw new ApplicationException("PayPal API Password is not set");
                if (String.IsNullOrEmpty(settings.Signature))
					throw new ApplicationException("PayPal API Signature is not set");

				var provider = PaymentService.LoadPaymentMethodBySystemName(PayPalExpressProvider.SystemName, true);
				var processor = provider != null ? provider.Value as PayPalExpressProvider : null;
				if (processor == null)
					throw new SmartException("PayPal Express Checkout module cannot be loaded");

				var processPaymentRequest = new PayPalProcessPaymentRequest();

                processPaymentRequest.StoreId = store.Id;

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

				resultTemp = _currencyService.ConvertFromPrimaryStoreCurrency(resultTemp, Services.WorkContext.WorkingCurrency);
				if (tempDiscount > decimal.Zero)
				{
					tempDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(tempDiscount, Services.WorkContext.WorkingCurrency);
				}

				processPaymentRequest.PaymentMethodSystemName = PayPalExpressProvider.SystemName;
				processPaymentRequest.OrderTotal = resultTemp;
				processPaymentRequest.Discount = tempDiscount;
				processPaymentRequest.IsRecurringPayment = false;

				//var selectedPaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);

				processPaymentRequest.CustomerId = Services.WorkContext.CurrentCustomer.Id;
				this.Session["OrderPaymentInfo"] = processPaymentRequest;

				var resp = processor.SetExpressCheckout(processPaymentRequest, cart);

				if (resp.Ack == AckCodeType.Success)
				{
					processPaymentRequest.PaypalToken = resp.Token;
					processPaymentRequest.OrderGuid = new Guid();
					processPaymentRequest.IsShippingMethodSet = ControllerContext.RouteData.IsRouteEqual("ShoppingCart", "Cart");
					this.Session["OrderPaymentInfo"] = processPaymentRequest;

					_genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, PayPalExpressProvider.SystemName, store.Id);

					var result = new RedirectResult(String.Format(settings.GetPayPalUrl() + "?cmd=_express-checkout&useraction=commit&token={0}", resp.Token));

					return result;
				}
				else
				{
					var error = new StringBuilder("We apologize, but an error has occured.<br />");
					foreach (var errormsg in resp.Errors)
					{
						error.AppendLine(String.Format("{0} | {1} | {2}", errormsg.ErrorCode, errormsg.ShortMessage, errormsg.LongMessage));
					}

					Logger.Error(new Exception(error.ToString()), resp.Errors[0].ShortMessage);
                    
                    NotifyError(error.ToString(), false);

                    return RedirectToAction("Cart", "ShoppingCart", new { area = "" });
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex);

                NotifyError(ex.Message, false);

                return RedirectToAction("Cart", "ShoppingCart", new { area = "" });

			}
		}

		public ActionResult GetDetails(string token)
		{
			var provider = PaymentService.LoadPaymentMethodBySystemName("Payments.PayPalExpress", true);
			var processor = provider != null ? provider.Value as PayPalExpressProvider : null;
			if (processor == null)
				throw new SmartException("PayPal Express module cannot be loaded");

			var resp = processor.GetExpressCheckoutDetails(token);

			if (resp.Ack == AckCodeType.Success)
			{
				var paymentInfo = this.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
				paymentInfo = processor.SetCheckoutDetails(paymentInfo, resp.GetExpressCheckoutDetailsResponseDetails);
				this.Session["OrderPaymentInfo"] = paymentInfo;

				var store = Services.StoreContext.CurrentStore;
				var customer = _customerService.GetCustomerById(paymentInfo.CustomerId);

				Services.WorkContext.CurrentCustomer = customer;
				_customerService.UpdateCustomer(Services.WorkContext.CurrentCustomer);

				var selectedShippingOption = Services.WorkContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption,	store.Id);
				if (selectedShippingOption != null)
				{
					return RedirectToAction("Confirm", "Checkout", new { area = "" });
				}
				else
				{
					//paymentInfo.RequiresPaymentWorkflow = false;
					_genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, paymentInfo.PaymentMethodSystemName, store.Id);
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

				Logger.Error(new Exception(error.ToString()), resp.Errors[0].ShortMessage);

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

			var validator = new PayPalExpressPaymentInfoValidator(Services.Localization);
			var model = new PayPalExpressPaymentInfoModel();

			var validationResult = validator.Validate(model);

			if (!validationResult.IsValid)
			{
				foreach (var error in validationResult.Errors)
				{
					warnings.Add(error.ErrorMessage);
				}
			}

			return warnings;
		}
	}
}