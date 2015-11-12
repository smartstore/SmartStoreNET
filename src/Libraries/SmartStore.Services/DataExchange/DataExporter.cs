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
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Email;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.DataExchange.Internal;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Utilities;
using SmartStore.Utilities.Threading;

namespace SmartStore.Services.DataExchange
{
	public partial class DataExporter : IDataExporter
	{
		private static readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

		#region Dependencies

		private readonly ICommonServices _services;
		private readonly Lazy<IPriceFormatter> _priceFormatter;
		private readonly Lazy<IDateTimeHelper> _dateTimeHelper;
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
		private readonly Lazy<IProductTemplateService> _productTemplateService;
        private readonly Lazy<IProductService> _productService;
		private readonly Lazy<IOrderService> _orderService;
		private readonly Lazy<IManufacturerService> _manufacturerService;
		private readonly Lazy<ICustomerService> _customerService;
		private readonly Lazy<IAddressService> _addressService;
		private readonly Lazy<ICountryService> _countryService;
        private readonly Lazy<IShipmentService> _shipmentService;
		private readonly Lazy<IGenericAttributeService> _genericAttributeService;
		private readonly Lazy<IEmailAccountService> _emailAccountService;
		private readonly Lazy<IEmailSender> _emailSender;
		private readonly Lazy<IDeliveryTimeService> _deliveryTimeService;
		private readonly Lazy<IQuantityUnitService> _quantityUnitService;

		private readonly Lazy<IRepository<Customer>>_customerRepository;
		private readonly Lazy<IRepository<NewsLetterSubscription>> _subscriptionRepository;

		private Lazy<DataExchangeSettings> _dataExchangeSettings;
		private Lazy<MediaSettings> _mediaSettings;

