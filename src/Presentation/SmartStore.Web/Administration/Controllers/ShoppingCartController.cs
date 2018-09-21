using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.ShoppingCart;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Security;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class ShoppingCartController : AdminControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;

        public ShoppingCartController(
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper, 
            IPriceFormatter priceFormatter,
            ITaxService taxService,
			IPriceCalculationService priceCalculationService)
        {
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _taxService = taxService;
            _priceCalculationService = priceCalculationService;
        }
        
        public ActionResult CurrentCarts()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageOrders))
            {
                return AccessDeniedView();
            }

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CurrentCarts(GridCommand command)
        {
			var gridModel = new GridModel<ShoppingCartModel>();

			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageOrders))
			{
				var q = new CustomerSearchQuery
				{
					OnlyWithCart = true,
					CartType = ShoppingCartType.ShoppingCart,
					PageIndex = command.Page - 1,
					PageSize = command.PageSize
				};

                var guestStr = T("Admin.Customers.Guest").Text;
                var customers = _customerService.SearchCustomers(q);
				
				gridModel.Data = customers.Select(x =>
				{
					return new ShoppingCartModel
					{
						CustomerId = x.Id,
						CustomerEmail = x.IsGuest() ? guestStr : x.Email,
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

			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageOrders))
			{
                decimal taxRate;
                var customer = _customerService.GetCustomerById(customerId);
				var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart);

				gridModel.Data = cart.Select(sci =>
				{
					var store = Services.StoreService.GetStoreById(sci.Item.StoreId);

					var sciModel = new ShoppingCartItemModel
					{
						Id = sci.Item.Id,
						Store = store != null ? store.Name : "".NaIfEmpty(),
						ProductId = sci.Item.ProductId,
						Quantity = sci.Item.Quantity,
						ProductName = sci.Item.Product.Name,
						ProductTypeName = sci.Item.Product.GetProductTypeLabel(Services.Localization),
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

        public ActionResult CurrentWishlists()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageOrders))
            {
                return AccessDeniedView();
            }

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult CurrentWishlists(GridCommand command)
        {
			var gridModel = new GridModel<ShoppingCartModel>();

			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageOrders))
			{
				var q = new CustomerSearchQuery
				{
					OnlyWithCart = true,
					CartType = ShoppingCartType.Wishlist,
					PageIndex = command.Page - 1,
					PageSize = command.PageSize
				};

                var guestStr = T("Admin.Customers.Guest").Text;
                var customers = _customerService.SearchCustomers(q);

				gridModel.Data = customers.Select(x =>
				{
					return new ShoppingCartModel
					{
						CustomerId = x.Id,
						CustomerEmail = x.IsGuest() ? guestStr : x.Email,
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

			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageOrders))
			{
                decimal taxRate;
                var customer = _customerService.GetCustomerById(customerId);
				var cart = customer.GetCartItems(ShoppingCartType.Wishlist);

				gridModel.Data = cart.Select(sci =>
				{
					var store = Services.StoreService.GetStoreById(sci.Item.StoreId);

					var sciModel = new ShoppingCartItemModel
					{
						Id = sci.Item.Id,
						Store = store != null ? store.Name : "".NaIfEmpty(),
						ProductId = sci.Item.ProductId,
						Quantity = sci.Item.Quantity,
						ProductName = sci.Item.Product.Name,
						ProductTypeName = sci.Item.Product.GetProductTypeLabel(Services.Localization),
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
    }
}
