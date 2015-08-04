using System;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Core.Domain
{
	public class ExportProfile : BaseEntity
	{
		public ExportProfile()
		{
			Limit = 100;
			BatchSize = 1000;
		}

		/// <summary>
		/// The name of the profile
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The name of the folder (file system)
		/// </summary>
		public string FolderName { get; set; }

		/// <summary>
		/// The system name of the export provider
		/// </summary>
		public string ProviderSystemName { get; set; }

		/// <summary>
		/// Whether the export profile is enabled
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// The scheduling task identifier
		/// </summary>
		public int SchedulingTaskId { get; set; }

		/// <summary>
		/// The profile GUID is the folder name in the file system
		/// </summary>
		public Guid ProfileGuid { get; set; }

		/// <summary>
		/// XML with filtering information
		/// </summary>
		public string Filtering { get; set; }

		/// <summary>
		/// The number of records to be skipped
		/// </summary>
		public int Offset { get; set; }

		/// <summary>
		/// How many records to be loaded per database round-trip
		/// </summary>
		public int Limit { get; set; }

		/// <summary>
		/// The maximum number of records of one processed batch
		/// </summary>
		public int BatchSize { get; set; }

		/// <summary>
		/// Whether to start a separate run-through for each store
		/// </summary>
		public bool PerStore { get; set; }

		/// <summary>
		/// The scheduling task
		/// </summary>
		public virtual ScheduleTask ScheduleTask { get; set; }
	}
}
