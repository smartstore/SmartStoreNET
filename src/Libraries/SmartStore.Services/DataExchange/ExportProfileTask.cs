using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
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
		private const int _pageSize = 100;

		private ICommonServices _services;
		private IExportService _exportService;
		private IProductService _productService;

		#region Entity to expando

		private ExpandoObject ToProduct(ExportProfileTaskContext ctx, Product product)
		{
			IDictionary<string, object> expando = new ExpandoObject();

			expando.Add("Id", product.Id);
			expando.Add("Name", product.Name);
			expando.Add("Sku", product.Sku);
			expando.Add("ShortDescription", product.ShortDescription);

			return expando as ExpandoObject;
		}

		private ExpandoObject ToExpando<T>(ExportProfileTaskContext ctx, T entity) where T : BaseEntity
		{
			Product product = null;

			if ((product = entity as Product) != null)
			{
				return ToProduct(ctx, product);
			}

			ctx.Log.Error("Unsupported entity type '{0}'".FormatInvariant(typeof(T).Name));

			return null;
		}

		#endregion

		#region Get entities

		private IEnumerable<Product> GetProducts(ExportProfileTaskContext ctx, int pageIndex)
		{
			_services.DbContext.DetachAll();

			if (!ctx.Cancellation.IsCancellationRequested)
			{
				var searchContext = new ProductSearchContext
				{
					OrderBy = ProductSortingEnum.CreatedOn,
					PageIndex = ctx.Profile.Offset + pageIndex,
					PageSize = _pageSize,
					StoreId = (ctx.Store == null ? 0 : ctx.Store.Id),
					VisibleIndividuallyOnly = true
				};

				var products = _productService.SearchProducts(searchContext);

				foreach (var product in products)
				{
					if (product.ProductType == ProductType.SimpleProduct || product.ProductType == ProductType.BundledProduct)
					{
						yield return product;
					}
					else if (product.ProductType == ProductType.GroupedProduct)
					{
						var associatedSearchContext = new ProductSearchContext
						{
							OrderBy = ProductSortingEnum.CreatedOn,
							PageSize = int.MaxValue,
							StoreId = (ctx.Store == null ? 0 : ctx.Store.Id),
							VisibleIndividuallyOnly = false,
							ParentGroupedProductId = product.Id
						};

						foreach (var associatedProduct in _productService.SearchProducts(associatedSearchContext))
						{
							yield return associatedProduct;
						}
					}
				}
			}
		}

		#endregion

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

		private bool ExportCoreInner(ExportProfileTaskContext ctx)
		{
			if (ctx.Provider == null)
			{
				ctx.Log.Error("Export aborted. Export provider cannot be loaded.");
				return false;
			}

			if (!ctx.Provider.IsValid())
			{
				ctx.Log.Error("Export aborted. Export provider is not valid.");
				return false;
			}

			// be careful with too long file system paths
			ctx.Export.FileNamePattern = string.Concat(
				"{0}-",
				ctx.Store == null ? "all-stores" : SeoHelper.GetSeName(ctx.Store.Name, true, false).ToValidFileName("").Truncate(20),
				"{1}",
				ctx.Provider.Value.FileExtension.ToLower().EnsureStartsWith("."));


			if (ctx.Provider.Value.EntityType == ExportEntityType.Product)
			{
				var anySingleProduct = _productService.SearchProducts(new ProductSearchContext
				{
					OrderBy = ProductSortingEnum.CreatedOn,
					PageIndex = ctx.Profile.Offset,
					PageSize = 1,
					StoreId = (ctx.Store == null ? 0 : ctx.Store.Id),
					VisibleIndividuallyOnly = true
				});

				ctx.Export.Data = new ExportSegmenter<Product>(
					pageIndex => GetProducts(ctx, pageIndex),
					entity => ToExpando<Product>(ctx, entity),
					_pageSize,
					anySingleProduct.TotalCount
				);
			}

			if (ctx.Export.Data == null)
			{
				ctx.Log.Error("Unsupported entity type '{0}'".FormatInvariant(ctx.Provider.Value.EntityType.ToString()));
			}
			else
			{
				ctx.Provider.Value.Execute(ctx.Export);
			}

			if (ctx.Cancellation.IsCancellationRequested)
			{
				ctx.Log.Warning("A cancellation has been requested");
				return false;
			}
			return true;
		}

		private void ExportCoreOuter(ExportProfileTaskContext ctx)
		{
			if (ctx.Profile == null || !ctx.Profile.Enabled)
				return;

			try
			{
				FileSystemHelper.ClearDirectory(ctx.Folder, false);

				using (var scope = new DbContextScope(autoDetectChanges: false, validateOnSave: false, forceNoTracking: true))
				using (var logger = new TraceLogger(Path.Combine(ctx.Folder, _logName)))
				{
					ctx.Log = logger;
					ctx.Export.Log = logger;

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

						if (!ctx.Profile.PerStore)
							logHead.Append("Store:\t\t\t\tprocessing all stores");

						ctx.Log.Information(logHead.ToString());
					}


					if (ctx.Profile.PerStore)
					{
						foreach (var store in _services.StoreService.GetAllStores().Where(x => ctx.Filter.StoreId == 0 || ctx.Filter.StoreId == x.Id))
						{
							ctx.Log.Information("Store:\t\t\t\tprocessing \"{0}\" (Id {1})".FormatInvariant(store.Name, store.Id));
							ctx.Store = store;

							if (!ExportCoreInner(ctx))
								break;
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
				//if (ctx.Export.ExportedRecords.HasValue)
				//	ctx.Log.Information("Number of exported records: {0}".FormatInvariant(ctx.Export.ExportedRecords.Value));

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
			Folder = FileSystemHelper.TempDir(@"Profile\Export\{0}".FormatInvariant(profile.FolderName));

			Export = new ExportExecuteContext(Cancellation, Folder);
		}

		public ExportProfile Profile { get; private set; }
		public Provider<IExportProvider> Provider { get; private set; }
		public ExportFilter Filter { get; private set; }
		public ExportProjection Projection { get; private set; }

		public CancellationToken Cancellation { get; private set; }
		public TraceLogger Log { get; set; }
		public Store Store { get; set; }

		public string Folder { get; private set; }

		public ExportExecuteContext Export { get; set; }
	}
}
