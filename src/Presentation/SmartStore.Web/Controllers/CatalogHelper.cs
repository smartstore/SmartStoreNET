using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Fakes;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Cms;
using SmartStore.Services.Customers;
using SmartStore.Services.DataExchange.Export;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Search;
using SmartStore.Services.Search.Modelling;
using SmartStore.Services.Seo;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Controllers
{
    public partial class CatalogHelper
    {
        private readonly ICommonServices _services;
        private readonly IMenuService _menuService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductService _productService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IMediaService _mediaService;
        private readonly ILocalizationService _localizationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IDownloadService _downloadService;
        private readonly MediaSettings _mediaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly TaxSettings _taxSettings;
        private readonly PerformanceSettings _performanceSettings;
        private readonly IMeasureService _measureService;
        private readonly IQuantityUnitService _quantityUnitService;
        private readonly MeasureSettings _measureSettings;
        private readonly IDeliveryTimeService _deliveryTimeService;
        private readonly Lazy<IDataExporter> _dataExporter;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly ICatalogSearchQueryFactory _catalogSearchQueryFactory;
        private readonly HttpRequestBase _httpRequest;
        private readonly UrlHelper _urlHelper;
        private readonly ProductUrlHelper _productUrlHelper;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILinkResolver _linkResolver;

        public CatalogHelper(
            ICommonServices services,
            IMenuService menuService,
            IManufacturerService manufacturerService,
            IProductService productService,
            IProductTemplateService productTemplateService,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter,
            ITaxService taxService,
            ICurrencyService currencyService,
            IMediaService mediaService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            ISpecificationAttributeService specificationAttributeService,
            IDateTimeHelper dateTimeHelper,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            IDownloadService downloadService,
            MediaSettings mediaSettings,
            CatalogSettings catalogSettings,
            CustomerSettings customerSettings,
            CaptchaSettings captchaSettings,
            IMeasureService measureService,
            IQuantityUnitService quantityUnitService,
            MeasureSettings measureSettings,
            TaxSettings taxSettings,
            PerformanceSettings performanceSettings,
            IDeliveryTimeService deliveryTimeService,
            Lazy<IDataExporter> dataExporter,
            ICatalogSearchService catalogSearchService,
            ICatalogSearchQueryFactory catalogSearchQueryFactory,
            HttpRequestBase httpRequest,
            UrlHelper urlHelper,
            ProductUrlHelper productUrlHelper,
            ILocalizedEntityService localizedEntityService,
            IUrlRecordService urlRecordService,
            ILinkResolver linkResolver)
        {
            _services = services;
            _menuService = menuService;
            _manufacturerService = manufacturerService;
            _productService = productService;
            _productTemplateService = productTemplateService;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _productAttributeFormatter = productAttributeFormatter;
            _taxService = taxService;
            _currencyService = currencyService;
            _mediaService = mediaService;
            _localizationService = _services.Localization;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _specificationAttributeService = specificationAttributeService;
            _dateTimeHelper = dateTimeHelper;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _downloadService = downloadService;
            _measureService = measureService;
            _quantityUnitService = quantityUnitService;
            _measureSettings = measureSettings;
            _taxSettings = taxSettings;
            _performanceSettings = performanceSettings;
            _deliveryTimeService = deliveryTimeService;
            _mediaSettings = mediaSettings;
            _catalogSettings = catalogSettings;
            _customerSettings = customerSettings;
            _captchaSettings = captchaSettings;
            _dataExporter = dataExporter;
            _catalogSearchService = catalogSearchService;
            _catalogSearchQueryFactory = catalogSearchQueryFactory;
            _httpRequest = httpRequest;
            _urlHelper = urlHelper;
            _productUrlHelper = productUrlHelper;
            _localizedEntityService = localizedEntityService;
            _urlRecordService = urlRecordService;
            _linkResolver = linkResolver;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;
        public ILogger Logger { get; set; } = NullLogger.Instance;
        public DbQuerySettings QuerySettings { get; set; } = DbQuerySettings.Default;

        #region Category

        public IList<CategorySummaryModel> MapCategorySummaryModel(IEnumerable<Category> categories, int pictureSize)
        {
            Guard.NotNull(categories, nameof(categories));

            var fileIds = categories
                .Select(x => x.MediaFileId ?? 0)
                .Where(x => x != 0)
                .Distinct()
                .ToArray();
            var files = _mediaService.GetFilesByIds(fileIds).ToDictionarySafe(x => x.Id);

            return categories
                .Select(c =>
                {
                    var name = c.GetLocalized(y => y.Name);
                    var model = new CategorySummaryModel
                    {
                        Id = c.Id,
                        Name = name
                    };

                    _services.DisplayControl.Announce(c);

                    // Generate URL.
                    if (c.ExternalLink.HasValue())
                    {
                        var link = _linkResolver.Resolve(c.ExternalLink);
                        if (link.Status == LinkStatus.Ok)
                        {
                            model.Url = link.Link;
                        }
                    }

                    if (model.Url.IsEmpty())
                    {
                        model.Url = _urlHelper.RouteUrl("Category", new { SeName = c.GetSeName() });
                    }

                    files.TryGetValue(c.MediaFileId ?? 0, out var file);

                    model.PictureModel = new PictureModel
                    {
                        PictureId = file?.Id ?? 0,
                        Size = pictureSize,
                        ImageUrl = _mediaService.GetUrl(file, pictureSize, null, !_catalogSettings.HideCategoryDefaultPictures),
                        FullSizeImageUrl = _mediaService.GetUrl(file, 0, null, false),
                        FullSizeImageWidth = file?.Dimensions.Width,
                        FullSizeImageHeight = file?.Dimensions.Height,
                        Title = file?.File?.GetLocalized(x => x.Title)?.Value.NullEmpty() ?? string.Format(T("Media.Category.ImageLinkTitleFormat"), name),
                        AlternateText = file?.File?.GetLocalized(x => x.Alt)?.Value.NullEmpty() ?? string.Format(T("Media.Category.ImageAlternateTextFormat"), name),
                        File = file
                    };

                    _services.DisplayControl.Announce(file?.File);

                    return model;
                })
                .ToList();
        }

        public IEnumerable<int> GetChildCategoryIds(int parentCategoryId, bool deep = true)
        {
            var root = _menuService.GetRootNode("Main");
            var node = root.SelectNodeById(parentCategoryId) ?? root.SelectNode(x => x.Value.EntityId == parentCategoryId);
            if (node != null)
            {
                var children = deep ? node.Flatten(false) : node.Children.Select(x => x.Value);
                var ids = children.Select(x => x.EntityId);
                return ids;
            }

            return Enumerable.Empty<int>();
        }

        public void GetBreadcrumb(IBreadcrumb breadcrumb, ControllerContext context, Product product = null)
        {
            var menu = _menuService.GetMenu("Main");
            var currentNode = menu.ResolveCurrentNode(context);

            if (currentNode != null)
            {
                currentNode.Trail.Where(x => !x.IsRoot).Each(x => breadcrumb.Track(x.Value));
            }

            // Add trail of parent product if product has no category assigned.
            if (product != null && !(breadcrumb.Trail?.Any() ?? false))
            {
                var parentProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentProduct != null)
                {
                    var fc = new FakeController();
                    var rd = new RouteData();
                    rd.Values.Add("currentProductId", parentProduct.Id);
                    var fcc = new ControllerContext(new RequestContext(context.HttpContext, rd), fc);
                    fc.ControllerContext = fcc;

                    currentNode = menu.ResolveCurrentNode(fcc);
                    if (currentNode != null)
                    {
                        currentNode.Trail.Where(x => !x.IsRoot).Each(x => breadcrumb.Track(x.Value));
                        var parentName = parentProduct.GetLocalized(x => x.Name);

                        breadcrumb.Track(new MenuItem
                        {
                            Text = parentName,
                            Rtl = parentName.CurrentLanguage.Rtl,
                            EntityId = parentProduct.Id,
                            Url = _urlHelper.RouteUrl("Product", new { SeName = parentProduct.GetSeName() })
                        });
                    }
                }
            }
        }

        #endregion

        public ProductDetailsModel PrepareProductDetailsPageModel(
            Product product,
            ProductVariantQuery query,
            bool isAssociatedProduct = false,
            ProductBundleItemData productBundleItem = null,
            IList<ProductBundleItemData> productBundleItems = null)
        {
            Guard.NotNull(product, nameof(product));

            var customer = _services.WorkContext.CurrentCustomer;
            var store = _services.StoreContext.CurrentStore;

            using (_services.Chronometer.Step("PrepareProductDetailsPageModel"))
            {
                var model = new ProductDetailsModel
                {
                    Id = product.Id,
                    Name = product.GetLocalized(x => x.Name),
                    ShortDescription = product.GetLocalized(x => x.ShortDescription),
                    FullDescription = product.GetLocalized(x => x.FullDescription, detectEmptyHtml: true),
                    MetaKeywords = product.GetLocalized(x => x.MetaKeywords),
                    MetaDescription = product.GetLocalized(x => x.MetaDescription),
                    MetaTitle = product.GetLocalized(x => x.MetaTitle),
                    SeName = product.GetSeName(),
                    ProductType = product.ProductType,
                    VisibleIndividually = product.Visibility != ProductVisibility.Hidden,
                    ReviewCount = product.ApprovedTotalReviews,
                    DisplayAdminLink = _services.Permissions.Authorize(Permissions.System.AccessBackend, customer),
                    Condition = product.Condition,
                    ShowCondition = _catalogSettings.ShowProductCondition,
                    LocalizedCondition = product.Condition.GetLocalizedEnum(_services.Localization, _services.WorkContext),
                    ShowSku = _catalogSettings.ShowProductSku,
                    Sku = product.Sku,
                    ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                    DisplayProductReviews = _catalogSettings.ShowProductReviewsInProductDetail && product.AllowCustomerReviews,
                    ManufacturerPartNumber = product.ManufacturerPartNumber,
                    ShowGtin = _catalogSettings.ShowGtin,
                    Gtin = product.Gtin,
                    StockAvailability = product.FormatStockMessage(_localizationService),
                    HasSampleDownload = product.IsDownload && product.HasSampleDownload,
                    IsCurrentCustomerRegistered = customer.IsRegistered(),
                    IsAssociatedProduct = isAssociatedProduct,
                    CompareEnabled = !isAssociatedProduct && _catalogSettings.CompareProductsEnabled,
                    TellAFriendEnabled = !isAssociatedProduct && _catalogSettings.EmailAFriendEnabled,
                    AskQuestionEnabled = !isAssociatedProduct && _catalogSettings.AskQuestionEnabled,
                    PriceDisplayStyle = _catalogSettings.PriceDisplayStyle,
                    DisplayTextForZeroPrices = _catalogSettings.DisplayTextForZeroPrices
                };

                model.Manufacturers = _catalogSettings.ShowManufacturerInProductDetail
                    ? PrepareManufacturersOverviewModel(_manufacturerService.GetProductManufacturersByProductId(product.Id), null, _catalogSettings.ShowManufacturerPicturesInProductDetail)
                    : null;

                // Social share code.
                if (_catalogSettings.ShowShareButton && _catalogSettings.PageShareCode.HasValue())
                {
                    var shareCode = _catalogSettings.PageShareCode;
                    if (_services.WebHelper.IsCurrentConnectionSecured())
                    {
                        // Need to change the addthis link to be https linked when the page is, so that the page doesn't ask about mixed mode when viewed in https...
                        shareCode = shareCode.Replace("http://", "https://");
                    }

                    model.ProductShareCode = shareCode;
                }

                // Get gift card values from query string.
                if (product.IsGiftCard)
                {
                    model.GiftCard.RecipientName = query.GetGiftCardValue(product.Id, 0, "RecipientName");
                    model.GiftCard.RecipientEmail = query.GetGiftCardValue(product.Id, 0, "RecipientEmail");
                    model.GiftCard.SenderName = query.GetGiftCardValue(product.Id, 0, "SenderName");
                    model.GiftCard.SenderEmail = query.GetGiftCardValue(product.Id, 0, "SenderEmail");
                    model.GiftCard.Message = query.GetGiftCardValue(product.Id, 0, "Message");
                }

                // Back in stock subscriptions.
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                     product.BackorderMode == BackorderMode.NoBackorders &&
                     product.AllowBackInStockSubscriptions &&
                     product.StockQuantity <= 0)
                {
                    // Out of stock.
                    model.DisplayBackInStockSubscription = true;
                    model.BackInStockAlreadySubscribed = _backInStockSubscriptionService.FindSubscription(customer.Id, product.Id, store.Id) != null;
                }

                // Template.
                var templateCacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_TEMPLATE_MODEL_KEY, product.ProductTemplateId);
                model.ProductTemplateViewPath = _services.Cache.Get(templateCacheKey, () =>
                {
                    var template = _productTemplateService.GetProductTemplateById(product.ProductTemplateId);
                    if (template == null)
                        template = _productTemplateService.GetAllProductTemplates().FirstOrDefault();
                    return template.ViewPath;
                });

                IList<ProductBundleItemData> bundleItems = null;
                ProductVariantAttributeCombination combination = null;

                if (product.ProductType == ProductType.GroupedProduct && !isAssociatedProduct)
                {
                    // Associated products.
                    var searchQuery = new CatalogSearchQuery()
                        .VisibleOnly(customer)
                        .HasStoreId(store.Id)
                        .HasParentGroupedProduct(product.Id);

                    var associatedProducts = _catalogSearchService.Search(searchQuery).Hits;

                    foreach (var associatedProduct in associatedProducts)
                    {
                        var assciatedProductModel = PrepareProductDetailsPageModel(associatedProduct, query, true, null, null);
                        model.AssociatedProducts.Add(assciatedProductModel);
                    }
                }
                else if (product.ProductType == ProductType.BundledProduct && productBundleItem == null)
                {
                    // Bundled items.
                    bundleItems = _productService.GetBundleItems(product.Id);

                    foreach (var itemData in bundleItems.Where(x => x.Item.Product.CanBeBundleItem()))
                    {
                        var item = itemData.Item;
                        var bundledProductModel = PrepareProductDetailsPageModel(item.Product, query, false, itemData, null);

                        bundledProductModel.ShowLegalInfo = false;
                        bundledProductModel.DeliveryTimesPresentation = DeliveryTimesPresentation.None;

                        bundledProductModel.BundleItem.Id = item.Id;
                        bundledProductModel.BundleItem.Quantity = item.Quantity;
                        bundledProductModel.BundleItem.HideThumbnail = item.HideThumbnail;
                        bundledProductModel.BundleItem.Visible = item.Visible;
                        bundledProductModel.BundleItem.IsBundleItemPricing = item.BundleProduct.BundlePerItemPricing;

                        var bundleItemName = item.GetLocalized(x => x.Name);
                        if (bundleItemName.Value.HasValue())
                            bundledProductModel.Name = bundleItemName;

                        var bundleItemShortDescription = item.GetLocalized(x => x.ShortDescription);
                        if (bundleItemShortDescription.Value.HasValue())
                            bundledProductModel.ShortDescription = bundleItemShortDescription;

                        model.BundledItems.Add(bundledProductModel);
                    }
                }

                model = PrepareProductDetailModel(model, product, query, isAssociatedProduct, productBundleItem, bundleItems);

                // Action items
                {
                    if (model.HasSampleDownload)
                    {
                        model.ActionItems["sample"] = new ProductDetailsModel.ActionItemModel
                        {
                            Key = "sample",
                            Title = T("Products.DownloadSample"),
                            CssClass = "action-download-sample",
                            IconCssClass = "fa fa-download",
                            Href = _urlHelper.Action("Sample", "Download", new { productId = model.Id }),
                            IsPrimary = true,
                            PrimaryActionColor = "danger"
                        };
                    }

                    if (!model.AddToCart.DisableWishlistButton && model.ProductType != ProductType.GroupedProduct)
                    {
                        model.ActionItems["wishlist"] = new ProductDetailsModel.ActionItemModel
                        {
                            Key = "wishlist",
                            Title = T("ShoppingCart.AddToWishlist.Short"),
                            Tooltip = T("ShoppingCart.AddToWishlist"),
                            CssClass = "ajax-cart-link action-add-to-wishlist",
                            IconCssClass = "icm icm-heart",
                            Href = _urlHelper.Action("AddProduct", "ShoppingCart", new { productId = model.Id, shoppingCartTypeId = (int)ShoppingCartType.Wishlist })
                        };
                    }

                    if (model.CompareEnabled)
                    {
                        model.ActionItems["compare"] = new ProductDetailsModel.ActionItemModel
                        {
                            Key = "compare",
                            Title = T("Common.Shopbar.Compare"),
                            Tooltip = T("Products.Compare.AddToCompareList"),
                            CssClass = "action-compare ajax-cart-link",
                            IconCssClass = "icm icm-repeat",
                            Href = _urlHelper.Action("AddProductToCompare", "Catalog", new { id = model.Id })
                        };
                    }

                    if (model.AskQuestionEnabled && !model.ProductPrice.CallForPrice)
                    {
                        model.ActionItems["ask"] = new ProductDetailsModel.ActionItemModel
                        {
                            Key = "ask",
                            Title = T("Products.AskQuestion.Short"),
                            Tooltip = T("Products.AskQuestion"),
                            CssClass = "action-ask-question",
                            IconCssClass = "icm icm-envelope",
                            Href = _urlHelper.Action("AskQuestion", new { id = model.Id })
                        };
                    }

                    if (model.TellAFriendEnabled)
                    {
                        model.ActionItems["tell"] = new ProductDetailsModel.ActionItemModel
                        {
                            Key = "tell",
                            Title = T("Products.EmailAFriend"),
                            CssClass = "action-bullhorn",
                            IconCssClass = "icm icm-bullhorn",
                            Href = _urlHelper.Action("EmailAFriend", new { id = model.Id })
                        };
                    }
                }

                IList<int> combinationPictureIds = null;

                if (productBundleItem == null)
                {
                    combinationPictureIds = _productAttributeService.GetAllProductVariantAttributeCombinationPictureIds(product.Id);
                    if (combination == null && model.SelectedCombination != null)
                        combination = model.SelectedCombination;
                }

                var files = _productService.GetProductPicturesByProductId(product.Id)
                    .Select(x => _mediaService.ConvertMediaFile(x.MediaFile))
                    .ToList();

                if (product.HasPreviewPicture && files.Count > 1)
                {
                    files.RemoveAt(0);
                }

                model.MediaGalleryModel = PrepareProductDetailsMediaGalleryModel(files, model.Name, combinationPictureIds, isAssociatedProduct, productBundleItem, combination);

                return model;
            }
        }

        public void PrepareProductReviewsModel(ProductReviewsModel model, Product product, int take = int.MaxValue)
        {
            Guard.NotNull(product, nameof(product));
            Guard.NotNull(model, nameof(model));

            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();

            var query = _services.DbContext
                .QueryForCollection<Product, ProductReview>(product, x => x.ProductReviews)
                .Where(x => x.IsApproved);

            if (DataSettings.Current.IsSqlServer)
            {
                // SQCE throw NotImplementedException with .Expand()
                query = query
                    .Expand(x => x.Customer.CustomerRoleMappings.Select(rm => rm.CustomerRole))
                    .Expand(x => x.Customer.CustomerContent);
            }

            int totalCount = query.Count();
            model.TotalReviewsCount = totalCount;

            var reviews = query
                .OrderByDescending(x => x.CreatedOnUtc)
                .Take(() => take)
                .ToList();

            foreach (var review in reviews)
            {
                model.Items.Add(new ProductReviewModel
                {
                    Id = review.Id,
                    CustomerId = review.CustomerId,
                    CustomerName = review.Customer.FormatUserName(),
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles && review.Customer != null && !review.Customer.IsGuest(),
                    Title = review.Title,
                    ReviewText = review.ReviewText,
                    Rating = review.Rating,
                    Helpfulness = new ProductReviewHelpfulnessModel
                    {
                        ProductReviewId = review.Id,
                        HelpfulYesTotal = review.HelpfulYesTotal,
                        HelpfulNoTotal = review.HelpfulNoTotal,
                    },
                    WrittenOnStr = _dateTimeHelper.ConvertToUserTime(review.CreatedOnUtc, DateTimeKind.Utc).ToString("D"),
                    WrittenOn = review.CreatedOnUtc
                });
            }

            model.CanCurrentCustomerLeaveReview = _catalogSettings.AllowAnonymousUsersToReviewProduct || !_services.WorkContext.CurrentCustomer.IsGuest();
            model.DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnProductReviewPage;
        }

        private MediaFileInfo PrepareMediaFileInfo(MediaFileInfo file, MediaGalleryModel model)
        {
            file.Alt = file.File.GetLocalized(x => x.Alt)?.Value.NullEmpty() ?? model.DefaultAlt;
            file.TitleAttribute = file.File.GetLocalized(x => x.Title)?.Value.NullEmpty() ?? model.ModelName;

            _services.DisplayControl.Announce(file.File);

            // Return for chaining
            return file;
        }

        public IList<ProductDetailsModel.TierPriceModel> CreateTierPriceModel(Product product, decimal adjustment = decimal.Zero)
        {
            var model = product.TierPrices
                .OrderBy(x => x.Quantity)
                .FilterByStore(_services.StoreContext.CurrentStore.Id)
                .FilterForCustomer(_services.WorkContext.CurrentCustomer)
                .ToList()
                .RemoveDuplicatedQuantities()
                .Select(tierPrice =>
                {
                    var currentAdjustment = adjustment;
                    var m = new ProductDetailsModel.TierPriceModel
                    {
                        Quantity = tierPrice.Quantity,
                    };

                    if (currentAdjustment != 0 && tierPrice.CalculationMethod == TierPriceCalculationMethod.Percental && _catalogSettings.ApplyTierPricePercentageToAttributePriceAdjustments)
                    {
                        currentAdjustment = currentAdjustment - (currentAdjustment / 100 * tierPrice.Price);
                    }
                    else
                    {
                        currentAdjustment = decimal.Zero;
                    }

                    decimal taxRate = decimal.Zero;
                    decimal priceBase = _taxService.GetProductPrice(product, _priceCalculationService.GetFinalPrice(product, _services.WorkContext.CurrentCustomer, currentAdjustment, _catalogSettings.DisplayTierPricesWithDiscounts, tierPrice.Quantity, null, null, true), out taxRate);
                    decimal price = _currencyService.ConvertFromPrimaryStoreCurrency(priceBase, _services.WorkContext.WorkingCurrency);
                    m.Price = _priceFormatter.FormatPrice(price, true, false);
                    return m;
                })
                .ToList();

            return model;
        }

        public MediaGalleryModel PrepareProductDetailsMediaGalleryModel(
            IList<MediaFileInfo> files,
            string productName,
            IList<int> allCombinationImageIds,
            bool isAssociatedProduct,
            ProductBundleItemData bundleItem = null,
            ProductVariantAttributeCombination combination = null)
        {
            var model = new MediaGalleryModel
            {
                ModelName = productName,
                DefaultAlt = T("Media.Product.ImageAlternateTextFormat", productName),
                BoxEnabled = true, // TODO: make a setting for this in the future
                ImageZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled,
                ImageZoomType = _mediaSettings.PictureZoomType,
                ThumbSize = _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage,
                ImageSize = _mediaSettings.ProductDetailsPictureSize
            };

            if (isAssociatedProduct)
            {
                model.ThumbSize = _mediaSettings.AssociatedProductPictureSize;
            }
            else if (bundleItem != null)
            {
                model.ThumbSize = _mediaSettings.BundledProductPictureSize;
            }

            MediaFileInfo defaultFile = null;
            var combiAssignedImages = combination?.GetAssignedMediaIds();

            if (files.Count > 0)
            {
                if (files.Count <= _catalogSettings.DisplayAllImagesNumber)
                {
                    // Show all images.
                    foreach (var file in files)
                    {
                        model.Files.Add(PrepareMediaFileInfo(file, model));

                        if (defaultFile == null && combiAssignedImages != null && combiAssignedImages.Contains(file.Id))
                        {
                            model.GalleryStartIndex = model.Files.Count - 1;
                            defaultFile = file;
                        }
                    }
                }
                else
                {
                    // Images not belonging to any combination...
                    allCombinationImageIds = allCombinationImageIds ?? new List<int>();
                    foreach (var file in files.Where(p => !allCombinationImageIds.Contains(p.Id)))
                    {
                        model.Files.Add(PrepareMediaFileInfo(file, model));
                    }

                    // Plus images belonging to selected combination.
                    if (combiAssignedImages != null)
                    {
                        foreach (var file in files.Where(p => combiAssignedImages.Contains(p.Id)))
                        {
                            model.Files.Add(PrepareMediaFileInfo(file, model));

                            if (defaultFile == null)
                            {
                                model.GalleryStartIndex = model.Files.Count - 1;
                                defaultFile = file;
                            }
                        }
                    }
                }

                if (defaultFile == null)
                {
                    model.GalleryStartIndex = 0;
                    defaultFile = files.First();
                }
            }

            if (defaultFile == null && !_catalogSettings.HideProductDefaultPictures)
            {
                var fallbackImageSize = _mediaSettings.ProductDetailsPictureSize;
                if (isAssociatedProduct)
                {
                    fallbackImageSize = _mediaSettings.AssociatedProductPictureSize;
                }
                else if (bundleItem != null)
                {
                    fallbackImageSize = _mediaSettings.BundledProductPictureSize;
                }

                model.FallbackUrl = _mediaService.GetFallbackUrl(fallbackImageSize);
            }

            return model;
        }

        public ProductDetailsModel PrepareProductDetailModel(
            ProductDetailsModel model,
            Product product,
            ProductVariantQuery query,
            bool isAssociatedProduct = false,
            ProductBundleItemData productBundleItem = null,
            IList<ProductBundleItemData> productBundleItems = null,
            int selectedQuantity = 1)
        {
            Guard.NotNull(model, nameof(model));
            Guard.NotNull(product, nameof(product));

            var store = _services.StoreContext.CurrentStore;
            var customer = _services.WorkContext.CurrentCustomer;
            var currency = _services.WorkContext.WorkingCurrency;

            var preSelectedPriceAdjustmentBase = decimal.Zero;
            var preSelectedWeightAdjustment = decimal.Zero;
            var displayPrices = _services.Permissions.Authorize(Permissions.Catalog.DisplayPrice);
            var isBundle = product.ProductType == ProductType.BundledProduct;
            var isBundleItemPricing = productBundleItem != null && productBundleItem.Item.BundleProduct.BundlePerItemPricing;
            var isBundlePricing = productBundleItem != null && !productBundleItem.Item.BundleProduct.BundlePerItemPricing;
            var bundleItemId = productBundleItem == null ? 0 : productBundleItem.Item.Id;

            var hasSelectedAttributesValues = false;
            var hasSelectedAttributes = query.Variants.Any();
            List<ProductVariantAttributeValue> selectedAttributeValues = null;
            var variantAttributes = isBundle ? new List<ProductVariantAttribute>() : _productAttributeService.GetProductVariantAttributesByProductId(product.Id);

            var res = new Dictionary<string, LocalizedString>(StringComparer.OrdinalIgnoreCase)
            {
                { "Products.Availability.IsNotActive", T("Products.Availability.IsNotActive") },
                { "Products.Availability.OutOfStock", T("Products.Availability.OutOfStock") },
                { "Products.Availability.Backordering", T("Products.Availability.Backordering") },
            };

            model.IsBundlePart = product.ProductType != ProductType.BundledProduct && productBundleItem != null;
            model.ProductPrice.DynamicPriceUpdate = _catalogSettings.EnableDynamicPriceUpdate;
            model.ProductPrice.BundleItemShowBasePrice = _catalogSettings.BundleItemShowBasePrice;

            if (!model.ProductPrice.DynamicPriceUpdate)
            {
                selectedQuantity = 1;
            }

            #region Product attributes

            // Bundles doesn't have attributes.
            if (!isBundle)
            {
                foreach (var attribute in variantAttributes)
                {
                    var pvaModel = new ProductDetailsModel.ProductVariantAttributeModel
                    {
                        Id = attribute.Id,
                        ProductId = attribute.ProductId,
                        BundleItemId = bundleItemId,
                        ProductAttributeId = attribute.ProductAttributeId,
                        ProductAttribute = attribute,
                        Alias = attribute.ProductAttribute.Alias,
                        Name = attribute.ProductAttribute.GetLocalized(x => x.Name),
                        Description = attribute.ProductAttribute.GetLocalized(x => x.Description),
                        TextPrompt = attribute.TextPrompt,
                        CustomData = attribute.CustomData,
                        IsRequired = attribute.IsRequired,
                        AttributeControlType = attribute.AttributeControlType,
                        AllowedFileExtensions = _catalogSettings.FileUploadAllowedExtensions
                    };

                    if (hasSelectedAttributes)
                    {
                        var selectedVariant = query.Variants.FirstOrDefault(x =>
                            x.ProductId == product.Id &&
                            x.BundleItemId == bundleItemId &&
                            x.AttributeId == attribute.ProductAttributeId &&
                            x.VariantAttributeId == attribute.Id);

                        if (selectedVariant != null)
                        {
                            switch (attribute.AttributeControlType)
                            {
                                case AttributeControlType.Datepicker:
                                    if (selectedVariant.Date.HasValue)
                                    {
                                        pvaModel.SelectedDay = selectedVariant.Date.Value.Day;
                                        pvaModel.SelectedMonth = selectedVariant.Date.Value.Month;
                                        pvaModel.SelectedYear = selectedVariant.Date.Value.Year;
                                    }
                                    break;
                                case AttributeControlType.FileUpload:
                                    pvaModel.UploadedFileGuid = selectedVariant.Value;

                                    Guid guid;
                                    if (selectedVariant.Value.HasValue() && Guid.TryParse(selectedVariant.Value, out guid))
                                    {
                                        var download = _downloadService.GetDownloadByGuid(guid);
                                        if (download?.MediaFile != null)
                                        {
                                            pvaModel.UploadedFileName = download.MediaFile.Name;
                                        }
                                    }
                                    break;
                                case AttributeControlType.TextBox:
                                case AttributeControlType.MultilineTextbox:
                                    pvaModel.TextValue = selectedVariant.Value;
                                    break;
                            }
                        }
                    }

                    // TODO: obsolete? Alias field is not used for custom values anymore, only for URL as URL variant alias.
                    if (attribute.AttributeControlType == AttributeControlType.Datepicker && pvaModel.Alias.HasValue() && RegularExpressions.IsYearRange.IsMatch(pvaModel.Alias))
                    {
                        var match = RegularExpressions.IsYearRange.Match(pvaModel.Alias);
                        pvaModel.BeginYear = match.Groups[1].Value.ToInt();
                        pvaModel.EndYear = match.Groups[2].Value.ToInt();
                    }

                    var preSelectedValueId = 0;
                    var pvaValues = attribute.ShouldHaveValues()
                        ? _productAttributeService.GetProductVariantAttributeValues(attribute.Id)
                        : new List<ProductVariantAttributeValue>();

                    foreach (var pvaValue in pvaValues)
                    {
                        ProductBundleItemAttributeFilter attributeFilter = null;

                        if (productBundleItem?.Item?.FilterOut(pvaValue, out attributeFilter) ?? false)
                        {
                            continue;
                        }
                        if (preSelectedValueId == 0 && attributeFilter != null && attributeFilter.IsPreSelected)
                        {
                            preSelectedValueId = attributeFilter.AttributeValueId;
                        }

                        var linkedProduct = _productService.GetProductById(pvaValue.LinkedProductId);

                        var pvaValueModel = new ProductDetailsModel.ProductVariantAttributeValueModel
                        {
                            Id = pvaValue.Id,
                            ProductAttributeValue = pvaValue,
                            PriceAdjustment = string.Empty,
                            Name = pvaValue.GetLocalized(x => x.Name),
                            Alias = pvaValue.Alias,
                            Color = pvaValue.Color, // Used with "Boxes" attribute type.
                            IsPreSelected = pvaValue.IsPreSelected
                        };

                        if (linkedProduct != null && linkedProduct.Visibility != ProductVisibility.Hidden)
                        {
                            pvaValueModel.SeName = linkedProduct.GetSeName();
                        }

                        // Display price if allowed.
                        if (displayPrices && !isBundlePricing)
                        {
                            var attributeValuePriceAdjustment = _priceCalculationService.GetProductVariantAttributeValuePriceAdjustment(pvaValue, product, customer, null, selectedQuantity);
                            var priceAdjustmentBase = _taxService.GetProductPrice(product, attributeValuePriceAdjustment, out var _);
                            var priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, currency);

                            if (_catalogSettings.ShowVariantCombinationPriceAdjustment && !product.CallForPrice)
                            {
                                if (priceAdjustmentBase > decimal.Zero)
                                {
                                    pvaValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment, true, false);
                                }
                                else if (priceAdjustmentBase < decimal.Zero)
                                {
                                    pvaValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment, true, false);
                                }
                            }

                            if (pvaValueModel.IsPreSelected)
                            {
                                preSelectedPriceAdjustmentBase = decimal.Add(preSelectedPriceAdjustmentBase, priceAdjustmentBase);
                                preSelectedWeightAdjustment = decimal.Add(preSelectedWeightAdjustment, pvaValue.WeightAdjustment);
                            }

                            if (_catalogSettings.ShowLinkedAttributeValueQuantity && pvaValue.ValueType == ProductVariantAttributeValueType.ProductLinkage)
                            {
                                pvaValueModel.QuantityInfo = pvaValue.Quantity;
                            }

                            pvaValueModel.PriceAdjustmentValue = priceAdjustment;
                        }

                        if (_catalogSettings.ShowLinkedAttributeValueImage && pvaValue.ValueType == ProductVariantAttributeValueType.ProductLinkage)
                        {
                            var linkageFile = _productService.GetProductPicturesByProductId(pvaValue.LinkedProductId, 1).FirstOrDefault();
                            if (linkageFile != null)
                            {
                                pvaValueModel.ImageUrl = _mediaService.GetUrl(linkageFile.MediaFile, _mediaSettings.VariantValueThumbPictureSize, null, false);
                            }
                        }
                        else if (pvaValue.MediaFileId != 0)
                        {
                            pvaValueModel.ImageUrl = _mediaService.GetUrl(pvaValue.MediaFileId, _mediaSettings.VariantValueThumbPictureSize, null, false);
                        }

                        pvaModel.Values.Add(pvaValueModel);
                    }

                    // We need selected attributes for initially displayed combination images and multiple selected checkbox values.
                    if (query.VariantCombinationId == 0)
                    {
                        ProductDetailsModel.ProductVariantAttributeValueModel defaultValue = null;

                        // Value pre-selected by a bundle item filter discards the default pre-selection.
                        if (preSelectedValueId != 0)
                        {
                            pvaModel.Values.Each(x => x.IsPreSelected = false);

                            defaultValue = pvaModel.Values.OfType<ProductDetailsModel.ProductVariantAttributeValueModel>().FirstOrDefault(v => v.Id == preSelectedValueId);

                            if (defaultValue != null)
                            {
                                defaultValue.IsPreSelected = true;
                                query.AddVariant(new ProductVariantQueryItem(defaultValue.Id.ToString())
                                {
                                    ProductId = product.Id,
                                    BundleItemId = bundleItemId,
                                    AttributeId = attribute.ProductAttributeId,
                                    VariantAttributeId = attribute.Id,
                                    Alias = attribute.ProductAttribute.Alias,
                                    ValueAlias = defaultValue.Alias
                                });
                            }
                        }

                        if (defaultValue == null)
                        {
                            foreach (var value in pvaModel.Values.Where(x => x.IsPreSelected))
                            {
                                query.AddVariant(new ProductVariantQueryItem(value.Id.ToString())
                                {
                                    ProductId = product.Id,
                                    BundleItemId = bundleItemId,
                                    AttributeId = attribute.ProductAttributeId,
                                    VariantAttributeId = attribute.Id,
                                    Alias = attribute.ProductAttribute.Alias,
                                    ValueAlias = value.Alias
                                });
                            }
                        }
                    }

                    model.ProductVariantAttributes.Add(pvaModel);
                }
            }

            #endregion

            #region Attribute combinations

            if (!isBundle)
            {
                if (query.Variants.Any() || query.VariantCombinationId != 0)
                {
                    // Merge with combination data if there's a match.
                    var warnings = new List<string>();
                    var attributeXml = string.Empty;
                    var checkAvailability = product.AttributeChoiceBehaviour == AttributeChoiceBehaviour.GrayOutUnavailable;

                    if (query.VariantCombinationId != 0)
                    {
                        attributeXml = _productAttributeService.GetProductVariantAttributeCombinationById(query.VariantCombinationId)?.AttributesXml ?? string.Empty;
                    }
                    else
                    {
                        attributeXml = query.CreateSelectedAttributesXml(
                            product.Id,
                            bundleItemId,
                            variantAttributes,
                            _productAttributeParser,
                            _localizationService,
                            _downloadService,
                            _catalogSettings,
                            _httpRequest,
                            warnings);
                    }

                    selectedAttributeValues = _productAttributeParser.ParseProductVariantAttributeValues(attributeXml).ToList();
                    hasSelectedAttributesValues = selectedAttributeValues.Any();

                    if (isBundlePricing)
                    {
                        model.AttributeInfo = _productAttributeFormatter.FormatAttributes(
                            product,
                            attributeXml,
                            customer,
                            separator: ", ",
                            renderPrices: false,
                            renderGiftCardAttributes: false,
                            allowHyperlinks: false);
                    }

                    model.SelectedCombination = _productAttributeParser.FindProductVariantAttributeCombination(product.Id, attributeXml);

                    if (model.SelectedCombination != null && model.SelectedCombination.IsActive == false)
                    {
                        model.IsAvailable = false;
                        model.StockAvailability = res["Products.Availability.IsNotActive"];
                    }

                    // Required for below product.IsAvailableByStock().
                    product.MergeWithCombination(model.SelectedCombination);

                    // Explicitly selected values always discards values pre-selected by merchant.
                    var selectedValueIds = selectedAttributeValues.Select(x => x.Id).ToArray();

                    foreach (var attribute in model.ProductVariantAttributes)
                    {
                        var updatePreSelection = selectedValueIds.Any() && selectedValueIds.Intersect(attribute.Values.Select(x => x.Id)).Any();

                        foreach (ProductDetailsModel.ProductVariantAttributeValueModel value in attribute.Values)
                        {
                            if (updatePreSelection)
                            {
                                value.IsPreSelected = selectedValueIds.Contains(value.Id);
                            }

                            if (!_catalogSettings.ShowVariantCombinationPriceAdjustment)
                            {
                                value.PriceAdjustment = string.Empty;
                            }

                            if (checkAvailability)
                            {
                                var availabilityInfo = _productAttributeParser.IsCombinationAvailable(
                                    product,
                                    variantAttributes,
                                    selectedAttributeValues,
                                    value.ProductAttributeValue);

                                if (availabilityInfo != null)
                                {
                                    value.IsUnavailable = true;

                                    // Set title attribute for unavailable option.
                                    if (product.DisplayStockAvailability && availabilityInfo.IsOutOfStock && availabilityInfo.IsActive)
                                    {
                                        value.Title = product.BackorderMode == BackorderMode.NoBackorders || product.BackorderMode == BackorderMode.AllowQtyBelow0
                                            ? res["Products.Availability.OutOfStock"]
                                            : res["Products.Availability.Backordering"];
                                    }
                                    else
                                    {
                                        value.Title = res["Products.Availability.IsNotActive"];
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Properties

            if ((productBundleItem != null && !productBundleItem.Item.BundleProduct.BundlePerItemShoppingCart) ||
                (product.ManageInventoryMethod == ManageInventoryMethod.ManageStockByAttributes && !hasSelectedAttributesValues))
            {
                // Cases where stock inventory is not functional (what ShoppingCartService.GetStandardWarnings and ProductService.AdjustInventory does not handle).
                model.IsAvailable = true;
                var hasAttributeCombinations = _services.DbContext.QueryForCollection(product, (Product p) => p.ProductVariantAttributeCombinations).Any();
                model.StockAvailability = !hasAttributeCombinations ? product.FormatStockMessage(_localizationService) : "";
            }
            else if (model.IsAvailable)
            {
                model.IsAvailable = product.IsAvailableByStock();
                model.StockAvailability = product.FormatStockMessage(_localizationService);
            }

            model.Id = product.Id;
            model.Name = product.GetLocalized(x => x.Name);
            model.Condition = product.Condition;
            model.ShowCondition = _catalogSettings.ShowProductCondition;
            model.LocalizedCondition = product.Condition.GetLocalizedEnum(_services.Localization, _services.WorkContext);
            model.ShowSku = _catalogSettings.ShowProductSku;
            model.Sku = product.Sku;
            model.ShortDescription = product.GetLocalized(x => x.ShortDescription);
            model.FullDescription = product.GetLocalized(x => x.FullDescription, detectEmptyHtml: true);
            model.MetaKeywords = product.GetLocalized(x => x.MetaKeywords);
            model.MetaDescription = product.GetLocalized(x => x.MetaDescription);
            model.MetaTitle = product.GetLocalized(x => x.MetaTitle);
            model.SeName = product.GetSeName();
            model.ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber;
            model.ManufacturerPartNumber = product.ManufacturerPartNumber;
            model.ShowDimensions = _catalogSettings.ShowDimensions;
            model.ShowWeight = _catalogSettings.ShowWeight;
            model.ShowGtin = _catalogSettings.ShowGtin;
            model.Gtin = product.Gtin;
            model.HasSampleDownload = product.IsDownload && product.HasSampleDownload;
            model.IsCurrentCustomerRegistered = customer.IsRegistered();
            model.IsBasePriceEnabled = product.BasePriceEnabled && !(isBundle && product.BundlePerItemPricing);
            model.BasePriceInfo = product.GetBasePriceInfo(_localizationService, _priceFormatter, _currencyService, _taxService, _priceCalculationService, customer, currency);
            model.ShowLegalInfo = !model.IsBundlePart && _taxSettings.ShowLegalHintsInProductDetails;
            model.BundleTitleText = product.GetLocalized(x => x.BundleTitleText);
            model.BundlePerItemPricing = product.BundlePerItemPricing;
            model.BundlePerItemShipping = product.BundlePerItemShipping;
            model.BundlePerItemShoppingCart = product.BundlePerItemShoppingCart;

            var taxDisplayType = _services.WorkContext.GetTaxDisplayTypeFor(customer, store.Id);
            string taxInfo = T(taxDisplayType == TaxDisplayType.IncludingTax ? "Tax.InclVAT" : "Tax.ExclVAT");

            var defaultTaxRate = string.Empty;
            if (_taxSettings.DisplayTaxRates)
            {
                var taxRate = _taxService.GetTaxRate(product, customer);
                if (taxRate != decimal.Zero)
                {
                    var formattedTaxRate = _priceFormatter.FormatTaxRate(taxRate);
                    defaultTaxRate = $"({formattedTaxRate}%)";
                }
            }

            var additionalShippingCosts = string.Empty;
            var addShippingPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.AdditionalShippingCharge, currency);

            if (addShippingPrice > 0)
            {
                additionalShippingCosts = T("Common.AdditionalShippingSurcharge").Text.FormatInvariant(_priceFormatter.FormatPrice(addShippingPrice, true, false)) + ", ";
            }

            if (!product.IsShipEnabled || (addShippingPrice == 0 && product.IsFreeShipping))
            {
                model.LegalInfo += product.IsTaxExempt
                    ? T("Common.FreeShipping")
                    : "{0} {1}, {2}".FormatInvariant(taxInfo, defaultTaxRate, T("Common.FreeShipping"));
            }
            else
            {
                var shippingInfoUrl = _urlHelper.Topic("ShippingInfo").ToString();

                if (shippingInfoUrl.IsEmpty())
                {
                    model.LegalInfo = T("Tax.LegalInfoProductDetail2",
                        product.IsTaxExempt ? "" : taxInfo,
                        product.IsTaxExempt ? "" : defaultTaxRate,
                        additionalShippingCosts);
                }
                else
                {
                    model.LegalInfo = T("Tax.LegalInfoProductDetail",
                        product.IsTaxExempt ? "" : taxInfo,
                        product.IsTaxExempt ? "" : defaultTaxRate,
                        additionalShippingCosts,
                        shippingInfoUrl);
                }
            }

            model.LegalInfo = model.LegalInfo.TrimSafe();

            var dimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId)?.SystemKeyword ?? string.Empty;

            model.WeightValue = product.Weight;
            if (!isBundle)
            {
                if (selectedAttributeValues != null)
                {
                    foreach (var attributeValue in selectedAttributeValues)
                    {
                        model.WeightValue = decimal.Add(model.WeightValue, attributeValue.WeightAdjustment);
                    }
                }
                else
                {
                    model.WeightValue = decimal.Add(model.WeightValue, preSelectedWeightAdjustment);
                }
            }

            model.Weight = (model.WeightValue > 0) ? "{0} {1}".FormatCurrent(model.WeightValue.ToString("G29"), _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId).SystemKeyword) : "";
            model.Height = (product.Height > 0) ? "{0} {1}".FormatCurrent(product.Height.ToString("G29"), dimension) : "";
            model.Length = (product.Length > 0) ? "{0} {1}".FormatCurrent(product.Length.ToString("G29"), dimension) : "";
            model.Width = (product.Width > 0) ? "{0} {1}".FormatCurrent(product.Width.ToString("G29"), dimension) : "";

            if (productBundleItem != null)
            {
                model.ThumbDimensions = _mediaSettings.BundledProductPictureSize;
            }
            else if (isAssociatedProduct)
            {
                model.ThumbDimensions = _mediaSettings.AssociatedProductPictureSize;
            }

            // Delivery Time.
            var deliveryPresentation = _catalogSettings.DeliveryTimesInProductDetail;

            if (model.IsAvailable)
            {
                var deliveryTime = _deliveryTimeService.GetDeliveryTime(product);
                if (deliveryTime != null)
                {
                    model.DeliveryTimeName = deliveryTime.GetLocalized(x => x.Name);
                    model.DeliveryTimeHexValue = deliveryTime.ColorHexValue;

                    if (deliveryPresentation == DeliveryTimesPresentation.DateOnly || deliveryPresentation == DeliveryTimesPresentation.LabelAndDate)
                    {
                        model.DeliveryTimeDate = _deliveryTimeService.GetFormattedDeliveryDate(deliveryTime);
                    }
                }
            }

            model.IsShipEnabled = product.IsShipEnabled;
            model.DeliveryTimesPresentation = deliveryPresentation;
            model.DisplayDeliveryTimeAccordingToStock = product.DisplayDeliveryTimeAccordingToStock(_catalogSettings);

            if (model.DeliveryTimeName.IsEmpty() && deliveryPresentation != DeliveryTimesPresentation.None)
            {
                model.DeliveryTimeName = T("ShoppingCart.NotAvailable");
            }

            var quantityUnit = _quantityUnitService.GetQuantityUnit(product);
            if (quantityUnit != null)
            {
                model.QuantityUnitName = quantityUnit.GetLocalized(x => x.Name);
            }

            // Back in stock subscriptions.
            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.StockQuantity <= 0)
            {
                // Out of stock.
                model.DisplayBackInStockSubscription = true;
                model.BackInStockAlreadySubscribed = _backInStockSubscriptionService.FindSubscription(customer.Id, product.Id, store.Id) != null;
            }

            #endregion

            #region Product price

            model.ProductPrice.ProductId = product.Id;
            model.ProductPrice.HidePrices = !displayPrices;
            model.ProductPrice.ShowLoginNote = !displayPrices && productBundleItem == null && _catalogSettings.ShowLoginForPriceNote;

            if (displayPrices)
			{
				if (product.CustomerEntersPrice && !isBundleItemPricing)
				{
					model.ProductPrice.CustomerEntersPrice = true;
				}
				else
				{
					if (product.CallForPrice && !isBundleItemPricing)
					{
						model.ProductPrice.CallForPrice = true;
                        model.HotlineTelephoneNumber = _services.Settings.LoadSetting<ContactDataSettings>().HotlineTelephoneNumber.NullEmpty();
                    }
                    else
					{
						var taxRate = decimal.Zero;
                        var oldPrice = decimal.Zero;
                        var finalPriceWithoutDiscountBase = decimal.Zero;
                        var finalPriceWithDiscountBase = decimal.Zero;
                        var attributesTotalPriceBase = decimal.Zero;
                        var attributesTotalPriceBaseOrig = decimal.Zero;
                        var finalPriceWithoutDiscount = decimal.Zero;
                        var finalPriceWithDiscount = decimal.Zero;
                        var oldPriceBase = _taxService.GetProductPrice(product, product.OldPrice, out taxRate);

                        if (model.ProductPrice.DynamicPriceUpdate && !isBundlePricing)
                        {
                            if (selectedAttributeValues != null)
                            {
                                selectedAttributeValues.Each(x => attributesTotalPriceBase += _priceCalculationService.GetProductVariantAttributeValuePriceAdjustment(x,
                                    product, customer, null, selectedQuantity));

                                selectedAttributeValues.Each(x => attributesTotalPriceBaseOrig += _priceCalculationService.GetProductVariantAttributeValuePriceAdjustment(x,
                                    product, customer, null, 1));
                            }
                            else
                            {
                                attributesTotalPriceBase = preSelectedPriceAdjustmentBase;
                            }
                        }

                        if (productBundleItem != null)
                        {
                            productBundleItem.AdditionalCharge = attributesTotalPriceBase;
                        }

                        finalPriceWithoutDiscountBase = _priceCalculationService.GetFinalPrice(product, productBundleItems,
                            customer, attributesTotalPriceBaseOrig, false, selectedQuantity, productBundleItem);

                        finalPriceWithDiscountBase = _priceCalculationService.GetFinalPrice(product, productBundleItems,
                            customer, attributesTotalPriceBase, true, selectedQuantity, productBundleItem);

                        var basePriceAdjustment = finalPriceWithDiscountBase - finalPriceWithoutDiscountBase;

                        finalPriceWithoutDiscountBase = _taxService.GetProductPrice(product, finalPriceWithoutDiscountBase, out taxRate);
                        finalPriceWithDiscountBase = _taxService.GetProductPrice(product, finalPriceWithDiscountBase, out taxRate);

                        oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, currency);

                        finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscountBase, currency);
                        finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscountBase, currency);

                        if (productBundleItem == null || isBundleItemPricing)
                        {
                            if (oldPriceBase > decimal.Zero && oldPriceBase > finalPriceWithoutDiscountBase)
                            {
                                model.ProductPrice.OldPriceValue = oldPrice;
                                model.ProductPrice.OldPrice = _priceFormatter.FormatPrice(oldPrice);
                            }

                            model.ProductPrice.Price = _priceFormatter.FormatPrice(finalPriceWithoutDiscount);

                            if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                            {
                                model.ProductPrice.PriceWithDiscount = _priceFormatter.FormatPrice(finalPriceWithDiscount);
                            }
                        }

                        if (product.SpecialPriceEndDateTimeUtc.HasValue && product.SpecialPriceEndDateTimeUtc > DateTime.UtcNow)
                            model.ProductPrice.PriceValidUntilUtc = product.SpecialPriceEndDateTimeUtc.Value.ToString("u");

                        model.ProductPrice.PriceValue = finalPriceWithoutDiscount;
                        model.ProductPrice.PriceWithDiscountValue = finalPriceWithDiscount;
                        model.BasePriceInfo = product.GetBasePriceInfo(
                            _localizationService,
                            _priceFormatter,
                            _currencyService,
                            _taxService,
                            _priceCalculationService,
                            customer,
                            currency,
                            attributesTotalPriceBase);

                        if (!string.IsNullOrWhiteSpace(model.ProductPrice.OldPrice) || !string.IsNullOrWhiteSpace(model.ProductPrice.PriceWithDiscount))
                        {
                            model.ProductPrice.NoteWithoutDiscount = T(isBundle && product.BundlePerItemPricing ? "Products.Bundle.PriceWithoutDiscount.Note" : "Products.Price");
                        }

                        if ((isBundle && product.BundlePerItemPricing && !string.IsNullOrWhiteSpace(model.ProductPrice.PriceWithDiscount)) || product.HasTierPrices)
                        {
                            if (!product.HasTierPrices)
                            {
                                model.ProductPrice.NoteWithDiscount = T("Products.Bundle.PriceWithDiscount.Note");
                            }

                            model.BasePriceInfo = product.GetBasePriceInfo(
                                _localizationService,
                                _priceFormatter,
                                _currencyService,
                                _taxService,
                                _priceCalculationService,
                                customer,
                                currency,
                                basePriceAdjustment);
                        }

                        // Calculate saving.
                        // Discounted price has priority over the old price (avoids differing percentage discount in product lists and detail page).
                        //var regularPrice = Math.Max(finalPriceWithoutDiscount, oldPrice);
                        var regularPrice = finalPriceWithDiscount < finalPriceWithoutDiscount
                            ? finalPriceWithoutDiscount
                            : oldPrice;

                        if (regularPrice > 0 && regularPrice > finalPriceWithDiscount)
                        {
                            model.ProductPrice.SavingPercent = (float)((regularPrice - finalPriceWithDiscount) / regularPrice) * 100;
                            model.ProductPrice.SavingAmount = _priceFormatter.FormatPrice(regularPrice - finalPriceWithDiscount, true, false);
                        }
                    }
                }
            }
            else
            {
                model.ProductPrice.OldPrice = null;
                model.ProductPrice.Price = null;
            }

            #endregion

            #region 'Add to cart' model

            model.AddToCart.ProductId = product.Id;
            model.AddToCart.EnteredQuantity = product.OrderMinimumQuantity > selectedQuantity ? product.OrderMinimumQuantity : selectedQuantity;
            model.AddToCart.MinOrderAmount = product.OrderMinimumQuantity;
            model.AddToCart.MaxOrderAmount = product.OrderMaximumQuantity;
            model.AddToCart.QuantityUnitName = model.QuantityUnitName; // TODO: (mc) remove 'QuantityUnitName' from parent model later
            model.AddToCart.QuantityStep = product.QuantityStep > 0 ? product.QuantityStep : 1;
            model.AddToCart.HideQuantityControl = product.HideQuantityControl;
            model.AddToCart.QuantiyControlType = product.QuantiyControlType;
            model.AddToCart.AvailableForPreOrder = product.AvailableForPreOrder;

            // 'add to cart', 'add to wishlist' buttons.
            model.AddToCart.DisableBuyButton = !displayPrices || product.DisableBuyButton ||
                !_services.Permissions.Authorize(Permissions.Cart.AccessShoppingCart);

            model.AddToCart.DisableWishlistButton = !displayPrices || product.DisableWishlistButton
                || product.ProductType == ProductType.GroupedProduct
                || !_services.Permissions.Authorize(Permissions.Cart.AccessWishlist);

            model.AddToCart.CustomerEntersPrice = product.CustomerEntersPrice;
            if (model.AddToCart.CustomerEntersPrice)
            {
                var minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MinimumCustomerEnteredPrice, currency);
                var maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MaximumCustomerEnteredPrice, currency);

                model.AddToCart.CustomerEnteredPrice = minimumCustomerEnteredPrice;

                model.AddToCart.CustomerEnteredPriceRange = string.Format(T("Products.EnterProductPrice.Range"),
                    _priceFormatter.FormatPrice(minimumCustomerEnteredPrice, true, false),
                    _priceFormatter.FormatPrice(maximumCustomerEnteredPrice, true, false));
            }

            var allowedQuantities = product.ParseAllowedQuatities();
            foreach (var qty in allowedQuantities)
            {
                model.AddToCart.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString()
                });
            }

            #endregion

            #region Gift card

            model.GiftCard.IsGiftCard = product.IsGiftCard;
            if (model.GiftCard.IsGiftCard)
            {
                model.GiftCard.GiftCardType = product.GiftCardType;
                model.GiftCard.SenderName = customer.GetFullName();
                model.GiftCard.SenderEmail = customer.Email;
            }

            #endregion

            _services.DisplayControl.Announce(product);

            return model;
        }

        public IList<ProductSpecificationModel> PrepareProductSpecificationModel(Product product)
        {
            Guard.NotNull(product, nameof(product));

            if (_services.Cache.IsDistributedCache)
            {
                // How bad we cannot cache LocalizedValue in distributed caches
                return Execute();
            }
            else
            {
                string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, product.Id, _services.WorkContext.WorkingLanguage.Id);
                return _services.Cache.Get(cacheKey, () =>
                {
                    return Execute();
                });
            }

            List<ProductSpecificationModel> Execute()
            {
                var model = _specificationAttributeService.GetProductSpecificationAttributesByProductId(product.Id, null, true)
                   .Select(psa =>
                   {
                       return new ProductSpecificationModel
                       {
                           SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
                           SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
                           SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name)
                       };
                   }).ToList();

                return model;
            }
        }

        public PictureModel PrepareCategoryPictureModel(
            Category category,
            string localizedName,
            IDictionary<int, MediaFileInfo> fileLookup = null)
        {
            MediaFileInfo file;

            if (fileLookup != null)
            {
                fileLookup.TryGetValue(category.MediaFileId ?? 0, out file);
            }
            else
            {
                file = _mediaService.GetFileById(category.MediaFileId ?? 0, MediaLoadFlags.AsNoTracking);
            }

            var model = new PictureModel
            {
                PictureId = category.MediaFileId.GetValueOrDefault(),
                Size = _mediaSettings.CategoryThumbPictureSize,
                ImageUrl = _mediaService.GetUrl(file, _mediaSettings.CategoryThumbPictureSize, null, !_catalogSettings.HideCategoryDefaultPictures),
                Title = file?.File?.GetLocalized(x => x.Title)?.Value.NullEmpty() ?? string.Format(T("Media.Category.ImageLinkTitleFormat"), localizedName),
                AlternateText = file?.File?.GetLocalized(x => x.Alt)?.Value.NullEmpty() ?? string.Format(T("Media.Category.ImageAlternateTextFormat"), localizedName),
                File = file
            };

            _services.DisplayControl.Announce(file?.File);

            return model;
        }

        public List<ManufacturerOverviewModel> PrepareManufacturersOverviewModel(
            ICollection<ProductManufacturer> manufacturers,
            IDictionary<int, ManufacturerOverviewModel> cachedModels = null,
            bool withPicture = false)
        {
            var model = new List<ManufacturerOverviewModel>();

            if (cachedModels == null)
            {
                cachedModels = new Dictionary<int, ManufacturerOverviewModel>();
            }

            IDictionary<int, MediaFileInfo> mediaFileLookup = null;
            if (withPicture)
            {
                mediaFileLookup = manufacturers
                    .Select(x => x.Manufacturer.MediaFile)
                    .Where(x => x != null)
                    .Distinct()
                    .Select(x => _mediaService.ConvertMediaFile(x))
                    .ToDictionarySafe(x => x.Id);
            }

            //var files = _mediaService.GetFilesByIds(fileIds).ToDictionarySafe(x => x.Id);

            foreach (var pm in manufacturers)
            {
                var manufacturer = pm.Manufacturer;

                if (!cachedModels.TryGetValue(manufacturer.Id, out ManufacturerOverviewModel item))
                {
                    item = new ManufacturerOverviewModel
                    {
                        Id = manufacturer.Id,
                        Name = manufacturer.GetLocalized(x => x.Name),
                        Description = manufacturer.GetLocalized(x => x.Description, true),
                        SeName = manufacturer.GetSeName()
                    };

                    if (withPicture)
                    {
                        item.Picture = PrepareManufacturerPictureModel(manufacturer, item.Name, mediaFileLookup);
                    }

                    cachedModels.Add(item.Id, item);
                }

                model.Add(item);
            }

            return model;
        }

        public PictureModel PrepareManufacturerPictureModel(
            Manufacturer manufacturer,
            string localizedName,
            IDictionary<int, MediaFileInfo> fileLookup = null)
        {
            MediaFileInfo file;

            if (fileLookup != null)
            {
                fileLookup.TryGetValue(manufacturer.MediaFileId ?? 0, out file);
            }
            else
            {
                file = _mediaService.GetFileById(manufacturer.MediaFileId ?? 0, MediaLoadFlags.AsNoTracking);
            }

            var model = new PictureModel
            {
                PictureId = manufacturer.MediaFileId.GetValueOrDefault(),
                Size = _mediaSettings.ManufacturerThumbPictureSize,
                ImageUrl = _mediaService.GetUrl(file, _mediaSettings.ManufacturerThumbPictureSize, null, !_catalogSettings.HideManufacturerDefaultPictures),
                Title = file?.File?.GetLocalized(x => x.Title)?.Value.NullEmpty() ?? string.Format(T("Media.Manufacturer.ImageLinkTitleFormat"), localizedName),
                AlternateText = file?.File?.GetLocalized(x => x.Alt)?.Value.NullEmpty() ?? string.Format(T("Media.Manufacturer.ImageAlternateTextFormat"), localizedName),
                File = file
            };

            _services.DisplayControl.Announce(file?.File);

            return model;
        }

        public ManufacturerNavigationModel PrepareManufacturerNavigationModel(int manufacturerItemsToDisplay)
        {
            var storeId = _services.StoreContext.CurrentStore.Id;
            var storeToken = QuerySettings.IgnoreMultiStore ? "0" : storeId.ToString();
            var rolesToken = QuerySettings.IgnoreAcl ? "0" : _services.WorkContext.CurrentCustomer.GetRolesIdent();

            var settingsKeyPart = string.Join(",",
                _catalogSettings.ShowManufacturersOnHomepage,
                _catalogSettings.ShowManufacturerPictures,
                _catalogSettings.HideManufacturerDefaultPictures,
                _mediaSettings.ManufacturerThumbPictureSize).ToLower();

            var cacheKey = string.Format(ModelCacheEventConsumer.MANUFACTURER_NAVIGATION_MODEL_KEY,
                settingsKeyPart,
                _services.WorkContext.WorkingLanguage.Id,
                storeToken,
                rolesToken,
                manufacturerItemsToDisplay);

            var cacheModel = _services.Cache.Get(cacheKey, () =>
            {
                var manufacturers = _manufacturerService.GetAllManufacturers(null, 0, manufacturerItemsToDisplay + 1, storeId);
                var files = new Dictionary<int, MediaFileInfo>();

                if (_catalogSettings.ShowManufacturerPictures)
                {
                    var fileIds = manufacturers
                        .Select(x => x.MediaFileId ?? 0)
                        .Where(x => x != 0)
                        .Distinct()
                        .ToArray();
                    files = _mediaService.GetFilesByIds(fileIds).ToDictionarySafe(x => x.Id);
                }

                var model = new ManufacturerNavigationModel
                {
                    DisplayManufacturers = _catalogSettings.ShowManufacturersOnHomepage,
                    DisplayImages = _catalogSettings.ShowManufacturerPictures,
                    DisplayAllManufacturersLink = manufacturers.Count > manufacturerItemsToDisplay,
                    HideManufacturerDefaultPictures = _catalogSettings.HideManufacturerDefaultPictures,
                    ManufacturerThumbPictureSize = _mediaSettings.ManufacturerThumbPictureSize
                };

                if (model.DisplayAllManufacturersLink)
                {
                    manufacturerItemsToDisplay -= 1;
                }

                foreach (var manufacturer in manufacturers.Take(manufacturerItemsToDisplay))
                {
                    files.TryGetValue(manufacturer.MediaFileId ?? 0, out var file);

                    var name = manufacturer.GetLocalized(x => x.Name);

                    model.Manufacturers.Add(new ManufacturerBriefInfoModel
                    {
                        Id = manufacturer.Id,
                        Name = name,
                        SeName = manufacturer.GetSeName(),
                        DisplayOrder = manufacturer.DisplayOrder,
                        FileId = manufacturer.MediaFileId,
                        AlternateText = file?.File?.GetLocalized(x => x.Alt)?.Value.NullEmpty() ?? name,
                        Title = file?.File?.GetLocalized(x => x.Title)?.Value.NullEmpty() ?? name
                    });
                }

                return model;
            }, TimeSpan.FromHours(6));

            return cacheModel;
        }
    }
}