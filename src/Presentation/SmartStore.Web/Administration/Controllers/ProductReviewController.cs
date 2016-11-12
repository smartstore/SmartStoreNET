using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public class ProductReviewController : AdminControllerBase
    {
        #region Fields

        private readonly ICustomerContentService _customerContentService;
        private readonly IProductService _productService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
		private readonly ICustomerService _customerService;

        #endregion Fields

        #region Constructors

        public ProductReviewController(ICustomerContentService customerContentService,
            IProductService productService, IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService, IPermissionService permissionService,
			ICustomerService customerService)
        {
            this._customerContentService = customerContentService;
            this._productService = productService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
			this._customerService = customerService;
        }

        #endregion

        #region Utilities

        [NonAction]
        private void PrepareProductReviewModel(ProductReviewModel model,
            ProductReview productReview, bool excludeProperties, bool formatReviewText)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (productReview == null)
                throw new ArgumentNullException("productReview");

            model.Id = productReview.Id;
            model.ProductId = productReview.ProductId;
            model.ProductName = productReview.Product.Name;
			model.ProductTypeName = productReview.Product.GetProductTypeLabel(_localizationService);
			model.ProductTypeLabelHint = productReview.Product.ProductTypeLabelHint;
            model.CustomerId = productReview.CustomerId;
			model.CustomerName = productReview.Customer.GetFullName();
            model.IpAddress = productReview.IpAddress;
            model.Rating = productReview.Rating;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(productReview.CreatedOnUtc, DateTimeKind.Utc);

			if (string.IsNullOrWhiteSpace(model.CustomerName) && !productReview.Customer.IsRegistered())
			{
				model.CustomerName = _localizationService.GetResource("Admin.Customers.Guest");
			}

            if (!excludeProperties)
            {
                model.Title = productReview.Title;
                if (formatReviewText)
                    model.ReviewText = Core.Html.HtmlUtils.FormatText(productReview.ReviewText, false, true, false, false, false, false);
                else
                    model.ReviewText = productReview.ReviewText;
                model.IsApproved = productReview.IsApproved;
            }
        }

        #endregion

        #region Methods

        //list
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var model = new ProductReviewListModel();
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command, ProductReviewListModel model)
        {
			var gridModel = new GridModel<ProductReviewModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				DateTime? createdOnFromValue = (model.CreatedOnFrom == null) ? null
					: (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);

				DateTime? createdToFromValue = (model.CreatedOnTo == null) ? null
					: (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

				var productReviews = _customerContentService.GetAllCustomerContent<ProductReview>(0, null, createdOnFromValue, createdToFromValue);

				gridModel.Data = productReviews.PagedForCommand(command).Select(x =>
				{
					var m = new ProductReviewModel();
					PrepareProductReviewModel(m, x, false, true);
					return m;
				});

				gridModel.Total = productReviews.Count;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<ProductReviewModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productReview = _customerContentService.GetCustomerContentById(id) as ProductReview;
            if (productReview == null)
                return RedirectToAction("List");

            var model = new ProductReviewModel();
            PrepareProductReviewModel(model, productReview, false, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(ProductReviewModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productReview = _customerContentService.GetCustomerContentById(model.Id) as ProductReview;
            if (productReview == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                productReview.Title = model.Title;
                productReview.ReviewText = model.ReviewText;
                productReview.IsApproved = model.IsApproved;
                _customerContentService.UpdateCustomerContent(productReview);
                
                //update product totals
                _productService.UpdateProductReviewTotals(productReview.Product);

				_customerService.RewardPointsForProductReview(productReview.Customer, productReview.Product, productReview.IsApproved);

                NotifySuccess(_localizationService.GetResource("Admin.Catalog.ProductReviews.Updated"));
                return continueEditing ? RedirectToAction("Edit", productReview.Id) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            PrepareProductReviewModel(model, productReview, true, false);
            return View(model);
        }
        
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productReview = _customerContentService.GetCustomerContentById(id) as ProductReview;
            if (productReview == null)
                return RedirectToAction("List");

            var product = productReview.Product;
            _customerContentService.DeleteCustomerContent(productReview);

            //update product totals
            _productService.UpdateProductReviewTotals(product);

            NotifySuccess(_localizationService.GetResource("Admin.Catalog.ProductReviews.Deleted"));
            return RedirectToAction("List");
        }
        
        [HttpPost]
        public ActionResult ApproveSelected(ICollection<int> selectedIds)
        {
			var result = true;

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
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

							//update product totals
							_productService.UpdateProductReviewTotals(productReview.Product);

							_customerService.RewardPointsForProductReview(productReview.Customer, productReview.Product, true);
						}
					}
				}

				NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
			}
			else
			{
				result = false;
				NotifyAccessDenied();
			}

            return Json(new { Result = result });
        }

        [HttpPost]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
			var result = true;

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
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
							//update product totals
							_productService.UpdateProductReviewTotals(product);
						}
					}
				}

				NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
			}
			else
			{
				result = false;
				NotifyAccessDenied();
			}

			return Json(new { Result = result });
		}

        [HttpPost]
        public ActionResult DisapproveSelected(ICollection<int> selectedIds)
        {
			var result = true;

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
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

							//update product totals
							_productService.UpdateProductReviewTotals(productReview.Product);

							_customerService.RewardPointsForProductReview(productReview.Customer, productReview.Product, false);
						}
					}
				}

				NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
			}
			else
			{
				result = false;
				NotifyAccessDenied();
			}

			return Json(new { Result = result });
		}

        #endregion
    }
}
