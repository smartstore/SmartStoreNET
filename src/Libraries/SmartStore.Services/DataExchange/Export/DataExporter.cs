using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Email;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.DataExchange.Export.Deployment;
using SmartStore.Services.DataExchange.Export.Internal;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Search;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;

namespace SmartStore.Services.DataExchange.Export
{
    public partial class DataExporter : IDataExporter
    {
        #region Dependencies

        private readonly ICommonServices _services;
        private readonly IDbContext _dbContext;
        private readonly HttpContextBase _httpContext;
        private readonly Lazy<IPriceFormatter> _priceFormatter;
        private readonly Lazy<IExportProfileService> _exportProfileService;
        private readonly Lazy<ILocalizedEntityService> _localizedEntityService;
        private readonly Lazy<ILanguageService> _languageService;
        private readonly Lazy<IUrlRecordService> _urlRecordService;
        private readonly Lazy<IMediaService> _mediaService;
        private readonly Lazy<IPriceCalculationService> _priceCalculationService;
        private readonly Lazy<ICurrencyService> _currencyService;
        private readonly Lazy<ITaxService> _taxService;
        private readonly Lazy<ICategoryService> _categoryService;
        private readonly Lazy<IProductAttributeParser> _productAttributeParser;
        private readonly Lazy<IProductAttributeService> _productAttributeService;
        private readonly Lazy<ISpecificationAttributeService> _specificationAttributeService;
        private readonly Lazy<IProductTemplateService> _productTemplateService;
        private readonly Lazy<ICategoryTemplateService> _categoryTemplateService;
        private readonly Lazy<IProductService> _productService;
        private readonly Lazy<IOrderService> _orderService;
        private readonly Lazy<IManufacturerService> _manufacturerService;
        private readonly ICustomerService _customerService;
        private readonly Lazy<IAddressService> _addressService;
        private readonly Lazy<ICountryService> _countryService;
        private readonly Lazy<IShipmentService> _shipmentService;
        private readonly Lazy<IGenericAttributeService> _genericAttributeService;
        private readonly Lazy<IEmailAccountService> _emailAccountService;
        private readonly Lazy<IQueuedEmailService> _queuedEmailService;
        private readonly Lazy<IEmailSender> _emailSender;
        private readonly Lazy<IDeliveryTimeService> _deliveryTimeService;
        private readonly Lazy<IQuantityUnitService> _quantityUnitService;
        private readonly Lazy<ICatalogSearchService> _catalogSearchService;
        private readonly Lazy<IDownloadService> _downloadService;
        private readonly Lazy<ProductUrlHelper> _productUrlHelper;

        private readonly Lazy<IRepository<Customer>> _customerRepository;
        private readonly Lazy<IRepository<NewsLetterSubscription>> _subscriptionRepository;
        private readonly Lazy<IRepository<Order>> _orderRepository;
        private readonly Lazy<IRepository<ShoppingCartItem>> _shoppingCartItemRepository;

        private readonly Lazy<MediaSettings> _mediaSettings;
        private readonly Lazy<ContactDataSettings> _contactDataSettings;
        private readonly Lazy<CustomerSettings> _customerSettings;
        private readonly Lazy<CatalogSettings> _catalogSettings;
        private readonly Lazy<LocalizationSettings> _localizationSettings;
        private readonly Lazy<TaxSettings> _taxSettings;
        private readonly Lazy<SeoSettings> _seoSettings;

