using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Html;
using SmartStore.Core.Security;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Seo;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Web.Controllers
{
    public partial class ProductController : PublicControllerBase
    {
        private readonly ICommonServices _services;
        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ITaxService _taxService;
        private readonly IMediaService _mediaService;
        private readonly ICustomerContentService _customerContentService;
        private readonly ICustomerService _customerService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IProductTagService _productTagService;
        private readonly IOrderReportService _orderReportService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly MediaSettings _mediaSettings;
        private readonly SeoSettings _seoSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CatalogHelper _helper;
        private readonly IBreadcrumb _breadcrumb;
        private readonly Lazy<PrivacySettings> _privacySettings;
        private readonly Lazy<TaxSettings> _taxSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IDownloadService _downloadService;
        private readonly ProductUrlHelper _productUrlHelper;
        private readonly IProductAttributeFormatter _productAttributeFormatter;

        public ProductController(
            ICommonServices services,
            IProductService productService,
            IProductAttributeService productAttributeService,
            ITaxService taxService,
            IMediaService mediaService,
            ICustomerContentService customerContentService,
            ICustomerService customerService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            IProductTagService productTagService,
            IOrderReportService orderReportService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            MediaSettings mediaSettings,
            SeoSettings seoSettings,
            CatalogSettings catalogSettings,
            ShoppingCartSettings shoppingCartSettings,
            LocalizationSettings localizationSettings,
            CaptchaSettings captchaSettings,
            CatalogHelper helper,
            IBreadcrumb breadcrumb,
            Lazy<PrivacySettings> privacySettings,
            Lazy<TaxSettings> taxSettings,
            ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser,
            IDownloadService downloadService,
            ProductUrlHelper productUrlHelper,
            IProductAttributeFormatter productAttributeFormatter)
        {
            _services = services;
            _productService = productService;
            _productAttributeService = productAttributeService;
            _taxService = taxService;
            _mediaService = mediaService;
            _customerContentService = customerContentService;
            _customerService = customerService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _productTagService = productTagService;
            _orderReportService = orderReportService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _mediaSettings = mediaSettings;
            _seoSettings = seoSettings;
            _catalogSettings = catalogSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _localizationSettings = localizationSettings;
            _captchaSettings = captchaSettings;
            _helper = helper;
            _breadcrumb = breadcrumb;
            _privacySettings = privacySettings;
            _taxSettings = taxSettings;
            _localizationService = localizationService;
            _downloadService = downloadService;
            _productAttributeParser = productAttributeParser;
            _productUrlHelper = productUrlHelper;
            _productAttributeFormatter = productAttributeFormatter;
        }

        #region Products

        [RewriteUrl(SslRequirement.No)]
        public ActionResult ProductDetails(int productId, string attributes, ProductVariantQuery query)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || product.IsSystemProduct)
                return HttpNotFound();

            // Is published? Check whether the current user has a "Manage catalog" permission.
            // It allows him to preview a product before publishing.
            if (!product.Published && !_services.Permissions.Authorize(Permissions.Catalog.Product.Read))
                return HttpNotFound();

            // ACL (access control list)
            if (!_aclService.Authorize(product))
                return HttpNotFound();

            // Store mapping
            if (!_storeMappingService.Authorize(product))
                return HttpNotFound();

            // Is product individually visible?
            if (product.Visibility == ProductVisibility.Hidden)
            {
                // Find parent grouped product.
                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return HttpNotFound();

                var seName = parentGroupedProduct.GetSeName();
                if (seName.IsEmpty())
                    return HttpNotFound();

                var routeValues = new RouteValueDictionary();
                routeValues.Add("SeName", seName);

                // Add query string parameters.
                Request.QueryString.AllKeys.Each(x => routeValues.Add(x, Request.QueryString[x]));

                return RedirectToRoute("Product", routeValues);
            }

            // Prepare the view model
            var model = _helper.PrepareProductDetailsPageModel(product, query);

            // Some cargo data
            model.PictureSize = _mediaSettings.ProductDetailsPictureSize;
            model.CanonicalUrlsEnabled = _seoSettings.CanonicalUrlsEnabled;
            model.HotlineTelephoneNumber = _services.Settings.LoadSetting<ContactDataSettings>().HotlineTelephoneNumber.NullEmpty();

            // Save as recently viewed
            _recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

            // Activity log
            _services.CustomerActivity.InsertActivity("PublicStore.ViewProduct", T("ActivityLog.PublicStore.ViewProduct"), product.Name);

            // Breadcrumb
            if (_catalogSettings.CategoryBreadcrumbEnabled)
            {
                _helper.GetBreadcrumb(_breadcrumb, ControllerContext, product);

                _breadcrumb.Track(new MenuItem
                {
                    Text = model.Name,
                    Rtl = model.Name.CurrentLanguage.Rtl,
                    EntityId = product.Id,
                    Url = Url.RouteUrl("Product", new { model.SeName })
                });
            }

            return View(model.ProductTemplateViewPath, model);
        }

        [ChildActionOnly]
        public ActionResult ReviewSummary(int id /* productId */)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
                throw new ArgumentException(T("Products.NotFound", id));

            var model = new ProductReviewOverviewModel
            {
                ProductId = product.Id,
                RatingSum = product.ApprovedRatingSum,
                TotalReviews = product.ApprovedTotalReviews,
                AllowCustomerReviews = product.AllowCustomerReviews
            };

            return PartialView("Product.ReviewSummary", model);
        }

        [ChildActionOnly]
        public ActionResult ProductSpecifications(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                throw new ArgumentException(T("Products.NotFound", productId));
            }

            var model = _helper.PrepareProductSpecificationModel(product);

            if (model.Count == 0)
            {
                return Content("");
            }

            return PartialView("Product.Specs", model);
        }

        [ChildActionOnly]
        public ActionResult ProductDetailReviews(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || !product.AllowCustomerReviews)
            {
                return Content("");
            }

            var model = new ProductReviewsModel();
            _helper.PrepareProductReviewsModel(model, product, 10);

            return PartialView("Product.Reviews", model);
        }

        [ChildActionOnly]
        public ActionResult ProductTierPrices(int productId)
        {
            if (!_services.Permissions.Authorize(Permissions.Catalog.DisplayPrice))
            {
                return new EmptyResult();
            }

            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                throw new ArgumentException(T("Products.NotFound", productId));
            }

            if (!product.HasTierPrices)
            {
                // No tier prices.
                return new EmptyResult();
            }

            var model = _helper.CreateTierPriceModel(product);

            return PartialView("Product.TierPrices", model);
        }

        [ChildActionOnly]
        public ActionResult RelatedProducts(int productId, int? productThumbPictureSize)
        {
            var products = new List<Product>();
            var relatedProducts = _productService.GetRelatedProductsByProductId1(productId);

            foreach (var product in _productService.GetProductsByIds(relatedProducts.Select(x => x.ProductId2).ToArray()))
            {
                // Ensure has ACL permission and appropriate store mapping
                if (_aclService.Authorize(product) && _storeMappingService.Authorize(product))
                    products.Add(product);
            }

            if (products.Count == 0)
            {
                return Content("");
            }

            var settings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Grid, x =>
            {
                x.ThumbnailSize = productThumbPictureSize;
                x.DeliveryTimesPresentation = DeliveryTimesPresentation.None;
            });

            var model = _helper.MapProductSummaryModel(products, settings);
            model.ShowBasePrice = false;

            return PartialView("Product.RelatedProducts", model);
        }

        [ChildActionOnly]
        public ActionResult ProductsAlsoPurchased(int productId, int? productThumbPictureSize)
        {
            if (!_catalogSettings.ProductsAlsoPurchasedEnabled)
            {
                return Content("");
            }

            // load and cache report
            var productIds = _services.Cache.Get(string.Format(ModelCacheEventConsumer.PRODUCTS_ALSO_PURCHASED_IDS_KEY, productId, _services.StoreContext.CurrentStore.Id), () =>
            {
                return _orderReportService.GetAlsoPurchasedProductsIds(_services.StoreContext.CurrentStore.Id, productId, _catalogSettings.ProductsAlsoPurchasedNumber);
            });

            // Load products
            var products = _productService.GetProductsByIds(productIds);

            // ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

            if (products.Count == 0)
            {
                return Content("");
            }

            // Prepare model
            var settings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Mini, x =>
            {
                x.ThumbnailSize = productThumbPictureSize;
            });

            var model = _helper.MapProductSummaryModel(products, settings);

            return PartialView("Product.AlsoPurchased", model);
        }

        [ChildActionOnly]
        public ActionResult CrossSellProducts(int? productThumbPictureSize)
        {
            var cart = _services.WorkContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _services.StoreContext.CurrentStore.Id);

            var products = _productService.GetCrosssellProductsByShoppingCart(cart, _shoppingCartSettings.CrossSellsNumber);

            // ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

            if (products.Any())
            {
                // Cross-sell products are dispalyed on the shopping cart page.
                // We know that the entire shopping cart page is not refresh
                // even if "ShoppingCartSettings.DisplayCartAfterAddingProduct" setting  is enabled.
                // That's why we force page refresh (redirect) in this case
                var settings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Grid, x =>
                {
                    x.ThumbnailSize = productThumbPictureSize;
                    x.ForceRedirectionAfterAddingToCart = true;
                });

                var model = _helper.MapProductSummaryModel(products, settings);

                return PartialView(model);
            }

            return PartialView(ProductSummaryModel.Empty);
        }

        [ActionName("BackInStockSubscribe")]
        public ActionResult BackInStockSubscribePopup(int id /* productId */)
        {
            var product = _productService.GetProductById(id);
            if (product == null || product.Deleted || product.IsSystemProduct)
            {
                throw new ArgumentException(T("Products.NotFound", id));
            }

            var customer = _services.WorkContext.CurrentCustomer;
            var store = _services.StoreContext.CurrentStore;

            var model = new BackInStockSubscribeModel();
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.IsCurrentCustomerRegistered = customer.IsRegistered();
            model.MaximumBackInStockSubscriptions = _catalogSettings.MaximumBackInStockSubscriptions;
            model.CurrentNumberOfBackInStockSubscriptions = _backInStockSubscriptionService
                 .GetAllSubscriptionsByCustomerId(customer.Id, store.Id, 0, 1)
                 .TotalCount;

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.StockQuantity <= 0)
            {
                // Out of stock.
                model.SubscriptionAllowed = true;
                model.AlreadySubscribed = _backInStockSubscriptionService.FindSubscription(customer.Id, product.Id, store.Id) != null;
            }

            return View("BackInStockSubscribePopup", model);
        }

        [HttpPost]
        public ActionResult BackInStockSubscribePopup(int id /* productId */, FormCollection form)
        {
            var product = _productService.GetProductById(id);
            if (product == null || product.Deleted || product.IsSystemProduct)
            {
                throw new ArgumentException(T("Products.NotFound", id));
            }

            if (!_services.WorkContext.CurrentCustomer.IsRegistered())
            {
                return Content(T("BackInStockSubscriptions.OnlyRegistered"));
            }

            if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                product.BackorderMode == BackorderMode.NoBackorders &&
                product.AllowBackInStockSubscriptions &&
                product.StockQuantity <= 0)
            {
                var customer = _services.WorkContext.CurrentCustomer;
                var store = _services.StoreContext.CurrentStore;

                // Out of stock.
                var subscription = _backInStockSubscriptionService.FindSubscription(customer.Id, product.Id, store.Id);
                if (subscription != null)
                {
                    // Unsubscribe.
                    _backInStockSubscriptionService.DeleteSubscription(subscription);
                    return Content("Unsubscribed");
                }
                else
                {
                    if (_backInStockSubscriptionService.GetAllSubscriptionsByCustomerId(customer.Id, store.Id, 0, 1).TotalCount >= _catalogSettings.MaximumBackInStockSubscriptions)
                    {
                        return Content(string.Format(T("BackInStockSubscriptions.MaxSubscriptions"), _catalogSettings.MaximumBackInStockSubscriptions));
                    }

                    // Subscribe.
                    subscription = new BackInStockSubscription
                    {
                        Customer = customer,
                        Product = product,
                        StoreId = store.Id,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    _backInStockSubscriptionService.InsertSubscription(subscription);
                    return Content("Subscribed");
                }
            }
            else
            {
                return Content(T("BackInStockSubscriptions.NotAllowed"));
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateProductDetails(int productId, string itemType, int bundleItemId, ProductVariantQuery query, FormCollection form)
        {
            int quantity = 1;
            int galleryStartIndex = -1;
            string galleryHtml = null;
            string dynamicThumbUrl = null;
            var isAssociated = itemType.IsCaseInsensitiveEqual("associateditem");
            var m = new ProductDetailsModel();
            var product = _productService.GetProductById(productId);
            var bItem = _productService.GetBundleItemById(bundleItemId);
            IList<ProductBundleItemData> bundleItems = null;
            ProductBundleItemData bundleItem = bItem == null ? null : new ProductBundleItemData(bItem);

            // Quantity required for tier prices.
            string quantityKey = form.AllKeys.FirstOrDefault(k => k.EndsWith("EnteredQuantity"));
            if (quantityKey.HasValue())
            {
                int.TryParse(form[quantityKey], out quantity);
            }

            if (product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing)
            {
                bundleItems = _productService.GetBundleItems(product.Id);
                if (query.Variants.Count > 0)
                {
                    // May add elements to query object if they are preselected by bundle item filter.
                    foreach (var itemData in bundleItems)
                    {
                        _helper.PrepareProductDetailsPageModel(itemData.Item.Product, query, false, itemData, null);
                    }
                }
            }

            // Get merged model data.
            _helper.PrepareProductDetailModel(m, product, query, isAssociated, bundleItem, bundleItems, quantity);

            if (bundleItem != null)
            {
                // Update bundle item thumbnail.
                if (!bundleItem.Item.HideThumbnail)
                {
                    var assignedMediaIds = m.SelectedCombination?.GetAssignedMediaIds() ?? new int[0];
                    if (assignedMediaIds.Any() && _mediaService.GetFileById(assignedMediaIds[0], MediaLoadFlags.AsNoTracking) != null)
                    {
                        var file = _productService.GetProductPicturesByProductId(bundleItem.Item.ProductId, 1)
                            .Select(x => x.MediaFile)
                            .FirstOrDefault();

                        dynamicThumbUrl = _mediaService.GetUrl(file, _mediaSettings.BundledProductPictureSize, null, false);
                    }
                }
            }
            else if (isAssociated)
            {
                // Update associated product thumbnail.
                var assignedMediaIds = m.SelectedCombination?.GetAssignedMediaIds() ?? new int[0];
                if (assignedMediaIds.Any() && _mediaService.GetFileById(assignedMediaIds[0], MediaLoadFlags.AsNoTracking) != null)
                {
                    var file = _productService.GetProductPicturesByProductId(productId, 1)
                        .Select(x => x.MediaFile)
                        .FirstOrDefault();

                    dynamicThumbUrl = _mediaService.GetUrl(file, _mediaSettings.AssociatedProductPictureSize, null, false);
                }
            }
            else if (product.ProductType != ProductType.BundledProduct)
            {
                // Update image gallery.
                var files = _productService.GetProductPicturesByProductId(productId)
                    .Select(x => _mediaService.ConvertMediaFile(x.MediaFile))
                    .ToList();

                if (product.HasPreviewPicture && files.Count > 1)
                {
                    files.RemoveAt(0);
                }

                if (files.Count <= _catalogSettings.DisplayAllImagesNumber)
                {
                    // All pictures rendered... only index is required.
                    galleryStartIndex = 0;

                    var assignedMediaIds = m.SelectedCombination?.GetAssignedMediaIds() ?? new int[0];
                    if (assignedMediaIds.Any())
                    {
                        var file = files.FirstOrDefault(p => p.Id == assignedMediaIds[0]);
                        galleryStartIndex = file == null ? 0 : files.IndexOf(file);
                    }
                }
                else
                {
                    var allCombinationPictureIds = _productAttributeService.GetAllProductVariantAttributeCombinationPictureIds(product.Id);

                    var mediaModel = _helper.PrepareProductDetailsMediaGalleryModel(
                        files,
                        product.GetLocalized(x => x.Name),
                        allCombinationPictureIds,
                        false,
                        bundleItem,
                        m.SelectedCombination);

                    galleryStartIndex = mediaModel.GalleryStartIndex;
                    galleryHtml = this.RenderPartialViewToString("Product.Media", mediaModel);
                }

                m.PriceDisplayStyle = _catalogSettings.PriceDisplayStyle;
                m.DisplayTextForZeroPrices = _catalogSettings.DisplayTextForZeroPrices;
            }

            object partials = null;

            if (m.IsBundlePart)
            {
                partials = new
                {
                    BundleItemPrice = this.RenderPartialViewToString("Product.Offer.Price", m),
                    BundleItemStock = this.RenderPartialViewToString("Product.StockInfo", m),
                    BundleItemVariants = this.RenderPartialViewToString("Product.Variants", m.ProductVariantAttributes)
                };
            }
            else
            {
                var dataDictAddToCart = new ViewDataDictionary();
                dataDictAddToCart.TemplateInfo.HtmlFieldPrefix = string.Format("addtocart_{0}", m.Id);

                decimal adjustment = decimal.Zero;
                decimal taxRate = decimal.Zero;
                var finalPriceWithDiscountBase = _taxService.GetProductPrice(product, product.Price, _services.WorkContext.CurrentCustomer, out taxRate);

                if (!_taxSettings.Value.PricesIncludeTax && _services.WorkContext.TaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    adjustment = (m.ProductPrice.PriceValue - finalPriceWithDiscountBase) / (taxRate / 100 + 1);
                }
                else if (_taxSettings.Value.PricesIncludeTax && _services.WorkContext.TaxDisplayType == TaxDisplayType.ExcludingTax)
                {
                    adjustment = (m.ProductPrice.PriceValue - finalPriceWithDiscountBase) * (taxRate / 100 + 1);
                }
                else
                {
                    adjustment = m.ProductPrice.PriceValue - finalPriceWithDiscountBase;
                }

                partials = new
                {
                    Attrs = this.RenderPartialViewToString("Product.Attrs", m),
                    Price = this.RenderPartialViewToString("Product.Offer.Price", m),
                    Stock = this.RenderPartialViewToString("Product.StockInfo", m),
                    Variants = this.RenderPartialViewToString("Product.Variants", m.ProductVariantAttributes),
                    OfferActions = this.RenderPartialViewToString("Product.Offer.Actions", m, dataDictAddToCart),
                    TierPrices = this.RenderPartialViewToString("Product.TierPrices", _helper.CreateTierPriceModel(product, adjustment)),
                    BundlePrice = product.ProductType == ProductType.BundledProduct ? this.RenderPartialViewToString("Product.Bundle.Price", m) : (string)null
                };
            }

            object data = new
            {
                Partials = partials,
                DynamicThumblUrl = dynamicThumbUrl,
                GalleryStartIndex = galleryStartIndex,
                GalleryHtml = galleryHtml
            };

            return new JsonResult { Data = data };
        }

        #endregion


        #region Product tags

        [ChildActionOnly]
        public ActionResult ProductTags(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                throw new ArgumentException(T("Products.NotFound", productId));
            }

            var store = _services.StoreContext.CurrentStore;

            var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCTTAG_BY_PRODUCT_MODEL_KEY, product.Id, _services.WorkContext.WorkingLanguage.Id, store.Id);
            var cacheModel = _services.Cache.Get(cacheKey, () =>
            {
                var model = product.ProductTags
                    .Where(x => x.Published && _productTagService.GetProductCount(x.Id, store.Id) > 0)
                    .Select(x =>
                    {
                        var ptModel = new ProductTagModel
                        {
                            Id = x.Id,
                            Name = x.GetLocalized(y => y.Name),
                            SeName = x.GetSeName(),
                            ProductCount = _productTagService.GetProductCount(x.Id, store.Id)
                        };
                        return ptModel;
                    })
                    .ToList();
                return model;
            });

            return PartialView("Product.Tags", cacheModel);
        }

        #endregion


        #region Product reviews

        [RewriteUrl(SslRequirement.No)]
        [GdprConsent]
        public ActionResult Reviews(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null || product.Deleted || product.IsSystemProduct || !product.Published || !product.AllowCustomerReviews)
            {
                return HttpNotFound();
            }

            var model = new ProductReviewsModel
            {
                Rating = _catalogSettings.DefaultProductRatingValue
            };

            _helper.PrepareProductReviewsModel(model, product);

            model.SuccessfullyAdded = (TempData["SuccessfullyAdded"] as bool?) ?? false;
            if (model.SuccessfullyAdded)
            {
                model.Result = T(_catalogSettings.ProductReviewsMustBeApproved
                    ? "Reviews.SeeAfterApproving"
                    : "Reviews.SuccessfullyAdded");
            }

            // Only registered users can leave reviews.
            if (_services.WorkContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                ModelState.AddModelError("", T("Reviews.OnlyRegisteredUsersCanWriteReviews"));
            }

            return View(model);
        }

        [HttpPost, ActionName("Reviews")]
        [ValidateCaptcha]
        [ValidateAntiForgeryToken]
        [GdprConsent]
        public ActionResult ReviewsAdd(int id, ProductReviewsModel model, string captchaError)
        {
            var product = _productService.GetProductById(id);
            if (product == null || product.Deleted || product.IsSystemProduct || !product.Published || !product.AllowCustomerReviews)
            {
                return HttpNotFound();
            }

            if (_captchaSettings.ShowOnProductReviewPage && captchaError.HasValue())
            {
                ModelState.AddModelError("", captchaError);
            }

            if (_services.WorkContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                ModelState.AddModelError("", T("Reviews.OnlyRegisteredUsersCanWriteReviews"));
            }

            if (ModelState.IsValid)
            {
                var rating = model.Rating;
                if (rating < 1 || rating > 5)
                {
                    rating = _catalogSettings.DefaultProductRatingValue;
                }

                var isApproved = !_catalogSettings.ProductReviewsMustBeApproved;
                var customer = _services.WorkContext.CurrentCustomer;

                var productReview = new ProductReview
                {
                    ProductId = product.Id,
                    CustomerId = customer.Id,
                    IpAddress = _services.WebHelper.GetCurrentIpAddress(),
                    Title = model.Title,
                    ReviewText = model.ReviewText,
                    Rating = rating,
                    HelpfulYesTotal = 0,
                    HelpfulNoTotal = 0,
                    IsApproved = isApproved,
                };
                _customerContentService.InsertCustomerContent(productReview);

                _productService.UpdateProductReviewTotals(product);

                if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                {
                    Services.MessageFactory.SendProductReviewNotificationMessage(productReview, _localizationSettings.DefaultAdminLanguageId);
                }

                _services.CustomerActivity.InsertActivity("PublicStore.AddProductReview", T("ActivityLog.PublicStore.AddProductReview"), product.Name);

                if (isApproved)
                {
                    _customerService.RewardPointsForProductReview(customer, product, true);
                }

                _helper.PrepareProductReviewsModel(model, product);

                TempData["SuccessfullyAdded"] = true;

                return RedirectToAction("Reviews");
            }

            // If we got this far, something failed, redisplay form.
            _helper.PrepareProductReviewsModel(model, product);
            return View(model);
        }

        [HttpPost]
        public ActionResult SetReviewHelpfulness(int productReviewId, bool washelpful)
        {
            var productReview = _customerContentService.GetCustomerContentById(productReviewId) as ProductReview;
            if (productReview == null)
                throw new ArgumentException(T("Reviews.NotFound", productReviewId));

            if (_services.WorkContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                return Json(new
                {
                    Success = false,
                    Result = T("Reviews.Helpfulness.OnlyRegistered").Text,
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            //customers aren't allowed to vote for their own reviews
            if (productReview.CustomerId == _services.WorkContext.CurrentCustomer.Id)
            {
                return Json(new
                {
                    Success = false,
                    Result = T("Reviews.Helpfulness.YourOwnReview").Text,
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            // delete previous helpfulness
            var oldPrh = (from prh in productReview.ProductReviewHelpfulnessEntries
                          where prh.CustomerId == _services.WorkContext.CurrentCustomer.Id
                          select prh).FirstOrDefault();
            if (oldPrh != null)
                _customerContentService.DeleteCustomerContent(oldPrh);

            // insert new helpfulness
            var newPrh = new ProductReviewHelpfulness
            {
                ProductReviewId = productReview.Id,
                CustomerId = _services.WorkContext.CurrentCustomer.Id,
                IpAddress = _services.WebHelper.GetCurrentIpAddress(),
                WasHelpful = washelpful,
                IsApproved = true, //always approved
            };
            _customerContentService.InsertCustomerContent(newPrh);

            // new totals
            int helpfulYesTotal = (from prh in productReview.ProductReviewHelpfulnessEntries
                                   where prh.WasHelpful
                                   select prh).Count();
            int helpfulNoTotal = (from prh in productReview.ProductReviewHelpfulnessEntries
                                  where !prh.WasHelpful
                                  select prh).Count();

            productReview.HelpfulYesTotal = helpfulYesTotal;
            productReview.HelpfulNoTotal = helpfulNoTotal;
            _customerContentService.UpdateCustomerContent(productReview);

            return Json(new
            {
                Success = true,
                Result = T("Reviews.Helpfulness.SuccessfullyVoted").Text,
                TotalYes = productReview.HelpfulYesTotal,
                TotalNo = productReview.HelpfulNoTotal
            });
        }

        #endregion


        #region Ask product question

        public ActionResult AskQuestionAjax(int id, ProductVariantQuery query)
        {
            // Get attributeXml from product variant query
            if (query != null && id > 0)
            {
                var attributes = _productAttributeService.GetProductVariantAttributesByProductId(id);
                var warnings = new List<string>();
                var attributesXml = query.CreateSelectedAttributesXml(id, 0, attributes, _productAttributeParser,
                   _localizationService, _downloadService, _catalogSettings, null, warnings);

                if (attributesXml.HasValue())
                {
                    TempData.Add("AskQuestionAttributesXml-" + id, attributesXml);
                }
            }

            return new JsonResult
            {
                Data = new { redirect = Url.Action("AskQuestion", new { id }) },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        [GdprConsent]
        public ActionResult AskQuestion(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null || product.Deleted || product.IsSystemProduct || !product.Published || !_catalogSettings.AskQuestionEnabled)
                return HttpNotFound();

            var attributesXml = "";
            if (TempData.TryGetValue("AskQuestionAttributesXml-" + id, out var obj))
            {
                attributesXml = obj as string;
            }

            // Check if saved attributeXml belongs to current product id
            var attributeInfo = "";
            if (attributesXml.HasValue())
            {
                attributeInfo = _productAttributeFormatter.FormatAttributes(
                            product,
                            attributesXml,
                            null,
                            separator: ", ",
                            renderPrices: false,
                            renderGiftCardAttributes: false,
                            allowHyperlinks: false);
            }

            var customer = _services.WorkContext.CurrentCustomer;
            var model = new ProductAskQuestionModel
            {
                Id = product.Id,
                ProductName = product.GetLocalized(x => x.Name),
                ProductSeName = product.GetSeName(),
                SenderEmail = customer.Email,
                SenderName = customer.GetFullName(),
                SenderNameRequired = _privacySettings.Value.FullNameOnProductRequestRequired,
                SenderPhone = customer.GetAttribute<string>(SystemCustomerAttributeNames.Phone),
                DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnAskQuestionPage,
                SelectedAttributes = attributeInfo,
                ProductUrl = _productUrlHelper.GetProductUrl(id, product.GetSeName(), attributesXml),
                IsQuoteRequest = product.CallForPrice
            };

            var questionTitle = model.IsQuoteRequest
                ? T("Products.AskQuestion.Question.QuoteRequest")
                : T("Products.AskQuestion.Question.GeneralInquiry");

            model.Question = questionTitle.Text.FormatCurrentUI(model.ProductName);

            return View(model);
        }

        [HttpPost, ActionName("AskQuestion")]
        [ValidateCaptcha, ValidateHoneypot]
        [GdprConsent]
        public ActionResult AskQuestionSend(ProductAskQuestionModel model, string captchaError)
        {
            var product = _productService.GetProductById(model.Id);
            if (product == null || product.Deleted || product.IsSystemProduct || !product.Published || !_catalogSettings.AskQuestionEnabled)
                return HttpNotFound();

            if (_captchaSettings.ShowOnAskQuestionPage && captchaError.HasValue())
            {
                ModelState.AddModelError("", captchaError);
            }

            model.ProductUrl = _services.StoreContext.CurrentStore.Url + model.ProductUrl.Substring(1);

            if (ModelState.IsValid)
            {
                var msg = Services.MessageFactory.SendProductQuestionMessage(
                    _services.WorkContext.CurrentCustomer,
                    product,
                    model.SenderEmail,
                    model.SenderName,
                    model.SenderPhone,
                    HtmlUtils.ConvertPlainTextToHtml(model.Question.HtmlEncode()),
                    HtmlUtils.ConvertPlainTextToHtml(model.SelectedAttributes.HtmlEncode()),
                    model.ProductUrl,
                    model.IsQuoteRequest);

                if (msg?.Email?.Id != null)
                {
                    NotifySuccess(T("Products.AskQuestion.Sent"), true);
                    return RedirectToRoute("Product", new { SeName = product.GetSeName() });
                }
                else
                {
                    ModelState.AddModelError("", T("Common.Error.SendMail"));
                }
            }

            // If we got this far, something failed, redisplay form.
            var customer = _services.WorkContext.CurrentCustomer;
            model.Id = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnAskQuestionPage;

            return View(model);
        }

        #endregion

        #region Email a friend

        [RewriteUrl(SslRequirement.No)]
        [GdprConsent]
        public ActionResult EmailAFriend(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null || product.Deleted || product.IsSystemProduct || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return HttpNotFound();

            var model = new ProductEmailAFriendModel();
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.YourEmailAddress = _services.WorkContext.CurrentCustomer.Email;
            model.AllowChangedCustomerEmail = _catalogSettings.AllowDifferingEmailAddressForEmailAFriend;
            model.DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnEmailProductToFriendPage;

            return View(model);
        }

        [HttpPost, ActionName("EmailAFriend")]
        [ValidateCaptcha]
        [GdprConsent]
        public ActionResult EmailAFriendSend(ProductEmailAFriendModel model, int id, string captchaError)
        {
            var product = _productService.GetProductById(id);
            if (product == null || product.Deleted || product.IsSystemProduct || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return HttpNotFound();

            if (_captchaSettings.ShowOnEmailProductToFriendPage && captchaError.HasValue())
            {
                ModelState.AddModelError("", captchaError);
            }

            // Check whether the current customer is guest and ia allowed to email a friend.
            if (_services.WorkContext.CurrentCustomer.IsGuest() && !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
            {
                ModelState.AddModelError("", T("Products.EmailAFriend.OnlyRegisteredUsers"));
            }

            if (ModelState.IsValid)
            {
                //email
                Services.MessageFactory.SendShareProductMessage(
                    _services.WorkContext.CurrentCustomer,
                    product,
                    model.YourEmailAddress,
                    model.FriendEmail,
                    Core.Html.HtmlUtils.ConvertPlainTextToHtml(model.PersonalMessage.HtmlEncode()));

                model.ProductId = product.Id;
                model.ProductName = product.GetLocalized(x => x.Name);
                model.ProductSeName = product.GetSeName();

                NotifySuccess(T("Products.EmailAFriend.SuccessfullySent"));

                return RedirectToRoute("Product", new { SeName = model.ProductSeName });
            }

            // If we got this far, something failed, redisplay form.
            model.ProductId = product.Id;
            model.ProductName = product.GetLocalized(x => x.Name);
            model.ProductSeName = product.GetSeName();
            model.AllowChangedCustomerEmail = _catalogSettings.AllowDifferingEmailAddressForEmailAFriend;
            model.DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnEmailProductToFriendPage;

            return View(model);
        }

        #endregion

    }
}
