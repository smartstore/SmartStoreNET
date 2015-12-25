using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Importer;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Customers.Importer;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Services.DataExchange.Import.Internal;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages.Importer;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Import
{
	public partial class DataImporter : IDataImporter
	{
		#region Dependencies

		private readonly ICommonServices _services;
		private readonly ICustomerService _customerService;

		private readonly Lazy<IRepository<NewsLetterSubscription>> _subscriptionRepository;
		private readonly Lazy<IRepository<Picture>> _pictureRepository;
		private readonly Lazy<IRepository<ProductPicture>> _productPictureRepository;
		private readonly Lazy<IRepository<ProductManufacturer>> _productManufacturerRepository;
		private readonly Lazy<IRepository<ProductCategory>> _productCategoryRepository;
		private readonly Lazy<IRepository<UrlRecord>> _urlRecordRepository;
		private readonly Lazy<IRepository<Product>> _productRepository;
		private readonly Lazy<IRepository<Customer>> _customerRepository;
		private readonly Lazy<IRepository<Category>> _categoryRepository;

		private readonly Lazy<ILanguageService> _languageService;
		private readonly Lazy<ILocalizedEntityService> _localizedEntityService;
		private readonly Lazy<IPictureService> _pictureService;
		private readonly Lazy<IManufacturerService> _manufacturerService;
		private readonly Lazy<ICategoryService> _categoryService;
		private readonly Lazy<ICategoryTemplateService> _categoryTemplateService;
		private readonly Lazy<IProductService> _productService;
		private readonly Lazy<IUrlRecordService> _urlRecordService;
		private readonly Lazy<IStoreMappingService> _storeMappingService;
		private readonly Lazy<IGenericAttributeService> _genericAttributeService;
		private readonly Lazy<IAffiliateService> _affiliateService;
		private readonly Lazy<ICountryService> _countryService;
		private readonly Lazy<IStateProvinceService> _stateProvinceService;

		private readonly Lazy<SeoSettings> _seoSettings;
		private readonly Lazy<CustomerSettings> _customerSettings;
		private readonly Lazy<DateTimeSettings> _dateTimeSettings;
		private readonly Lazy<ForumSettings> _forumSettings;

		public DataImporter(
			ICommonServices services,
			ICustomerService customerService,
			Lazy<IRepository<NewsLetterSubscription>> subscriptionRepository,
			Lazy<IRepository<Picture>> pictureRepository,
			Lazy<IRepository<ProductPicture>> productPictureRepository,
			Lazy<IRepository<ProductManufacturer>> productManufacturerRepository,
			Lazy<IRepository<ProductCategory>> productCategoryRepository,
			Lazy<IRepository<UrlRecord>> urlRecordRepository,
			Lazy<IRepository<Product>> productRepository,
			Lazy<IRepository<Customer>> customerRepository,
			Lazy<IRepository<Category>> categoryRepository,
			Lazy<ILanguageService> languageService,
			Lazy<ILocalizedEntityService> localizedEntityService,
			Lazy<IPictureService> pictureService,
			Lazy<IManufacturerService> manufacturerService,
			Lazy<ICategoryService> categoryService,
			Lazy<ICategoryTemplateService> categoryTemplateService,
			Lazy<IProductService> productService,
			Lazy<IUrlRecordService> urlRecordService,
			Lazy<IStoreMappingService> storeMappingService,
			Lazy<IGenericAttributeService> genericAttributeService,
			Lazy<IAffiliateService> affiliateService,
			Lazy<ICountryService> countryService,
			Lazy<IStateProvinceService> stateProvinceService,
			Lazy<SeoSettings> seoSettings,
			Lazy<CustomerSettings> customerSettings,
			Lazy<DateTimeSettings> dateTimeSettings,
			Lazy<ForumSettings> forumSettings)
		{
			_services = services;
			_customerService = customerService;
			_subscriptionRepository = subscriptionRepository;
			_pictureRepository = pictureRepository;
			_productPictureRepository = productPictureRepository;
			_productManufacturerRepository = productManufacturerRepository;
			_productCategoryRepository = productCategoryRepository;
			_urlRecordRepository = urlRecordRepository;
			_productRepository = productRepository;
			_customerRepository = customerRepository;
			_categoryRepository = categoryRepository;

			_languageService = languageService;
			_localizedEntityService = localizedEntityService;
			_pictureService = pictureService;
			_manufacturerService = manufacturerService;
			_categoryService = categoryService;
			_categoryTemplateService = categoryTemplateService;
			_productService = productService;
			_urlRecordService = urlRecordService;
			_storeMappingService = storeMappingService;
			_genericAttributeService = genericAttributeService;
			_affiliateService = affiliateService;
			_countryService = countryService;
			_stateProvinceService = stateProvinceService;

			_seoSettings = seoSettings;
			_customerSettings = customerSettings;
			_dateTimeSettings = dateTimeSettings;
			_forumSettings = forumSettings;

			T = NullLocalizer.Instance;
		}

		#endregion

		public Localizer T { get; set; }

		private bool HasPermission(DataImporterContext ctx)
		{
			var customer = _customerService.GetCustomerById(ctx.Request.CustomerId);

			if (ctx.Request.Profile.EntityType == ImportEntityType.Product || ctx.Request.Profile.EntityType == ImportEntityType.Category)
				return _services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog, customer);

			if (ctx.Request.Profile.EntityType == ImportEntityType.Customer)
				return _services.Permissions.Authorize(StandardPermissionProvider.ManageCustomers, customer);

			if (ctx.Request.Profile.EntityType == ImportEntityType.NewsLetterSubscription)
				return _services.Permissions.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers, customer);

			return true;
		}

		private void LogResult(DataImporterContext ctx)
		{
			var result = ctx.ExecuteContext.Result;
			var sb = new StringBuilder();

			sb.AppendLine();
			sb.AppendFormat("Started:\t\t\t{0}\r\n", result.StartDateUtc.ToLocalTime());
			sb.AppendFormat("Finished:\t\t\t{0}\r\n", result.EndDateUtc.ToLocalTime());
			sb.AppendFormat("Duration:\t\t\t{0}\r\n", (result.EndDateUtc - result.StartDateUtc).ToString("g"));
			sb.AppendLine();
			sb.AppendFormat("Total rows:\t\t\t{0}\r\n", result.TotalRecords);
			sb.AppendFormat("Rows processed:\t\t{0}\r\n", result.AffectedRecords);
			sb.AppendFormat("Records imported:\t{0}\r\n", result.NewRecords);
			sb.AppendFormat("Records updated:\t{0}\r\n", result.ModifiedRecords);
			sb.AppendLine();
			sb.AppendFormat("Warnings:\t\t\t{0}\r\n", result.Messages.Count(x => x.MessageType == ImportMessageType.Warning));
			sb.AppendFormat("Errors:\t\t\t\t{0}", result.Messages.Count(x => x.MessageType == ImportMessageType.Error));

			ctx.Log.Information(sb.ToString());

			foreach (var message in result.Messages)
			{
				if (message.MessageType == ImportMessageType.Error)
					ctx.Log.Error(message.ToString());
				else if (message.MessageType == ImportMessageType.Warning)
					ctx.Log.Warning(message.ToString());
				else
					ctx.Log.Information(message.ToString());
			}
		}

		private void ImportCoreInner(DataImporterContext ctx, string filePath)
		{
			ctx.ExecuteContext.Result.Clear();

			{
				var logHead = new StringBuilder();
				logHead.AppendLine();
				logHead.AppendLine(new string('-', 40));
				logHead.AppendLine("SmartStore.NET:\t\tv." + SmartStoreVersion.CurrentFullVersion);
				logHead.Append("Import profile:\t\t" + ctx.Request.Profile.Name);
				logHead.AppendLine(ctx.Request.Profile.Id == 0 ? " (volatile)" : " (Id {0})".FormatInvariant(ctx.Request.Profile.Id));

				logHead.AppendLine("Entity:\t\t\t\t" + ctx.Request.Profile.EntityType.ToString());
				logHead.Append("File:\t\t\t\t" + Path.GetFileName(filePath));

				ctx.Log.Information(logHead.ToString());
			}

			if (!File.Exists(filePath))
			{
				throw new SmartException("File does not exist {0}.".FormatInvariant(filePath));
			}

			CsvConfiguration csvConfiguration = null;
			var extension = Path.GetExtension(filePath);
			var take = (ctx.Request.Profile.Take > 0 ? ctx.Request.Profile.Take : int.MaxValue);

			if (extension.IsCaseInsensitiveEqual(".csv"))
			{
				var converter = new CsvConfigurationConverter();
				csvConfiguration = converter.ConvertFrom<CsvConfiguration>(ctx.Request.Profile.FileTypeConfiguration);
			}

			if (csvConfiguration == null)
			{
				csvConfiguration = CsvConfiguration.ExcelFriendlyConfiguration;
				ctx.Log.Warning("No CSV configuration provided for import profile. Fallback to Excel friendly configuration.");
			}

			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				ctx.ExecuteContext.DataTable = LightweightDataTable.FromFile(Path.GetFileName(filePath), stream, stream.Length, csvConfiguration, ctx.Request.Profile.Skip, take);

				try
				{
					ctx.Importer.Execute(ctx.ExecuteContext);
				}
				catch (Exception exception)
				{
					ctx.ExecuteContext.Abort = DataExchangeAbortion.Hard;
					ctx.Log.Error("The importer failed: {0}.".FormatInvariant(exception.ToAllMessages()), exception);
				}

				ctx.ExecuteContext.Result.EndDateUtc = DateTime.UtcNow;

				if (ctx.ExecuteContext.IsMaxFailures)
					ctx.Log.Warning("Import aborted. The maximum number of failures has been reached.");

				if (ctx.CancellationToken.IsCancellationRequested)
					ctx.Log.Warning("Import aborted. A cancellation has been requested.");

				LogResult(ctx);
			}
		}

		private void ImportCoreOuter(DataImporterContext ctx)
		{
			if (ctx.Request.Profile == null || !ctx.Request.Profile.Enabled)
				return;

			var logPath = ctx.Request.Profile.GetImportLogPath();

			FileSystemHelper.Delete(logPath);

			using (var logger = new TraceLogger(logPath))
			{
				try
				{
					ctx.Log = logger;

					if (ctx.Request.CustomerId == 0)
						ctx.Request.CustomerId = _services.WorkContext.CurrentCustomer.Id;  // fallback to system background task customer

					ctx.ExecuteContext.CustomerId = ctx.Request.CustomerId;

					{
						var mapConverter = new ColumnMapConverter();
						ctx.ExecuteContext.ColumnMap = mapConverter.ConvertFrom<ColumnMap>(ctx.Request.Profile.ColumnMapping) ?? new ColumnMap();
					}

					var files = ctx.Request.Profile.GetImportFiles();

					if (files.Count == 0)
						throw new SmartException("No files to import found.");

					if (!HasPermission(ctx))
						throw new SmartException("You do not have permission to perform the selected import.");

					if (ctx.Request.Profile.EntityType == ImportEntityType.Product)
					{
						ctx.Importer = new ProductImporter(
							_productPictureRepository.Value,
							_productManufacturerRepository.Value,
							_productCategoryRepository.Value,
							_urlRecordRepository.Value,
							_productRepository.Value,
							_services,
							_languageService.Value,
							_localizedEntityService.Value,
							_pictureService.Value,
							_manufacturerService.Value,
							_categoryService.Value,
							_productService.Value,
							_urlRecordService.Value,
							_storeMappingService.Value,
							_seoSettings.Value);
					}
					else if (ctx.Request.Profile.EntityType == ImportEntityType.Customer)
					{
						ctx.Importer = new CustomerImporter(
							_services,
							_customerRepository.Value,
							_genericAttributeService.Value,
							_customerService,
							_affiliateService.Value,
							_countryService.Value,
							_stateProvinceService.Value,
							_customerSettings.Value,
							_dateTimeSettings.Value,
							_forumSettings.Value);
					}
					else if (ctx.Request.Profile.EntityType == ImportEntityType.NewsLetterSubscription)
					{
						ctx.Importer = new NewsLetterSubscriptionImporter(
							_services,
							_subscriptionRepository.Value);
					}
					else if (ctx.Request.Profile.EntityType == ImportEntityType.Category)
					{
						ctx.Importer = new CategoryImporter(
							_categoryRepository.Value,
							_urlRecordRepository.Value,
							_pictureRepository.Value,
							_services,
							_urlRecordService.Value,
							_categoryTemplateService.Value,
							_storeMappingService.Value,
							_pictureService.Value,
							_seoSettings.Value);
					}
					else
					{
						throw new SmartException("Unsupported entity type {0}.".FormatInvariant(ctx.Request.Profile.EntityType.ToString()));
					}

					files.ForEach(x => ImportCoreInner(ctx, x));
				}
				catch (Exception exception)
				{
					logger.ErrorsAll(exception);
				}
				finally
				{
					try
					{
						ctx.Request.CustomData.Clear();

						ctx.Log = null;
					}
					catch (Exception exception)
					{
						logger.ErrorsAll(exception);
					}
				}
			}
		}

		public void Import(DataImportRequest request, CancellationToken cancellationToken)
		{
			Guard.ArgumentNotNull(() => request);
			Guard.ArgumentNotNull(() => cancellationToken);

			var ctx = new DataImporterContext(request, cancellationToken, T("Admin.DataExchange.Import.ProgressInfo"));

			ImportCoreOuter(ctx);

			cancellationToken.ThrowIfCancellationRequested();
		}
	}
}
