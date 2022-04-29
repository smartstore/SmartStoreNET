using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Pdf;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Pdf;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Seo;
using SmartStore.Web.Models.Order;

namespace SmartStore.Web.Controllers
{
    public partial class OrderController : PublicControllerBase
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPdfConverter _pdfConverter;
        private readonly ProductUrlHelper _productUrlHelper;
        private readonly OrderHelper _orderHelper;
        private readonly IOrderService _orderService;
        private readonly IShipmentService _shipmentService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentService _paymentService;
        private readonly IShippingService _shippingService;
        private readonly ICountryService _countryService;
        private readonly OrderSettings _orderSettings;

        #endregion

        #region Constructors

        public OrderController(
            IDateTimeHelper dateTimeHelper,
            IPdfConverter pdfConverter,
            ProductUrlHelper productUrlHelper,
            OrderHelper orderHelper,
            IOrderService orderService,
            IShipmentService shipmentService,
            IOrderProcessingService orderProcessingService,
            IPaymentService paymentService,
            IShippingService shippingService,
            ICountryService countryService,
            OrderSettings orderSettings)
        {
            _dateTimeHelper = dateTimeHelper;
            _pdfConverter = pdfConverter;
            _productUrlHelper = productUrlHelper;
            _orderHelper = orderHelper;
            _orderService = orderService;
            _shipmentService = shipmentService;
            _orderProcessingService = orderProcessingService;
            _paymentService = paymentService;
            _shippingService = shippingService;
            _countryService = countryService;
            _orderSettings = orderSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected ShipmentDetailsModel PrepareShipmentDetailsModel(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException("shipment");

            var order = shipment.Order;
            if (order == null)
                throw new SmartException(T("Order.NotFound", shipment.OrderId));

            var store = Services.StoreService.GetStoreById(order.StoreId) ?? Services.StoreContext.CurrentStore;
            var catalogSettings = Services.Settings.LoadSetting<CatalogSettings>(store.Id);
            var shippingSettings = Services.Settings.LoadSetting<ShippingSettings>(store.Id);

            var model = new ShipmentDetailsModel
            {
                Id = shipment.Id,
                TrackingNumber = shipment.TrackingNumber,
                TrackingNumberUrl = shipment.TrackingUrl
            };

            if (shipment.ShippedDateUtc.HasValue)
            {
                model.ShippedDate = _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
            }

            if (shipment.DeliveryDateUtc.HasValue)
            {
                model.DeliveryDate = _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);
            }

            var srcm = _shippingService.LoadShippingRateComputationMethodBySystemName(order.ShippingRateComputationMethodSystemName);

            if (srcm != null && srcm.IsShippingRateComputationMethodActive(shippingSettings))
            {
                var shipmentTracker = srcm.Value.ShipmentTracker;
                if (shipmentTracker != null)
                {
                    // The URL entered by the merchant takes precedence over an automatically generated URL.
                    if (model.TrackingNumberUrl.IsEmpty())
                    {
                        model.TrackingNumberUrl = shipmentTracker.GetUrl(shipment.TrackingNumber);
                    }

                    if (shippingSettings.DisplayShipmentEventsToCustomers)
                    {
                        var shipmentEvents = shipmentTracker.GetShipmentEvents(shipment.TrackingNumber);
                        if (shipmentEvents != null)
                        {
                            foreach (var shipmentEvent in shipmentEvents)
                            {
                                var shipmentEventCountry = _countryService.GetCountryByTwoLetterIsoCode(shipmentEvent.CountryCode);

                                var shipmentStatusEventModel = new ShipmentDetailsModel.ShipmentStatusEventModel
                                {
                                    Country = shipmentEventCountry != null ? shipmentEventCountry.GetLocalized(x => x.Name) : shipmentEvent.CountryCode,
                                    Date = shipmentEvent.Date,
                                    EventName = shipmentEvent.EventName,
                                    Location = shipmentEvent.Location
                                };

                                model.ShipmentStatusEvents.Add(shipmentStatusEventModel);
                            }
                        }
                    }
                }
            }

            // Products in this shipment.
            model.ShowSku = catalogSettings.ShowProductSku;

            foreach (var shipmentItem in shipment.ShipmentItems)
            {
                var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                if (orderItem == null)
                    continue;

                orderItem.Product.MergeWithCombination(orderItem.AttributesXml);

                var shipmentItemModel = new ShipmentDetailsModel.ShipmentItemModel
                {
                    Id = shipmentItem.Id,
                    Sku = orderItem.Product.Sku,
                    ProductId = orderItem.Product.Id,
                    ProductName = orderItem.Product.GetLocalized(x => x.Name),
                    ProductSeName = orderItem.Product.GetSeName(),
                    AttributeInfo = orderItem.AttributeDescription,
                    QuantityOrdered = orderItem.Quantity,
                    QuantityShipped = shipmentItem.Quantity
                };

                shipmentItemModel.ProductUrl = _productUrlHelper.GetProductUrl(shipmentItemModel.ProductSeName, orderItem);

                model.Items.Add(shipmentItemModel);
            }

            model.Order = _orderHelper.PrepareOrderDetailsModel(order);
            return model;
        }

        #endregion

        #region Order details

        [RewriteUrl(SslRequirement.Yes)]
        public ActionResult Details(int id)
        {
            var order = _orderService.GetOrderById(id);

            if (IsNonExistentOrder(order))
                return HttpNotFound();

            if (IsUnauthorizedOrder(order))
                return new HttpUnauthorizedResult();

            var model = _orderHelper.PrepareOrderDetailsModel(order);
            return View(model);
        }

