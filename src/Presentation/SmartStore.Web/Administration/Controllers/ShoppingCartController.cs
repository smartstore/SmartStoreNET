using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.ShoppingCart;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
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

        [Permission(Permissions.Cart.Read)]
        public ActionResult CurrentCarts()
        {
            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cart.Read)]
        public ActionResult CurrentCarts(GridCommand command)
        {
            var gridModel = new GridModel<ShoppingCartModel>();

            var query = new CustomerSearchQuery
            {
                OnlyWithCart = true,
                CartType = ShoppingCartType.ShoppingCart,
                PageIndex = command.Page - 1,
                PageSize = command.PageSize
            };

            var guestStr = T("Admin.Customers.Guest").Text;
            var customers = _customerService.SearchCustomers(query);

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

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cart.Read)]
        public ActionResult GetCartDetails(int customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            var models = GetCartItemModels(ShoppingCartType.ShoppingCart, customer);

            var gridModel = new GridModel<ShoppingCartItemModel>
            {
                Data = models,
                Total = models.Count
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Cart.Read)]
        public ActionResult CurrentWishlists()
        {
            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cart.Read)]
        public ActionResult CurrentWishlists(GridCommand command)
        {
            var gridModel = new GridModel<ShoppingCartModel>();

            var query = new CustomerSearchQuery
            {
                OnlyWithCart = true,
                CartType = ShoppingCartType.Wishlist,
                PageIndex = command.Page - 1,
                PageSize = command.PageSize
            };

            var guestStr = T("Admin.Customers.Guest").Text;
            var customers = _customerService.SearchCustomers(query);

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

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cart.Read)]
        public ActionResult GetWishlistDetails(int customerId)
        {
            var customer = _customerService.GetCustomerById(customerId);
            var models = GetCartItemModels(ShoppingCartType.Wishlist, customer);

            var gridModel = new GridModel<ShoppingCartItemModel>
            {
                Data = models,
                Total = models.Count
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }

        private List<ShoppingCartItemModel> GetCartItemModels(ShoppingCartType cartType, Customer customer)
        {
            decimal taxRate;
            var cart = customer.GetCartItems(cartType);
            var stores = Services.StoreService.GetAllStores().ToDictionary(x => x.Id, x => x);

            var result = cart.Select(sci =>
            {
                stores.TryGetValue(sci.Item.StoreId, out var store);

                var model = new ShoppingCartItemModel
                {
                    Id = sci.Item.Id,
                    Store = store?.Name?.NaIfEmpty(),
                    ProductId = sci.Item.ProductId,
                    Quantity = sci.Item.Quantity,
                    ProductName = sci.Item.Product.GetLocalized(x => x.Name),
                    ProductTypeName = sci.Item.Product.GetProductTypeLabel(Services.Localization),
                    ProductTypeLabelHint = sci.Item.Product.ProductTypeLabelHint,
                    UnitPrice = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate)),
                    Total = _priceFormatter.FormatPrice(_taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetSubTotal(sci, true), out taxRate)),
                    UpdatedOn = _dateTimeHelper.ConvertToUserTime(sci.Item.UpdatedOnUtc, DateTimeKind.Utc)
                };

                return model;
            });

            return result.ToList();
        }
    }
}
