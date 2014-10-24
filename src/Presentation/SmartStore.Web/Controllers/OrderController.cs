﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Html;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Models.Order;

namespace SmartStore.Web.Controllers
{
    public partial class OrderController : PublicControllerBase
    {
		#region Fields

        private readonly IOrderService _orderService;
        private readonly IShipmentService _shipmentService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPaymentService _paymentService;
        private readonly ILocalizationService _localizationService;
        private readonly IPdfService _pdfService;
        private readonly IShippingService _shippingService;
        private readonly ICountryService _countryService;
        private readonly IWebHelper _webHelper;
		private readonly IProductService _productService;
		private readonly IProductAttributeFormatter _productAttributeFormatter;

        private readonly OrderSettings _orderSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly AddressSettings _addressSettings;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
		private readonly PluginMediator _pluginMediator;

        #endregion

		#region Constructors

        public OrderController(IOrderService orderService, 
            IShipmentService shipmentService, IWorkContext workContext,
            ICurrencyService currencyService, IPriceFormatter priceFormatter,
            IOrderProcessingService orderProcessingService, IDateTimeHelper dateTimeHelper,
            IPaymentService paymentService, ILocalizationService localizationService,
            IPdfService pdfService, IShippingService shippingService,
            ICountryService countryService, IWebHelper webHelper, 
            CatalogSettings catalogSettings, OrderSettings orderSettings,
            TaxSettings taxSettings, PdfSettings pdfSettings,
            ShippingSettings shippingSettings, AddressSettings addressSettings,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
			IProductService productService,
			IProductAttributeFormatter productAttributeFormatter,
			PluginMediator pluginMediator)
        {
            this._orderService = orderService;
            this._shipmentService = shipmentService;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._priceFormatter = priceFormatter;
            this._orderProcessingService = orderProcessingService;
            this._dateTimeHelper = dateTimeHelper;
            this._paymentService = paymentService;
            this._localizationService = localizationService;
            this._pdfService = pdfService;
            this._shippingService = shippingService;
            this._countryService = countryService;
            this._webHelper = webHelper;
			this._productService = productService;
			this._productAttributeFormatter = productAttributeFormatter;

            this._catalogSettings = catalogSettings;
            this._orderSettings = orderSettings;
            this._taxSettings = taxSettings;
            this._pdfSettings = pdfSettings;
            this._shippingSettings = shippingSettings;
            this._addressSettings = addressSettings;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
			this._pluginMediator = pluginMediator;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected OrderDetailsModel PrepareOrderDetailsModel(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");
            var model = new OrderDetailsModel();

            model.Id = order.Id;
            model.OrderNumber = order.GetOrderNumber();
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.IsReOrderAllowed = _orderSettings.IsReOrderAllowed;
            model.IsReturnRequestAllowed = _orderProcessingService.IsReturnRequestAllowed(order);
            model.DisplayPdfInvoice = _pdfSettings.Enabled;

            //shipping info
            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;
                model.ShippingAddress.PrepareModel(order.ShippingAddress, false, _addressSettings);
                model.ShippingMethod = order.ShippingMethod;
   

                //shipments (only already shipped)
                var shipments = order.Shipments.Where(x => x.ShippedDateUtc.HasValue).OrderBy(x => x.CreatedOnUtc).ToList();
                foreach (var shipment in shipments)
                {
                    var shipmentModel = new OrderDetailsModel.ShipmentBriefModel()
                    {
                        Id = shipment.Id,
                        TrackingNumber = shipment.TrackingNumber,
                    };
                    if (shipment.ShippedDateUtc.HasValue)
                        shipmentModel.ShippedDate = _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
                    if (shipment.DeliveryDateUtc.HasValue)
                        shipmentModel.DeliveryDate = _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);
                    model.Shipments.Add(shipmentModel);
                }
            }


            //billing info
            model.BillingAddress.PrepareModel(order.BillingAddress, false, _addressSettings);

            //VAT number
            model.VatNumber = order.VatNumber;

            //payment method
            var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
			model.PaymentMethod = paymentMethod != null ? _pluginMediator.GetLocalizedFriendlyName(paymentMethod.Metadata) : order.PaymentMethodSystemName;
            model.CanRePostProcessPayment = _paymentService.CanRePostProcessPayment(order);

            //purchase order number (we have to find a better to inject this information because it's related to a certain plugin)
            if (paymentMethod != null && paymentMethod.Metadata.SystemName.Equals("Payments.PurchaseOrder", StringComparison.InvariantCultureIgnoreCase))
            {
                model.DisplayPurchaseOrderNumber = true;
                model.PurchaseOrderNumber = order.PurchaseOrderNumber;
            }


