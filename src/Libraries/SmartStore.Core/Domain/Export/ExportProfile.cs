using System;
using SmartStore.Core.Domain.Export;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Core.Domain
{
	public class ExportProfile : BaseEntity
	{
		/// <summary>
		/// The name of the profile
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The system name of the export provider
		/// </summary>
		public string ProviderSystemName { get; set; }

		/// <summary>
		/// Whether the export profile is enabled
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// The export file type identifier
		/// </summary>
		public int FileTypeId { get; set; }

		/// <summary>
		/// The export file type
		/// </summary>
		public ExportFileType FileType
		{
			get
			{
				return (ExportFileType)FileTypeId;
			}
			set
			{
				FileTypeId = (int)value;
			}
		}

		/// <summary>
		/// The scheduling task identifier
		/// </summary>
		public int SchedulingTaskId { get; set; }

		/// <summary>
		/// The profile GUID is the folder name in the file system
		/// </summary>
		public Guid ProfileGuid { get; set; }

		/// <summary>
		/// XML with partioning information
		/// </summary>
		public string Partitioning { get; set; }

		/// <summary>
		/// XML with filtering information
		/// </summary>
		public string Filtering { get; set; }

		/// <summary>
		/// Last start time the profile has been executed
		/// </summary>
		public DateTime? LastExecutionStartUtc { get; set; }

		/// <summary>
		/// /// Last end time the profile has been executed
		/// </summary>
		public DateTime? LastExecutionEndUtc { get; set; }

		/// <summary>
		/// Message of the last execution
		/// </summary>
		public string LastExecutionMessage { get; set; }

		/// <summary>
		/// The scheduling task
		/// </summary>
		public virtual ScheduleTask ScheduleTask { get; set; }
	}
}
