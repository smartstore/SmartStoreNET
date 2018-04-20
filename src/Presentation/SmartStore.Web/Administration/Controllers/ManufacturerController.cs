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

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class ManufacturerController : AdminControllerBase
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
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

		public ManufacturerController(ICategoryService categoryService, IManufacturerService manufacturerService,
            IManufacturerTemplateService manufacturerTemplateService, IProductService productService,
			IStoreService storeService,	IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService, IPictureService pictureService,
            ILanguageService languageService, ILocalizationService localizationService, ILocalizedEntityService localizedEntityService,
            IWorkContext workContext,
            ICustomerActivityService customerActivityService,
			IPermissionService permissionService,
			IDiscountService discountService,
			IDateTimeHelper dateTimeHelper,
            AdminAreaSettings adminAreaSettings,
			CatalogSettings catalogSettings)
        {
            this._categoryService = categoryService;
            this._manufacturerTemplateService = manufacturerTemplateService;
            this._manufacturerService = manufacturerService;
            this._productService = productService;
			this._storeService = storeService;
			this._storeMappingService = storeMappingService;
            this._urlRecordService = urlRecordService;
            this._pictureService = pictureService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._workContext = workContext;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
			this._discountService = discountService;
			this._dateTimeHelper = dateTimeHelper;
            this._adminAreaSettings = adminAreaSettings;
            this._catalogSettings = catalogSettings;
		}

        #endregion
        
        #region Utilities

        [NonAction]
        protected void UpdateLocales(Manufacturer manufacturer, ManufacturerModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(manufacturer,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(manufacturer,
                                                           x => x.Description,
                                                           localized.Description,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(manufacturer,
                                                           x => x.MetaKeywords,
                                                           localized.MetaKeywords,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(manufacturer,
                                                           x => x.MetaDescription,
                                                           localized.MetaDescription,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(manufacturer,
                                                           x => x.MetaTitle,
                                                           localized.MetaTitle,
                                                           localized.LanguageId);

                //search engine name
                var seName = manufacturer.ValidateSeName(localized.SeName, localized.Name, false, localized.LanguageId);
                _urlRecordService.SaveSlug(manufacturer, seName, localized.LanguageId);
            }
        }

        [NonAction]
        protected void UpdatePictureSeoNames(Manufacturer manufacturer)
        {
			var picture = _pictureService.GetPictureById(manufacturer.PictureId.GetValueOrDefault());
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(manufacturer.Name));
        }

        [NonAction]
        protected void PrepareTemplatesModel(ManufacturerModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var templates = _manufacturerTemplateService.GetAllManufacturerTemplates();
            foreach (var template in templates)
            {
                model.AvailableManufacturerTemplates.Add(new SelectListItem()
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }
        }

		[NonAction]
		private void PrepareManufacturerModel(ManufacturerModel model, Manufacturer manufacturer, bool excludeProperties)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			if (!excludeProperties)
			{
				model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(manufacturer);
				model.SelectedDiscountIds = (manufacturer != null ? manufacturer.AppliedDiscounts.Select(d => d.Id).ToArray() : new int[0]);
			}

			if (manufacturer != null)
			{
				model.CreatedOn = _dateTimeHelper.ConvertToUserTime(manufacturer.CreatedOnUtc, DateTimeKind.Utc);
				model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(manufacturer.UpdatedOnUtc, DateTimeKind.Utc);
			}

			model.GridPageSize = _adminAreaSettings.GridPageSize;
			model.AvailableStores = _storeService.GetAllStores().ToSelectListItems(model.SelectedStoreIds);
			model.AvailableDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, null, true).ToList();
		}

		#endregion

		#region List

		//ajax
		public ActionResult AllManufacturers(string label, int selectedId)
        {
            var manufacturers = _manufacturerService.GetAllManufacturers(true);

            if (label.HasValue())
            {
                manufacturers.Insert(0, new Manufacturer { Name = label, Id = 0 });
            }

            var list = from m in manufacturers
                       select new
                       {
                           id = m.Id.ToString(),
                           text = m.Name,
                           selected = m.Id == selectedId
                       };

			var mainList = list.ToList();

			var mruList = new MostRecentlyUsedList<string>(
				_workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.MostRecentlyUsedManufacturers),
				_catalogSettings.MostRecentlyUsedManufacturersMaxSize)
				.Reverse()
				.Select(x => 
				{
					var item = manufacturers.FirstOrDefault(m => m.Id.ToString() == x);
					if (item != null)
					{
						return new
						{
							id = x,
							text = item.Name,
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
					new Dictionary<string, object> { ["text"] = T("Admin.Catalog.Manufacturers").Text, ["children"] = mainList, ["main"] = true }
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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

			var model = new ManufacturerListModel
			{
				GridPageSize = _adminAreaSettings.GridPageSize
			};

			model.AvailableStores = _storeService.GetAllStores().ToSelectListItems();

			return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command, ManufacturerListModel model)
        {
			var gridModel = new GridModel<ManufacturerModel>();

			model.AvailableStores = _storeService.GetAllStores().ToSelectListItems();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var manufacturers = _manufacturerService.GetAllManufacturers(model.SearchManufacturerName, command.Page - 1, command.PageSize,
					model.SearchStoreId, true);

				gridModel.Data = manufacturers.Select(x => x.ToModel());
				gridModel.Total = manufacturers.TotalCount;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<ManufacturerModel>();

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
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var model = new ManufacturerModel();
            
			//locales
            AddLocales(_languageService, model.Locales);
            
			//templates
            PrepareTemplatesModel(model);
			PrepareManufacturerModel(model, null, false);
            
            model.Published = true;
            
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(ManufacturerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var manufacturer = model.ToEntity();

				MediaHelper.UpdatePictureTransientStateFor(manufacturer, m => m.PictureId);
                
				_manufacturerService.InsertManufacturer(manufacturer);
                
				// search engine name
                model.SeName = manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true);
                _urlRecordService.SaveSlug(manufacturer, model.SeName, 0);
                
				// locales
                UpdateLocales(manufacturer, model);

				// discounts
				var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, null, true);
				foreach (var discount in allDiscounts)
				{
					if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
						manufacturer.AppliedDiscounts.Add(discount);
				}

				var hasDiscountsApplied = manufacturer.AppliedDiscounts.Count > 0;
				if (hasDiscountsApplied)
				{
					manufacturer.HasDiscountsApplied = manufacturer.AppliedDiscounts.Count > 0;
					_manufacturerService.UpdateManufacturer(manufacturer);
				}

				// update picture seo file name
				UpdatePictureSeoNames(manufacturer);
				
				// Stores
				_storeMappingService.SaveStoreMappings<Manufacturer>(manufacturer, model.SelectedStoreIds);

                // activity log
                _customerActivityService.InsertActivity("AddNewManufacturer", _localizationService.GetResource("ActivityLog.AddNewManufacturer"), manufacturer.Name);

                NotifySuccess(_localizationService.GetResource("Admin.Catalog.Manufacturers.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = manufacturer.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            //templates
            PrepareTemplatesModel(model);
			PrepareManufacturerModel(model, null, true);

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var manufacturer = _manufacturerService.GetManufacturerById(id);
            if (manufacturer == null || manufacturer.Deleted)
                return RedirectToAction("List");

            var model = manufacturer.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = manufacturer.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = manufacturer.GetLocalized(x => x.Description, languageId, false, false);
                locale.MetaKeywords = manufacturer.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = manufacturer.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = manufacturer.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = manufacturer.GetSeName(languageId, false, false);
            });
            //templates
            PrepareTemplatesModel(model);
			PrepareManufacturerModel(model, manufacturer, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(ManufacturerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var manufacturer = _manufacturerService.GetManufacturerById(model.Id);
            if (manufacturer == null || manufacturer.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                manufacturer = model.ToEntity(manufacturer);
				MediaHelper.UpdatePictureTransientStateFor(manufacturer, m => m.PictureId);

				////TBD: is it really necessary here already?
				//manufacturer.UpdatedOnUtc = DateTime.UtcNow;
				//_manufacturerService.UpdateManufacturer(manufacturer);

				// search engine name
				model.SeName = manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true);
				_urlRecordService.SaveSlug(manufacturer, model.SeName, 0);
                
				// locales
                UpdateLocales(manufacturer, model);

				// discounts
				var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToManufacturers, null, true);
				foreach (var discount in allDiscounts)
				{
					if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
					{
						if (manufacturer.AppliedDiscounts.Where(d => d.Id == discount.Id).Count() == 0)
							manufacturer.AppliedDiscounts.Add(discount);
					}
					else
					{
						if (manufacturer.AppliedDiscounts.Where(d => d.Id == discount.Id).Count() > 0)
							manufacturer.AppliedDiscounts.Remove(discount);
					}
				}

				manufacturer.HasDiscountsApplied = manufacturer.AppliedDiscounts.Count > 0;

				// Commit now
				_manufacturerService.UpdateManufacturer(manufacturer);

				// update picture seo file name
				UpdatePictureSeoNames(manufacturer);
				
				// Stores
				_storeMappingService.SaveStoreMappings<Manufacturer>(manufacturer, model.SelectedStoreIds);

                // activity log
                _customerActivityService.InsertActivity("EditManufacturer", _localizationService.GetResource("ActivityLog.EditManufacturer"), manufacturer.Name);

                NotifySuccess(_localizationService.GetResource("Admin.Catalog.Manufacturers.Updated"));
                return continueEditing ? RedirectToAction("Edit", manufacturer.Id) : RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            //templates
            PrepareTemplatesModel(model);
			PrepareManufacturerModel(model, manufacturer, true);

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var manufacturer = _manufacturerService.GetManufacturerById(id);
            if (manufacturer == null)
                return RedirectToAction("List");

            _manufacturerService.DeleteManufacturer(manufacturer);

            //activity log
            _customerActivityService.InsertActivity("DeleteManufacturer", _localizationService.GetResource("ActivityLog.DeleteManufacturer"), manufacturer.Name);

            NotifySuccess(_localizationService.GetResource("Admin.Catalog.Manufacturers.Deleted"));
            return RedirectToAction("List");
        }
        
        #endregion

        #region Products

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductList(GridCommand command, int manufacturerId)
        {
			var model = new GridModel<ManufacturerModel.ManufacturerProductModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var productManufacturers = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId,	command.Page - 1, command.PageSize, true);

				var productIds = productManufacturers.Select(x => x.ProductId).ToArray();
				var products = _productService.GetProductsByIds(productIds);

				model.Data = productManufacturers
					.Select(x =>
					{
						var product = products.FirstOrDefault(y => y.Id == x.ProductId);

						return new ManufacturerModel.ManufacturerProductModel
						{
							Id = x.Id,
							ManufacturerId = x.ManufacturerId,
							ProductId = x.ProductId,
							ProductName = product.Name,
							Sku = product.Sku,
							ProductTypeName = product.GetProductTypeLabel(_localizationService),
							ProductTypeLabelHint = product.ProductTypeLabelHint,
							Published = product.Published,
							IsFeaturedProduct = x.IsFeaturedProduct,
							DisplayOrder1 = x.DisplayOrder
						};
					});

				model.Total = productManufacturers.TotalCount;
			}
			else
			{
				model.Data = Enumerable.Empty<ManufacturerModel.ManufacturerProductModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductUpdate(GridCommand command, ManufacturerModel.ManufacturerProductModel model)
        {
			var productManufacturer = _manufacturerService.GetProductManufacturerById(model.Id);

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
				productManufacturer.DisplayOrder = model.DisplayOrder1;

				_manufacturerService.UpdateProductManufacturer(productManufacturer);
			}

            return ProductList(command, productManufacturer.ManufacturerId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductDelete(int id, GridCommand command)
        {
			var productManufacturer = _manufacturerService.GetProductManufacturerById(id);
			var manufacturerId = productManufacturer.ManufacturerId;

			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				_manufacturerService.DeleteProductManufacturer(productManufacturer);
			}

            return ProductList(command, manufacturerId);
        }       

		[HttpPost]
		public ActionResult ProductAdd(int manufacturerId, int[] selectedProductIds)
		{
			if (_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
			{
				var products = _productService.GetProductsByIds(selectedProductIds);
				ProductManufacturer productManu = null;
				var maxDisplayOrder = -1;

				foreach (var product in products)
				{
					var existingProductManus = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId, 0, int.MaxValue, true);

					if (!existingProductManus.Any(x => x.ProductId == product.Id && x.ManufacturerId == manufacturerId))
					{
						if (maxDisplayOrder == -1 && (productManu = existingProductManus.OrderByDescending(x => x.DisplayOrder).FirstOrDefault()) != null)
						{
							maxDisplayOrder = productManu.DisplayOrder;
						}

						_manufacturerService.InsertProductManufacturer(new ProductManufacturer
						{
							ManufacturerId = manufacturerId,
							ProductId = product.Id,
							IsFeaturedProduct = false,
							DisplayOrder = ++maxDisplayOrder
						});
					}
				}
			}
			else
			{
				NotifyAccessDenied();
			}

			return new EmptyResult();
		}

		#endregion
	}
}
