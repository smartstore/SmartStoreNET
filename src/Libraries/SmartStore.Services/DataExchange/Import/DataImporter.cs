using System;
using System.IO;
using System.Text;
using System.Threading;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Customers;
using SmartStore.Services.DataExchange.Csv;
using SmartStore.Services.DataExchange.Import.Internal;
using SmartStore.Services.Messages.Importer;
using SmartStore.Services.Security;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Import
{
	public partial class DataImporter : IDataImporter
	{
		private readonly ICommonServices _services;
		private readonly ICustomerService _customerService;
		private readonly Lazy<IRepository<NewsLetterSubscription>> _subscriptionRepository;

		public DataImporter(
			ICommonServices services,
			ICustomerService customerService,
			Lazy<IRepository<NewsLetterSubscription>> subscriptionRepository)
		{
			_services = services;
			_customerService = customerService;
			_subscriptionRepository = subscriptionRepository;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		private bool HasPermission(DataImporterContext ctx)
		{
			var customer = _customerService.GetCustomerById(ctx.Request.CustomerId);

			if (ctx.Request.Profile.EntityType == ImportEntityType.Product)
				return _services.Permissions.Authorize(StandardPermissionProvider.ManageCatalog, customer);

			if (ctx.Request.Profile.EntityType == ImportEntityType.Customer)
				return _services.Permissions.Authorize(StandardPermissionProvider.ManageCustomers, customer);

			if (ctx.Request.Profile.EntityType == ImportEntityType.NewsLetterSubscription)
				return _services.Permissions.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers, customer);

			return true;
		}

		private void ImportCoreInner(DataImporterContext ctx, string filePath)
		{
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
				throw new SmartException("File does not exist {0}".FormatInvariant(filePath));
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
				csvConfiguration = new CsvConfiguration();
			}

			using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				ctx.ExecuteContext.DataTable = LightweightDataTable.FromFile(Path.GetFileName(filePath), stream, stream.Length, csvConfiguration, ctx.Request.Profile.Skip, take);

				ctx.Importer.Execute(ctx.ExecuteContext);

				ctx.Log.Information("Importer reports {0} added and {1} updated record(s)".FormatInvariant(ctx.ExecuteContext.RecordsAdded, ctx.ExecuteContext.RecordsUpdated));

				if (ctx.ExecuteContext.IsMaxFailures)
					ctx.Log.Warning("Export aborted. The maximum number of failures has been reached");

				if (ctx.CancellationToken.IsCancellationRequested)
					ctx.Log.Warning("Import aborted. A cancellation has been requested");
			}
		}

		private void ImportCoreOuter(DataImporterContext ctx)
		{
			if (ctx.Request.Profile == null || !ctx.Request.Profile.Enabled)
				return;

			if (ctx.Request.CustomerId == 0)
				ctx.Request.CustomerId = _services.WorkContext.CurrentCustomer.Id;

			var logPath = ctx.Request.Profile.GetImportLogPath();

			FileSystemHelper.Delete(logPath);

			using (var logger = new TraceLogger(logPath))
			{
				try
				{
					ctx.Log = logger;
					ctx.ExecuteContext.Log = logger;

					var files = ctx.Request.Profile.GetImportFiles();

					if (files.Count == 0)
						throw new SmartException("No files to import found");

					if (!HasPermission(ctx))
						throw new SmartException("You do not have permission to perform the selected import");

					if (ctx.Request.Profile.EntityType == ImportEntityType.Product)
					{						
					}
					else if (ctx.Request.Profile.EntityType == ImportEntityType.Customer)
					{
					}
					else if (ctx.Request.Profile.EntityType == ImportEntityType.NewsLetterSubscription)
					{
						ctx.Importer = new NewsLetterSubscriptionImporter(_services, _subscriptionRepository.Value);
					}
					else
					{
						throw new SmartException("Unsupported entity type {0}".FormatInvariant(ctx.Request.Profile.EntityType.ToString()));
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
