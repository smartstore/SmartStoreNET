using System;
using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core.Data.Hooks;

namespace SmartStore.Core.Domain.Tasks
{
    [Hookable(false)]
    public class ScheduleTaskHistory : BaseEntity
    {
        public int ScheduleTaskId { get; set; }

        [Index("IX_MachineName")]
        public string MachineName { get; set; }

        [Index("IX_NextRun")]
        public DateTime? NextRunUtc { get; set; }

        [Index("IX_Started_Finished", 0)]
        public DateTime? StartedOnUtc { get; set; }

        [Index("IX_Started_Finished", 1)]
        public DateTime? FinishedOnUtc { get; set; }

        public DateTime? SucceededOnUtc { get; set; }

        public string Error { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the current percentual progress for a running task
        /// </summary>
        public int? ProgressPercent { get; set; }

        /// <summary>
        /// Gets or sets the current progress message for a running task
        /// </summary>
        public string ProgressMessage { get; set; }

        /// <summary>
        /// Gets or sets the schedule task.
        /// </summary>
        public virtual ScheduleTask ScheduleTask { get; set; }
    }
}
