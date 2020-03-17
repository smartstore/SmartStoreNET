using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Store;
using SmartStore.Admin.Models.Stores;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class StoreController : AdminControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        //private readonly IMediaService _mediaService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IShippingService _shippingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderReportService _orderReportService;

        public StoreController(
            ICurrencyService currencyService,
            IProductService productService,
            IProductAttributeService productAttributeService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            ICustomerService customerService,
            IOrderService orderService,
            //IMediaService mediaService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IShoppingCartService shoppingCartService,
            IShippingService shippingService,
            IPaymentService paymentService,
            IOrderReportService orderReportService)
        {
            _currencyService = currencyService;
            _productService = productService;
            _productAttributeService = productAttributeService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _customerService = customerService;
            _categoryService = categoryService;
            _orderService = orderService;
            //_mediaService = mediaService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _shoppingCartService = shoppingCartService;
            _shippingService = shippingService;
            _paymentService = paymentService;
            _orderReportService = orderReportService;
        }

        private void PrepareStoreModel(StoreModel model, Store store)
        {
            model.AvailableCurrencies = _currencyService.GetAllCurrencies(false, store == null ? 0 : store.Id)
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
                .ToList();
        }

        // Ajax.
        public ActionResult AllStores(string label, string selectedIds)
        {
            var stores = Services.StoreService.GetAllStores();
            var ids = selectedIds.ToIntArray();

            if (label.HasValue())
            {
                stores.Insert(0, new Store { Name = label, Id = 0 });
            }

            var list =
                from m in stores
                select new
                {
                    id = m.Id.ToString(),
                    text = m.Name,
                    selected = ids.Contains(m.Id)
                };

            return new JsonResult { Data = list.ToList(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        #region list

        [Permission(Permissions.Configuration.Store.Read)]
        public ActionResult List()
        {
            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Store.Read)]
        public ActionResult List(GridCommand command)
        {
            var gridModel = new GridModel<StoreModel>();

            var storeModels = Services.StoreService.GetAllStores()
                .Select(x =>
                {
                    var model = x.ToModel();

                    PrepareStoreModel(model, x);

                    model.Hosts = model.Hosts.EmptyNull().Replace(",", "<br />");

                    return model;
                })
                .ToList();

            gridModel.Data = storeModels;
            gridModel.Total = storeModels.Count();

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Configuration.Store.Create)]
        public ActionResult Create()
        {
            var model = new StoreModel();
            PrepareStoreModel(model, null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Configuration.Store.Create)]
        public ActionResult Create(StoreModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var store = model.ToEntity();

                // Ensure we have "/" at the end.
                store.Url = store.Url.EnsureEndsWith("/");
                Services.StoreService.InsertStore(store);

                NotifySuccess(T("Admin.Configuration.Stores.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
            }

            PrepareStoreModel(model, null);
            return View(model);
        }

        [Permission(Permissions.Configuration.Store.Read)]
        public ActionResult Edit(int id)
        {
            var store = Services.StoreService.GetStoreById(id);
            if (store == null)
            {
                return RedirectToAction("List");
            }

            var model = store.ToModel();
            PrepareStoreModel(model, store);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [Permission(Permissions.Configuration.Store.Update)]
        public ActionResult Edit(StoreModel model, bool continueEditing)
        {
            var store = Services.StoreService.GetStoreById(model.Id);
            if (store == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                store = model.ToEntity(store);

                // Ensure we have "/" at the end.
                store.Url = store.Url.EnsureEndsWith("/");
                Services.StoreService.UpdateStore(store);

                NotifySuccess(T("Admin.Configuration.Stores.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
            }

            PrepareStoreModel(model, store);
            return View(model);
        }

        [HttpPost]
        [Permission(Permissions.Configuration.Store.Delete)]
        public ActionResult Delete(int id)
        {
            var store = Services.StoreService.GetStoreById(id);
            if (store == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                Services.StoreService.DeleteStore(store);

                // When we delete a store we should also ensure that all "per store" settings will also be deleted.
                var settingsToDelete = Services.Settings
                    .GetAllSettings()
                    .Where(s => s.StoreId == id)
                    .ToList();

                settingsToDelete.ForEach(x => Services.Settings.DeleteSetting(x));

                // When we had two stores and now have only one store, we also should delete all "per store" settings.
                var allStores = Services.StoreService.GetAllStores();

                if (allStores.Count == 1)
                {
                    settingsToDelete = Services.Settings
                        .GetAllSettings()
                        .Where(s => s.StoreId == allStores[0].Id)
                        .ToList();

                    settingsToDelete.ForEach(x => Services.Settings.DeleteSetting(x));
                }

                NotifySuccess(T("Admin.Configuration.Stores.Deleted"));
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            return RedirectToAction("Edit", new { id = store.Id });
        }

        #endregion

        [Permission(Permissions.Customer.Read, false)]
        public ActionResult StoreDashboardReport()
        {
            var model = new StoreDashboardReportModel();

            model.StoreStatisticsReport.Add(
                T("Admin.Catalog.Products") + ":",
                string.Format("{0:#,##0}", _productService.CountAllProducts()));

            model.StoreStatisticsReport.Add(
                T("Admin.Catalog.Products.Pictures") + ":",
                string.Format("{0:#,##0}", _pictureService.GetPictures(0, int.MaxValue).TotalCount));

            model.StoreStatisticsReport.Add(
                T("Admin.Catalog.Categories") + ":",
                string.Format("{0:#,##0}", _categoryService.GetAllCategories().Count));

            model.StoreStatisticsReport.Add(
                T("Admin.Catalog.Manufacturers") + ":",
                string.Format("{0:#,##0}", _manufacturerService.GetAllManufacturers().Count));

            model.StoreStatisticsReport.Add(
                T("Admin.Catalog.Attributes") + ":",
                string.Format("{0:#,##0}", _productAttributeService.GetAllProductAttributes(0, int.MaxValue).Count));

            model.StoreStatisticsReport.Add(
                T("Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations") + ":",
                string.Format("{0:#,##0}", _productService.CountAllProductVariants()));

            model.StoreStatisticsReport.Add(
                T("Account.CustomerOrders") + ":",
                string.Format("{0:#,##0}", _orderService.GetAllOrders(0, 0, int.MaxValue).Count));

            model.StoreStatisticsReport.Add(
                T("Admin.Sales") + ":",
                _priceFormatter.FormatPrice(_orderService.GetAllOrders(0, 0, int.MaxValue).Sum(x => x.OrderTotal), true, false));

            model.StoreStatisticsReport.Add(
                T("Admin.Customers.OnlineCustomers") + ":",
                string.Format("{0:#,##0}", _customerService.GetOnlineCustomers(DateTime.UtcNow.AddMinutes(-15), null, 0, int.MaxValue).Count));

            model.StoreStatisticsReport.Add(
                T("Admin.Customers.Customers") + ":",
                string.Format("{0:#,##0}", _customerService.CountAllCustomers()));

            model.StoreStatisticsReport.Add(
                T("Admin.Promotions.NewsLetterSubscriptions.Short") + ":",
                string.Format("{0:#,##0}", _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions("", 0, int.MaxValue).Count));

            model.StoreStatisticsReport.Add(
               T("Admin.CurrentCarts") + ":",
               _priceFormatter.FormatPrice(_shoppingCartService.GetAllOpenCartsSubTotal(), true, false));
            
            model.StoreStatisticsReport.Add(
                T("Admin.Configuration.Shipping.Methods") + ":",
                string.Format("{0:#,##0}", _shippingService.GetAllShippingMethods().Count));

            model.StoreStatisticsReport.Add(
               T("Admin.CurrentWishlists") + ":",
               _priceFormatter.FormatPrice(_shoppingCartService.GetAllOpenWishlistsSubTotal(), true, false));

            model.StoreStatisticsReport.Add(
                T("Admin.Configuration.Payment.Methods") + ":",
                string.Format("{0:#,##0}", _paymentService.GetAllPaymentMethods().Count));

            model.StoreStatisticsReport.Add(
                T("Admin.SalesReport.NeverSold") + ":",
                string.Format("{0:#,##0} " + T("Admin.Catalog.Products"), _orderReportService.ProductsNeverSold(null, null, 0, int.MaxValue).Count));

            return PartialView(model);
        }
    }
}
