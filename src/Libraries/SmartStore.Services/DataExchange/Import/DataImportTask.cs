using SmartStore.Services.Tasks;

namespace SmartStore.Services.DataExchange.Import
{
	// note: namespace persisted in ScheduleTask.Type
	public partial class DataImportTask : ITask
	{
		private readonly IDataImporter _importer;
		private readonly IImportProfileService _importProfileService;

		public DataImportTask(
			IDataImporter importer,
			IImportProfileService importProfileService)
		{
			_importer = importer;
			_importProfileService = importProfileService;
		}

		public void Execute(TaskExecutionContext ctx)
		{
			var profileId = ctx.ScheduleTask.Alias.ToInt();
			var profile = _importProfileService.GetImportProfileById(profileId);

			var request = new DataImportRequest(profile);

			request.ProgressValueSetter = delegate (int val, int max, string msg)
			{
				ctx.SetProgress(val, max, msg, true);
			};

			if (ctx.Parameters.ContainsKey(TaskExecutor.CurrentCustomerIdParamName))
			{
				request.CustomerId = ctx.Parameters[TaskExecutor.CurrentCustomerIdParamName].ToInt();       // do not use built-in background tasks customer
			}

			_importer.Import(request, ctx.CancellationToken);
		}
	}
}
