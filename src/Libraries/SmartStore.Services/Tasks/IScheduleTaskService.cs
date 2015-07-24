using System.Collections.Generic;
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
        IList<ScheduleTask> GetPendingTasks();

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
		/// Calculates - according to their intervals - all task next run times
		/// and saves them to the database.
		/// </summary>
		/// <param name="isAppStart">When <c>true</c>, determines stale tasks and fixes their states to idle.</param>
		void CalculateNextRunTimes(IEnumerable<ScheduleTask> tasks, bool isAppStart = false);
    }
}
