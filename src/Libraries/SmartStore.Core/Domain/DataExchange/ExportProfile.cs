using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Core.Domain
{
	public class ExportProfile : BaseEntity
	{
		private ICollection<ExportDeployment> _deployments;

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
		/// XML with filtering information
		/// </summary>
		public string Filtering { get; set; }

		/// <summary>
		/// XML with projection information
		/// </summary>
		public string Projection { get; set; }

		/// <summary>
		/// Provider specific configuration data
		/// </summary>
		public string ProviderConfigData { get; set; }

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
		/// Whether to combine and compress the export files in a ZIP archive
		/// </summary>
		public bool CreateZipArchive { get; set; }

		/// <summary>
		/// Email addresses (semicolon separated) where to send a notification message of the completion of the export
		/// </summary>
		public string CompletedEmailAddresses { get; set; }

		/// <summary>
		/// The scheduling task
		/// </summary>
		public virtual ScheduleTask ScheduleTask { get; set; }

		/// <summary>
		/// Gets or sets export deployments
		/// </summary>
		public virtual ICollection<ExportDeployment> Deployments
		{
			get { return _deployments ?? (_deployments = new HashSet<ExportDeployment>()); }
			set { _deployments = value; }
		}
	}
}
