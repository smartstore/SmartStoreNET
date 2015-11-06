using SmartStore.Core.Localization;
using SmartStore.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SmartStore.Services.DataExchange
{
	internal class DataExportTask : ITask
	{
		private readonly IDataExporter _exporter;
		private readonly IExportProfileService _exportProfileService;

		public DataExportTask(
			IDataExporter exporter,
			IExportProfileService exportProfileService)
		{
			_exporter = exporter;
			_exportProfileService = exportProfileService;
        }

		public Localizer T { get; set; }

		public void Execute(TaskExecutionContext ctx)
		{
			// TODO: proper error handling

			var profileId = ctx.ScheduleTask.Alias.ToInt();
			var profile = _exportProfileService.GetExportProfileById(profileId);

			// TODO: find a better way to transmit selected entity ids (e.g. new TaskExecutionContext.Parameters property)
			var selectedIdsCacheKey = profile.GetSelectedEntityIdsCacheKey();
			var selectedEntityIds = HttpRuntime.Cache[selectedIdsCacheKey] as string;
			HttpRuntime.Cache.Remove(selectedIdsCacheKey);

			// load provider
			var provider = _exportProfileService.LoadProvider(profile.ProviderSystemName);
			if (provider == null)
				throw new SmartException(T("Admin.Common.ProviderNotLoaded", profile.ProviderSystemName.NaIfEmpty()));

			// build export request
			var request = new DataExportRequest(profile);
			request.ProgressSetter = delegate(int val, int max, string msg)
			{
				ctx.SetProgress(val, max, msg);
			};
			if (selectedEntityIds.HasValue())
			{
				request.EntitiesToExport = selectedEntityIds.ToIntArray();
            }

			// process!
			_exporter.Export(request, ctx.CancellationToken);

			ctx.CancellationToken.ThrowIfCancellationRequested();
		}
	}
}
