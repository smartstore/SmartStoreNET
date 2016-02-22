using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Email;
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
using SmartStore.Services.Messages;
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
		private readonly IImportProfileService _importProfileService;

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
		private readonly Lazy<IProductTemplateService> _productTemplateService;
		private readonly Lazy<IProductService> _productService;
		private readonly Lazy<IUrlRecordService> _urlRecordService;
		private readonly Lazy<IStoreMappingService> _storeMappingService;
		private readonly Lazy<IGenericAttributeService> _genericAttributeService;
		private readonly Lazy<IAffiliateService> _affiliateService;
		private readonly Lazy<ICountryService> _countryService;
		private readonly Lazy<IStateProvinceService> _stateProvinceService;
		private readonly Lazy<IEmailAccountService> _emailAccountService;
		private readonly Lazy<IEmailSender> _emailSender;
		private readonly Lazy<FileDownloadManager> _fileDownloadManager;

		private readonly Lazy<SeoSettings> _seoSettings;
		private readonly Lazy<CustomerSettings> _customerSettings;
		private readonly Lazy<DateTimeSettings> _dateTimeSettings;
		private readonly Lazy<ForumSettings> _forumSettings;
		private readonly Lazy<ContactDataSettings> _contactDataSettings;
		private readonly Lazy<DataExchangeSettings> _dataExchangeSettings;

		public DataImporter(
			ICommonServices services,
			ICustomerService customerService,
			IImportProfileService importProfileService,
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
			Lazy<IProductTemplateService> productTemplateService,
			Lazy<IProductService> productService,
			Lazy<IUrlRecordService> urlRecordService,
			Lazy<IStoreMappingService> storeMappingService,
			Lazy<IGenericAttributeService> genericAttributeService,
			Lazy<IAffiliateService> affiliateService,
			Lazy<ICountryService> countryService,
			Lazy<IStateProvinceService> stateProvinceService,
			Lazy<IEmailAccountService> emailAccountService,
			Lazy<IEmailSender> emailSender,
			Lazy<FileDownloadManager> fileDownloadManager,
			Lazy<SeoSettings> seoSettings,
			Lazy<CustomerSettings> customerSettings,
			Lazy<DateTimeSettings> dateTimeSettings,
			Lazy<ForumSettings> forumSettings,
			Lazy<ContactDataSettings> contactDataSettings,
			Lazy<DataExchangeSettings> dataExchangeSettings)
		{
			_services = services;
			_customerService = customerService;
			_importProfileService = importProfileService;

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
			_productTemplateService = productTemplateService;
			_productService = productService;
			_urlRecordService = urlRecordService;
			_storeMappingService = storeMappingService;
			_genericAttributeService = genericAttributeService;
			_affiliateService = affiliateService;
			_countryService = countryService;
			_stateProvinceService = stateProvinceService;
			_emailAccountService = emailAccountService;
			_emailSender = emailSender;
			_fileDownloadManager = fileDownloadManager;

			_seoSettings = seoSettings;
			_customerSettings = customerSettings;
			_dateTimeSettings = dateTimeSettings;
			_forumSettings = forumSettings;
			_contactDataSettings = contactDataSettings;
			_dataExchangeSettings = dataExchangeSettings;

			T = NullLocalizer.Instance;
		}

		#endregion

		public Localizer T { get; set; }

		private bool HasPermission(DataImporterContext ctx)
		{
			if (ctx.Request.HasPermission)
				return true;

			var customer = _services.WorkContext.CurrentCustomer;

			if (customer.SystemName == SystemCustomerNames.BackgroundTask)
				return true;

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
			sb.AppendFormat("Started:\t\t{0}\r\n", result.StartDateUtc.ToLocalTime());
			sb.AppendFormat("Finished:\t\t{0}\r\n", result.EndDateUtc.ToLocalTime());
			sb.AppendFormat("Duration:\t\t{0}\r\n", (result.EndDateUtc - result.StartDateUtc).ToString("g"));
			sb.AppendLine();
			sb.AppendFormat("Total rows:\t\t{0}\r\n", result.TotalRecords);
			sb.AppendFormat("Rows processed:\t\t{0}\r\n", result.AffectedRecords);
			sb.AppendFormat("Records imported:\t{0}\r\n", result.NewRecords);
			sb.AppendFormat("Records updated:\t{0}\r\n", result.ModifiedRecords);
			sb.AppendLine();
			sb.AppendFormat("Warnings:\t\t{0}\r\n", result.Warnings);
			sb.AppendFormat("Errors:\t\t\t{0}", result.Errors);

			ctx.Log.Information(sb.ToString());

			foreach (var message in result.Messages)
			{
				if (message.MessageType == ImportMessageType.Error)
					ctx.Log.Error(message.ToString(), message.FullMessage);
				else if (message.MessageType == ImportMessageType.Warning)
					ctx.Log.Warning(message.ToString());
				else
					ctx.Log.Information(message.ToString());
			}
		}

		private void SendCompletionEmail(DataImporterContext ctx)
		{
			var emailAccount = _emailAccountService.Value.GetDefaultEmailAccount();
			var smtpContext = new SmtpContext(emailAccount);
			var message = new EmailMessage();

			var store = _services.StoreContext.CurrentStore;
			var storeInfo = "{0} ({1})".FormatInvariant(store.Name, store.Url);
			var intro = _services.Localization.GetResource("Admin.DataExchange.Import.CompletedEmail.Body").FormatInvariant(storeInfo);
			var body = new StringBuilder(intro);
			var result = ctx.ExecuteContext.Result;

			if (result.LastError.HasValue())
			{
				body.AppendFormat("<p style=\"color: #B94A48;\">{0}</p>", result.LastError);
			}

			body.Append("<p>");

			body.AppendFormat("<div>{0}: {1} &middot; {2}: {3}</div>",
				T("Admin.Common.TotalRows"), result.TotalRecords,
				T("Admin.Common.Skipped"), result.SkippedRecords);

			body.AppendFormat("<div>{0}: {1} &middot; {2}: {3}</div>",
				T("Admin.Common.NewRecords"), result.NewRecords,
				T("Admin.Common.Updated"), result.ModifiedRecords);

			body.AppendFormat("<div>{0}: {1} &middot; {2}: {3}</div>",
				T("Admin.Common.Errors"), result.Errors,
				T("Admin.Common.Warnings"), result.Warnings);

			body.Append("</p>");

			message.From = new EmailAddress(emailAccount.Email, emailAccount.DisplayName);

			if (_contactDataSettings.Value.WebmasterEmailAddress.HasValue())
				message.To.Add(new EmailAddress(_contactDataSettings.Value.WebmasterEmailAddress));

			if (message.To.Count == 0 && _contactDataSettings.Value.CompanyEmailAddress.HasValue())
				message.To.Add(new EmailAddress(_contactDataSettings.Value.CompanyEmailAddress));

			if (message.To.Count == 0)
				message.To.Add(new EmailAddress(emailAccount.Email, emailAccount.DisplayName));

			message.Subject = T("Admin.DataExchange.Import.CompletedEmail.Subject").Text.FormatInvariant(ctx.Request.Profile.Name);

			message.Body = body.ToString();

			_emailSender.Value.SendEmail(smtpContext, message);

			//Core.Infrastructure.EngineContext.Current.Resolve<IQueuedEmailService>().InsertQueuedEmail(new QueuedEmail
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
			//_services.DbContext.SaveChanges();
		}

		private void ImportCoreInner(DataImporterContext ctx, string filePath)
		{
			if (ctx.ExecuteContext.Abort == DataExchangeAbortion.Hard)
				return;

			{
				var logHead = new StringBuilder();
				logHead.AppendLine();
				logHead.AppendLine(new string('-', 40));
				logHead.AppendLine("SmartStore.NET:\t\tv." + SmartStoreVersion.CurrentFullVersion);
				logHead.Append("Import profile:\t\t" + ctx.Request.Profile.Name);
				logHead.AppendLine(ctx.Request.Profile.Id == 0 ? " (volatile)" : " (Id {0})".FormatInvariant(ctx.Request.Profile.Id));

				logHead.AppendLine("Entity:\t\t\t" + ctx.Request.Profile.EntityType.ToString());
				logHead.AppendLine("File:\t\t\t" + Path.GetFileName(filePath));

				var customer = _services.WorkContext.CurrentCustomer;
				logHead.Append("Executed by:\t\t" + (customer.Email.HasValue() ? customer.Email : customer.SystemName));

				ctx.Log.Information(logHead.ToString());
			}

			if (!File.Exists(filePath))
			{
				throw new SmartException("File does not exist {0}.".FormatInvariant(filePath));
			}

			CsvConfiguration csvConfiguration = null;
			var extension = Path.GetExtension(filePath);

			if (extension.IsCaseInsensitiveEqual(".csv"))
			{
				var converter = new CsvConfigurationConverter();
				csvConfiguration = converter.ConvertFrom<CsvConfiguration>(ctx.Request.Profile.FileTypeConfiguration);
			}

			if (csvConfiguration == null)
			{
				csvConfiguration = CsvConfiguration.ExcelFriendlyConfiguration;
			}

			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				ctx.ExecuteContext.DataTable = LightweightDataTable.FromFile(
					Path.GetFileName(filePath),
					stream,
					stream.Length,
					csvConfiguration,
					ctx.Request.Profile.Skip,
					ctx.Request.Profile.Take > 0 ? ctx.Request.Profile.Take : int.MaxValue
				);

				try
				{
					ctx.Importer.Execute(ctx.ExecuteContext);
				}
				catch (Exception exception)
				{
					ctx.ExecuteContext.Abort = DataExchangeAbortion.Hard;
					ctx.ExecuteContext.Result.AddError(exception, "The importer failed: {0}.".FormatInvariant(exception.ToAllMessages()));
				}

				if (ctx.ExecuteContext.IsMaxFailures)
					ctx.ExecuteContext.Result.AddWarning("Import aborted. The maximum number of failures has been reached.");

				if (ctx.CancellationToken.IsCancellationRequested)
					ctx.ExecuteContext.Result.AddWarning("Import aborted. A cancellation has been requested.");
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

					ctx.ExecuteContext.Log = logger;
					ctx.ExecuteContext.Languages = _languageService.Value.GetAllLanguages(true);
					ctx.ExecuteContext.UpdateOnly = ctx.Request.Profile.UpdateOnly;
					ctx.ExecuteContext.KeyFieldNames = ctx.Request.Profile.KeyFieldNames.SplitSafe(",");
					ctx.ExecuteContext.ImportFolder = ctx.Request.Profile.GetImportFolder();

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
							_localizedEntityService.Value,
							_pictureService.Value,
							_manufacturerService.Value,
							_categoryService.Value,
							_productService.Value,
							_urlRecordService.Value,
							_productTemplateService.Value,
							_storeMappingService.Value,
							_fileDownloadManager.Value,
							_seoSettings.Value,
							_dataExchangeSettings.Value);
					}
					else if (ctx.Request.Profile.EntityType == ImportEntityType.Customer)
					{
						ctx.Importer = new CustomerImporter(
							_customerRepository.Value,
							_pictureRepository.Value,
							_services,
							_genericAttributeService.Value,
							_customerService,
							_pictureService.Value,
							_affiliateService.Value,
							_countryService.Value,
							_stateProvinceService.Value,
							_fileDownloadManager.Value,
							_customerSettings.Value,
							_dateTimeSettings.Value,
							_forumSettings.Value,
							_dataExchangeSettings.Value);
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
							_localizedEntityService.Value,
							_fileDownloadManager.Value,
							_seoSettings.Value,
							_dataExchangeSettings.Value);
					}
					else
					{
						throw new SmartException("Unsupported entity type {0}.".FormatInvariant(ctx.Request.Profile.EntityType.ToString()));
					}

					files.ForEach(x => ImportCoreInner(ctx, x));
				}
				catch (Exception exception)
				{
					ctx.ExecuteContext.Result.AddError(exception);
				}
				finally
				{
					try
					{
						// database context sharing problem: if there are entities in modified state left by the provider due to SaveChanges failure,
						// then all subsequent SaveChanges would fail too (e.g. IImportProfileService.UpdateImportProfile, IScheduledTaskService.UpdateTask...).
						// so whatever it is, detach\dispose all that the tracker still has tracked.

						_services.DbContext.DetachAll(false);
					}
					catch (Exception exception)
					{
						ctx.ExecuteContext.Result.AddError(exception);
					}

					try
					{
						SendCompletionEmail(ctx);
					}
					catch (Exception exception)
					{
						ctx.ExecuteContext.Result.AddError(exception);
					}

					try
					{
						ctx.ExecuteContext.Result.EndDateUtc = DateTime.UtcNow;

						LogResult(ctx);
					}
					catch (Exception exception)
					{
						logger.ErrorsAll(exception);
					}

					try
					{
						ctx.Request.Profile.ResultInfo = XmlHelper.Serialize(ctx.ExecuteContext.Result.Clone());

						_importProfileService.UpdateImportProfile(ctx.Request.Profile);
					}
					catch (Exception exception)
					{
						logger.ErrorsAll(exception);
					}

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
