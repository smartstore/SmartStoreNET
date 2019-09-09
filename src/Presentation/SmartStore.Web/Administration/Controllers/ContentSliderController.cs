using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Discounts;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Core.Events;
using SmartStore.Services.ContentSlider;
using SmartStore.Core.Domain.ContentSlider;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ContentSliderController : AdminControllerBase
    {
        #region Fields

        private readonly IContentSliderService _contentSliderService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly IDiscountService _discountService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Constructors

        public ContentSliderController(
            IContentSliderService contentSliderService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IManufacturerTemplateService manufacturerTemplateService,
            IProductService productService,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IWorkContext workContext,
            ICustomerActivityService customerActivityService,
            IPermissionService permissionService,
            IDiscountService discountService,
            IDateTimeHelper dateTimeHelper,
            AdminAreaSettings adminAreaSettings,
            CatalogSettings catalogSettings)
        {
            _contentSliderService = contentSliderService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _productService = productService;
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _workContext = workContext;
            _customerActivityService = customerActivityService;
            _permissionService = permissionService;
            _discountService = discountService;
            _dateTimeHelper = dateTimeHelper;
            _adminAreaSettings = adminAreaSettings;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected void UpdatePictureSeoNames(Manufacturer manufacturer)
        {
            _pictureService.SetSeoFilename(manufacturer.PictureId.GetValueOrDefault(), _pictureService.GetPictureSeName(manufacturer.Name));
        }

        [NonAction]
        private void PrepareContentSliderModel(ContentSliderModel model, ContentSlider slider, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //if (!excludeProperties)
            //{
            //    model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(manufacturer);
            //    model.SelectedDiscountIds = (manufacturer != null ? manufacturer.AppliedDiscounts.Select(d => d.Id).ToArray() : new int[0]);
            //}

            //if (manufacturer != null)
            //{
            //    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(manufacturer.CreatedOnUtc, DateTimeKind.Utc);
            //    model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(manufacturer.UpdatedOnUtc, DateTimeKind.Utc);
            //}

            //model.GridPageSize = _adminAreaSettings.GridPageSize;
            //model.AvailableStores = _storeService.GetAllStores().ToSelectListItems(model.SelectedStoreIds);
            //model.AvailableDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, null, true).ToList();
        }

        #endregion

        #region List

        // AJAX
        public ActionResult AllContentSliders(string label, int selectedId)
        {
            var contentsliders = _contentSliderService.GetAllContentSliders();

            if (label.HasValue())
            {
                contentsliders.Insert(0, new ContentSlider { SliderName = label, Id = 0 });
            }

            var list = from m in contentsliders
                       select new
                       {
                           id = m.Id.ToString(),
                           text = m.SliderName,
                           selected = m.Id == selectedId
                       };

            var mainList = list.ToList();

            var mruList = new TrimmedBuffer<string>(
                _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.MostRecentlyUsedManufacturers),
                _catalogSettings.MostRecentlyUsedManufacturersMaxSize)
                .Reverse()
                .Select(x =>
                {
                    var item = contentsliders.FirstOrDefault(m => m.Id.ToString() == x);
                    if (item != null)
                    {
                        return new
                        {
                            id = x,
                            text = item.SliderName,
                            selected = false
                        };
                    }

                    return null;
                })
                .Where(x => x != null)
                .ToList();

            object data = mainList;
            if (mruList.Count > 0)
            {
                data = new List<object>
                {
                    new Dictionary<string, object> { ["text"] = T("Common.Mru").Text, ["children"] = mruList },
                    new Dictionary<string, object> { ["text"] = T("Admin.CMS.ContentSlider").Text, ["children"] = mainList, ["main"] = true }
                };
            }

            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            var model = new ContentSliderListModel
            {
                GridPageSize = _adminAreaSettings.GridPageSize
            };

            model.AvailableStores = _storeService.GetAllStores().ToSelectListItems();

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command, ManufacturerListModel model)
        {
            var gridModel = new GridModel<ContentSliderModel>();

            model.AvailableStores = _storeService.GetAllStores().ToSelectListItems();

            if (_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
            {
                var contentSliders = _contentSliderService.GetAllContentSliders(model.SearchManufacturerName, command.Page - 1, command.PageSize,
                    model.SearchStoreId, true);
                var contentslidermodels = contentSliders.Select(x => x.ToModel());
                List<ContentSliderModel> slidersList = new List<ContentSliderModel>();

                foreach (var slider in contentslidermodels)
                {
                    ContentSliderModel SliderObject = slider;
                    SliderObject.SliderTypeName = ((SliderType)slider.SliderType).ToString();

                    slidersList.Add(SliderObject);
                }

                gridModel.Data = slidersList.OrderBy(x => x.Id);
                gridModel.Total = contentSliders.TotalCount;
            }
            else
            {
                gridModel.Data = Enumerable.Empty<ContentSliderModel>();

                NotifyAccessDenied();
            }

            return new JsonResult
            {
                Data = gridModel
            };
        }

        #endregion

        #region Create / Edit / Delete

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            var model = new ContentSliderModel();

            //locales
            AddLocales(_languageService, model.Locales);

            model.IsActive = true;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(ContentSliderModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var manufacturer = model.ToEntity();

                //MediaHelper.UpdatePictureTransientStateFor(manufacturer, m => m.PictureId);

                //_manufacturerService.InsertManufacturer(manufacturer);

                //// search engine name
                //model.SeName = manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true);
                //_urlRecordService.SaveSlug(manufacturer, model.SeName, 0);

                //// locales
                //UpdateLocales(manufacturer, model);

                //// discounts
                //var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, null, true);
                //foreach (var discount in allDiscounts)
                //{
                //    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                //        manufacturer.AppliedDiscounts.Add(discount);
                //}

                //var hasDiscountsApplied = manufacturer.AppliedDiscounts.Count > 0;
                //if (hasDiscountsApplied)
                //{
                //    manufacturer.HasDiscountsApplied = manufacturer.AppliedDiscounts.Count > 0;
                //    _manufacturerService.UpdateManufacturer(manufacturer);
                //}

                //// update picture seo file name
                //UpdatePictureSeoNames(manufacturer);

                //// Stores
                //SaveStoreMappings(manufacturer, model);

                //// activity log
                //_customerActivityService.InsertActivity("AddNewManufacturer", _localizationService.GetResource("ActivityLog.AddNewManufacturer"), manufacturer.Name);

                NotifySuccess(_localizationService.GetResource("Admin.CMS.ContentSlider.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = manufacturer.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            var contentslider = _contentSliderService.GetContentSliders(id);
            if (contentslider == null)
                return RedirectToAction("List");

            var model = contentslider.ToModel();

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateInput(false)]
        public ActionResult Edit(ContentSliderModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            var contentslider = _contentSliderService.GetContentSliders(model.Id);
            if (contentslider == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                contentslider = model.ToEntity(contentslider);
                //MediaHelper.UpdatePictureTransientStateFor(contentslider, m => m.PictureId);

                // search engine name
                //model.SeName = contentslider.ValidateSeName(model.SeName, contentslider.SliderName, true);
                //_urlRecordService.SaveSlug(contentslider, model.SeName, 0);

                // locales
                //UpdateLocales(contentslider, model);

                // discounts
                //var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, null, true);
                
                //contentslider.HasDiscountsApplied = contentslider.AppliedDiscounts.Count > 0;
                //contentslider.UpdatedOnUtc = DateTime.UtcNow;

                // Commit now
                _contentSliderService.UpdateContentSlider(contentslider);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, contentslider, form));

                // update picture seo file name
                //UpdatePictureSeoNames(contentslider);

                // Stores
                //SaveStoreMappings(contentslider, model);

                // activity log
                _customerActivityService.InsertActivity("EditManufacturer", _localizationService.GetResource("ActivityLog.EditManufacturer"), contentslider.SliderName);

                NotifySuccess(_localizationService.GetResource("Admin.CMS.ContentSlider.Updated"));
                return continueEditing ? RedirectToAction("Edit", contentslider.Id) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            var manufacturer = _manufacturerService.GetManufacturerById(id);
            if (manufacturer == null)
                return RedirectToAction("List");

            _manufacturerService.DeleteManufacturer(manufacturer);

            //activity log
            _customerActivityService.InsertActivity("DeleteManufacturer", _localizationService.GetResource("ActivityLog.DeleteManufacturer"), manufacturer.Name);

            NotifySuccess(_localizationService.GetResource("Admin.CMS.ContentSlider.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Slides

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult SlideList(GridCommand command, int sliderId)
        {
            var model = new GridModel<ContentSliderModel.SliderSlidModel>();

            if (_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
            {
                var contentsliderSlides = _contentSliderService.GetSlidesContentSliderBySliderId(sliderId, command.Page - 1, command.PageSize, true);

                model.Data = contentsliderSlides
                    .Select(x =>
                    {
                        return new ContentSliderModel.SliderSlidModel
                        {
                            Id = x.Id,
                            SlideId = x.Id,
                            SliderId = x.SliderId,
                            DisplayButton = x.DisplayButton,
                            DisplayPrice = x.DisplayPrice,
                            IsActive = x.IsActive,
                            ItemId = x.ItemId,
                            SlideContent = x.SlideContent,
                            SlideTitle = x.SlideTitle,
                            DisplayOrder1 = x.DisplayOrder,
                            SlideTypeName = ((SlideType)x.SlideType).ToString(),
                            SlideType = x.SlideType
                        };
                    });

                model.Total = contentsliderSlides.TotalCount;
            }
            else
            {
                model.Data = Enumerable.Empty<ContentSliderModel.SliderSlidModel>();

                NotifyAccessDenied();
            }

            return new JsonResult
            {
                Data = model
            };
        }

        //[GridAction(EnableCustomBinding = true)]
        //public ActionResult ProductUpdate(GridCommand command, ContentSliderModel.ManufacturerProductModel model)
        //{
        //    var productManufacturer = _manufacturerService.GetProductManufacturerById(model.Id);

        //    if (_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
        //    {
        //        productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
        //        productManufacturer.DisplayOrder = model.DisplayOrder1;

        //        _manufacturerService.UpdateProductManufacturer(productManufacturer);
        //    }

        //    return ProductList(command, productManufacturer.ManufacturerId);
        //}

        //[GridAction(EnableCustomBinding = true)]
        //public ActionResult ProductDelete(int id, GridCommand command)
        //{
        //    var productManufacturer = _manufacturerService.GetProductManufacturerById(id);
        //    var manufacturerId = productManufacturer.ManufacturerId;

        //    if (_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
        //    {
        //        _manufacturerService.DeleteProductManufacturer(productManufacturer);
        //    }

        //    return ProductList(command, manufacturerId);
        //}

        //[HttpPost]
        //public ActionResult ProductAdd(int manufacturerId, int[] selectedProductIds)
        //{
        //    if (_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
        //    {
        //        var products = _productService.GetProductsByIds(selectedProductIds);
        //        ProductManufacturer productManu = null;
        //        var maxDisplayOrder = -1;

        //        foreach (var product in products)
        //        {
        //            var existingProductManus = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId, 0, int.MaxValue, true);

        //            if (!existingProductManus.Any(x => x.ProductId == product.Id && x.ManufacturerId == manufacturerId))
        //            {
        //                if (maxDisplayOrder == -1 && (productManu = existingProductManus.OrderByDescending(x => x.DisplayOrder).FirstOrDefault()) != null)
        //                {
        //                    maxDisplayOrder = productManu.DisplayOrder;
        //                }

        //                _manufacturerService.InsertProductManufacturer(new ProductManufacturer
        //                {
        //                    ManufacturerId = manufacturerId,
        //                    ProductId = product.Id,
        //                    IsFeaturedProduct = false,
        //                    DisplayOrder = ++maxDisplayOrder
        //                });
        //            }
        //        }
        //    }
        //    else
        //    {
        //        NotifyAccessDenied();
        //    }

        //    return new EmptyResult();
        //}

        #endregion
    }
}
