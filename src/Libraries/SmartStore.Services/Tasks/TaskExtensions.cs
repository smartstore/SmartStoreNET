using System.Linq;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Services.Tasks
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Gets the running history entry for a scheduled task.
        /// </summary>
        /// <param name="task">Scheduled task.</param>
        /// <param name="machineName">Machine name, can be <c>null</c>.</param>
        /// <returns>Schedule task history entry.</returns>
        public static ScheduleTaskHistory GetRunningHistoryEntry(this ScheduleTask task, string machineName)
        {
            Guard.NotNull(task, nameof(task));

            var query = task.ScheduleTaskHistory.Where(x => x.IsRunning);
            if (machineName.HasValue())
            {
                query = query.Where(x => x.MachineName == machineName);
            }

            var entry = query
                .OrderByDescending(x => x.StartedOnUtc)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            return entry;
        }
    }
}
