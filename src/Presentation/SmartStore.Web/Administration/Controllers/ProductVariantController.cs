using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Logging;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Tax;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;
using SmartStore.Services.Stores;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ProductVariantController : AdminControllerBase
    {
        #region Fields

        private readonly IProductService _productService;
		private readonly IStoreService _storeService;
        private readonly IPictureService _pictureService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IWorkContext _workContext;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IPermissionService _permissionService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly ICurrencyService _currencyService;
        private readonly IDownloadService _downloadService;
        private readonly IDeliveryTimeService _deliveryTimesService;
        private readonly IPriceFormatter _priceFormatter;   //codehint: sm-add

        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly AdminAreaSettings _adminAreaSettings;
        #endregion

        #region Constructors

        public ProductVariantController(IProductService productService,
			IStoreService storeService, IPictureService pictureService,
            ILanguageService languageService, ILocalizedEntityService localizedEntityService,
            IDiscountService discountService, ICustomerService customerService,
            ILocalizationService localizationService, IProductAttributeService productAttributeService,
            ITaxCategoryService taxCategoryService, IWorkContext workContext,
            IProductAttributeFormatter productAttributeFormatter, IShoppingCartService shoppingCartService,
            IProductAttributeParser productAttributeParser, ICustomerActivityService customerActivityService,
            IPermissionService permissionService, IPriceFormatter priceFormatter,
            ICategoryService categoryService, IManufacturerService manufacturerService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            ICurrencyService currencyService, IDownloadService downloadService,
            IDeliveryTimeService deliveryTimesService,
            CatalogSettings catalogSettings, CurrencySettings currencySettings,
            IMeasureService measureService, MeasureSettings measureSettings,
            AdminAreaSettings adminAreaSettings)
        {
            this._localizedEntityService = localizedEntityService;
            this._pictureService = pictureService;
			this._storeService = storeService;
            this._languageService = languageService;
            this._productService = productService;
            this._discountService = discountService;
            this._customerService = customerService;
            this._localizationService = localizationService;
            this._productAttributeService = productAttributeService;
            this._taxCategoryService = taxCategoryService;
            this._workContext = workContext;
            this._productAttributeFormatter = productAttributeFormatter;
            this._shoppingCartService = shoppingCartService;
            this._productAttributeParser = productAttributeParser;
            this._customerActivityService = customerActivityService;
            this._permissionService = permissionService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._backInStockSubscriptionService = backInStockSubscriptionService;
            this._currencyService = currencyService;
            this._downloadService = downloadService;
            this._deliveryTimesService = deliveryTimesService;
            this._priceFormatter = priceFormatter;  //codehint: sm-add

            this._catalogSettings = catalogSettings;
            this._currencySettings = currencySettings;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._adminAreaSettings = adminAreaSettings;
        }
        
        #endregion

        #region Utilities

        [NonAction]
        protected void UpdateLocales(ProductVariant variant, ProductVariantModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(variant,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(variant,
                                                               x => x.Description,
                                                               localized.Description,
                                                               localized.LanguageId);
            }
        }

        [NonAction]
        protected void UpdatePictureSeoNames(ProductVariant variant)
        {
            var picture = _pictureService.GetPictureById(variant.PictureId);
            if (picture != null)
                _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(variant.FullProductName));
        }

        [NonAction]
        protected void UpdateAttributeValueLocales(ProductVariantAttributeValue pvav, ProductVariantModel.ProductVariantAttributeValueModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(pvav,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);
            }
        }

        [NonAction]
        protected void PrepareProductModel(ProductVariantModel model, Product product)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            model.ProductName = product.Name;
        }

        [NonAction]
        protected void PrepareProductVariantModel(ProductVariantModel model, ProductVariant variant, bool setPredefinedValues)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //tax categories
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            foreach (var tc in taxCategories)
                model.AvailableTaxCategories.Add(new SelectListItem() { Text = tc.Name, Value = tc.Id.ToString(), Selected = variant != null && !setPredefinedValues && tc.Id == variant.TaxCategoryId });

            // codehint: sm-add (delivery times)
            var deliveryTimes = _deliveryTimesService.GetAllDeliveryTimes();
            foreach (var dt in deliveryTimes)
                model.AvailableDeliveryTimes.Add(new SelectListItem() { Text = dt.Name, Value = dt.Id.ToString(), Selected = variant != null && !setPredefinedValues && dt.Id == variant.DeliveryTimeId.GetValueOrDefault() });

            // codehint: sm-add (BasePrice aka PAnGV)
            var measureUnits = _measureService.GetAllMeasureWeights().Select(x => x.SystemKeyword).Concat(_measureService.GetAllMeasureDimensions().Select(x => x.SystemKeyword)).ToList();

			// don't forget biz import!
			if (!setPredefinedValues && variant != null && variant.BasePrice.MeasureUnit.HasValue() && !measureUnits.Exists(u => u.IsCaseInsensitiveEqual(variant.BasePrice.MeasureUnit))) {
				measureUnits.Add(variant.BasePrice.MeasureUnit);
			}

            foreach (var mu in measureUnits)
                model.AvailableMeasureUnits.Add(new SelectListItem() { Text = mu, Value = mu, Selected = variant != null && !setPredefinedValues && mu.Equals(variant.BasePrice.MeasureUnit, StringComparison.OrdinalIgnoreCase) });

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).Name;
            model.BaseDimensionIn = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId).Name;

            if (setPredefinedValues)
            {
                model.MaximumCustomerEnteredPrice = 1000;
                model.MaxNumberOfDownloads = 10;
                model.RecurringCycleLength = 100;
                model.RecurringTotalCycles = 10;
                model.StockQuantity = 10000;
                model.NotifyAdminForQuantityBelow = 1;
                model.OrderMinimumQuantity = 1;
                model.OrderMaximumQuantity = 10000;
                model.DisplayOrder = 1;

                model.UnlimitedDownloads = true;
                model.IsShipEnabled = true;
                model.Published = true;
            }
        }

        [NonAction]
        protected void PrepareDiscountModel(ProductVariantModel model, ProductVariant variant, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            var discounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, null, true);
            model.AvailableDiscounts = discounts.ToList();

            if (!excludeProperties)
            {
                model.SelectedDiscountIds = variant.AppliedDiscounts.Select(d => d.Id).ToArray();
            }
        }

        [NonAction]
        protected void PrepareProductAttributesMapping(ProductVariantModel model, ProductVariant variant)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.NumberOfAvailableProductAttributes = _productAttributeService.GetAllProductAttributes().Count;
        }

        #endregion

        #region List / Create / Edit / Delete

        public ActionResult Create(int productId, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var product = _productService.GetProductById(productId);
            if (product == null)
                //No product review found with the specified id
                return RedirectToAction("Edit", "Product", new { id = productId });

            var model = new ProductVariantModel()
            {
                ProductId = productId,
            };
            //locales
            AddLocales(_languageService, model.Locales);
            //common
            PrepareProductModel(model, product);
            PrepareProductVariantModel(model, null, true);
            //attributes
            PrepareProductAttributesMapping(model, null);
            //discounts
            PrepareDiscountModel(model, null, true);

            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Create(ProductVariantModel model, bool continueEditing, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var variant = model.ToEntity();
                variant.CreatedOnUtc = DateTime.UtcNow;
                variant.UpdatedOnUtc = DateTime.UtcNow;
                //insert variant
                _productService.InsertProductVariant(variant);
                //locales
                UpdateLocales(variant, model);
                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, null, true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                        variant.AppliedDiscounts.Add(discount);
                }
                _productService.UpdateProductVariant(variant);
                //update "HasDiscountsApplied" property
                _productService.UpdateHasDiscountsApplied(variant);
                //update picture seo file name
                UpdatePictureSeoNames(variant);

				//activity log
                _customerActivityService.InsertActivity("AddNewProductVariant", _localizationService.GetResource("ActivityLog.AddNewProductVariant"), variant.Name);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Variants.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = variant.Id, selectedTab = selectedTab }) : RedirectToAction("Edit", "Product", new { id = variant.ProductId });
            }


            //If we got this far, something failed, redisplay form
            var product = _productService.GetProductById(model.ProductId);
            if (product == null)
                throw new ArgumentException("No product found with the specified id");
            //common
            PrepareProductModel(model, product);
            PrepareProductVariantModel(model, null, false);
            //attributes
            PrepareProductAttributesMapping(model, null);
            //discounts
            PrepareDiscountModel(model, null, true);
            return View(model);
        }

        public ActionResult Edit(int id, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var variant = _productService.GetProductVariantById(id);
            if (variant == null || variant.Deleted)
                //No product variant found with the specified id
                return RedirectToAction("List", "Product");

            var model = variant.ToModel();
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = variant.GetLocalized(x => x.Name, languageId, false, false);
                locale.Description = variant.GetLocalized(x => x.Description, languageId, false, false);
            });
            //common
            PrepareProductModel(model, variant.Product);
            PrepareProductVariantModel(model, variant, false);
            //attributes
            PrepareProductAttributesMapping(model, variant);
            //discounts
            PrepareDiscountModel(model, variant, false);

            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Edit(ProductVariantModel model, bool continueEditing, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var variant = _productService.GetProductVariantById(model.Id);
            if (variant == null || variant.Deleted)
                //No product variant found with the specified id
                return RedirectToAction("List", "Product");

            if (ModelState.IsValid)
            {
                int prevPictureId = variant.PictureId;
                var prevStockQuantity = variant.StockQuantity;
                variant = model.ToEntity(variant);
                variant.UpdatedOnUtc = DateTime.UtcNow;
				variant.AvailableEndDateTimeUtc = variant.AvailableEndDateTimeUtc.ToEndOfTheDay();
				variant.SpecialPriceEndDateTimeUtc = variant.SpecialPriceEndDateTimeUtc.ToEndOfTheDay();

                //save variant
                _productService.UpdateProductVariant(variant);
                //locales
                UpdateLocales(variant, model);
                //discounts
                var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, null, true);
                foreach (var discount in allDiscounts)
                {
                    if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                    {
                        //new role
                        if (variant.AppliedDiscounts.Where(d => d.Id == discount.Id).Count() == 0)
                            variant.AppliedDiscounts.Add(discount);
                    }
                    else
                    {
                        //removed role
                        if (variant.AppliedDiscounts.Where(d => d.Id == discount.Id).Count() > 0)
                            variant.AppliedDiscounts.Remove(discount);
                    }
                }
                _productService.UpdateProductVariant(variant);
                //update "HasDiscountsApplied" property
                _productService.UpdateHasDiscountsApplied(variant);
                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != variant.PictureId)
                {
                    var prevPicture = _pictureService.GetPictureById(prevPictureId);
                    if (prevPicture != null)
                        _pictureService.DeletePicture(prevPicture);
                }
                //update picture seo file name
                UpdatePictureSeoNames(variant);
                //back in stock notifications
                if (variant.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    variant.BackorderMode == BackorderMode.NoBackorders &&
                    variant.AllowBackInStockSubscriptions &&
                    variant.StockQuantity > 0 &&
                    prevStockQuantity <= 0 &&
                    variant.Published &&
                    !variant.Deleted)
                {
                    _backInStockSubscriptionService.SendNotificationsToSubscribers(variant);
                }
                //activity log
                _customerActivityService.InsertActivity("EditProductVariant", _localizationService.GetResource("ActivityLog.EditProductVariant"), variant.FullProductName);

                SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Variants.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = model.Id, selectedTab = selectedTab }) : RedirectToAction("Edit", "Product", new { id = variant.ProductId });
            }

            //If we got this far, something failed, redisplay form
            //common
            PrepareProductModel(model, variant.Product);
            PrepareProductVariantModel(model, variant, false);
            //attributes
            PrepareProductAttributesMapping(model, variant);
            //discounts
            PrepareDiscountModel(model, variant, true);
            return View(model);
        }
        
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var variant = _productService.GetProductVariantById(id);
            if (variant == null)
                //No product variant found with the specified id
                return RedirectToAction("List", "Product");

            var productId = variant.ProductId;
            _productService.DeleteProductVariant(variant);

            //activity log
            _customerActivityService.InsertActivity("DeleteProductVariant", _localizationService.GetResource("ActivityLog.DeleteProductVariant"), variant.Name);

            SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Variants.Deleted"));
            return RedirectToAction("Edit", "Product", new { id = productId });
        }

        public ActionResult LowStockReport()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var allVariants = _productService.GetLowStockProductVariants();
            var model = new GridModel<ProductVariantModel>()
            {
                Data = allVariants.Take(_adminAreaSettings.GridPageSize).Select(x =>
                {
                    var variantModel = x.ToModel();
                    //Full product variant name
                    variantModel.Name = !String.IsNullOrEmpty(x.Name) ? string.Format("{0} ({1})", x.Product.Name, x.Name) : x.Product.Name;
                    return variantModel;
                }),
                Total = allVariants.Count
            };

            return View(model);
        }
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult LowStockReportList(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var allVariants = _productService.GetLowStockProductVariants();
            var model = new GridModel<ProductVariantModel>()
            {
                Data = allVariants.PagedForCommand(command).Select(x =>
                {
                    var variantModel = x.ToModel();
                    //Full product variant name
                    variantModel.Name = !String.IsNullOrEmpty(x.Name) ? string.Format("{0} ({1})", x.Product.Name, x.Name) : x.Product.Name;
                    return variantModel;
                }),
                Total = allVariants.Count
            };
            return new JsonResult
            {
                Data = model
            };
        }

        #endregion

        #region Bulk editing

        public ActionResult BulkEdit()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var model = new BulkEditListModel();

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

            return View(model);
        }
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult BulkEditSelect(GridCommand command, BulkEditListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var gridModel = new GridModel();
            var productVariants = _productService.SearchProductVariants(model.SearchCategoryId,
                model.SearchManufacturerId, model.SearchProductName, false,
                command.Page - 1, command.PageSize, true);
            gridModel.Data = productVariants.Select(x =>
            {
                var productVariantModel = new BulkEditProductVariantModel()
                {
                    Id = x.Id,
                    Name =  x.FullProductName,
                    Sku = x.Sku,
                    OldPrice = x.OldPrice,
                    Price = x.Price,
                    ManageInventoryMethod = x.ManageInventoryMethod.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id),
                    StockQuantity = x.StockQuantity,
                    Published = x.Published
                };

                return productVariantModel;
            });
            gridModel.Total = productVariants.TotalCount;
            return new JsonResult
            {
                Data = gridModel
            };
        }
        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult BulkEditSave(GridCommand command, 
            [Bind(Prefix = "updated")]IEnumerable<BulkEditProductVariantModel> updatedProductVariants,
            [Bind(Prefix = "deleted")]IEnumerable<BulkEditProductVariantModel> deletedProductVariants,
            BulkEditListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (updatedProductVariants != null)
            {
                foreach (var pvModel in updatedProductVariants)
                {
                    //update
                    var pv = _productService.GetProductVariantById(pvModel.Id);
                    if (pv != null)
                    {
                        pv.Sku = pvModel.Sku;
                        pv.Price = pvModel.Price;
                        pv.OldPrice = pvModel.OldPrice;
                        pv.StockQuantity = pvModel.StockQuantity;
                        pv.Published = pvModel.Published;
                        _productService.UpdateProductVariant(pv);
                    }
                }
            }
            if (deletedProductVariants != null)
            {
                foreach (var pvModel in deletedProductVariants)
                {
                    //delete
                    var pv = _productService.GetProductVariantById(pvModel.Id);
                    if (pv != null)
                        _productService.DeleteProductVariant(pv);
                }
            }
            return BulkEditSelect(command, model);
        }
        #endregion

        #region Tier prices

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult TierPriceList(GridCommand command, int productVariantId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productVariant = _productService.GetProductVariantById(productVariantId);
            if (productVariant == null)
                throw new ArgumentException("No product variant found with the specified id");

			var tierPricesModel = productVariant.TierPrices
				.OrderBy(x => x.StoreId)
				.ThenBy(x => x.Quantity)
				.ThenBy(x => x.CustomerRoleId)
                .Select(x =>
                {
					var storeName = "";
					if (x.StoreId > 0)
					{
						var store = _storeService.GetStoreById(x.StoreId);
						storeName = store != null ? store.Name : "[Deleted]";
					}
					else
					{
						storeName = _localizationService.GetResource("Admin.Common.StoresAll");
					}
                    return new ProductVariantModel.TierPriceModel()
                    {
                        Id = x.Id,
                        StoreId = x.StoreId,
						Store = storeName,
                        CustomerRole = x.CustomerRoleId.HasValue ? _customerService.GetCustomerRoleById(x.CustomerRoleId.Value).Name : _localizationService.GetResource("Admin.Catalog.Products.Variants.TierPrices.Fields.CustomerRole.AllRoles"),
                        ProductVariantId = x.ProductVariantId,
                        CustomerRoleId = x.CustomerRoleId.HasValue ? x.CustomerRoleId.Value : 0,
                        Quantity = x.Quantity,
                        Price1 = x.Price
                    };
                })
                .ToList();

            var model = new GridModel<ProductVariantModel.TierPriceModel>
            {
                Data = tierPricesModel,
                Total = tierPricesModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult TierPriceInsert(GridCommand command, ProductVariantModel.TierPriceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var tierPrice = new TierPrice()
            {
                ProductVariantId = model.ProductVariantId,
				// use Store property (not Store propertyId) because appropriate property is stored in it
				StoreId = model.Store.ToInt(),
                // codehint: sm-edit
                // use CustomerRole property (not CustomerRoleId) because appropriate property is stored in it
                CustomerRoleId = model.CustomerRole.IsNumeric() && Int32.Parse(model.CustomerRole) != 0 ? Int32.Parse(model.CustomerRole) : (int?)null,
                Quantity = model.Quantity,
                Price = model.Price1
            };
            _productService.InsertTierPrice(tierPrice);

            //update "HasTierPrices" property
            var productVariant = _productService.GetProductVariantById(model.ProductVariantId);
            _productService.UpdateHasTierPricesProperty(productVariant);

            return TierPriceList(command, model.ProductVariantId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult TierPriceUpdate(GridCommand command, ProductVariantModel.TierPriceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var tierPrice = _productService.GetTierPriceById(model.Id);
            if (tierPrice == null)
                throw new ArgumentException("No tier price found with the specified id");

			//use Store property (not Store propertyId) because appropriate property is stored in it
			tierPrice.StoreId = model.Store.ToInt();
            //use CustomerRole property (not CustomerRoleId) because appropriate property is stored in it
            // codehint: sm-edit
            tierPrice.CustomerRoleId = model.CustomerRole.IsNumeric() && Int32.Parse(model.CustomerRole) != 0 ? Int32.Parse(model.CustomerRole) : (int?)null;
            tierPrice.Quantity = model.Quantity;
            tierPrice.Price = model.Price1;
            _productService.UpdateTierPrice(tierPrice);

            return TierPriceList(command, tierPrice.ProductVariantId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult TierPriceDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var tierPrice = _productService.GetTierPriceById(id);
            if (tierPrice == null)
                throw new ArgumentException("No tier price found with the specified id");

            var productVariantId = tierPrice.ProductVariantId;
            var productVariant = _productService.GetProductVariantById(productVariantId);
            _productService.DeleteTierPrice(tierPrice);

            //update "HasTierPrices" property
            _productService.UpdateHasTierPricesProperty(productVariant);

            return TierPriceList(command, productVariantId);
        }

        #endregion

        #region Product variant attributes

        // ajax
        // codehint: sm-add
        public ActionResult AllProductVariantAttributes(string label, int selectedId)
        {
            var attributes = _productAttributeService.GetAllProductAttributes();
            if (label.HasValue())
            {
                attributes.Insert(0, new ProductAttribute { Name = label, Id = 0 });
            }

            var list = from attr in attributes
                       select new
                       {
                           id = attr.Id.ToString(),
                           text = attr.Name,
                           selected = attr.Id == selectedId
                       };

            return new JsonResult { Data = list.ToList(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttributeList(GridCommand command, int productVariantId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(productVariantId);
            var productVariantAttributesModel = productVariantAttributes
                .Select(x =>
                {
                    var pvaModel =  new ProductVariantModel.ProductVariantAttributeModel()
                    {
                        Id = x.Id,
                        ProductVariantId = x.ProductVariantId,
                        ProductAttribute = _productAttributeService.GetProductAttributeById(x.ProductAttributeId).Name,
                        ProductAttributeId = x.ProductAttributeId,
                        TextPrompt = x.TextPrompt,
                        IsRequired = x.IsRequired,
                        AttributeControlType = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext),
                        AttributeControlTypeId = x.AttributeControlTypeId,
                        DisplayOrder1 = x.DisplayOrder
                    };

                    if (x.ShouldHaveValues())
                    {
                        pvaModel.ViewEditUrl = Url.Action("EditAttributeValues", "ProductVariant", new { productVariantAttributeId = x.Id });
                        pvaModel.ViewEditText = string.Format(_localizationService.GetResource("Admin.Catalog.Products.Variants.ProductVariantAttributes.Attributes.Values.ViewLink"), x.ProductVariantAttributeValues != null ? x.ProductVariantAttributeValues.Count : 0);
                    }
                    return pvaModel;
                })
                .ToList();

            var model = new GridModel<ProductVariantModel.ProductVariantAttributeModel>
            {
                Data = productVariantAttributesModel,
                Total = productVariantAttributesModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttributeInsert(GridCommand command, ProductVariantModel.ProductVariantAttributeModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var pva = new ProductVariantAttribute()
            {
                ProductVariantId = model.ProductVariantId,
                ProductAttributeId = Int32.Parse(model.ProductAttribute), //use ProductAttribute property (not ProductAttributeId) because appropriate property is stored in it
                TextPrompt = model.TextPrompt,
                IsRequired = model.IsRequired,
                AttributeControlTypeId = Int32.Parse(model.AttributeControlType), //use AttributeControlType property (not AttributeControlTypeId) because appropriate property is stored in it
                DisplayOrder = model.DisplayOrder1
            };
            _productAttributeService.InsertProductVariantAttribute(pva);

            return ProductVariantAttributeList(command, model.ProductVariantId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttributeUpdate(GridCommand command, ProductVariantModel.ProductVariantAttributeModel model) // codehint: sm-edit
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var pva = _productAttributeService.GetProductVariantAttributeById(model.Id);
            if (pva == null)
                throw new ArgumentException("No product variant attribute found with the specified id");

            //use ProductAttribute property (not ProductAttributeId) because appropriate property is stored in it
            pva.ProductAttributeId = Int32.Parse(model.ProductAttribute);
            pva.TextPrompt = model.TextPrompt;
            pva.IsRequired = model.IsRequired;
            //use AttributeControlType property (not AttributeControlTypeId) because appropriate property is stored in it
            pva.AttributeControlTypeId = Int32.Parse(model.AttributeControlType);
            pva.DisplayOrder = model.DisplayOrder1;
            _productAttributeService.UpdateProductVariantAttribute(pva);

            return ProductVariantAttributeList(command, pva.ProductVariantId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttributeDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var pva = _productAttributeService.GetProductVariantAttributeById(id);
            if (pva == null)
                throw new ArgumentException("No product variant attribute found with the specified id");

            var productVariantId = pva.ProductVariantId;
            _productAttributeService.DeleteProductVariantAttribute(pva);

            return ProductVariantAttributeList(command, productVariantId);
        }

        #endregion

        #region Product variant attribute values

        //list
        public ActionResult EditAttributeValues(int productVariantAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var pva = _productAttributeService.GetProductVariantAttributeById(productVariantAttributeId);
            var model = new ProductVariantModel.ProductVariantAttributeValueListModel()
            {
                ProductVariantName = pva.ProductVariant.Product.Name + " " + pva.ProductVariant.Name,
                ProductVariantId = pva.ProductVariantId,
                ProductVariantAttributeName = pva.ProductAttribute.Name,
                ProductVariantAttributeId = pva.Id,
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductAttributeValueList(int productVariantAttributeId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var values = _productAttributeService.GetProductVariantAttributeValues(productVariantAttributeId);


            var gridModel = new GridModel<ProductVariantModel.ProductVariantAttributeValueModel>
            {
                Data = values.Select(x =>
                {
                    return new ProductVariantModel.ProductVariantAttributeValueModel()
                    {
                        Id = x.Id,
                        ProductVariantAttributeId = x.ProductVariantAttributeId,
                        Name = x.ProductVariantAttribute.AttributeControlType != AttributeControlType.ColorSquares ? x.Name : string.Format("{0} - {1}", x.Name, x.ColorSquaresRgb),
                        Alias = x.Alias,
                        ColorSquaresRgb = x.ColorSquaresRgb,
                        PriceAdjustment = x.PriceAdjustment,
                        WeightAdjustment = x.WeightAdjustment,
                        IsPreSelected = x.IsPreSelected,
                        DisplayOrder = x.DisplayOrder,
                    };
                }),
                Total = values.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        //delete
        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductAttributeValueDelete(int pvavId, int productVariantAttributeId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var pvav = _productAttributeService.GetProductVariantAttributeValueById(pvavId);
            if (pvav == null)
                throw new ArgumentException("No product variant attribute value found with the specified id");

            _productAttributeService.DeleteProductVariantAttributeValue(pvav);

            return ProductAttributeValueList(productVariantAttributeId, command);
        }


        //create
        public ActionResult ProductAttributeValueCreatePopup(int productAttributeAttributeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();
            
            var pva = _productAttributeService.GetProductVariantAttributeById(productAttributeAttributeId);
            var model = new ProductVariantModel.ProductVariantAttributeValueModel();
            model.ProductVariantAttributeId = productAttributeAttributeId;

            //color squares
            model.DisplayColorSquaresRgb = pva.AttributeControlType == AttributeControlType.ColorSquares;
            model.ColorSquaresRgb = "#000000";

            //locales
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost]
        public ActionResult ProductAttributeValueCreatePopup(string btnId, string formId, ProductVariantModel.ProductVariantAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var pva = _productAttributeService.GetProductVariantAttributeById(model.ProductVariantAttributeId);
            if (pva == null)
                //No product variant attribute found with the specified id
                return RedirectToAction("List", "Product");

            if (pva.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
                try
                {
                    var color = System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError("", exc.Message);
                }
            }

            if (ModelState.IsValid)
            {
                var pvav = new ProductVariantAttributeValue()
                {
                    ProductVariantAttributeId = model.ProductVariantAttributeId,
                    Name = model.Name,
                    Alias = model.Alias,
                    ColorSquaresRgb = model.ColorSquaresRgb,
                    PriceAdjustment = model.PriceAdjustment,
                    WeightAdjustment = model.WeightAdjustment,
                    IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder
                };

                _productAttributeService.InsertProductVariantAttributeValue(pvav);
                UpdateAttributeValueLocales(pvav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        //edit
        public ActionResult ProductAttributeValueEditPopup(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var pvav = _productAttributeService.GetProductVariantAttributeValueById(id);
            if (pvav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            var model = new ProductVariantModel.ProductVariantAttributeValueModel()
            {
                ProductVariantAttributeId = pvav.ProductVariantAttributeId,
                Name= pvav.Name,
                Alias = pvav.Alias,
                ColorSquaresRgb = pvav.ColorSquaresRgb,
                DisplayColorSquaresRgb = pvav.ProductVariantAttribute.AttributeControlType == AttributeControlType.ColorSquares,
                PriceAdjustment = pvav.PriceAdjustment,
                WeightAdjustment = pvav.WeightAdjustment,
                IsPreSelected = pvav.IsPreSelected,
                DisplayOrder = pvav.DisplayOrder
            };
            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = pvav.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        public ActionResult ProductAttributeValueEditPopup(string btnId, string formId, ProductVariantModel.ProductVariantAttributeValueModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var pvav = _productAttributeService.GetProductVariantAttributeValueById(model.Id);
            if (pvav == null)
                //No attribute value found with the specified id
                return RedirectToAction("List", "Product");

            if (pvav.ProductVariantAttribute.AttributeControlType == AttributeControlType.ColorSquares)
            {
                //ensure valid color is chosen/entered
                if (String.IsNullOrEmpty(model.ColorSquaresRgb))
                    ModelState.AddModelError("", "Color is required");
                try
                {
                    var color = System.Drawing.ColorTranslator.FromHtml(model.ColorSquaresRgb);
                }
                catch (Exception exc)
                {
                    ModelState.AddModelError("", exc.Message);
                }
            }

            if (ModelState.IsValid)
            {
                pvav.Name = model.Name;
                pvav.Alias = model.Alias;
                pvav.ColorSquaresRgb = model.ColorSquaresRgb;
                pvav.PriceAdjustment = model.PriceAdjustment;
                pvav.WeightAdjustment = model.WeightAdjustment;
                pvav.IsPreSelected = model.IsPreSelected;
                pvav.DisplayOrder = model.DisplayOrder;
                _productAttributeService.UpdateProductVariantAttributeValue(pvav);

                UpdateAttributeValueLocales(pvav, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Product variant attribute combinations

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttributeCombinationList(GridCommand command, int productVariantId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

			// codehint: sm-edit
			// TODO: Replace ProductVariantModel.ProductVariantAttributeCombinationModel by AddProductVariantAttributeCombinationModel
			// when there's no grid-inline-editing anymore.

            var variant = _productService.GetProductVariantById(productVariantId);
			var productVariantAttributeCombinations = _productAttributeService.GetAllProductVariantAttributeCombinations(productVariantId, true);

			var productUrlTitle = _localizationService.GetResource("Common.OpenInShop");
			var productUrl = Url.RouteUrl("Product", new { SeName = variant.Product.GetSeName() });
			productUrl = "{0}{1}attributes=".FormatWith(productUrl, productUrl.Contains("?") ? "&" : "?");

            var productVariantAttributesModel = productVariantAttributeCombinations.Select(x => {
                var pvacModel = x.ToModel();
                PrepareProductAttributeCombinationModel(pvacModel, x, variant, true);

				// codehint: sm-add
				pvacModel.ProductUrl = productUrl + _productAttributeParser.SerializeQueryData(variant.Id, x.AttributesXml);
				pvacModel.ProductUrlTitle = productUrlTitle;

				//if (x.IsDefaultCombination)
				//	pvacModel.AttributesXml = "<b>{0}</b>".FormatWith(pvacModel.AttributesXml);		// codehint: sm-add

                //warnings
                var warnings = _shoppingCartService.GetShoppingCartItemAttributeWarnings(ShoppingCartType.ShoppingCart, x.ProductVariant, x.AttributesXml);
                pvacModel.Warnings.AddRange(warnings);

                return pvacModel;
            }).ToList();

            var model = new GridModel<ProductVariantAttributeCombinationModel>
            {
                Data = productVariantAttributesModel,
                Total = productVariantAttributesModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductVariantAttributeCombinationDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var pvac = _productAttributeService.GetProductVariantAttributeCombinationById(id);
            if (pvac == null)
                throw new ArgumentException("No product variant attribute combination found with the specified id");

            var productVariantId = pvac.ProductVariantId;
            _productAttributeService.DeleteProductVariantAttributeCombination(pvac);

            return ProductVariantAttributeCombinationList(command, productVariantId);
        }

        // create new form
        public ActionResult AttributeCombinationCreatePopup(string btnId, string formId, int productVariantId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var variant = _productService.GetProductVariantById(productVariantId);
            if (variant == null)
                //No product variant found with the specified id
                return RedirectToAction("List", "Product");

            var model = new ProductVariantAttributeCombinationModel();

            PrepareProductAttributeCombinationModel(model, null, variant);
            PrepareVariantCombinationAttributes(model, variant);
            PrepareVariantCombinationPictures(model, variant);
			PrepareDeliveryTimes(model);	// codehint: sm-add
			PrepareViewBag(btnId, formId, false, false);		// codehint: sm-edit

            return View(model);
        }

        // create new save
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AttributeCombinationCreatePopup(string btnId, string formId, int productVariantId, 
            ProductVariantAttributeCombinationModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var variant = _productService.GetProductVariantById(productVariantId);
            if (variant == null)
                //No product variant found with the specified id
                return RedirectToAction("List", "Product");

			// codehint: sm-edit
			var warnings = new List<string>();
			var variantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(variant.Id);

			string attributeXml = form.CreateSelectedAttributesXml(variant.Id, variantAttributes, _productAttributeParser, _localizationService, 
				_downloadService, _catalogSettings, this.Request, warnings, false);

            warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(ShoppingCartType.ShoppingCart, variant, attributeXml));

			if (null != _productAttributeParser.FindProductVariantAttributeCombination(variant, attributeXml, true)) {
				warnings.Add(_localizationService.GetResource("Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.CombiExists"));
			}

            if (warnings.Count == 0)
            {
                var combination = model.ToEntity();
                combination.AttributesXml = attributeXml;
				combination.SetAssignedPictureIds(model.AssignedPictureIds);

                _productAttributeService.InsertProductVariantAttributeCombination(combination);
            }

			PrepareProductAttributeCombinationModel(model, null, variant);
			PrepareVariantCombinationAttributes(model, variant);
			PrepareVariantCombinationPictures(model, variant);
			PrepareDeliveryTimes(model);
			PrepareViewBag(btnId, formId, warnings.Count == 0, false);

			if (warnings.Count > 0)
				model.Warnings = warnings;

			return View(model);
        }

        // edit form
        public ActionResult AttributeCombinationEditPopup(int id, string btnId, string formId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var combination = _productAttributeService.GetProductVariantAttributeCombinationById(id);
            if (combination == null)
            {
                return RedirectToAction("List", "Product");
            }

            var variant = _productService.GetProductVariantById(combination.ProductVariantId);
            if (variant == null)
                // No product variant found with the specified id
                return RedirectToAction("List", "Product");

            var model = combination.ToModel();

			PrepareProductAttributeCombinationModel(model, combination, variant, true);		// codehint: sm-edit
            PrepareVariantCombinationAttributes(model, variant);
            PrepareVariantCombinationPictures(model, variant);
			PrepareDeliveryTimes(model, model.DeliveryTimeId);	// codehint: sm-add
			PrepareViewBag(btnId, formId);		// codehint: sm-add

            return View(model);
        }

        // edit save
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AttributeCombinationEditPopup(string btnId, string formId, ProductVariantAttributeCombinationModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

			// codehint: sm-edit
			if (ModelState.IsValid) {
				var combination = _productAttributeService.GetProductVariantAttributeCombinationById(model.Id);
				if (combination == null)
					return RedirectToAction("List", "Product");

				string attributeXml = combination.AttributesXml;
				combination = model.ToEntity(combination);
				combination.AttributesXml = attributeXml;
				combination.SetAssignedPictureIds(model.AssignedPictureIds);

				_productAttributeService.UpdateProductVariantAttributeCombination(combination);

				PrepareViewBag(btnId, formId, true);
			}
			return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CreateAllAttributeCombinations(ProductVariantAttributeCombinationModel model, int productVariantId) {
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
				return AccessDeniedView();

			var variant = _productService.GetProductVariantById(productVariantId);
			if (variant == null)
				return RedirectToAction("List", "Product");

			_productAttributeService.CreateAllProductVariantAttributeCombinations(variant);
			
			return new JsonResult { Data = "" };
		}

		/// <remarks>Checks if an attribute combination already exists.</remarks>
		/// <remarks>codehint: sm-add</remarks>
		[HttpPost]
		public ActionResult CombinationExistenceNote(int productVariantId, FormCollection form) {
			// no further authorization here

			var warnings = new List<string>();
			var variantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(productVariantId);

			string attributeXml = form.CreateSelectedAttributesXml(productVariantId, variantAttributes, _productAttributeParser,
				_localizationService, _downloadService, _catalogSettings, this.Request, warnings, false);

			bool exists = (null != _productAttributeParser.FindProductVariantAttributeCombination(productVariantId, attributeXml, true));

			if (!exists) {
				var variant = _productService.GetProductVariantById(productVariantId);
				if (variant != null)
					warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(ShoppingCartType.ShoppingCart, variant, attributeXml));
			}

			if (warnings.Count > 0) {
				return new JsonResult {
					Data = new {
						Message = warnings[0],
						HasWarning = true
					}
				};
			}

			return new JsonResult {
				Data = new {
					Message = _localizationService.GetResource("Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.{0}".FormatWith(exists ? "CombiExists" : "CombiNotExists")),
					HasWarning = exists
				}
			};
		}

        [NonAction]
        protected void PrepareProductAttributeCombinationModel(ProductVariantAttributeCombinationModel model, ProductVariantAttributeCombination entity, ProductVariant variant, bool formatAttributes = false)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (variant == null)
                throw new ArgumentNullException("variant");

			model.ProductVariantId = variant.Id;

            if (entity == null)
            {
                // is a new entity, so initialize it properly
                model.StockQuantity = 10000;
				model.IsActive = true;	// codehint: sm-add
				model.AllowOutOfStockOrders = true;		// codehint: sm-add
            }

            if (formatAttributes && entity != null)
            {
                model.AttributesXml = _productAttributeFormatter.FormatAttributes(variant, entity.AttributesXml, _workContext.CurrentCustomer, "<br />", true, true, true, false);
            }
        }

        private void PrepareVariantCombinationAttributes(ProductVariantAttributeCombinationModel model, ProductVariant variant)
        {
            var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductVariantId(variant.Id);
            foreach (var attribute in productVariantAttributes)
            {
                var pvaModel = new ProductVariantAttributeCombinationModel.ProductVariantAttributeModel()
                {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = attribute.ProductAttribute.Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var pvaValues = _productAttributeService.GetProductVariantAttributeValues(attribute.Id);
                    foreach (var pvaValue in pvaValues)
                    {
                        var pvaValueModel = new ProductVariantAttributeCombinationModel.ProductVariantAttributeValueModel()
                        {
                            Id = pvaValue.Id,
                            Name = pvaValue.Name,
                            IsPreSelected = pvaValue.IsPreSelected
                        };
                        pvaModel.Values.Add(pvaValueModel);
                    }
                }

                model.ProductVariantAttributes.Add(pvaModel);
            }
        }

        private void PrepareVariantCombinationPictures(ProductVariantAttributeCombinationModel model, ProductVariant variant)
        {
            var pictures = _pictureService.GetPicturesByProductId(variant.ProductId);
            foreach (var picture in pictures)
            {
                var assignablePicture = new ProductVariantAttributeCombinationModel.PictureSelectItemModel();
                assignablePicture.Id = picture.Id;
                assignablePicture.IsAssigned = model.AssignedPictureIds.Contains(picture.Id);
                assignablePicture.PictureUrl = _pictureService.GetPictureUrl(picture.Id, 125, false);
                model.AssignablePictures.Add(assignablePicture);
            }
        }

		// codehint: sm-add
		private void PrepareViewBag(string btnId, string formId, bool refreshPage = false, bool isEdit = true) {
			ViewBag.btnId = btnId;
			ViewBag.formId = formId;
			ViewBag.RefreshPage = refreshPage;
			ViewBag.IsEdit = isEdit;
		}
		private void PrepareDeliveryTimes(ProductVariantAttributeCombinationModel model, int? selectId = null) {
			var deliveryTimes = _deliveryTimesService.GetAllDeliveryTimes();

			foreach (var dt in deliveryTimes) {
				model.AvailableDeliveryTimes.Add(new SelectListItem() {
					Text = dt.Name,
					Value = dt.Id.ToString(),
					Selected = (selectId == dt.Id)
				});
			}
		}

        #endregion

        [HttpPost]
        public ActionResult GetBasePrice(int productVariantId, string basePriceMeasureUnit, decimal basePriceAmount, int basePriceBaseAmount)
        {
            var variant = _productService.GetProductVariantById(productVariantId);

			//string unit = basePriceBaseAmount.ToString() + " " + basePriceMeasureUnit;

			//// preis / (QuantityUnit * MeasureUnitBase) €
			//string price = _priceFormatter.FormatPrice((variant.Price / (basePriceAmount * basePriceBaseAmount)), false, false);

			////{0} pro Einheit (Grundpreis: {1} pro {2})
			//string basePrice = string.Format(_localizationService.GetResource("Admin.Catalog.Products.Variants.Fields.BasePriceInfo"), basePriceAmount, price, unit);

			string basePrice = "";

			if (basePriceAmount != Decimal.Zero) {
                decimal basePriceValue = Convert.ToDecimal((variant.Price / basePriceAmount) * basePriceBaseAmount);

				string basePriceFormatted = _priceFormatter.FormatPrice(basePriceValue, false, false);
				string unit = "{0} {1}".FormatWith(basePriceBaseAmount, basePriceMeasureUnit);

                basePrice = _localizationService.GetResource("Admin.Catalog.Products.Variants.Fields.BasePriceInfo").FormatWith(basePriceAmount.ToString("G29") + " " + basePriceMeasureUnit, basePriceFormatted, unit);
			}

            return Json(new { Result = true, BasePrice = basePrice });
        }

    }
}
