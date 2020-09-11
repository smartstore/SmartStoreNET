using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Media
{
    /// <summary>
    /// Represents a download
    /// </summary>
    [DataContract]
    public partial class Download : BaseEntity//, ITransient
    {
        /// <summary>
        /// Gets or sets a GUID
        /// </summary>
        [DataMember]
        [Index]
        public Guid DownloadGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DownloadUrl property should be used
        /// </summary>
		[DataMember]
        public bool UseDownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets a download URL
        /// </summary>
		[DataMember]
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity transient/preliminary
        /// </summary>
        [DataMember]
        [Index("IX_UpdatedOn_IsTransient", 1)]
        public bool IsTransient { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        [DataMember]
        [Index("IX_UpdatedOn_IsTransient", 0)]
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the media file identifier
        /// </summary>
        [DataMember]
        public int? MediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the media file
        /// </summary>
        [DataMember]
        public virtual MediaFile MediaFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the corresponding entity id
        /// </summary>
        [DataMember]
        [Index("IX_EntityId_EntityName", 0)]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the corresponding entity name
        /// </summary>
        [DataMember]
        [Index("IX_EntityId_EntityName", 1)]
        [StringLength(100)]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets a value the verion info
        /// </summary>
        [DataMember]
        [StringLength(30)]
        public string FileVersion { get; set; }

        /// <summary>
        /// Gets or sets a value which contains information about changes of the current download version
        /// </summary>
        [DataMember]
        public string Changelog { get; set; }
    }
}