		public DataExporter(
			ICommonServices services,
			Lazy<IPriceFormatter> priceFormatter,
			Lazy<IDateTimeHelper> dateTimeHelper,
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
			Lazy<IProductTemplateService> productTemplateService,
			Lazy<IProductService> productService,
			Lazy<IOrderService> orderService,
			Lazy<IManufacturerService> manufacturerService,
			Lazy<ICustomerService> customerService,
			Lazy<IAddressService> addressService,
			Lazy<ICountryService> countryService,
			Lazy<IShipmentService> shipmentService,
			Lazy<IGenericAttributeService> genericAttributeService,
			Lazy<IEmailAccountService> emailAccountService,
			Lazy<IEmailSender> emailSender,
			Lazy<IDeliveryTimeService> deliveryTimeService,
			Lazy<IQuantityUnitService> quantityUnitService,
			Lazy<IRepository<Customer>> customerRepository,
			Lazy<IRepository<NewsLetterSubscription>> subscriptionRepository,
			Lazy<DataExchangeSettings> dataExchangeSettings,
			Lazy<MediaSettings> mediaSettings)
		{
			_priceFormatter = priceFormatter;
			_dateTimeHelper = dateTimeHelper;
			_services = services;
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
			_productTemplateService = productTemplateService;
			_productService = productService;
			_orderService = orderService;
			_manufacturerService = manufacturerService;
			_customerService = customerService;
			_addressService = addressService;
			_countryService = countryService;
			_shipmentService = shipmentService;
			_genericAttributeService = genericAttributeService;
			_emailAccountService = emailAccountService;
			_emailSender = emailSender;
			_deliveryTimeService = deliveryTimeService;
			_quantityUnitService = quantityUnitService;

			_customerRepository = customerRepository;
			_subscriptionRepository = subscriptionRepository;

			_dataExchangeSettings = dataExchangeSettings;
			_mediaSettings = mediaSettings;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		#endregion

		#region Utilities

		private void SetProgress(DataExporterContext ctx, int loadedRecords)
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

		private void SetProgress(DataExporterContext ctx, string message)
		{
			if (!ctx.IsPreview && message.HasValue())
			{
				ctx.Request.ProgressMessageSetter.Invoke(message);
			}
		}

		private IExportDataSegmenterProvider CreateSegmenter(DataExporterContext ctx, int pageIndex = 0)
		{
			var offset = ctx.Request.Profile.Offset + (pageIndex * PageSize);

			var limit = (ctx.IsPreview ? PageSize : ctx.Request.Profile.Limit);

			var recordsPerSegment = (ctx.IsPreview ? 0 : ctx.Request.Profile.BatchSize);

			var totalCount = ctx.Request.Profile.Offset + ctx.RecordsPerStore.First(x => x.Key == ctx.Store.Id).Value;

			switch (ctx.Request.Provider.Value.EntityType)
			{
				case ExportEntityType.Product:
					ctx.ExecuteContext.Segmenter = new ExportDataSegmenter<Product>
					(
						skip => GetProducts(ctx, skip),
						entities =>
						{
							// load data behind navigation properties for current queue in one go
							ctx.ProductExportContext = new ProductExportContext(entities,
								x => _productAttributeService.Value.GetProductVariantAttributesByProductIds(x, null),
								x => _productAttributeService.Value.GetProductVariantAttributeCombinations(x),
								x => _productService.Value.GetTierPricesByProductIds(x, (ctx.Projection.CurrencyId ?? 0) != 0 ? ctx.ContextCustomer : null, ctx.Store.Id),
								x => _categoryService.Value.GetProductCategoriesByProductIds(x),
								x => _manufacturerService.Value.GetProductManufacturersByProductIds(x),
								x => _productService.Value.GetProductPicturesByProductIds(x),
								x => _productService.Value.GetProductTagsByProductIds(x),
								x => _productService.Value.GetAppliedDiscountsByProductIds(x),
								x => _productService.Value.GetProductSpecificationAttributesByProductIds(x),
								x => _productService.Value.GetBundleItemsByProductIds(x, true)
							);
						},
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				case ExportEntityType.Order:
					ctx.ExecuteContext.Segmenter = new ExportDataSegmenter<Order>
					(
						skip => GetOrders(ctx, skip),
						entities =>
						{
							ctx.OrderExportContext = new OrderExportContext(entities,
								x => _customerService.Value.GetCustomersByIds(x),
								x => _customerService.Value.GetRewardPointsHistoriesByCustomerIds(x),
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
					ctx.ExecuteContext.Segmenter = new ExportDataSegmenter<Manufacturer>
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
					ctx.ExecuteContext.Segmenter = new ExportDataSegmenter<Category>
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
					ctx.ExecuteContext.Segmenter = new ExportDataSegmenter<Customer>
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
					ctx.ExecuteContext.Segmenter = new ExportDataSegmenter<NewsLetterSubscription>
					(
						skip => GetNewsLetterSubscriptions(ctx, skip),
						null,
						entity => Convert(ctx, entity),
						offset, PageSize, limit, recordsPerSegment, totalCount
					);
					break;

				default:
					ctx.ExecuteContext.Segmenter = null;
					break;
			}

			return ctx.ExecuteContext.Segmenter as IExportDataSegmenterProvider;
		}

		private bool CallProvider(DataExporterContext ctx, string streamId, string method, string path)
		{
			if (method != "Execute" && method != "OnExecuted")
				throw new SmartException("Unknown export method {0}".FormatInvariant(method.NaIfEmpty()));

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

					if (ctx.IsFileBasedExport && path.HasValue())
					{
						if (!ctx.ExecuteContext.DataStream.CanSeek)
							ctx.Log.Warning("Data stream seems to be closed!");

						ctx.ExecuteContext.DataStream.Seek(0, SeekOrigin.Begin);

						using (_rwLock.GetWriteLock())
						using (var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
						{
							ctx.Log.Information("Creating file " + path);

							ctx.ExecuteContext.DataStream.CopyTo(fileStream);
						}
					}
				}
			}
			catch (Exception exc)
			{
				ctx.ExecuteContext.Abort = ExportAbortion.Hard;
				ctx.Log.Error("The provider failed at the {0} method: {1}".FormatInvariant(method, exc.ToAllMessages()), exc);
				ctx.Result.LastError = exc.ToString();
			}
			finally
			{
				if (ctx.ExecuteContext.DataStream != null)
				{
					ctx.ExecuteContext.DataStream.Dispose();
					ctx.ExecuteContext.DataStream = null;
				}
			}

			return (ctx.ExecuteContext.Abort != ExportAbortion.Hard);
		}

		private void SendCompletionEmail(DataExporterContext ctx)
		{
			var emailAccount = _emailAccountService.Value.GetEmailAccountById(ctx.Request.Profile.EmailAccountId);
			var smtpContext = new SmtpContext(emailAccount);
			var message = new EmailMessage();

			var storeInfo = "{0} ({1})".FormatInvariant(ctx.Store.Name, ctx.Store.Url);

			message.To.AddRange(ctx.Request.Profile.CompletedEmailAddresses.SplitSafe(",").Where(x => x.IsEmail()).Select(x => new EmailAddress(x)));
			message.From = new EmailAddress(emailAccount.Email, emailAccount.DisplayName);

			message.Subject = _services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Subject", ctx.Projection.LanguageId ?? 0)
				.FormatInvariant(ctx.Request.Profile.Name);

			message.Body = _services.Localization.GetResource("Admin.DataExchange.Export.CompletedEmail.Body", ctx.Projection.LanguageId ?? 0)
				.FormatInvariant(storeInfo);

			_emailSender.Value.SendEmail(smtpContext, message);
		}

		#endregion

		#region Getting data

		private IQueryable<Product> GetProductQuery(DataExporterContext ctx, int skip, int take)
		{
			IQueryable<Product> query = null;

			if (ctx.Request.ProductQuery == null)
			{
				var searchContext = new ProductSearchContext
				{
					OrderBy = ProductSortingEnum.CreatedOn,
					ProductIds = ctx.Request.EntitiesToExport,
					StoreId = (ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId),
					VisibleIndividuallyOnly = true,
					PriceMin = ctx.Filter.PriceMinimum,
					PriceMax = ctx.Filter.PriceMaximum,
					IsPublished = ctx.Filter.IsPublished,
					WithoutCategories = ctx.Filter.WithoutCategories,
					WithoutManufacturers = ctx.Filter.WithoutManufacturers,
					ManufacturerId = ctx.Filter.ManufacturerId ?? 0,
					FeaturedProducts = ctx.Filter.FeaturedProducts,
					ProductType = ctx.Filter.ProductType,
					ProductTagId = ctx.Filter.ProductTagId ?? 0,
					IdMin = ctx.Filter.IdMinimum ?? 0,
					IdMax = ctx.Filter.IdMaximum ?? 0,
					AvailabilityMinimum = ctx.Filter.AvailabilityMinimum,
					AvailabilityMaximum = ctx.Filter.AvailabilityMaximum
				};

				if (!ctx.Filter.IsPublished.HasValue)
					searchContext.ShowHidden = true;

				if (ctx.Filter.CategoryIds != null && ctx.Filter.CategoryIds.Length > 0)
					searchContext.CategoryIds = ctx.Filter.CategoryIds.ToList();

				if (ctx.Filter.CreatedFrom.HasValue)
					searchContext.CreatedFromUtc = _dateTimeHelper.Value.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _dateTimeHelper.Value.CurrentTimeZone);

				if (ctx.Filter.CreatedTo.HasValue)
					searchContext.CreatedToUtc = _dateTimeHelper.Value.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _dateTimeHelper.Value.CurrentTimeZone);

				query = _productService.Value.PrepareProductSearchQuery(searchContext);

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
			var result = new List<Product>();

			var products = GetProductQuery(ctx, skip, PageSize).ToList();

			foreach (var product in products)
			{
				if (product.ProductType == ProductType.SimpleProduct || product.ProductType == ProductType.BundledProduct)
				{
					result.Add(product);
				}
				else if (product.ProductType == ProductType.GroupedProduct)
				{
					if (ctx.Projection.NoGroupedProducts && !ctx.IsPreview)
					{
						var associatedSearchContext = new ProductSearchContext
						{
							OrderBy = ProductSortingEnum.CreatedOn,
							PageSize = int.MaxValue,
							StoreId = (ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId),
							VisibleIndividuallyOnly = false,
							ParentGroupedProductId = product.Id
						};

						foreach (var associatedProduct in _productService.Value.SearchProducts(associatedSearchContext))
						{
							result.Add(associatedProduct);
						}
					}
					else
					{
						result.Add(product);
					}
				}
			}

			try
			{
				SetProgress(ctx, products.Count);

				_services.DbContext.DetachEntities(result);
			}
			catch { }

			return result;
		}

		private IQueryable<Order> GetOrderQuery(DataExporterContext ctx, int skip, int take)
		{
			var query = _orderService.Value.GetOrders(
				ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId,
				ctx.Projection.CustomerId ?? 0,
				ctx.Filter.CreatedFrom.HasValue ? (DateTime?)_dateTimeHelper.Value.ConvertToUtcTime(ctx.Filter.CreatedFrom.Value, _dateTimeHelper.Value.CurrentTimeZone) : null,
				ctx.Filter.CreatedTo.HasValue ? (DateTime?)_dateTimeHelper.Value.ConvertToUtcTime(ctx.Filter.CreatedTo.Value, _dateTimeHelper.Value.CurrentTimeZone) : null,
				ctx.Filter.OrderStatusIds,
				ctx.Filter.PaymentStatusIds,
				ctx.Filter.ShippingStatusIds,
				null,
				null,
				null);

			if (ctx.Request.EntitiesToExport.Count > 0)
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
				ctx.EntityIdsLoaded = ctx.EntityIdsLoaded
					.Union(orders.Select(x => x.Id))
					.Distinct()
					.ToList();
			}

			try
			{
				SetProgress(ctx, orders.Count);

				_services.DbContext.DetachEntities<Order>(orders);
			}
			catch { }

			return orders;
		}

		private IQueryable<Manufacturer> GetManufacturerQuery(DataExporterContext ctx, int skip, int take)
		{
			var showHidden = !ctx.Filter.IsPublished.HasValue;
			var storeId = (ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId);

			var query = _manufacturerService.Value.GetManufacturers(showHidden, storeId);

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

			try
			{
				SetProgress(ctx, manus.Count);

				_services.DbContext.DetachEntities<Manufacturer>(manus);
			}
			catch { }

			return manus;
		}

		private IQueryable<Category> GetCategoryQuery(DataExporterContext ctx, int skip, int take)
		{
			var showHidden = !ctx.Filter.IsPublished.HasValue;
			var storeId = (ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId);

			var query = _categoryService.Value.GetCategories(null, showHidden, null, true, storeId);

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

			try
			{
				SetProgress(ctx, categories.Count);

				_services.DbContext.DetachEntities<Category>(categories);
			}
			catch { }

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

			if (ctx.Request.EntitiesToExport.Count > 0)
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

			try
			{
				SetProgress(ctx, customers.Count);

				_services.DbContext.DetachEntities<Customer>(customers);
			}
			catch { }

			return customers;
		}

		private IQueryable<NewsLetterSubscription> GetNewsLetterSubscriptionQuery(DataExporterContext ctx, int skip, int take)
		{
			var storeId = (ctx.Request.Profile.PerStore ? ctx.Store.Id : ctx.Filter.StoreId);

			var query = _subscriptionRepository.Value.TableUntracked;

			if (storeId > 0)
			{
				query = query.Where(x => x.StoreId == storeId);
			}

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

			try
			{
				SetProgress(ctx, subscriptions.Count);

				_services.DbContext.DetachEntities<NewsLetterSubscription>(subscriptions);
			}
			catch { }

			return subscriptions;
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
				ctx.ContextCustomer = _customerService.Value.GetCustomerById(ctx.Projection.CustomerId.Value);
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
					}
				}

				ctx.RecordsPerStore.Add(store.Id, totalCount);
			}

