using System;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Events;
using SmartStore.Services.Tasks;

namespace SmartStore.Services.ExportImport
{
	public partial class ExportService : IExportService
	{
		private readonly IRepository<ExportProfile> _exportProfileRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly Lazy<IScheduleTaskService> _scheduleTaskService;

		public ExportService(
			IRepository<ExportProfile> exportProfileRepository,
			IEventPublisher eventPublisher,
			Lazy<IScheduleTaskService> scheduleTaskService)
		{
			_exportProfileRepository = exportProfileRepository;
			_eventPublisher = eventPublisher;
			_scheduleTaskService = scheduleTaskService;
		}

		#region Export profiles

		public virtual void InsertExportProfile(ExportProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			_exportProfileRepository.Insert(profile);

			_eventPublisher.EntityInserted(profile);
		}

		public virtual void UpdateExportProfile(ExportProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			_exportProfileRepository.Update(profile);

			_eventPublisher.EntityUpdated(profile);
		}

		public virtual void DeleteExportProfile(ExportProfile profile)
		{
			if (profile == null)
				throw new ArgumentNullException("profile");

			if (profile.ScheduleTask != null)
			{
				_scheduleTaskService.Value.DeleteTask(profile.ScheduleTask);
			}

			_exportProfileRepository.Delete(profile);

			_eventPublisher.EntityDeleted(profile);
		}

		public virtual IQueryable<ExportProfile> GetExportProfiles(bool? enabled = null)
		{
			var query =
				from x in _exportProfileRepository.Table.Expand(x => x.ScheduleTask)
				select x;

			if (enabled.HasValue)
			{
				query = query.Where(x => x.Enabled == enabled.Value);
			}

			return query.OrderBy(x => x.Name);
		}

		#endregion
	}
}
