using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Localization;
using SmartStore.Utilities;
using SmartStore.Services.Helpers;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using SmartStore.Core.Logging;
using SmartStore.Core;

namespace SmartStore.Services.Tasks
{
    public partial class ScheduleTaskService : IScheduleTaskService
    {
        private readonly IRepository<ScheduleTask> _taskRepository;
		private readonly IDateTimeHelper _dtHelper;

		public ScheduleTaskService(IRepository<ScheduleTask> taskRepository, IDateTimeHelper dtHelper)
        {
            _taskRepository = taskRepository;
			_dtHelper = dtHelper;

			T = NullLocalizer.Instance;
			Logger = NullLogger.Instance;
        }

		public Localizer T { get; set; }
		public ILogger Logger { get; set; }

        public virtual void DeleteTask(ScheduleTask task)
        {
			Guard.NotNull(task, nameof(task));

            _taskRepository.Delete(task);
        }

        public virtual ScheduleTask GetTaskById(int taskId)
        {
            if (taskId == 0)
                return null;

			return Retry.Run(
				() => _taskRepository.GetById(taskId),
				3, TimeSpan.FromMilliseconds(100),
				RetryOnDeadlockException);
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

			return Retry.Run(
				() => query.ToList(),
				3, TimeSpan.FromMilliseconds(100),
				RetryOnDeadlockException);
		}

        public virtual IList<ScheduleTask> GetPendingTasks()
        {
            var now = DateTime.UtcNow;

            var query = from t in _taskRepository.Table
						where t.NextRunUtc.HasValue && t.NextRunUtc <= now && t.Enabled
                        orderby t.NextRunUtc
                        select t;

			return Retry.Run(
				() => query.ToList(),
				3, TimeSpan.FromMilliseconds(100),
				RetryOnDeadlockException);
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
			var query = GetRunningTasksQuery();

			return Retry.Run(
				() => query.ToList(), 
				3, TimeSpan.FromMilliseconds(100), 
				RetryOnDeadlockException);
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
			Guard.NotNull(task, nameof(task));

			_taskRepository.Insert(task);
        }

        public virtual void UpdateTask(ScheduleTask task)
        {
			Guard.NotNull(task, nameof(task));

			try
			{
				using (var scope = new DbContextScope(_taskRepository.Context, autoCommit: true))
				{
					Retry.Run(() => _taskRepository.Update(task), 3, TimeSpan.FromMilliseconds(50), (attempt, exception) =>
					{
						var ex = exception as DbUpdateConcurrencyException;
						if (ex == null) return;

						var entry = ex.Entries.Single();
						var current = (ScheduleTask)entry.CurrentValues.ToObject(); // from current scope

						// When 'StopOnError' is true, the 'Enabled' property could have been set to true on exception.
						var prop = entry.Property("Enabled");
						var enabledModified = !prop.CurrentValue.Equals(prop.OriginalValue);

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

						if (attempt == 3)
						{
							_taskRepository.Update(task);
						}
					});
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
				throw;
			}
        }

		public ScheduleTask GetOrAddTask<T>(Action<ScheduleTask> newAction) where T : ITask
		{
			Guard.NotNull(newAction, nameof(newAction));

			var type = typeof(T);

			if (type.IsAbstract || type.IsInterface || type.IsNotPublic)
			{
				throw new InvalidOperationException("Only concrete public task types can be registered.");
			}

			var scheduleTask = this.GetTaskByType<T>();

			if (scheduleTask == null)
			{
				scheduleTask = new ScheduleTask { Type = type.AssemblyQualifiedNameWithoutVersion() };
				newAction(scheduleTask);
				InsertTask(scheduleTask);
			}

			return scheduleTask;
		}

		public void CalculateFutureSchedules(IEnumerable<ScheduleTask> tasks, bool isAppStart = false)
		{
			Guard.NotNull(tasks, nameof(tasks));
			
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
					FixTypeName(task);
				}
				else
				{
					UpdateTask(task);
				}
			}
			
			if (isAppStart)
			{
				// On app start this method's execution is thread-safe, making it sufficient
				// to commit all changes in one go.
				_taskRepository.Context.SaveChanges();
			}
		}

		private void FixTypeName(ScheduleTask task)
		{
			// in versions prior V3 a double space could exist in ScheduleTask type name
			if (task.Type.IndexOf(",  ") > 0)
			{
				task.Type = task.Type.Replace(",  ", ", ");
			}
		}

		public virtual DateTime? GetNextSchedule(ScheduleTask task)
		{
			if (task.Enabled)
			{
				try
				{
					var localTimeZone = _dtHelper.DefaultStoreTimeZone;
					var baseTime = TimeZoneInfo.ConvertTime(DateTime.UtcNow, localTimeZone);
					var next = CronExpression.GetNextSchedule(task.CronExpression, baseTime);
					var utcTime = _dtHelper.ConvertToUtcTime(next, localTimeZone);

					return utcTime;
				}
				catch (Exception ex)
				{
					Logger.ErrorFormat(ex, "Could not calculate next schedule time for task '{0}'", task.Name);
				}
			}

			return null;
		}

		private static void RetryOnDeadlockException(int attemp, Exception ex)
		{
			var isDeadLockException = 
				(ex as EntityCommandExecutionException).IsDeadlockException() || 
				(ex as SqlException).IsDeadlockException();

			if (!isDeadLockException)
			{
				// we only want to retry on deadlock stuff
				throw ex;
			}
		}
	}
}
