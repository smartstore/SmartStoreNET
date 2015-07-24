using System;

namespace SmartStore.Services.Tasks
{
	/// <summary>
	/// Contains information about a task's progress
	/// </summary>
	[Serializable]
	public class TaskProgressInfo : ICloneable<TaskProgressInfo>
	{
		/// <summary>
		/// The ScheduleTask identifier
		/// </summary>
		public int ScheduleTaskId { get; set; }

		/// <summary>
		/// The type of the concrete <see cref="ITask"/> implementation currently running
		/// </summary>
		public Type TaskType { get; set; }

		/// <summary>
		/// Gets the current progress percentage of the task
		/// </summary>
		public float? Progress { get; set; }

		/// <summary>
		/// Gets the current progress message of the task
		/// </summary>
		public string Message { get; set; }

		public string StateName
		{
			get
			{
				return ScheduleTaskId.ToString();
			}
		}

		public TaskProgressInfo Clone()
		{
			return (TaskProgressInfo)this.MemberwiseClone();
		}

		object ICloneable.Clone()
		{
			return this.MemberwiseClone();
		}
	}
}
