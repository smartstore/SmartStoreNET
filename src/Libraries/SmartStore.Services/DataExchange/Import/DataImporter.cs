using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Email;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Services.DataExchange.Import.Internal;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Utilities;
using SmartStore.Data.Caching;
using SmartStore.Services.Seo;
using System.Collections.Generic;
using SmartStore.Services.DataExchange.Import.Events;

namespace SmartStore.Services.DataExchange.Import
{
	public partial class DataImporter : IDataImporter
	{
		private readonly ICommonServices _services;
		private readonly IImportProfileService _importProfileService;
		private readonly ILanguageService _languageService;
		private readonly Func<ImportEntityType, IEntityImporter> _importerFactory;
		private readonly Lazy<IEmailAccountService> _emailAccountService;
		private readonly Lazy<IEmailSender> _emailSender;
		private readonly Lazy<ContactDataSettings> _contactDataSettings;
		private readonly Lazy<DataExchangeSettings> _dataExchangeSettings;
		private readonly IDbCache _dbCache;
		private readonly IUrlRecordService _urlRecordService;
		private readonly ILocalizedEntityService _localizedEntityService;

		public DataImporter(
			ICommonServices services,
			IImportProfileService importProfileService,
			ILanguageService languageService,
			Func<ImportEntityType, IEntityImporter> importerFactory,
			Lazy<IEmailAccountService> emailAccountService,
			Lazy<IEmailSender> emailSender,
			Lazy<ContactDataSettings> contactDataSettings,
			Lazy<DataExchangeSettings> dataExchangeSettings,
			IDbCache dbCache,
			IUrlRecordService urlRecordService,
			ILocalizedEntityService localizedEntityService)
		{
			_services = services;
			_importProfileService = importProfileService;
			_languageService = languageService;
			_importerFactory = importerFactory;
			_emailAccountService = emailAccountService;
			_emailSender = emailSender;
			_contactDataSettings = contactDataSettings;
			_dataExchangeSettings = dataExchangeSettings;
			_dbCache = dbCache;
			_urlRecordService = urlRecordService;
			_localizedEntityService = localizedEntityService;

			T = NullLocalizer.Instance;
		}

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

			ctx.Log.Info(sb.ToString());

			foreach (var message in result.Messages)
			{
				if (message.MessageType == ImportMessageType.Error)
					ctx.Log.Error(new Exception(message.FullMessage), message.ToString());
				else if (message.MessageType == ImportMessageType.Warning)
					ctx.Log.Warn(message.ToString());
				else
					ctx.Log.Info(message.ToString());
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

				ctx.Log.Info(logHead.ToString());
			}

			if (!File.Exists(filePath))
			{
				throw new SmartException("File does not exist {0}.".FormatInvariant(filePath));
			}

			CsvConfiguration csvConfiguration = null;
			var extension = Path.GetExtension(filePath);

			if ((new string[] { ".csv", ".txt", ".tab" }).Contains(extension, StringComparer.OrdinalIgnoreCase))
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
				var scopes = new List<IDisposable>();

				try
				{
					_dbCache.Enabled = false;
					scopes.Add(_localizedEntityService.BeginScope());
					scopes.Add(_urlRecordService.BeginScope());

					ctx.Log = logger;

					ctx.ExecuteContext.Request = ctx.Request;
					ctx.ExecuteContext.DataExchangeSettings = _dataExchangeSettings.Value;
					ctx.ExecuteContext.Services = _services;
					ctx.ExecuteContext.Log = logger;
					ctx.ExecuteContext.Languages = _languageService.GetAllLanguages(true);
					ctx.ExecuteContext.UpdateOnly = ctx.Request.Profile.UpdateOnly;
					ctx.ExecuteContext.KeyFieldNames = ctx.Request.Profile.KeyFieldNames.SplitSafe(",");
					ctx.ExecuteContext.ImportFolder = ctx.Request.Profile.GetImportFolder();
					ctx.ExecuteContext.ExtraData = XmlHelper.Deserialize<ImportExtraData>(ctx.Request.Profile.ExtraData);

					{
						var mapConverter = new ColumnMapConverter();
						ctx.ExecuteContext.ColumnMap = mapConverter.ConvertFrom<ColumnMap>(ctx.Request.Profile.ColumnMapping) ?? new ColumnMap();
					}

					var files = ctx.Request.Profile.GetImportFiles();

					if (files.Count == 0)
						throw new SmartException("No files to import.");

					if (!HasPermission(ctx))
						throw new SmartException("You do not have permission to perform the selected import.");

					ctx.Importer = _importerFactory(ctx.Request.Profile.EntityType);

					_services.EventPublisher.Publish(new ImportExecutingEvent(ctx.ExecuteContext));

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
						_dbCache.Enabled = true;
						scopes.Each(x => x.Dispose());

						_services.EventPublisher.Publish(new ImportExecutedEvent(ctx.ExecuteContext));
					}
					catch (Exception exception)
					{
						ctx.ExecuteContext.Result.AddError(exception);
					}

					try
					{
						// database context sharing problem: if there are entities in modified state left by the provider due to SaveChanges failure,
						// then all subsequent SaveChanges would fail too (e.g. IImportProfileService.UpdateImportProfile, IScheduledTaskService.UpdateTask...).
						// so whatever it is, detach\dispose all what the tracker still has tracked.

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
			Guard.NotNull(request, nameof(request));
			Guard.NotNull(cancellationToken, nameof(cancellationToken));

			var ctx = new DataImporterContext(request, cancellationToken, T("Admin.DataExchange.Import.ProgressInfo"));

			ImportCoreOuter(ctx);

			cancellationToken.ThrowIfCancellationRequested();
		}
	}
}