        public DataExporter(
            ICommonServices services,
            IDbContext dbContext,
            HttpContextBase httpContext,
            Lazy<IPriceFormatter> priceFormatter,
            Lazy<IExportProfileService> exportProfileService,
            Lazy<ILocalizedEntityService> localizedEntityService,
            Lazy<ILanguageService> languageService,
            Lazy<IUrlRecordService> urlRecordService,
            Lazy<IMediaService> mediaService,
            Lazy<IPriceCalculationService> priceCalculationService,
            Lazy<ICurrencyService> currencyService,
            Lazy<ITaxService> taxService,
            Lazy<ICategoryService> categoryService,
            Lazy<IProductAttributeParser> productAttributeParser,
            Lazy<IProductAttributeService> productAttributeService,
            Lazy<ISpecificationAttributeService> specificationAttributeService,
            Lazy<IProductTemplateService> productTemplateService,
            Lazy<ICategoryTemplateService> categoryTemplateService,
            Lazy<IProductService> productService,
            Lazy<IOrderService> orderService,
            Lazy<IManufacturerService> manufacturerService,
            ICustomerService customerService,
            Lazy<IAddressService> addressService,
            Lazy<ICountryService> countryService,
            Lazy<IShipmentService> shipmentService,
            Lazy<IGenericAttributeService> genericAttributeService,
            Lazy<IEmailAccountService> emailAccountService,
            Lazy<IQueuedEmailService> queuedEmailService,
            Lazy<IEmailSender> emailSender,
            Lazy<IDeliveryTimeService> deliveryTimeService,
            Lazy<IQuantityUnitService> quantityUnitService,
            Lazy<ICatalogSearchService> catalogSearchService,
            Lazy<IDownloadService> downloadService,
            Lazy<ProductUrlHelper> productUrlHelper,
            Lazy<IRepository<Customer>> customerRepository,
            Lazy<IRepository<NewsLetterSubscription>> subscriptionRepository,
            Lazy<IRepository<Order>> orderRepository,
            Lazy<IRepository<ShoppingCartItem>> shoppingCartItemRepository,
            Lazy<MediaSettings> mediaSettings,
            Lazy<ContactDataSettings> contactDataSettings,
            Lazy<CustomerSettings> customerSettings,
            Lazy<CatalogSettings> catalogSettings,
            Lazy<LocalizationSettings> localizationSettings,
            Lazy<TaxSettings> taxSettings,
            Lazy<SeoSettings> seoSettings)
        {
            _services = services;
            _dbContext = dbContext;
            _httpContext = httpContext;
            _priceFormatter = priceFormatter;
            _exportProfileService = exportProfileService;
            _localizedEntityService = localizedEntityService;
            _languageService = languageService;
            _urlRecordService = urlRecordService;
            _mediaService = mediaService;
            _priceCalculationService = priceCalculationService;
            _currencyService = currencyService;
            _taxService = taxService;
            _categoryService = categoryService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _specificationAttributeService = specificationAttributeService;
            _productTemplateService = productTemplateService;
            _categoryTemplateService = categoryTemplateService;
            _productService = productService;
            _orderService = orderService;
            _manufacturerService = manufacturerService;
            _customerService = customerService;
            _addressService = addressService;
            _countryService = countryService;
            _shipmentService = shipmentService;
            _genericAttributeService = genericAttributeService;
            _emailAccountService = emailAccountService;
            _queuedEmailService = queuedEmailService;
            _emailSender = emailSender;
            _deliveryTimeService = deliveryTimeService;
            _quantityUnitService = quantityUnitService;
            _catalogSearchService = catalogSearchService;
            _downloadService = downloadService;
            _productUrlHelper = productUrlHelper;

            _customerRepository = customerRepository;
            _subscriptionRepository = subscriptionRepository;
            _orderRepository = orderRepository;
            _shoppingCartItemRepository = shoppingCartItemRepository;

            _mediaSettings = mediaSettings;
            _contactDataSettings = contactDataSettings;
            _customerSettings = customerSettings;
            _catalogSettings = catalogSettings;
            _localizationSettings = localizationSettings;
            _taxSettings = taxSettings;
            _seoSettings = seoSettings;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;

        #endregion

        #region Utilities

        private LocalizedPropertyCollection CreateTranslationCollection(string keyGroup, IEnumerable<BaseEntity> entities)
        {
            if (entities == null || !entities.Any())
            {
                return new LocalizedPropertyCollection(keyGroup, null, Enumerable.Empty<LocalizedProperty>());
            }

            var collection = _localizedEntityService.Value.GetLocalizedPropertyCollection(keyGroup, entities.Select(x => x.Id).Distinct().ToArray());
            return collection;
        }

        private UrlRecordCollection CreateUrlRecordCollection(string entityName, IEnumerable<BaseEntity> entities)
        {
            if (entities == null || !entities.Any())
            {
                return new UrlRecordCollection(entityName, null, Enumerable.Empty<UrlRecord>());
            }

            var collection = _urlRecordService.Value.GetUrlRecordCollection(entityName, null, entities.Select(x => x.Id).Distinct().ToArray());
            return collection;
        }

        private void SetProgress(DataExporterContext ctx, int loadedRecords)
        {
            try
            {
                if (!ctx.IsPreview && loadedRecords > 0)
                {
                    var totalRecords = ctx.StatsPerStore.Sum(x => x.Value.TotalRecords);

                    if (ctx.Request.Profile.Limit > 0 && totalRecords > ctx.Request.Profile.Limit)
                    {
                        totalRecords = ctx.Request.Profile.Limit;
                    }

                    ctx.RecordCount = Math.Min(ctx.RecordCount + loadedRecords, totalRecords);
                    var msg = ctx.ProgressInfo.FormatInvariant(ctx.RecordCount.ToString("N0"), totalRecords.ToString("N0"));
                    ctx.Request.ProgressValueSetter.Invoke(ctx.RecordCount, totalRecords, msg);
                }
            }
            catch { }
        }

        private void SetProgress(DataExporterContext ctx, string message)
        {
            try
            {
                if (!ctx.IsPreview && message.HasValue())
                {
                    ctx.Request.ProgressValueSetter.Invoke(0, 0, message);
                }
            }
            catch { }
        }

        private bool HasPermission(DataExporterContext ctx)
        {
            if (ctx.Request.HasPermission)
            {
                return true;
            }

            var customer = _services.WorkContext.CurrentCustomer;

            if (customer.SystemName == SystemCustomerNames.BackgroundTask)
            {
                return true;
            }

            return _services.Permissions.Authorize(Permissions.Configuration.Export.Execute);
        }

        private void DetachAllEntitiesAndClear(DataExporterContext ctx)
        {
            try
            {
                ctx.AssociatedProductContext?.Clear();

                if (ctx.ProductExportContext != null)
                {
                    _dbContext.DetachEntities(x =>
                    {
                        return x is Product || x is Discount || x is ProductVariantAttributeCombination || x is ProductVariantAttribute || x is ProductVariantAttributeValue || x is ProductAttribute ||
                               x is MediaFile || x is ProductBundleItem || x is ProductBundleItemAttributeFilter || x is ProductCategory || x is ProductManufacturer || x is Category || x is Manufacturer ||
                               x is ProductMediaFile || x is ProductTag || x is ProductSpecificationAttribute || x is SpecificationAttributeOption || x is SpecificationAttribute || x is TierPrice || x is ProductReview ||
                               x is ProductReviewHelpfulness || x is DeliveryTime || x is QuantityUnit || x is Download || x is MediaStorage || x is GenericAttribute || x is UrlRecord;
                    });

                    ctx.ProductExportContext.Clear();
                }

                if (ctx.OrderExportContext != null)
                {
                    _dbContext.DetachEntities(x =>
                    {
                        return x is Order || x is Address || x is GenericAttribute || x is Customer ||
                               x is OrderItem || x is RewardPointsHistory || x is Shipment || x is ProductVariantAttributeCombination;
                    });

                    ctx.OrderExportContext.Clear();
                }

                if (ctx.CategoryExportContext != null)
                {
                    _dbContext.DetachEntities(x =>
                    {
                        return x is Category || x is MediaFile || x is ProductCategory;
                    });

                    ctx.CategoryExportContext.Clear();
                }

                if (ctx.ManufacturerExportContext != null)
                {
                    _dbContext.DetachEntities(x =>
                    {
                        return x is Manufacturer || x is MediaFile || x is ProductManufacturer;
                    });

                    ctx.ManufacturerExportContext.Clear();
                }

                if (ctx.CustomerExportContext != null)
                {
                    _dbContext.DetachEntities(x =>
                    {
                        return x is Customer || x is GenericAttribute || x is CustomerContent;
                    });

                    ctx.CustomerExportContext.Clear();
                }

                switch (ctx.Request.Provider.Value.EntityType)
                {
                    case ExportEntityType.ShoppingCartItem:
                        _dbContext.DetachEntities(x =>
                        {
                            return x is ShoppingCartItem || x is Customer || x is Product || x is ProductVariantAttributeCombination;
                        });
                        break;
                    case ExportEntityType.NewsLetterSubscription:
                        _dbContext.DetachEntities(x =>
                        {
                            return x is NewsLetterSubscription || x is Customer;
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                ctx.Log.Warn(ex, "Detaching entities failed.");
            }

            //((DbContext)_dbContext).DumpAttachedEntities();
        }

        private IExportDataSegmenterProvider CreateSegmenter(DataExporterContext ctx)
        {
            var stats = ctx.StatsPerStore[ctx.Store.Id];
            var offset = Math.Max(ctx.Request.Profile.Offset, 0);
            var limit = Math.Max(ctx.Request.Profile.Limit, 0);
            var recordsPerSegment = ctx.IsPreview ? 0 : Math.Max(ctx.Request.Profile.BatchSize, 0);
            var totalRecords = offset + stats.TotalRecords;

            switch (ctx.Request.Provider.Value.EntityType)
            {
                case ExportEntityType.Product:
                    ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Product>
                    (
                        () => GetProducts(ctx),
                        entities =>
                        {
                            // Load data behind navigation properties for current queue in one go.
                            ctx.ProductExportContext = CreateProductExportContext(entities, ctx.ContextCustomer, ctx.Store.Id);
                            ctx.AssociatedProductContext = null;

                            var context = ctx.ProductExportContext;
                            if (!ctx.Projection.NoGroupedProducts && entities.Where(x => x.ProductType == ProductType.GroupedProduct).Any())
                            {
                                context.AssociatedProducts.LoadAll();
                                var associatedProducts = context.AssociatedProducts.SelectMany(x => x.Value);
                                ctx.AssociatedProductContext = CreateProductExportContext(associatedProducts, ctx.ContextCustomer, ctx.Store.Id);

                                var allProductEntities = entities.Where(x => x.ProductType != ProductType.GroupedProduct).Concat(associatedProducts);
                                ctx.TranslationsPerPage[nameof(Product)] = CreateTranslationCollection(nameof(Product), allProductEntities);
                                ctx.UrlRecordsPerPage[nameof(Product)] = CreateUrlRecordCollection(nameof(Product), allProductEntities);
                            }
                            else
                            {
                                ctx.TranslationsPerPage[nameof(Product)] = CreateTranslationCollection(nameof(Product), entities);
                                ctx.UrlRecordsPerPage[nameof(Product)] = CreateUrlRecordCollection(nameof(Product), entities);
                            }

                            context.ProductTags.LoadAll();
                            context.ProductBundleItems.LoadAll();
                            context.SpecificationAttributes.LoadAll();
                            context.Attributes.LoadAll();

                            var psa = context.SpecificationAttributes.SelectMany(x => x.Value);
                            var sao = psa.Select(x => x.SpecificationAttributeOption);
                            var sa = psa.Select(x => x.SpecificationAttributeOption.SpecificationAttribute);

                            var pva = context.Attributes.SelectMany(x => x.Value);
                            var pvav = pva.SelectMany(x => x.ProductVariantAttributeValues);
                            var pa = pva.Select(x => x.ProductAttribute);

                            ctx.TranslationsPerPage[nameof(ProductTag)] = CreateTranslationCollection(nameof(ProductTag), context.ProductTags.SelectMany(x => x.Value));
                            ctx.TranslationsPerPage[nameof(ProductBundleItem)] = CreateTranslationCollection(nameof(ProductBundleItem), context.ProductBundleItems.SelectMany(x => x.Value));
                            ctx.TranslationsPerPage[nameof(SpecificationAttribute)] = CreateTranslationCollection(nameof(SpecificationAttribute), sa);
                            ctx.TranslationsPerPage[nameof(SpecificationAttributeOption)] = CreateTranslationCollection(nameof(SpecificationAttributeOption), sao);
                            ctx.TranslationsPerPage[nameof(ProductAttribute)] = CreateTranslationCollection(nameof(ProductAttribute), pa);
                            ctx.TranslationsPerPage[nameof(ProductVariantAttributeValue)] = CreateTranslationCollection(nameof(ProductVariantAttributeValue), pvav);
                        },
                        entity => Convert(ctx, entity),
                        offset, PageSize, limit, recordsPerSegment, totalRecords
                    );
                    break;

                case ExportEntityType.Order:
                    ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Order>
                    (
                        () => GetOrders(ctx),
                        entities =>
                        {
                            ctx.OrderExportContext = new OrderExportContext(entities,
                                x => _customerService.GetCustomersByIds(x),
                                x => _genericAttributeService.Value.GetAttributesForEntity(x, "Customer"),
                                x => _customerService.GetRewardPointsHistoriesByCustomerIds(x),
                                x => _addressService.Value.GetAddressByIds(x),
                                x => _orderService.Value.GetOrderItemsByOrderIds(x),
                                x => _shipmentService.Value.GetShipmentsByOrderIds(x)
                            );

                            ctx.OrderExportContext.OrderItems.LoadAll();

                            var orderItems = ctx.OrderExportContext.OrderItems.SelectMany(x => x.Value);
                            var products = orderItems.Select(x => x.Product);

                            ctx.TranslationsPerPage[nameof(Product)] = CreateTranslationCollection(nameof(Product), products);
                            ctx.UrlRecordsPerPage[nameof(Product)] = CreateUrlRecordCollection(nameof(Product), products);
                        },
                        entity => Convert(ctx, entity),
                        offset, PageSize, limit, recordsPerSegment, totalRecords
                    );
                    break;

                case ExportEntityType.Manufacturer:
                    ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Manufacturer>
                    (
                        () => GetManufacturers(ctx),
                        entities =>
                        {
                            ctx.ManufacturerExportContext = new ManufacturerExportContext(entities,
                                x => _manufacturerService.Value.GetProductManufacturersByManufacturerIds(x, true),
                                x => _mediaService.Value.GetFilesByIds(x)
                            );
                        },
                        entity => Convert(ctx, entity),
                        offset, PageSize, limit, recordsPerSegment, totalRecords
                    );
                    break;

                case ExportEntityType.Category:
                    ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Category>
                    (
                        () => GetCategories(ctx),
                        entities =>
                        {
                            ctx.CategoryExportContext = new CategoryExportContext(entities,
                                x => _categoryService.Value.GetProductCategoriesByCategoryIds(x),
                                x => _mediaService.Value.GetFilesByIds(x)
                            );
                        },
                        entity => Convert(ctx, entity),
                        offset, PageSize, limit, recordsPerSegment, totalRecords
                    );
                    break;

                case ExportEntityType.Customer:
                    ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Customer>
                    (
                        () => GetCustomers(ctx),
                        entities =>
                        {
                            ctx.CustomerExportContext = new CustomerExportContext(entities,
                                x => _genericAttributeService.Value.GetAttributesForEntity(x, "Customer")
                            );
                        },
                        entity => Convert(ctx, entity),
                        offset, PageSize, limit, recordsPerSegment, totalRecords
                    );
                    break;

                case ExportEntityType.NewsLetterSubscription:
                    ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<NewsLetterSubscription>
                    (
                        () => GetNewsLetterSubscriptions(ctx),
                        null,
                        entity => Convert(ctx, entity),
                        offset, PageSize, limit, recordsPerSegment, totalRecords
                    );
                    break;

                case ExportEntityType.ShoppingCartItem:
                    ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<ShoppingCartItem>
                    (
                        () => GetShoppingCartItems(ctx),
                        entities =>
                        {
                            var products = entities.Select(x => x.Product);
                            ctx.TranslationsPerPage[nameof(Product)] = CreateTranslationCollection(nameof(Product), products);
                            ctx.UrlRecordsPerPage[nameof(Product)] = CreateUrlRecordCollection(nameof(Product), products);
                        },
                        entity => Convert(ctx, entity),
                        offset, PageSize, limit, recordsPerSegment, totalRecords
                    );
                    break;

                default:
                    ctx.ExecuteContext.DataSegmenter = null;
                    break;
            }

            return ctx.ExecuteContext.DataSegmenter as IExportDataSegmenterProvider;
        }

        private Stream CreateStream(string path)
        {
            if (path.HasValue())
            {
                return new FileStream(path, FileMode.Create, FileAccess.Write);
            }

            return new MemoryStream();
        }

        private IEnumerable<ExportDataUnit> GetDataUnitsForRelatedEntities(DataExporterContext ctx)
        {
            // Related data is data without own export provider or importer. For a flat formatted export
            // they have to be exported together with metadata to know what to be edited.
            RelatedEntityType[] types;

            switch (ctx.Request.Provider.Value.EntityType)
            {
                case ExportEntityType.Product:
                    types = new RelatedEntityType[]
                    {
                        RelatedEntityType.TierPrice,
                        RelatedEntityType.ProductVariantAttributeValue,
                        RelatedEntityType.ProductVariantAttributeCombination
                    };
                    break;
                default:
                    return Enumerable.Empty<ExportDataUnit>();
            }

            var result = new List<ExportDataUnit>();
            var context = ctx.ExecuteContext;
            var fileExtension = Path.GetExtension(context.FileName);

            foreach (var type in types)
            {
                var unit = new ExportDataUnit
                {
                    RelatedType = type,
                    DisplayInFileDialog = true
                };

                // Convention: Must end with type name because that's how the import identifies the entity.
                // Be careful in case of accidents with file names. They must not be too long.
                var fileName = $"{ctx.Store.Id}-{context.FileIndex.ToString("D4")}-{type.ToString()}";

                if (File.Exists(Path.Combine(context.Folder, fileName + fileExtension)))
                {
                    fileName = $"{CommonHelper.GenerateRandomDigitCode(4)}-{fileName}";
                }

                unit.FileName = fileName + fileExtension;
                unit.DataStream = new ExportFileStream(CreateStream(Path.Combine(context.Folder, unit.FileName)));

                result.Add(unit);
            }

            return result;
        }

        private bool CallProvider(DataExporterContext ctx, string method, string path)
        {
            if (method != "Execute" && method != "OnExecuted")
            {
                throw new SmartException($"Unknown export method {method.NaIfEmpty()}.");
            }

            try
            {
                using (ctx.ExecuteContext.DataStream = new ExportFileStream(CreateStream(ctx.IsFileBasedExport ? path : null)))
                {
                    if (method == "Execute")
                    {
                        ctx.Request.Provider.Value.Execute(ctx.ExecuteContext);
                    }
                    else if (method == "OnExecuted")
                    {
                        ctx.Request.Provider.Value.OnExecuted(ctx.ExecuteContext);
                    }
                }
            }
            catch (Exception ex)
            {
                ctx.ExecuteContext.Abort = DataExchangeAbortion.Hard;
                ctx.Log.ErrorFormat(ex, $"The provider failed at the {method.NaIfEmpty()} method.");
                ctx.Result.LastError = ex.ToAllMessages(true);
            }
            finally
            {
                ctx.ExecuteContext.DataStream?.Dispose();
                ctx.ExecuteContext.DataStream = null;

                if (ctx.ExecuteContext.Abort == DataExchangeAbortion.Hard && ctx.IsFileBasedExport)
                {
                    FileSystemHelper.DeleteFile(path);
                }

                if (method == "Execute")
                {
                    var unitFileInfos = new StringBuilder();
                    var relatedDataUnits = ctx.ExecuteContext.ExtraDataUnits
                        .Where(x => x.RelatedType.HasValue && x.FileName.HasValue() && x.DataStream != null)
                        .ToList();

                    foreach (var unit in relatedDataUnits)
                    {
                        // Set unit.DataStream to null so that providers know that this unit should no longer be written to.
                        // We need these units later for ExportProfile.ResultInfo.
                        unit.DataStream?.Dispose();
                        unit.DataStream = null;

                        var unitPath = Path.Combine(ctx.ExecuteContext.Folder, unit.FileName);

                        if (ctx.ExecuteContext.Abort == DataExchangeAbortion.Hard)
                        {
                            if (ctx.IsFileBasedExport)
                            {
                                FileSystemHelper.DeleteFile(unitPath);
                            }
                        }
                        else
                        {
                            unitFileInfos.Append($"\r\nProvider reports {unit.RecordsSucceeded.ToString("N0")} successfully exported record(s) of type {unit.RelatedType.Value.ToString()} to {unitPath}.");
                        }
                    }

                    if (ctx.ExecuteContext.Abort != DataExchangeAbortion.Hard)
                    {
                        ctx.Log.Info($"Provider reports {ctx.ExecuteContext.RecordsSucceeded.ToString("N0")} successfully exported record(s) of type {ctx.Request.Provider.Value.EntityType.ToString()} to {path.NaIfEmpty()}.");
                    }

                    ctx.Log.Info(unitFileInfos.ToString());
                }
            }

            return ctx.ExecuteContext.Abort != DataExchangeAbortion.Hard;
        }

        private bool Deploy(DataExporterContext ctx, string zipPath)
        {
            var allSucceeded = true;
            var deployments = ctx.Request.Profile.Deployments.OrderBy(x => x.DeploymentTypeId).Where(x => x.Enabled);

            if (deployments.Count() == 0)
                return false;

            var context = new ExportDeploymentContext
            {
                T = T,
                Log = ctx.Log,
                FolderContent = ctx.FolderContent,
                ZipPath = zipPath,
                CreateZipArchive = ctx.Request.Profile.CreateZipArchive
            };

            foreach (var deployment in deployments)
            {
                IFilePublisher publisher = null;

                context.Result = new DataDeploymentResult
                {
                    LastExecutionUtc = DateTime.UtcNow
                };

                try
                {
                    switch (deployment.DeploymentType)
                    {
                        case ExportDeploymentType.Email:
                            publisher = new EmailFilePublisher(_emailAccountService.Value, _queuedEmailService.Value);
                            break;
                        case ExportDeploymentType.FileSystem:
                            publisher = new FileSystemFilePublisher();
                            break;
                        case ExportDeploymentType.Ftp:
                            publisher = new FtpFilePublisher();
                            break;
                        case ExportDeploymentType.Http:
                            publisher = new HttpFilePublisher();
                            break;
                        case ExportDeploymentType.PublicFolder:
                            publisher = new PublicFolderPublisher();
                            break;
                    }

                    if (publisher != null)
                    {
                        publisher.Publish(context, deployment);

                        if (!context.Result.Succeeded)
                            allSucceeded = false;
                    }
                }
                catch (Exception ex)
                {
                    allSucceeded = false;

                    if (context.Result != null)
                    {
                        context.Result.LastError = ex.ToAllMessages(true);
                    }

                    ctx.Log.Error(ex, $"Deployment \"{deployment.Name}\" of type {deployment.DeploymentType.ToString()} failed.");
                }

                deployment.ResultInfo = XmlHelper.Serialize(context.Result);

                _exportProfileService.Value.UpdateExportDeployment(deployment);
            }

            return allSucceeded;
        }

        private void SendCompletionEmail(DataExporterContext ctx, string zipPath)
        {
            var emailAccount = _emailAccountService.Value.GetEmailAccountById(ctx.Request.Profile.EmailAccountId);
            if (emailAccount == null)
            {
                return;
            }

            var downloadUrl = "{0}Admin/Export/DownloadExportFile/{1}?name=".FormatInvariant(_services.WebHelper.GetStoreLocation(ctx.Store.SslEnabled), ctx.Request.Profile.Id);
            var languageId = ctx.Projection.LanguageId ?? 0;
            var smtpContext = new SmtpContext(emailAccount);
            var message = new EmailMessage();

            var storeInfo = "{0} ({1})".FormatInvariant(ctx.Store.Name, ctx.Store.Url);
            var intro = _services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Body", languageId).FormatInvariant(storeInfo);
            var body = new StringBuilder(intro);

            if (ctx.Result.LastError.HasValue())
            {
                body.AppendFormat("<p style=\"color: #B94A48;\">{0}</p>", ctx.Result.LastError);
            }

            if (ctx.IsFileBasedExport && File.Exists(zipPath))
            {
                var fileName = Path.GetFileName(zipPath);
                body.AppendFormat("<p><a href='{0}{1}' download>{2}</a></p>", downloadUrl, HttpUtility.UrlEncode(fileName), fileName);
            }

            if (ctx.IsFileBasedExport && ctx.Result.Files.Any())
            {
                body.Append("<p>");
                foreach (var file in ctx.Result.Files)
                {
                    body.AppendFormat("<div><a href='{0}{1}' download>{2}</a></div>", downloadUrl, HttpUtility.UrlEncode(file.FileName), file.FileName);
                }
                body.Append("</p>");
            }

            message.From = new EmailAddress(emailAccount.Email, emailAccount.DisplayName);

            if (ctx.Request.Profile.CompletedEmailAddresses.HasValue())
                message.To.AddRange(ctx.Request.Profile.CompletedEmailAddresses.SplitSafe(",").Where(x => x.IsEmail()).Select(x => new EmailAddress(x)));

            if (message.To.Count == 0 && _contactDataSettings.Value.CompanyEmailAddress.HasValue())
                message.To.Add(new EmailAddress(_contactDataSettings.Value.CompanyEmailAddress));

            if (message.To.Count == 0)
                message.To.Add(new EmailAddress(emailAccount.Email, emailAccount.DisplayName));

            message.Subject = _services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Subject", languageId)
                .FormatInvariant(ctx.Request.Profile.Name);

            message.Body = body.ToString();

            _emailSender.Value.SendEmail(smtpContext, message);

            //_queuedEmailService.Value.InsertQueuedEmail(new QueuedEmail
            //{
            //	From = emailAccount.Email,
            //	FromName = emailAccount.DisplayName,
            //	To = message.To.First().Address,
            //	Subject = message.Subject,
            //	Body = message.Body,
            //	CreatedOnUtc = DateTime.UtcNow,
            //	EmailAccountId = emailAccount.Id,
            //	SendManually = true
            //});
            //_dbContext.SaveChanges();
        }

        #endregion

        #region Getting data

        public virtual ProductExportContext CreateProductExportContext(
            IEnumerable<Product> products = null,
            Customer customer = null,
            int? storeId = null,
            int? maxPicturesPerProduct = null,
            bool includeHidden = true)
        {
            if (customer == null)
                customer = _services.WorkContext.CurrentCustomer;

            if (!storeId.HasValue)
                storeId = _services.StoreContext.CurrentStore.Id;

            var context = new ProductExportContext(products,
                x => _productAttributeService.Value.GetProductVariantAttributesByProductIds(x, null),
                x => _productAttributeService.Value.GetProductVariantAttributeCombinations(x),
                x => _specificationAttributeService.Value.GetProductSpecificationAttributesByProductIds(x),
                x => _productService.Value.GetTierPricesByProductIds(x, customer, storeId.GetValueOrDefault()),
                x => _categoryService.Value.GetProductCategoriesByProductIds(x, null, includeHidden),
                x => _manufacturerService.Value.GetProductManufacturersByProductIds(x, includeHidden),
                x => _productService.Value.GetAppliedDiscountsByProductIds(x),
                x => _productService.Value.GetBundleItemsByProductIds(x, includeHidden),
                x => _productService.Value.GetAssociatedProductsByProductIds(x, includeHidden),
                x => _productService.Value.GetProductPicturesByProductIds(x, maxPicturesPerProduct),
                x => _productService.Value.GetProductTagsByProductIds(x, includeHidden),
                x => _downloadService.Value.GetDownloadsByEntityIds(x, nameof(Product))
            );

            return context;
        }

        private IQueryable<Product> GetProductQuery(DataExporterContext ctx, int? skip, int take)
        {
            IQueryable<Product> query = null;

            // Skip used for preview (ordinary paging) or initial query (offset option).
            var skipValue = skip.GetValueOrDefault();
            if (skipValue == 0 && ctx.LastId == 0)
            {
                // Initial query. Apply offset.
                skipValue = Math.Max(ctx.Request.Profile.Offset, 0);
            }

            if (ctx.Request.ProductQuery == null)
            {
                var f = ctx.Filter;
                var createdFrom = f.CreatedFrom.HasValue ? (DateTime?)_services.DateTimeHelper.ConvertToUtcTime(f.CreatedFrom.Value, _services.DateTimeHelper.CurrentTimeZone) : null;
                var createdTo = f.CreatedTo.HasValue ? (DateTime?)_services.DateTimeHelper.ConvertToUtcTime(f.CreatedTo.Value, _services.DateTimeHelper.CurrentTimeZone) : null;

                var searchQuery = new CatalogSearchQuery()
                    .WithCurrency(ctx.ContextCurrency)
                    .WithLanguage(ctx.ContextLanguage)
                    .HasStoreId(ctx.Request.Profile.PerStore ? ctx.Store.Id : f.StoreId)
                    .PriceBetween(f.PriceMinimum, f.PriceMaximum)
                    .WithStockQuantity(f.AvailabilityMinimum, f.AvailabilityMaximum)
                    .CreatedBetween(createdFrom, createdTo);

                if (f.Visibility.HasValue)
                    searchQuery = searchQuery.WithVisibility(f.Visibility.Value);

                if (f.IsPublished.HasValue)
                    searchQuery = searchQuery.PublishedOnly(f.IsPublished.Value);

                if (f.ProductType.HasValue)
                    searchQuery = searchQuery.IsProductType(f.ProductType.Value);

                if (f.ProductTagId.HasValue)
                    searchQuery = searchQuery.WithProductTagIds(f.ProductTagId.Value);

                if (f.WithoutManufacturers.HasValue)
                    searchQuery = searchQuery.HasAnyManufacturer(!f.WithoutManufacturers.Value);
                else if (f.ManufacturerId.HasValue)
                    searchQuery = searchQuery.WithManufacturerIds(f.FeaturedProducts, f.ManufacturerId.Value);

                if (f.WithoutCategories.HasValue)
                    searchQuery = searchQuery.HasAnyCategory(!f.WithoutCategories.Value);
                else if (f.CategoryIds != null && f.CategoryIds.Length > 0)
                    searchQuery = searchQuery.WithCategoryIds(f.FeaturedProducts, f.CategoryIds);

                if (ctx.Request.EntitiesToExport.Count > 0)
                    searchQuery = searchQuery.WithProductIds(ctx.Request.EntitiesToExport.ToArray());
                else
                    searchQuery = searchQuery.WithProductId(f.IdMinimum, f.IdMaximum);

                query = _catalogSearchService.Value.PrepareQuery(searchQuery);
            }
            else
            {
                query = ctx.Request.ProductQuery;
            }

            query = query.OrderBy(x => x.Id);

            if (skipValue > 0)
            {
                query = query.Skip(() => skipValue);
            }
            else if (ctx.LastId > 0)
            {
                // Fast, ID based paging.
                query = query.Where(x => x.Id > ctx.LastId);
            }

            if (take != int.MaxValue)
            {
                query = query.Take(() => take);
            }

            return query;
        }

        private List<Product> GetProducts(DataExporterContext ctx)
        {
            DetachAllEntitiesAndClear(ctx);

            var stats = ctx.StatsPerStore[ctx.Store.Id];
            if (ctx.LastId >= stats.MaxId)
            {
                // End of data reached.
                return null;
            }

            var products = GetProductQuery(ctx, null, PageSize).ToList();
            if (!products.Any())
            {
                return null;
            }

            var result = new List<Product>();
            Multimap<int, Product> associatedProducts = null;

            if (ctx.Projection.NoGroupedProducts)
            {
                var groupedProductIds = products.Where(x => x.ProductType == ProductType.GroupedProduct).Select(x => x.Id).ToArray();
                associatedProducts = _productService.Value.GetAssociatedProductsByProductIds(groupedProductIds, true);
            }

            foreach (var product in products)
            {
                if (product.ProductType == ProductType.SimpleProduct || product.ProductType == ProductType.BundledProduct)
                {
                    // We use ctx.EntityIdsPerSegment to avoid exporting products multiple times per segment\file (cause of associated products).
                    if (ctx.EntityIdsPerSegment.Add(product.Id))
                    {
                        result.Add(product);
                    }
                }
                else if (product.ProductType == ProductType.GroupedProduct)
                {
                    if (ctx.Projection.NoGroupedProducts)
                    {
                        if (associatedProducts.ContainsKey(product.Id))
                        {
                            foreach (var associatedProduct in associatedProducts[product.Id])
                            {
                                if (ctx.Projection.OnlyIndividuallyVisibleAssociated && associatedProduct.Visibility == ProductVisibility.Hidden)
                                {
                                    continue;
                                }
                                if (ctx.Filter.IsPublished.HasValue && ctx.Filter.IsPublished.Value != associatedProduct.Published)
                                {
                                    continue;
                                }

                                if (ctx.EntityIdsPerSegment.Add(associatedProduct.Id))
                                {
                                    result.Add(associatedProduct);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (ctx.EntityIdsPerSegment.Add(product.Id))
                        {
                            result.Add(product);
                        }
                    }
                }
            }

            ctx.LastId = products.Last().Id;
            SetProgress(ctx, products.Count);

            return result;
        }

        private IQueryable<Order> GetOrderQuery(DataExporterContext ctx, int? skip, int take)
        {
            var skipValue = skip.GetValueOrDefault();
            if (skipValue == 0 && ctx.LastId == 0)
            {
                skipValue = Math.Max(ctx.Request.Profile.Offset, 0);
            }

            var query = _orderService.Value.GetOrders(
                ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId,
                ctx.Projection.CustomerId ?? 0,
                ctx.Filter.CreatedFrom.HasValue ? (DateTime?)_services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _services.DateTimeHelper.CurrentTimeZone) : null,
                ctx.Filter.CreatedTo.HasValue ? (DateTime?)_services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _services.DateTimeHelper.CurrentTimeZone) : null,
                ctx.Filter.OrderStatusIds,
                ctx.Filter.PaymentStatusIds,
                ctx.Filter.ShippingStatusIds,
                null,
                null,
                null);

            if (ctx.Request.EntitiesToExport.Any())
            {
                query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));
            }

            query = query.OrderBy(x => x.Id);

            if (skipValue > 0)
            {
                query = query.Skip(() => skipValue);
            }
            else if (ctx.LastId > 0)
            {
                query = query.Where(x => x.Id > ctx.LastId);
            }

            if (take != int.MaxValue)
            {
                query = query.Take(() => take);
            }

            return query;
        }

        private List<Order> GetOrders(DataExporterContext ctx)
        {
            DetachAllEntitiesAndClear(ctx);

            var stats = ctx.StatsPerStore[ctx.Store.Id];
            if (ctx.LastId >= stats.MaxId)
            {
                // End of data reached.
                return null;
            }

            var orders = GetOrderQuery(ctx, null, PageSize).ToList();
            if (!orders.Any())
            {
                return null;
            }

            if (ctx.Projection.OrderStatusChange != ExportOrderStatusChange.None)
            {
                ctx.SetLoadedEntityIds(orders.Select(x => x.Id));
            }

            ctx.LastId = orders.Last().Id;
            SetProgress(ctx, orders.Count);

            return orders;
        }

        private IQueryable<Manufacturer> GetManufacturerQuery(DataExporterContext ctx, int? skip, int take)
        {
            var storeId = ctx.Request.Profile.PerStore ? ctx.Store.Id : 0;

            var skipValue = skip.GetValueOrDefault();
            if (skipValue == 0 && ctx.LastId == 0)
            {
                skipValue = Math.Max(ctx.Request.Profile.Offset, 0);
            }

            var query = _manufacturerService.Value.GetManufacturers(true, storeId);

            if (ctx.Request.EntitiesToExport.Any())
            {
                query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));
            }

            query = query.OrderBy(x => x.Id);

            if (skipValue > 0)
            {
                query = query.Skip(() => skipValue);
            }
            else if (ctx.LastId > 0)
            {
                query = query.Where(x => x.Id > ctx.LastId);
            }

            if (take != int.MaxValue)
            {
                query = query.Take(() => take);
            }

            return query;
        }

        private List<Manufacturer> GetManufacturers(DataExporterContext ctx)
        {
            DetachAllEntitiesAndClear(ctx);

            var stats = ctx.StatsPerStore[ctx.Store.Id];
            if (ctx.LastId >= stats.MaxId)
            {
                // End of data reached.
                return null;
            }

            var manus = GetManufacturerQuery(ctx, null, PageSize).ToList();
            if (!manus.Any())
            {
                return null;
            }

            ctx.LastId = manus.Last().Id;
            SetProgress(ctx, manus.Count);

            return manus;
        }

        private IQueryable<Category> GetCategoryQuery(DataExporterContext ctx, int? skip, int take)
        {
            var storeId = ctx.Request.Profile.PerStore ? ctx.Store.Id : 0;

            var skipValue = skip.GetValueOrDefault();
            if (skipValue == 0 && ctx.LastId == 0)
            {
                skipValue = Math.Max(ctx.Request.Profile.Offset, 0);
            }

            var query = _categoryService.Value.BuildCategoriesQuery(null, true, null, storeId);

            if (ctx.Request.EntitiesToExport.Any())
            {
                query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));
            }

