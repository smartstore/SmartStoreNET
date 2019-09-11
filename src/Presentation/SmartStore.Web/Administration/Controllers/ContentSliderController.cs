using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
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
using SmartStore.Services.ContentSlider;
using SmartStore.Core.Domain.ContentSlider;
using SmartStore.Admin.Models.ContentSlider;

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
        protected void UpdatePictureSeoNames(Slide slide)
        {
            _pictureService.SetSeoFilename(slide.PictureId.GetValueOrDefault(), _pictureService.GetPictureSeName(slide.SlideTitle));
        }

        [NonAction]
        public void UpdateLocales(ContentSlider contentSlider, ContentSliderModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(contentSlider,
                                                               x => x.SliderName,
                                                               localized.SliderName,
                                                               localized.LanguageId);
            }
        }

        [NonAction]
        public void UpdateLocales(Slide slide, SliderSlideModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(slide,
                                                               x => x.SlideTitle,
                                                               localized.SlideTitle,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(slide,
                                                               x => x.SlideContent,
                                                               localized.SlideContent,
                                                               localized.LanguageId);
            }
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
                _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.MostRecentlyUsedContentSliders),
                _catalogSettings.MostRecentlyUsedContentSlidersMaxSize)
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
        public ActionResult List(GridCommand command, ContentSliderListModel model)
        {
            var gridModel = new GridModel<ContentSliderModel>();

            model.AvailableStores = _storeService.GetAllStores().ToSelectListItems();

            if (_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
            {
                var contentSliders = _contentSliderService.GetAllContentSliders(model.SearchContentSliderName, command.Page - 1, command.PageSize,
                    model.SearchStoreId, true);
                var contentslidermodels = contentSliders.Select(x => x.ToModel());
                //List<ContentSliderModel> slidersList = new List<ContentSliderModel>();

                //foreach (var slider in contentslidermodels)
                //{
                //    ContentSliderModel SliderObject = slider;
                //    SliderObject.SliderTypeName = ((SliderType)slider.SliderType).ToString();

                //    slidersList.Add(SliderObject);
                //}

                gridModel.Data = contentslidermodels.OrderBy(x => x.Id);
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
            model.SliderType = -1;
            model.Height = 500;
            model.Delay = 3000;

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
                if (model.SliderType == (int)SliderType.HomePageSlider)
                    model.ItemId = null;

                var contentSlider = model.ToEntity();

                _contentSliderService.InsertContentSlider(contentSlider);

                // locales
                UpdateLocales(contentSlider, model);

                // activity log
                _customerActivityService.InsertActivity("AddNewContentSlider", _localizationService.GetResource("ActivityLog.AddNewContentSlider"), contentSlider.SliderName);

                NotifySuccess(_localizationService.GetResource("Admin.CMS.ContentSlider.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = contentSlider.Id }) : RedirectToAction("List");
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

            //locales
            //AddLocales(_languageService, model.Locales);

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.SliderName = contentslider.GetLocalized(x => x.SliderName, languageId, false, false);
            });

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
                if (model.SliderType == (int)SliderType.HomePageSlider)
                    model.ItemId = null;

                List<Slide> SlidesList = contentslider.Slides.ToList();

                contentslider = model.ToEntity(contentslider);

                contentslider.Slides = SlidesList;

                //locales
                UpdateLocales(contentslider, model);

                // Commit now
                _contentSliderService.UpdateContentSlider(contentslider);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, contentslider, form));

                // activity log
                _customerActivityService.InsertActivity("EditContentSlider", _localizationService.GetResource("ActivityLog.EditContentSlider"), contentslider.SliderName);

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

            var contentSlider = _contentSliderService.GetContentSliders(id);
            if (contentSlider == null)
                return RedirectToAction("List");

            _contentSliderService.DeleteContentSlider(contentSlider);

            //activity log
            _customerActivityService.InsertActivity("DeleteContentSlider", _localizationService.GetResource("ActivityLog.DeleteContentSlider"), contentSlider.SliderName);

            NotifySuccess(_localizationService.GetResource("Admin.CMS.ContentSlider.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Slides

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult SlideList(GridCommand command, int sliderId)
        {
            var model = new GridModel<SliderSlideModel>();

            if (_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
            {
                var contentsliderSlides = _contentSliderService.GetSlidesContentSliderBySliderId(sliderId, command.Page - 1, command.PageSize, true);

                model.Data = contentsliderSlides
                    .Select(x =>
                    {
                        return new SliderSlideModel
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
                            DisplayOrder = x.DisplayOrder,
                            //SlideTypeName = ((SlideType)x.SlideType).ToString(),
                            SlideType = x.SlideType
                        };
                    });

                model.Total = contentsliderSlides.TotalCount;
            }
            else
            {
                model.Data = Enumerable.Empty<SliderSlideModel>();

                NotifyAccessDenied();
            }

            return new JsonResult
            {
                Data = model
            };
        }


        public ActionResult SlideCreate(int SliderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            var model = new SliderSlideModel();
            model.SliderId = SliderId;
            model.SlideType = -1; ;

            //locales
            AddLocales(_languageService, model.Locales);

            model.IsActive = true;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult SlideCreate(SliderSlideModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                if (model.SlideType == (int)SlideType.NormalSlide)
                {
                    model.ItemId = null;
                    model.DisplayPrice = false;
                    model.DisplayButton = false;
                }
                else if (model.SlideType == (int)SlideType.CategorySlide)
                {
                    model.DisplayPrice = false;
                    model.DisplayButton = false;
                }
                else if (model.SlideType == (int)SlideType.ManufacturerSlide)
                {
                    model.DisplayPrice = false;
                    model.DisplayButton = false;
                }

                var slide = model.ToEntity();

                _contentSliderService.InsertContentSliderSlide(slide);

                // locales
                UpdateLocales(slide, model);

                UpdatePictureSeoNames(slide);

                // activity log
                _customerActivityService.InsertActivity("AddNewSliderSlide", _localizationService.GetResource("ActivityLog.AddNewSliderSlide"), slide.SliderId);

                NotifySuccess(_localizationService.GetResource("Admin.CMS.ContentSlider.Slide.Added"));
                return continueEditing ? RedirectToAction("SlideEdit", new { id = slide.Id }) : RedirectToAction("Edit", new { id = slide.SliderId });
            }

            //If we got this far, something failed, redisplay form

            return View(model);
        }

        public ActionResult SlideEdit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            var slide = _contentSliderService.GetContentSliderSlideById(id);
            if (slide == null)
                return RedirectToAction("Edit", new { id = slide.SliderId });

            var model = slide.ToModel();

            //locales
            //AddLocales(_languageService, model.Locales);

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.SlideTitle = slide.GetLocalized(x => x.SlideTitle, languageId, false, false);
                locale.SlideContent = slide.GetLocalized(x => x.SlideContent, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateInput(false)]
        public ActionResult SlideEdit(SliderSlideModel model, bool continueEditing, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            var slide = _contentSliderService.GetContentSliderSlideById(model.Id);
            if (slide == null)
                return RedirectToAction("Edit", new { id = slide.SliderId });

            if (ModelState.IsValid)
            {
                if (model.SlideType == (int)SlideType.NormalSlide)
                {
                    model.ItemId = null;
                    model.DisplayPrice = false;
                    model.DisplayButton = false;
                }
                else if (model.SlideType == (int)SlideType.CategorySlide)
                {
                    model.DisplayPrice = false;
                    model.DisplayButton = false;
                }
                else if (model.SlideType == (int)SlideType.ManufacturerSlide)
                {
                    model.DisplayPrice = false;
                    model.DisplayButton = false;
                }

                slide = model.ToEntity(slide);

                //locales
                UpdateLocales(slide, model);

                // Commit now
                _contentSliderService.UpdateContentSliderSlide(slide);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, slide, form));

                // activity log
                _customerActivityService.InsertActivity("EditSlide", _localizationService.GetResource("ActivityLog.EditSlide"), slide.SliderId);

                NotifySuccess(_localizationService.GetResource("Admin.CMS.ContentSlider.Slide.Updated"));
                return continueEditing ? RedirectToAction("SlideEdit", new { id = slide.Id }) : RedirectToAction("Edit", new { id = slide.SliderId });
            }

            //If we got this far, something failed, redisplay form

            return View(model);
        }

        [HttpGet]
        public ActionResult SlideDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageContentSlider))
                return AccessDeniedView();

            var slide = _contentSliderService.GetContentSliderSlideById(id);
            if (slide == null)
                return RedirectToAction("List");

            _contentSliderService.DeleteContentSliderSlide(slide);

            //activity log
            _customerActivityService.InsertActivity("DeleteSlide", _localizationService.GetResource("ActivityLog.DeleteSlide"), slide.SliderId);

            NotifySuccess(_localizationService.GetResource("Admin.CMS.ContentSlider.Slide.Deleted"));
            return RedirectToAction("Edit", new { id = slide.SliderId });
        }

        #endregion
    }
}
