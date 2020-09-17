using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.DataExchange.Export;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Search;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Controllers
{
    public partial class CatalogHelper
    {
        public void MapListActions(ProductSummaryModel model, IPagingOptions entity, string defaultPageSizeOptions)
        {
            var searchQuery = _catalogSearchQueryFactory.Current;

            // View mode
            model.AllowViewModeChanging = _catalogSettings.AllowProductViewModeChanging;

            // Sorting
            model.AllowSorting = _catalogSettings.AllowProductSorting;
            if (model.AllowSorting)
            {
                model.CurrentSortOrder = searchQuery?.CustomData.Get("CurrentSortOrder").Convert<int?>();

                model.AvailableSortOptions = _services.Cache.Get("pres:productlistsortoptions-{0}".FormatInvariant(_services.WorkContext.WorkingLanguage.Id), () =>
                {
                    var dict = new Dictionary<int, string>();
                    foreach (ProductSortingEnum enumValue in Enum.GetValues(typeof(ProductSortingEnum)))
                    {
                        if (enumValue == ProductSortingEnum.CreatedOnAsc || enumValue == ProductSortingEnum.Initial)
                            continue;

                        dict[(int)enumValue] = enumValue.GetLocalizedEnum(_localizationService, _services.WorkContext);
                    }

                    return dict;
                });

                if (!searchQuery.Origin.IsCaseInsensitiveEqual("Search/Search"))
                {
                    model.RelevanceSortOrderName = T("Products.Sorting.Featured");
                    if ((int)ProductSortingEnum.Relevance == (model.CurrentSortOrder ?? 1))
                    {
                        model.CurrentSortOrderName = model.RelevanceSortOrderName;
                    }
                }

                if (model.CurrentSortOrderName.IsEmpty())
                {
                    model.CurrentSortOrderName = model.AvailableSortOptions.Get(model.CurrentSortOrder ?? 1) ?? model.AvailableSortOptions.First().Value;
                }
            }

            // Pagination
            if (entity?.AllowCustomersToSelectPageSize ?? _catalogSettings.AllowCustomersToSelectPageSize)
            {
                try
                {
                    model.AvailablePageSizes = (entity?.PageSizeOptions.NullEmpty() ?? defaultPageSizeOptions).Convert<List<int>>();
                }
                catch
                {
                    model.AvailablePageSizes = new int[] { 12, 24, 36, 48, 72, 120 };
                }
            }

            model.AllowFiltering = true;
        }

        public ProductSummaryMappingSettings GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode viewMode)
        {
            return GetBestFitProductSummaryMappingSettings(viewMode, null);
        }

        public ProductSummaryMappingSettings GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode viewMode, Action<ProductSummaryMappingSettings> fn)
        {
            var settings = new ProductSummaryMappingSettings
            {
                ViewMode = viewMode,
                MapPrices = true,
                MapPictures = true,
                ThumbnailSize = _mediaSettings.ProductThumbPictureSize
            };

            if (viewMode == ProductSummaryViewMode.Grid)
            {
                settings.MapShortDescription = _catalogSettings.ShowShortDescriptionInGridStyleLists;
                settings.MapManufacturers = _catalogSettings.ShowManufacturerInGridStyleLists;
                settings.MapColorAttributes = _catalogSettings.ShowColorSquaresInLists;
                settings.MapAttributes = _catalogSettings.ShowProductOptionsInLists;
                settings.MapReviews = _catalogSettings.ShowProductReviewsInProductLists;
                settings.DeliveryTimesPresentation = _catalogSettings.DeliveryTimesInLists;
            }
            else if (viewMode == ProductSummaryViewMode.List)
            {
                settings.MapShortDescription = true;
                settings.MapLegalInfo = _taxSettings.ShowLegalHintsInProductList;
                settings.MapManufacturers = true;
                settings.MapColorAttributes = _catalogSettings.ShowColorSquaresInLists;
                settings.MapAttributes = _catalogSettings.ShowProductOptionsInLists;
                //settings.MapSpecificationAttributes = true; // TODO: (mc) What about SpecAttrs in List-Mode (?) Option?
                settings.MapReviews = _catalogSettings.ShowProductReviewsInProductLists;
                settings.DeliveryTimesPresentation = _catalogSettings.DeliveryTimesInLists;
                settings.MapDimensions = _catalogSettings.ShowDimensions;
            }
            else if (viewMode == ProductSummaryViewMode.Compare)
            {
                settings.MapShortDescription = _catalogSettings.IncludeShortDescriptionInCompareProducts;
                settings.MapFullDescription = _catalogSettings.IncludeFullDescriptionInCompareProducts;
                settings.MapLegalInfo = _taxSettings.ShowLegalHintsInProductList;
                settings.MapManufacturers = true;
                settings.MapAttributes = true;
                settings.MapSpecificationAttributes = true;
                settings.MapReviews = _catalogSettings.ShowProductReviewsInProductLists;
                settings.DeliveryTimesPresentation = _catalogSettings.DeliveryTimesInLists;
                settings.MapDimensions = _catalogSettings.ShowDimensions;
            }

            fn?.Invoke(settings);

            return settings;
        }

        public virtual ProductSummaryModel MapProductSummaryModel(IList<Product> products, ProductSummaryMappingSettings settings)
        {
            Guard.NotNull(products, nameof(products));

            return MapProductSummaryModel(new PagedList<Product>(products, 0, int.MaxValue), settings);
        }

        public virtual ProductSummaryModel MapProductSummaryModel(IPagedList<Product> products, ProductSummaryMappingSettings settings)
        {
            Guard.NotNull(products, nameof(products));

            if (settings == null)
            {
                settings = new ProductSummaryMappingSettings();
            }

            using (_services.Chronometer.Step("MapProductSummaryModel"))
            {
                var model = new ProductSummaryModel(products)
                {
                    ViewMode = settings.ViewMode,
                    GridColumnSpan = _catalogSettings.GridStyleListColumnSpan,
                    ShowSku = _catalogSettings.ShowProductSku,
                    ShowWeight = _catalogSettings.ShowWeight,
                    ShowDimensions = settings.MapDimensions,
                    ShowLegalInfo = settings.MapLegalInfo,
                    ShowDescription = settings.MapShortDescription,
                    ShowFullDescription = settings.MapFullDescription,
                    ShowRatings = settings.MapReviews,
                    ShowPrice = settings.MapPrices,
                    ShowBasePrice = settings.MapPrices && _catalogSettings.ShowBasePriceInProductLists && settings.ViewMode != ProductSummaryViewMode.Mini,
                    ShowShippingSurcharge = settings.MapPrices && settings.ViewMode != ProductSummaryViewMode.Mini,
                    ShowButtons = settings.ViewMode != ProductSummaryViewMode.Mini,
                    ShowBrand = settings.MapManufacturers,
                    ForceRedirectionAfterAddingToCart = settings.ForceRedirectionAfterAddingToCart,
                    CompareEnabled = _catalogSettings.CompareProductsEnabled,
                    WishlistEnabled = _services.Permissions.Authorize(Permissions.Cart.AccessWishlist),
                    BuyEnabled = !_catalogSettings.HideBuyButtonInLists,
                    ThumbSize = settings.ThumbnailSize,
                    ShowDiscountBadge = _catalogSettings.ShowDiscountSign,
                    ShowNewBadge = _catalogSettings.LabelAsNewForMaxDays.HasValue,
                    DeliveryTimesPresentation = settings.DeliveryTimesPresentation,
                };

                if (products.Count == 0)
                {
                    // No products, stop here.
                    return model;
                }

                // PERF!!
                var store = _services.StoreContext.CurrentStore;
                var customer = _services.WorkContext.CurrentCustomer;
                var currency = _services.WorkContext.WorkingCurrency;
                var language = _services.WorkContext.WorkingLanguage;
                var allowPrices = _services.Permissions.Authorize(Permissions.Catalog.DisplayPrice);
                var allowShoppingCart = _services.Permissions.Authorize(Permissions.Cart.AccessShoppingCart);
                var allowWishlist = _services.Permissions.Authorize(Permissions.Cart.AccessWishlist);
                var taxDisplayType = _services.WorkContext.GetTaxDisplayTypeFor(customer, store.Id);
                var cachedManufacturerModels = new Dictionary<int, ManufacturerOverviewModel>();
                var prefetchTranslations = settings.PrefetchTranslations == true || (settings.PrefetchTranslations == null && _performanceSettings.AlwaysPrefetchTranslations);
                var prefetchSlugs = settings.PrefetchUrlSlugs == true || (settings.PrefetchUrlSlugs == null && _performanceSettings.AlwaysPrefetchUrlSlugs);
                var allProductIds = prefetchSlugs || prefetchTranslations ? products.Select(x => x.Id).ToArray() : new int[0];

                //var productIds = products.Select(x => x.Id).ToArray();

                string taxInfo = T(taxDisplayType == TaxDisplayType.IncludingTax ? "Tax.InclVAT" : "Tax.ExclVAT");
                var legalInfo = "";

                var res = new Dictionary<string, LocalizedString>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Products.CallForPrice", T("Products.CallForPrice") },
                    { "Products.PriceRangeFrom", T("Products.PriceRangeFrom") },
                    { "Media.Product.ImageLinkTitleFormat", T("Media.Product.ImageLinkTitleFormat") },
                    { "Media.Product.ImageAlternateTextFormat", T("Media.Product.ImageAlternateTextFormat") },
                    { "Products.DimensionsValue", T("Products.DimensionsValue") },
                    { "Common.AdditionalShippingSurcharge", T("Common.AdditionalShippingSurcharge") }
                };

                if (settings.MapLegalInfo)
                {
                    var shippingInfoUrl = _urlHelper.Topic("shippinginfo").ToString();
                    legalInfo = shippingInfoUrl.HasValue()
                        ? T("Tax.LegalInfoShort").Text.FormatInvariant(taxInfo, shippingInfoUrl)
                        : T("Tax.LegalInfoShort2").Text.FormatInvariant(taxInfo);
                }

                if (prefetchSlugs)
                {
                    _urlRecordService.PrefetchUrlRecords(nameof(Product), new[] { language.Id, 0 }, allProductIds);
                }

                if (prefetchTranslations)
                {
                    // Prefetch all delivery time translations
                    _localizedEntityService.PrefetchLocalizedProperties(nameof(DeliveryTime), language.Id, null);
                }

                using (var scope = new DbContextScope(ctx: _services.DbContext, autoCommit: false, validateOnSave: false))
                {
                    // Run in uncommitting scope, because pictures could be updated (IsNew property) 
                    var batchContext = _dataExporter.Value.CreateProductExportContext(products, customer, null, 1, false);

                    if (settings.MapPrices)
                    {
                        batchContext.AppliedDiscounts.LoadAll();
                        batchContext.TierPrices.LoadAll();
                    }

                    if (settings.MapAttributes || settings.MapColorAttributes)
                    {
                        batchContext.Attributes.LoadAll();

                        if (prefetchTranslations)
                        {
                            // Prefetch all product attribute translations
                            PrefetchTranslations(
                                nameof(ProductAttribute),
                                language.Id,
                                batchContext.Attributes.SelectMany(x => x.Value).Select(x => x.ProductAttribute));

                            // Prefetch all variant attribute value translations
                            PrefetchTranslations(
                                nameof(ProductVariantAttributeValue),
                                language.Id,
                                batchContext.Attributes.SelectMany(x => x.Value).SelectMany(x => x.ProductVariantAttributeValues));
                        }
                    }

                    if (settings.MapManufacturers)
                    {
                        batchContext.ProductManufacturers.LoadAll();
                    }

                    if (settings.MapSpecificationAttributes)
                    {
                        batchContext.SpecificationAttributes.LoadAll();

                        if (prefetchTranslations)
                        {
                            // Prefetch all spec attribute option translations
                            PrefetchTranslations(
                                nameof(SpecificationAttributeOption),
                                language.Id,
                                batchContext.SpecificationAttributes.SelectMany(x => x.Value).Select(x => x.SpecificationAttributeOption));

                            // Prefetch all spec attribute translations
                            PrefetchTranslations(
                                nameof(SpecificationAttribute),
                                language.Id,
                                batchContext.SpecificationAttributes.SelectMany(x => x.Value).Select(x => x.SpecificationAttributeOption.SpecificationAttribute));
                        }
                    }

                    // If a size has been set in the view, we use it in priority
                    int thumbSize = model.ThumbSize ?? _mediaSettings.ProductThumbPictureSize;

                    var mapItemContext = new MapProductSummaryItemContext
                    {
                        BatchContext = batchContext,
                        CachedManufacturerModels = cachedManufacturerModels,
                        Currency = currency,
                        LegalInfo = legalInfo,
                        Model = model,
                        Resources = res,
                        Settings = settings,
                        Customer = customer,
                        Store = store,
                        AllowPrices = allowPrices,
                        AllowShoppingCart = allowShoppingCart,
                        AllowWishlist = allowWishlist,
                        TaxDisplayType = taxDisplayType
                    };

                    if (settings.MapPictures)
                    {
                        var fileIds = products
                            .Select(x => x.MainPictureId ?? 0)
                            .Where(x => x != 0)
                            .Distinct()
                            .ToArray();

                        mapItemContext.MediaFiles = _mediaService.GetFilesByIds(fileIds).ToDictionarySafe(x => x.Id);
                    }

                    foreach (var product in products)
                    {
                        MapProductSummaryItem(product, mapItemContext);
                    }

                    _services.DisplayControl.AnnounceRange(products);

                    scope.Commit();

                    batchContext.Clear();

                    // don't show stuff without data at all
                    model.ShowDescription = model.ShowDescription && model.Items.Any(x => x.ShortDescription?.Value?.HasValue() == true);
                    model.ShowBrand = model.ShowBrand && model.Items.Any(x => x.Manufacturer != null);

                    return model;
                }
            }
        }

        private void PrefetchTranslations(string keyGroup, int languageId, IEnumerable<BaseEntity> entities)
        {
            if (entities.Any())
            {
                _localizedEntityService.PrefetchLocalizedProperties(keyGroup, languageId, entities.Select(x => x.Id).Distinct().ToArray());
            }
        }

        private void MapProductSummaryItem(Product product, MapProductSummaryItemContext ctx)
        {
            var contextProduct = product;
            var finalPrice = decimal.Zero;
            var model = ctx.Model;
            var settings = ctx.Settings;

            var item = new ProductSummaryModel.SummaryItem(ctx.Model)
            {
                Id = product.Id,
                Name = product.GetLocalized(x => x.Name),
                SeName = product.GetSeName()
            };

            if (model.ShowDescription)
            {
                item.ShortDescription = product.GetLocalized(x => x.ShortDescription);
            }

            if (settings.MapFullDescription)
            {
                item.FullDescription = product.GetLocalized(x => x.FullDescription, detectEmptyHtml: true);
            }

            // Price
            if (settings.MapPrices)
            {
                finalPrice = MapSummaryItemPrice(product, ref contextProduct, item, ctx);
            }

            // (Color) Attributes
            if (settings.MapColorAttributes || settings.MapAttributes)
            {
                var attributes = ctx.BatchContext.Attributes.GetOrLoad(contextProduct.Id);
                var cachedAttributeNames = new Dictionary<int, LocalizedValue<string>>();

                // Color squares
                if (attributes.Any() && settings.MapColorAttributes)
                {
                    var colorAttributes = attributes
                        .Where(x => x.IsListTypeAttribute())
                        .SelectMany(x => x.ProductVariantAttributeValues)
                        .Where(x => x.Color.HasValue() && !x.Color.IsCaseInsensitiveEqual("transparent"))
                        .Distinct()
                        .Take(20) // limit results
                        .Select(x =>
                        {
                            var attr = x.ProductVariantAttribute.ProductAttribute;
                            var attrName = cachedAttributeNames.Get(attr.Id) ?? (cachedAttributeNames[attr.Id] = attr.GetLocalized(l => l.Name));

                            return new ProductSummaryModel.ColorAttributeValue
                            {
                                Id = x.Id,
                                Color = x.Color,
                                Alias = x.Alias,
                                FriendlyName = x.GetLocalized(l => l.Name),
                                AttributeId = x.ProductVariantAttributeId,
                                AttributeName = attrName,
                                ProductAttributeId = attr.Id,
                                ProductUrl = _productUrlHelper.GetProductUrl(product.Id, item.SeName, 0, x)
                            };
                        })
                        .ToList();

                    item.ColorAttributes = colorAttributes;

                    // TODO: (mc) Resolve attribute value images also
                }

                // Variant Attributes
                if (attributes.Any() && settings.MapAttributes)
                {
                    if (item.ColorAttributes != null && item.ColorAttributes.Any())
                    {
                        var processedIds = item.ColorAttributes.Select(x => x.AttributeId).Distinct().ToArray();
                        attributes = attributes.Where(x => !processedIds.Contains(x.Id)).ToList();
                    }

                    foreach (var attr in attributes)
                    {
                        var pa = attr.ProductAttribute;
                        item.Attributes.Add(new ProductSummaryModel.Attribute
                        {
                            Id = attr.Id,
                            Alias = pa.Alias,
                            Name = cachedAttributeNames.Get(pa.Id) ?? (cachedAttributeNames[pa.Id] = pa.GetLocalized(l => l.Name))
                        });
                    }
                }
            }

            // Picture
            if (settings.MapPictures)
            {
                var thumbSize = model.ThumbSize ?? _mediaSettings.ProductThumbPictureSize;

                ctx.MediaFiles.TryGetValue(product.MainPictureId ?? 0, out var file);

                item.Picture = new PictureModel
                {
                    PictureId = file?.Id ?? 0,
                    Size = thumbSize,
                    ImageUrl = _mediaService.GetUrl(file, thumbSize, null, !_catalogSettings.HideProductDefaultPictures),
                    FullSizeImageUrl = _mediaService.GetUrl(file, 0, null, !_catalogSettings.HideProductDefaultPictures),
                    FullSizeImageWidth = file?.Dimensions.Width,
                    FullSizeImageHeight = file?.Dimensions.Height,
                    Title = file?.File?.GetLocalized(x => x.Title)?.Value.NullEmpty() ?? string.Format(ctx.Resources["Media.Product.ImageLinkTitleFormat"], item.Name),
                    AlternateText = file?.File?.GetLocalized(x => x.Alt)?.Value.NullEmpty() ?? string.Format(ctx.Resources["Media.Product.ImageAlternateTextFormat"], item.Name),
                    File = file
                };

                _services.DisplayControl.Announce(file?.File);
            }

            // Manufacturers
            if (settings.MapManufacturers)
            {
                item.Manufacturer = PrepareManufacturersOverviewModel(
                    ctx.BatchContext.ProductManufacturers.GetOrLoad(product.Id),
                    ctx.CachedManufacturerModels,
                    _catalogSettings.ShowManufacturerLogoInLists && settings.ViewMode == ProductSummaryViewMode.List).FirstOrDefault();
            }

            // Spec Attributes
            if (settings.MapSpecificationAttributes)
            {
                item.SpecificationAttributes.AddRange(MapProductSpecificationModels(ctx.BatchContext.SpecificationAttributes.GetOrLoad(product.Id)));
            }

            item.MinPriceProductId = contextProduct.Id;
            item.Sku = contextProduct.Sku;

            // Measure Dimensions
            if (model.ShowDimensions && (contextProduct.Width != 0 || contextProduct.Height != 0 || contextProduct.Length != 0))
            {
                item.Dimensions = ctx.Resources["Products.DimensionsValue"].Text.FormatCurrent(
                    contextProduct.Width.ToString("N2"),
                    contextProduct.Height.ToString("N2"),
                    contextProduct.Length.ToString("N2")
                );
                item.DimensionMeasureUnit = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId).SystemKeyword;
            }

            // Delivery Times.
            item.HideDeliveryTime = product.ProductType == ProductType.GroupedProduct;
            if (!item.HideDeliveryTime && model.DeliveryTimesPresentation != DeliveryTimesPresentation.None)
            {
                // We cannot include ManageInventoryMethod.ManageStockByAttributes because it's only functional with MergeWithCombination.
                //item.StockAvailablity = contextProduct.FormatStockMessage(_localizationService);
                //item.DisplayDeliveryTimeAccordingToStock = contextProduct.DisplayDeliveryTimeAccordingToStock(_catalogSettings);

                //var deliveryTime = _deliveryTimeService.GetDeliveryTime(contextProduct);
                //if (deliveryTime != null)
                //{
                //	item.DeliveryTimeName = deliveryTime.GetLocalized(x => x.Name);
                //	item.DeliveryTimeHexValue = deliveryTime.ColorHexValue;
                //}

                var deliveryTimeId = product.DeliveryTimeId ?? 0;
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock && product.StockQuantity <= 0 && _catalogSettings.DeliveryTimeIdForEmptyStock.HasValue)
                {
                    deliveryTimeId = _catalogSettings.DeliveryTimeIdForEmptyStock.Value;
                }

                var deliveryTime = _deliveryTimeService.GetDeliveryTimeById(deliveryTimeId);
                if (deliveryTime != null)
                {
                    item.DeliveryTimeName = deliveryTime.GetLocalized(x => x.Name);
                    item.DeliveryTimeHexValue = deliveryTime.ColorHexValue;

                    // Due to lack of space, the grid view does not show a date for the delivery time.
                    if (settings.ViewMode >= ProductSummaryViewMode.List &&
                        (model.DeliveryTimesPresentation == DeliveryTimesPresentation.DateOnly || model.DeliveryTimesPresentation == DeliveryTimesPresentation.LabelAndDate))
                    {
                        item.DeliveryTimeDate = _deliveryTimeService.GetFormattedDeliveryDate(deliveryTime);
                    }
                }

                item.DisplayDeliveryTimeAccordingToStock = product.ManageInventoryMethod == ManageInventoryMethod.ManageStock
                    ? product.StockQuantity > 0 || (product.StockQuantity <= 0 && _catalogSettings.DeliveryTimeIdForEmptyStock.HasValue)
                    : true;

                if (product.DisplayStockAvailability && product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                {
                    if (product.StockQuantity > 0)
                    {
                        item.StockAvailablity = product.DisplayStockQuantity
                            ? T("Products.Availability.InStockWithQuantity", product.StockQuantity)
                            : T("Products.Availability.InStock");
                    }
                    else
                    {
                        item.StockAvailablity = product.BackorderMode == BackorderMode.NoBackorders || product.BackorderMode == BackorderMode.AllowQtyBelow0
                            ? T("Products.Availability.OutOfStock")
                            : T("Products.Availability.Backordering");
                    }
                }
            }

            item.LegalInfo = ctx.LegalInfo;
            item.RatingSum = product.ApprovedRatingSum;
            item.TotalReviews = product.ApprovedTotalReviews;
            item.IsShippingEnabled = contextProduct.IsShipEnabled;

            if (finalPrice != decimal.Zero && model.ShowBasePrice)
            {
                item.BasePriceInfo = contextProduct.GetBasePriceInfo(finalPrice, _localizationService, _priceFormatter, ctx.Currency);
            }

            if (settings.MapPrices)
            {
                var addShippingPrice = _currencyService.ConvertCurrency(contextProduct.AdditionalShippingCharge, ctx.Store.PrimaryStoreCurrency, ctx.Currency);

                if (addShippingPrice > 0)
                {
                    item.TransportSurcharge = ctx.Resources["Common.AdditionalShippingSurcharge"].Text.FormatCurrent(_priceFormatter.FormatPrice(addShippingPrice, true, false));
                }

                item.PriceDisplayStyle = _catalogSettings.PriceDisplayStyle;
                item.DisplayTextForZeroPrices = _catalogSettings.DisplayTextForZeroPrices;
            }

            if (model.ShowWeight && contextProduct.Weight > 0)
            {
                var measureWeightName = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId)?.GetLocalized(x => x.Name) ?? string.Empty;

                item.Weight = "{0} {1}".FormatCurrent(contextProduct.Weight.ToString("N2"), measureWeightName);
            }

            // New Badge
            if (product.IsNew(_catalogSettings))
            {
                item.Badges.Add(new ProductSummaryModel.Badge
                {
                    Label = T("Common.New"),
                    Style = BadgeStyle.Success
                });
            }

            model.Items.Add(item);
        }

        /// <param name="contextProduct">The product or the first associated product of a group.</param>
        /// <returns>The final price</returns>
        private decimal MapSummaryItemPrice(Product product, ref Product contextProduct, ProductSummaryModel.SummaryItem item, MapProductSummaryItemContext ctx)
        {
            var displayFromMessage = false;
            var taxRate = decimal.Zero;
            var oldPriceBase = decimal.Zero;
            var oldPrice = decimal.Zero;
            var finalPriceBase = decimal.Zero;
            var finalPrice = decimal.Zero;
            var displayPrice = decimal.Zero;
            ICollection<Product> associatedProducts = null;

            var priceModel = new ProductSummaryModel.PriceModel();
            item.Price = priceModel;

            if (product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing && !ctx.BatchContext.ProductBundleItems.FullyLoaded)
            {
                ctx.BatchContext.ProductBundleItems.LoadAll();
            }

            if (product.ProductType == ProductType.GroupedProduct)
            {
                priceModel.DisableBuyButton = true;
                priceModel.DisableWishlistButton = true;
                priceModel.AvailableForPreOrder = false;

                if (ctx.GroupedProducts == null)
                {
                    // One-time batched retrieval of all associated products.
                    var searchQuery = new CatalogSearchQuery()
                        .PublishedOnly(true)
                        .HasStoreId(ctx.Store.Id)
                        .HasParentGroupedProduct(ctx.BatchContext.ProductIds.ToArray());

                    // Get all associated products for this batch grouped by ParentGroupedProductId.
                    var allAssociatedProducts = _catalogSearchService.Search(searchQuery).Hits
                        .OrderBy(x => x.ParentGroupedProductId)
                        .ThenBy(x => x.DisplayOrder);

                    ctx.GroupedProducts = allAssociatedProducts.ToMultimap(x => x.ParentGroupedProductId, x => x);
                    ctx.AssociatedProductBatchContext = _dataExporter.Value.CreateProductExportContext(allAssociatedProducts, ctx.Customer, null, null, false);
                }

                associatedProducts = ctx.GroupedProducts[product.Id];
                if (associatedProducts.Any())
                {
                    contextProduct = associatedProducts.OrderBy(x => x.DisplayOrder).First();

                    _services.DisplayControl.Announce(contextProduct);
                }
            }
            else
            {
                priceModel.DisableBuyButton = product.DisableBuyButton || !ctx.AllowShoppingCart || !ctx.AllowPrices;
                priceModel.DisableWishlistButton = product.DisableWishlistButton || !ctx.AllowWishlist || !ctx.AllowPrices;
                priceModel.AvailableForPreOrder = product.AvailableForPreOrder;
            }

            // Return if there's no pricing at all.
            if (contextProduct == null || contextProduct.CustomerEntersPrice || !ctx.AllowPrices || _catalogSettings.PriceDisplayType == PriceDisplayType.Hide)
            {
                return finalPrice;
            }

            // Return if group has no associated products.
            if (product.ProductType == ProductType.GroupedProduct && !associatedProducts.Any())
            {
                return finalPrice;
            }

            // Call for price.
            priceModel.CallForPrice = contextProduct.CallForPrice;
            if (contextProduct.CallForPrice)
            {
                priceModel.Price = ctx.Resources["Products.CallForPrice"];
                return finalPrice;
            }

            // Calculate prices.
            var batchContext = product.ProductType == ProductType.GroupedProduct ? ctx.AssociatedProductBatchContext : ctx.BatchContext;

            if (_catalogSettings.PriceDisplayType == PriceDisplayType.PreSelectedPrice)
            {
                displayPrice = _priceCalculationService.GetPreselectedPrice(contextProduct, ctx.Customer, ctx.Currency, batchContext);
            }
            else if (_catalogSettings.PriceDisplayType == PriceDisplayType.PriceWithoutDiscountsAndAttributes)
            {
                displayPrice = _priceCalculationService.GetFinalPrice(contextProduct, null, ctx.Customer, decimal.Zero, false, 1, null, batchContext);
            }
            else
            {
                // Display lowest price.
                if (product.ProductType == ProductType.GroupedProduct)
                {
                    displayFromMessage = true;
                    displayPrice = _priceCalculationService.GetLowestPrice(product, ctx.Customer, batchContext, associatedProducts, out contextProduct) ?? decimal.Zero;
                }
                else
                {
                    displayPrice = _priceCalculationService.GetLowestPrice(product, ctx.Customer, batchContext, out displayFromMessage);
                }
            }

            oldPriceBase = _taxService.GetProductPrice(contextProduct, contextProduct.OldPrice, out taxRate);
            finalPriceBase = _taxService.GetProductPrice(contextProduct, displayPrice, out taxRate);

            oldPrice = _currencyService.ConvertFromPrimaryStoreCurrency(oldPriceBase, ctx.Currency);
            finalPrice = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceBase, ctx.Currency);

            priceModel.PriceValue = finalPrice;
            priceModel.Price = displayFromMessage
                ? string.Format(ctx.Resources["Products.PriceRangeFrom"], _priceFormatter.FormatPrice(finalPrice))
                : _priceFormatter.FormatPrice(finalPrice);

            priceModel.HasDiscount = oldPriceBase > decimal.Zero && oldPriceBase > finalPriceBase;
            if (priceModel.HasDiscount)
            {
                priceModel.RegularPriceValue = oldPrice;
                priceModel.RegularPrice = _priceFormatter.FormatPrice(oldPrice);
            }

            // Calculate saving.
            var finalPriceWithDiscount = _priceCalculationService.GetFinalPrice(contextProduct, null, ctx.Customer, decimal.Zero, true, 1, null, batchContext);
            finalPriceWithDiscount = _taxService.GetProductPrice(contextProduct, finalPriceWithDiscount, out taxRate);
            finalPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithDiscount, ctx.Currency);

            var finalPriceWithoutDiscount = finalPrice;
            if (_catalogSettings.PriceDisplayType != PriceDisplayType.PriceWithoutDiscountsAndAttributes)
            {
                finalPriceWithoutDiscount = _priceCalculationService.GetFinalPrice(contextProduct, null, ctx.Customer, decimal.Zero, false, 1, null, batchContext);
                finalPriceWithoutDiscount = _taxService.GetProductPrice(contextProduct, finalPriceWithoutDiscount, out taxRate);
                finalPriceWithoutDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(finalPriceWithoutDiscount, ctx.Currency);
            }

            // Discounted price has priority over the old price (avoids differing percentage discount in product lists and detail page).
            var regularPrice = finalPriceWithDiscount < finalPriceWithoutDiscount
                ? finalPriceWithoutDiscount
                : oldPrice;

            if (regularPrice > 0 && regularPrice > finalPriceWithDiscount)
            {
                priceModel.HasDiscount = true;
                priceModel.SavingPercent = (float)((regularPrice - finalPriceWithDiscount) / regularPrice) * 100;
                priceModel.SavingAmount = _priceFormatter.FormatPrice(regularPrice - finalPriceWithDiscount, true, false);

                if (!priceModel.RegularPriceValue.HasValue)
                {
                    priceModel.RegularPriceValue = regularPrice;
                    priceModel.RegularPrice = _priceFormatter.FormatPrice(regularPrice);
                }

                if (ctx.Model.ShowDiscountBadge)
                {
                    item.Badges.Add(new ProductSummaryModel.Badge
                    {
                        Label = T("Products.SavingBadgeLabel", priceModel.SavingPercent.ToString("N0")),
                        Style = BadgeStyle.Danger
                    });
                }
            }

            return finalPrice;
        }

        private IEnumerable<ProductSpecificationModel> MapProductSpecificationModels(IEnumerable<ProductSpecificationAttribute> attributes)
        {
            Guard.NotNull(attributes, nameof(attributes));

            if (!attributes.Any())
                return Enumerable.Empty<ProductSpecificationModel>();

            var productId = attributes.First().ProductId;

            string cacheKey = string.Format(ModelCacheEventConsumer.PRODUCT_SPECS_MODEL_KEY, productId, _services.WorkContext.WorkingLanguage.Id);
            return _services.Cache.Get(cacheKey, () =>
            {
                var model = attributes.Select(psa =>
                {
                    return new ProductSpecificationModel
                    {
                        SpecificationAttributeId = psa.SpecificationAttributeOption.SpecificationAttributeId,
                        SpecificationAttributeName = psa.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name),
                        SpecificationAttributeOption = psa.SpecificationAttributeOption.GetLocalized(x => x.Name)
                    };
                }).ToList();

                return model;
            });
        }

        private class MapProductSummaryItemContext
        {
            public MapProductSummaryItemContext()
            {
                MediaFiles = new Dictionary<int, MediaFileInfo>();
            }

            public ProductSummaryModel Model { get; set; }
            public ProductSummaryMappingSettings Settings { get; set; }
            public ProductExportContext BatchContext { get; set; }
            public ProductExportContext AssociatedProductBatchContext { get; set; }
            public Multimap<int, Product> GroupedProducts { get; set; }
            public Dictionary<int, ManufacturerOverviewModel> CachedManufacturerModels { get; set; }
            public Dictionary<int, MediaFileInfo> MediaFiles { get; set; }
            public Dictionary<string, LocalizedString> Resources { get; set; }
            public string LegalInfo { get; set; }
            public Customer Customer { get; set; }
            public Store Store { get; set; }
            public Currency Currency { get; set; }

            public bool AllowPrices { get; set; }
            public bool AllowShoppingCart { get; set; }
            public bool AllowWishlist { get; set; }
            public TaxDisplayType TaxDisplayType { get; set; }
        }
    }

    public class ProductSummaryMappingSettings
    {
        public ProductSummaryMappingSettings()
        {
            MapPrices = true;
            MapPictures = true;
            ViewMode = ProductSummaryViewMode.Grid;
        }

        public ProductSummaryViewMode ViewMode { get; set; }

        public bool MapPrices { get; set; }
        public bool MapPictures { get; set; }
        public bool MapDimensions { get; set; }
        public bool MapSpecificationAttributes { get; set; }
        public bool MapColorAttributes { get; set; }
        public bool MapAttributes { get; set; }
        public bool MapManufacturers { get; set; }
        public bool MapShortDescription { get; set; }
        public bool MapFullDescription { get; set; }
        public bool MapLegalInfo { get; set; }
        public bool MapReviews { get; set; }
        public DeliveryTimesPresentation DeliveryTimesPresentation { get; set; }

        public bool ForceRedirectionAfterAddingToCart { get; set; }
        public int? ThumbnailSize { get; set; }

        public bool? PrefetchTranslations { get; set; }
        public bool? PrefetchUrlSlugs { get; set; }
    }
}