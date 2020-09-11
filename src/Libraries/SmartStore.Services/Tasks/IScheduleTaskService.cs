using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Services.Tasks
{
    /// <summary>
    /// Task service interface
    /// </summary>
    public partial interface IScheduleTaskService
    {
        /// <summary>
        /// Deletes a task
        /// </summary>
        /// <param name="task">Task</param>
        void DeleteTask(ScheduleTask task);

        /// <summary>
        /// Gets a task
        /// </summary>
        /// <param name="taskId">Task identifier</param>
        /// <returns>Task</returns>
        ScheduleTask GetTaskById(int taskId);

        /// <summary>
        /// Gets a task by its type
        /// </summary>
        /// <param name="type">Task type</param>
        /// <returns>Task</returns>
        ScheduleTask GetTaskByType(string type);

        /// <summary>
        /// Gets all tasks
        /// </summary>
		/// <param name="includeDisabled">A value indicating whether to load disabled tasks also</param>
        /// <returns>Tasks</returns>
        IList<ScheduleTask> GetAllTasks(bool includeDisabled = false);

        /// <summary>
        /// Gets all pending tasks
        /// </summary>
        /// <returns>Tasks</returns>
        Task<IList<ScheduleTask>> GetPendingTasksAsync();

        /// <summary>
        /// Inserts a task
        /// </summary>
        /// <param name="task">Task</param>
        void InsertTask(ScheduleTask task);

        /// <summary>
        /// Updates the task
        /// </summary>
        /// <param name="task">Task</param>
        void UpdateTask(ScheduleTask task);

        /// <summary>
        /// Inserts a new task definition to the database or returns an existing one
        /// </summary>
        /// <typeparam name="T">The concrete implementation of the task</typeparam>
        /// <param name="action">Wraps the newly created <see cref="ScheduleTask"/> instance</param>
        /// <returns>A newly created or existing task instance</returns>
        /// <remarks>
        /// This method does NOT update an already exising task
        /// </remarks>
        ScheduleTask GetOrAddTask<T>(Action<ScheduleTask> newAction) where T : ITask;

        /// <summary>
        /// Calculates - according to their cron expressions - all task future schedules
        /// and saves them to the database.
        /// </summary>
        /// <param name="isAppStart">When <c>true</c>, determines stale tasks and fixes their states to idle.</param>
        void CalculateFutureSchedules(IEnumerable<ScheduleTask> tasks, bool isAppStart = false);

        /// <summary>
        /// Calculates the next schedule according to the task's cron expression
        /// </summary>
        /// <param name="task">ScheduleTask</param>
        /// <returns>The next schedule or <c>null</c> if the task is disabled</returns>
        DateTime? GetNextSchedule(ScheduleTask task);

        #region Schedule task history

        /// <summary>
        /// Gets a list of <see cref="ScheduleTaskHistory"/> entries.
        /// </summary>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="taskId">Task identifier.</param>
        /// <param name="forCurrentMachine">Filter by current machine.</param>
        /// <param name="lastEntryOnly">Return the last history entry only.</param>
        /// <param name="isRunning">Filter by running entries.</param>
        /// <returns><see cref="ScheduleTaskHistory"/> entries.</returns>
        IPagedList<ScheduleTaskHistory> GetHistoryEntries(
            int pageIndex,
            int pageSize,
            int taskId = 0,
            bool forCurrentMachine = false,
            bool lastEntryOnly = false,
            bool? isRunning = null);

        /// <summary>
        /// Gets a list of <see cref="ScheduleTaskHistory"/> entries.
        /// </summary>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="task">Schedule task entity.</param>
        /// <param name="forCurrentMachine">Filter by current machine.</param>
        /// <param name="isRunning">Filter by running entries.</param>
        /// <returns><see cref="ScheduleTaskHistory"/> entries.</returns>
        IPagedList<ScheduleTaskHistory> GetHistoryEntries(
            int pageIndex,
            int pageSize,
            ScheduleTask task,
            bool forCurrentMachine = false,
            bool? isRunning = null);

        /// <summary>
        /// Get last history entry by task identifier.
        /// </summary>
        /// <param name="taskId">Task identifier.</param>
        /// <param name="isRunning">Filter by running entries.</param>
        /// <returns><see cref="ScheduleTaskHistory"/> entry.</returns>
        ScheduleTaskHistory GetLastHistoryEntryByTaskId(int taskId, bool? isRunning = null);

        /// <summary>
        /// Get last history entry for a task.
        /// </summary>
        /// <param name="task">Schedule task entity.</param>
        /// <param name="isRunning">Filter by running entries.</param>
        /// <returns><see cref="ScheduleTaskHistory"/> entry.</returns>
        ScheduleTaskHistory GetLastHistoryEntryByTask(ScheduleTask task, bool? isRunning = null);

        /// <summary>
        /// Get a history entry by identifier.
        /// </summary>
        /// <param name="id">History entry identifier.</param>
        /// <returns><see cref="ScheduleTaskHistory"/> entry.</returns>
        ScheduleTaskHistory GetHistoryEntryById(int id);

        /// <summary>
        /// Inserts a <see cref="ScheduleTaskHistory"/> entry.
        /// </summary>
        /// <param name="historyEntry"><see cref="ScheduleTaskHistory"/> entry.</param>
        void InsertHistoryEntry(ScheduleTaskHistory historyEntry);

        /// <summary>
        /// Updates a <see cref="ScheduleTaskHistory"/> entry.
        /// </summary>
        /// <param name="historyEntry"><see cref="ScheduleTaskHistory"/> entry.</param>
        void UpdateHistoryEntry(ScheduleTaskHistory historyEntry);

        /// <summary>
        /// Delete a <see cref="ScheduleTaskHistory"/> entry.
        /// </summary>
        /// <param name="historyEntry"><see cref="ScheduleTaskHistory"/> entry.</param>
        void DeleteHistoryEntry(ScheduleTaskHistory historyEntry);

        /// <summary>
        /// Deletes old <see cref="ScheduleTaskHistory"/> entries.
        /// </summary>
        /// <returns>Number of deleted entries.</returns>
        int DeleteHistoryEntries();

        #endregion
    }

    public static class IScheduleTaskServiceExtensions
    {
        public static ScheduleTask GetTaskByType<T>(this IScheduleTaskService service) where T : ITask
        {
            return service.GetTaskByType(typeof(T));
        }

        public static ScheduleTask GetTaskByType(this IScheduleTaskService service, Type taskType)
        {
            Guard.NotNull(taskType, nameof(taskType));

            var name = taskType.AssemblyQualifiedNameWithoutVersion();

            return service.GetTaskByType(name);
        }

        public static bool TryDeleteTask<T>(this IScheduleTaskService service) where T : ITask
        {
            var task = service.GetTaskByType(typeof(T));

            if (task != null)
            {
                service.DeleteTask(task);
                return true;
            }

            return false;
        }
    }
}
