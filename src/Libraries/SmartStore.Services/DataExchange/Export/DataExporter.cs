using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Email;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
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
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;

namespace SmartStore.Services.DataExchange.Export
{
	public partial class DataExporter : IDataExporter
	{
		private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

		#region Dependencies

		private readonly ICommonServices _services;
		private readonly IDbContext _dbContext;
		private readonly HttpContextBase _httpContext;
		private readonly Lazy<IPriceFormatter> _priceFormatter;
		private readonly Lazy<IExportProfileService> _exportProfileService;
        private readonly Lazy<ILocalizedEntityService> _localizedEntityService;
		private readonly Lazy<ILanguageService> _languageService;
        private readonly Lazy<IUrlRecordService> _urlRecordService;
		private readonly Lazy<IPictureService> _pictureService;
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
		private readonly Lazy<ProductUrlHelper> _productUrlHelper;

		private readonly Lazy<IRepository<Customer>>_customerRepository;
		private readonly Lazy<IRepository<NewsLetterSubscription>> _subscriptionRepository;
		private readonly Lazy<IRepository<Order>> _orderRepository;
		private readonly Lazy<IRepository<ShoppingCartItem>> _shoppingCartItemRepository;

		private readonly Lazy<MediaSettings> _mediaSettings;
		private readonly Lazy<ContactDataSettings> _contactDataSettings;
		private readonly Lazy<CustomerSettings> _customerSettings;
		private readonly Lazy<CatalogSettings> _catalogSettings;
		private readonly Lazy<LocalizationSettings> _localizationSettings;
		private readonly Lazy<TaxSettings> _taxSettings;

