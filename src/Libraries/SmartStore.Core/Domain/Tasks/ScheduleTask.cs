
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace SmartStore.Core.Domain.Tasks
{
    [DebuggerDisplay("{Name} (Type: {Type})")]
    public class ScheduleTask : BaseEntity
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
        /// Gets or sets the run period (in seconds)
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// Gets or sets the type of appropriate ITask class
        /// </summary>
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

        public DateTime? LastStartUtc { get; set; }

        public DateTime? LastEndUtc { get; set; }

        public DateTime? LastSuccessUtc { get; set; }

		public string LastError { get; set; }

        public bool IsHidden { get; set; }

		/// <summary>
		/// Gets a value indicating whether a task is running
		/// </summary>
		public bool IsRunning
		{
			get
			{
				var result = (LastStartUtc.HasValue && LastStartUtc.Value > LastEndUtc.GetValueOrDefault());
				return result;
			}
		}
    }
}