            //totals)
            switch (order.CustomerTaxDisplayType)
            {
                case TaxDisplayType.ExcludingTax:
                    {
                        //order subtotal
                        var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                        model.OrderSubtotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                        //discount (applied to order subtotal)
                        var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                        if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                            model.OrderSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                        //order shipping
                        var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                        model.OrderShipping = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                        //payment method additional fee
                        var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                        if (paymentMethodAdditionalFeeExclTaxInCustomerCurrency > decimal.Zero)
                            model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                    }
                    break;
                case TaxDisplayType.IncludingTax:
                    {
                        //order subtotal
                        var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                        model.OrderSubtotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                        //discount (applied to order subtotal)
                        var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                        if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                            model.OrderSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                        //order shipping
                        var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                        model.OrderShipping = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                        //payment method additional fee
                        var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                        if (paymentMethodAdditionalFeeInclTaxInCustomerCurrency > decimal.Zero)
                            model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                    }
                    break;
            }

            //tax
            bool displayTax = true;
            bool displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    displayTaxRates = _taxSettings.DisplayTaxRates && order.TaxRatesDictionary.Count > 0;
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    //TODO pass languageId to _priceFormatter.FormatPrice
                    model.Tax = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);

                    foreach (var tr in order.TaxRatesDictionary)
                    {
                        model.TaxRates.Add(new OrderDetailsModel.TaxRate()
                        {
                            Rate = _priceFormatter.FormatTaxRate(tr.Key),
                            //TODO pass languageId to _priceFormatter.FormatPrice
                            Value = _priceFormatter.FormatPrice(_currencyService.ConvertCurrency(tr.Value, order.CurrencyRate), true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage),
                        });
                    }
                }
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;


            //discount (applied to order total)
            var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
            if (orderDiscountInCustomerCurrency > decimal.Zero)
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);


            //gift cards
            foreach (var gcuh in order.GiftCardUsageHistory)
            {
                model.GiftCards.Add(new OrderDetailsModel.GiftCard()
                {
                    CouponCode = gcuh.GiftCard.GiftCardCouponCode,
                    Amount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage),
                });
            }

            //reward points           
            if (order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            model.OrderTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, _workContext.WorkingLanguage);

            //checkout attributes
            //codehint: sm-edit
            model.CheckoutAttributeInfo = HtmlUtils.ConvertPlainTextToTable(HtmlUtils.ConvertHtmlToPlainText(order.CheckoutAttributeDescription));
            
            //order notes
            foreach (var orderNote in order.OrderNotes
                .Where(on => on.DisplayToCustomer)
                .OrderByDescending(on => on.CreatedOnUtc)
                .ToList())
            {
                model.OrderNotes.Add(new OrderDetailsModel.OrderNote()
                {
                    Note = orderNote.FormatOrderNoteText(),
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc)
                });
            }


            //purchased products
            model.ShowSku = _catalogSettings.ShowProductSku;
            var orderItems = _orderService.GetAllOrderItems(order.Id, null, null, null, null, null, null);

            foreach (var orderItem in orderItems)
            {
				var orderItemModel = PrepareOrderItemModel(order, orderItem);

                model.Items.Add(orderItemModel);
            }

            return model;
        }

        [NonAction]
        protected ShipmentDetailsModel PrepareShipmentDetailsModel(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = shipment.Order;
            if (order == null)
                throw new Exception("order cannot be loaded");
            var model = new ShipmentDetailsModel();
            
            model.Id = shipment.Id;
            if (shipment.ShippedDateUtc.HasValue)
                model.ShippedDate = _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
            if (shipment.DeliveryDateUtc.HasValue)
                model.DeliveryDate = _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);
            
            //tracking number and shipment information
            model.TrackingNumber = shipment.TrackingNumber;
            var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName(order.ShippingRateComputationMethodSystemName);
            if (srcm != null && srcm.IsShippingRateComputationMethodActive(_shippingSettings))
            {
                var shipmentTracker = srcm.Value.ShipmentTracker;
                if (shipmentTracker != null)
                {
                    model.TrackingNumberUrl = shipmentTracker.GetUrl(shipment.TrackingNumber);
                    if (_shippingSettings.DisplayShipmentEventsToCustomers)
                    {
                        var shipmentEvents = shipmentTracker.GetShipmentEvents(shipment.TrackingNumber);
                        if (shipmentEvents != null)
                            foreach (var shipmentEvent in shipmentEvents)
                            {
                                var shipmentStatusEventModel = new ShipmentDetailsModel.ShipmentStatusEventModel();
                                var shipmentEventCountry = _countryService.GetCountryByTwoLetterIsoCode(shipmentEvent.CountryCode);
                                shipmentStatusEventModel.Country = shipmentEventCountry != null
                                                                       ? shipmentEventCountry.GetLocalized(x => x.Name)
                                                                       : shipmentEvent.CountryCode;
                                shipmentStatusEventModel.Date = shipmentEvent.Date;
                                shipmentStatusEventModel.EventName = shipmentEvent.EventName;
                                shipmentStatusEventModel.Location = shipmentEvent.Location;
                                model.ShipmentStatusEvents.Add(shipmentStatusEventModel);
                            }
                    }
                }
            }
            
            //products in this shipment
            model.ShowSku = _catalogSettings.ShowProductSku;
            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                if (orderItem == null)
                    continue;

                orderItem.Product.MergeWithCombination(orderItem.AttributesXml);
                var shipmentItemModel = new ShipmentDetailsModel.ShipmentItemModel()
                {
                    Id = shipmentItem.Id,
                    Sku = orderItem.Product.Sku,
                    ProductId = orderItem.Product.Id,
					ProductName = orderItem.Product.GetLocalized(x => x.Name),
                    ProductSeName = orderItem.Product.GetSeName(),
                    AttributeInfo = orderItem.AttributeDescription,
                    QuantityOrdered = orderItem.Quantity,
                    QuantityShipped = shipmentItem.Quantity,
                };
                model.Items.Add(shipmentItemModel);
            }

            //order details model
            model.Order = PrepareOrderDetailsModel(order);
            
            return model;
        }

		private OrderDetailsModel.OrderItemModel PrepareOrderItemModel(Order order, OrderItem orderItem)
		{
			orderItem.Product.MergeWithCombination(orderItem.AttributesXml);

			var model = new OrderDetailsModel.OrderItemModel()
			{
				Id = orderItem.Id,
				Sku = orderItem.Product.Sku,
				ProductId = orderItem.Product.Id,
				ProductName = orderItem.Product.GetLocalized(x => x.Name),
				ProductSeName = orderItem.Product.GetSeName(),
				ProductType = orderItem.Product.ProductType,
				Quantity = orderItem.Quantity,
				AttributeInfo = orderItem.AttributeDescription
			};

			if (orderItem.Product.ProductType == ProductType.BundledProduct && orderItem.BundleData.HasValue())
			{
				var bundleData = orderItem.GetBundleData();

				model.BundlePerItemPricing = orderItem.Product.BundlePerItemPricing;
				model.BundlePerItemShoppingCart = bundleData.Any(x => x.PerItemShoppingCart);

				foreach (var bundleItem in bundleData)
				{
					var bundleItemModel = new OrderDetailsModel.BundleItemModel()
					{
						Sku = bundleItem.Sku,
						ProductName = bundleItem.ProductName,
						ProductSeName = bundleItem.ProductSeName,
						VisibleIndividually = bundleItem.VisibleIndividually,
						Quantity = bundleItem.Quantity,
						DisplayOrder = bundleItem.DisplayOrder,
						AttributeInfo = bundleItem.AttributesInfo
					};

					if (model.BundlePerItemShoppingCart)
					{
						decimal priceWithDiscount = _currencyService.ConvertCurrency(bundleItem.PriceWithDiscount, order.CurrencyRate);
						bundleItemModel.PriceWithDiscount = _priceFormatter.FormatPrice(priceWithDiscount, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
					}
					
					model.BundleItems.Add(bundleItemModel);
				}
			}

			//unit price, subtotal
			switch (order.CustomerTaxDisplayType)
			{
				case TaxDisplayType.ExcludingTax:
					{
						var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
						model.UnitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);

						var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
						model.SubTotal = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
					}
					break;
				case TaxDisplayType.IncludingTax:
					{
						var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
						model.UnitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);

						var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
						model.SubTotal = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
					}
					break;
			}
			return model;
		}

        #endregion

        #region Order details

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult Details(int id)
        {
			if (id < 1)
				return HttpNotFound();
			
			var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            var model = PrepareOrderDetailsModel(order);

            return View(model);
        }

		[ActionName("print")]
        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult PrintOrderDetails(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            var model = PrepareOrderDetailsModel(order);
            model.PrintMode = true;

            return View("Details", model);
        }

		[ActionName("pdf")]
        public ActionResult GetPdfInvoice(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            var orders = new List<Order>();
            orders.Add(order);

			return File(_pdfService.PrintOrdersToPdf(orders), MediaTypeNames.Application.Pdf, "order-{0}.pdf".FormatWith(order.Id));
        }

        public ActionResult ReOrder(int id)
        {
			if (id < 1)
				return HttpNotFound();
			
			var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            _orderProcessingService.ReOrder(order);
            return RedirectToRoute("ShoppingCart");
        }

        [HttpPost, ActionName("Details")]
        [FormValueRequired("repost-payment")]
        public ActionResult RePostPayment(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            if (!_paymentService.CanRePostProcessPayment(order))
				return RedirectToAction("Details", "Order", new { id = order.Id });

            var postProcessPaymentRequest = new PostProcessPaymentRequest()
            {
                Order = order
            };
            _paymentService.PostProcessPayment(postProcessPaymentRequest);

            if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
            {
                //redirection or POST has been done in PostProcessPayment
                return Content("Redirected");
            }
            else
            {
                //if no redirection has been done (to a third-party payment page)
                //theoretically it's not possible
				return RedirectToAction("Details", "Order", new { id = order.Id });
            }
        }

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult ShipmentDetails(int shipmentId)
        {
            var shipment = _shipmentService.GetShipmentById(shipmentId);
            if (shipment == null)
                return new HttpUnauthorizedResult();

            var order = shipment.Order;
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            var model = PrepareShipmentDetailsModel(shipment);

            return View(model);
        }
        #endregion
    }
}