			return result;
		}

		private void ExportCoreInner(DataExporterContext ctx, Store store)
		{
			if (ctx.ExecuteContext.Abort != ExportAbortion.None)
				return;

			int fileIndex = 0;

			ctx.Store = store;

			{
				var logHead = new StringBuilder();
				logHead.AppendLine();
				logHead.AppendLine(new string('-', 40));
				logHead.AppendLine("SmartStore.NET:\t\tv." + SmartStoreVersion.CurrentFullVersion);
				logHead.Append("Export profile:\t\t" + ctx.Request.Profile.Name);
				logHead.AppendLine(ctx.Request.Profile.Id == 0 ? " (volatile)" : " (Id {0})".FormatInvariant(ctx.Request.Profile.Id));

				logHead.AppendLine("Export provider:\t{0} ({1})".FormatInvariant(ctx.Request.Provider.Metadata.FriendlyName, ctx.Request.Profile.ProviderSystemName));

				var plugin = ctx.Request.Provider.Metadata.PluginDescriptor;
				logHead.Append("Plugin:\t\t\t\t");
				logHead.AppendLine(plugin == null ? "".NaIfEmpty() : "{0} ({1}) v.{2}".FormatInvariant(plugin.FriendlyName, plugin.SystemName, plugin.Version.ToString()));

				logHead.AppendLine("Entity:\t\t\t\t" + ctx.Request.Provider.Value.EntityType.ToString());

				var storeInfo = (ctx.Request.Profile.PerStore ? "{0} (Id {1})".FormatInvariant(ctx.Store.Name, ctx.Store.Id) : "All stores");
				logHead.Append("Store:\t\t\t\t" + storeInfo);

				ctx.Log.Information(logHead.ToString());
			}

			ctx.ExecuteContext.Store = ToDynamic(ctx, ctx.Store);

			ctx.ExecuteContext.MaxFileNameLength = _dataExchangeSettings.Value.MaxFileNameLength;

			ctx.ExecuteContext.HasPublicDeployment = ctx.Request.Profile.Deployments.Any(x => x.IsPublic && x.DeploymentType == ExportDeploymentType.FileSystem);

			ctx.ExecuteContext.PublicFolderPath = (ctx.ExecuteContext.HasPublicDeployment ? Path.Combine(HttpRuntime.AppDomainAppPath, PublicFolder) : null);

			var fileExtension = (ctx.Request.Provider.Value.FileExtension.HasValue() ? ctx.Request.Provider.Value.FileExtension.ToLower().EnsureStartsWith(".") : "");


			using (var segmenter = CreateSegmenter(ctx))
			{
				if (segmenter == null)
				{
					throw new SmartException("Unsupported entity type '{0}'".FormatInvariant(ctx.Request.Provider.Value.EntityType.ToString()));
				}

				if (segmenter.TotalRecords <= 0)
				{
					ctx.Log.Information("There are no records to export");
				}

				while (ctx.ExecuteContext.Abort == ExportAbortion.None && segmenter.HasData)
				{
					segmenter.RecordPerSegmentCount = 0;
					ctx.ExecuteContext.RecordsSucceeded = 0;

					string path = null;

					if (ctx.IsFileBasedExport)
					{
						var resolvedPattern = ctx.Request.Profile.ResolveFileNamePattern(ctx.Store, ++fileIndex, ctx.ExecuteContext.MaxFileNameLength);

						ctx.ExecuteContext.FileName = resolvedPattern + fileExtension;
						path = Path.Combine(ctx.ExecuteContext.Folder, ctx.ExecuteContext.FileName);

						if (ctx.ExecuteContext.HasPublicDeployment)
							ctx.ExecuteContext.PublicFileUrl = ctx.Store.Url.EnsureEndsWith("/") + PublicFolder.EnsureEndsWith("/") + ctx.ExecuteContext.FileName;
					}

					if (CallProvider(ctx, null, "Execute", path))
					{
						ctx.Log.Information("Provider reports {0} successful exported record(s)".FormatInvariant(ctx.ExecuteContext.RecordsSucceeded));

						// create info for deployment list in profile edit
						if (ctx.IsFileBasedExport)
						{
							ctx.Result.Files.Add(new DataExportResult.ExportFileInfo
							{
								StoreId = ctx.Store.Id,
								FileName = ctx.ExecuteContext.FileName
							});
						}
					}

					if (ctx.ExecuteContext.IsMaxFailures)
						ctx.Log.Warning("Export aborted. The maximum number of failures has been reached");

					if (ctx.CancellationToken.IsCancellationRequested)
						ctx.Log.Warning("Export aborted. A cancellation has been requested");
				}

				if (ctx.ExecuteContext.Abort != ExportAbortion.Hard)
				{
					// always call OnExecuted
					if (ctx.ExecuteContext.ExtraDataStreams.Count == 0)
						ctx.ExecuteContext.ExtraDataStreams.Add(new ExportExtraStreams());

					ctx.ExecuteContext.ExtraDataStreams.ForEach(x =>
					{
						var path = (x.FileName.HasValue() ? Path.Combine(ctx.ExecuteContext.Folder, x.FileName) : null);

						CallProvider(ctx, x.Id, "OnExecuted", path);
					});

					ctx.ExecuteContext.ExtraDataStreams.Clear();
				}
			}
		}

