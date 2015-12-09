using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Core.Domain
{
	public class ImportProfile : BaseEntity
	{
		/// <summary>
		/// The name of the profile
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The name of the folder (file system)
		/// </summary>
		public string FolderName { get; set; }

		/// <summary>
		/// The type of the entity
		/// </summary>
		public string EntityType { get; set; }

		/// <summary>
		/// Whether the profile is enabled
		/// </summary>
		public bool Enabled { get; set; }

		/// <summary>
		/// Number of records to bypass
		/// </summary>
		public int Skip { get; set; }

		/// <summary>
		/// Maximum number of records to return
		/// </summary>
		public int Take { get; set; }

		/// <summary>
		/// File type specific configuration
		/// </summary>
		public string FileTypeConfiguration { get; set; }

		/// <summary>
		/// Mapping of import columns
		/// </summary>
		public string ColumnMapping { get; set; }

		/// <summary>
		/// Whether to delete import files after import
		/// </summary>
		public bool Cleanup { get; set; }

		/// <summary>
		/// The scheduling task identifier
		/// </summary>
		public int SchedulingTaskId { get; set; }

		/// <summary>
		/// The scheduling task
		/// </summary>
		public virtual ScheduleTask ScheduleTask { get; set; }
	}
}
