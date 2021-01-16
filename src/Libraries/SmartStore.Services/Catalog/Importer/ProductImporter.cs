using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core.Async;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.IO;
using SmartStore.Core.Localization;
using SmartStore.Data.Utilities;
using SmartStore.Services.DataExchange.Import;
using SmartStore.Services.DataExchange.Import.Events;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Utilities;

namespace SmartStore.Services.Catalog.Importer
{
    public class ProductImporter : EntityImporterBase
    {
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<TierPrice> _tierPriceRepository;
        private readonly IRepository<ProductVariantAttributeValue> _attributeValueRepository;
        private readonly IRepository<ProductVariantAttributeCombination> _attributeCombinationRepository;
        private readonly IMediaService _mediaService;
        private readonly IFolderService _folderService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly FileDownloadManager _fileDownloadManager;

        private static readonly Dictionary<string, Expression<Func<Product, string>>> _localizableProperties = new Dictionary<string, Expression<Func<Product, string>>>
        {
            { "Name", x => x.Name },
            { "ShortDescription", x => x.ShortDescription },
            { "FullDescription", x => x.FullDescription },
            { "MetaKeywords", x => x.MetaKeywords },
            { "MetaDescription", x => x.MetaDescription },
            { "MetaTitle", x => x.MetaTitle },
            { "BundleTitleText", x => x.BundleTitleText }
        };

