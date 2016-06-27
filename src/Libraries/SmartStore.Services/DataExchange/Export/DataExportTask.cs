﻿using System.Linq;
using SmartStore.Core.Localization;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.DataExchange.Export
{
	// note: namespace persisted in ScheduleTask.Type
	public partial class DataExportTask : ITask
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
			var profileId = ctx.ScheduleTask.Alias.ToInt();
			var profile = _exportProfileService.GetExportProfileById(profileId);

			// load provider
			var provider = _exportProfileService.LoadProvider(profile.ProviderSystemName);
			if (provider == null)
				throw new SmartException(T("Admin.Common.ProviderNotLoaded", profile.ProviderSystemName.NaIfEmpty()));

			// build export request
			var request = new DataExportRequest(profile, provider);

			request.ProgressValueSetter = delegate (int val, int max, string msg)
			{
				ctx.SetProgress(val, max, msg, true);
			};

			if (ctx.Parameters.ContainsKey("SelectedIds"))
			{
				request.EntitiesToExport = ctx.Parameters["SelectedIds"]
					.SplitSafe(",")
					.Select(x => x.ToInt())
					.ToList();
			}

			// process!
			_exporter.Export(request, ctx.CancellationToken);
		}
	}
}