            query = query.OrderBy(x => x.Id);

            if (skipValue > 0)
            {
                query = query.Skip(() => skipValue);
            }
            else if (ctx.LastId > 0)
            {
                query = query.Where(x => x.Id > ctx.LastId);
            }

            if (take != int.MaxValue)
            {
                query = query.Take(() => take);
            }

            return query;
        }

        private List<Category> GetCategories(DataExporterContext ctx)
        {
            DetachAllEntitiesAndClear(ctx);

            var stats = ctx.StatsPerStore[ctx.Store.Id];
            if (ctx.LastId >= stats.MaxId)
            {
                // End of data reached.
                return null;
            }

            var categories = GetCategoryQuery(ctx, null, PageSize).ToList();
            if (!categories.Any())
            {
                return null;
            }

            ctx.LastId = categories.Last().Id;
            SetProgress(ctx, categories.Count);

            return categories;
        }

        private IQueryable<Customer> GetCustomerQuery(DataExporterContext ctx, int? skip, int take)
        {
            var skipValue = skip.GetValueOrDefault();
            if (skipValue == 0 && ctx.LastId == 0)
            {
                skipValue = Math.Max(ctx.Request.Profile.Offset, 0);
            }

            var query = _customerRepository.Value.TableUntracked
                .Expand(x => x.BillingAddress)
                .Expand(x => x.ShippingAddress)
                .Expand(x => x.Addresses.Select(y => y.Country))
                .Expand(x => x.Addresses.Select(y => y.StateProvince))
                .Expand(x => x.CustomerRoleMappings.Select(rm => rm.CustomerRole))
                .Where(x => !x.Deleted);

            if (ctx.Filter.IsActiveCustomer.HasValue)
            {
                query = query.Where(x => x.Active == ctx.Filter.IsActiveCustomer.Value);
            }

            if (ctx.Filter.IsTaxExempt.HasValue)
            {
                query = query.Where(x => x.IsTaxExempt == ctx.Filter.IsTaxExempt.Value);
            }

            if (ctx.Filter.CustomerRoleIds != null && ctx.Filter.CustomerRoleIds.Any())
            {
                query = query.Where(x => x.CustomerRoleMappings.Select(y => y.CustomerRoleId).Intersect(ctx.Filter.CustomerRoleIds).Any());
            }

            if (ctx.Filter.BillingCountryIds != null && ctx.Filter.BillingCountryIds.Any())
            {
                query = query.Where(x => x.BillingAddress != null && ctx.Filter.BillingCountryIds.Contains(x.BillingAddress.CountryId ?? 0));
            }

            if (ctx.Filter.ShippingCountryIds != null && ctx.Filter.ShippingCountryIds.Any())
            {
                query = query.Where(x => x.ShippingAddress != null && ctx.Filter.ShippingCountryIds.Contains(x.ShippingAddress.CountryId ?? 0));
            }

            if (ctx.Filter.LastActivityFrom.HasValue)
            {
                var activityFrom = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.LastActivityFrom.Value, _services.DateTimeHelper.CurrentTimeZone);
                query = query.Where(x => activityFrom <= x.LastActivityDateUtc);
            }

