
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using SmartStore.Core.Data.Hooks;

namespace SmartStore.Core.Domain.Tasks
{
    [DebuggerDisplay("{Name} (Type: {Type})")]
	[Hookable(false)]
	public class ScheduleTask : BaseEntity, ICloneable<ScheduleTask>
    {
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
        /// Gets or sets the value indicating whether a task should be stopped on some error
        /// </summary>
        public bool StopOnError { get; set; }

        [Index("IX_NextRun_Enabled", 0)]
        public DateTime? NextRunUtc { get; set; }

		[Index("IX_LastStart_LastEnd", 0)]
        public DateTime? LastStartUtc { get; set; }

		[Index("IX_LastStart_LastEnd", 1)]
        public DateTime? LastEndUtc { get; set; }

        public DateTime? LastSuccessUtc { get; set; }

		public string LastError { get; set; }

        public bool IsHidden { get; set; }

		/// <summary>
		/// Gets or sets a value indicating the current percentual progress for a running task
		/// </summary>
		public int? ProgressPercent { get; set; }

		/// <summary>
		/// Gets or sets the current progress message for a running task
		/// </summary>
		public string ProgressMessage { get; set; }

		/// <summary>
		/// Concurrency Token
		/// </summary>
		[Timestamp]
		public byte[] RowVersion { get; set; }

		/// <summary>
		/// Gets a value indicating whether a task is running
		/// </summary>
		public bool IsRunning
		{
			get
			{
				var result = LastStartUtc.HasValue && LastStartUtc.Value > LastEndUtc.GetValueOrDefault();
				return result;
			}
		}

		/// <summary>
		/// Gets a value indicating whether a task is scheduled for execution (Enabled = true and NextRunUtc &lt;= UtcNow )
		/// </summary>
		public bool IsPending
		{
			get
			{
				var result = Enabled && NextRunUtc.HasValue && NextRunUtc <= DateTime.UtcNow;
				return result;
			}
		}

		public ScheduleTask Clone()
		{
			return (ScheduleTask)this.MemberwiseClone();
		}

		object ICloneable.Clone()
		{
			return this.MemberwiseClone();
		}
	}
}