		private void ExportCoreOuter(DataExporterContext ctx)
		{
			if (ctx.Request.Profile == null || !ctx.Request.Profile.Enabled)
				return;

			FileSystemHelper.Delete(ctx.LogPath);
			FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
			FileSystemHelper.Delete(ctx.ZipPath);

			using (var logger = new TraceLogger(ctx.LogPath))
			{
				try
				{
					if (!ctx.Request.Provider.IsValid())
					{
						throw new SmartException("Export aborted because the export provider is not valid");
					}

					foreach (var item in ctx.Request.CustomData)
					{
						ctx.ExecuteContext.CustomProperties.Add(item.Key, item.Value);
					}

					ctx.Log = logger;
					ctx.ExecuteContext.Log = logger;
					ctx.ProgressInfo = T("Admin.DataExchange.Export.ProgressInfo");

					if (ctx.Request.Profile.ProviderConfigData.HasValue())
					{
						var configInfo = ctx.Request.Provider.Value.ConfigurationInfo;
						if (configInfo != null)
						{
							ctx.ExecuteContext.ConfigurationData = XmlHelper.Deserialize(ctx.Request.Profile.ProviderConfigData, configInfo.ModelType);
						}
					}

					// TODO: lazyLoading: false, proxyCreation: false possible? how to identify all properties of all data levels of all entities
					// that require manual resolving for now and for future? fragile, susceptible to faults (e.g. price calculation)...
					using (var scope = new DbContextScope(_services.DbContext, autoDetectChanges: false, proxyCreation: true, validateOnSave: false, forceNoTracking: true))
					{
						ctx.DeliveryTimes = _deliveryTimeService.Value.GetAllDeliveryTimes().ToDictionary(x => x.Id);
						ctx.QuantityUnits = _quantityUnitService.Value.GetAllQuantityUnits().ToDictionary(x => x.Id);
						ctx.ProductTemplates = _productTemplateService.Value.GetAllProductTemplates().ToDictionary(x => x.Id);

						if (ctx.Request.Provider.Value.EntityType == ExportEntityType.Product)
						{
							var allCategories = _categoryService.Value.GetAllCategories(showHidden: true, applyNavigationFilters: false);
							ctx.Categories = allCategories.ToDictionary(x => x.Id);
						}

						if (ctx.Request.Provider.Value.EntityType == ExportEntityType.Order)
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

						stores.ForEach(x => ExportCoreInner(ctx, x));
					}

					if (!ctx.IsPreview && ctx.ExecuteContext.Abort != ExportAbortion.Hard)
					{
						if (ctx.IsFileBasedExport)
						{
							if (ctx.Request.Profile.CreateZipArchive || ctx.Request.Profile.Deployments.Any(x => x.Enabled && x.CreateZip))
							{
								ZipFile.CreateFromDirectory(ctx.FolderContent, ctx.ZipPath, CompressionLevel.Fastest, true);
							}

							SetProgress(ctx, T("Common.Deployment"));

							// TODO: deployment
							//foreach (var deployment in ctx.Request.Profile.Deployments.OrderBy(x => x.DeploymentTypeId).Where(x => x.Enabled))
							//{
							//	try
							//	{
							//		switch (deployment.DeploymentType)
							//		{
							//			case ExportDeploymentType.FileSystem:
							//				DeployFileSystem(ctx, deployment);
							//				break;
							//			case ExportDeploymentType.Email:
							//				DeployEmail(ctx, deployment);
							//				break;
							//			case ExportDeploymentType.Http:
							//				DeployHttp(ctx, deployment);
							//				break;
							//			case ExportDeploymentType.Ftp:
							//				DeployFtp(ctx, deployment);
							//				break;
							//		}
							//	}
							//	catch (Exception exc)
							//	{
							//		logger.Error("Deployment \"{0}\" of type {1} failed: {2}".FormatInvariant(
							//			deployment.Name, deployment.DeploymentType.ToString(), exc.Message), exc);
							//	}
							//}
						}

						if (ctx.Request.Profile.EmailAccountId != 0 && ctx.Request.Profile.CompletedEmailAddresses.HasValue())
						{
							SendCompletionEmail(ctx);
						}
					}
				}
				catch (Exception exc)
				{
					logger.Error(exc);
					ctx.Result.LastError = exc.ToString();
				}
				finally
				{
					try
					{
						if (!ctx.IsPreview && ctx.Request.Profile.Id != 0)
						{
							ctx.Request.Profile.ResultInfo = XmlHelper.Serialize<DataExportResult>(ctx.Result);

							_exportProfileService.Value.UpdateExportProfile(ctx.Request.Profile);
						}
					}
					catch { }

					try
					{
						if (ctx.IsFileBasedExport && ctx.ExecuteContext.Abort != ExportAbortion.Hard && ctx.Request.Profile.Cleanup)
						{
							FileSystemHelper.ClearDirectory(ctx.FolderContent, false);
						}
					}
					catch { }

					try
					{
						ctx.NewsletterSubscriptions.Clear();
						ctx.ProductTemplates.Clear();
						ctx.Countries.Clear();
						ctx.Stores.Clear();
						ctx.QuantityUnits.Clear();
						ctx.DeliveryTimes.Clear();
						ctx.CategoryPathes.Clear();
						ctx.Categories.Clear();
						ctx.ProductExportContext = null;
						ctx.OrderExportContext = null;
						ctx.ManufacturerExportContext = null;
						ctx.CategoryExportContext = null;
						ctx.CustomerExportContext = null;

						ctx.Request.EntitiesToExport.Clear();
						ctx.Request.CustomData.Clear();

						ctx.ExecuteContext.CustomProperties.Clear();
						ctx.ExecuteContext.Log = null;
						ctx.Log = null;
					}
					catch { }
				}
			}

			if (ctx.IsPreview || ctx.ExecuteContext.Abort == ExportAbortion.Hard)
				return;
		}