            if (ctx.Filter.LastActivityTo.HasValue)
            {
                var activityTo = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.LastActivityTo.Value, _services.DateTimeHelper.CurrentTimeZone);
                query = query.Where(x => activityTo >= x.LastActivityDateUtc);
            }

            if (ctx.Filter.HasSpentAtLeastAmount.HasValue)
            {
                query = query
                    .Join(_orderRepository.Value.Table, x => x.Id, y => y.CustomerId, (x, y) => new { Customer = x, Order = y })
                    .GroupBy(x => x.Customer.Id)
                    .Select(x => new
                    {
                        Customer = x.FirstOrDefault().Customer,
                        OrderTotal = x.Sum(y => y.Order.OrderTotal)
                    })
                    .Where(x => x.OrderTotal >= ctx.Filter.HasSpentAtLeastAmount.Value)
                    .Select(x => x.Customer);
            }

            if (ctx.Filter.HasPlacedAtLeastOrders.HasValue)
            {
                query = query
                    .Join(_orderRepository.Value.Table, x => x.Id, y => y.CustomerId, (x, y) => new { Customer = x, Order = y })
                    .GroupBy(x => x.Customer.Id)
                    .Select(x => new
                    {
                        Customer = x.FirstOrDefault().Customer,
                        OrderCount = x.Count()
                    })
                    .Where(x => x.OrderCount >= ctx.Filter.HasPlacedAtLeastOrders.Value)
                    .Select(x => x.Customer);
            }

            if (ctx.Request.EntitiesToExport.Any())
            {
                query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));
            }

            query = query.OrderBy(x => x.Id);

            if (skipValue > 0)
            {
                query = query.Skip(() => skipValue);
            }
            else if (ctx.LastId > 0)
            {
                query = query.Where(x => x.Id > ctx.LastId);
            }

            if (take != int.MaxValue)
            {
                query = query.Take(() => take);
            }

            return query;
        }

        private List<Customer> GetCustomers(DataExporterContext ctx)
        {
            DetachAllEntitiesAndClear(ctx);

            var stats = ctx.StatsPerStore[ctx.Store.Id];
            if (ctx.LastId >= stats.MaxId)
            {
                // End of data reached.
                return null;
            }

            var customers = GetCustomerQuery(ctx, null, PageSize).ToList();
            if (!customers.Any())
            {
                return null;
            }

            ctx.LastId = customers.Last().Id;
            SetProgress(ctx, customers.Count);

            return customers;
        }

        private IQueryable<NewsLetterSubscription> GetNewsLetterSubscriptionQuery(DataExporterContext ctx, int? skip, int take)
        {
            var storeId = ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId;

            var skipValue = skip.GetValueOrDefault();
            if (skipValue == 0 && ctx.LastId == 0)
            {
                skipValue = Math.Max(ctx.Request.Profile.Offset, 0);
            }

            var customerQuery = _customerRepository.Value.TableUntracked.Where(x => !x.Deleted);

            var query =
                from ns in _subscriptionRepository.Value.TableUntracked
                join c in customerQuery on ns.Email equals c.Email into customers
                from c in customers.DefaultIfEmpty()
                select new NewsletterSubscriber
                {
                    Subscription = ns,
                    Customer = c
                };

            if (storeId > 0)
            {
                query = query.Where(x => x.Subscription.StoreId == storeId);
            }

            if (ctx.Filter.IsActiveSubscriber.HasValue)
            {
                query = query.Where(x => x.Subscription.Active == ctx.Filter.IsActiveSubscriber.Value);
            }

            if (ctx.Filter.WorkingLanguageId != null && ctx.Filter.WorkingLanguageId != 0)
            {
                var defaultLanguage = _languageService.Value.GetAllLanguages().FirstOrDefault();
                var isDefaultLanguage = ctx.Filter.WorkingLanguageId == defaultLanguage.Id;

                if (isDefaultLanguage)
                {
                    query = query.Where(x => x.Subscription.WorkingLanguageId == 0 || x.Subscription.WorkingLanguageId == ctx.Filter.WorkingLanguageId);
                }
                else
                {
                    query = query.Where(x => x.Subscription.WorkingLanguageId == ctx.Filter.WorkingLanguageId);
                }
            }

            if (ctx.Filter.CreatedFrom.HasValue)
            {
                var createdFrom = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _services.DateTimeHelper.CurrentTimeZone);
                query = query.Where(x => createdFrom <= x.Subscription.CreatedOnUtc);
            }

            if (ctx.Filter.CreatedTo.HasValue)
            {
                var createdTo = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _services.DateTimeHelper.CurrentTimeZone);
                query = query.Where(x => createdTo >= x.Subscription.CreatedOnUtc);
            }

            if (ctx.Filter.CustomerRoleIds != null && ctx.Filter.CustomerRoleIds.Any())
            {
                query = query.Where(x => x.Customer.CustomerRoleMappings.Select(y => y.CustomerRoleId).Intersect(ctx.Filter.CustomerRoleIds).Any());
            }

            if (ctx.Request.EntitiesToExport.Any())
            {
                query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Subscription.Id));
            }

            query = query.OrderBy(x => x.Subscription.Id);

            if (skipValue > 0)
            {
                query = query.Skip(() => skipValue);
            }
            else if (ctx.LastId > 0)
            {
                query = query.Where(x => x.Subscription.Id > ctx.LastId);
            }

            if (take != int.MaxValue)
            {
                query = query.Take(() => take);
            }

            return query.Select(x => x.Subscription);
        }

        private List<NewsLetterSubscription> GetNewsLetterSubscriptions(DataExporterContext ctx)
        {
            DetachAllEntitiesAndClear(ctx);

            var stats = ctx.StatsPerStore[ctx.Store.Id];
            if (ctx.LastId >= stats.MaxId)
            {
                // End of data reached.
                return null;
            }

            var subscriptions = GetNewsLetterSubscriptionQuery(ctx, null, PageSize).ToList();
            if (!subscriptions.Any())
            {
                return null;
            }

            ctx.LastId = subscriptions.Last().Id;
            SetProgress(ctx, subscriptions.Count);

            return subscriptions;
        }

        private IQueryable<ShoppingCartItem> GetShoppingCartItemQuery(DataExporterContext ctx, int? skip, int take)
        {
            var storeId = ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId;

            var skipValue = skip.GetValueOrDefault();
            if (skipValue == 0 && ctx.LastId == 0)
            {
                skipValue = Math.Max(ctx.Request.Profile.Offset, 0);
            }

            var query = _shoppingCartItemRepository.Value.TableUntracked
                .Expand(x => x.Customer)
                .Expand(x => x.Customer.CustomerRoleMappings.Select(rm => rm.CustomerRole))
                .Expand(x => x.Product)
                .Where(x => !x.Customer.Deleted);

            if (storeId > 0)
            {
                query = query.Where(x => x.StoreId == storeId);
            }

            if (ctx.Request.ActionOrigin.IsCaseInsensitiveEqual("CurrentCarts"))
            {
                query = query.Where(x => x.ShoppingCartTypeId == (int)ShoppingCartType.ShoppingCart);
            }
            else if (ctx.Request.ActionOrigin.IsCaseInsensitiveEqual("CurrentWishlists"))
            {
                query = query.Where(x => x.ShoppingCartTypeId == (int)ShoppingCartType.Wishlist);
            }
            else if (ctx.Filter.ShoppingCartTypeId.HasValue)
            {
                query = query.Where(x => x.ShoppingCartTypeId == ctx.Filter.ShoppingCartTypeId.Value);
            }

            if (ctx.Filter.IsActiveCustomer.HasValue)
            {
                query = query.Where(x => x.Customer.Active == ctx.Filter.IsActiveCustomer.Value);
            }

            if (ctx.Filter.IsTaxExempt.HasValue)
            {
                query = query.Where(x => x.Customer.IsTaxExempt == ctx.Filter.IsTaxExempt.Value);
            }

            if (ctx.Filter.CustomerRoleIds != null && ctx.Filter.CustomerRoleIds.Any())
            {
                query = query.Where(x => x.Customer.CustomerRoleMappings.Select(y => y.CustomerRoleId).Intersect(ctx.Filter.CustomerRoleIds).Any());
            }

            if (ctx.Filter.LastActivityFrom.HasValue)
            {
                var activityFrom = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.LastActivityFrom.Value, _services.DateTimeHelper.CurrentTimeZone);
                query = query.Where(x => activityFrom <= x.Customer.LastActivityDateUtc);
            }

            if (ctx.Filter.LastActivityTo.HasValue)
            {
                var activityTo = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.LastActivityTo.Value, _services.DateTimeHelper.CurrentTimeZone);
                query = query.Where(x => activityTo >= x.Customer.LastActivityDateUtc);
            }

            if (ctx.Filter.CreatedFrom.HasValue)
            {
                var createdFrom = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _services.DateTimeHelper.CurrentTimeZone);
                query = query.Where(x => createdFrom <= x.CreatedOnUtc);
            }

            if (ctx.Filter.CreatedTo.HasValue)
            {
                var createdTo = _services.DateTimeHelper.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _services.DateTimeHelper.CurrentTimeZone);
                query = query.Where(x => createdTo >= x.CreatedOnUtc);
            }

            if (ctx.Projection.NoBundleProducts)
            {
                query = query.Where(x => x.Product.ProductTypeId != (int)ProductType.BundledProduct);
            }
            else
            {
                query = query.Where(x => x.BundleItemId == null);
            }

            if (ctx.Request.EntitiesToExport.Any())
            {
                query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));
            }

            query = query.OrderBy(x => x.Id);

            if (skipValue > 0)
            {
                query = query.Skip(() => skipValue);
            }
            else if (ctx.LastId > 0)
            {
                query = query.Where(x => x.Id > ctx.LastId);
            }

            if (take != int.MaxValue)
            {
                query = query.Take(take);
            }

            return query;
        }

        private List<ShoppingCartItem> GetShoppingCartItems(DataExporterContext ctx)
        {
            DetachAllEntitiesAndClear(ctx);

            var stats = ctx.StatsPerStore[ctx.Store.Id];
            if (ctx.LastId >= stats.MaxId)
            {
                // End of data reached.
                return null;
            }

            var cartItems = GetShoppingCartItemQuery(ctx, null, PageSize).ToList();
            if (!cartItems.Any())
            {
                return null;
            }

            ctx.LastId = cartItems.Last().Id;
            SetProgress(ctx, cartItems.Count);

            return cartItems;
        }

        #endregion

        private List<Store> Init(DataExporterContext ctx)
        {
            List<Store> result = null;

            ctx.ContextCurrency = _currencyService.Value.GetCurrencyById(ctx.Projection.CurrencyId ?? 0) ?? _services.WorkContext.WorkingCurrency;
            ctx.ContextCustomer = _customerService.GetCustomerById(ctx.Projection.CustomerId ?? 0) ?? _services.WorkContext.CurrentCustomer;
            ctx.ContextLanguage = _languageService.Value.GetLanguageById(ctx.Projection.LanguageId ?? 0) ?? _services.WorkContext.WorkingLanguage;

            ctx.Stores = _services.StoreService.GetAllStores().ToDictionary(x => x.Id, x => x);
            ctx.Languages = _languageService.Value.GetAllLanguages(true).ToDictionary(x => x.Id, x => x);

            if (ctx.IsPreview)
            {
                foreach (var name in new string[] { nameof(Currency), nameof(Country), nameof(StateProvince), nameof(DeliveryTime), nameof(QuantityUnit), nameof(Category), nameof(Manufacturer) })
                {
                    ctx.Translations[name] = new LocalizedPropertyCollection(name, null, Enumerable.Empty<LocalizedProperty>());
                }

                foreach (var name in new string[] { nameof(Product), nameof(ProductTag), nameof(ProductBundleItem), nameof(SpecificationAttribute), nameof(SpecificationAttributeOption),
                    nameof(ProductAttribute), nameof(ProductVariantAttributeValue) })
                {
                    ctx.TranslationsPerPage[name] = new LocalizedPropertyCollection(name, null, Enumerable.Empty<LocalizedProperty>());
                }

                ctx.UrlRecords[nameof(Category)] = new UrlRecordCollection(nameof(Category), null, Enumerable.Empty<UrlRecord>());
                ctx.UrlRecords[nameof(Manufacturer)] = new UrlRecordCollection(nameof(Manufacturer), null, Enumerable.Empty<UrlRecord>());
                ctx.UrlRecordsPerPage[nameof(Product)] = new UrlRecordCollection(nameof(Product), null, Enumerable.Empty<UrlRecord>());
            }
            else
            {
                // Get all translations and slugs for certain entities in one go.
                ctx.Translations[nameof(Currency)] = _localizedEntityService.Value.GetLocalizedPropertyCollection(nameof(Currency), null);
                ctx.Translations[nameof(Country)] = _localizedEntityService.Value.GetLocalizedPropertyCollection(nameof(Country), null);
                ctx.Translations[nameof(StateProvince)] = _localizedEntityService.Value.GetLocalizedPropertyCollection(nameof(StateProvince), null);
                ctx.Translations[nameof(DeliveryTime)] = _localizedEntityService.Value.GetLocalizedPropertyCollection(nameof(DeliveryTime), null);
                ctx.Translations[nameof(QuantityUnit)] = _localizedEntityService.Value.GetLocalizedPropertyCollection(nameof(QuantityUnit), null);
                ctx.Translations[nameof(Manufacturer)] = _localizedEntityService.Value.GetLocalizedPropertyCollection(nameof(Manufacturer), null);
                ctx.Translations[nameof(Category)] = _localizedEntityService.Value.GetLocalizedPropertyCollection(nameof(Category), null);

                ctx.UrlRecords[nameof(Category)] = _urlRecordService.Value.GetUrlRecordCollection(nameof(Category), null, null);
                ctx.UrlRecords[nameof(Manufacturer)] = _urlRecordService.Value.GetUrlRecordCollection(nameof(Manufacturer), null, null);
            }

            if (!ctx.IsPreview && ctx.Request.Profile.PerStore)
            {
                result = new List<Store>(ctx.Stores.Values.Where(x => x.Id == ctx.Filter.StoreId || ctx.Filter.StoreId == 0));
            }
            else
            {
                int? storeId = ctx.Filter.StoreId == 0 ? ctx.Projection.StoreId : ctx.Filter.StoreId;
                ctx.Store = ctx.Stores.Values.FirstOrDefault(x => x.Id == (storeId ?? _services.StoreContext.CurrentStore.Id));

                result = new List<Store> { ctx.Store };
            }

            // Get record stats for each store.
            foreach (var store in result)
            {
                ctx.Store = store;

                IQueryable<BaseEntity> query = null;

                switch (ctx.Request.Provider.Value.EntityType)
                {
                    case ExportEntityType.Product:
                        query = GetProductQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue);
                        break;
                    case ExportEntityType.Order:
                        query = GetOrderQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue);
                        break;
                    case ExportEntityType.Manufacturer:
                        query = GetManufacturerQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue);
                        break;
                    case ExportEntityType.Category:
                        query = GetCategoryQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue);
                        break;
                    case ExportEntityType.Customer:
                        query = GetCustomerQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue);
                        break;
                    case ExportEntityType.NewsLetterSubscription:
                        query = GetNewsLetterSubscriptionQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue);
                        break;
                    case ExportEntityType.ShoppingCartItem:
                        query = GetShoppingCartItemQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue);
                        break;
                }

                var stats = new RecordStats
                {
                    TotalRecords = query.Count()
                };

                if (!ctx.IsPreview)
                {
                    stats.MaxId = query.Max(x => (int?)x.Id) ?? 0;
                }

                ctx.StatsPerStore[store.Id] = stats;
            }

            return result;
        }

        private void ExportCoreInner(DataExporterContext ctx, Store store)
        {
            var context = ctx.ExecuteContext;
            var profile = ctx.Request.Profile;
            var provider = ctx.Request.Provider;

            if (context.Abort != DataExchangeAbortion.None)
            {
                return;
            }

            {
                var logHead = new StringBuilder();
                logHead.AppendLine();
                logHead.AppendLine(new string('-', 40));
                logHead.AppendLine("Smartstore: v." + SmartStoreVersion.CurrentFullVersion);
                logHead.Append("Export profile: " + profile.Name);
                logHead.AppendLine(profile.Id == 0 ? " (volatile)" : $" (Id {profile.Id})");

                if (provider.Metadata.FriendlyName.HasValue())
                {
                    logHead.AppendLine($"Export provider: {provider.Metadata.FriendlyName} ({profile.ProviderSystemName})");
                }
                else
                {
                    logHead.AppendLine("Export provider: " + profile.ProviderSystemName);
                }

                var plugin = provider.Metadata.PluginDescriptor;
                logHead.Append("Plugin: ");
                logHead.AppendLine(plugin == null ? "".NaIfEmpty() : $"{plugin.FriendlyName} ({plugin.SystemName}) v.{plugin.Version.ToString()}");
                logHead.AppendLine("Entity: " + provider.Value.EntityType.ToString());

                try
                {
                    var uri = new Uri(store.Url);
                    logHead.AppendLine($"Store: {uri.DnsSafeHost.NaIfEmpty()} (Id {store.Id})");
                }
                catch { }

                var customer = _services.WorkContext.CurrentCustomer;
                logHead.Append("Executed by: " + (customer.Email.HasValue() ? customer.Email : customer.SystemName));

                ctx.Log.Info(logHead.ToString());
            }

            var dataExchangeSettings = _services.Settings.LoadSetting<DataExchangeSettings>(store.Id);
            var publicDeployment = profile.Deployments.FirstOrDefault(x => x.DeploymentType == ExportDeploymentType.PublicFolder);

            ctx.Store = store;
            context.FileIndex = 0;
            context.Store = ToDynamic(ctx, ctx.Store);
            context.MaxFileNameLength = dataExchangeSettings.MaxFileNameLength;
            context.HasPublicDeployment = publicDeployment != null;
            context.PublicFolderPath = publicDeployment.GetDeploymentFolder(true);
            context.PublicFolderUrl = publicDeployment.GetPublicFolderUrl(_services, ctx.Store);

            var fileExtension = provider.Value.FileExtension.HasValue() ? provider.Value.FileExtension.ToLower().EnsureStartsWith(".") : "";

            using (var segmenter = CreateSegmenter(ctx))
            {
                if (segmenter == null)
                {
                    throw new SmartException($"Unsupported entity type '{provider.Value.EntityType.ToString()}'.");
                }

                if (segmenter.TotalRecords <= 0)
                {
                    ctx.Log.Info("There are no records to export.");
                }

                while (context.Abort == DataExchangeAbortion.None && segmenter.HasData)
                {
                    segmenter.RecordPerSegmentCount = 0;
                    context.RecordsSucceeded = 0;

                    string path = null;

                    if (ctx.IsFileBasedExport)
                    {
                        context.FileIndex = context.FileIndex + 1;
                        context.FileName = profile.ResolveFileNamePattern(ctx.Store, context.FileIndex, context.MaxFileNameLength) + fileExtension;
                        path = Path.Combine(context.Folder, context.FileName);

                        if (profile.ExportRelatedData && ctx.Supports(ExportFeatures.UsesRelatedDataUnits))
                        {
                            context.ExtraDataUnits.AddRange(GetDataUnitsForRelatedEntities(ctx));
                        }
                    }

                    if (CallProvider(ctx, "Execute", path))
                    {
                        if (ctx.IsFileBasedExport && File.Exists(path))
                        {
                            ctx.Result.Files.Add(new DataExportResult.ExportFileInfo
                            {
                                StoreId = ctx.Store.Id,
                                FileName = context.FileName
                            });
                        }
                    }

                    ctx.EntityIdsPerSegment.Clear();

                    if (context.IsMaxFailures)
                    {
                        ctx.Log.Warn("Export aborted. The maximum number of failures has been reached.");
                    }
                    if (ctx.CancellationToken.IsCancellationRequested)
                    {
                        ctx.Log.Warn("Export aborted. A cancellation has been requested.");
                    }
                }

                if (context.Abort != DataExchangeAbortion.Hard)
                {
                    var calledExecuted = false;

                    context.ExtraDataUnits.ForEach(x =>
                    {
                        context.DataStreamId = x.Id;

                        var success = true;
                        var path = x.FileName.HasValue() ? Path.Combine(context.Folder, x.FileName) : null;
                        if (!x.RelatedType.HasValue)
                        {
                            // Unit added by provider.
                            calledExecuted = true;
                            success = CallProvider(ctx, "OnExecuted", path);
                        }

                        if (success && ctx.IsFileBasedExport && x.DisplayInFileDialog && File.Exists(path))
                        {
                            // Save info about extra file.
                            ctx.Result.Files.Add(new DataExportResult.ExportFileInfo
                            {
                                StoreId = ctx.Store.Id,
                                FileName = x.FileName,
                                Label = x.Label,
                                RelatedType = x.RelatedType
                            });
                        }
                    });

                    if (!calledExecuted)
                    {
                        // Always call OnExecuted.
                        CallProvider(ctx, "OnExecuted", null);
                    }
                }

                context.ExtraDataUnits.Clear();
            }
        }

        private void ExportCoreOuter(DataExporterContext ctx)
        {
            var profile = ctx.Request.Profile;
            var logPath = profile.GetExportLogPath();
            var zipPath = profile.GetExportZipPath();

            FileSystemHelper.DeleteFile(logPath);
            FileSystemHelper.DeleteFile(zipPath);
            FileSystemHelper.ClearDirectory(ctx.FolderContent, false);

            using (var logger = new TraceLogger(logPath))
            {
                try
                {
                    ctx.Log = logger;
                    ctx.ExecuteContext.Log = logger;
                    ctx.ProgressInfo = T("Admin.DataExchange.Export.ProgressInfo");

                    if (!ctx.Request.Provider.IsValid())
                        throw new SmartException("Export aborted because the export provider is not valid.");

                    if (!HasPermission(ctx))
                        throw new SmartException("You do not have permission to perform the selected export.");

                    foreach (var item in ctx.Request.CustomData)
                    {
                        ctx.ExecuteContext.CustomProperties.Add(item.Key, item.Value);
                    }

                    if (profile.ProviderConfigData.HasValue())
                    {
                        var configInfo = ctx.Request.Provider.Value.ConfigurationInfo;
                        if (configInfo != null)
                        {
                            ctx.ExecuteContext.ConfigurationData = XmlHelper.Deserialize(profile.ProviderConfigData, configInfo.ModelType);
                        }
                    }

                    // lazyLoading: false, proxyCreation: false impossible due to price calculation.
                    using (var scope = new DbContextScope(_dbContext, autoDetectChanges: false, proxyCreation: true, validateOnSave: false, forceNoTracking: true))
                    {
                        ctx.DeliveryTimes = _deliveryTimeService.Value.GetAllDeliveryTimes().ToDictionary(x => x.Id);
                        ctx.QuantityUnits = _quantityUnitService.Value.GetAllQuantityUnits().ToDictionary(x => x.Id);
                        ctx.ProductTemplates = _productTemplateService.Value.GetAllProductTemplates().ToDictionary(x => x.Id, x => x.ViewPath);
                        ctx.CategoryTemplates = _categoryTemplateService.Value.GetAllCategoryTemplates().ToDictionary(x => x.Id, x => x.ViewPath);

                        if (ctx.Request.Provider.Value.EntityType == ExportEntityType.Product ||
                            ctx.Request.Provider.Value.EntityType == ExportEntityType.Order)
                        {
                            ctx.Countries = _countryService.Value.GetAllCountries(true).ToDictionary(x => x.Id, x => x);
                        }

                        if (ctx.Request.Provider.Value.EntityType == ExportEntityType.Customer)
                        {
                            var subscriptionEmails = _subscriptionRepository.Value.TableUntracked
                                .Where(x => x.Active)
                                .Select(x => x.Email)
                                .Distinct()
                                .ToList();

                            ctx.NewsletterSubscriptions = new HashSet<string>(subscriptionEmails, StringComparer.OrdinalIgnoreCase);
                        }

                        var stores = Init(ctx);

                        ctx.ExecuteContext.Language = ToDynamic(ctx, ctx.ContextLanguage);
                        ctx.ExecuteContext.Customer = ToDynamic(ctx, ctx.ContextCustomer);
                        ctx.ExecuteContext.Currency = ToDynamic(ctx, ctx.ContextCurrency);
                        ctx.ExecuteContext.Profile = ToDynamic(ctx, profile);

                        stores.ForEach(x => ExportCoreInner(ctx, x));
                    }

                    if (!ctx.IsPreview && ctx.ExecuteContext.Abort != DataExchangeAbortion.Hard)
                    {
                        if (ctx.IsFileBasedExport)
                        {
                            if (profile.CreateZipArchive)
                            {
                                ZipFile.CreateFromDirectory(ctx.FolderContent, zipPath, CompressionLevel.Fastest, false);
                            }

                            if (profile.Deployments.Any(x => x.Enabled))
                            {
                                SetProgress(ctx, T("Common.Publishing"));

                                var allDeploymentsSucceeded = Deploy(ctx, zipPath);

                                if (allDeploymentsSucceeded && profile.Cleanup)
                                {
                                    logger.Info("Cleaning up export folder");

                                    FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
                                }
                            }
                        }

                        if (profile.EmailAccountId != 0 && !ctx.Supports(ExportFeatures.CanOmitCompletionMail))
                        {
                            SendCompletionEmail(ctx, zipPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorsAll(ex);
                    ctx.Result.LastError = ex.ToAllMessages(true);
                }
                finally
                {
                    try
                    {
                        if (!ctx.IsPreview && profile.Id != 0)
                        {
                            ctx.Result.Files = ctx.Result.Files.OrderBy(x => x.RelatedType).ToList();
                            profile.ResultInfo = XmlHelper.Serialize(ctx.Result);

                            _exportProfileService.Value.UpdateExportProfile(profile);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorsAll(ex);
                    }

                    DetachAllEntitiesAndClear(ctx);

                    try
                    {
                        ctx.NewsletterSubscriptions.Clear();
                        ctx.ProductTemplates.Clear();
                        ctx.CategoryTemplates.Clear();
                        ctx.Countries.Clear();
                        ctx.Languages.Clear();
                        ctx.QuantityUnits.Clear();
                        ctx.DeliveryTimes.Clear();
                        ctx.Stores.Clear();
                        ctx.Translations.Clear();
                        ctx.UrlRecords.Clear();

                        ctx.TranslationsPerPage?.Clear();
                        ctx.UrlRecordsPerPage?.Clear();

                        ctx.Request.CustomData.Clear();

                        ctx.ExecuteContext.CustomProperties.Clear();
                        ctx.ExecuteContext.Log = null;
                        ctx.Log = null;
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorsAll(ex);
                    }
                }
            }

            if (ctx.IsPreview || ctx.ExecuteContext.Abort == DataExchangeAbortion.Hard)
            {
                return;
            }

            // Post process order entities.
            if (ctx.EntityIdsLoaded.Any() && ctx.Request.Provider.Value.EntityType == ExportEntityType.Order && ctx.Projection.OrderStatusChange != ExportOrderStatusChange.None)
            {
                using (var logger = new TraceLogger(logPath))
                {
                    try
                    {
                        int? orderStatusId = null;

                        if (ctx.Projection.OrderStatusChange == ExportOrderStatusChange.Processing)
                        {
                            orderStatusId = (int)OrderStatus.Processing;
                        }
                        else if (ctx.Projection.OrderStatusChange == ExportOrderStatusChange.Complete)
                        {
                            orderStatusId = (int)OrderStatus.Complete;
                        }

                        using (var scope = new DbContextScope(_dbContext, false, null, false, false, false, false))
                        {
                            foreach (var chunk in ctx.EntityIdsLoaded.Slice(128))
                            {
                                var entities = _orderRepository.Value.Table.Where(x => chunk.Contains(x.Id)).ToList();

                                entities.ForEach(x => x.OrderStatusId = (orderStatusId ?? x.OrderStatusId));

                                _dbContext.SaveChanges();
                            }
                        }

                        logger.Info($"Updated order status for {ctx.EntityIdsLoaded.Count()} order(s).");
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorsAll(ex);
                        ctx.Result.LastError = ex.ToAllMessages(true);
                    }
                }
            }
        }

        /// <summary>
        /// The name of the public export folder
        /// </summary>
        public static string PublicFolder => "Exchange";

        public static int PageSize => 100;

        public DataExportResult Export(DataExportRequest request, CancellationToken cancellationToken)
        {
            var ctx = new DataExporterContext(request, cancellationToken);

            if (!(request?.Profile?.Enabled ?? false))
            {
                return ctx.Result;
            }

            var lockKey = $"dataexporter:profile:{request.Profile.Id}";
            if (KeyedLock.IsLockHeld(lockKey))
            {
                ctx.Result.LastError = $"The execution of the profile \"{request.Profile.Name.NaIfEmpty()}\" (ID {request.Profile.Id}) is locked.";
                return ctx.Result;
            }

            using (KeyedLock.Lock(lockKey))
            {
                ExportCoreOuter(ctx);
            }

            cancellationToken.ThrowIfCancellationRequested();
            return ctx.Result;
        }

        public DataExportPreviewResult Preview(DataExportRequest request, int pageIndex)
        {
            var result = new DataExportPreviewResult();
            var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5.0));
            var ctx = new DataExporterContext(request, cancellation.Token, true);

            Init(ctx);

            var limit = Math.Max(request.Profile.Limit, 0);
            var take = limit > 0 && limit < PageSize ? limit : PageSize;
            var skip = Math.Max(ctx.Request.Profile.Offset, 0) + (pageIndex * take);

            if (!HasPermission(ctx))
            {
                throw new SmartException(T("Admin.AccessDenied"));
            }

            result.TotalRecords = ctx.StatsPerStore.First().Value.TotalRecords;

            switch (request.Provider.Value.EntityType)
            {
                case ExportEntityType.Product:
                    {
                        var items = GetProductQuery(ctx, skip, take).ToList();
                        items.Each(x => result.Data.Add(ToDynamic(ctx, x)));
                    }
                    break;
                case ExportEntityType.Order:
                    {
                        var items = GetOrderQuery(ctx, skip, take).ToList();
                        items.Each(x => result.Data.Add(ToDynamic(ctx, x)));
                    }
                    break;
                case ExportEntityType.Category:
                    {
                        var items = GetCategoryQuery(ctx, skip, take).ToList();
                        items.Each(x => result.Data.Add(ToDynamic(ctx, x)));
                    }
                    break;
                case ExportEntityType.Manufacturer:
                    {
                        var items = GetManufacturerQuery(ctx, skip, take).ToList();
                        items.Each(x => result.Data.Add(ToDynamic(ctx, x)));
                    }
                    break;
                case ExportEntityType.Customer:
                    {
                        var items = GetCustomerQuery(ctx, skip, take).ToList();
                        items.Each(x => result.Data.Add(ToDynamic(ctx, x)));
                    }
                    break;
                case ExportEntityType.NewsLetterSubscription:
                    {
                        var items = GetNewsLetterSubscriptionQuery(ctx, skip, take).ToList();
                        items.Each(x => result.Data.Add(ToDynamic(ctx, x)));
                    }
                    break;
                case ExportEntityType.ShoppingCartItem:
                    {
                        var items = GetShoppingCartItemQuery(ctx, skip, take).ToList();
                        items.Each(x => result.Data.Add(ToDynamic(ctx, x)));
                    }
                    break;
            }

            return result;
        }
    }
}
