using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Email;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Data.Caching;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Services.DataExchange.Import.Events;
using SmartStore.Services.DataExchange.Import.Internal;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Seo;
using SmartStore.Utilities;

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
            {
                return true;
            }

            var customer = _services.WorkContext.CurrentCustomer;

            if (customer.SystemName == SystemCustomerNames.BackgroundTask)
            {
                return true;
            }

            return _services.Permissions.Authorize(Permissions.Configuration.Import.Execute);
        }

        private void LogResults(DataImporterContext ctx)
        {
            var sb = new StringBuilder();

            foreach (var item in ctx.Results)
            {
                var result = item.Value;
                var entityName = item.Key.HasValue() ? item.Key : ctx.Request.Profile.EntityType.ToString();

                sb.Clear();
                sb.AppendLine();
                sb.AppendLine(new string('-', 40));
                sb.AppendLine("Object:         " + entityName);
                sb.AppendLine("Started:        " + result.StartDateUtc.ToLocalTime());
                sb.AppendLine("Finished:       " + result.EndDateUtc.ToLocalTime());
                sb.AppendLine("Duration:       " + (result.EndDateUtc - result.StartDateUtc).ToString("g"));
                sb.AppendLine("Rows total:     " + result.TotalRecords);
                sb.AppendLine("Rows processed: " + result.AffectedRecords);
                sb.AppendLine("Rows imported:  " + result.NewRecords);
                sb.AppendLine("Rows updated:   " + result.ModifiedRecords);
                sb.AppendLine("Warnings:       " + result.Warnings);
                sb.Append("Errors:         " + result.Errors);
                ctx.Log.Info(sb.ToString());

                foreach (var message in result.Messages)
                {
                    if (message.MessageType == ImportMessageType.Error)
                    {
                        ctx.Log.Error(new Exception(message.FullMessage), message.ToString());
                    }
                    else if (message.MessageType == ImportMessageType.Warning)
                    {
                        ctx.Log.Warn(message.ToString());
                    }
                    else
                    {
                        ctx.Log.Info(message.ToString());
                    }
                }
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

        private void ImportCoreInner(DataImporterContext ctx, ImportFile file)
        {
            var context = ctx.ExecuteContext;
            var profile = ctx.Request.Profile;

            if (context.Abort == DataExchangeAbortion.Hard)
            {
                return;
            }

            if (!File.Exists(file.Path))
            {
                throw new SmartException($"File does not exist {file.Path}.");
            }

            using (var stream = new FileStream(file.Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var csvConfiguration = file.IsCsv
                    ? (new CsvConfigurationConverter().ConvertFrom<CsvConfiguration>(profile.FileTypeConfiguration) ?? CsvConfiguration.ExcelFriendlyConfiguration)
                    : CsvConfiguration.ExcelFriendlyConfiguration;

                context.DataTable = LightweightDataTable.FromFile(
                    file.Name,
                    stream,
                    stream.Length,
                    csvConfiguration,
                    profile.Skip,
                    profile.Take > 0 ? profile.Take : int.MaxValue);

                context.ColumnMap = file.RelatedType.HasValue ? new ColumnMap() : ctx.ColumnMap;
                context.File = file;

                try
                {
                    ctx.Importer.Execute(context);
                }
                catch (Exception ex)
                {
                    context.Abort = DataExchangeAbortion.Hard;
                    context.Result.AddError(ex, $"The importer failed: {ex.ToAllMessages()}.");
                }
                finally
                {
                    context.Result.EndDateUtc = DateTime.UtcNow;

                    if (context.IsMaxFailures)
                    {
                        context.Result.AddWarning("Import aborted. The maximum number of failures has been reached.");
                    }
                    if (ctx.CancellationToken.IsCancellationRequested)
                    {
                        context.Result.AddWarning("Import aborted. A cancellation has been requested.");
                    }
                }
            }
        }

        private void ImportCoreOuter(DataImporterContext ctx)
        {
            var customer = _services.WorkContext.CurrentCustomer;
            var profile = ctx.Request.Profile;
            var logPath = profile.GetImportLogPath();
            FileSystemHelper.DeleteFile(logPath);

            using (var logger = new TraceLogger(logPath))
            {
                var scopes = new List<IDisposable>();

                try
                {
                    var files = profile.GetImportFiles(profile.ImportRelatedData);
                    var groupedFiles = files.GroupBy(x => x.RelatedType);

                    if (!files.Any())
                        throw new SmartException("No files to import.");

                    if (!HasPermission(ctx))
                        throw new SmartException("You do not have permission to perform the selected import.");

                    _dbCache.Enabled = false;
                    _services.MediaService.ImagePostProcessingEnabled = false;
                    scopes.Add(_localizedEntityService.BeginScope());
                    scopes.Add(_urlRecordService.BeginScope());

                    ctx.Log = logger;
                    ctx.Importer = _importerFactory(profile.EntityType);

                    ctx.ExecuteContext.Request = ctx.Request;
                    ctx.ExecuteContext.DataExchangeSettings = _dataExchangeSettings.Value;
                    ctx.ExecuteContext.Services = _services;
                    ctx.ExecuteContext.Log = logger;
                    ctx.ExecuteContext.Languages = _languageService.GetAllLanguages(true);
                    ctx.ExecuteContext.UpdateOnly = profile.UpdateOnly;
                    ctx.ExecuteContext.KeyFieldNames = profile.KeyFieldNames.SplitSafe(",");
                    ctx.ExecuteContext.ImportFolder = profile.GetImportFolder();
                    ctx.ExecuteContext.ExtraData = XmlHelper.Deserialize<ImportExtraData>(profile.ExtraData);

                    var sb = new StringBuilder();
                    sb.AppendLine();
                    sb.AppendLine(new string('-', 40));
                    sb.AppendLine("Smartstore: v." + SmartStoreVersion.CurrentFullVersion);
                    sb.AppendLine("Import profile: {0} {1}".FormatInvariant(profile.Name, profile.Id == 0 ? " (volatile)" : $" (Id {profile.Id})"));
                    foreach (var fileGroup in groupedFiles)
                    {
                        var entityName = fileGroup.Key.HasValue ? fileGroup.Key.Value.ToString() : profile.EntityType.ToString();
                        var fileNames = string.Join(", ", fileGroup.Select(x => x.Name));
                        sb.AppendLine("{0} files: {1}".FormatInvariant(entityName, fileNames));
                    }
                    sb.Append("Executed by: " + customer.Email.NullEmpty() ?? customer.SystemName.NaIfEmpty());
                    ctx.Log.Info(sb.ToString());

                    _services.EventPublisher.Publish(new ImportExecutingEvent(ctx.ExecuteContext));

                    foreach (var fileGroup in groupedFiles)
                    {
                        ctx.ExecuteContext.Result = ctx.Results[fileGroup.Key.HasValue ? fileGroup.Key.Value.ToString() : string.Empty] = new ImportResult();

                        fileGroup.Each(x => ImportCoreInner(ctx, x));
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorsAll(ex);
                }
                finally
                {
                    try
                    {
                        _dbCache.Enabled = true;
                        _services.MediaService.ImagePostProcessingEnabled = true;
                        scopes.Each(x => x.Dispose());

                        _services.EventPublisher.Publish(new ImportExecutedEvent(ctx.ExecuteContext));
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorsAll(ex);
                    }

                    try
                    {
                        // Database context sharing problem: if there are entities in modified state left by the provider due to SaveChanges failure,
                        // then all subsequent SaveChanges would fail too (e.g. IImportProfileService.UpdateImportProfile, IScheduledTaskService.UpdateTask...).
                        // so whatever it is, detach\dispose all what the tracker still has tracked.

                        _services.DbContext.DetachAll(false);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorsAll(ex);
                    }

                    try
                    {
                        SendCompletionEmail(ctx);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorsAll(ex);
                    }

                    try
                    {
                        LogResults(ctx);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorsAll(ex);
                    }

                    try
                    {
                        if (ctx.Results.TryGetValue(string.Empty, out var result))
                        {
                            profile.ResultInfo = XmlHelper.Serialize(result.Clone());
                            _importProfileService.UpdateImportProfile(profile);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorsAll(ex);
                    }

                    try
                    {
                        ctx.Request.CustomData.Clear();
                        ctx.Results.Clear();
                        ctx.Log = null;
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorsAll(ex);
                    }
                }
            }
        }

        public void Import(DataImportRequest request, CancellationToken cancellationToken)
        {
            Guard.NotNull(request, nameof(request));
            Guard.NotNull(cancellationToken, nameof(cancellationToken));

            if (request.Profile != null && request.Profile.Enabled)
            {
                var ctx = new DataImporterContext(request, cancellationToken, T("Admin.DataExchange.Import.ProgressInfo"));
                ImportCoreOuter(ctx);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