		/// <summary>
		/// The name of the public export folder
		/// </summary>
		public static string PublicFolder
		{
			get { return "Exchange"; }
		}

		public static int PageSize
		{
			get { return 100; }
		}

		public DataExportResult Export(DataExportRequest request, CancellationToken cancellationToken)
		{
			var ctx = new DataExporterContext(request, cancellationToken);

			ExportCoreOuter(ctx);

			if (ctx.Result != null && ctx.Result.Succeeded && ctx.Result.Files.Count > 0)
			{
				string prefix = null;
				string suffix = null;
				var extension = Path.GetExtension(ctx.Result.Files.First().FileName);

				if (request.Provider.Value.EntityType == ExportEntityType.Product)
					prefix = T("Admin.Catalog.Products");
				else if (request.Provider.Value.EntityType == ExportEntityType.Order)
					prefix = T("Admin.Orders");
				else if (request.Provider.Value.EntityType == ExportEntityType.Category)
					prefix = T("Admin.Catalog.Categories");
				else if (request.Provider.Value.EntityType == ExportEntityType.Manufacturer)
					prefix = T("Admin.Catalog.Manufacturers");
				else if (request.Provider.Value.EntityType == ExportEntityType.Customer)
					prefix = T("Admin.Customers");
				else if (request.Provider.Value.EntityType == ExportEntityType.NewsLetterSubscription)
					prefix = T("Admin.Promotions.NewsLetterSubscriptions");
				else
					prefix = request.Provider.Value.EntityType.ToString();

				var selectedEntityCount = (request.EntitiesToExport == null ? 0 : request.EntitiesToExport.Count);

				if (selectedEntityCount == 0)
					suffix = T("Common.All");
				else
					suffix = (selectedEntityCount == 1 ? request.EntitiesToExport.First().ToString() : T("Admin.Common.Selected").Text);

				ctx.Result.DownloadFileName = string.Concat(prefix, "-", suffix).ToLower().ToValidFileName() + extension;
			}

			return ctx.Result;
		}

		public IList<dynamic> Preview(DataExportRequest request, int pageIndex, int? totalRecords = null)
		{
			var resultData = new List<dynamic>();
			var cancellation = new CancellationTokenSource(TimeSpan.FromMinutes(5.0));

			var ctx = new DataExporterContext(request, cancellation.Token, true);

			var unused = Init(ctx, totalRecords);

			using (var segmenter = CreateSegmenter(ctx, pageIndex))
			{
				if (segmenter == null)
				{
					throw new SmartException("Unsupported entity type '{0}'".FormatInvariant(ctx.Request.Provider.Value.EntityType.ToString()));
				}

				while (segmenter.HasData)
				{
					segmenter.RecordPerSegmentCount = 0;

					while (segmenter.ReadNextSegment())
					{
						resultData.AddRange(segmenter.CurrentSegment);
					}
				}
			}

			if (ctx.Result.LastError.HasValue())
			{
				_services.Notifier.Error(ctx.Result.LastError);
			}

			return resultData;
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
