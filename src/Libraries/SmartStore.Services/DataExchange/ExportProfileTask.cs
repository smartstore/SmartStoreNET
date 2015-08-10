using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange
{
	public class ExportProfileTask : ITask
	{
		private const string _logName = "log.txt";

		private ICommonServices _services;
		private IExportService _exportService;

		private void Cleanup(ExportProfileTaskContext ctx)
		{
			if (!ctx.Profile.Cleanup)
				return;

			FileSystemHelper.ClearDirectory(ctx.Folder, false, new List<string> { _logName });
			
			// TODO: more deployment specific here
		}

		private void ExportCore(ExportProfileTaskContext ctx)
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
				ctx.Log.Error("Export provider cannot be loaded.");
				return;
			}

		}

		public void Execute(TaskExecutionContext context)
		{
			_services = context.Resolve<ICommonServices>();
			_exportService = context.Resolve<IExportService>();

			var profileId = context.ScheduleTask.Alias.ToInt();
			var profile = _exportService.GetExportProfileById(profileId);

			Execute(profile, null);
		}

		public void Execute(ExportProfile profile, IComponentContext context)
		{
			if (profile == null || !profile.Enabled)
				return;

			if (context != null)
			{
				_services = context.Resolve<ICommonServices>();
				_exportService = context.Resolve<IExportService>();
			}

			var ctx = new ExportProfileTaskContext(profile, _exportService.LoadProvider(profile.ProviderSystemName));

			try
			{
				using (var scope = new DbContextScope(autoDetectChanges: false, validateOnSave: false, forceNoTracking: true))
				using (var logger = new TraceLogger(Path.Combine(ctx.Folder, _logName)))
				{
					ctx.Log = logger;

					if (ctx.Profile.PerStore)
					{
						foreach (var store in _services.StoreService.GetAllStores().Where(x => x.Id != ctx.Filter.StoreId))
						{
							ctx.Store = store;
							ExportCore(ctx);
						}
					}
					else
					{
						ExportCore(ctx);
					}
				}
			}
			finally
			{
				try
				{
					Cleanup(ctx);
				}
				catch { }
			}
		}
	}


	internal class ExportProfileTaskContext
	{
		public ExportProfileTaskContext(ExportProfile profile, Provider<IExportProvider> provider)
		{
			Debug.Assert(profile.FolderName.HasValue(), "Folder name must not be empty.");

			Profile = profile;
			Provider = provider;
			Filter = XmlHelper.Deserialize<ExportFilter>(profile.Filtering);
			Projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);

			Folder = FileSystemHelper.TempDir(@"Profile\Export\{0}".FormatInvariant(profile.FolderName));
		}

		public ExportProfile Profile { get; private set; }
		public Provider<IExportProvider> Provider { get; private set; }
		public ExportFilter Filter { get; private set; }
		public ExportProjection Projection { get; private set; }

		public string Folder { get; private set; }
		public string FileNameSuggestion
		{
			get
			{
				string name = null;

				if (Store != null)
					name = Store.Name;

				if (name.IsEmpty())
					name = "all-stores";

				// be careful with too long file system paths
				name = SeoHelper.GetSeName(name.NaIfEmpty(), true, false).ToValidPath("").Truncate(20);
				return name;
			}
		}

		public TraceLogger Log { get; set; }
		public Store Store { get; set; }
	}
}
