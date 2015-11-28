using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Localization;
using SmartStore.Services.Helpers;

namespace SmartStore.Services.Tasks
{
    /// <summary>
    /// Task service
    /// </summary>
    public partial class ScheduleTaskService : IScheduleTaskService
    {
        #region Fields

        private readonly IRepository<ScheduleTask> _taskRepository;
		private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

		public ScheduleTaskService(IRepository<ScheduleTask> taskRepository, IDateTimeHelper dateTimeHelper)
        {
            this._taskRepository = taskRepository;
			this._dateTimeHelper = dateTimeHelper;

			T = NullLocalizer.Instance;
        }

		public Localizer T { get; set; }

        #endregion

        #region Methods

        public virtual void DeleteTask(ScheduleTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _taskRepository.Delete(task);
        }

        public virtual ScheduleTask GetTaskById(int taskId)
        {
            if (taskId == 0)
                return null;

            return _taskRepository.GetById(taskId);
        }

        public virtual ScheduleTask GetTaskByType(string type)
        {
			try
			{
				if (type.HasValue())
				{
					var query = _taskRepository.Table
						.Where(t => t.Type == type)
						.OrderByDescending(t => t.Id);

					var task = query.FirstOrDefault();
					return task;
				}
			}
			catch (Exception exc)
			{
				// do not throw an exception if the underlying provider failed on Open.
				exc.Dump();
			}
			return null;
        }

		public virtual IList<ScheduleTask> GetAllTasks(bool includeDisabled = false)
        {
            var query = _taskRepository.Table;
			if (!includeDisabled)
            {
                query = query.Where(t => t.Enabled);
            }
            query = query.OrderByDescending(t => t.Enabled);

            var tasks = query.ToList();
            return tasks;
        }

        public virtual IList<ScheduleTask> GetPendingTasks()
        {
            var now = DateTime.UtcNow;

            var query = from t in _taskRepository.Table
						where t.NextRunUtc.HasValue && t.NextRunUtc <= now && t.Enabled
                        orderby t.NextRunUtc
                        select t;

            return query.ToList();
        }

		public virtual bool HasRunningTasks()
		{
			var query = GetRunningTasksQuery();
			return query.Any();
		}

		public virtual bool IsTaskRunning(int taskId)
		{
			if (taskId <= 0)
				return false;

			var query = GetRunningTasksQuery();
			query.Where(t => t.Id == taskId);
			return query.Any();
		}

		public virtual IList<ScheduleTask> GetRunningTasks()
		{
			return GetRunningTasksQuery().ToList();
		}

		private IQueryable<ScheduleTask> GetRunningTasksQuery()
		{
			var query = from t in _taskRepository.Table
						where t.LastStartUtc.HasValue && t.LastStartUtc.Value > (t.LastEndUtc ?? DateTime.MinValue)
						orderby t.LastStartUtc
						select t;

			return query;
		}


        public virtual void InsertTask(ScheduleTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

            _taskRepository.Insert(task);
        }

        public virtual void UpdateTask(ScheduleTask task)
        {
            if (task == null)
                throw new ArgumentNullException("task");

			bool saveFailed;
			bool? autoCommit = null;

			do
			{
				saveFailed = false;

				// ALWAYS save immediately
				try
				{
					autoCommit = _taskRepository.AutoCommitEnabled;
					_taskRepository.AutoCommitEnabled = true;
					_taskRepository.Update(task);
				}
				catch (DbUpdateConcurrencyException ex)
				{
					saveFailed = true;
					
					var entry = ex.Entries.Single();
					var current = (ScheduleTask)entry.CurrentValues.ToObject(); // from current scope

					// When 'StopOnError' is true, the 'Enabled' property could have been be set to true on exception.
					var enabledModified = entry.Property("Enabled").IsModified;

					// Save current cron expression
					var cronExpression = task.CronExpression;

					// Fetch Name, CronExpression, Enabled & StopOnError from database
					// (these were possibly edited thru the backend)
					_taskRepository.Context.ReloadEntity(task);

					// Do we have to reschedule the task?
					var cronModified = cronExpression != task.CronExpression;

					// Copy execution specific data from current to reloaded entity 
					task.LastEndUtc = current.LastEndUtc;
					task.LastError = current.LastError;
					task.LastStartUtc = current.LastStartUtc;
					task.LastSuccessUtc = current.LastSuccessUtc;
					task.ProgressMessage = current.ProgressMessage;
					task.ProgressPercent = current.ProgressPercent;
					task.NextRunUtc = current.NextRunUtc;
					if (enabledModified)
					{
						task.Enabled = current.Enabled;
					}
					if (task.NextRunUtc.HasValue && cronModified)
					{
						// reschedule task
						task.NextRunUtc = GetNextSchedule(task);
					}
				}
				finally
				{
					_taskRepository.AutoCommitEnabled = autoCommit;
				}
			} while (saveFailed);
        }

		public void CalculateFutureSchedules(IEnumerable<ScheduleTask> tasks, bool isAppStart = false)
		{
			Guard.ArgumentNotNull(() => tasks);
			
			using (var scope = new DbContextScope(autoCommit: false))
			{
				var now = DateTime.UtcNow;
				foreach (var task in tasks)
				{
					task.NextRunUtc = GetNextSchedule(task);
					if (isAppStart)
					{
						task.ProgressPercent = null;
						task.ProgressMessage = null;
						if (task.LastEndUtc.GetValueOrDefault() < task.LastStartUtc)
						{
							task.LastEndUtc = task.LastStartUtc;
							task.LastError = T("Admin.System.ScheduleTasks.AbnormalAbort");
						}
					}
					this.UpdateTask(task);
				}

				scope.Commit();
			}
		}

		public virtual DateTime? GetNextSchedule(ScheduleTask task)
		{
			if (task.Enabled)
			{
				try
				{
					var baseTime = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow);
					var next = CronExpression.GetNextSchedule(task.CronExpression, baseTime);
					return _dateTimeHelper.ConvertToUtcTime(next);
				}
				catch { }
			}

			return null;
		}

        #endregion

	}
}
