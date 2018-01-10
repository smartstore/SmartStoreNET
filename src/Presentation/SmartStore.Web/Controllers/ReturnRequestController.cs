using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tax;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Models.Order;

namespace SmartStore.Web.Controllers
{
	public partial class ReturnRequestController : PublicControllerBase
    {
		#region Fields

        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
		private readonly ProductUrlHelper _productUrlHelper;
		private readonly LocalizationSettings _localizationSettings;
        private readonly OrderSettings _orderSettings;

        #endregion

		#region Constructors

        public ReturnRequestController(
			IOrderService orderService,
			IWorkContext workContext, IStoreContext storeContext,
            ICurrencyService currencyService, IPriceFormatter priceFormatter,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            ICustomerService customerService,
			ProductUrlHelper productUrlHelper,
			LocalizationSettings localizationSettings,
            OrderSettings orderSettings)
        {
            _orderService = orderService;
            _workContext = workContext;
			_storeContext = storeContext;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _orderProcessingService = orderProcessingService;
            _localizationService = localizationService;
            _customerService = customerService;
			_productUrlHelper = productUrlHelper;
            _localizationSettings = localizationSettings;
            _orderSettings = orderSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected SubmitReturnRequestModel PrepareReturnRequestModel(SubmitReturnRequestModel model, Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (model == null)
                throw new ArgumentNullException("model");

            model.OrderId = order.Id;

			string returnRequestReasons = _orderSettings.GetLocalized(x => x.ReturnRequestReasons, order.CustomerLanguageId, true, false);
			string returnRequestActions = _orderSettings.GetLocalized(x => x.ReturnRequestActions, order.CustomerLanguageId, true, false);

            //return reasons
            foreach (var rrr in returnRequestReasons.SplitSafe(","))
            {
                model.AvailableReturnReasons.Add(new SelectListItem { Text = rrr, Value = rrr });
            }

            //return actions
            foreach (var rra in returnRequestActions.SplitSafe(","))
            {
                model.AvailableReturnActions.Add(new SelectListItem { Text = rra, Value = rra });
            }

            //products
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

				//unit price
				switch (order.CustomerTaxDisplayType)
                {
                    case TaxDisplayType.ExcludingTax:
                        {
                            var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                            orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, false);
                        }
                        break;
                    case TaxDisplayType.IncludingTax:
                        {
                            var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                            orderItemModel.UnitPrice = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, _workContext.WorkingLanguage, true);
                        }
                        break;
                }

				model.Items.Add(orderItemModel);
            }

            return model;
        }

        #endregion
        
        #region Return requests

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
		public ActionResult ReturnRequest(int id /* orderId */)
        {
			var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToRoute("HomePage");

            var model = new SubmitReturnRequestModel();
            model = PrepareReturnRequestModel(model, order);
            return View(model);
        }

        [HttpPost, ActionName("ReturnRequest")] 
        [ValidateInput(false)]
        public ActionResult ReturnRequestSubmit(int id /* orderId */, SubmitReturnRequestModel model, FormCollection form)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted || _workContext.CurrentCustomer.Id != order.CustomerId)
                return new HttpUnauthorizedResult();

            if (!_orderProcessingService.IsReturnRequestAllowed(order))
                return RedirectToRoute("HomePage");

            int count = 0;
            foreach (var orderItem in order.OrderItems)
            {
                int quantity = 0; //parse quantity
                foreach (string formKey in form.AllKeys)
                    if (formKey.Equals(string.Format("quantity{0}", orderItem.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out quantity);
                        break;
                    }
                if (quantity > 0)
                {
                    var rr = new ReturnRequest
                    {
						StoreId = _storeContext.CurrentStore.Id,
                        OrderItemId = orderItem.Id,
                        Quantity = quantity,
                        CustomerId = _workContext.CurrentCustomer.Id,
                        ReasonForReturn = model.ReturnReason,
                        RequestedAction = model.ReturnAction,
                        CustomerComments = model.Comments,
                        StaffNotes = string.Empty,
                        ReturnRequestStatus = ReturnRequestStatus.Pending
                    };
                    _workContext.CurrentCustomer.ReturnRequests.Add(rr);
                    _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                    // notify store owner here (email)
                    Services.MessageFactory.SendNewReturnRequestStoreOwnerNotification(rr, orderItem, _localizationSettings.DefaultAdminLanguageId);

                    count++;
                }
            }

            model = PrepareReturnRequestModel(model, order);
            if (count > 0)
                model.Result = _localizationService.GetResource("ReturnRequests.Submitted");
            else
                model.Result = _localizationService.GetResource("ReturnRequests.NoItemsSubmitted");

            return View(model);
        }

        #endregion
    }
}
