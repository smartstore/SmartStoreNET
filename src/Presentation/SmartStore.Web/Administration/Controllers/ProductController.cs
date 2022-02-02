using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Autofac;
using NuGet;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Collections;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Core.Search;
using SmartStore.Core.Security;
using SmartStore.Data.Utilities;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Discounts;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Search;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ProductController : AdminControllerBase
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICustomerService _customerService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IMediaService _mediaService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IProductTagService _productTagService;
        private readonly ICopyProductService _copyProductService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IAclService _aclService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDiscountService _discountService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IRepository<ProductVariantAttributeCombination> _pvacRepository;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly CatalogSettings _catalogSettings;
        private readonly IDownloadService _downloadService;
        private readonly IDeliveryTimeService _deliveryTimesService;
        private readonly IQuantityUnitService _quantityUnitService;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDbContext _dbContext;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICommonServices _services;
        private readonly ICountryService _countryService;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly ProductUrlHelper _productUrlHelper;
        private readonly SeoSettings _seoSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly SearchSettings _searchSettings;

        #endregion

        #region Constructors

        public ProductController(
            IProductService productService,
            IProductTemplateService productTemplateService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            ICustomerService customerService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            ISpecificationAttributeService specificationAttributeService,
            IMediaService mediaService,
            ITaxCategoryService taxCategoryService,
            IProductTagService productTagService,
            ICopyProductService copyProductService,
            ICustomerActivityService customerActivityService,
            IAclService aclService,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            AdminAreaSettings adminAreaSettings,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IProductAttributeService productAttributeService,
            IRepository<ProductVariantAttributeCombination> pvacRepository,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            IShoppingCartService shoppingCartService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            CatalogSettings catalogSettings,
            IDownloadService downloadService,
            IDeliveryTimeService deliveryTimesService,
            IQuantityUnitService quantityUnitService,
            IMeasureService measureService,
            MeasureSettings measureSettings,
            IPriceFormatter priceFormatter,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            ICommonServices services,
            ICountryService countryService,
            ICatalogSearchService catalogSearchService,
            ProductUrlHelper productUrlHelper,
            SeoSettings seoSettings,
            MediaSettings mediaSettings,
            SearchSettings searchSettings)
        {
            _productService = productService;
            _productTemplateService = productTemplateService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _customerService = customerService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _specificationAttributeService = specificationAttributeService;
            _mediaService = mediaService;
            _taxCategoryService = taxCategoryService;
            _productTagService = productTagService;
            _copyProductService = copyProductService;
            _customerActivityService = customerActivityService;
            _aclService = aclService;
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _adminAreaSettings = adminAreaSettings;
            _dateTimeHelper = dateTimeHelper;
            _discountService = discountService;
            _productAttributeService = productAttributeService;
            _pvacRepository = pvacRepository;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _shoppingCartService = shoppingCartService;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _catalogSettings = catalogSettings;
            _downloadService = downloadService;
            _deliveryTimesService = deliveryTimesService;
            _quantityUnitService = quantityUnitService;
            _measureService = measureService;
            _measureSettings = measureSettings;
            _priceFormatter = priceFormatter;
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _services = services;
            _countryService = countryService;
            _catalogSearchService = catalogSearchService;
            _productUrlHelper = productUrlHelper;
            _seoSettings = seoSettings;
            _mediaSettings = mediaSettings;
            _searchSettings = searchSettings;
        }

        #endregion

        #region Update[...]

        [NonAction]
        protected void UpdateProductGeneralInfo(Product product, ProductModel model, out bool nameChanged)
        {
            var p = product;
            var m = model;

            p.ProductTypeId = m.ProductTypeId;
            p.Visibility = m.Visibility;
            p.Condition = m.Condition;
            p.ProductTemplateId = m.ProductTemplateId;

            nameChanged = !string.Equals(p.Name, m.Name, StringComparison.CurrentCultureIgnoreCase);

            p.Name = m.Name;
            p.ShortDescription = m.ShortDescription;
            p.FullDescription = m.FullDescription;
            p.Sku = m.Sku;
            p.ManufacturerPartNumber = m.ManufacturerPartNumber;
            p.Gtin = m.Gtin;
            p.AdminComment = m.AdminComment;
            p.AvailableStartDateTimeUtc = m.AvailableStartDateTimeUtc;
            p.AvailableEndDateTimeUtc = m.AvailableEndDateTimeUtc;

            p.AllowCustomerReviews = m.AllowCustomerReviews;
            p.ShowOnHomePage = m.ShowOnHomePage;
            p.HomePageDisplayOrder = m.HomePageDisplayOrder;
            p.Published = m.Published;
            p.RequireOtherProducts = m.RequireOtherProducts;
            p.RequiredProductIds = m.RequiredProductIds;
            p.AutomaticallyAddRequiredProducts = m.AutomaticallyAddRequiredProducts;

            p.IsGiftCard = m.IsGiftCard;
            p.GiftCardTypeId = m.GiftCardTypeId;

            p.IsRecurring = m.IsRecurring;
            p.RecurringCycleLength = m.RecurringCycleLength;
            p.RecurringCyclePeriodId = m.RecurringCyclePeriodId;
            p.RecurringTotalCycles = m.RecurringTotalCycles;

            p.IsShipEnabled = m.IsShipEnabled;
            p.DeliveryTimeId = m.DeliveryTimeId == 0 ? null : m.DeliveryTimeId;
            p.QuantityUnitId = m.QuantityUnitId == 0 ? null : m.QuantityUnitId;
            p.IsFreeShipping = m.IsFreeShipping;
            p.AdditionalShippingCharge = m.AdditionalShippingCharge;
            p.Weight = m.Weight;
            p.Length = m.Length;
            p.Width = m.Width;
            p.Height = m.Height;

            p.IsEsd = m.IsEsd;
            p.IsTaxExempt = m.IsTaxExempt;
            p.TaxCategoryId = m.TaxCategoryId ?? 0;
            p.CustomsTariffNumber = m.CustomsTariffNumber;
            p.CountryOfOriginId = m.CountryOfOriginId == 0 ? null : m.CountryOfOriginId;

            p.AvailableEndDateTimeUtc = p.AvailableEndDateTimeUtc.ToEndOfTheDay();
            p.SpecialPriceEndDateTimeUtc = p.SpecialPriceEndDateTimeUtc.ToEndOfTheDay();
        }

        [NonAction]
        protected void UpdateProductDownloads(Product product, ProductModel model)
        {
            if (!Services.Permissions.Authorize(Permissions.Media.Download.Update))
            {
                return;
            }

            var p = product;
            var m = model;

            p.IsDownload = m.IsDownload;
            //p.DownloadId = m.DownloadId ?? 0;
            p.UnlimitedDownloads = m.UnlimitedDownloads;
            p.MaxNumberOfDownloads = m.MaxNumberOfDownloads;
            p.DownloadExpirationDays = m.DownloadExpirationDays;
            p.DownloadActivationTypeId = m.DownloadActivationTypeId;
            p.HasUserAgreement = m.HasUserAgreement;
            p.UserAgreementText = m.UserAgreementText;
            p.HasSampleDownload = m.HasSampleDownload;
            p.SampleDownloadId = m.SampleDownloadId == 0 ? null : m.SampleDownloadId;
        }

        [NonAction]
        protected void UpdateProductInventory(Product product, ProductModel model)
        {
            var p = product;
            var m = model;
            var updateStockQuantity = true;
            var stockQuantityInDatabase = product.StockQuantity;

            if (p.ManageInventoryMethod == ManageInventoryMethod.ManageStock && p.Id != 0)
            {
                if (m.OriginalStockQuantity != stockQuantityInDatabase)
                {
                    // The stock has changed since the edit page was loaded, e.g. because an order has been placed.
                    updateStockQuantity = false;

                    if (m.StockQuantity != m.OriginalStockQuantity)
                    {
                        // The merchant has changed the stock quantity manually.
                        NotifyWarning(T("Admin.Catalog.Products.StockQuantityNotChanged", stockQuantityInDatabase.ToString("N0")));
                    }
                }
            }

            if (updateStockQuantity)
            {
                p.StockQuantity = m.StockQuantity;
            }

            p.ManageInventoryMethodId = m.ManageInventoryMethodId;
            p.DisplayStockAvailability = m.DisplayStockAvailability;
            p.DisplayStockQuantity = m.DisplayStockQuantity;
            p.MinStockQuantity = m.MinStockQuantity;
            p.LowStockActivityId = m.LowStockActivityId;
            p.NotifyAdminForQuantityBelow = m.NotifyAdminForQuantityBelow;
            p.BackorderModeId = m.BackorderModeId;
            p.AllowBackInStockSubscriptions = m.AllowBackInStockSubscriptions;
            p.OrderMinimumQuantity = m.OrderMinimumQuantity;
            p.OrderMaximumQuantity = m.OrderMaximumQuantity;
            p.QuantityStep = m.QuantityStep;
            p.HideQuantityControl = m.HideQuantityControl;

            if (p.ManageInventoryMethod == ManageInventoryMethod.ManageStock && updateStockQuantity)
            {
                // Back in stock notifications.
                if (p.BackorderMode == BackorderMode.NoBackorders &&
                    p.AllowBackInStockSubscriptions &&
                    p.StockQuantity > 0 &&
                    stockQuantityInDatabase <= 0 &&
                    p.Published &&
                    !p.Deleted &&
                    !p.IsSystemProduct)
                {
                    _backInStockSubscriptionService.SendNotificationsToSubscribers(p);
                }

                if (p.StockQuantity != stockQuantityInDatabase)
                {
                    _productService.AdjustInventory(p, true, 0, string.Empty);
                }
            }
        }

        [NonAction]
        protected void UpdateProductBundleItems(Product product, ProductModel model)
        {
            var p = product;
            var m = model;

            p.BundleTitleText = m.BundleTitleText;
            p.BundlePerItemPricing = m.BundlePerItemPricing;
            p.BundlePerItemShipping = m.BundlePerItemShipping;
            p.BundlePerItemShoppingCart = m.BundlePerItemShoppingCart;

            // SEO
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(product, x => x.BundleTitleText, localized.BundleTitleText, localized.LanguageId);
            }
        }

        [NonAction]
        protected void UpdateProductPrice(Product product, ProductModel model)
        {
            var p = product;
            var m = model;

            p.Price = m.Price;
            p.OldPrice = m.OldPrice;
            p.ProductCost = m.ProductCost;
            p.SpecialPrice = m.SpecialPrice;
            p.SpecialPriceStartDateTimeUtc = m.SpecialPriceStartDateTimeUtc;
            p.SpecialPriceEndDateTimeUtc = m.SpecialPriceEndDateTimeUtc;
            p.DisableBuyButton = m.DisableBuyButton;
            p.DisableWishlistButton = m.DisableWishlistButton;
            p.AvailableForPreOrder = m.AvailableForPreOrder;
            p.CallForPrice = m.CallForPrice;
            p.CustomerEntersPrice = m.CustomerEntersPrice;
            p.MinimumCustomerEnteredPrice = m.MinimumCustomerEnteredPrice;
            p.MaximumCustomerEnteredPrice = m.MaximumCustomerEnteredPrice;

            p.BasePriceEnabled = m.BasePriceEnabled;
            p.BasePriceBaseAmount = m.BasePriceBaseAmount;
            p.BasePriceAmount = m.BasePriceAmount;
            p.BasePriceMeasureUnit = m.BasePriceMeasureUnit;

            // Discounts.
            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, null, true);
            foreach (var discount in allDiscounts)
            {
                if (model.SelectedDiscountIds != null && model.SelectedDiscountIds.Contains(discount.Id))
                {
                    if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) == 0)
                        product.AppliedDiscounts.Add(discount);
                }
                else
                {
                    if (product.AppliedDiscounts.Count(d => d.Id == discount.Id) > 0)
                        product.AppliedDiscounts.Remove(discount);
                }
            }
        }

        [NonAction]
        protected void UpdateProductAttributes(Product product, ProductModel model)
        {
            product.AttributeChoiceBehaviour = model.AttributeChoiceBehaviour;
        }

        [NonAction]
        protected void UpdateProductSeo(Product product, ProductModel model)
        {
            var p = product;
            var m = model;

            p.MetaKeywords = m.MetaKeywords;
            p.MetaDescription = m.MetaDescription;
            p.MetaTitle = m.MetaTitle;

            var service = _localizedEntityService;
            foreach (var localized in model.Locales)
            {
                service.SaveLocalizedValue(product, x => x.MetaKeywords, localized.MetaKeywords, localized.LanguageId);
                service.SaveLocalizedValue(product, x => x.MetaDescription, localized.MetaDescription, localized.LanguageId);
                service.SaveLocalizedValue(product, x => x.MetaTitle, localized.MetaTitle, localized.LanguageId);
            }
        }

        [NonAction]
        protected void UpdateProductPictures(Product product, ProductModel model)
        {
            product.HasPreviewPicture = model.HasPreviewPicture;
        }

        [NonAction]
        private void UpdateLocales(ProductTag productTag, ProductTagModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(productTag, x => x.Name, localized.Name, localized.LanguageId);
            }
        }

        [NonAction]
        private void UpdateLocales(ProductVariantAttributeValue pvav, ProductModel.ProductVariantAttributeValueModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(pvav, x => x.Name, localized.Name, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(pvav, x => x.Alias, localized.Alias, localized.LanguageId);
            }
        }

        [NonAction]
        private void UpdateDataOfExistingProduct(Product product, ProductModel model, bool editMode, bool nameChanged)
        {
            var p = product;
            var m = model;

            //var seoTabLoaded = m.LoadedTabs.Contains("SEO", StringComparer.OrdinalIgnoreCase);

            // SEO.
            m.SeName = p.ValidateSeName(m.SeName, p.Name, true, _urlRecordService, _seoSettings);
            _urlRecordService.SaveSlug(p, m.SeName, 0);

            if (editMode)
            {
                // We need the event to be fired.
                _productService.UpdateProduct(p);
            }

            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(product, x => x.Name, localized.Name, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product, x => x.ShortDescription, localized.ShortDescription, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(product, x => x.FullDescription, localized.FullDescription, localized.LanguageId);

                var localizedSeName = p.ValidateSeName(localized.SeName, localized.Name, false, _urlRecordService, _seoSettings, localized.LanguageId);
                _urlRecordService.SaveSlug(p, localizedSeName, localized.LanguageId);
            }

            _productTagService.UpdateProductTags(p, m.ProductTags);

            SaveStoreMappings(p, model.SelectedStoreIds);
            SaveAclMappings(p, model.SelectedCustomerRoleIds);
        }

        #endregion

        #region Utitilies

        [NonAction]
        protected void PrepareProductModel(ProductModel model, Product product, bool setPredefinedValues, bool excludeProperties)
        {
            Guard.NotNull(model, nameof(model));

            if (product != null)
            {
                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct != null)
                {
                    model.AssociatedToProductId = product.ParentGroupedProductId;
                    model.AssociatedToProductName = parentGroupedProduct.Name;
                }

                model.CreatedOn = _dateTimeHelper.ConvertToUserTime(product.CreatedOnUtc, DateTimeKind.Utc);
                model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(product.UpdatedOnUtc, DateTimeKind.Utc);
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(product);
                model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccessTo(product);
                model.OriginalStockQuantity = product.StockQuantity;

                if (product.LimitedToStores)
                {
                    var storeMappings = _storeMappingService.GetStoreMappings(product);
                    if (storeMappings.FirstOrDefault(x => x.StoreId == _services.StoreContext.CurrentStore.Id) == null)
                    {
                        var storeMapping = storeMappings.FirstOrDefault();
                        if (storeMapping != null)
                        {
                            var store = _services.StoreService.GetStoreById(storeMapping.StoreId);
                            if (store != null)
                                model.ProductUrl = store.Url.EnsureEndsWith("/") + product.GetSeName();
                        }
                    }
                }

                if (model.ProductUrl.IsEmpty())
                {
                    model.ProductUrl = Url.RouteUrl("Product", new { SeName = product.GetSeName() }, Request.Url.Scheme);
                }

                // Downloads.
                var productDownloads = _downloadService.GetDownloadsFor(product, true);

                model.DownloadVersions = productDownloads
                    .Select(x => new DownloadVersion
                    {
                        FileVersion = x.FileVersion,
                        DownloadId = x.Id,
                        FileName = x.UseDownloadUrl ? x.DownloadUrl : x.MediaFile?.Name,
                        DownloadUrl = x.UseDownloadUrl ? x.DownloadUrl : Url.Action("DownloadFile", "Download", new { downloadId = x.Id })
                    })
                    .ToList();

                var currentDownload = productDownloads.FirstOrDefault();

                model.DownloadId = currentDownload?.Id;
                model.CurrentDownload = currentDownload;
                if (currentDownload != null && currentDownload.MediaFile != null)
                {
                    model.DownloadThumbUrl = _mediaService.GetUrl(currentDownload.MediaFile.Id, _mediaSettings.CartThumbPictureSize, null, true);
                    currentDownload.DownloadUrl = Url.Action("DownloadFile", "Download", new { downloadId = currentDownload.Id });
                }

                model.DownloadFileVersion = (currentDownload?.FileVersion).EmptyNull();
                model.OldSampleDownloadId = model.SampleDownloadId;

                // Media files.
                var file = _mediaService.GetFileById(product.MainPictureId ?? 0);
                model.PictureThumbnailUrl = _mediaService.GetUrl(file, _mediaSettings.CartThumbPictureSize);
                model.NoThumb = file == null;

                PrepareProductPictureModel(model);
                model.AddPictureModel.PictureId = product.MainPictureId ?? 0;
            }

            model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;
            model.BaseWeightIn = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId)?.GetLocalized(x => x.Name) ?? string.Empty;
            model.BaseDimensionIn = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId)?.GetLocalized(x => x.Name) ?? string.Empty;

            model.NumberOfAvailableProductAttributes = _productAttributeService.GetAllProductAttributes(0, 1).TotalCount;
            model.NumberOfAvailableManufacturers = _manufacturerService.GetAllManufacturers("", pageIndex: 0, pageSize: 1, showHidden: true).TotalCount;
            model.NumberOfAvailableCategories = _categoryService.GetAllCategories(pageIndex: 0, pageSize: 1, showHidden: true).TotalCount;

            // Copy product.
            if (product != null)
            {
                model.CopyProductModel.Id = product.Id;
                model.CopyProductModel.Name = T("Admin.Common.CopyOf", product.Name);
                model.CopyProductModel.Published = true;
            }

            // Templates.
            var templates = _productTemplateService.GetAllProductTemplates();
            foreach (var template in templates)
            {
                model.AvailableProductTemplates.Add(new SelectListItem
                {
                    Text = template.Name,
                    Value = template.Id.ToString()
                });
            }

            // Product tags.
            if (product != null)
            {
                model.ProductTags = product.ProductTags.Select(x => x.Name).ToArray();
            }

            var allTags = _productTagService.GetAllProductTagNames();
            model.AvailableProductTags = new MultiSelectList(allTags, model.ProductTags);

            // Tax categories.
            var taxCategories = _taxCategoryService.GetAllTaxCategories();
            foreach (var tc in taxCategories)
            {
                model.AvailableTaxCategories.Add(new SelectListItem
                {
                    Text = tc.Name,
                    Value = tc.Id.ToString(),
                    Selected = product != null && !setPredefinedValues && tc.Id == product.TaxCategoryId
                });
            }

            // Do not pre-select a tax category that is not stored.
            if (product != null && product.TaxCategoryId == 0)
            {
                model.AvailableTaxCategories.Insert(0, new SelectListItem { Text = T("Common.PleaseSelect"), Value = "", Selected = true });
            }

            // Delivery times.
            if (setPredefinedValues)
            {
                var defaultDeliveryTime = _deliveryTimesService.GetDefaultDeliveryTime();
                model.DeliveryTimeId = defaultDeliveryTime?.Id;
            }

            // Quantity units.
            var quantityUnits = _quantityUnitService.GetAllQuantityUnits();
            foreach (var mu in quantityUnits)
            {
                model.AvailableQuantityUnits.Add(new SelectListItem
                {
                    Text = mu.Name,
                    Value = mu.Id.ToString(),
                    Selected = product != null && !setPredefinedValues && mu.Id == product.QuantityUnitId.GetValueOrDefault()
                });
            }

            // BasePrice aka PAnGV
            var measureUnits = _measureService.GetAllMeasureWeights()
                .Select(x => x.SystemKeyword).Concat(_measureService.GetAllMeasureDimensions().Select(x => x.SystemKeyword)).ToList();

            // Don't forget biz import!
            if (product != null && !setPredefinedValues && product.BasePriceMeasureUnit.HasValue() && !measureUnits.Exists(u => u.IsCaseInsensitiveEqual(product.BasePriceMeasureUnit)))
            {
                measureUnits.Add(product.BasePriceMeasureUnit);
            }

            foreach (var mu in measureUnits)
            {
                model.AvailableMeasureUnits.Add(new SelectListItem
                {
                    Text = mu,
                    Value = mu,
                    Selected = product != null && !setPredefinedValues && mu.Equals(product.BasePriceMeasureUnit, StringComparison.OrdinalIgnoreCase)
                });
            }

            // Specification attributes.
            var specificationAttributes = _specificationAttributeService.GetSpecificationAttributes().ToList();
            for (int i = 0; i < specificationAttributes.Count; i++)
            {
                var sa = specificationAttributes[i];
                model.AddSpecificationAttributeModel.AvailableAttributes.Add(new SelectListItem { Text = sa.Name, Value = sa.Id.ToString() });
                if (i == 0)
                {
                    // Attribute options.
                    foreach (var sao in _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttribute(sa.Id))
                    {
                        model.AddSpecificationAttributeModel.AvailableOptions.Add(new SelectListItem { Text = sao.Name, Value = sao.Id.ToString() });
                    }
                }
            }

            if (product != null && !excludeProperties)
            {
                model.SelectedDiscountIds = product.AppliedDiscounts.Select(d => d.Id).ToArray();
            }

            var inventoryMethods = ((ManageInventoryMethod[])Enum.GetValues(typeof(ManageInventoryMethod))).Where(
                x => (model.ProductTypeId == (int)ProductType.BundledProduct && x == ManageInventoryMethod.ManageStockByAttributes) ? false : true
            );

            foreach (var inventoryMethod in inventoryMethods)
            {
                model.AvailableManageInventoryMethods.Add(new SelectListItem
                {
                    Value = ((int)inventoryMethod).ToString(),
                    Text = inventoryMethod.GetLocalizedEnum(_localizationService, _workContext),
                    Selected = ((int)inventoryMethod == model.ManageInventoryMethodId)
                });
            }

            model.AvailableCountries = _countryService.GetAllCountries(true)
                .Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = product != null && x.Id == product.CountryOfOriginId
                })
                .ToList();

            if (setPredefinedValues)
            {
                model.MaximumCustomerEnteredPrice = 1000;
                model.MaxNumberOfDownloads = 10;
                model.RecurringCycleLength = 100;
                model.RecurringTotalCycles = 10;
                model.StockQuantity = 10000;
                model.NotifyAdminForQuantityBelow = 1;
                model.OrderMinimumQuantity = 1;
                model.OrderMaximumQuantity = 100;
                model.QuantityStep = 1;
                model.HideQuantityControl = false;
                model.UnlimitedDownloads = true;
                model.IsShipEnabled = true;
                model.AllowCustomerReviews = true;
                model.Published = true;
                model.HasPreviewPicture = false;
            }
        }

        private void PrepareProductPictureModel(ProductModel model)
        {
            Guard.NotNull(model, nameof(model));

            var productPictures = _productService.GetProductPicturesByProductId(model.Id);

            model.ProductMediaFiles = productPictures
                .Select(x =>
                {
                    var media = new ProductMediaFile
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,
                        MediaFileId = x.MediaFileId,
                        DisplayOrder = x.DisplayOrder,
                        MediaFile = x.MediaFile
                    };

                    return media;
                })
                .ToList();
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> query, GridCommand command)
        {
            if (command.SortDescriptors != null && command.SortDescriptors.Count > 0)
            {
                var sort = command.SortDescriptors.First();
                if (sort.Member == "Name")
                {
                    if (sort.SortDirection == ListSortDirection.Ascending)
                        query = query.OrderBy(x => x.Name);
                    else
                        query = query.OrderByDescending(x => x.Name);
                }
                else if (sort.Member == "Sku")
                {
                    if (sort.SortDirection == ListSortDirection.Ascending)
                        query = query.OrderBy(x => x.Sku);
                    else
                        query = query.OrderByDescending(x => x.Sku);
                }
                else if (sort.Member == "Price")
                {
                    if (sort.SortDirection == ListSortDirection.Ascending)
                        query = query.OrderBy(x => x.Price);
                    else
                        query = query.OrderByDescending(x => x.Price);
                }
                else if (sort.Member == "OldPrice")
                {
                    if (sort.SortDirection == ListSortDirection.Ascending)
                        query = query.OrderBy(x => x.OldPrice);
                    else
                        query = query.OrderByDescending(x => x.OldPrice);
                }
                else if (sort.Member == "StockQuantity")
                {
                    if (sort.SortDirection == ListSortDirection.Ascending)
                        query = query.OrderBy(x => x.StockQuantity);
                    else
                        query = query.OrderByDescending(x => x.StockQuantity);
                }
                else if (sort.Member == "CreatedOn")
                {
                    if (sort.SortDirection == ListSortDirection.Ascending)
                        query = query.OrderBy(x => x.CreatedOnUtc);
                    else
                        query = query.OrderByDescending(x => x.CreatedOnUtc);
                }
                else if (sort.Member == "UpdatedOn")
                {
                    if (sort.SortDirection == ListSortDirection.Ascending)
                        query = query.OrderBy(x => x.UpdatedOnUtc);
                    else
                        query = query.OrderByDescending(x => x.UpdatedOnUtc);
                }
                else if (sort.Member == "Published")
                {
                    if (sort.SortDirection == ListSortDirection.Ascending)
                        query = query.OrderBy(x => x.Published);
                    else
                        query = query.OrderByDescending(x => x.Published);
                }
                else if (sort.Member == "LimitedToStores")
                {
                    if (sort.SortDirection == ListSortDirection.Ascending)
                        query = query.OrderBy(x => x.LimitedToStores);
                    else
                        query = query.OrderByDescending(x => x.LimitedToStores);
                }
                else if (sort.Member == "ManageInventoryMethod")
                {
                    if (sort.SortDirection == ListSortDirection.Ascending)
                        query = query.OrderBy(x => x.ManageInventoryMethodId);
                    else
                        query = query.OrderByDescending(x => x.ManageInventoryMethodId);
                }
                else
                {
                    query = query.OrderBy(x => x.Name);
                }
            }
            else
            {
                query = query.OrderBy(x => x.Name);
            }

            return query;
        }

        #endregion

        #region Misc

        [HttpPost]
        public ActionResult GetBasePrice(int productId, string basePriceMeasureUnit, decimal basePriceAmount, int basePriceBaseAmount)
        {
            var product = _productService.GetProductById(productId);
            string basePrice = "";

            if (basePriceAmount != Decimal.Zero)
            {
                decimal basePriceValue = Convert.ToDecimal((product.Price / basePriceAmount) * basePriceBaseAmount);

                string basePriceFormatted = _priceFormatter.FormatPrice(basePriceValue, true, false);
                string unit = "{0} {1}".FormatWith(basePriceBaseAmount, basePriceMeasureUnit);

                basePrice = _localizationService.GetResource("Admin.Catalog.Products.Fields.BasePriceInfo").FormatWith(
                    basePriceAmount.ToString("G29") + " " + basePriceMeasureUnit, basePriceFormatted, unit);
            }

            return Json(new { Result = true, BasePrice = basePrice });
        }

        #endregion

        #region Product list / create / edit / delete

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult List(ProductListModel model)
        {
            model.DisplayProductPictures = _adminAreaSettings.DisplayProductPictures;
            model.IsSingleStoreMode = _storeService.IsSingleStoreMode();
            model.GridPageSize = _adminAreaSettings.GridPageSize;

            foreach (var c in _categoryService.GetCategoryTree(includeHidden: true).FlattenNodes(false))
            {
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetCategoryNameIndented(), Value = c.Id.ToString() });
            }

            foreach (var m in _manufacturerService.GetAllManufacturers(true))
            {
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            }

            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult ProductList(GridCommand command, ProductListModel model)
        {
            var gridModel = new GridModel<ProductModel>();

            var fields = new List<string> { "name" };
            if (_searchSettings.SearchFields.Contains("sku"))
                fields.Add("sku");
            if (_searchSettings.SearchFields.Contains("shortdescription"))
                fields.Add("shortdescription");

            var searchQuery = new CatalogSearchQuery(fields.ToArray(), model.SearchProductName)
                .HasStoreId(model.SearchStoreId)
                .WithCurrency(_workContext.WorkingCurrency)
                .WithLanguage(_workContext.WorkingLanguage);

            if (model.SearchIsPublished.HasValue)
                searchQuery = searchQuery.PublishedOnly(model.SearchIsPublished.Value);

            if (model.SearchHomePageProducts.HasValue)
                searchQuery = searchQuery.HomePageProductsOnly(model.SearchHomePageProducts.Value);

            if (model.SearchProductTypeId > 0)
                searchQuery = searchQuery.IsProductType((ProductType)model.SearchProductTypeId);

            if (model.SearchWithoutManufacturers.HasValue)
                searchQuery = searchQuery.HasAnyManufacturer(!model.SearchWithoutManufacturers.Value);
            else if (model.SearchManufacturerId != 0)
                searchQuery = searchQuery.WithManufacturerIds(null, model.SearchManufacturerId);

            if (model.SearchWithoutCategories.HasValue)
                searchQuery = searchQuery.HasAnyCategory(!model.SearchWithoutCategories.Value);
            else if (model.SearchCategoryId != 0)
                searchQuery = searchQuery.WithCategoryIds(null, model.SearchCategoryId);

            IPagedList<Product> products;

            if (_searchSettings.UseCatalogSearchInBackend)
            {
                searchQuery = searchQuery.Slice((command.Page - 1) * command.PageSize, command.PageSize);

                if (command.SortDescriptors?.Any() ?? false)
                {
                    var sort = command.SortDescriptors.First();
                    switch (sort.Member)
                    {
                        case "Name":
                            searchQuery = searchQuery.SortBy(sort.SortDirection == ListSortDirection.Ascending ? ProductSortingEnum.NameAsc : ProductSortingEnum.NameDesc);
                            break;
                        case "Price":
                            searchQuery = searchQuery.SortBy(sort.SortDirection == ListSortDirection.Ascending ? ProductSortingEnum.PriceAsc : ProductSortingEnum.PriceDesc);
                            break;
                        case "CreatedOn":
                            searchQuery = searchQuery.SortBy(sort.SortDirection == ListSortDirection.Ascending ? ProductSortingEnum.CreatedOnAsc : ProductSortingEnum.CreatedOn);
                            break;
                    }
                }

                if (!searchQuery.Sorting.Any())
                {
                    searchQuery = searchQuery.SortBy(ProductSortingEnum.NameAsc);
                }

                var searchResult = _catalogSearchService.Search(searchQuery);
                products = searchResult.Hits;
            }
            else
            {
                var query = _catalogSearchService.PrepareQuery(searchQuery);
                query = ApplySorting(query, command);

                products = new PagedList<Product>(query, command.Page - 1, command.PageSize);
            }

            var fileIds = products
                .Select(x => x.MainPictureId ?? 0)
                .Where(x => x != 0)
                .Distinct()
                .ToArray();
            var files = _mediaService.GetFilesByIds(fileIds).ToDictionarySafe(x => x.Id);

            gridModel.Data = products.Select(x =>
            {
                var productModel = new ProductModel
                {
                    Sku = x.Sku,
                    Published = x.Published,
                    ProductTypeLabelHint = x.ProductTypeLabelHint,
                    Name = x.Name,
                    Id = x.Id,
                    StockQuantity = x.StockQuantity,
                    Price = x.Price,
                    LimitedToStores = x.LimitedToStores
                };

                files.TryGetValue(x.MainPictureId ?? 0, out var file);

                productModel.PictureThumbnailUrl = _mediaService.GetUrl(file, _mediaSettings.CartThumbPictureSize);
                productModel.NoThumb = file == null;

                productModel.ProductTypeName = x.GetProductTypeLabel(_localizationService);
                productModel.UpdatedOn = _dateTimeHelper.ConvertToUserTime(x.UpdatedOnUtc, DateTimeKind.Utc);
                productModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);

                return productModel;
            });

            gridModel.Total = products.TotalCount;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-product-by-sku")]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult GoToSku(ProductListModel model)
        {
            var sku = model.GoDirectlyToSku;

            if (sku.HasValue())
            {
                var product = _productService.GetProductBySku(sku);
                if (product != null)
                {
                    return RedirectToAction("Edit", "Product", new { id = product.Id });
                }

                var combination = _productAttributeService.GetProductVariantAttributeCombinationBySku(sku);

                if (combination != null)
                {
                    if (combination.Product == null)
                    {
                        NotifyWarning(T("Products.NotFound", combination.ProductId));
                    }
                    else if (combination.Product.Deleted)
                    {
                        NotifyWarning(T("Products.Deleted", combination.ProductId));
                    }
                    else
                    {
                        return RedirectToAction("Edit", "Product", new { id = combination.Product.Id });
                    }
                }
            }

            // Not found.
            return List(model);
        }

        [Permission(Permissions.Catalog.Product.Create)]
        public ActionResult Create()
        {
            var model = new ProductModel();
            PrepareProductModel(model, null, true, true);
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.Create)]
        public ActionResult Create(ProductModel model, bool continueEditing, FormCollection form)
        {
            if (model.DownloadFileVersion.HasValue() && model.DownloadId != null)
            {
                try
                {
                    var test = new SemanticVersion(model.DownloadFileVersion).ToString();
                }
                catch
                {
                    ModelState.AddModelError("FileVersion", T("Admin.Catalog.Products.Download.SemanticVersion.NotValid"));
                }
            }

            if (ModelState.IsValid)
            {
                var product = new Product();

                MapModelToProduct(model, product, form, out _);

                product.StockQuantity = 10000;
                product.OrderMinimumQuantity = 1;
                product.OrderMaximumQuantity = 100;
                product.HideQuantityControl = false;
                product.IsShipEnabled = true;
                product.AllowCustomerReviews = true;
                product.Published = true;
                product.MaximumCustomerEnteredPrice = 1000;

                if (product.ProductType == ProductType.BundledProduct)
                {
                    product.BundleTitleText = T("Products.Bundle.BundleIncludes");
                }

                _productService.InsertProduct(product);

                UpdateDataOfExistingProduct(product, model, false, false);

                _productService.UpdateHasDiscountsApplied(product);

                _customerActivityService.InsertActivity("AddNewProduct", T("ActivityLog.AddNewProduct"), product.Name);

                if (continueEditing)
                {
                    // ensure that the same tab gets selected in edit view
                    var selectedTab = TempData["SelectedTab.product-edit"] as SmartStore.Web.Framework.UI.SelectedTabInfo;
                    if (selectedTab != null)
                    {
                        selectedTab.Path = Url.Action("Edit", new System.Web.Routing.RouteValueDictionary { { "id", product.Id } });
                    }
                }

                NotifySuccess(T("Admin.Catalog.Products.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = product.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.
            PrepareProductModel(model, null, false, true);

            return View(model);
        }

        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult Edit(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
            {
                NotifyWarning(T("Products.NotFound", id));
                return RedirectToAction("List");
            }

            if (product.Deleted)
            {
                NotifyWarning(T("Products.Deleted", id));
                return RedirectToAction("List");
            }

            var model = product.ToModel();
            PrepareProductModel(model, product, false, false);

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = product.GetLocalized(x => x.Name, languageId, false, false);
                locale.ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, false, false);
                locale.FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false, false);
                locale.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = product.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = product.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = product.GetSeName(languageId, false, false);
                locale.BundleTitleText = product.GetLocalized(x => x.BundleTitleText, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.Update)]
        public ActionResult Edit(ProductModel model, bool continueEditing, FormCollection form)
        {
            var product = _productService.GetProductById(model.Id);
            if (product == null)
            {
                NotifyWarning(T("Products.NotFound", model.Id));
                return RedirectToAction("List");
            }

            if (product.Deleted)
            {
                NotifyWarning(T("Products.Deleted", model.Id));
                return RedirectToAction("List");
            }

            UpdateDataOfProductDownloads(model);

            if (ModelState.IsValid)
            {
                MapModelToProduct(model, product, form, out var nameChanged);
                UpdateDataOfExistingProduct(product, model, true, nameChanged);

                _productService.UpdateHasDiscountsApplied(product);

                _customerActivityService.InsertActivity("EditProduct", T("ActivityLog.EditProduct"), product.Name);

                NotifySuccess(T("Admin.Catalog.Products.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = product.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.
            PrepareProductModel(model, product, false, true);

            return View(model);
        }

        [NonAction]
        protected void UpdateDataOfProductDownloads(ProductModel model)
        {
            var testVersions = (new string[] { model.DownloadFileVersion, model.NewVersion }).Where(x => x.HasValue());
            var saved = false;
            foreach (var testVersion in testVersions)
            {
                try
                {
                    var test = new SemanticVersion(testVersion).ToString();

                    // Insert versioned downloads here so they won't be saved if version ain't correct.
                    // If NewVersionDownloadId has value
                    if (model.NewVersion.HasValue() && !saved)
                    {
                        InsertProductDownload(model.NewVersionDownloadId, model.Id, model.NewVersion);
                        saved = true;
                    }
                    else
                    {
                        InsertProductDownload(model.DownloadId, model.Id, model.DownloadFileVersion);
                    }
                }
                catch
                {
                    ModelState.AddModelError("DownloadFileVersion", T("Admin.Catalog.Products.Download.SemanticVersion.NotValid"));
                }
            }

            var isUrlDownload = Request.Form["is-url-download-" + model.SampleDownloadId] == "true";
            var setOldFileToTransient = false;

            if (model.SampleDownloadId != model.OldSampleDownloadId && model.SampleDownloadId != 0 && !isUrlDownload)
            {
                // Insert sample download if a new file was uploaded.
                model.SampleDownloadId = InsertSampleDownload(model.SampleDownloadId, model.Id);

                setOldFileToTransient = true;
            }
            else if (isUrlDownload)
            {
                var download = _downloadService.GetDownloadById((int)model.SampleDownloadId);
                download.IsTransient = false;
                _downloadService.UpdateDownload(download);

                setOldFileToTransient = true;
            }

            if (setOldFileToTransient && model.OldSampleDownloadId > 0)
            {
                var download = _downloadService.GetDownloadById((int)model.OldSampleDownloadId);
                download.IsTransient = true;
                _downloadService.UpdateDownload(download);
            }
        }

        [NonAction]
        protected void InsertProductDownload(int? fileId, int entityId, string fileVersion = "")
        {
            if (fileId > 0)
            {
                var isUrlDownload = Request.Form["is-url-download-" + fileId] == "true";

                if (!isUrlDownload)
                {
                    var mediaFileInfo = _mediaService.GetFileById((int)fileId);
                    var download = new Download
                    {
                        MediaFile = mediaFileInfo.File,
                        EntityId = entityId,
                        EntityName = "Product",
                        DownloadGuid = Guid.NewGuid(),
                        UseDownloadUrl = false,
                        DownloadUrl = string.Empty,
                        UpdatedOnUtc = DateTime.UtcNow,
                        IsTransient = false,
                        FileVersion = fileVersion
                    };

                    _downloadService.InsertDownload(download);
                }
                else
                {
                    var download = _downloadService.GetDownloadById((int)fileId);
                    download.FileVersion = fileVersion;
                    download.IsTransient = false;
                    _downloadService.UpdateDownload(download);
                }
            }
        }

        [NonAction]
        protected int? InsertSampleDownload(int? fileId, int entityId)
        {
            if (fileId > 0)
            {
                var mediaFileInfo = _mediaService.GetFileById((int)fileId);
                var download = new Download
                {
                    MediaFile = mediaFileInfo.File,
                    EntityId = entityId,
                    EntityName = "Product",
                    DownloadGuid = Guid.NewGuid(),
                    UseDownloadUrl = false,
                    DownloadUrl = string.Empty,
                    UpdatedOnUtc = DateTime.UtcNow,
                    IsTransient = false
                };

                _downloadService.InsertDownload(download);

                return download.Id;
            }

            return null;
        }


        [NonAction]
        protected void MapModelToProduct(ProductModel model, Product product, FormCollection form, out bool nameChanged)
        {
            if (model.LoadedTabs == null || model.LoadedTabs.Length == 0)
            {
                model.LoadedTabs = new string[] { "Info" };
            }

            nameChanged = false;

            foreach (var tab in model.LoadedTabs)
            {
                switch (tab.ToLowerInvariant())
                {
                    case "info":
                        UpdateProductGeneralInfo(product, model, out nameChanged);
                        break;
                    case "inventory":
                        UpdateProductInventory(product, model);
                        break;
                    case "bundleitems":
                        UpdateProductBundleItems(product, model);
                        break;
                    case "price":
                        UpdateProductPrice(product, model);
                        break;
                    case "attributes":
                        UpdateProductAttributes(product, model);
                        break;
                    case "downloads":
                        UpdateProductDownloads(product, model);
                        break;
                    case "pictures":
                        UpdateProductPictures(product, model);
                        break;
                    case "seo":
                        UpdateProductSeo(product, model);
                        break;
                }
            }

            _eventPublisher.Publish(new ModelBoundEvent(model, product, form));
        }

        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult LoadEditTab(int id, string tabName, string viewPath = null)
        {
            try
            {
                if (id == 0)
                {
                    // Is create mode.
                    return PartialView("_Create.SaveFirst");
                }

                if (tabName.IsEmpty())
                {
                    return Content("A unique tab name has to specified (route parameter: tabName)");
                }

                var product = _productService.GetProductById(id);
                var model = product.ToModel();

                PrepareProductModel(model, product, false, false);

                AddLocales(_languageService, model.Locales, (locale, languageId) =>
                {
                    locale.Name = product.GetLocalized(x => x.Name, languageId, false, false);
                    locale.ShortDescription = product.GetLocalized(x => x.ShortDescription, languageId, false, false);
                    locale.FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false, false);
                    locale.MetaKeywords = product.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = product.GetLocalized(x => x.MetaDescription, languageId, false, false);
                    locale.MetaTitle = product.GetLocalized(x => x.MetaTitle, languageId, false, false);
                    locale.SeName = product.GetSeName(languageId, false, false);
                    locale.BundleTitleText = product.GetLocalized(x => x.BundleTitleText, languageId, false, false);
                });

                return PartialView(viewPath.NullEmpty() ?? "_CreateOrUpdate." + tabName, model);
            }
            catch (Exception ex)
            {
                return Content("Error while loading template: " + ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.Delete)]
        public ActionResult Delete(int id)
        {
            var product = _productService.GetProductById(id);
            _productService.DeleteProduct(product);

            _customerActivityService.InsertActivity("DeleteProduct", T("ActivityLog.DeleteProduct"), product.Name);

            NotifySuccess(T("Admin.Catalog.Products.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.Delete)]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (selectedIds?.Any() ?? false)
            {
                var products = _productService.GetProductsByIds(selectedIds.ToArray());
                products.Each(x => _productService.DeleteProduct(x));
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.Create)]
        public ActionResult CopyProduct(ProductModel model)
        {
            var copyModel = model.CopyProductModel;
            try
            {
                Product newProduct = null;
                var product = _productService.GetProductById(copyModel.Id);

                for (var i = 1; i <= copyModel.NumberOfCopies; ++i)
                {
                    var newName = copyModel.NumberOfCopies > 1 ? $"{copyModel.Name} {i}" : copyModel.Name;
                    newProduct = _copyProductService.CopyProduct(product, newName, copyModel.Published);
                }

                if (newProduct != null)
                {
                    NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

                    return RedirectToAction("Edit", new { id = newProduct.Id });
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                NotifyError(ex.ToAllMessages());
            }

            return RedirectToAction("Edit", new { id = copyModel.Id });
        }

        #endregion

        #region Product categories

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult ProductCategoryList(GridCommand command, int productId)
        {
            var model = new GridModel<ProductModel.ProductCategoryModel>();
            var productCategories = _categoryService.GetProductCategoriesByProductId(productId, true);

            var productCategoriesModel = productCategories
                .Select(x =>
                {
                    var node = _categoryService.GetCategoryTree(x.CategoryId, true);
                    return new ProductModel.ProductCategoryModel
                    {
                        Id = x.Id,
                        Category = node != null ? _categoryService.GetCategoryPath(node, aliasPattern: "<span class='badge badge-secondary'>{0}</span>") : string.Empty,
                        ProductId = x.ProductId,
                        CategoryId = x.CategoryId,
                        IsFeaturedProduct = x.IsFeaturedProduct,
                        DisplayOrder = x.DisplayOrder,
                        IsSystemMapping = x.IsSystemMapping
                    };
                })
                .ToList();

            model.Data = productCategoriesModel;
            model.Total = productCategoriesModel.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditCategory)]
        public ActionResult ProductCategoryInsert(GridCommand command, ProductModel.ProductCategoryModel model)
        {
            var productCategory = new ProductCategory
            {
                ProductId = model.ProductId,
                CategoryId = int.Parse(model.Category), // Use Category property (not CategoryId) because appropriate property is stored in it.
                IsFeaturedProduct = model.IsFeaturedProduct,
                DisplayOrder = model.DisplayOrder
            };

            _categoryService.InsertProductCategory(productCategory);

            var mru = new TrimmedBuffer<string>(
                _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.MostRecentlyUsedCategories),
                model.Category,
                _catalogSettings.MostRecentlyUsedCategoriesMaxSize);

            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.MostRecentlyUsedCategories, mru.ToString());

            return ProductCategoryList(command, model.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditCategory)]
        public ActionResult ProductCategoryUpdate(GridCommand command, ProductModel.ProductCategoryModel model)
        {
            var productCategory = _categoryService.GetProductCategoryById(model.Id);
            var categoryChanged = int.Parse(model.Category) != productCategory.CategoryId;

            // Use Category property (not CategoryId) because appropriate property is stored in it.
            productCategory.CategoryId = int.Parse(model.Category);
            productCategory.IsFeaturedProduct = model.IsFeaturedProduct;
            productCategory.DisplayOrder = model.DisplayOrder;

            _categoryService.UpdateProductCategory(productCategory);

            if (categoryChanged)
            {
                var mru = new TrimmedBuffer<string>(
                    _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.MostRecentlyUsedCategories),
                    model.Category,
                    _catalogSettings.MostRecentlyUsedCategoriesMaxSize);

                _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.MostRecentlyUsedCategories, mru.ToString());
            }

            return ProductCategoryList(command, productCategory.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditCategory)]
        public ActionResult ProductCategoryDelete(int id, GridCommand command)
        {
            var productCategory = _categoryService.GetProductCategoryById(id);
            var productId = productCategory.ProductId;

            _categoryService.DeleteProductCategory(productCategory);

            return ProductCategoryList(command, productId);
        }

        #endregion

        #region Product manufacturers

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult ProductManufacturerList(GridCommand command, int productId)
        {
            var model = new GridModel<ProductModel.ProductManufacturerModel>();
            var productManufacturers = _manufacturerService.GetProductManufacturersByProductId(productId, true);

            var productManufacturersModel = productManufacturers
                .Select(x =>
                {
                    return new ProductModel.ProductManufacturerModel
                    {
                        Id = x.Id,
                        Manufacturer = _manufacturerService.GetManufacturerById(x.ManufacturerId).Name,
                        ProductId = x.ProductId,
                        ManufacturerId = x.ManufacturerId,
                        IsFeaturedProduct = x.IsFeaturedProduct,
                        DisplayOrder = x.DisplayOrder
                    };
                })
                .ToList();

            model.Data = productManufacturersModel;
            model.Total = productManufacturersModel.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditManufacturer)]
        public ActionResult ProductManufacturerInsert(GridCommand command, ProductModel.ProductManufacturerModel model)
        {
            var productManufacturer = new ProductManufacturer
            {
                ProductId = model.ProductId,
                ManufacturerId = int.Parse(model.Manufacturer), // Use Manufacturer property (not ManufacturerId) because appropriate property is stored in it.
                IsFeaturedProduct = model.IsFeaturedProduct,
                DisplayOrder = model.DisplayOrder
            };

            _manufacturerService.InsertProductManufacturer(productManufacturer);

            var mru = new TrimmedBuffer<string>(
                _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.MostRecentlyUsedManufacturers),
                model.Manufacturer,
                _catalogSettings.MostRecentlyUsedManufacturersMaxSize);

            _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.MostRecentlyUsedManufacturers, mru.ToString());

            return ProductManufacturerList(command, model.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditManufacturer)]
        public ActionResult ProductManufacturerUpdate(GridCommand command, ProductModel.ProductManufacturerModel model)
        {
            var productManufacturer = _manufacturerService.GetProductManufacturerById(model.Id);
            var manufacturerChanged = int.Parse(model.Manufacturer) != productManufacturer.ManufacturerId;

            // Use Manufacturer property (not ManufacturerId) because appropriate property is stored in it.
            productManufacturer.ManufacturerId = int.Parse(model.Manufacturer);
            productManufacturer.IsFeaturedProduct = model.IsFeaturedProduct;
            productManufacturer.DisplayOrder = model.DisplayOrder;

            _manufacturerService.UpdateProductManufacturer(productManufacturer);

            if (manufacturerChanged)
            {
                var mru = new TrimmedBuffer<string>(
                    _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.MostRecentlyUsedManufacturers),
                    model.Manufacturer,
                    _catalogSettings.MostRecentlyUsedManufacturersMaxSize);

                _genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer, SystemCustomerAttributeNames.MostRecentlyUsedManufacturers, mru.ToString());
            }

            return ProductManufacturerList(command, productManufacturer.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditManufacturer)]
        public ActionResult ProductManufacturerDelete(int id, GridCommand command)
        {
            var productManufacturer = _manufacturerService.GetProductManufacturerById(id);
            var productId = productManufacturer.ProductId;

            _manufacturerService.DeleteProductManufacturer(productManufacturer);

            return ProductManufacturerList(command, productId);
        }

        #endregion

        #region Related products

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult RelatedProductList(GridCommand command, int productId)
        {
            var model = new GridModel<ProductModel.RelatedProductModel>();
            var relatedProducts = _productService.GetRelatedProductsByProductId1(productId, true);

            var relatedProductsModel = relatedProducts
                .Select(x =>
                {
                    var product2 = _productService.GetProductById(x.ProductId2);

                    return new ProductModel.RelatedProductModel()
                    {
                        Id = x.Id,
                        ProductId2 = x.ProductId2,
                        Product2Name = product2.Name,
                        ProductTypeName = product2.GetProductTypeLabel(_localizationService),
                        ProductTypeLabelHint = product2.ProductTypeLabelHint,
                        DisplayOrder = x.DisplayOrder,
                        Product2Sku = product2.Sku,
                        Product2Published = product2.Published
                    };
                })
                .ToList();

            model.Data = relatedProductsModel;
            model.Total = relatedProductsModel.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditPromotion)]
        public ActionResult RelatedProductUpdate(GridCommand command, ProductModel.RelatedProductModel model)
        {
            var relatedProduct = _productService.GetRelatedProductById(model.Id);
            relatedProduct.DisplayOrder = model.DisplayOrder;

            _productService.UpdateRelatedProduct(relatedProduct);

            return RelatedProductList(command, relatedProduct.ProductId1);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditPromotion)]
        public ActionResult RelatedProductDelete(int id, GridCommand command)
        {
            var relatedProduct = _productService.GetRelatedProductById(id);
            var productId = relatedProduct.ProductId1;

            _productService.DeleteRelatedProduct(relatedProduct);

            return RelatedProductList(command, productId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditPromotion)]
        public ActionResult RelatedProductAdd(int productId, int[] selectedProductIds)
        {
            var products = _productService.GetProductsByIds(selectedProductIds);
            RelatedProduct relation = null;
            var maxDisplayOrder = -1;

            foreach (var product in products)
            {
                var existingRelations = _productService.GetRelatedProductsByProductId1(productId);

                if (existingRelations.FindRelatedProduct(productId, product.Id) == null)
                {
                    if (maxDisplayOrder == -1 && (relation = existingRelations.OrderByDescending(x => x.DisplayOrder).FirstOrDefault()) != null)
                    {
                        maxDisplayOrder = relation.DisplayOrder;
                    }

                    _productService.InsertRelatedProduct(new RelatedProduct
                    {
                        ProductId1 = productId,
                        ProductId2 = product.Id,
                        DisplayOrder = ++maxDisplayOrder
                    });
                }
            }

            return new EmptyResult();
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditPromotion)]
        public ActionResult CreateAllMutuallyRelatedProducts(int productId)
        {
            string message = null;
            var product = _productService.GetProductById(productId);
            if (product != null)
            {
                var count = _productService.EnsureMutuallyRelatedProducts(productId);
                message = T("Admin.Common.CreateMutuallyAssociationsResult", count);
            }

            return new JsonResult
            {
                Data = new { Message = message }
            };
        }

        #endregion

        #region Cross-sell products

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult CrossSellProductList(GridCommand command, int productId)
        {
            var model = new GridModel<ProductModel.CrossSellProductModel>();
            var crossSellProducts = _productService.GetCrossSellProductsByProductId1(productId, true);

            var crossSellProductsModel = crossSellProducts
                .Select(x =>
                {
                    var product2 = _productService.GetProductById(x.ProductId2);

                    return new ProductModel.CrossSellProductModel
                    {
                        Id = x.Id,
                        ProductId2 = x.ProductId2,
                        Product2Name = product2.Name,
                        ProductTypeName = product2.GetProductTypeLabel(_localizationService),
                        ProductTypeLabelHint = product2.ProductTypeLabelHint,
                        Product2Sku = product2.Sku,
                        Product2Published = product2.Published
                    };
                })
                .ToList();

            model.Data = crossSellProductsModel;
            model.Total = crossSellProductsModel.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditPromotion)]
        public ActionResult CrossSellProductDelete(int id, GridCommand command)
        {
            var crossSellProduct = _productService.GetCrossSellProductById(id);
            var productId = crossSellProduct.ProductId1;

            _productService.DeleteCrossSellProduct(crossSellProduct);

            return CrossSellProductList(command, productId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditPromotion)]
        public ActionResult CrossSellProductAdd(int productId, int[] selectedProductIds)
        {
            var products = _productService.GetProductsByIds(selectedProductIds);

            foreach (var product in products)
            {
                var existingRelations = _productService.GetCrossSellProductsByProductId1(productId);

                if (existingRelations.FindCrossSellProduct(productId, product.Id) == null)
                {
                    _productService.InsertCrossSellProduct(new CrossSellProduct
                    {
                        ProductId1 = productId,
                        ProductId2 = product.Id
                    });
                }
            }

            return new EmptyResult();
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditPromotion)]
        public ActionResult CreateAllMutuallyCrossSellProducts(int productId)
        {
            string message = null;
            var product = _productService.GetProductById(productId);
            if (product != null)
            {
                var count = _productService.EnsureMutuallyCrossSellProducts(productId);
                message = T("Admin.Common.CreateMutuallyAssociationsResult", count);
            }

            return new JsonResult
            {
                Data = new { Message = message }
            };
        }

        #endregion

        #region Associated products

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult AssociatedProductList(GridCommand command, int productId)
        {
            var model = new GridModel<ProductModel.AssociatedProductModel>();
            var searchQuery = new CatalogSearchQuery()
                .HasParentGroupedProduct(productId);

            var query = _catalogSearchService.PrepareQuery(searchQuery);
            var associatedProducts = query.OrderBy(p => p.DisplayOrder).ToList();

            var associatedProductsModel = associatedProducts.Select(x =>
            {
                return new ProductModel.AssociatedProductModel
                {
                    Id = x.Id,
                    ProductName = x.Name,
                    ProductTypeName = x.GetProductTypeLabel(_localizationService),
                    ProductTypeLabelHint = x.ProductTypeLabelHint,
                    DisplayOrder = x.DisplayOrder,
                    Sku = x.Sku,
                    Published = x.Published
                };
            })
            .ToList();

            model.Data = associatedProductsModel;
            model.Total = associatedProductsModel.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditAssociatedProduct)]
        public ActionResult AssociatedProductUpdate(GridCommand command, ProductModel.AssociatedProductModel model)
        {
            var associatedProduct = _productService.GetProductById(model.Id);
            associatedProduct.DisplayOrder = model.DisplayOrder;

            _productService.UpdateProduct(associatedProduct);

            return AssociatedProductList(command, associatedProduct.ParentGroupedProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditAssociatedProduct)]
        public ActionResult AssociatedProductDelete(int id, GridCommand command)
        {
            var product = _productService.GetProductById(id);
            var originalParentGroupedProductId = product.ParentGroupedProductId;
            product.ParentGroupedProductId = 0;

            _productService.UpdateProduct(product);

            return AssociatedProductList(command, originalParentGroupedProductId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditAssociatedProduct)]
        public ActionResult AssociatedProductAdd(int productId, int[] selectedProductIds)
        {
            var searchQuery = new CatalogSearchQuery()
                .HasParentGroupedProduct(productId);

            var query = _catalogSearchService.PrepareQuery(searchQuery);
            var maxDisplayOrder = query
                .Select(x => x.DisplayOrder)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            var products = _productService.GetProductsByIds(selectedProductIds);
            foreach (var product in products)
            {
                if (product.ParentGroupedProductId != productId)
                {
                    product.ParentGroupedProductId = productId;
                    product.DisplayOrder = ++maxDisplayOrder;

                    _productService.UpdateProduct(product);
                }
            }

            return new EmptyResult();
        }

        #endregion

        #region Bundle items

        private void PrepareBundleItemEditModel(ProductBundleItemModel model, ProductBundleItem bundleItem, string btnId, string formId, bool refreshPage = false)
        {
            ViewBag.BtnId = btnId;
            ViewBag.FormId = formId;
            ViewBag.RefreshPage = refreshPage;

            if (bundleItem == null)
            {
                ViewBag.Title = _localizationService.GetResource("Admin.Catalog.Products.BundleItems.EditOf");
                return;
            }

            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(bundleItem.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(bundleItem.UpdatedOnUtc, DateTimeKind.Utc);
            model.IsPerItemPricing = bundleItem.BundleProduct.BundlePerItemPricing;

            if (model.Locales.Count == 0)
            {
                AddLocales(_languageService, model.Locales, (locale, languageId) =>
                {
                    locale.Name = bundleItem.GetLocalized(x => x.Name, languageId, false, false);
                    locale.ShortDescription = bundleItem.GetLocalized(x => x.ShortDescription, languageId, false, false);
                });
            }

            ViewBag.Title = "{0} {1} ({2})".FormatWith(
                _localizationService.GetResource("Admin.Catalog.Products.BundleItems.EditOf"), bundleItem.Product.Name, bundleItem.Product.Sku);

            var attributes = _productAttributeService.GetProductVariantAttributesByProductId(bundleItem.ProductId);

            foreach (var attribute in attributes)
            {
                var attributeModel = new ProductBundleItemAttributeModel()
                {
                    Id = attribute.Id,
                    Name = (attribute.ProductAttribute.Alias.HasValue() ?
                        "{0} ({1})".FormatWith(attribute.ProductAttribute.Name, attribute.ProductAttribute.Alias) : attribute.ProductAttribute.Name)
                };

                var attributeValues = _productAttributeService.GetProductVariantAttributeValues(attribute.Id);

                foreach (var attributeValue in attributeValues)
                {
                    var filteredValue = bundleItem.AttributeFilters.FirstOrDefault(x => x.AttributeId == attribute.Id && x.AttributeValueId == attributeValue.Id);

                    attributeModel.Values.Add(new SelectListItem()
                    {
                        Text = attributeValue.Name,
                        Value = attributeValue.Id.ToString(),
                        Selected = (filteredValue != null)
                    });

                    if (filteredValue != null)
                    {
                        attributeModel.PreSelect.Add(new SelectListItem()
                        {
                            Text = attributeValue.Name,
                            Value = attributeValue.Id.ToString(),
                            Selected = filteredValue.IsPreSelected
                        });
                    }
                }

                if (attributeModel.Values.Count > 0)
                {
                    if (attributeModel.PreSelect.Count > 0)
                        attributeModel.PreSelect.Insert(0, new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.PleaseSelect") });

                    model.Attributes.Add(attributeModel);
                }
            }
        }
        private void SaveFilteredAttributes(ProductBundleItem bundleItem, FormCollection form)
        {
            _productAttributeService.DeleteProductBundleItemAttributeFilter(bundleItem);

            var allFilterKeys = form.AllKeys.Where(x => x.HasValue() && x.StartsWith(ProductBundleItemAttributeModel.AttributeControlPrefix));

            foreach (var key in allFilterKeys)
            {
                int attributeId = key.Substring(ProductBundleItemAttributeModel.AttributeControlPrefix.Length).ToInt();
                string preSelectId = form[ProductBundleItemAttributeModel.PreSelectControlPrefix + attributeId.ToString()].EmptyNull();

                foreach (var valueId in form[key].SplitSafe(","))
                {
                    var attributeFilter = new ProductBundleItemAttributeFilter()
                    {
                        BundleItemId = bundleItem.Id,
                        AttributeId = attributeId,
                        AttributeValueId = valueId.ToInt(),
                        IsPreSelected = (preSelectId == valueId)
                    };

                    _productAttributeService.InsertProductBundleItemAttributeFilter(attributeFilter);
                }
            }
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult BundleItemList(GridCommand command, int productId)
        {
            var model = new GridModel<ProductModel.BundleItemModel>();
            var bundleItems = _productService.GetBundleItems(productId, true).Select(x => x.Item);

            var bundleItemsModel = bundleItems.Select(x =>
            {
                return new ProductModel.BundleItemModel
                {
                    Id = x.Id,
                    ProductId = x.Product.Id,
                    ProductName = x.Product.Name,
                    ProductTypeName = x.Product.GetProductTypeLabel(_localizationService),
                    ProductTypeLabelHint = x.Product.ProductTypeLabelHint,
                    Sku = x.Product.Sku,
                    Quantity = x.Quantity,
                    Discount = x.Discount,
                    DisplayOrder = x.DisplayOrder,
                    Visible = x.Visible,
                    Published = x.Published
                };
            }).ToList();

            model.Data = bundleItemsModel;
            model.Total = bundleItemsModel.Count;

            return new JsonResult { Data = model };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditBundle)]
        public ActionResult BundleItemDelete(int id, GridCommand command)
        {
            var bundleItem = _productService.GetBundleItemById(id);

            _productService.DeleteBundleItem(bundleItem);

            return BundleItemList(command, bundleItem.BundleProductId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditBundle)]
        public ActionResult BundleItemAdd(int productId, int[] selectedProductIds)
        {
            var utcNow = DateTime.UtcNow;
            var products = _productService.GetProductsByIds(selectedProductIds);

            var maxDisplayOrder = _productService.GetBundleItems(productId, true)
                .OrderByDescending(x => x.Item.DisplayOrder)
                .Select(x => x.Item.DisplayOrder)
                .FirstOrDefault();

            foreach (var product in products.Where(x => x.CanBeBundleItem()))
            {
                var attributes = _productAttributeService.GetProductVariantAttributesByProductId(product.Id);

                if (attributes.Count > 0 && attributes.Any(a => a.ProductVariantAttributeValues.Any(v => v.ValueType == ProductVariantAttributeValueType.ProductLinkage)))
                {
                    NotifyError(T("Admin.Catalog.Products.BundleItems.NoAttributeWithProductLinkage"));
                }
                else
                {
                    var bundleItem = new ProductBundleItem
                    {
                        ProductId = product.Id,
                        BundleProductId = productId,
                        Quantity = 1,
                        Visible = true,
                        Published = true,
                        DisplayOrder = ++maxDisplayOrder
                    };

                    _productService.InsertBundleItem(bundleItem);
                }
            }

            return new EmptyResult();
        }

        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult BundleItemEditPopup(int id, string btnId, string formId)
        {
            var bundleItem = _productService.GetBundleItemById(id);
            if (bundleItem == null)
            {
                throw new ArgumentException("No bundle item found with the specified id");
            }

            var model = bundleItem.ToModel();
            PrepareBundleItemEditModel(model, bundleItem, btnId, formId);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditBundle)]
        public ActionResult BundleItemEditPopup(string btnId, string formId, bool continueEditing, ProductBundleItemModel model, FormCollection form)
        {
            ViewBag.CloseWindow = !continueEditing;

            if (ModelState.IsValid)
            {
                var bundleItem = _productService.GetBundleItemById(model.Id);
                if (bundleItem == null)
                {
                    throw new ArgumentException("No bundle item found with the specified id");
                }

                bundleItem = model.ToEntity(bundleItem);

                _productService.UpdateBundleItem(bundleItem);

                foreach (var localized in model.Locales)
                {
                    _localizedEntityService.SaveLocalizedValue(bundleItem, x => x.Name, localized.Name, localized.LanguageId);
                    _localizedEntityService.SaveLocalizedValue(bundleItem, x => x.ShortDescription, localized.ShortDescription, localized.LanguageId);
                }

                if (bundleItem.FilterAttributes)
                {
                    // Only update filters if attribute filtering is activated to reduce payload.
                    SaveFilteredAttributes(bundleItem, form);
                }

                PrepareBundleItemEditModel(model, bundleItem, btnId, formId, true);

                if (continueEditing)
                {
                    NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));
                }
            }
            else
            {
                PrepareBundleItemEditModel(model, null, btnId, formId);
            }
            return View(model);
        }

        #endregion

        #region Product pictures

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SortPictures(string pictures, int entityId)
        {
            var response = new List<dynamic>();

            try
            {
                using (var scope = new DbContextScope(ctx: Services.DbContext, validateOnSave: false, autoDetectChanges: false, autoCommit: false))
                {
                    var files = _productService.GetProductPicturesByProductId(entityId).ToList();
                    var pictureIds = new HashSet<int>(pictures.SplitSafe(",").Select(x => Convert.ToInt32(x)));
                    var ordinal = 5;

                    foreach (var id in pictureIds)
                    {
                        var productPicture = files.Where(x => x.Id == id).FirstOrDefault();
                        if (productPicture != null)
                        {
                            productPicture.DisplayOrder = ordinal;
                            _productService.UpdateProductPicture(productPicture);

                            // Add all relevant data of product picture to response.
                            dynamic file = new
                            {
                                DisplayOrder = productPicture.DisplayOrder,
                                MediaFileId = productPicture.MediaFileId,
                                EntityMediaId = productPicture.Id
                            };

                            response.Add(file);
                        }
                        ordinal += 5;
                    }

                    scope.Commit();
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex.Message);
                return new HttpStatusCodeResult(501, ex.Message);
            }

            return Json(new { success = true, response }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditPicture)]
        public ActionResult ProductMediaFilesAdd(string mediaFileIds, int entityId)
        {
            var ids = mediaFileIds
                .ToIntArray()
                .Distinct()
                .ToArray();

            if (!ids.Any())
            {
                throw new ArgumentException("Missing picture identifiers.");
            }

            var success = false;
            var product = _productService.GetProductById(entityId);
            if (product == null)
            {
                throw new ArgumentException(T("Products.NotFound", entityId));
            }

            var response = new List<dynamic>();
            var existingFiles = product.ProductPictures.Select(x => x.MediaFileId).ToList();
            var files = _mediaService.GetFilesByIds(ids, MediaLoadFlags.AsNoTracking).ToDictionary(x => x.Id);

            foreach (var id in ids)
            {
                var exists = existingFiles.Contains(id);

                // No duplicate assignments!
                if (!exists)
                {
                    var productPicture = new ProductMediaFile
                    {
                        MediaFileId = id,
                        ProductId = entityId
                    };

                    _productService.InsertProductPicture(productPicture);

                    files.TryGetValue(id, out var file);

                    success = true;

                    dynamic respObj = new
                    {
                        MediaFileId = id,
                        ProductMediaFileId = productPicture.Id,
                        file?.Name
                    };

                    response.Add(respObj);
                }
            }

            return Json(new
            {
                success,
                response,
                message = T("Admin.Product.Picture.Added").JsText.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditPicture)]
        public ActionResult ProductPictureDelete(int id)
        {
            var productPicture = _productService.GetProductPictureById(id);
            if (productPicture != null)
            {
                _productService.DeleteProductPicture(productPicture);
            }

            // TODO: (mm) (mc) OPTIONALLY delete file!
            //var file = _mediaService.GetFileById(productPicture.MediaFileId);
            //if (file != null)
            //{
            //    _mediaService.DeleteFile(file.File, true);
            //}

            NotifySuccess(T("Admin.Catalog.Products.ProductPictures.Delete.Success"));
            return new HttpStatusCodeResult(200);
        }

        #endregion

        #region Product specification attributes

        [Permission(Permissions.Catalog.Product.EditAttribute)]
        public ActionResult ProductSpecificationAttributeAdd(
            int specificationAttributeOptionId,
            bool? allowFiltering,
            bool? showOnProductPage,
            int displayOrder,
            int productId)
        {
            var success = false;
            var message = string.Empty;

            var psa = new ProductSpecificationAttribute
            {
                SpecificationAttributeOptionId = specificationAttributeOptionId,
                ProductId = productId,
                AllowFiltering = allowFiltering,
                ShowOnProductPage = showOnProductPage,
                DisplayOrder = displayOrder,
            };

            try
            {
                _specificationAttributeService.InsertProductSpecificationAttribute(psa);
                success = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return Json(new { success, message });
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult ProductSpecAttrList(GridCommand command, int productId)
        {
            var model = new GridModel<ProductSpecificationAttributeModel>();
            var productSpecAttributes = _specificationAttributeService.GetProductSpecificationAttributesByProductId(productId);
            var specAttributeIds = productSpecAttributes.Select(x => x.SpecificationAttributeOption.SpecificationAttributeId).ToArray();
            var specOptions = _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeIds(specAttributeIds);

            var productSpecModel = productSpecAttributes
                .Select(x =>
                {
                    var attributeId = x.SpecificationAttributeOption.SpecificationAttributeId;
                    var psaModel = new ProductSpecificationAttributeModel
                    {
                        Id = x.Id,
                        SpecificationAttributeName = x.SpecificationAttributeOption.SpecificationAttribute.Name,
                        SpecificationAttributeOptionName = x.SpecificationAttributeOption.Name,
                        SpecificationAttributeId = attributeId,
                        SpecificationAttributeOptionId = x.SpecificationAttributeOptionId,
                        AllowFiltering = x.AllowFiltering,
                        ShowOnProductPage = x.ShowOnProductPage,
                        DisplayOrder = x.DisplayOrder
                    };

                    if (specOptions.ContainsKey(attributeId))
                    {
                        psaModel.SpecificationAttributeOptions = specOptions[attributeId]
                            .Select(y => new ProductSpecificationAttributeModel.SpecificationAttributeOption
                            {
                                id = y.Id,
                                name = y.Name,
                                text = y.Name
                            })
                            .ToList();

                        psaModel.SpecificationAttributeOptionsUrl = Url.Action("GetOptionsByAttributeId", "SpecificationAttribute", new { attributeId = attributeId });
                    }

                    return psaModel;
                })
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.SpecificationAttributeId)
                .ToList();

            model.Data = productSpecModel;
            model.Total = productSpecModel.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditAttribute)]
        public ActionResult ProductSpecAttrUpdate(int psaId, ProductSpecificationAttributeModel model, GridCommand command)
        {
            var psa = _specificationAttributeService.GetProductSpecificationAttributeById(psaId);
            psa.AllowFiltering = model.AllowFiltering;
            psa.ShowOnProductPage = model.ShowOnProductPage;
            psa.DisplayOrder = model.DisplayOrder;

            _specificationAttributeService.UpdateProductSpecificationAttribute(psa);

            return ProductSpecAttrList(command, psa.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditAttribute)]
        public ActionResult ProductSpecAttrDelete(int psaId, GridCommand command)
        {
            var psa = _specificationAttributeService.GetProductSpecificationAttributeById(psaId);
            var productId = psa.ProductId;

            _specificationAttributeService.DeleteProductSpecificationAttribute(psa);

            return ProductSpecAttrList(command, productId);
        }

        #endregion

        #region Product tags

        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult ProductTags()
        {
            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult ProductTags(GridCommand command)
        {
            var model = new GridModel<ProductTagModel>();

            var tags = _productTagService.GetAllProductTags(true)
                .OrderByDescending(x => _productTagService.GetProductCount(x.Id, 0, true))
                .Select(x =>
                {
                    return new ProductTagModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Published = x.Published,
                        ProductCount = _productTagService.GetProductCount(x.Id, 0, true)
                    };
                })
                .ForCommand(command)
                .ToList();

            model.Data = tags.PagedForCommand(command);
            model.Total = tags.Count();

            return new JsonResult
            {
                Data = model
            };
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditTag)]
        public ActionResult ProductTagDelete(int id, GridCommand command)
        {
            var tag = _productTagService.GetProductTagById(id);

            _productTagService.DeleteProductTag(tag);

            return ProductTags(command);
        }

        [Permission(Permissions.Catalog.Product.EditTag)]
        public ActionResult EditProductTag(int id)
        {
            var productTag = _productTagService.GetProductTagById(id);
            if (productTag == null)
            {
                return HttpNotFound();
            }

            var model = new ProductTagModel
            {
                Id = productTag.Id,
                Name = productTag.Name,
                Published = productTag.Published,
                ProductCount = _productTagService.GetProductCount(productTag.Id, 0, true)
            };

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = productTag.GetLocalized(x => x.Name, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditTag)]
        public ActionResult EditProductTag(string btnId, string formId, ProductTagModel model)
        {
            var productTag = _productTagService.GetProductTagById(model.Id);
            if (productTag == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                productTag.Name = model.Name;
                productTag.Published = model.Published;

                _productTagService.UpdateProductTag(productTag);

                UpdateLocales(productTag, model);

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
            }

            return View(model);
        }

        #endregion

        #region Low stock reports

        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult LowStockReport()
        {
            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult LowStockReportList(GridCommand command)
        {
            var model = new GridModel<ProductModel>();
            var allProducts = _productService.GetLowStockProducts();

            model.Data = allProducts.PagedForCommand(command).Select(x =>
            {
                var productModel = x.ToModel();
                productModel.ProductTypeName = x.GetProductTypeLabel(_localizationService);

                return productModel;
            });

            model.Total = allProducts.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        #endregion

        #region Bulk editing

        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult BulkEdit()
        {
            var allStores = _services.StoreService.GetAllStores();
            var model = new BulkEditListModel
            {
                GridPageSize = _adminAreaSettings.GridPageSize
            };

            foreach (var c in _categoryService.GetCategoryTree(includeHidden: true).FlattenNodes(false))
            {
                model.AvailableCategories.Add(new SelectListItem { Text = c.GetCategoryNameIndented(), Value = c.Id.ToString() });
            }

            foreach (var m in _manufacturerService.GetAllManufacturers(true))
            {
                model.AvailableManufacturers.Add(new SelectListItem { Text = m.Name, Value = m.Id.ToString() });
            }

            foreach (var s in allStores)
            {
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            }

            model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult BulkEditSelect(GridCommand command, BulkEditListModel model)
        {
            var gridModel = new GridModel<BulkEditProductModel>();

            var fields = new List<string> { "name" };
            if (_searchSettings.SearchFields.Contains("sku"))
                fields.Add("sku");
            if (_searchSettings.SearchFields.Contains("shortdescription"))
                fields.Add("shortdescription");

            var searchQuery = new CatalogSearchQuery(fields.ToArray(), model.SearchProductName)
                .HasStoreId(model.SearchStoreId)
                .WithLanguage(_workContext.WorkingLanguage);

            if (model.SearchProductTypeId > 0)
                searchQuery = searchQuery.IsProductType((ProductType)model.SearchProductTypeId);

            if (model.SearchManufacturerId != 0)
                searchQuery = searchQuery.WithManufacturerIds(null, model.SearchManufacturerId);

            if (model.SearchCategoryId != 0)
                searchQuery = searchQuery.WithCategoryIds(null, model.SearchCategoryId);

            var query = _catalogSearchService.PrepareQuery(searchQuery);
            query = ApplySorting(query, command);

            var products = new PagedList<Product>(query, command.Page - 1, command.PageSize);

            gridModel.Data = products.Select(x =>
            {
                var productModel = new BulkEditProductModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ProductTypeName = x.GetProductTypeLabel(_localizationService),
                    ProductTypeLabelHint = x.ProductTypeLabelHint,
                    Sku = x.Sku,
                    OldPrice = x.OldPrice,
                    Price = x.Price,
                    ManageInventoryMethod = x.ManageInventoryMethod.GetLocalizedEnum(_localizationService, _workContext.WorkingLanguage.Id),
                    StockQuantity = x.StockQuantity,
                    Published = x.Published
                };

                return productModel;
            });

            gridModel.Total = products.TotalCount;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Update)]
        public ActionResult BulkEditSave(GridCommand command,
            [Bind(Prefix = "updated")] IEnumerable<BulkEditProductModel> updatedProducts,
            [Bind(Prefix = "deleted")] IEnumerable<BulkEditProductModel> deletedProducts,
            BulkEditListModel model)
        {
            if (updatedProducts != null)
            {
                foreach (var pModel in updatedProducts)
                {
                    var product = _productService.GetProductById(pModel.Id);
                    if (product != null)
                    {
                        var prevStockQuantity = product.StockQuantity;

                        product.Sku = pModel.Sku;
                        product.Price = pModel.Price;
                        product.OldPrice = pModel.OldPrice;
                        product.StockQuantity = pModel.StockQuantity;
                        product.Published = pModel.Published;

                        _productService.UpdateProduct(product);

                        // back in stock notifications
                        if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                            product.BackorderMode == BackorderMode.NoBackorders &&
                            product.AllowBackInStockSubscriptions &&
                            product.StockQuantity > 0 &&
                            prevStockQuantity <= 0 &&
                            product.Published &&
                            !product.Deleted &&
                            !product.IsSystemProduct)
                        {
                            _backInStockSubscriptionService.SendNotificationsToSubscribers(product);
                        }

                        if (product.StockQuantity != prevStockQuantity && product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        {
                            _productService.AdjustInventory(product, true, 0, string.Empty);
                        }
                    }
                }
            }

            if (deletedProducts != null)
            {
                foreach (var pModel in deletedProducts)
                {
                    var product = _productService.GetProductById(pModel.Id);
                    if (product != null)
                    {
                        _productService.DeleteProduct(product);
                    }
                }
            }

            return BulkEditSelect(command, model);
        }

        #endregion

        #region Tier prices

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditTierPrice)]
        public ActionResult TierPriceList(GridCommand command, int productId)
        {
            var model = new GridModel<ProductModel.TierPriceModel>();
            var product = _productService.GetProductById(productId);

            string allRolesString = T("Admin.Catalog.Products.TierPrices.Fields.CustomerRole.AllRoles");
            string allStoresString = T("Admin.Common.StoresAll");
            string deletedString = "[{0}]".FormatInvariant(T("Admin.Common.Deleted"));

            var customerRoles = new Dictionary<int, CustomerRole>();
            var stores = new Dictionary<int, Store>();

            if (product.TierPrices.Any())
            {
                var customerRoleIds = new HashSet<int>(product.TierPrices
                    .Select(x => x.CustomerRoleId ?? 0)
                    .Where(x => x != 0));

                var customerRolesQuery = _customerService.GetAllCustomerRoles(true).SourceQuery;
                customerRoles = customerRolesQuery
                    .Where(x => customerRoleIds.Contains(x.Id))
                    .ToList()
                    .ToDictionary(x => x.Id);

                stores = _services.StoreService.GetAllStores().ToDictionary(x => x.Id);
            }

            var tierPricesModel = product.TierPrices
                .OrderBy(x => x.StoreId)
                .ThenBy(x => x.Quantity)
                .ThenBy(x => x.CustomerRoleId)
                .Select(x =>
                {
                    var tierPriceModel = new ProductModel.TierPriceModel
                    {
                        Id = x.Id,
                        StoreId = x.StoreId,
                        CustomerRoleId = x.CustomerRoleId ?? 0,
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        CalculationMethodId = (int)x.CalculationMethod,
                        Price1 = x.Price
                    };

                    switch (x.CalculationMethod)
                    {
                        case TierPriceCalculationMethod.Fixed:
                            tierPriceModel.CalculationMethod = T("Admin.Product.Price.Tierprices.Fixed").Text;
                            break;
                        case TierPriceCalculationMethod.Adjustment:
                            tierPriceModel.CalculationMethod = T("Admin.Product.Price.Tierprices.Adjustment").Text;
                            break;
                        case TierPriceCalculationMethod.Percental:
                            tierPriceModel.CalculationMethod = T("Admin.Product.Price.Tierprices.Percental").Text;
                            break;
                        default:
                            tierPriceModel.CalculationMethod = x.CalculationMethod.ToString();
                            break;
                    }

                    if (x.CustomerRoleId.HasValue)
                    {
                        customerRoles.TryGetValue(x.CustomerRoleId.Value, out var role);
                        tierPriceModel.CustomerRole = role?.Name.NullEmpty() ?? allRolesString;
                    }
                    else
                    {
                        tierPriceModel.CustomerRole = allRolesString;
                    }

                    if (x.StoreId > 0)
                    {
                        stores.TryGetValue(x.StoreId, out var store);
                        tierPriceModel.Store = store?.Name.NullEmpty() ?? deletedString;
                    }
                    else
                    {
                        tierPriceModel.Store = allStoresString;
                    }

                    return tierPriceModel;
                })
                .ToList();

            model.Data = tierPricesModel;
            model.Total = tierPricesModel.Count();

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditTierPrice)]
        public ActionResult TierPriceInsert(GridCommand command, ProductModel.TierPriceModel model)
        {
            var product = _productService.GetProductById(model.ProductId);

            var tierPrice = new TierPrice
            {
                ProductId = model.ProductId,
                // Use Store property (not Store propertyId) because appropriate property is stored in it.
                StoreId = model.Store.ToInt(),
                // Use CustomerRole property (not CustomerRoleId) because appropriate property is stored in it.
                CustomerRoleId = model.CustomerRole.IsNumeric() && int.Parse(model.CustomerRole) != 0 ? int.Parse(model.CustomerRole) : (int?)null,
                Quantity = model.Quantity,
                Price = model.Price1,
                CalculationMethod = model.CalculationMethod == null ? TierPriceCalculationMethod.Fixed : (TierPriceCalculationMethod)(int.Parse(model.CalculationMethod))
            };

            _productService.InsertTierPrice(tierPrice);

            // Update "HasTierPrices" property.
            _productService.UpdateHasTierPricesProperty(product);

            return TierPriceList(command, model.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditTierPrice)]
        public ActionResult TierPriceUpdate(GridCommand command, ProductModel.TierPriceModel model)
        {
            var tierPrice = _productService.GetTierPriceById(model.Id);

            // Use Store property (not Store propertyId) because appropriate property is stored in it.
            tierPrice.StoreId = model.Store.ToInt();
            // Use CustomerRole property (not CustomerRoleId) because appropriate property is stored in it.
            tierPrice.CustomerRoleId = model.CustomerRole.IsNumeric() && int.Parse(model.CustomerRole) != 0 ? int.Parse(model.CustomerRole) : (int?)null;
            tierPrice.Quantity = model.Quantity;
            tierPrice.Price = model.Price1;
            tierPrice.CalculationMethod = model.CalculationMethod == null ? TierPriceCalculationMethod.Fixed : (TierPriceCalculationMethod)(int.Parse(model.CalculationMethod));

            _productService.UpdateTierPrice(tierPrice);

            return TierPriceList(command, tierPrice.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditTierPrice)]
        public ActionResult TierPriceDelete(int id, GridCommand command)
        {
            var tierPrice = _productService.GetTierPriceById(id);
            var productId = tierPrice.ProductId;
            var product = _productService.GetProductById(productId);

            _productService.DeleteTierPrice(tierPrice);

            // Update "HasTierPrices" property.
            _productService.UpdateHasTierPricesProperty(product);

            return TierPriceList(command, productId);
        }

        #endregion

        #region Product variant attributes

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult ProductVariantAttributeList(GridCommand command, int productId)
        {
            var model = new GridModel<ProductModel.ProductVariantAttributeModel>();
            var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductId(productId);
            var productVariantAttributesModel = productVariantAttributes
                .Select(x =>
                {
                    var pvaModel = new ProductModel.ProductVariantAttributeModel
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,
                        ProductAttribute = _productAttributeService.GetProductAttributeById(x.ProductAttributeId).Name,
                        ProductAttributeId = x.ProductAttributeId,
                        TextPrompt = x.TextPrompt,
                        CustomData = x.CustomData,
                        IsRequired = x.IsRequired,
                        AttributeControlType = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext),
                        AttributeControlTypeId = x.AttributeControlTypeId,
                        DisplayOrder1 = x.DisplayOrder
                    };

                    if (x.ShouldHaveValues())
                    {
                        pvaModel.ValueCount = x.ProductVariantAttributeValues != null ? x.ProductVariantAttributeValues.Count : 0;
                        pvaModel.ViewEditUrl = Url.Action("EditAttributeValues", "Product", new { productVariantAttributeId = x.Id });
                        pvaModel.ViewEditText = T("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.ViewLink", pvaModel.ValueCount);

                        if (x.ProductAttribute.ProductAttributeOptionsSets.Any())
                        {
                            var optionsSets = new StringBuilder($"<option>{T("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.CopyOptions")}</option>");
                            x.ProductAttribute.ProductAttributeOptionsSets.Each(set => optionsSets.Append($"<option value=\"{set.Id}\">{set.Name}</option>"));
                            pvaModel.OptionsSets = optionsSets.ToString();
                        }
                    }

                    return pvaModel;
                })
                .ToList();

            model.Data = productVariantAttributesModel;
            model.Total = productVariantAttributesModel.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult ProductVariantAttributeInsert(GridCommand command, ProductModel.ProductVariantAttributeModel model)
        {
            var pva = new ProductVariantAttribute
            {
                ProductId = model.ProductId,
                // Use ProductAttribute property (not ProductAttributeId) because appropriate property is stored in it.
                ProductAttributeId = model.ProductAttribute.ToInt(),
                TextPrompt = model.TextPrompt,
                CustomData = model.CustomData,
                IsRequired = model.IsRequired,
                // Use AttributeControlType property (not AttributeControlTypeId) because appropriate property is stored in it.
                AttributeControlTypeId = model.AttributeControlType.ToInt((int)AttributeControlType.DropdownList),
                DisplayOrder = model.DisplayOrder1
            };

            try
            {
                if (pva.ProductAttributeId != 0)
                {
                    _productAttributeService.InsertProductVariantAttribute(pva);
                }
            }
            catch (Exception ex)
            {
                Services.Notifier.Error(ex.Message);
            }

            return ProductVariantAttributeList(command, model.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult ProductVariantAttributeUpdate(GridCommand command, ProductModel.ProductVariantAttributeModel model)
        {
            var pva = _productAttributeService.GetProductVariantAttributeById(model.Id);

            // Use ProductAttribute property (not ProductAttributeId) because appropriate property is stored in it.
            pva.ProductAttributeId = int.Parse(model.ProductAttribute);
            pva.TextPrompt = model.TextPrompt;
            pva.CustomData = model.CustomData;
            pva.IsRequired = model.IsRequired;
            // Use AttributeControlType property (not AttributeControlTypeId) because appropriate property is stored in it.
            pva.AttributeControlTypeId = int.Parse(model.AttributeControlType);
            pva.DisplayOrder = model.DisplayOrder1;

            try
            {
                _productAttributeService.UpdateProductVariantAttribute(pva);
            }
            catch (Exception ex)
            {
                NotifyError(ex.Message);
            }

            return ProductVariantAttributeList(command, pva.ProductId);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult ProductVariantAttributeDelete(int id, GridCommand command)
        {
            var pva = _productAttributeService.GetProductVariantAttributeById(id);
            var productId = pva.ProductId;

            _productAttributeService.DeleteProductVariantAttribute(pva);

            return ProductVariantAttributeList(command, productId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult CopyAttributeOptions(int productVariantAttributeId, int optionsSetId, bool deleteExistingValues)
        {
            var pva = _productAttributeService.GetProductVariantAttributeById(productVariantAttributeId);
            if (pva == null)
            {
                NotifyError(T("Products.Variants.NotFound", productVariantAttributeId));
            }
            else
            {
                try
                {
                    var numberOfCopiedOptions = _productAttributeService.CopyAttributeOptions(pva, optionsSetId, deleteExistingValues);

                    NotifySuccess(string.Concat(T("Admin.Common.TaskSuccessfullyProcessed"), " ",
                        T("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Values.NumberOfCopiedOptions", numberOfCopiedOptions)));
                }
                catch (Exception ex)
                {
                    NotifyError(ex.Message);
                }
            }

            return new JsonResult { Data = string.Empty };
        }

        #endregion

        #region Product variant attribute values

        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult EditAttributeValues(int productVariantAttributeId)
        {
            var pva = _productAttributeService.GetProductVariantAttributeById(productVariantAttributeId);
            if (pva == null)
                throw new ArgumentException(T("Products.Variants.NotFound", productVariantAttributeId));

            var product = _productService.GetProductById(pva.ProductId);
            if (product == null)
                throw new ArgumentException(T("Products.NotFound", pva.ProductId));

            var model = new ProductModel.ProductVariantAttributeValueListModel
            {
                ProductName = product.Name,
                ProductId = pva.ProductId,
                ProductVariantAttributeName = pva.ProductAttribute.Name,
                ProductVariantAttributeId = pva.Id
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult ProductAttributeValueList(int productVariantAttributeId, GridCommand command)
        {
            var gridModel = new GridModel<ProductModel.ProductVariantAttributeValueModel>();
            var pva = _productAttributeService.GetProductVariantAttributeById(productVariantAttributeId);
            var values = _productAttributeService.GetProductVariantAttributeValues(productVariantAttributeId);

            gridModel.Data = values.Select(x =>
            {
                var linkedProduct = _productService.GetProductById(x.LinkedProductId);

                var model = new ProductModel.ProductVariantAttributeValueModel
                {
                    Id = x.Id,
                    ProductVariantAttributeId = x.ProductVariantAttributeId,
                    Name = x.Name,
                    NameString = Server.HtmlEncode(x.Color.IsEmpty() ? x.Name : string.Format("{0} - {1}", x.Name, x.Color)),
                    Alias = x.Alias,
                    Color = x.Color,
                    PictureId = x.MediaFileId,
                    PriceAdjustment = x.PriceAdjustment,
                    PriceAdjustmentString = x.ValueType == ProductVariantAttributeValueType.Simple ? x.PriceAdjustment.ToString("G29") : "",
                    WeightAdjustment = x.WeightAdjustment,
                    WeightAdjustmentString = x.ValueType == ProductVariantAttributeValueType.Simple ? x.WeightAdjustment.ToString("G29") : "",
                    IsPreSelected = x.IsPreSelected,
                    DisplayOrder = x.DisplayOrder,
                    ValueTypeId = x.ValueTypeId,
                    TypeName = x.ValueType.GetLocalizedEnum(_localizationService, _workContext),
                    TypeNameClass = x.ValueType == ProductVariantAttributeValueType.ProductLinkage ? "fa fa-link mr-2" : "d-none hide hidden-xs-up",
                    LinkedProductId = x.LinkedProductId,
                    Quantity = x.Quantity
                };

                if (linkedProduct != null)
                {
                    model.LinkedProductName = linkedProduct.GetLocalized(p => p.Name);
                    model.LinkedProductTypeName = linkedProduct.GetProductTypeLabel(_localizationService);
                    model.LinkedProductTypeLabelHint = linkedProduct.ProductTypeLabelHint;

                    if (model.Quantity > 1)
                    {
                        model.QuantityInfo = " × {0}".FormatWith(model.Quantity);
                    }
                }

                return model;
            });

            gridModel.Total = values.Count();

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult ProductAttributeValueCreatePopup(int productVariantAttributeId)
        {
            var pva = _productAttributeService.GetProductVariantAttributeById(productVariantAttributeId);
            if (pva == null)
                throw new ArgumentException(T("Products.Variants.NotFound", productVariantAttributeId));

            var model = new ProductModel.ProductVariantAttributeValueModel
            {
                ProductId = pva.ProductId,
                ProductVariantAttributeId = productVariantAttributeId,
                IsListTypeAttribute = pva.IsListTypeAttribute(),
                Color = "",
                Quantity = 1
            };

            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult ProductAttributeValueCreatePopup(string btnId, string formId, ProductModel.ProductVariantAttributeValueModel model)
        {
            var pva = _productAttributeService.GetProductVariantAttributeById(model.ProductVariantAttributeId);
            if (pva == null)
            {
                return RedirectToAction("List", "Product");
            }

            if (model.ValueTypeId == (int)ProductVariantAttributeValueType.ProductLinkage)
            {
                if (_productService.IsBundleItem(pva.ProductId))
                {
                    var product = _productService.GetProductById(pva.ProductId);
                    var productName = product?.Name.NaIfEmpty();

                    ModelState.AddModelError(string.Empty, T("Admin.Catalog.Products.BundleItems.NoProductLinkageForBundleItem", productName));
                }
            }

            if (ModelState.IsValid)
            {
                var pvav = new ProductVariantAttributeValue
                {
                    ProductVariantAttributeId = model.ProductVariantAttributeId,
                    Name = model.Name,
                    Alias = model.Alias,
                    Color = model.Color,
                    MediaFileId = model.PictureId,
                    PriceAdjustment = model.PriceAdjustment,
                    WeightAdjustment = model.WeightAdjustment,
                    IsPreSelected = model.IsPreSelected,
                    DisplayOrder = model.DisplayOrder,
                    ValueTypeId = model.ValueTypeId,
                    Quantity = model.Quantity
                };

                pvav.LinkedProductId = pvav.ValueType == ProductVariantAttributeValueType.Simple ? 0 : model.LinkedProductId;

                try
                {
                    _productAttributeService.InsertProductVariantAttributeValue(pvav);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }

                try
                {
                    UpdateLocales(pvav, model);
                }
                catch { }

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            // If we got this far, something failed, redisplay form.
            return View(model);
        }

        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult ProductAttributeValueEditPopup(int id)
        {
            var pvav = _productAttributeService.GetProductVariantAttributeValueById(id);
            if (pvav == null)
            {
                return RedirectToAction("List", "Product");
            }

            var linkedProduct = _productService.GetProductById(pvav.LinkedProductId);

            var model = new ProductModel.ProductVariantAttributeValueModel
            {
                ProductId = pvav.ProductVariantAttribute.ProductId,
                ProductVariantAttributeId = pvav.ProductVariantAttributeId,
                Name = pvav.Name,
                Alias = pvav.Alias,
                Color = pvav.Color,
                PictureId = pvav.MediaFileId,
                IsListTypeAttribute = pvav.ProductVariantAttribute.IsListTypeAttribute(),
                PriceAdjustment = pvav.PriceAdjustment,
                WeightAdjustment = pvav.WeightAdjustment,
                IsPreSelected = pvav.IsPreSelected,
                DisplayOrder = pvav.DisplayOrder,
                ValueTypeId = pvav.ValueTypeId,
                TypeName = pvav.ValueType.GetLocalizedEnum(_localizationService, _workContext),
                TypeNameClass = (pvav.ValueType == ProductVariantAttributeValueType.ProductLinkage ? "fa fa-link mr-2" : "d-none hide hidden-xs-up"),
                LinkedProductId = pvav.LinkedProductId,
                Quantity = pvav.Quantity
            };

            if (linkedProduct != null)
            {
                model.LinkedProductName = linkedProduct.GetLocalized(p => p.Name);
                model.LinkedProductTypeName = linkedProduct.GetProductTypeLabel(_localizationService);
                model.LinkedProductTypeLabelHint = linkedProduct.ProductTypeLabelHint;

                if (model.Quantity > 1)
                {
                    model.QuantityInfo = " × {0}".FormatWith(model.Quantity);
                }
            }

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = pvav.GetLocalized(x => x.Name, languageId, false, false);
                locale.Alias = pvav.GetLocalized(x => x.Alias, languageId, false, false);
            });

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult ProductAttributeValueEditPopup(string btnId, string formId, ProductModel.ProductVariantAttributeValueModel model)
        {
            var pvav = _productAttributeService.GetProductVariantAttributeValueById(model.Id);
            if (pvav == null)
            {
                return RedirectToAction("List", "Product");
            }

            if (model.ValueTypeId == (int)ProductVariantAttributeValueType.ProductLinkage)
            {
                if (_productService.IsBundleItem(pvav.ProductVariantAttribute.ProductId))
                {
                    var product = _productService.GetProductById(pvav.ProductVariantAttribute.ProductId);
                    var productName = product?.Name.NaIfEmpty();

                    ModelState.AddModelError(string.Empty, T("Admin.Catalog.Products.BundleItems.NoProductLinkageForBundleItem", productName));
                }
            }

            if (ModelState.IsValid)
            {
                pvav.Name = model.Name;
                pvav.Alias = model.Alias;
                pvav.Color = model.Color;
                pvav.MediaFileId = model.PictureId;
                pvav.PriceAdjustment = model.PriceAdjustment;
                pvav.WeightAdjustment = model.WeightAdjustment;
                pvav.IsPreSelected = model.IsPreSelected;
                pvav.DisplayOrder = model.DisplayOrder;
                pvav.ValueTypeId = model.ValueTypeId;
                pvav.Quantity = model.Quantity;
                pvav.LinkedProductId = pvav.ValueType == ProductVariantAttributeValueType.Simple ? 0 : model.LinkedProductId;

                try
                {
                    _productAttributeService.UpdateProductVariantAttributeValue(pvav);

                    UpdateLocales(pvav, model);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                    return View(model);
                }

                ViewBag.RefreshPage = true;
                ViewBag.btnId = btnId;
                ViewBag.formId = formId;
                return View(model);
            }

            // If we got this far, something failed, redisplay form.
            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult ProductAttributeValueDelete(int pvavId, int productVariantAttributeId, GridCommand command)
        {
            var pvav = _productAttributeService.GetProductVariantAttributeValueById(pvavId);

            _productAttributeService.DeleteProductVariantAttributeValue(pvav);

            return ProductAttributeValueList(productVariantAttributeId, command);
        }

        #endregion

        #region Product variant attribute combinations

        private void PrepareProductAttributeCombinationModel(
            ProductVariantAttributeCombinationModel model,
            ProductVariantAttributeCombination entity,
            Product product, bool formatAttributes = false)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (product == null)
                throw new ArgumentNullException("variant");

            model.ProductId = product.Id;
            model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;
            model.BaseDimensionIn = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId)?.GetLocalized(x => x.Name) ?? string.Empty;

            if (entity == null)
            {
                // It's a new entity, so initialize it properly.
                model.StockQuantity = 10000;
                model.IsActive = true;
                model.AllowOutOfStockOrders = true;
            }

            if (formatAttributes && entity != null)
            {
                model.AttributesXml = _productAttributeFormatter.FormatAttributes(product, entity.AttributesXml, _workContext.CurrentCustomer, "<br />", true, true, true, false);
            }
        }
        private void PrepareVariantCombinationAttributes(ProductVariantAttributeCombinationModel model, Product product)
        {
            var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductId(product.Id);
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

        private void PrepareVariantCombinationPictures(ProductVariantAttributeCombinationModel model, Product product)
        {
            var files = _productService.GetProductPicturesByProductId(product.Id)
                .Select(x => x.MediaFile)
                .ToList();

            foreach (var file in files)
            {
                model.AssignablePictures.Add(new ProductVariantAttributeCombinationModel.PictureSelectItemModel
                {
                    Id = file.Id,
                    IsAssigned = model.AssignedPictureIds.Contains(file.Id),
                    Media = _mediaService.ConvertMediaFile(file)
                });
            }
        }
        private void PrepareViewBag(string btnId, string formId, bool refreshPage = false, bool isEdit = true)
        {
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            ViewBag.RefreshPage = refreshPage;
            ViewBag.IsEdit = isEdit;
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult ProductVariantAttributeCombinationList(GridCommand command, int productId)
        {
            var model = new GridModel<ProductVariantAttributeCombinationModel>();
            var customer = _workContext.CurrentCustomer;
            var product = _productService.GetProductById(productId);
            var allCombinations = _productAttributeService.GetAllProductVariantAttributeCombinations(product.Id, command.Page - 1, command.PageSize);
            var productUrlTitle = T("Common.OpenInShop");
            var productSeName = product.GetSeName();

            _productAttributeParser.PrefetchProductVariantAttributes(allCombinations.Select(x => x.AttributesXml));

            var productVariantAttributesModel = allCombinations.Select(x =>
            {
                var pvacModel = x.ToModel();
                pvacModel.ProductId = product.Id;
                pvacModel.ProductUrlTitle = productUrlTitle;
                pvacModel.ProductUrl = _productUrlHelper.GetProductUrl(product.Id, productSeName, x.AttributesXml);
                pvacModel.AttributesXml = _productAttributeFormatter.FormatAttributes(product, x.AttributesXml, customer, "<br />", true, true, true, false);

                // Not really necessary here:
                //var warnings = _shoppingCartService.GetShoppingCartItemAttributeWarnings(
                //		customer,
                //		ShoppingCartType.ShoppingCart,
                //		x.Product,
                //		x.AttributesXml,
                //		combination: x);

                //pvacModel.Warnings.AddRange(warnings);

                return pvacModel;
            })
            .ToList();

            model.Data = productVariantAttributesModel;
            model.Total = allCombinations.TotalCount;

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult ProductVariantAttributeCombinationDelete(int id, GridCommand command)
        {
            var pvac = _productAttributeService.GetProductVariantAttributeCombinationById(id);
            var productId = pvac.ProductId;

            _productAttributeService.DeleteProductVariantAttributeCombination(pvac);

            var product = _productService.GetProductById(productId);
            _productService.UpdateLowestAttributeCombinationPriceProperty(product);

            return ProductVariantAttributeCombinationList(command, productId);
        }

        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult AttributeCombinationCreatePopup(string btnId, string formId, int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                return RedirectToAction("List", "Product");
            }

            var model = new ProductVariantAttributeCombinationModel();

            PrepareProductAttributeCombinationModel(model, null, product);
            PrepareVariantCombinationAttributes(model, product);
            PrepareVariantCombinationPictures(model, product);
            PrepareViewBag(btnId, formId, false, false);

            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult AttributeCombinationCreatePopup(
            string btnId,
            string formId,
            int productId,
            ProductVariantAttributeCombinationModel model,
            ProductVariantQuery query)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                return RedirectToAction("List", "Product");
            }

            var warnings = new List<string>();
            var variantAttributes = _productAttributeService.GetProductVariantAttributesByProductId(product.Id);

            var attributeXml = query.CreateSelectedAttributesXml(product.Id, 0, variantAttributes, _productAttributeParser, _localizationService,
                _downloadService, _catalogSettings, this.Request, warnings);

            warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, product, attributeXml));

            if (_productAttributeParser.FindProductVariantAttributeCombination(product.Id, attributeXml) != null)
            {
                warnings.Add(_localizationService.GetResource("Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.CombiExists"));
            }

            if (warnings.Count == 0)
            {
                var combination = model.ToEntity();
                combination.AttributesXml = attributeXml;
                combination.SetAssignedMediaIds(model.AssignedPictureIds);

                _productAttributeService.InsertProductVariantAttributeCombination(combination);

                _productService.UpdateLowestAttributeCombinationPriceProperty(product);
            }

            PrepareProductAttributeCombinationModel(model, null, product);
            PrepareVariantCombinationAttributes(model, product);
            PrepareVariantCombinationPictures(model, product);
            PrepareViewBag(btnId, formId, warnings.Count == 0, false);

            if (warnings.Count > 0)
            {
                model.Warnings = warnings;
            }

            return View(model);
        }

        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult AttributeCombinationEditPopup(int id, string btnId, string formId)
        {
            var combination = _productAttributeService.GetProductVariantAttributeCombinationById(id);
            if (combination == null)
            {
                return RedirectToAction("List", "Product");
            }

            var product = _productService.GetProductById(combination.ProductId);
            if (product == null)
            {
                return RedirectToAction("List", "Product");
            }

            var model = combination.ToModel();

            PrepareProductAttributeCombinationModel(model, combination, product, true);
            PrepareVariantCombinationAttributes(model, product);
            PrepareVariantCombinationPictures(model, product);
            PrepareViewBag(btnId, formId);

            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult AttributeCombinationEditPopup(string btnId, string formId, ProductVariantAttributeCombinationModel model, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var combination = _productAttributeService.GetProductVariantAttributeCombinationById(model.Id);
                if (combination == null)
                {
                    return RedirectToAction("List", "Product");
                }

                var attributeXml = combination.AttributesXml;
                combination = model.ToEntity(combination);
                combination.AttributesXml = attributeXml;
                combination.SetAssignedMediaIds(model.AssignedPictureIds);

                _productAttributeService.UpdateProductVariantAttributeCombination(combination);

                _productService.UpdateLowestAttributeCombinationPriceProperty(combination.Product);

                PrepareViewBag(btnId, formId, true);
            }

            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult CreateAllAttributeCombinations(ProductVariantAttributeCombinationModel model, int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                throw new ArgumentException(T("Products.NotFound", productId));
            }

            _productAttributeService.CreateAllProductVariantAttributeCombinations(product);

            _productService.UpdateLowestAttributeCombinationPriceProperty(product);

            return new JsonResult { Data = "" };
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Catalog.Product.EditVariant)]
        public ActionResult DeleteAllAttributeCombinations(ProductVariantAttributeCombinationModel model, int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                throw new ArgumentException(T("Products.NotFound", productId));
            }

            _pvacRepository.DeleteAll(x => x.ProductId == product.Id);

            _productService.UpdateLowestAttributeCombinationPriceProperty(product);

            return new JsonResult { Data = "" };
        }

        [HttpPost]
        [Permission(Permissions.Catalog.Product.Read)]
        public ActionResult CombinationExistenceNote(int productId, ProductVariantQuery query)
        {
            var warnings = new List<string>();
            var attributes = _productAttributeService.GetProductVariantAttributesByProductId(productId);

            var attributeXml = query.CreateSelectedAttributesXml(productId, 0, attributes, _productAttributeParser,
                _localizationService, _downloadService, _catalogSettings, Request, warnings);

            var exists = _productAttributeParser.FindProductVariantAttributeCombination(productId, attributeXml) != null;
            if (!exists)
            {
                var product = _productService.GetProductById(productId);
                if (product != null)
                {
                    warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, product, attributeXml));
                }
            }

            if (warnings.Count > 0)
            {
                return new JsonResult
                {
                    Data = new
                    {
                        Message = warnings[0],
                        HasWarning = true
                    }
                };
            }

            return new JsonResult
            {
                Data = new
                {
                    Message = _localizationService.GetResource(exists ?
                        "Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.CombiExists" :
                        "Admin.Catalog.Products.Variants.ProductVariantAttributes.AttributeCombinations.CombiNotExists"
                    ),
                    HasWarning = exists
                }
            };
        }

        #endregion

        #region Downloads

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Media.Download.Delete)]
        public ActionResult DeleteDownloadVersion(int downloadId, int productId)
        {
            var download = _downloadService.GetDownloadById(downloadId);
            if (download == null)
                return HttpNotFound();

            _downloadService.DeleteDownload(download);
            
            return new JsonResult { 
                Data = new {
                    success = true,
                    Message = T("Admin.Common.TaskSuccessfullyProcessed").Text
                } 
            };
        }

        #endregion

        #region Hidden normalizers

        [Permission(Permissions.Catalog.Product.Update)]
        public ActionResult FixProductMainPictureIds(DateTime? ifModifiedSinceUtc = null)
        {
            var count = DataMigrator.FixProductMainPictureIds(_dbContext, ifModifiedSinceUtc);

            return Content("Fixed {0} ids.".FormatInvariant(count));
        }

        #endregion
    }
}
