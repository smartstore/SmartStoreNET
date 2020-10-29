using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Html;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class ProductReviewController : AdminControllerBase
    {
        private readonly ICustomerContentService _customerContentService;
        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;

        public ProductReviewController(
            ICustomerContentService customerContentService,
            IProductService productService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            ICustomerService customerService)
        {
            _customerContentService = customerContentService;
            _productService = productService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _customerService = customerService;
        }

        [NonAction]
        private void PrepareProductReviewModel(ProductReviewModel model, ProductReview productReview, bool excludeProperties, bool formatReviewText)
        {
            Guard.NotNull(model, nameof(model));
            Guard.NotNull(productReview, nameof(productReview));

            model.Id = productReview.Id;
            model.ProductId = productReview.ProductId;
            model.ProductName = productReview.Product.Name;
            model.ProductTypeName = productReview.Product.GetProductTypeLabel(_localizationService);
            model.ProductTypeLabelHint = productReview.Product.ProductTypeLabelHint;
            model.CustomerId = productReview.CustomerId;
            model.CustomerName = productReview.Customer.GetDisplayName(T);
            model.IpAddress = productReview.IpAddress;
            model.Rating = productReview.Rating;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(productReview.CreatedOnUtc, DateTimeKind.Utc);

            if (!excludeProperties)
            {
                model.Title = productReview.Title;
                model.IsApproved = productReview.IsApproved;

                model.ReviewText = formatReviewText
                    ? HtmlUtils.ConvertPlainTextToHtml(productReview.ReviewText.HtmlEncode())
                    : productReview.ReviewText;
            }
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Catalog.ProductReview.Read)]
        public ActionResult List()
        {
            var model = new ProductReviewListModel();
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.ProductReview.Read)]
        public ActionResult List(GridCommand command, ProductReviewListModel model)
        {
            DateTime? createdFrom = (model.CreatedOnFrom == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? createdTo = (model.CreatedOnTo == null) ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var productReviews = _customerContentService.GetAllCustomerContent<ProductReview>(
                0,
                null,
                createdFrom,
                createdTo,
                command.Page - 1,
                command.PageSize);

            var gridModel = new GridModel<ProductReviewModel>
            {
                Total = productReviews.TotalCount
            };

            gridModel.Data = productReviews.Select(x =>
            {
                var m = new ProductReviewModel();
                PrepareProductReviewModel(m, x, false, true);
                return m;
            });

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Catalog.ProductReview.Read)]
        public ActionResult Edit(int id)
        {
            var productReview = _customerContentService.GetCustomerContentById(id) as ProductReview;
            if (productReview == null)
            {
                return RedirectToAction("List");
            }

            var model = new ProductReviewModel();
            PrepareProductReviewModel(model, productReview, false, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.ProductReview.Update)]
        public ActionResult Edit(ProductReviewModel model, bool continueEditing)
        {
            var productReview = _customerContentService.GetCustomerContentById(model.Id) as ProductReview;
            if (productReview == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                productReview.Title = model.Title;
                productReview.ReviewText = model.ReviewText;
                productReview.IsApproved = model.IsApproved;
                _customerContentService.UpdateCustomerContent(productReview);

                _productService.UpdateProductReviewTotals(productReview.Product);

                _customerService.RewardPointsForProductReview(productReview.Customer, productReview.Product, productReview.IsApproved);

                NotifySuccess(T("Admin.Catalog.ProductReviews.Updated"));
                return continueEditing ? RedirectToAction("Edit", productReview.Id) : RedirectToAction("List");
            }

            PrepareProductReviewModel(model, productReview, true, false);
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.ProductReview.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var productReview = _customerContentService.GetCustomerContentById(id) as ProductReview;
            if (productReview == null)
            {
                return RedirectToAction("List");
            }

            var product = productReview.Product;
            _customerContentService.DeleteCustomerContent(productReview);

            _productService.UpdateProductReviewTotals(product);

            NotifySuccess(T("Admin.Catalog.ProductReviews.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.ProductReview.Approve)]
        public ActionResult ApproveSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    var productReview = _customerContentService.GetCustomerContentById(id) as ProductReview;
                    if (productReview != null)
                    {
                        productReview.IsApproved = true;
                        _customerContentService.UpdateCustomerContent(productReview);

                        _productService.UpdateProductReviewTotals(productReview.Product);

                        _customerService.RewardPointsForProductReview(productReview.Customer, productReview.Product, true);
                    }
                }
            }

            NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.ProductReview.Delete)]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    var productReview = _customerContentService.GetCustomerContentById(id) as ProductReview;
                    if (productReview != null)
                    {
                        var product = productReview.Product;
                        _customerContentService.DeleteCustomerContent(productReview);

                        _productService.UpdateProductReviewTotals(product);
                    }
                }
            }

            NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.ProductReview.Approve)]
        public ActionResult DisapproveSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                foreach (var id in selectedIds)
                {
                    var productReview = _customerContentService.GetCustomerContentById(id) as ProductReview;
                    if (productReview != null)
                    {
                        productReview.IsApproved = false;
                        _customerContentService.UpdateCustomerContent(productReview);

                        _productService.UpdateProductReviewTotals(productReview.Product);

                        _customerService.RewardPointsForProductReview(productReview.Customer, productReview.Product, false);
                    }
                }
            }

            NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

            return Json(new { success = true });
        }
    }
}
