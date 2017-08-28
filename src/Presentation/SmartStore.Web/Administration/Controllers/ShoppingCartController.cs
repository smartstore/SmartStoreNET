using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.ShoppingCart;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class ShoppingCartController : AdminControllerBase
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
		private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Constructors

        public ShoppingCartController(ICustomerService customerService,
            IDateTimeHelper dateTimeHelper, IPriceFormatter priceFormatter,
			IStoreService storeService, ITaxService taxService,
			IPriceCalculationService priceCalculationService,
            IPermissionService permissionService, ILocalizationService localizationService)
        {
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
			this._storeService = storeService;
            this._taxService = taxService;
            this._priceCalculationService = priceCalculationService;
            this._permissionService = permissionService;
            this._localizationService = localizationService;
        }

        #endregion
        
        #region Methods

        //shopping carts
        public ActionResult CurrentCarts()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CurrentCarts(GridCommand command)
        {
			var gridModel = new GridModel<ShoppingCartModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
			{
				var customers = _customerService.GetAllCustomers(null, null, null, null, null, null, null,
					0, 0, null, null, null, true, ShoppingCartType.ShoppingCart, command.Page - 1, command.PageSize);
				
				gridModel.Data = customers.Select(x =>
				{
					return new ShoppingCartModel
					{
						CustomerId = x.Id,
						CustomerEmail = x.IsGuest() ? T("Admin.Customers.Guest").Text : x.Email,
						TotalItems = x.CountProductsInCart(ShoppingCartType.ShoppingCart)
					};
				});
				
				gridModel.Total = customers.TotalCount;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<ShoppingCartModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GetCartDetails(int customerId)
        {
			var gridModel = new GridModel<ShoppingCartItemModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
			{
				var customer = _customerService.GetCustomerById(customerId);
				var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart);

				gridModel.Data = cart.Select(sci =>
				{
					decimal taxRate;
					var store = _storeService.GetStoreById(sci.Item.StoreId);

					var sciModel = new ShoppingCartItemModel
					{
						Id = sci.Item.Id,
						Store = store != null ? store.Name : "".NaIfEmpty(),
						ProductId = sci.Item.ProductId,
						Quantity = sci.Item.Quantity,
						ProductName = sci.Item.Product.Name,
						ProductTypeName = sci.Item.Product.GetProductTypeLabel(_localizationService),
						ProductTypeLabelHint = sci.Item.Product.ProductTypeLabelHint,
						UnitPrice = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate)),
						Total = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetSubTotal(sci, true), out taxRate)),
						UpdatedOn = _dateTimeHelper.ConvertToUserTime(sci.Item.UpdatedOnUtc, DateTimeKind.Utc)
					};
					return sciModel;
				});

				gridModel.Total = cart.Count;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<ShoppingCartItemModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }


        //wishlists
        public ActionResult CurrentWishlists()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CurrentWishlists(GridCommand command)
        {
			var gridModel = new GridModel<ShoppingCartModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
			{
				var customers = _customerService.GetAllCustomers(null, null, null, null, null, null, null,
					0, 0, null, null, null,	true, ShoppingCartType.Wishlist, command.Page - 1, command.PageSize);

				gridModel.Data = customers.Select(x =>
				{
					return new ShoppingCartModel
					{
						CustomerId = x.Id,
						CustomerEmail = x.IsGuest() ? T("Admin.Customers.Guest").Text : x.Email,
						TotalItems = x.CountProductsInCart(ShoppingCartType.Wishlist)
					};
				});

				gridModel.Total = customers.TotalCount;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<ShoppingCartModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GetWishlistDetails(int customerId)
        {
			var gridModel = new GridModel<ShoppingCartItemModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
			{
				var customer = _customerService.GetCustomerById(customerId);
				var cart = customer.GetCartItems(ShoppingCartType.Wishlist);

				gridModel.Data = cart.Select(sci =>
				{
					decimal taxRate;
					var store = _storeService.GetStoreById(sci.Item.StoreId);

					var sciModel = new ShoppingCartItemModel
					{
						Id = sci.Item.Id,
						Store = store != null ? store.Name : "".NaIfEmpty(),
						ProductId = sci.Item.ProductId,
						Quantity = sci.Item.Quantity,
						ProductName = sci.Item.Product.Name,
						ProductTypeName = sci.Item.Product.GetProductTypeLabel(_localizationService),
						ProductTypeLabelHint = sci.Item.Product.ProductTypeLabelHint,
						UnitPrice = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate)),
						Total = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetSubTotal(sci, true), out taxRate)),
						UpdatedOn = _dateTimeHelper.ConvertToUserTime(sci.Item.UpdatedOnUtc, DateTimeKind.Utc)
					};
					return sciModel;
				});

				gridModel.Total = cart.Count;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<ShoppingCartItemModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }

        #endregion
    }
}
