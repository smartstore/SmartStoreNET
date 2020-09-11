using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
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
        private readonly Lazy<CommonSettings> _commonSettings;

        public ScheduleTaskService(
            IRepository<ScheduleTask> taskRepository,
            IRepository<ScheduleTaskHistory> taskHistoryRepository,
            IDateTimeHelper dtHelper,
            IApplicationEnvironment env,
            Lazy<CommonSettings> commonSettings)
        {
            _taskRepository = taskRepository;
            _taskHistoryRepository = taskHistoryRepository;
            _dtHelper = dtHelper;
            _env = env;
            _commonSettings = commonSettings;

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
                // Do not throw an exception if the underlying provider failed on Open.
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

        public async virtual Task<IList<ScheduleTask>> GetPendingTasksAsync()
        {
            var now = DateTime.UtcNow;
            var machineName = _env.MachineName;

            var query = (
                from t in _taskRepository.Table
                where t.NextRunUtc.HasValue && t.NextRunUtc <= now && t.Enabled
                select new
                {
                    Task = t,
                    LastEntry = t.ScheduleTaskHistory
                        .Where(th => !t.RunPerMachine || (t.RunPerMachine && th.MachineName == machineName))
                        .OrderByDescending(th => th.StartedOnUtc)
                        .ThenByDescending(th => th.Id)
                        .FirstOrDefault()
                });

            var tasks = await Retry.RunAsync(
                () => query.ToListAsync(),
                3, TimeSpan.FromMilliseconds(100),
                RetryOnDeadlockException);

            var pendingTasks = tasks
                .Select(x =>
                {
                    x.Task.LastHistoryEntry = x.LastEntry;
                    return x.Task;
                })
                .Where(x => x.IsPending)
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.NextRunUtc.Value)
                .ToList();

            return pendingTasks;
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
                _taskRepository.Update(task);
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
                // Normalize task history entries.
                // That is, no task can run when the application starts and therefore no entry may be marked as running.
                var entries = _taskHistoryRepository.Table
                    .Where(x =>
                        x.IsRunning ||
                        x.ProgressPercent != null ||
                        !string.IsNullOrEmpty(x.ProgressMessage) ||
                        (x.FinishedOnUtc != null && x.FinishedOnUtc < x.StartedOnUtc)
                    )
                    .ToList();

                if (entries.Any())
                {
                    string abnormalAbort = T("Admin.System.ScheduleTasks.AbnormalAbort");
                    foreach (var entry in entries)
                    {
                        var invalidTimeRange = entry.FinishedOnUtc.HasValue && entry.FinishedOnUtc < entry.StartedOnUtc;
                        if (invalidTimeRange || entry.IsRunning)
                        {
                            entry.Error = abnormalAbort;
                        }

                        entry.IsRunning = false;
                        entry.ProgressPercent = null;
                        entry.ProgressMessage = null;
                        if (invalidTimeRange)
                        {
                            entry.FinishedOnUtc = entry.StartedOnUtc;
                        }
                    }

                    _taskHistoryRepository.UpdateRange(entries);
                    _taskHistoryRepository.Context.SaveChanges();
                }
            }
        }

        private void FixTypeName(ScheduleTask task)
        {
            // In versions prior V3 a double space could exist in ScheduleTask type name.
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
                // We only want to retry on deadlock stuff.
                throw ex;
            }
        }

        #region Schedule task history

        protected virtual IQueryable<ScheduleTaskHistory> GetHistoryEntriesQuery(
            int taskId = 0,
            bool forCurrentMachine = false,
            bool lastEntryOnly = false,
            bool? isRunning = null)
        {
            var query = _taskHistoryRepository.TableUntracked;

            if (lastEntryOnly)
            {
                query =
                    from th in query
                    group th by th.ScheduleTaskId into grp
                    select grp
                        .OrderByDescending(x => x.StartedOnUtc)
                        .ThenByDescending(x => x.Id)
                        .FirstOrDefault();
            }

            if (taskId != 0)
            {
                query = query.Where(x => x.ScheduleTaskId == taskId);
            }
            if (forCurrentMachine)
            {
                var machineName = _env.MachineName;
                query = query.Where(x => x.MachineName == machineName);
            }
            if (isRunning.HasValue)
            {
                query = query.Where(x => x.IsRunning == isRunning.Value);
            }

            query = query
                .OrderByDescending(x => x.StartedOnUtc)
                .ThenByDescending(x => x.Id);

            return query;
        }

        protected virtual IQueryable<ScheduleTaskHistory> GetHistoryEntriesQuery(
            ScheduleTask task,
            bool forCurrentMachine = false,
            bool? isRunning = null)
        {
            _taskRepository.Context.LoadCollection(
                task,
                (ScheduleTask x) => x.ScheduleTaskHistory,
                false,
                (IQueryable<ScheduleTaskHistory> query) =>
                {
                    if (forCurrentMachine)
                    {
                        var machineName = _env.MachineName;
                        query = query.Where(x => x.MachineName == machineName);
                    }
                    if (isRunning.HasValue)
                    {
                        query = query.Where(x => x.IsRunning == isRunning.Value);
                    }

                    query = query
                        .OrderByDescending(x => x.StartedOnUtc)
                        .ThenByDescending(x => x.Id);

                    return query;
                });

            return task.ScheduleTaskHistory.AsQueryable();
        }

        public virtual IPagedList<ScheduleTaskHistory> GetHistoryEntries(
            int pageIndex,
            int pageSize,
            int taskId = 0,
            bool forCurrentMachine = false,
            bool lastEntryOnly = false,
            bool? isRunning = null)
        {
            var query = GetHistoryEntriesQuery(taskId, forCurrentMachine, lastEntryOnly, isRunning);
            var entries = new PagedList<ScheduleTaskHistory>(query, pageIndex, pageSize);
            return entries;
        }

        public virtual IPagedList<ScheduleTaskHistory> GetHistoryEntries(
            int pageIndex,
            int pageSize,
            ScheduleTask task,
            bool forCurrentMachine = false,
            bool? isRunning = null)
        {
            if (task == null)
            {
                return new PagedList<ScheduleTaskHistory>(new List<ScheduleTaskHistory>(), pageIndex, pageSize);
            }

            var query = GetHistoryEntriesQuery(task, forCurrentMachine, isRunning);
            var entries = new PagedList<ScheduleTaskHistory>(query, pageIndex, pageSize);
            return entries;
        }

        public virtual ScheduleTaskHistory GetLastHistoryEntryByTaskId(int taskId, bool? isRunning = null)
        {
            if (taskId == 0)
            {
                return null;
            }

            var query = GetHistoryEntriesQuery(taskId, true, false, isRunning);
            query = query.Expand(x => x.ScheduleTask);

            var entry = Retry.Run(
                () => query.FirstOrDefault(),
                3, TimeSpan.FromMilliseconds(100),
                RetryOnDeadlockException);

            return entry;
        }

        public virtual ScheduleTaskHistory GetLastHistoryEntryByTask(ScheduleTask task, bool? isRunning = null)
        {
            if (task == null)
            {
                return null;
            }

            var query = GetHistoryEntriesQuery(task, true, isRunning);

            var entry = Retry.Run(
                () => query.FirstOrDefault(),
                3, TimeSpan.FromMilliseconds(100),
                RetryOnDeadlockException);

            return entry;
        }

        public virtual ScheduleTaskHistory GetHistoryEntryById(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return _taskHistoryRepository.GetById(id);
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
            catch (Exception ex)
            {
                Logger.Error(ex);
                // Do not throw.
            }
        }

        public virtual void DeleteHistoryEntry(ScheduleTaskHistory historyEntry)
        {
            Guard.NotNull(historyEntry, nameof(historyEntry));
            Guard.IsTrue(!historyEntry.IsRunning, nameof(historyEntry.IsRunning), "Cannot delete a running schedule task history entry.");

            _taskHistoryRepository.Delete(historyEntry);
        }

        public virtual int DeleteHistoryEntries()
        {
            var count = 0;
            var idsToDelete = new HashSet<int>();

            if (_commonSettings.Value.MaxScheduleHistoryAgeInDays > 0)
            {
                var earliestDate = DateTime.UtcNow.AddDays(-1 * _commonSettings.Value.MaxScheduleHistoryAgeInDays);
                var ids = _taskHistoryRepository.TableUntracked
                    .Where(x => x.StartedOnUtc <= earliestDate && !x.IsRunning)
                    .Select(x => x.Id)
                    .ToList();

                idsToDelete.AddRange(ids);
            }

            // We have to group by task otherwise we would only keep entries from very frequently executed tasks.
            if (_commonSettings.Value.MaxNumberOfScheduleHistoryEntries > 0)
            {
                var query =
                    from th in _taskHistoryRepository.TableUntracked
                    where !th.IsRunning
                    group th by th.ScheduleTaskId into grp
                    select grp
                        .OrderByDescending(x => x.StartedOnUtc)
                        .ThenByDescending(x => x.Id)
                        .Skip(_commonSettings.Value.MaxNumberOfScheduleHistoryEntries)
                        .Select(x => x.Id);

                var ids = query.SelectMany(x => x).ToList();

                idsToDelete.AddRange(ids);
            }

            try
            {
                if (idsToDelete.Any())
                {
                    using (var scope = new DbContextScope(_taskHistoryRepository.Context, autoCommit: false))
                    {
                        var pageIndex = 0;
                        IPagedList<int> pagedIds = null;

                        do
                        {
                            pagedIds = new PagedList<int>(idsToDelete, pageIndex++, 100);

                            var entries = _taskHistoryRepository.Table
                                .Where(x => pagedIds.Contains(x.Id))
                                .ToList();

                            entries.Each(x => DeleteHistoryEntry(x));
                            count += scope.Commit();
                        }
                        while (pagedIds.HasNextPage);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return count;
        }

        #endregion
    }
}
