using System;
using System.ComponentModel.DataAnnotations.Schema;
using SmartStore.Core.Data.Hooks;

namespace SmartStore.Core.Domain.Tasks
{
    [Hookable(false)]
    public class ScheduleTaskHistory : BaseEntity, ICloneable<ScheduleTaskHistory>
    {
        /// <summary>
        /// Gets or sets the schedule task identifier.
        /// </summary>
        public int ScheduleTaskId { get; set; }

        /// <summary>
        /// Gets or sets whether the task is running.
        /// </summary>
        [Index("IX_MachineName_IsRunning", 1)]
        public bool IsRunning { get; set; }

        /// <summary>
        /// Gets or sets the server machine name.
        /// </summary>
        [Index("IX_MachineName_IsRunning", 0)]
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the date when the task was started. It is also the date when this entry was created.
        /// </summary>
        [Index("IX_Started_Finished", 0)]
        public DateTime StartedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date when the task has been finished.
        /// </summary>
        [Index("IX_Started_Finished", 1)]
        public DateTime? FinishedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date when the task succeeded.
        /// </summary>
        public DateTime? SucceededOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the last error message.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the current percentual progress for a running task.
        /// </summary>
        public int? ProgressPercent { get; set; }

        /// <summary>
        /// Gets or sets the current progress message for a running task.
        /// </summary>
        public string ProgressMessage { get; set; }

        /// <summary>
        /// Gets or sets the schedule task.
        /// </summary>
        public virtual ScheduleTask ScheduleTask { get; set; }

        public ScheduleTaskHistory Clone()
        {
            var clone = (ScheduleTaskHistory)this.MemberwiseClone();
            clone.ScheduleTask = this.ScheduleTask.Clone();
            return clone;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