		public DataExporter(
			ICommonServices services,
			IDbContext dbContext,
			HttpContextBase httpContext,
			Lazy<IPriceFormatter> priceFormatter,
			Lazy<IExportProfileService> exportProfileService,
			Lazy<ILocalizedEntityService> localizedEntityService,
			Lazy<ILanguageService> languageService,
			Lazy<IUrlRecordService> urlRecordService,
			Lazy<IPictureService> pictureService,
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
			Lazy<TaxSettings> taxSettings)
		{
			_services = services;
			_dbContext = dbContext;
			_httpContext = httpContext;
			_priceFormatter = priceFormatter;
			_exportProfileService = exportProfileService;
			_localizedEntityService = localizedEntityService;
			_languageService = languageService;
			_urlRecordService = urlRecordService;
			_pictureService = pictureService;
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

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		#endregion

		#region Utilities

		private void SetProgress(DataExporterContext ctx, int loadedRecords)
		{
			try
			{
				if (!ctx.IsPreview && loadedRecords > 0)
				{
					int totalRecords = ctx.RecordsPerStore.Sum(x => x.Value);

					if (ctx.Request.Profile.Limit > 0 && totalRecords > ctx.Request.Profile.Limit)
						totalRecords = ctx.Request.Profile.Limit;

					ctx.RecordCount = Math.Min(ctx.RecordCount + loadedRecords, totalRecords);
					var msg = ctx.ProgressInfo.FormatInvariant(ctx.RecordCount, totalRecords);
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

			switch (ctx.Request.Provider.Value.EntityType)
			{
				case ExportEntityType.Product:
				case ExportEntityType.Category:
				case ExportEntityType.Manufacturer:
					return _services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog, customer);

				case ExportEntityType.Customer:
					return _services.Permissions.Authorize(StandardPermissionProvider.ManageCustomers, customer);

				case ExportEntityType.Order:
				case ExportEntityType.ShoppingCartItem:
					return _services.Permissions.Authorize(StandardPermissionProvider.ManageOrders, customer);

				case ExportEntityType.NewsLetterSubscription:
					return _services.Permissions.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers, customer);
			}

			return true;
		}

		private void DetachAllEntitiesAndClear(DataExporterContext ctx)
		{
			try
			{
				if (ctx.ProductExportContext != null)
				{
					_dbContext.DetachEntities(x =>
					{
						return x is Product || x is Discount || x is ProductVariantAttributeCombination || x is ProductVariantAttribute || 
							   x is Picture || x is ProductBundleItem || x is ProductCategory || x is ProductManufacturer ||
							   x is ProductPicture || x is ProductTag || x is ProductSpecificationAttribute || x is TierPrice;
					});

					ctx.ProductExportContext.Clear();
				}

				if (ctx.OrderExportContext != null)
				{
					_dbContext.DetachEntities(x =>
					{
						return x is Order || x is Address || x is GenericAttribute || x is Customer ||
							   x is OrderItem || x is RewardPointsHistory || x is Shipment;
					});

					ctx.OrderExportContext.Clear();
				}

				if (ctx.CategoryExportContext != null)
				{
					_dbContext.DetachEntities(x =>
					{
						return x is Category || x is Picture || x is ProductCategory;
					});

					ctx.CategoryExportContext.Clear();
				}

				if (ctx.ManufacturerExportContext != null)
				{
					_dbContext.DetachEntities(x =>
					{
						return x is Manufacturer || x is Picture || x is ProductManufacturer;
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

				if (ctx.Request.Provider.Value.EntityType == ExportEntityType.ShoppingCartItem)
				{
					_dbContext.DetachEntities(x =>
					{
						return x is ShoppingCartItem || x is Customer || x is Product;
					});
				}
			}
			catch (Exception ex)
			{
				ctx.Log.Warn(ex, "Detaching entities failed.");
			}
		}

		private IExportDataSegmenterProvider CreateSegmenter(DataExporterContext ctx, int pageIndex = 0)
		{
			var offset = Math.Max(ctx.Request.Profile.Offset, 0) + (pageIndex * PageSize);
			var limit = Math.Max(ctx.Request.Profile.Limit, 0);
			var recordsPerSegment = (ctx.IsPreview ? 0 : Math.Max(ctx.Request.Profile.BatchSize, 0));
			var totalCount = Math.Max(ctx.Request.Profile.Offset, 0) + ctx.RecordsPerStore.First(x => x.Key == ctx.Store.Id).Value;
			
			switch (ctx.Request.Provider.Value.EntityType)
			{
				case ExportEntityType.Product:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Product>
					(
						skip => GetProducts(ctx, skip),
						entities =>
						{
							// load data behind navigation properties for current queue in one go
							ctx.ProductExportContext = CreateProductExportContext(entities, ctx.ContextCustomer, ctx.Store.Id);
						},
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Order:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Order>
					(
						skip => GetOrders(ctx, skip),
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
						},
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Manufacturer:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Manufacturer>
					(
						skip => GetManufacturers(ctx, skip),
						entities =>
						{
							ctx.ManufacturerExportContext = new ManufacturerExportContext(entities,
								x => _manufacturerService.Value.GetProductManufacturersByManufacturerIds(x),
								x => _pictureService.Value.GetPicturesByIds(x)
							);
						},
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Category:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Category>
					(
						skip => GetCategories(ctx, skip),
						entities =>
						{
							ctx.CategoryExportContext = new CategoryExportContext(entities,
								x => _categoryService.Value.GetProductCategoriesByCategoryIds(x),
								x => _pictureService.Value.GetPicturesByIds(x)
							);
						},
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Customer:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<Customer>
					(
						skip => GetCustomers(ctx, skip),
						entities =>
						{
							ctx.CustomerExportContext = new CustomerExportContext(entities,
								x => _genericAttributeService.Value.GetAttributesForEntity(x, "Customer")
							);
						},
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.NewsLetterSubscription:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<NewsLetterSubscription>
					(
						skip => GetNewsLetterSubscriptions(ctx, skip),
						null,
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.ShoppingCartItem:
					ctx.ExecuteContext.DataSegmenter = new ExportDataSegmenter<ShoppingCartItem>
					(
						skip => GetShoppingCartItems(ctx, skip),
						null,
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				default:
					ctx.ExecuteContext.DataSegmenter = null;
					break;
			}

			return ctx.ExecuteContext.DataSegmenter as IExportDataSegmenterProvider;
		}

		private bool CallProvider(DataExporterContext ctx, string streamId, string method, string path)
		{
			if (method != "Execute" && method != "OnExecuted")
			{
				throw new SmartException($"Unknown export method {method.NaIfEmpty()}.");
			}

			try
			{
				ctx.ExecuteContext.DataStreamId = streamId;

				using (ctx.ExecuteContext.DataStream = new MemoryStream())
				{
					if (method == "Execute")
					{
						ctx.Request.Provider.Value.Execute(ctx.ExecuteContext);
					}
					else if (method == "OnExecuted")
					{
						ctx.Request.Provider.Value.OnExecuted(ctx.ExecuteContext);
					}

					if (ctx.IsFileBasedExport && path.HasValue() && ctx.ExecuteContext.DataStream.Length > 0)
					{
						if (!ctx.ExecuteContext.DataStream.CanSeek)
						{
							ctx.Log.Warn("Data stream seems to be closed!");
						}

						ctx.ExecuteContext.DataStream.Seek(0, SeekOrigin.Begin);

						using (_rwLock.GetWriteLock())
						using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
						{
							ctx.Log.Info($"Creating file {path}.");
							ctx.ExecuteContext.DataStream.CopyTo(fileStream);
						}
					}
				}
			}
			catch (Exception exception)
			{
				ctx.ExecuteContext.Abort = DataExchangeAbortion.Hard;
				ctx.Log.ErrorFormat(exception, $"The provider failed at the {method.NaIfEmpty()} method.");
				ctx.Result.LastError = exception.ToString();
			}
			finally
			{
				if (ctx.ExecuteContext.DataStream != null)
				{
					ctx.ExecuteContext.DataStream.Dispose();
					ctx.ExecuteContext.DataStream = null;
				}

				if (ctx.ExecuteContext.Abort == DataExchangeAbortion.Hard && ctx.IsFileBasedExport && path.HasValue())
				{
					FileSystemHelper.Delete(path);
				}
			}

			return (ctx.ExecuteContext.Abort != DataExchangeAbortion.Hard);
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
				catch (Exception exception)
				{
					allSucceeded = false;

					if (context.Result != null)
					{
						context.Result.LastError = exception.ToAllMessages();
					}

					ctx.Log.ErrorFormat(exception, "Deployment \"{0}\" of type {1} failed", deployment.Name, deployment.DeploymentType.ToString());
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
			var intro =_services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Body", languageId).FormatInvariant(storeInfo);
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
			bool showHidden = true)
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
				x => _categoryService.Value.GetProductCategoriesByProductIds(x, null, showHidden),
				x => _manufacturerService.Value.GetProductManufacturersByProductIds(x),
				x => _productService.Value.GetAppliedDiscountsByProductIds(x),
				x => _productService.Value.GetBundleItemsByProductIds(x, showHidden),
				x => _pictureService.Value.GetPicturesByProductIds(x, maxPicturesPerProduct, true),
				x => _productService.Value.GetProductPicturesByProductIds(x),
				x => _productService.Value.GetProductTagsByProductIds(x)
			);

			return context;
		}

		private IQueryable<Product> GetProductQuery(DataExporterContext ctx, int skip, int take)
		{
			IQueryable<Product> query = null;

			if (ctx.Request.ProductQuery == null)
			{
				var f = ctx.Filter;
				var createdFrom = f.CreatedFrom.HasValue ? (DateTime?)_services.DateTimeHelper.ConvertToUtcTime(f.CreatedFrom.Value, _services.DateTimeHelper.CurrentTimeZone) : null;
				var createdTo = f.CreatedTo.HasValue ? (DateTime?)_services.DateTimeHelper.ConvertToUtcTime(f.CreatedTo.Value, _services.DateTimeHelper.CurrentTimeZone) : null;

				var searchQuery = new CatalogSearchQuery()
					.WithCurrency(ctx.ContextCurrency)
					.WithLanguage(ctx.ContextLanguage)
					.HasStoreId(ctx.Request.Profile.PerStore ? ctx.Store.Id : f.StoreId)
					.VisibleIndividuallyOnly(true)
					.PriceBetween(f.PriceMinimum, f.PriceMaximum)
					.WithStockQuantity(f.AvailabilityMinimum, f.AvailabilityMaximum)
					.CreatedBetween(createdFrom, createdTo);

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
				query = query.OrderByDescending(x => x.CreatedOnUtc);
			}
			else
			{
				query = ctx.Request.ProductQuery;
			}

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<Product> GetProducts(DataExporterContext ctx, int skip)
		{
			// we use ctx.EntityIdsPerSegment to avoid exporting products multiple times per segment\file (cause of associated products).

			var result = new List<Product>();

			var products = GetProductQuery(ctx, skip, PageSize).ToList();

			foreach (var product in products)
			{
				if (product.ProductType == ProductType.SimpleProduct || product.ProductType == ProductType.BundledProduct)
				{
					if (!ctx.EntityIdsPerSegment.Contains(product.Id))
					{
						result.Add(product);
						ctx.EntityIdsPerSegment.Add(product.Id);
					}
				}
				else if (product.ProductType == ProductType.GroupedProduct)
				{
					if (ctx.Projection.NoGroupedProducts)
					{
						var searchQuery = new CatalogSearchQuery()
							.HasParentGroupedProduct(product.Id)
							.HasStoreId(ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId);

						if (ctx.Projection.OnlyIndividuallyVisibleAssociated)
							searchQuery = searchQuery.VisibleIndividuallyOnly(true);

						if (ctx.Filter.IsPublished.HasValue)
							searchQuery = searchQuery.PublishedOnly(ctx.Filter.IsPublished.Value);

						var query = _catalogSearchService.Value.PrepareQuery(searchQuery);
						var associatedProducts = query.OrderBy(p => p.DisplayOrder).ToList();

						foreach (var associatedProduct in associatedProducts)
						{
							if (!ctx.EntityIdsPerSegment.Contains(associatedProduct.Id))
							{
								result.Add(associatedProduct);
								ctx.EntityIdsPerSegment.Add(associatedProduct.Id);
							}
						}
					}
					else
					{
						if (!ctx.EntityIdsPerSegment.Contains(product.Id))
						{
							result.Add(product);
							ctx.EntityIdsPerSegment.Add(product.Id);
						}
					}
				}
			}

			SetProgress(ctx, products.Count);

			return result;
		}

		private IQueryable<Order> GetOrderQuery(DataExporterContext ctx, int skip, int take)
		{
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
				query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));

			query = query.OrderByDescending(x => x.CreatedOnUtc);

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<Order> GetOrders(DataExporterContext ctx, int skip)
		{
			var orders = GetOrderQuery(ctx, skip, PageSize).ToList();

			if (ctx.Projection.OrderStatusChange != ExportOrderStatusChange.None)
			{
				ctx.SetLoadedEntityIds(orders.Select(x => x.Id));
			}

			SetProgress(ctx, orders.Count);

			return orders;
		}

		private IQueryable<Manufacturer> GetManufacturerQuery(DataExporterContext ctx, int skip, int take)
		{
			var storeId = ctx.Request.Profile.PerStore ? ctx.Store.Id : 0;
			var query = _manufacturerService.Value.GetManufacturers(true, storeId);

			if (ctx.Request.EntitiesToExport.Any())
				query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));

			query = query.OrderBy(x => x.DisplayOrder);

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<Manufacturer> GetManufacturers(DataExporterContext ctx, int skip)
		{
			var manus = GetManufacturerQuery(ctx, skip, PageSize).ToList();

			SetProgress(ctx, manus.Count);

			return manus;
		}

		private IQueryable<Category> GetCategoryQuery(DataExporterContext ctx, int skip, int take)
		{
			var storeId = ctx.Request.Profile.PerStore ? ctx.Store.Id : 0;
			var query = _categoryService.Value.BuildCategoriesQuery(null, true, null, storeId);

			if (ctx.Request.EntitiesToExport.Any())
				query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));

			query = query
				.OrderBy(x => x.ParentCategoryId)
				.ThenBy(x => x.DisplayOrder);

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<Category> GetCategories(DataExporterContext ctx, int skip)
		{
			var categories = GetCategoryQuery(ctx, skip, PageSize).ToList();

			SetProgress(ctx, categories.Count);

			return categories;
		}

		private IQueryable<Customer> GetCustomerQuery(DataExporterContext ctx, int skip, int take)
		{
			var query = _customerRepository.Value.TableUntracked
				.Expand(x => x.BillingAddress)
				.Expand(x => x.ShippingAddress)
				.Expand(x => x.Addresses.Select(y => y.Country))
				.Expand(x => x.Addresses.Select(y => y.StateProvince))
				.Expand(x => x.CustomerRoles)
				.Where(x => !x.Deleted);

			if (ctx.Filter.IsActiveCustomer.HasValue)
				query = query.Where(x => x.Active == ctx.Filter.IsActiveCustomer.Value);

			if (ctx.Filter.IsTaxExempt.HasValue)
				query = query.Where(x => x.IsTaxExempt == ctx.Filter.IsTaxExempt.Value);

			if (ctx.Filter.CustomerRoleIds != null && ctx.Filter.CustomerRoleIds.Length > 0)
				query = query.Where(x => x.CustomerRoles.Select(y => y.Id).Intersect(ctx.Filter.CustomerRoleIds).Any());

			if (ctx.Filter.BillingCountryIds != null && ctx.Filter.BillingCountryIds.Length > 0)
				query = query.Where(x => x.BillingAddress != null && ctx.Filter.BillingCountryIds.Contains(x.BillingAddress.Id));

			if (ctx.Filter.ShippingCountryIds != null && ctx.Filter.ShippingCountryIds.Length > 0)
				query = query.Where(x => x.ShippingAddress != null && ctx.Filter.ShippingCountryIds.Contains(x.ShippingAddress.Id));

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
				query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));

			query = query.OrderByDescending(x => x.CreatedOnUtc);

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<Customer> GetCustomers(DataExporterContext ctx, int skip)
		{
			var customers = GetCustomerQuery(ctx, skip, PageSize).ToList();

			SetProgress(ctx, customers.Count);

			return customers;
		}

		private IQueryable<NewsLetterSubscription> GetNewsLetterSubscriptionQuery(DataExporterContext ctx, int skip, int take)
		{
			var storeId = (ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId);

			var query = _subscriptionRepository.Value.TableUntracked;

			if (storeId > 0)
				query = query.Where(x => x.StoreId == storeId);

			if (ctx.Filter.IsActiveSubscriber.HasValue)
				query = query.Where(x => x.Active == ctx.Filter.IsActiveSubscriber.Value);

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

			if (ctx.Request.EntitiesToExport.Any())
				query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));

			query = query
				.OrderBy(x => x.StoreId)
				.ThenBy(x => x.Email);

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<NewsLetterSubscription> GetNewsLetterSubscriptions(DataExporterContext ctx, int skip)
		{
			var subscriptions = GetNewsLetterSubscriptionQuery(ctx, skip, PageSize).ToList();

			SetProgress(ctx, subscriptions.Count);

			return subscriptions;
		}

		private IQueryable<ShoppingCartItem> GetShoppingCartItemQuery(DataExporterContext ctx, int skip, int take)
		{
			var storeId = (ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId);

			var query = _shoppingCartItemRepository.Value.TableUntracked
				.Expand(x => x.Customer)
				.Expand(x => x.Customer.CustomerRoles)
				.Expand(x => x.Product)
				.Where(x => !x.Customer.Deleted);   //  && !x.Product.Deleted

			if (storeId > 0)
				query = query.Where(x => x.StoreId == storeId);

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
				query = query.Where(x => x.Customer.Active == ctx.Filter.IsActiveCustomer.Value);

			if (ctx.Filter.IsTaxExempt.HasValue)
				query = query.Where(x => x.Customer.IsTaxExempt == ctx.Filter.IsTaxExempt.Value);

			if (ctx.Filter.CustomerRoleIds != null && ctx.Filter.CustomerRoleIds.Length > 0)
				query = query.Where(x => x.Customer.CustomerRoles.Select(y => y.Id).Intersect(ctx.Filter.CustomerRoleIds).Any());

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
				query = query.Where(x => ctx.Request.EntitiesToExport.Contains(x.Id));

			query = query
				.OrderBy(x => x.ShoppingCartTypeId)
				.ThenBy(x => x.CustomerId)
				.ThenByDescending(x => x.CreatedOnUtc);

			if (skip > 0)
				query = query.Skip(skip);

			if (take != int.MaxValue)
				query = query.Take(take);

			return query;
		}

		private List<ShoppingCartItem> GetShoppingCartItems(DataExporterContext ctx, int skip)
		{
			var shoppingCartItems = GetShoppingCartItemQuery(ctx, skip, PageSize).ToList();

			SetProgress(ctx, shoppingCartItems.Count);

			return shoppingCartItems;
		}

		#endregion

		private List<Store> Init(DataExporterContext ctx, int? totalRecords = null)
		{
			// Init base things that are even required for preview. Init all other things (regular export) in ExportCoreOuter.
			List<Store> result = null;

			if (ctx.Projection.CurrencyId.HasValue)
				ctx.ContextCurrency = _currencyService.Value.GetCurrencyById(ctx.Projection.CurrencyId.Value);
			else
				ctx.ContextCurrency = _services.WorkContext.WorkingCurrency;

			if (ctx.Projection.CustomerId.HasValue)
				ctx.ContextCustomer = _customerService.GetCustomerById(ctx.Projection.CustomerId.Value);
			else
				ctx.ContextCustomer = _services.WorkContext.CurrentCustomer;

			if (ctx.Projection.LanguageId.HasValue)
				ctx.ContextLanguage = _languageService.Value.GetLanguageById(ctx.Projection.LanguageId.Value);
			else
				ctx.ContextLanguage = _services.WorkContext.WorkingLanguage;

			ctx.Stores = _services.StoreService.GetAllStores().ToDictionary(x => x.Id, x => x);
			ctx.Languages = _languageService.Value.GetAllLanguages(true).ToDictionary(x => x.Id, x => x);

			if (!ctx.IsPreview && ctx.Request.Profile.PerStore)
			{
				result = new List<Store>(ctx.Stores.Values.Where(x => x.Id == ctx.Filter.StoreId || ctx.Filter.StoreId == 0));
			}
			else
			{
				int? storeId = (ctx.Filter.StoreId == 0 ? ctx.Projection.StoreId : ctx.Filter.StoreId);

				ctx.Store = ctx.Stores.Values.FirstOrDefault(x => x.Id == (storeId ?? _services.StoreContext.CurrentStore.Id));

				result = new List<Store> { ctx.Store };
			}

			// get total records for progress
			foreach (var store in result)
			{
				ctx.Store = store;

				int totalCount = 0;

				if (totalRecords.HasValue)
				{
					totalCount = totalRecords.Value;    // speed up preview by not counting total at each page
				}
				else
				{
					switch (ctx.Request.Provider.Value.EntityType)
					{
						case ExportEntityType.Product:
							totalCount = GetProductQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.Order:
							totalCount = GetOrderQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.Manufacturer:
							totalCount = GetManufacturerQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.Category:
							totalCount = GetCategoryQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.Customer:
							totalCount = GetCustomerQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.NewsLetterSubscription:
							totalCount = GetNewsLetterSubscriptionQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
						case ExportEntityType.ShoppingCartItem:
							totalCount = GetShoppingCartItemQuery(ctx, ctx.Request.Profile.Offset, int.MaxValue).Count();
							break;
					}
				}

				ctx.RecordsPerStore.Add(store.Id, totalCount);
			}

			return result;
		}

		private void ExportCoreInner(DataExporterContext ctx, Store store)
		{
			if (ctx.ExecuteContext.Abort != DataExchangeAbortion.None)
				return;

			var fileIndex = 0;
			var dataExchangeSettings = _services.Settings.LoadSetting<DataExchangeSettings>(store.Id);

			ctx.Store = store;

			{
				var logHead = new StringBuilder();
				logHead.AppendLine();
				logHead.AppendLine(new string('-', 40));
				logHead.AppendLine("SmartStore.NET:\t\tv." + SmartStoreVersion.CurrentFullVersion);
				logHead.Append("Export profile:\t\t" + ctx.Request.Profile.Name);
				logHead.AppendLine(ctx.Request.Profile.Id == 0 ? " (volatile)" : " (Id {0})".FormatInvariant(ctx.Request.Profile.Id));

				if (ctx.Request.Provider.Metadata.FriendlyName.HasValue())
					logHead.AppendLine("Export provider:\t{0} ({1})".FormatInvariant(ctx.Request.Provider.Metadata.FriendlyName, ctx.Request.Profile.ProviderSystemName));
				else
					logHead.AppendLine("Export provider:\t{0}".FormatInvariant(ctx.Request.Profile.ProviderSystemName));

				var plugin = ctx.Request.Provider.Metadata.PluginDescriptor;
				logHead.Append("Plugin:\t\t\t");
				logHead.AppendLine(plugin == null ? "".NaIfEmpty() : "{0} ({1}) v.{2}".FormatInvariant(plugin.FriendlyName, plugin.SystemName, plugin.Version.ToString()));

				logHead.AppendLine("Entity:\t\t\t" + ctx.Request.Provider.Value.EntityType.ToString());

				try
				{
					var uri = new Uri(store.Url);
					logHead.AppendLine("Store:\t\t\t{0} (Id {1})".FormatInvariant(uri.DnsSafeHost.NaIfEmpty(), ctx.Store.Id));
				}
				catch {	}

				var customer = _services.WorkContext.CurrentCustomer;
				logHead.Append("Executed by:\t\t" + (customer.Email.HasValue() ? customer.Email : customer.SystemName));

				ctx.Log.Info(logHead.ToString());
			}

			ctx.ExecuteContext.Store = ToDynamic(ctx, ctx.Store);
			ctx.ExecuteContext.MaxFileNameLength = dataExchangeSettings.MaxFileNameLength;

			var publicDeployment = ctx.Request.Profile.Deployments.FirstOrDefault(x => x.DeploymentType == ExportDeploymentType.PublicFolder);
			ctx.ExecuteContext.HasPublicDeployment = (publicDeployment != null);
			ctx.ExecuteContext.PublicFolderPath = publicDeployment.GetDeploymentFolder(true);
			ctx.ExecuteContext.PublicFolderUrl = publicDeployment.GetPublicFolderUrl(_services, ctx.Store);

			var fileExtension = (ctx.Request.Provider.Value.FileExtension.HasValue() ? ctx.Request.Provider.Value.FileExtension.ToLower().EnsureStartsWith(".") : "");


			using (var segmenter = CreateSegmenter(ctx))
			{
				if (segmenter == null)
				{
					throw new SmartException("Unsupported entity type '{0}'.".FormatInvariant(ctx.Request.Provider.Value.EntityType.ToString()));
				}

				if (segmenter.TotalRecords <= 0)
				{
					ctx.Log.Info("There are no records to export.");
				}

				while (ctx.ExecuteContext.Abort == DataExchangeAbortion.None && segmenter.HasData)
				{
					segmenter.RecordPerSegmentCount = 0;
					ctx.ExecuteContext.RecordsSucceeded = 0;

					string path = null;

					if (ctx.IsFileBasedExport)
					{
						var resolvedPattern = ctx.Request.Profile.ResolveFileNamePattern(ctx.Store, ++fileIndex, ctx.ExecuteContext.MaxFileNameLength);

						ctx.ExecuteContext.FileName = resolvedPattern + fileExtension;
						path = Path.Combine(ctx.ExecuteContext.Folder, ctx.ExecuteContext.FileName);
					}

					if (CallProvider(ctx, null, "Execute", path))
					{
						ctx.Log.Info("Provider reports {0} successfully exported record(s).".FormatInvariant(ctx.ExecuteContext.RecordsSucceeded));

						if (ctx.IsFileBasedExport && File.Exists(path))
						{
							ctx.Result.Files.Add(new DataExportResult.ExportFileInfo
							{
								StoreId = ctx.Store.Id,
								FileName = ctx.ExecuteContext.FileName,
								IsDataFile = true
							});
						}
					}

					ctx.EntityIdsPerSegment.Clear();

					if (ctx.ExecuteContext.IsMaxFailures)
						ctx.Log.Warn("Export aborted. The maximum number of failures has been reached.");

					if (ctx.CancellationToken.IsCancellationRequested)
						ctx.Log.Warn("Export aborted. A cancellation has been requested.");

					DetachAllEntitiesAndClear(ctx);
				}

				if (ctx.ExecuteContext.Abort != DataExchangeAbortion.Hard)
				{
					// always call OnExecuted
					if (ctx.ExecuteContext.ExtraDataUnits.Count == 0)
						ctx.ExecuteContext.ExtraDataUnits.Add(new ExportDataUnit());

					ctx.ExecuteContext.ExtraDataUnits.ForEach(x =>
					{
						var path = (x.FileName.HasValue() ? Path.Combine(ctx.ExecuteContext.Folder, x.FileName) : null);
						if (CallProvider(ctx, x.Id, "OnExecuted", path))
						{
							if (x.DisplayInFileDialog && ctx.IsFileBasedExport && File.Exists(path))
							{
								// save info about extra file
								ctx.Result.Files.Add(new DataExportResult.ExportFileInfo
								{
									StoreId = ctx.Store.Id,
									FileName = x.FileName,
									Label = x.Label,
									IsDataFile = false
								});
							}
						}
					});

					ctx.ExecuteContext.ExtraDataUnits.Clear();
				}
			}
		}

		private void ExportCoreOuter(DataExporterContext ctx)
		{
			if (ctx.Request.Profile == null || !ctx.Request.Profile.Enabled)
				return;

			var logPath = ctx.Request.Profile.GetExportLogPath();
			var zipPath = ctx.Request.Profile.GetExportZipPath();

            FileSystemHelper.Delete(logPath);
			FileSystemHelper.Delete(zipPath);
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

					if (ctx.Request.Profile.ProviderConfigData.HasValue())
					{
						var configInfo = ctx.Request.Provider.Value.ConfigurationInfo;
						if (configInfo != null)
						{
							ctx.ExecuteContext.ConfigurationData = XmlHelper.Deserialize(ctx.Request.Profile.ProviderConfigData, configInfo.ModelType);
						}
					}

					// lazyLoading: false, proxyCreation: false impossible. how to identify all properties of all data levels of all entities
					// that require manual resolving for now and for future? fragile, susceptible to faults (e.g. price calculation)...
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
						ctx.ExecuteContext.Profile = ToDynamic(ctx, ctx.Request.Profile);

						stores.ForEach(x => ExportCoreInner(ctx, x));
					}

					if (!ctx.IsPreview && ctx.ExecuteContext.Abort != DataExchangeAbortion.Hard)
					{
						if (ctx.IsFileBasedExport)
						{
							if (ctx.Request.Profile.CreateZipArchive)
							{
								ZipFile.CreateFromDirectory(ctx.FolderContent, zipPath, CompressionLevel.Fastest, false);
							}

							if (ctx.Request.Profile.Deployments.Any(x => x.Enabled))
							{
								SetProgress(ctx, T("Common.Publishing"));

								var allDeploymentsSucceeded = Deploy(ctx, zipPath);

								if (allDeploymentsSucceeded && ctx.Request.Profile.Cleanup)
								{
									logger.Info("Cleaning up export folder");

									FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
								}
							}
						}

						if (ctx.Request.Profile.EmailAccountId != 0 && !ctx.Supports(ExportFeatures.CanOmitCompletionMail))
						{
							SendCompletionEmail(ctx, zipPath);
						}
					}
				}
				catch (Exception exception)
				{
					logger.ErrorsAll(exception);
					ctx.Result.LastError = exception.ToString();
				}
				finally
				{
					try
					{
						if (!ctx.IsPreview && ctx.Request.Profile.Id != 0)
						{
							ctx.Request.Profile.ResultInfo = XmlHelper.Serialize(ctx.Result);

							_exportProfileService.Value.UpdateExportProfile(ctx.Request.Profile);
						}
					}
					catch (Exception exception)
					{
						logger.ErrorsAll(exception);
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

						ctx.Request.CustomData.Clear();

						ctx.ExecuteContext.CustomProperties.Clear();
						ctx.ExecuteContext.Log = null;
						ctx.Log = null;
					}
					catch (Exception exception)
					{
						logger.ErrorsAll(exception);
					}
				}
			}

			if (ctx.IsPreview || ctx.ExecuteContext.Abort == DataExchangeAbortion.Hard)
				return;

			// Post process order entities.
			if (ctx.EntityIdsLoaded.Any() && ctx.Request.Provider.Value.EntityType == ExportEntityType.Order && ctx.Projection.OrderStatusChange != ExportOrderStatusChange.None)
			{
				using (var logger = new TraceLogger(logPath))
				{
					try
					{
						int? orderStatusId = null;

						if (ctx.Projection.OrderStatusChange == ExportOrderStatusChange.Processing)
							orderStatusId = (int)OrderStatus.Processing;
						else if (ctx.Projection.OrderStatusChange == ExportOrderStatusChange.Complete)
							orderStatusId = (int)OrderStatus.Complete;

						using (var scope = new DbContextScope(_dbContext, false, null, false, false, false, false))
						{
							foreach (var chunk in ctx.EntityIdsLoaded.Slice(128))
							{
								var entities = _orderRepository.Value.Table.Where(x => chunk.Contains(x.Id)).ToList();

								entities.ForEach(x => x.OrderStatusId = (orderStatusId ?? x.OrderStatusId));

								_dbContext.SaveChanges();
							}
						}

						logger.Info("Updated order status for {0} order(s).".FormatInvariant(ctx.EntityIdsLoaded.Count()));
					}
					catch (Exception exception)
					{
						logger.ErrorsAll(exception);
						ctx.Result.LastError = exception.ToString();
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

			ExportCoreOuter(ctx);

			cancellationToken.ThrowIfCancellationRequested();

			return ctx.Result;
		}

		public IList<dynamic> Preview(DataExportRequest request, int pageIndex, int? totalRecords = null)
		{
			var result = new List<dynamic>();
			var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5.0));
			var ctx = new DataExporterContext(request, cancellation.Token, true);

			var unused = Init(ctx, totalRecords);
			var offset = Math.Max(ctx.Request.Profile.Offset, 0) + (pageIndex * PageSize);

			if (!HasPermission(ctx))
			{
				throw new SmartException(T("Admin.AccessDenied"));
			}

			switch (request.Provider.Value.EntityType)
			{
				case ExportEntityType.Product:
					{
						var items = GetProductQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
				case ExportEntityType.Order:
					{
						var items = GetOrderQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
				case ExportEntityType.Category:
					{
						var items = GetCategoryQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
				case ExportEntityType.Manufacturer:
					{
						var items = GetManufacturerQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
				case ExportEntityType.Customer:
					{
						var items = GetCustomerQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
				case ExportEntityType.NewsLetterSubscription:
					{
						var items = GetNewsLetterSubscriptionQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
				case ExportEntityType.ShoppingCartItem:
					{
						var items = GetShoppingCartItemQuery(ctx, offset, PageSize).ToList();
						items.Each(x => result.Add(ToDynamic(ctx, x)));
					}
					break;
			}

			return result;
		}

		public int GetDataCount(DataExportRequest request)
		{
			var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5.0));
			var ctx = new DataExporterContext(request, cancellation.Token, true);
			var unused = Init(ctx);

			var totalCount = ctx.RecordsPerStore.First().Value;
			return totalCount;
		}
	}
}
