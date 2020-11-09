using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using SmartStore.Core.Data.Hooks;

namespace SmartStore.Core.Domain.Tasks
{
    public enum TaskPriority
    {
        Low = -1,
        Normal = 0,
        High = 1
    }

    [DebuggerDisplay("{Name} (Type: {Type})")]
    [Hookable(false)]
    public class ScheduleTask : BaseEntity, ICloneable<ScheduleTask>
    {
        private ICollection<ScheduleTaskHistory> _scheduleTaskHistory;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the task alias (an optional key for advanced customization)
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets the CRON expression used to calculate future schedules
        /// </summary>
        public string CronExpression { get; set; }

        /// <summary>
        /// Gets or sets the type of appropriate ITask class
        /// </summary>
		[Index("IX_Type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether a task is enabled
        /// </summary>
        [Index("IX_NextRun_Enabled", 1)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the task priority. Tasks with higher priority run first when multiple tasks are pending.
        /// </summary>
        public TaskPriority Priority { get; set; } = TaskPriority.Normal;

        /// <summary>
        /// Gets or sets the value indicating whether a task should be stopped on some error
        /// </summary>
        public bool StopOnError { get; set; }

        [Index("IX_NextRun_Enabled", 0)]
        public DateTime? NextRunUtc { get; set; }

        /// <summary>
        /// Indicates whether the task is hidden.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Indicates whether the task is executed decidedly on each machine of a web farm.
        /// </summary>
        public bool RunPerMachine { get; set; }

        /// <summary>
        /// Gets a value indicating whether a task is scheduled for execution (Enabled = true and NextRunUtc &lt;= UtcNow and is not running).
        /// </summary>
        public bool IsPending
        {
            get
            {
                var result = Enabled && NextRunUtc.HasValue && NextRunUtc <= DateTime.UtcNow && (LastHistoryEntry == null || !LastHistoryEntry.IsRunning);
                return result;
            }
        }

        public ScheduleTaskHistory LastHistoryEntry { get; set; }

        /// <summary>
        /// Gets or sets the schedule task history.
        /// </summary>
        public virtual ICollection<ScheduleTaskHistory> ScheduleTaskHistory
        {
            get => _scheduleTaskHistory ?? (_scheduleTaskHistory = new HashSet<ScheduleTaskHistory>());
            protected set => _scheduleTaskHistory = value;
        }

        public ScheduleTask Clone()
        {
            var task = new ScheduleTask
            {
                Name = Name,
                Alias = Alias,
                CronExpression = CronExpression,
                Type = Type,
                Enabled = Enabled,
                StopOnError = StopOnError,
                NextRunUtc = NextRunUtc,
                IsHidden = IsHidden,
                RunPerMachine = RunPerMachine
            };

            return task;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
