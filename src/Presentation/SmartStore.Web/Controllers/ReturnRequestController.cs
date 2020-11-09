using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tax;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Seo;
using SmartStore.Web.Models.Order;

namespace SmartStore.Web.Controllers
{
    public partial class ReturnRequestController : PublicControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ICustomerService _customerService;
        private readonly ProductUrlHelper _productUrlHelper;
        private readonly LocalizationSettings _localizationSettings;
        private readonly OrderSettings _orderSettings;

        public ReturnRequestController(
            IOrderService orderService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IOrderProcessingService orderProcessingService,
            ICustomerService customerService,
            ProductUrlHelper productUrlHelper,
            LocalizationSettings localizationSettings,
            OrderSettings orderSettings)
        {
            _orderService = orderService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _orderProcessingService = orderProcessingService;
            _customerService = customerService;
            _productUrlHelper = productUrlHelper;
            _localizationSettings = localizationSettings;
            _orderSettings = orderSettings;
        }

        #region Utilities

        [NonAction]
        protected SubmitReturnRequestModel PrepareReturnRequestModel(SubmitReturnRequestModel model, Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (model == null)
                throw new ArgumentNullException("model");

            model.OrderId = order.Id;
            
            var language = Services.WorkContext.WorkingLanguage;
            string returnRequestReasons = _orderSettings.GetLocalizedSetting(x => x.ReturnRequestReasons, order.CustomerLanguageId, order.StoreId, true, false);
            string returnRequestActions = _orderSettings.GetLocalizedSetting(x => x.ReturnRequestActions, order.CustomerLanguageId, order.StoreId, true, false);

            // Return reasons.
            foreach (var rrr in returnRequestReasons.SplitSafe(","))
            {
                model.AvailableReturnReasons.Add(new SelectListItem { Text = rrr, Value = rrr });
            }

            // Return actions.
            foreach (var rra in returnRequestActions.SplitSafe(","))
            {
                model.AvailableReturnActions.Add(new SelectListItem { Text = rra, Value = rra });
            }

            // Products.
            var orderItems = _orderService.GetAllOrderItems(order.Id, null, null, null, null, null, null);

            foreach (var orderItem in orderItems)
            {
                var orderItemModel = new SubmitReturnRequestModel.OrderItemModel
                {
                    Id = orderItem.Id,
                    ProductId = orderItem.Product.Id,
                    ProductName = orderItem.Product.GetLocalized(x => x.Name),
                    ProductSeName = orderItem.Product.GetSeName(),
                    AttributeInfo = orderItem.AttributeDescription,
                    Quantity = orderItem.Quantity
                };

                orderItemModel.ProductUrl = _productUrlHelper.GetProductUrl(orderItemModel.ProductSeName, orderItem);

                // Unit price.
                switch (order.CustomerTaxDisplayType)
                {
                    case TaxDisplayType.ExcludingTax:
                        {
                            var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                            orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                        }
                        break;
                    case TaxDisplayType.IncludingTax:
                        {
                            var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                            orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                        }
                        break;
                }

                model.Items.Add(orderItemModel);
            }

            return model;
        }

        #endregion

        #region Return requests

        [RewriteUrl(SslRequirement.Yes)]
        public ActionResult ReturnRequest(int id /* orderId */)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted || Services.WorkContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToRoute("HomePage");

            var model = new SubmitReturnRequestModel();
            model = PrepareReturnRequestModel(model, order);
            return View(model);
        }

        [HttpPost, ActionName("ReturnRequest")]
        public ActionResult ReturnRequestSubmit(int id /* orderId */, SubmitReturnRequestModel model, FormCollection form)
        {
            var order = _orderService.GetOrderById(id);
            var customer = Services.WorkContext.CurrentCustomer;

            if (order == null || order.Deleted || customer.Id != order.CustomerId)
            {
                return new HttpUnauthorizedResult();
            }

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
            {
                return RedirectToRoute("HomePage");
            }

            foreach (var orderItem in order.OrderItems)
            {
                var quantity = 0;
                foreach (var formKey in form.AllKeys)
                {
                    if (formKey.Equals(string.Format("quantity{0}", orderItem.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out quantity);
                        break;
                    }
                }

                if (quantity > 0)
                {
                    var rr = new ReturnRequest
                    {
                        StoreId = Services.StoreContext.CurrentStore.Id,
                        OrderItemId = orderItem.Id,
                        Quantity = quantity,
                        CustomerId = customer.Id,
                        ReasonForReturn = model.ReturnReason,
                        RequestedAction = model.ReturnAction,
                        CustomerComments = model.Comments,
                        StaffNotes = string.Empty,
                        ReturnRequestStatus = ReturnRequestStatus.Pending
                    };
                    customer.ReturnRequests.Add(rr);
                    _customerService.UpdateCustomer(customer);

                    model.AddedReturnRequestIds.Add(rr.Id);

                    // Notify store owner here (email).
                    Services.MessageFactory.SendNewReturnRequestStoreOwnerNotification(rr, orderItem, _localizationSettings.DefaultAdminLanguageId);
                }
            }

            model = PrepareReturnRequestModel(model, order);

            if (model.AddedReturnRequestIds.Any())
            {
                model.Result = T("ReturnRequests.Submitted");
            }
            else
            {
                NotifyWarning(T("ReturnRequests.NoItemsSubmitted"));
            }

            return View(model);
        }

        #endregion
    }
}