        public ProductImporter(
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<Product> productRepository,
            IRepository<TierPrice> tierPriceRepository,
            IRepository<ProductVariantAttributeValue> attributeValueRepository,
            IRepository<ProductVariantAttributeCombination> attributeCombinationRepository,
            IMediaService mediaService,
            IFolderService folderService,
            IManufacturerService manufacturerService,
            ICategoryService categoryService,
            IProductService productService,
            IProductTemplateService productTemplateService,
            IProductAttributeService productAttributeService,
            FileDownloadManager fileDownloadManager)
        {
            _productManufacturerRepository = productManufacturerRepository;
            _productCategoryRepository = productCategoryRepository;
            _productTagRepository = productTagRepository;
            _productRepository = productRepository;
            _tierPriceRepository = tierPriceRepository;
            _attributeValueRepository = attributeValueRepository;
            _attributeCombinationRepository = attributeCombinationRepository;
            _mediaService = mediaService;
            _folderService = folderService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _productService = productService;
            _productTemplateService = productTemplateService;
            _productAttributeService = productAttributeService;
            _fileDownloadManager = fileDownloadManager;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;

        protected override void Import(ImportExecuteContext context)
        {
            using (var scope = new DbContextScope(ctx: context.Services.DbContext, hooksEnabled: false, autoDetectChanges: false, proxyCreation: false, validateOnSave: false))
            {
                Initialize(context);

                if (context.File.RelatedType.HasValue)
                {
                    switch (context.File.RelatedType.Value)
                    {
                        case RelatedEntityType.TierPrice:
                            ImportTierPrices(context);
                            break;
                        case RelatedEntityType.ProductVariantAttributeValue:
                            ImportAttributeValues(context);
                            break;
                        case RelatedEntityType.ProductVariantAttributeCombination:
                            ImportAttributeCombinations(context);
                            break;
                    }
                }
                else
                {
                    ImportProducts(context);
                }
            }
        }

        protected virtual void ImportProducts(ImportExecuteContext context)
        {
            var segmenter = context.DataSegmenter;
            var srcToDestId = new Dictionary<int, ImportProductMapping>();
            var templateViewPaths = _productTemplateService.GetAllProductTemplates().ToDictionarySafe(x => x.ViewPath, x => x.Id);

            while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
            {
                var batch = segmenter.GetCurrentBatch<Product>();

                // Perf: detach entities
                _productRepository.Context.DetachEntities(x =>
                {
                    return x is Product || x is UrlRecord || x is StoreMapping || x is ProductVariantAttribute || x is LocalizedProperty ||
                            x is ProductBundleItem || x is ProductCategory || x is ProductManufacturer || x is Category || x is Manufacturer ||
                            x is ProductMediaFile || x is MediaFile || x is ProductTag || x is TierPrice;
                });
                //_productRepository.Context.DetachAll(true);

                context.SetProgress(segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

                // ===========================================================================
                // 1.) Import products
                // ===========================================================================
                int savedProducts = 0;
                try
                {
                    savedProducts = ProcessProducts(context, batch, templateViewPaths, srcToDestId);
                }
                catch (Exception ex)
                {
                    context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessProducts");
                }

                // reduce batch to saved (valid) products.
                // No need to perform import operations on errored products.
                batch = batch.Where(x => x.Entity != null && !x.IsTransient).ToArray();

                // update result object
                context.Result.NewRecords += batch.Count(x => x.IsNew);
                context.Result.ModifiedRecords += Math.Max(0, savedProducts - context.Result.NewRecords);

                // ===========================================================================
                // 2.) Import SEO Slugs
                // IMPORTANT: Unlike with Products AutoCommitEnabled must be TRUE,
                //            as Slugs are going to be validated against existing ones in DB.
                // ===========================================================================
                if (segmenter.HasColumn("SeName", true) || batch.Any(x => x.IsNew || x.NameChanged))
                {
                    try
                    {
                        _productRepository.Context.AutoDetectChangesEnabled = true;
                        ProcessSlugs(context, batch, typeof(Product).Name);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessSlugs");
                    }
                    finally
                    {
                        _productRepository.Context.AutoDetectChangesEnabled = false;
                    }
                }

                // ===========================================================================
                // 3.) Import StoreMappings
                // ===========================================================================
                if (segmenter.HasColumn("StoreIds"))
                {
                    try
                    {
                        ProcessStoreMappings(context, batch);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessStoreMappings");
                    }
                }

                // ===========================================================================
                // 4.) Import Localizations
                // ===========================================================================
                try
                {
                    ProcessLocalizations(context, batch, _localizableProperties);
                }
                catch (Exception ex)
                {
                    context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessLocalizations");
                }

                // ===========================================================================
                // 5.) Import product category mappings
                // ===========================================================================
                if (segmenter.HasColumn("CategoryIds"))
                {
                    try
                    {
                        ProcessProductCategories(context, batch);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessProductCategories");
                    }
                }

                // ===========================================================================
                // 6.) Import product manufacturer mappings
                // ===========================================================================
                if (segmenter.HasColumn("ManufacturerIds"))
                {
                    try
                    {
                        ProcessProductManufacturers(context, batch);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessProductManufacturers");
                    }
                }

                // ===========================================================================
                // 7.) Import product picture mappings
                // ===========================================================================
                if (segmenter.HasColumn("ImageUrls"))
                {
                    try
                    {
                        ProcessProductPictures(context, batch);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessProductPictures");
                    }
                }

                // ===========================================================================
                // 8.) Import product tag names
                // ===========================================================================
                if (segmenter.HasColumn("TagNames"))
                {
                    try
                    {
                        ProcessProductTags(context, batch);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessProductTags");
                    }
                }

                context.Services.EventPublisher.Publish(new ImportBatchExecutedEvent<Product>(context, batch));
            }

            // ===========================================================================
            // 9.) Map parent id of inserted products
            // ===========================================================================
            if (srcToDestId.Any() && segmenter.HasColumn("Id") && segmenter.HasColumn("ParentGroupedProductId") && !segmenter.IsIgnored("ParentGroupedProductId"))
            {
                segmenter.Reset();

                while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
                {
                    var batch = segmenter.GetCurrentBatch<Product>();

                    _productRepository.Context.DetachAll(false);

                    try
                    {
                        ProcessProductMappings(context, batch, srcToDestId);
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddError(ex, segmenter.CurrentSegment, "ProcessParentMappings");
                    }
                }
            }

            // ===========================================================================
            // 10.) PostProcess: normalization
            // ===========================================================================          
            DataMigrator.FixProductMainPictureIds(_productRepository.Context, UtcNow);
        }

        protected virtual void ImportTierPrices(ImportExecuteContext context)
        {
            var segmenter = context.DataSegmenter;
            var entityName = RelatedEntityType.TierPrice.GetLocalizedEnum(context.Services.Localization, context.Services.WorkContext);
            var processingInfo = T("Admin.Common.ProcessingInfo").Text;

            while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
            {
                var savedEntities = 0;
                var batch = segmenter.GetCurrentBatch<TierPrice>();
                var msg = processingInfo.FormatInvariant(entityName, segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

                _tierPriceRepository.Context.DetachEntities(x => x is TierPrice || x is Product);

                context.SetProgress(msg);

                try
                {
                    _tierPriceRepository.AutoCommitEnabled = false;

                    foreach (var row in batch)
                    {
                        var id = row.GetDataValue<int>("Id");
                        var tierPrice = id > 0 ? _tierPriceRepository.GetById(id) : null;

                        if (tierPrice == null)
                        {
                            if (context.UpdateOnly)
                            {
                                ++context.Result.SkippedRecords;
                                continue;
                            }

                            // ProductId is required for new tier prices.
                            var productId = row.GetDataValue<int>("ProductId");
                            if (productId == 0)
                            {
                                ++context.Result.SkippedRecords;
                                context.Result.AddError("The 'ProductId' field is required for new tier prices. Skipping row.", row.GetRowInfo(), "ProductId");
                                continue;
                            }

                            tierPrice = new TierPrice
                            {
                                ProductId = productId
                            };
                        }

                        row.Initialize(tierPrice, null);

                        // Ignore ProductId field. We only update volatile property values and want to avoid accidents.
                        row.SetProperty(context.Result, (x) => x.StoreId);
                        row.SetProperty(context.Result, (x) => x.CustomerRoleId);
                        row.SetProperty(context.Result, (x) => x.Quantity);
                        row.SetProperty(context.Result, (x) => x.Price);

                        if (row.TryGetDataValue("CalculationMethod", out int calcMethod))
                        {
                            tierPrice.CalculationMethod = (TierPriceCalculationMethod)calcMethod;
                        }

                        if (row.IsTransient)
                        {
                            _tierPriceRepository.Insert(tierPrice);
                        }
                        else
                        {
                            //_tierPriceRepository.Update(tierPrice);   // unnecessary: we use DetectChanges()
                        }
                    }

                    savedEntities = _tierPriceRepository.Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    context.Result.AddError(ex, segmenter.CurrentSegment, "ImportTierPrices");
                }

                batch = batch.Where(x => x.Entity != null && !x.IsTransient).ToArray();

                context.Result.NewRecords += batch.Count(x => x.IsNew);
                context.Result.ModifiedRecords += Math.Max(0, savedEntities - context.Result.NewRecords);

                // Update has tier prices property for inserted records.
                var insertedProductIds = new HashSet<int>(batch.Where(x => x.IsNew).Select(x => x.Entity.ProductId));
                var products = _productService.GetProductsByIds(insertedProductIds.ToArray());
                if (products.Any())
                {
                    products.Each(x => _productService.UpdateHasTierPricesProperty(x));
                    _productRepository.Context.SaveChanges();
                }
            }
        }

        protected virtual void ImportAttributeValues(ImportExecuteContext context)
        {
            var segmenter = context.DataSegmenter;
            var entityName = RelatedEntityType.ProductVariantAttributeValue.GetLocalizedEnum(context.Services.Localization, context.Services.WorkContext);
            var processingInfo = T("Admin.Common.ProcessingInfo").Text;

            while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
            {
                var savedEntities = 0;
                var batch = segmenter.GetCurrentBatch<ProductVariantAttributeValue>();
                var msg = processingInfo.FormatInvariant(entityName, segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

                _attributeValueRepository.Context.DetachEntities(x => x is ProductVariantAttributeValue);

                context.SetProgress(msg);

                try
                {
                    _attributeValueRepository.AutoCommitEnabled = false;

                    foreach (var row in batch)
                    {
                        var id = row.GetDataValue<int>("Id");
                        var attributeValue = id > 0 ? _attributeValueRepository.GetById(id) : null;

                        if (attributeValue == null)
                        {
                            if (context.UpdateOnly)
                            {
                                ++context.Result.SkippedRecords;
                                continue;
                            }

                            // ProductVariantAttributeId is required for new attribute values.
                            var pvaId = row.GetDataValue<int>("ProductVariantAttributeId");
                            if (pvaId == 0)
                            {
                                ++context.Result.SkippedRecords;
                                context.Result.AddError("The 'ProductVariantAttributeId' field is required for new attribute values. Skipping row.", row.GetRowInfo(), "ProductVariantAttributeId");
                                continue;
                            }

                            if (!row.HasDataValue("Name"))
                            {
                                ++context.Result.SkippedRecords;
                                context.Result.AddError("The 'Name' field is required for new attribute values. Skipping row.", row.GetRowInfo(), "Name");
                                continue;
                            }

                            attributeValue = new ProductVariantAttributeValue
                            {
                                ProductVariantAttributeId = pvaId
                            };
                        }

                        row.Initialize(attributeValue, null);

                        // Ignore ProductVariantAttributeId field. We only update volatile property values and want to avoid accidents.
                        row.SetProperty(context.Result, (x) => x.Alias);
                        row.SetProperty(context.Result, (x) => x.Name);
                        row.SetProperty(context.Result, (x) => x.Color);
                        row.SetProperty(context.Result, (x) => x.PriceAdjustment);
                        row.SetProperty(context.Result, (x) => x.WeightAdjustment);
                        row.SetProperty(context.Result, (x) => x.Quantity, 10000);
                        row.SetProperty(context.Result, (x) => x.IsPreSelected);
                        row.SetProperty(context.Result, (x) => x.DisplayOrder);
                        row.SetProperty(context.Result, (x) => x.ValueTypeId);
                        row.SetProperty(context.Result, (x) => x.LinkedProductId);

                        if (row.IsTransient)
                        {
                            _attributeValueRepository.Insert(attributeValue);
                        }
                        else
                        {
                            //_attributeValueRepository.Update(attributeValue);   // unnecessary: we use DetectChanges()
                        }
                    }

                    savedEntities = _attributeValueRepository.Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    context.Result.AddError(ex, segmenter.CurrentSegment, "ImportAttributeValues");
                }

                batch = batch.Where(x => x.Entity != null && !x.IsTransient).ToArray();

                context.Result.NewRecords += batch.Count(x => x.IsNew);
                context.Result.ModifiedRecords += Math.Max(0, savedEntities - context.Result.NewRecords);

                // MediaHelper.UpdatePictureTransientStateFor() not required, I guess, because there is no picture upload.
            }
        }

        protected virtual void ImportAttributeCombinations(ImportExecuteContext context)
        {
            var segmenter = context.DataSegmenter;
            var entityName = RelatedEntityType.ProductVariantAttributeCombination.GetLocalizedEnum(context.Services.Localization, context.Services.WorkContext);
            var processingInfo = T("Admin.Common.ProcessingInfo").Text;
            var lowestCombinationPriceProductIds = new HashSet<int>();

            while (context.Abort == DataExchangeAbortion.None && segmenter.ReadNextBatch())
            {
                var savedEntities = 0;
                var batch = segmenter.GetCurrentBatch<ProductVariantAttributeCombination>();
                var msg = processingInfo.FormatInvariant(entityName, segmenter.CurrentSegmentFirstRowIndex - 1, segmenter.TotalRows);

                _attributeCombinationRepository.Context.DetachEntities(x => x is ProductVariantAttributeCombination || x is Product);

                context.SetProgress(msg);

                try
                {
                    _attributeCombinationRepository.AutoCommitEnabled = false;

                    foreach (var row in batch)
                    {
                        var id = row.GetDataValue<int>("Id");
                        var combination = id > 0 ? _attributeCombinationRepository.GetById(id) : null;

                        // No Id? Try key fields.
                        if (combination == null)
                        {
                            foreach (var keyName in context.KeyFieldNames)
                            {
                                var keyValue = row.GetDataValue<string>(keyName);
                                if (keyValue.HasValue())
                                {
                                    switch (keyName)
                                    {
                                        case "Sku":
                                            combination = _productAttributeService.GetProductVariantAttributeCombinationBySku(keyValue);
                                            break;
                                        case "Gtin":
                                            combination = _productAttributeService.GetAttributeCombinationByGtin(keyValue);
                                            break;
                                        case "ManufacturerPartNumber":
                                            combination = _productAttributeService.GetAttributeCombinationByMpn(keyValue);
                                            break;
                                    }
                                }

                                if (combination != null)
                                {
                                    break;
                                }
                            }
                        }

                        if (combination == null)
                        {
                            // We do not insert records here to avoid inconsistent attribute combination data.
                            ++context.Result.SkippedRecords;
                            context.Result.AddError("The 'Id' or another key field is required. Inserting attribute combinations not supported. Skipping row.", row.GetRowInfo(), "Id");
                            continue;
                        }

                        row.Initialize(combination, null);

                        if (row.TryGetDataValue("Price", out decimal? price) && price != combination.Price)
                        {
                            lowestCombinationPriceProductIds.Add(combination.ProductId);
                        }

                        // Ignore ProductId field. We only update volatile property values and want to avoid accidents.
                        row.SetProperty(context.Result, (x) => x.Sku);
                        row.SetProperty(context.Result, (x) => x.Gtin);
                        row.SetProperty(context.Result, (x) => x.ManufacturerPartNumber);
                        row.SetProperty(context.Result, (x) => x.StockQuantity, 10000);
                        row.SetProperty(context.Result, (x) => x.Price);
                        row.SetProperty(context.Result, (x) => x.Length);
                        row.SetProperty(context.Result, (x) => x.Width);
                        row.SetProperty(context.Result, (x) => x.Height);
                        row.SetProperty(context.Result, (x) => x.BasePriceAmount);
                        row.SetProperty(context.Result, (x) => x.BasePriceBaseAmount);
                        row.SetProperty(context.Result, (x) => x.AssignedMediaFileIds);
                        row.SetProperty(context.Result, (x) => x.IsActive, true);
                        row.SetProperty(context.Result, (x) => x.AllowOutOfStockOrders);
                        row.SetProperty(context.Result, (x) => x.DeliveryTimeId);
                        row.SetProperty(context.Result, (x) => x.QuantityUnitId);
                        row.SetProperty(context.Result, (x) => x.AttributesXml);
                    }

                    savedEntities = _attributeCombinationRepository.Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    context.Result.AddError(ex, segmenter.CurrentSegment, "ImportAttributeCombinations");
                }

                batch = batch.Where(x => x.Entity != null && !x.IsTransient).ToArray();

                context.Result.NewRecords += batch.Count(x => x.IsNew);
                context.Result.ModifiedRecords += Math.Max(0, savedEntities - context.Result.NewRecords);

                // Update lowest attribute combination price property.
                var products = _productService.GetProductsByIds(lowestCombinationPriceProductIds.ToArray());
                if (products.Any())
                {
                    products.Each(x => _productService.UpdateLowestAttributeCombinationPriceProperty(x));
                    _productRepository.Context.SaveChanges();
                }
            }
        }

        protected virtual int ProcessProducts(
            ImportExecuteContext context,
            IEnumerable<ImportRow<Product>> batch,
            Dictionary<string, int> templateViewPaths,
            Dictionary<int, ImportProductMapping> srcToDestId)
        {
            _productRepository.AutoCommitEnabled = false;

            var defaultTemplateId = templateViewPaths["Product"];
            var hasNameColumn = context.DataSegmenter.HasColumn("Name");

            foreach (var row in batch)
            {
                Product product = null;
                var id = row.GetDataValue<int>("Id");

                foreach (var keyName in context.KeyFieldNames)
                {
                    var keyValue = row.GetDataValue<string>(keyName);

                    if (keyValue.HasValue() || id > 0)
                    {
                        switch (keyName)
                        {
                            case "Id":
                                product = _productRepository.GetById(id); // get it uncached
                                break;
                            case "Sku":
                                product = _productService.GetProductBySku(keyValue);
                                break;
                            case "Gtin":
                                product = _productService.GetProductByGtin(keyValue);
                                break;
                            case "ManufacturerPartNumber":
                                product = _productService.GetProductByManufacturerPartNumber(keyValue);
                                break;
                            case "Name":
                                product = _productService.GetProductByName(keyValue);
                                break;
                        }
                    }

                    if (product != null)
                        break;
                }

                if (product == null)
                {
                    if (context.UpdateOnly)
                    {
                        ++context.Result.SkippedRecords;
                        continue;
                    }

                    // A name is required for new products.
                    if (!row.HasDataValue("Name"))
                    {
                        ++context.Result.SkippedRecords;
                        context.Result.AddError("The 'Name' field is required for new products. Skipping row.", row.GetRowInfo(), "Name");
                        continue;
                    }

                    product = new Product();
                }

                var name = row.GetDataValue<string>("Name");

                row.Initialize(product, name ?? product.Name);

                if (!row.IsNew && hasNameColumn)
                {
                    if (!product.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        // Perf: use this later for SeName updates.
                        row.NameChanged = true;
                    }
                }

                row.SetProperty(context.Result, (x) => x.ProductTypeId, (int)ProductType.SimpleProduct);
                row.SetProperty(context.Result, (x) => x.Name);
                row.SetProperty(context.Result, (x) => x.ShortDescription);
                row.SetProperty(context.Result, (x) => x.FullDescription);
                row.SetProperty(context.Result, (x) => x.AdminComment);
                row.SetProperty(context.Result, (x) => x.ShowOnHomePage);
                row.SetProperty(context.Result, (x) => x.HomePageDisplayOrder);
                row.SetProperty(context.Result, (x) => x.MetaKeywords);
                row.SetProperty(context.Result, (x) => x.MetaDescription);
                row.SetProperty(context.Result, (x) => x.MetaTitle);
                row.SetProperty(context.Result, (x) => x.AllowCustomerReviews, true);
                row.SetProperty(context.Result, (x) => x.ApprovedRatingSum);
                row.SetProperty(context.Result, (x) => x.NotApprovedRatingSum);
                row.SetProperty(context.Result, (x) => x.ApprovedTotalReviews);
                row.SetProperty(context.Result, (x) => x.NotApprovedTotalReviews);
                row.SetProperty(context.Result, (x) => x.Published, true);
                row.SetProperty(context.Result, (x) => x.Sku);
                row.SetProperty(context.Result, (x) => x.ManufacturerPartNumber);
                row.SetProperty(context.Result, (x) => x.Gtin);
                row.SetProperty(context.Result, (x) => x.IsGiftCard);
                row.SetProperty(context.Result, (x) => x.GiftCardTypeId);
                row.SetProperty(context.Result, (x) => x.RequireOtherProducts);
                row.SetProperty(context.Result, (x) => x.RequiredProductIds);   // TODO: global scope
                row.SetProperty(context.Result, (x) => x.AutomaticallyAddRequiredProducts);
                row.SetProperty(context.Result, (x) => x.IsDownload);
                //row.SetProperty(context.Result, (x) => x.DownloadId);
                //row.SetProperty(context.Result, (x) => x.UnlimitedDownloads, true);
                row.SetProperty(context.Result, (x) => x.MaxNumberOfDownloads, 10);
                row.SetProperty(context.Result, (x) => x.DownloadExpirationDays);
                row.SetProperty(context.Result, (x) => x.DownloadActivationTypeId, 1);
                row.SetProperty(context.Result, (x) => x.HasSampleDownload);
                row.SetProperty(context.Result, (x) => x.SampleDownloadId, (int?)null, ZeroToNull);    // TODO: global scope
                row.SetProperty(context.Result, (x) => x.HasUserAgreement);
                row.SetProperty(context.Result, (x) => x.UserAgreementText);
                row.SetProperty(context.Result, (x) => x.IsRecurring);
                row.SetProperty(context.Result, (x) => x.RecurringCycleLength, 100);
                row.SetProperty(context.Result, (x) => x.RecurringCyclePeriodId);
                row.SetProperty(context.Result, (x) => x.RecurringTotalCycles, 10);
                row.SetProperty(context.Result, (x) => x.IsShipEnabled, true);
                row.SetProperty(context.Result, (x) => x.IsFreeShipping);
                row.SetProperty(context.Result, (x) => x.AdditionalShippingCharge);
                row.SetProperty(context.Result, (x) => x.IsEsd);
                row.SetProperty(context.Result, (x) => x.IsTaxExempt);
                row.SetProperty(context.Result, (x) => x.TaxCategoryId, 1);    // TODO: global scope
                row.SetProperty(context.Result, (x) => x.ManageInventoryMethodId);
                row.SetProperty(context.Result, (x) => x.StockQuantity, 10000);
                row.SetProperty(context.Result, (x) => x.DisplayStockAvailability);
                row.SetProperty(context.Result, (x) => x.DisplayStockQuantity);
                row.SetProperty(context.Result, (x) => x.MinStockQuantity);
                row.SetProperty(context.Result, (x) => x.LowStockActivityId);
                row.SetProperty(context.Result, (x) => x.NotifyAdminForQuantityBelow, 1);
                row.SetProperty(context.Result, (x) => x.BackorderModeId);
                row.SetProperty(context.Result, (x) => x.AllowBackInStockSubscriptions);
                row.SetProperty(context.Result, (x) => x.OrderMinimumQuantity, 1);
                row.SetProperty(context.Result, (x) => x.OrderMaximumQuantity, 100);
                row.SetProperty(context.Result, (x) => x.QuantityStep, 1);
                row.SetProperty(context.Result, (x) => x.HideQuantityControl);
                row.SetProperty(context.Result, (x) => x.AllowedQuantities);
                row.SetProperty(context.Result, (x) => x.DisableBuyButton);
                row.SetProperty(context.Result, (x) => x.DisableWishlistButton);
                row.SetProperty(context.Result, (x) => x.AvailableForPreOrder);
                row.SetProperty(context.Result, (x) => x.CallForPrice);
                row.SetProperty(context.Result, (x) => x.Price);
                row.SetProperty(context.Result, (x) => x.OldPrice);
                row.SetProperty(context.Result, (x) => x.ProductCost);
                row.SetProperty(context.Result, (x) => x.SpecialPrice);
                row.SetProperty(context.Result, (x) => x.SpecialPriceStartDateTimeUtc);
                row.SetProperty(context.Result, (x) => x.SpecialPriceEndDateTimeUtc);
                row.SetProperty(context.Result, (x) => x.CustomerEntersPrice);
                row.SetProperty(context.Result, (x) => x.MinimumCustomerEnteredPrice);
                row.SetProperty(context.Result, (x) => x.MaximumCustomerEnteredPrice, 1000);
                // HasTierPrices... ignore as long as no tier prices are imported
                // LowestAttributeCombinationPrice... ignore as long as no combinations are imported
                row.SetProperty(context.Result, (x) => x.Weight);
                row.SetProperty(context.Result, (x) => x.Length);
                row.SetProperty(context.Result, (x) => x.Width);
                row.SetProperty(context.Result, (x) => x.Height);
                row.SetProperty(context.Result, (x) => x.DisplayOrder);
                row.SetProperty(context.Result, (x) => x.DeliveryTimeId);      // TODO: global scope
                row.SetProperty(context.Result, (x) => x.QuantityUnitId);      // TODO: global scope
                row.SetProperty(context.Result, (x) => x.BasePriceEnabled);
                row.SetProperty(context.Result, (x) => x.BasePriceMeasureUnit);
                row.SetProperty(context.Result, (x) => x.BasePriceAmount);
                row.SetProperty(context.Result, (x) => x.BasePriceBaseAmount);
                row.SetProperty(context.Result, (x) => x.BundleTitleText);
                row.SetProperty(context.Result, (x) => x.BundlePerItemShipping);
                row.SetProperty(context.Result, (x) => x.BundlePerItemPricing);
                row.SetProperty(context.Result, (x) => x.BundlePerItemShoppingCart);
                row.SetProperty(context.Result, (x) => x.AvailableStartDateTimeUtc);
                row.SetProperty(context.Result, (x) => x.AvailableEndDateTimeUtc);
                // With new entities, "LimitedToStores" is an implicit field, meaning
                // it has to be set to true by code if it's absent but "StoreIds" exists.
                row.SetProperty(context.Result, (x) => x.LimitedToStores, !row.GetDataValue<List<int>>("StoreIds").IsNullOrEmpty());
                row.SetProperty(context.Result, (x) => x.CustomsTariffNumber);
                row.SetProperty(context.Result, (x) => x.CountryOfOriginId);

                if (row.TryGetDataValue("QuantiyControlType", out int qct))
                {
                    product.QuantiyControlType = (QuantityControlType)qct;
                }
                if (row.TryGetDataValue("AttributeChoiceBehaviour", out int attributeChoiceBehaviour))
                {
                    product.AttributeChoiceBehaviour = (AttributeChoiceBehaviour)attributeChoiceBehaviour;
                }
                if (row.TryGetDataValue("Visibility", out int visibilityValue))
                {
                    product.Visibility = (ProductVisibility)visibilityValue;
                }
                if (row.TryGetDataValue("Condition", out int conditionValue))
                {
                    product.Condition = (ProductCondition)conditionValue;
                }

                if (row.TryGetDataValue("ProductTemplateViewPath", out string tvp, row.IsTransient))
                {
                    product.ProductTemplateId = tvp.HasValue() && templateViewPaths.ContainsKey(tvp) ? templateViewPaths[tvp] : defaultTemplateId;
                }

                if (id != 0 && !srcToDestId.ContainsKey(id))
                {
                    srcToDestId.Add(id, new ImportProductMapping { Inserted = row.IsTransient });
                }

                if (row.IsTransient)
                {
                    _productRepository.Insert(product);
                }
                else
                {
                    product.UpdatedOnUtc = DateTime.UtcNow;
                    //_productRepository.Update(product); // unnecessary: we use DetectChanges()
                }
            }

            // Commit whole batch at once.
            var num = _productRepository.Context.SaveChanges();

            // Get new product ids.
            foreach (var row in batch)
            {
                var id = row.GetDataValue<int>("Id");
                if (id != 0 && srcToDestId.ContainsKey(id))
                {
                    srcToDestId[id].DestinationId = row.Entity.Id;
                }
            }

            return num;
        }

        protected virtual int ProcessProductMappings(
            ImportExecuteContext context,
            IEnumerable<ImportRow<Product>> batch,
            Dictionary<int, ImportProductMapping> srcToDestId)
        {
            _productRepository.AutoCommitEnabled = false;

            foreach (var row in batch)
            {
                var id = row.GetDataValue<int>("Id");
                var parentGroupedProductId = row.GetDataValue<int>("ParentGroupedProductId");

                if (id != 0 && parentGroupedProductId != 0 && srcToDestId.ContainsKey(id) && srcToDestId.ContainsKey(parentGroupedProductId))
                {
                    if (srcToDestId[id].DestinationId != 0)
                    {
                        var product = _productRepository.GetById(srcToDestId[id].DestinationId);
                        if (product != null)
                        {
                            product.ParentGroupedProductId = srcToDestId[parentGroupedProductId].DestinationId;
                            //_productRepository.Update(product);  // unnecessary: we use DetectChanges()
                        }
                    }
                }
            }

            var num = _productRepository.Context.SaveChanges();
            return num;
        }

        protected virtual void ProcessProductPictures(ImportExecuteContext context, IEnumerable<ImportRow<Product>> batch)
        {
            _productRepository.AutoCommitEnabled = false;

            var numberOfPictures = context.ExtraData.NumberOfPictures ?? int.MaxValue;
            var productIds = batch.Select(x => x.Entity.Id).ToArray();
            var tmpFileMap = _productService.GetProductPicturesByProductIds(productIds, null, MediaLoadFlags.None);

            foreach (var row in batch)
            {
                var rawImageUrls = row.GetDataValue<string>("ImageUrls");

                // Force pipe symbol as separator because file names can contain commas or semicolons.
                var imageUrls = rawImageUrls.SplitSafe("|");
                if (imageUrls.Length == 0)
                {
                    continue;
                }
                
                var productId = row.Entity.Id;
                var imageNumber = 0;
                var displayOrder = -1;
                var imageFiles = new List<FileDownloadManagerItem>();
                var catalogAlbumId = _folderService.GetNodeByPath(SystemAlbumProvider.Catalog).Value.Id;

                // Collect required image file infos.
                foreach (var urlOrPath in imageUrls)
                {
                    var image = CreateDownloadImage(context, urlOrPath, ++imageNumber);
                    if (image != null)
                    {
                        imageFiles.Add(image);

                        if (imageFiles.Count >= numberOfPictures)
                            break;
                    }
                }

                // Download images.
                if (imageFiles.Any(x => x.Url.HasValue()))
                {
                    AsyncRunner.RunSync(() => _fileDownloadManager.DownloadAsync(DownloaderContext, imageFiles.Where(x => x.Url.HasValue() && !x.Success.HasValue)));

                    var hasDuplicateFileNames = imageFiles
                        .Where(x => x.Url.HasValue())
                        .Select(x => x.FileName)
                        .GroupBy(x => x)
                        .Any(x => x.Count() > 1);

                    if (hasDuplicateFileNames)
                    {
                        context.Result.AddWarning($"Found duplicate names (not supported yet). File names in URLs have to be unique!", row.GetRowInfo(), "ImageUrls");
                    }
                }

                // Import images.
                foreach (var image in imageFiles.OrderBy(x => x.DisplayOrder))
                {
                    try
                    {
                        if ((image.Success ?? false) && File.Exists(image.Path))
                        {
                            Succeeded(image);
                            using (var stream = File.OpenRead(image.Path))
                            {
                                if ((stream?.Length ?? 0) > 0)
                                {
                                    MediaFile sourceFile = null;
                                    var currentFiles = tmpFileMap.ContainsKey(productId)
                                        ? tmpFileMap[productId]
                                        : Enumerable.Empty<ProductMediaFile>();

                                    if (displayOrder == -1)
                                    {
                                        displayOrder = currentFiles.Any() ? currentFiles.Select(x => x.DisplayOrder).Max() : 0;
                                    }

                                    if (_mediaService.FindEqualFile(stream, currentFiles.Select(x => x.MediaFile), true, out var _))
                                    {
                                        context.Result.AddInfo($"Found equal file in product data for '{image.FileName}'. Skipping file.", row.GetRowInfo(), "ImageUrls");
                                    }
                                    else if (_mediaService.FindEqualFile(stream, image.FileName, catalogAlbumId, true, out sourceFile))
                                    {
                                        context.Result.AddInfo($"Found equal file in catalog album for '{image.FileName}'. Assigning existing file instead.", row.GetRowInfo(), "ImageUrls");
                                    }
                                    else
                                    {
                                        var path = _mediaService.CombinePaths(SystemAlbumProvider.Catalog, image.FileName);
                                        sourceFile = _mediaService.SaveFile(path, stream, false, DuplicateFileHandling.Rename)?.File;
                                    }

                                    if (sourceFile?.Id > 0)
                                    {
                                        var productMediaFile = new ProductMediaFile
                                        {
                                            ProductId = productId,
                                            MediaFileId = sourceFile.Id,
                                            DisplayOrder = ++displayOrder
                                        };

                                        _productService.InsertProductPicture(productMediaFile);

                                        productMediaFile.MediaFile = sourceFile;

                                        tmpFileMap.Add(productId, productMediaFile);
                                        // Update for FixProductMainPictureIds.
                                        row.Entity.UpdatedOnUtc = DateTime.UtcNow;

                                        //_productRepository.Update(row.Entity);
                                    }
                                }
                            }
                        }
                        else if (image.Url.HasValue())
                        {
                            context.Result.AddInfo($"Download failed for image {image.Url}.", row.GetRowInfo(), "ImageUrls" + image.DisplayOrder.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddWarning(ex.ToAllMessages(), row.GetRowInfo(), "ImageUrls" + image.DisplayOrder.ToString());
                    }
                }
            }

            _productRepository.Context.SaveChanges();
        }

        protected virtual int ProcessProductManufacturers(ImportExecuteContext context, IEnumerable<ImportRow<Product>> batch)
        {
            _productManufacturerRepository.AutoCommitEnabled = false;

            foreach (var row in batch)
            {
                var manufacturerIds = row.GetDataValue<List<int>>("ManufacturerIds");
                if (!manufacturerIds.IsNullOrEmpty())
                {
                    try
                    {
                        foreach (var id in manufacturerIds)
                        {
                            if (_productManufacturerRepository.TableUntracked.Where(x => x.ProductId == row.Entity.Id && x.ManufacturerId == id).FirstOrDefault() == null)
                            {
                                // Ensure that manufacturer exists.
                                var manufacturer = _manufacturerService.GetManufacturerById(id);
                                if (manufacturer != null)
                                {
                                    var productManufacturer = new ProductManufacturer
                                    {
                                        ProductId = row.Entity.Id,
                                        ManufacturerId = manufacturer.Id,
                                        IsFeaturedProduct = false,
                                        DisplayOrder = 1
                                    };
                                    _productManufacturerRepository.Insert(productManufacturer);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddWarning(ex.Message, row.GetRowInfo(), "ManufacturerIds");
                    }
                }
            }

            // Commit whole batch at once.
            var num = _productManufacturerRepository.Context.SaveChanges();
            return num;
        }

        protected virtual int ProcessProductCategories(ImportExecuteContext context, IEnumerable<ImportRow<Product>> batch)
        {
            _productCategoryRepository.AutoCommitEnabled = false;

            foreach (var row in batch)
            {
                var categoryIds = row.GetDataValue<List<int>>("CategoryIds");
                if (!categoryIds.IsNullOrEmpty())
                {
                    try
                    {
                        foreach (var id in categoryIds)
                        {
                            if (!_productCategoryRepository.TableUntracked.Any(x => x.ProductId == row.Entity.Id && x.CategoryId == id))
                            {
                                // Ensure that category exists.
                                var category = _categoryService.GetCategoryById(id);
                                if (category != null)
                                {
                                    var productCategory = new ProductCategory
                                    {
                                        ProductId = row.Entity.Id,
                                        CategoryId = category.Id,
                                        IsFeaturedProduct = false,
                                        DisplayOrder = 1
                                    };
                                    _productCategoryRepository.Insert(productCategory);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Result.AddWarning(ex.Message, row.GetRowInfo(), "CategoryIds");
                    }
                }
            }

            // Commit whole batch at once,
            var num = _productCategoryRepository.Context.SaveChanges();
            return num;
        }

        protected virtual void ProcessProductTags(ImportExecuteContext context, IEnumerable<ImportRow<Product>> batch)
        {
            // True, cause product tags must be saved and assigned an id prior adding a mapping.
            _productTagRepository.AutoCommitEnabled = true;

            var productIds = batch.Select(x => x.Entity.Id).ToList();
            var tagsPerBatch = _productRepository.TableUntracked
                .Expand(x => x.ProductTags)
                .Where(x => productIds.Contains(x.Id))
                .ToDictionary(x => x.Id, x => x.ProductTags);

            foreach (var row in batch)
            {
                try
                {
                    var product = row.Entity;
                    var tags = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

                    foreach (var str in row.GetDataValue<string>("TagNames").SplitSafe("|"))
                    {
                        var arr = str.SplitSafe("~");
                        if (arr.Length > 0)
                        {
                            tags[arr[0]] = arr.Length > 1 ? arr[1].ToBool(true) : true;
                        }
                    }

                    if (!tagsPerBatch.TryGetValue(product.Id, out var existingTags))
                    {
                        existingTags = new List<ProductTag>();
                    }

                    if (!tags.Any())
                    {
                        // Remove all tags.
                        if (existingTags.Any())
                        {
                            _productTagRepository.Context.LoadCollection(product, (Product x) => x.ProductTags);
                            product.ProductTags.Clear();
                        }
                    }
                    else
                    {
                        // Remove tags.
                        var tagsToRemove = new List<ProductTag>();
                        foreach (var existingTag in existingTags)
                        {
                            if (!tags.Keys.Any(x => x.IsCaseInsensitiveEqual(existingTag.Name)))
                            {
                                tagsToRemove.Add(existingTag);
                            }
                        }
                        if (tagsToRemove.Any())
                        {
                            _productTagRepository.Context.LoadCollection(product, (Product x) => x.ProductTags);
                            tagsToRemove.Each(x => product.ProductTags.Remove(x));
                        }

                        // Add tags.
                        foreach (var tag in tags)
                        {
                            if (!existingTags.Any(x => x.Name.IsCaseInsensitiveEqual(tag.Key)))
                            {
                                var productTag = _productTagRepository.Table.FirstOrDefault(x => x.Name == tag.Key);
                                if (productTag == null)
                                {
                                    productTag = new ProductTag
                                    {
                                        Name = tag.Key,
                                        Published = tag.Value
                                    };
                                    _productTagRepository.Insert(productTag);
                                }

                                product.ProductTags.Add(productTag);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    context.Result.AddWarning(ex.Message, row.GetRowInfo(), "TagNames");
                }
            }

            // Commit whole batch at once.
            _productTagRepository.Context.SaveChanges();
        }

        private int? ZeroToNull(object value, CultureInfo culture)
        {
            if (CommonHelper.TryConvert<int>(value, culture, out int result) && result > 0)
            {
                return result;
            }

            return (int?)null;
        }

        public static string[] SupportedKeyFields => new string[] { "Id", "Sku", "Gtin", "ManufacturerPartNumber", "Name" };

        public static string[] DefaultKeyFields => new string[] { "Sku", "Gtin", "ManufacturerPartNumber" };

        public class ImportProductMapping
        {
            public int DestinationId { get; set; }
            public bool Inserted { get; set; }
        }
    }
}
