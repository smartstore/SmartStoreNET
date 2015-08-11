using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange
{
	public class ExportProfileTask : ITask
	{
		private const string _logName = "log.txt";
		private const int _maxErrors = 20;

		private ICommonServices _services;
		private IExportService _exportService;
		private IProductService _productService;

		private void InitDependencies(TaskExecutionContext context)
		{
			_services = context.Resolve<ICommonServices>();
			_exportService = context.Resolve<IExportService>();
			_productService = context.Resolve<IProductService>();
		}

		private void Cleanup(ExportProfileTaskContext ctx)
		{
			if (!ctx.Profile.Cleanup)
				return;

			FileSystemHelper.ClearDirectory(ctx.Folder, false, new List<string> { _logName });
			
			// TODO: more deployment specific here
		}

		private string GetFilePath(ExportProfileTaskContext ctx, int index)
		{
			string result = null;

			if (ctx.Store != null)
				result = ctx.Store.Name;

			if (result.IsEmpty())
				result = "all-stores";

			// be careful with too long file system paths
			result = "{0}_{1}".FormatInvariant(
				index.ToString("D5"),
				SeoHelper.GetSeName(result.NaIfEmpty(), true, false).ToValidPath("").Truncate(20)
			);

			return Path.Combine(ctx.Folder, result);
		}

		private void FlyOutEndOfFile(ExportProfileTaskContext ctx)
		{
			try
			{
				if (ctx.Export.File.IsOpen)
				{
					ctx.Export.File.IsEndOfFile = true;
					ctx.Export.Record = null;
					ctx.Provider.Value.Execute(ctx.Export);
				}
			}
			catch { }
			finally
			{
				ctx.Export.File.Close();
			}
		}

		private ExportRecord CreateExportRecord<T>(T entity)
		{
			if (typeof(T) == typeof(Product))
			{
				// TODO
			}
			else
			{
				Debug.Fail("Unsupported entity type '{0}'".FormatInvariant(typeof(T).Name));
			}

			return null;
		}

		private void FlyOutEntity<T>(ExportProfileTaskContext ctx, T entity)
		{
			try
			{
				ctx.Export.Record = CreateExportRecord<T>(entity);
			}
			catch (Exception exc)
			{
				++ctx.CountErrors;
				ctx.Log.Error("Failed to create export record.", exc);
			}

			if (ctx.Export.File == null)
				ctx.Export.File.Open(GetFilePath(ctx, ++ctx.CountFiles));

			try
			{
				// do not collect data, do not produce overhead, fly out each individual item
				ctx.Provider.Value.Execute(ctx.Export);

				++ctx.Export.RecordCount;
				++ctx.Export.File.ItemCount;
			}
			catch (Exception exc)
			{
				++ctx.CountErrors;
				ctx.Log.Error("Fly-out data to export provider failed.", exc);
			}
			finally
			{
				ctx.Export.File.IsBeginOfFile = false;
			}

			var isBatchFull = (ctx.Profile.BatchSize > 0 && ctx.Export.File.ItemCount >= ctx.Profile.BatchSize);

			if (isBatchFull)
				FlyOutEndOfFile(ctx);

			try
			{
				foreach (var entry in ctx.Export.Logs)
					ctx.Log.InsertLog(entry);
			}
			catch { }
			finally
			{
				if (ctx.Export.Logs != null)
					ctx.Export.Logs.Clear();
			}

			if (ctx.CountErrors >= _maxErrors)
				throw new SmartException("Aborting because the maximum number of errors ({0}) has been reached.", _maxErrors);
		}

		private void ExportCoreInner(ExportProfileTaskContext ctx)
		{
			FileSystemHelper.ClearDirectory(ctx.Folder, false);

			{
				var logHead = new StringBuilder();
				logHead.AppendLine();
				logHead.AppendLine(new string('-', 40));
				logHead.AppendLine("SmartStore.NET:\t\tv." + SmartStoreVersion.CurrentFullVersion);
				logHead.AppendLine("Export profile:\t\t{0} (Id {1})".FormatInvariant(ctx.Profile.Name, ctx.Profile.Id));

				var plugin = ctx.Provider.Metadata.PluginDescriptor;
				logHead.Append("Plugin:\t\t\t\t");
				logHead.AppendLine(plugin == null ? "".NaIfEmpty() : "{0} ({1}) v.{2}".FormatInvariant(plugin.FriendlyName, plugin.SystemName, plugin.Version.ToString()));

				logHead.AppendLine("Export provider:\t{0} ({1})".FormatInvariant(ctx.Provider == null ? "".NaIfEmpty() : ctx.Provider.Metadata.FriendlyName, ctx.Profile.ProviderSystemName));

				logHead.Append("Store:\t\t\t\t");
				logHead.Append(ctx.Store == null ? "all stores" : "{0} (Id {1})".FormatInvariant(ctx.Store.Name, ctx.Store.Id));

				ctx.Log.Information(logHead.ToString());
			}

			if (ctx.Provider == null)
			{
				ctx.Log.Error("Export aborted. Export provider cannot be loaded.");
				return;
			}

			if (!ctx.Provider.IsValid())
			{
				ctx.Log.Error("Export aborted. Export provider is not valid.");
				return;
			}

			for (ctx.PageIndex = 0; ctx.PageIndex < 9999999; ++ctx.PageIndex)
			{
				_services.DbContext.DetachAll();

				var hasNextPage = false;

				if (ctx.Provider.Value.EntityType == ExportEntityType.Product)
				{
					var searchContext = new ProductSearchContext
					{
						OrderBy = ProductSortingEnum.CreatedOn,
						PageIndex = ctx.PageIndex,
						PageSize = ctx.Profile.Limit,
						StoreId = (ctx.Store == null ? 0 : ctx.Store.Id),
						VisibleIndividuallyOnly = true
					};

					var products = _productService.SearchProducts(searchContext);

					hasNextPage = products.HasNextPage;

					foreach (var product in products)
					{
						if (product.ProductType == ProductType.SimpleProduct || product.ProductType == ProductType.BundledProduct)
						{
							FlyOutEntity<Product>(ctx, product);
						}
						else if (product.ProductType == ProductType.GroupedProduct)
						{
							searchContext.PageSize = int.MaxValue;
							searchContext.VisibleIndividuallyOnly = false;
							searchContext.ParentGroupedProductId = product.Id;

							foreach (var associatedProduct in _productService.SearchProducts(searchContext))
							{
								FlyOutEntity<Product>(ctx, product);
							}
						}
					}
				}


				if (ctx.Cancellation.IsCancellationRequested)
					ctx.Log.Warning("A cancellation has been requested");

				if (ctx.Cancellation.IsCancellationRequested || !hasNextPage)
					break;
			}

			FlyOutEndOfFile(ctx);
		}

		private void ExportCoreOuter(ExportProfileTaskContext ctx)
		{
			if (ctx.Profile == null || !ctx.Profile.Enabled)
				return;

			try
			{
				using (var scope = new DbContextScope(autoDetectChanges: false, validateOnSave: false, forceNoTracking: true))
				using (var logger = new TraceLogger(Path.Combine(ctx.Folder, _logName)))
				{
					ctx.Log = logger;

					if (ctx.Profile.PerStore)
					{
						foreach (var store in _services.StoreService.GetAllStores().Where(x => ctx.Filter.StoreId == 0 || ctx.Filter.StoreId == x.Id))
						{
							ctx.Store = store;
							ExportCoreInner(ctx);
						}
					}
					else
					{
						ExportCoreInner(ctx);
					}
				}
			}
			catch (Exception exc)
			{
				ctx.Log.Error(exc);
			}
			finally
			{
				FlyOutEndOfFile(ctx);

				ctx.Log.Information("Number of created files: {0}".FormatInvariant(ctx.CountFiles));

				try
				{
					Cleanup(ctx);
				}
				catch { }
			}
		}


		public void Execute(TaskExecutionContext context)
		{
			InitDependencies(context);

			var profileId = context.ScheduleTask.Alias.ToInt();
			var profile = _exportService.GetExportProfileById(profileId);

			var ctx = new ExportProfileTaskContext(profile, _exportService.LoadProvider(profile.ProviderSystemName), context.CancellationToken);

			ExportCoreOuter(ctx);
		}

		public void Execute(ExportProfile profile, IComponentContext context)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			if (context == null)
				throw new ArgumentNullException("context");

			InitDependencies(new TaskExecutionContext(context, null));

			var cancellation = new CancellationTokenSource(TimeSpan.FromHours(4.0));

			var ctx = new ExportProfileTaskContext(profile, _exportService.LoadProvider(profile.ProviderSystemName), cancellation.Token);

			ExportCoreOuter(ctx);
		}
	}


	internal class ExportProfileTaskContext
	{
		public ExportProfileTaskContext(ExportProfile profile, Provider<IExportProvider> provider, CancellationToken cancellation)
		{
			Debug.Assert(profile.FolderName.HasValue(), "Folder name must not be empty.");

			Profile = profile;
			Provider = provider;
			Filter = XmlHelper.Deserialize<ExportFilter>(profile.Filtering);
			Projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);
			Cancellation = cancellation;

			Export = new ExportExecuteContext
			{
				Logs = new List<LogContext>()
			};

			Folder = FileSystemHelper.TempDir(@"Profile\Export\{0}".FormatInvariant(profile.FolderName));
		}

		public ExportProfile Profile { get; private set; }
		public Provider<IExportProvider> Provider { get; private set; }
		public ExportFilter Filter { get; private set; }
		public ExportProjection Projection { get; private set; }

		public CancellationToken Cancellation { get; private set; }
		public TraceLogger Log { get; set; }
		public Store Store { get; set; }

		public string Folder { get; private set; }

		public int PageIndex { get; set; }
		public int CountFiles { get; set; }
		public int CountErrors { get; set; }

		public ExportExecuteContext Export { get; set; }
	}
}
