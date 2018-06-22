using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services.Helpers;
using SmartStore.Utilities;

namespace SmartStore.Services.Tasks
{
    public partial class ScheduleTaskService : IScheduleTaskService
    {
        private readonly IRepository<ScheduleTask> _taskRepository;
        private readonly IRepository<ScheduleTaskHistory> _taskHistoryRepository;
        private readonly IDateTimeHelper _dtHelper;
        private readonly IApplicationEnvironment _env;

        public ScheduleTaskService(
            IRepository<ScheduleTask> taskRepository,
            IRepository<ScheduleTaskHistory> taskHistoryRepository,
            IDateTimeHelper dtHelper,
            IApplicationEnvironment env)
        {
            _taskRepository = taskRepository;
            _taskHistoryRepository = taskHistoryRepository;
			_dtHelper = dtHelper;
            _env = env;

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
            var machineName = _env.MachineName;

            var query =
                from t in _taskRepository.Table
                where t.NextRunUtc.HasValue && t.NextRunUtc <= now && t.Enabled
                select new
                {
                    Task = t,
                    LastHistoryEntry = t.ScheduleTaskHistory
                        .Where(th => !t.RunPerMachine || (t.RunPerMachine && th.MachineName == machineName))
                        .OrderByDescending(th => th.StartedOnUtc)
                        .ThenByDescending(th => th.Id)
                        .FirstOrDefault()
                };

            var tasks = Retry.Run(
                () => query.ToList(),
                3, TimeSpan.FromMilliseconds(100),
                RetryOnDeadlockException);

            var pendingTasks = tasks
                .Where(x => x.LastHistoryEntry == null || !x.LastHistoryEntry.IsRunning)
                .Select(x => x.Task)
                .ToList();

            return pendingTasks;


            //var query = from t in _taskRepository.Table
            //            where t.NextRunUtc.HasValue && t.NextRunUtc <= now && t.Enabled
            //            orderby t.NextRunUtc
            //            select t;

    //        return Retry.Run(
				//() => query.ToList(),
				//3, TimeSpan.FromMilliseconds(100),
				//RetryOnDeadlockException);
		}

		//public virtual bool HasRunningTasks()
		//{
		//	var query = GetRunningTasksQuery();
		//	return query.Any();
		//}

		//public virtual bool IsTaskRunning(int taskId)
		//{
		//	if (taskId <= 0)
		//		return false;

		//	var query = GetRunningTasksQuery();
		//	query.Where(t => t.Id == taskId);
		//	return query.Any();
		//}

		//public virtual IList<ScheduleTask> GetRunningTasks()
		//{
		//	var query = GetRunningTasksQuery();

		//	return Retry.Run(
		//		() => query.ToList(), 
		//		3, TimeSpan.FromMilliseconds(100), 
		//		RetryOnDeadlockException);
		//}

  //      private IQueryable<ScheduleTask> GetRunningTasksQuery()
		//{
  //          var query = from t in _taskRepository.Table
  //                      where t.LastStartUtc.HasValue && t.LastStartUtc.Value > (t.LastEndUtc ?? DateTime.MinValue)
  //                      orderby t.LastStartUtc
  //                      select t;

  //          return query;
		//}


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
                _taskRepository.Update(task);

    //            using (var scope = new DbContextScope(_taskRepository.Context, autoCommit: true))
				//{
				//	Retry.Run(() => _taskRepository.Update(task), 3, TimeSpan.FromMilliseconds(50), (attempt, exception) =>
				//	{
				//		var ex = exception as DbUpdateConcurrencyException;
				//		if (ex == null) return;

				//		var entry = ex.Entries.Single();
				//		var current = (ScheduleTask)entry.CurrentValues.ToObject(); // from current scope

				//		// When 'StopOnError' is true, the 'Enabled' property could have been set to true on exception.
				//		var prop = entry.Property("Enabled");
				//		var enabledModified = !prop.CurrentValue.Equals(prop.OriginalValue);

				//		// Save current cron expression
				//		var cronExpression = task.CronExpression;

				//		// Fetch Name, CronExpression, Enabled & StopOnError from database
				//		// (these were possibly edited thru the backend)
				//		_taskRepository.Context.ReloadEntity(task);

				//		// Do we have to reschedule the task?
				//		var cronModified = cronExpression != task.CronExpression;

