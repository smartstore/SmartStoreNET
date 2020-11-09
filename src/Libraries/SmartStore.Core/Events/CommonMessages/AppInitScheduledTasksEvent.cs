using System.Collections.Generic;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Core.Events
{
    /// <summary>
    /// to initialize scheduled tasks in Application_Start
    /// </summary>
    public class AppInitScheduledTasksEvent
    {
        public IList<ScheduleTask> ScheduledTasks { get; set; }
    }
}
