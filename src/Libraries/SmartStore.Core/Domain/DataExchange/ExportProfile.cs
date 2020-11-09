using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Core.Domain
{
    public class ExportProfile : BaseEntity, ICloneable<ExportProfile>
    {
        private ICollection<ExportDeployment> _deployments;

        public ExportProfile()
        {
            Enabled = true;
            PerStore = true;
            Cleanup = true;
            EmailAccountId = 0;
        }

        /// <summary>
        /// The name of the profile
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The root path of the export folder
        /// </summary>
        public string FolderName { get; set; }

        /// <summary>
        /// The pattern for file names
        /// </summary>
        public string FileNamePattern { get; set; }

        /// <summary>
        /// The system name of the profile
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// The system name of the export provider
        /// </summary>
        public string ProviderSystemName { get; set; }

        /// <summary>
        /// Whether the profile is an unremovable system profile
        /// </summary>
        public bool IsSystemProfile { get; set; }

        /// <summary>
        /// Whether the export profile is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Whether the export related data
        /// </summary>
        public bool ExportRelatedData { get; set; }

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
        /// XML with provider specific configuration data
        /// </summary>
        public string ProviderConfigData { get; set; }

        /// <summary>
        /// XML with information about the last export
        /// </summary>
        public string ResultInfo { get; set; }

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
        /// Email Account identifier used to send a notification message an export completes
        /// </summary>
        public int EmailAccountId { get; set; }

        /// <summary>
        /// Email addresses where to send the notification message
        /// </summary>
        public string CompletedEmailAddresses { get; set; }

        /// <summary>
        /// Whether to combine and compress the export files in a ZIP archive
        /// </summary>
        public bool CreateZipArchive { get; set; }

        /// <summary>
        /// Whether to delete unneeded files after deployment
        /// </summary>
        public bool Cleanup { get; set; }


        /// <summary>
        /// The scheduling task
        /// </summary>
        public virtual ScheduleTask ScheduleTask { get; set; }

        /// <summary>
        /// Gets or sets export deployments
        /// </summary>
        public virtual ICollection<ExportDeployment> Deployments
        {
            get => _deployments ?? (_deployments = new HashSet<ExportDeployment>());
            set => _deployments = value;
        }

        public ExportProfile Clone()
        {
            var profile = new ExportProfile
            {
                Name = this.Name,
                FolderName = null,
                FileNamePattern = this.FileNamePattern,
                ProviderSystemName = this.ProviderSystemName,
                Enabled = this.Enabled,
                SchedulingTaskId = 0,
                Filtering = this.Filtering,
                Projection = this.Projection,
                ProviderConfigData = this.ProviderConfigData,
                ResultInfo = null,
                Offset = this.Offset,
                Limit = this.Limit,
                BatchSize = this.BatchSize,
                PerStore = this.PerStore,
                EmailAccountId = this.EmailAccountId,
                CompletedEmailAddresses = this.CompletedEmailAddresses,
                CreateZipArchive = this.CreateZipArchive,
                Cleanup = this.Cleanup
            };
            return profile;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