				//		// Copy execution specific data from current to reloaded entity 
				//		task.LastEndUtc = current.LastEndUtc;
				//		task.LastError = current.LastError;
				//		task.LastStartUtc = current.LastStartUtc;
				//		task.LastSuccessUtc = current.LastSuccessUtc;
				//		task.ProgressMessage = current.ProgressMessage;
				//		task.ProgressPercent = current.ProgressPercent;
				//		task.NextRunUtc = current.NextRunUtc;
				//		if (enabledModified)
				//		{
				//			task.Enabled = current.Enabled;
				//		}
				//		if (task.NextRunUtc.HasValue && cronModified)
				//		{
				//			// reschedule task
				//			task.NextRunUtc = GetNextSchedule(task);
				//		}

				//		if (attempt == 3)
				//		{
				//			_taskRepository.Update(task);
				//		}
				//	});
				//}
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
					//task.ProgressPercent = null;
					//task.ProgressMessage = null;
					//if (task.LastEndUtc.GetValueOrDefault() < task.LastStartUtc)
					//{
					//	task.LastEndUtc = task.LastStartUtc;
					//	task.LastError = T("Admin.System.ScheduleTasks.AbnormalAbort");
					//}
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

            if (isAppStart)
            {
                // Empty progress information.
                var entriesWithProgress = _taskHistoryRepository.Table
                    .Where(x => x.ProgressPercent != null || !string.IsNullOrEmpty(x.ProgressMessage))
                    .ToList();

                if (entriesWithProgress.Any())
                {
                    foreach (var entry in entriesWithProgress)
                    {
                        entry.ProgressPercent = null;
                        entry.ProgressMessage = null;
                    }
                    _taskHistoryRepository.UpdateRange(entriesWithProgress);
                    _taskHistoryRepository.Context.SaveChanges();
                }
            }

            if (isAppStart)
            {
                // Normalize invalid finish date.
                var entriesWithInvalidDate = _taskHistoryRepository.Table
                    .Where(x => x.FinishedOnUtc != null && x.FinishedOnUtc < x.StartedOnUtc)
                    .ToList();

                if (entriesWithInvalidDate.Any())
                {
                    string abnormalAbort = T("Admin.System.ScheduleTasks.AbnormalAbort");
                    foreach (var entry in entriesWithInvalidDate)
                    {
                        entry.FinishedOnUtc = entry.StartedOnUtc;
                        entry.Error = abnormalAbort;
                    }
                    _taskHistoryRepository.UpdateRange(entriesWithInvalidDate);
                    _taskHistoryRepository.Context.SaveChanges();
                }
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


        #region Schedule task history

        public virtual IList<ScheduleTaskHistory> GetRunningHistoryEntries()
        {
            var machineName = _env.MachineName;

            var query =
                from t in _taskRepository.TableUntracked
                join th in _taskHistoryRepository.TableUntracked on t.Id equals th.ScheduleTaskId
                where th.IsRunning && th.MachineName == machineName
                select th;

            var groupQuery =
                from th in query
                group th by th.ScheduleTaskId into grp
                select grp
                    .OrderByDescending(x => x.StartedOnUtc)
                    .ThenByDescending(x => x.Id)
                    .FirstOrDefault();

            return Retry.Run(
				() => groupQuery.ToList(), 
				3, TimeSpan.FromMilliseconds(100), 
				RetryOnDeadlockException);
        }

        public virtual ScheduleTaskHistory GetRunningHistoryEntryByTaskId(int taskId)
        {
            if (taskId == 0)
            {
                return null;
            }

            var machineName = _env.MachineName;

            var historyEntry = _taskHistoryRepository.TableUntracked.Expand(x => x.ScheduleTask)
                .Where(x => x.ScheduleTaskId == taskId && x.IsRunning && x.MachineName == machineName)
                .OrderByDescending(x => x.StartedOnUtc)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            return historyEntry;
        }

        public virtual void InsertHistoryEntry(ScheduleTaskHistory historyEntry)
        {
            Guard.NotNull(historyEntry, nameof(historyEntry));

            _taskHistoryRepository.Insert(historyEntry);
        }

        public virtual void UpdateHistoryEntry(ScheduleTaskHistory historyEntry)
        {
            Guard.NotNull(historyEntry, nameof(historyEntry));

            try
            {
                _taskHistoryRepository.Update(historyEntry);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                // Do not throw.
            }
        }

        #endregion
    }
}
