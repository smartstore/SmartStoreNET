using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.ExportImport;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Mvc;
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
        private readonly IExportManager _exportManager;
        private readonly IWorkContext _workContext;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
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
            IExportManager exportManager, IWorkContext workContext,
            ICustomerActivityService customerActivityService, IPermissionService permissionService,
			IDateTimeHelper dateTimeHelper,
            AdminAreaSettings adminAreaSettings, CatalogSettings catalogSettings)
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
            this._exportManager = exportManager;
            this._workContext = workContext;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
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

			model.AvailableStores = _storeService
				.GetAllStores()
				.Select(s => s.ToModel())
				.ToList();

			if (!excludeProperties)
			{
				if (manufacturer != null)
				{
					model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(manufacturer);
				}
				else
				{
					model.SelectedStoreIds = new int[0];
				}
			}

			if (manufacturer != null)
			{
				model.CreatedOn = _dateTimeHelper.ConvertToUserTime(manufacturer.CreatedOnUtc, DateTimeKind.Utc);
				model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(manufacturer.UpdatedOnUtc, DateTimeKind.Utc);
			}
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

			var data = list.ToList();

			var mru = new MostRecentlyUsedList<string>(_workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.MostRecentlyUsedManufacturers),
				_catalogSettings.MostRecentlyUsedManufacturersMaxSize);

			// TODO: insert disabled option separator (select2 v.3.4.2 or higher required)
			//if (mru.Count > 0)
			//{
			//	data.Insert(0, new
			//	{
			//		id = "",
			//		text = "----------------------",
			//		selected = false,
			//		disabled = true
			//	});
			//}

			for (int i = mru.Count - 1; i >= 0; --i)
			{
				string id = mru[i];
				var item = manufacturers.FirstOrDefault(x => x.Id.ToString() == id);
				if (item != null)
				{
					data.Insert(0, new
					{
						id = id,
						text = item.Name,
						selected = false
					});
				}
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

            var model = new ManufacturerListModel();
            var manufacturers = _manufacturerService.GetAllManufacturers(null, 0, _adminAreaSettings.GridPageSize, true);
            model.Manufacturers = new GridModel<ManufacturerModel>
            {
                Data = manufacturers.Select(x => x.ToModel()),
                Total = manufacturers.TotalCount
            };
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command, ManufacturerListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var manufacturers = _manufacturerService.GetAllManufacturers(model.SearchManufacturerName,
                command.Page - 1, command.PageSize, true);
            var gridModel = new GridModel<ManufacturerModel>
            {
                Data = manufacturers.Select(x => x.ToModel()),
                Total = manufacturers.TotalCount
            };
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
            
			//default values
            model.PageSize = 12;
            model.Published = true;

            model.AllowCustomersToSelectPageSize = true;
            
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Create(ManufacturerModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var manufacturer = model.ToEntity();
                manufacturer.CreatedOnUtc = DateTime.UtcNow;
                manufacturer.UpdatedOnUtc = DateTime.UtcNow;

				MediaHelper.UpdatePictureTransientStateFor(manufacturer, m => m.PictureId);
                
				_manufacturerService.InsertManufacturer(manufacturer);
                
				//search engine name
                model.SeName = manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true);
                _urlRecordService.SaveSlug(manufacturer, model.SeName, 0);
                
				//locales
                UpdateLocales(manufacturer, model);
                
				//update picture seo file name
                UpdatePictureSeoNames(manufacturer);
				
				//Stores
				_storeMappingService.SaveStoreMappings<Manufacturer>(manufacturer, model.SelectedStoreIds);

                //activity log
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

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
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
                manufacturer.UpdatedOnUtc = DateTime.UtcNow;
                _manufacturerService.UpdateManufacturer(manufacturer);
                
				//search engine name
                model.SeName = manufacturer.ValidateSeName(model.SeName, manufacturer.Name, true);
                _urlRecordService.SaveSlug(manufacturer, model.SeName, 0);
                
				//locales
                UpdateLocales(manufacturer, model);
                
				//update picture seo file name
                UpdatePictureSeoNames(manufacturer);
				
				//Stores
				_storeMappingService.SaveStoreMappings<Manufacturer>(manufacturer, model.SelectedStoreIds);

                //activity log
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

        #region Export / Import

        public ActionResult ExportXml()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            try
            {
                var manufacturers = _manufacturerService.GetAllManufacturers(true);
                var xml = _exportManager.ExportManufacturersToXml(manufacturers);
                return new XmlDownloadResult(xml, "manufacturers.xml");
            }
            catch (Exception exc)
            {
                NotifyError(exc);
                return RedirectToAction("List");
            }
        }

        #endregion

        #region Products

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductList(GridCommand command, int manufacturerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productManufacturers = _manufacturerService.GetProductManufacturersByManufacturerId(manufacturerId,
                command.Page - 1, command.PageSize, true);

            var model = new GridModel<ManufacturerModel.ManufacturerProductModel>
            {
                Data = productManufacturers
                .Select(x =>
                {
					var product = _productService.GetProductById(x.ProductId);

                    return new ManufacturerModel.ManufacturerProductModel()
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
                }),
                Total = productManufacturers.TotalCount
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductUpdate(GridCommand command, ManufacturerModel.ManufacturerProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productManufacturer = _manufacturerService.GetProductManufacturerById(model.Id);
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
            productManufacturer.DisplayOrder = model.DisplayOrder1;
            _manufacturerService.UpdateProductManufacturer(productManufacturer);

            return ProductList(command, productManufacturer.ManufacturerId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productManufacturer = _manufacturerService.GetProductManufacturerById(id);
            if (productManufacturer == null)
                throw new ArgumentException("No product manufacturer mapping found with the specified id");

            var manufacturerId = productManufacturer.ManufacturerId;
            _manufacturerService.DeleteProductManufacturer(productManufacturer);

            return ProductList(command, manufacturerId);
        }

        public ActionResult ProductAddPopup(int manufacturerId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var ctx = new ProductSearchContext();
            ctx.LanguageId = _workContext.WorkingLanguage.Id;
            ctx.OrderBy = ProductSortingEnum.Position;
            ctx.PageSize = _adminAreaSettings.GridPageSize;
            ctx.ShowHidden = true;

            var products = _productService.SearchProducts(ctx);

            var model = new ManufacturerModel.AddManufacturerProductModel();
			model.Products = new GridModel<ProductModel>
			{
				Data = products.Select(x =>
				{
					var productModel = x.ToModel();
					productModel.ProductTypeName = x.GetProductTypeLabel(_localizationService);

					return productModel;
				}),
				Total = products.TotalCount
			};

            //categories
            var allCategories = _categoryService.GetAllCategories(showHidden: true);
            var mappedCategories = allCategories.ToDictionary(x => x.Id);
            foreach (var c in allCategories)
            {
                model.AvailableCategories.Add(new SelectListItem() { Text = c.GetCategoryNameWithPrefix(_categoryService, mappedCategories), Value = c.Id.ToString() });
            }

            //manufacturers
            foreach (var m in _manufacturerService.GetAllManufacturers(true))
            {
                model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });
            }

			//product types
			model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
			model.AvailableProductTypes.Insert(0, new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductAddPopupList(GridCommand command, ManufacturerModel.AddManufacturerProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var gridModel = new GridModel();

            var ctx = new ProductSearchContext();

            if (model.SearchCategoryId > 0)
                ctx.CategoryIds.Add(model.SearchCategoryId);

            ctx.ManufacturerId = model.SearchManufacturerId;
            ctx.Keywords = model.SearchProductName;
            ctx.LanguageId = _workContext.WorkingLanguage.Id;
            ctx.OrderBy = ProductSortingEnum.Position;
            ctx.PageIndex = command.Page - 1;
            ctx.PageSize = command.PageSize;
            ctx.ShowHidden = true;
			ctx.ProductType = model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null;

            var products = _productService.SearchProducts(ctx);

            gridModel.Data = products.Select(x =>
			{
				var productModel = x.ToModel();
				productModel.ProductTypeName = x.GetProductTypeLabel(_localizationService);

				return productModel;
			});
            gridModel.Total = products.TotalCount;
            return new JsonResult
            {
                Data = gridModel
            };
        }
        
        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult ProductAddPopup(string btnId, string formId, ManufacturerModel.AddManufacturerProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (model.SelectedProductIds != null)
            {
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        var existingProductmanufacturers = _manufacturerService.GetProductManufacturersByManufacturerId(model.ManufacturerId, 0, int.MaxValue, true);

						if (!existingProductmanufacturers.Any(x => x.ProductId == id && x.ManufacturerId == model.ManufacturerId))
                        {
                            _manufacturerService.InsertProductManufacturer(new ProductManufacturer
							{
								ManufacturerId = model.ManufacturerId,
								ProductId = id,
								IsFeaturedProduct = false,
								DisplayOrder = 1
							});
                        }
                    }
                }
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            model.Products = new GridModel<ProductModel>();
            return View(model);
        }

        #endregion
    }
}