        [RewriteUrl(SslRequirement.Yes)]
        public ActionResult Print(int id, bool pdf = false)
        {
            var order = _orderService.GetOrderById(id);

            if (IsNonExistentOrder(order))
                return HttpNotFound();

            if (IsUnauthorizedOrder(order))
                return new HttpUnauthorizedResult();

            var model = _orderHelper.PrepareOrderDetailsModel(order);
            var fileName = T("Order.PdfInvoiceFileName", order.Id);

            return PrintCore(new List<OrderDetailsModel> { model }, pdf, fileName);
        }

        [AdminAuthorize]
        [Permission(Permissions.Order.Read)]
        public ActionResult PrintMany(string ids = null, bool pdf = false)
        {
            const int maxOrders = 500;
            IList<Order> orders = null;
            var totalCount = 0;

            using (var scope = new DbContextScope(Services.DbContext, autoDetectChanges: false, forceNoTracking: true))
            {
                if (ids != null)
                {
                    orders = _orderService.GetOrdersByIds(ids.ToIntArray());
                    totalCount = orders.Count;
                }
                else
                {
                    var pagedOrders = _orderService.SearchOrders(0, 0, null, null, null, null, null, null, null, null, 0, 1);
                    totalCount = pagedOrders.TotalCount;

                    if (totalCount > 0 && totalCount <= maxOrders)
                    {
                        orders = _orderService.SearchOrders(0, 0, null, null, null, null, null, null, null, null, 0, int.MaxValue);
                    }
                }
            }

            if (totalCount == 0)
            {
                NotifyInfo(T("Admin.Common.ExportNoData"));
                return RedirectToReferrer();
            }

            if (totalCount > maxOrders)
            {
                NotifyWarning(T("Admin.Common.ExportToPdf.TooManyItems"));
                return RedirectToReferrer();
            }

            var listModel = orders.Select(x => _orderHelper.PrepareOrderDetailsModel(x)).ToList();

            return PrintCore(listModel, pdf, "orders.pdf");
        }

        [NonAction]
        private ActionResult PrintCore(List<OrderDetailsModel> model, bool pdf, string pdfFileName)
        {
            ViewBag.PdfMode = pdf;
            var viewName = "Details.Print";

            if (pdf)
            {
                // TODO: (mc) this is bad for multi-document processing, where orders can originate from different stores.
                var storeId = model[0].StoreId;
                var routeValues = new RouteValueDictionary
                {
                    ["storeId"] = storeId,
                    ["lid"] = Services.WorkContext.WorkingLanguage.Id
                };
                var pdfSettings = Services.Settings.LoadSetting<PdfSettings>(storeId);

                var settings = new PdfConvertSettings
                {
                    Size = pdfSettings.LetterPageSizeEnabled ? PdfPageSize.Letter : PdfPageSize.A4,
                    Margins = new PdfPageMargins { Top = 35, Bottom = 35 },
                    Page = new PdfViewContent(viewName, model, this.ControllerContext),
                    Header = new PdfRouteContent("PdfReceiptHeader", "Common", routeValues, this.ControllerContext),
                    Footer = new PdfRouteContent("PdfReceiptFooter", "Common", routeValues, this.ControllerContext)
                };

                return new PdfResult(_pdfConverter, settings) { FileName = pdfFileName };
            }

            return View(viewName, model);
        }

        public ActionResult ReOrder(int id)
        {
            var order = _orderService.GetOrderById(id);

            if (IsNonExistentOrder(order))
                return HttpNotFound();

            if (IsUnauthorizedOrder(order))
                return new HttpUnauthorizedResult();

            _orderProcessingService.ReOrder(order);
            return RedirectToRoute("ShoppingCart");
        }

        [HttpPost, ActionName("Details")]
        [FormValueRequired("repost-payment")]
        public ActionResult RePostPayment(int id)
        {
            var order = _orderService.GetOrderById(id);

            if (IsNonExistentOrder(order))
                return HttpNotFound();

            if (IsUnauthorizedOrder(order))
                return new HttpUnauthorizedResult();

            try
            {
                if (_paymentService.CanRePostProcessPayment(order))
                {
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = order,
                        IsRePostProcessPayment = true
                    };

                    _paymentService.PostProcessPayment(postProcessPaymentRequest);

                    if (postProcessPaymentRequest.RedirectUrl.HasValue())
                    {
                        return Redirect(postProcessPaymentRequest.RedirectUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            return RedirectToAction("Details", "Order", new { id = order.Id });
        }

        [RewriteUrl(SslRequirement.Yes)]
        public ActionResult ShipmentDetails(int id /* shipmentId */)
        {
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                return HttpNotFound();

            var order = shipment.Order;

            if (IsNonExistentOrder(order))
                return HttpNotFound();

            if (IsUnauthorizedOrder(order))
                return new HttpUnauthorizedResult();

            var model = PrepareShipmentDetailsModel(shipment);

            return View(model);
        }

        private bool IsNonExistentOrder(Order order)
        {
            var result = order == null || order.Deleted;

            if (!Services.Permissions.Authorize(Permissions.Order.Read))
            {
                result = result || (order.StoreId != 0 && order.StoreId != Services.StoreContext.CurrentStore.Id);

                if (_orderSettings.DisplayOrdersOfAllStores)
                {
                    result = false;
                }
            }

            return result;
        }

        private bool IsUnauthorizedOrder(Order order)
        {
            if (!Services.Permissions.Authorize(Permissions.Order.Read))
                return order == null || order.CustomerId != Services.WorkContext.CurrentCustomer.Id;
            else
                return order == null;
        }

        #endregion
    }
}
