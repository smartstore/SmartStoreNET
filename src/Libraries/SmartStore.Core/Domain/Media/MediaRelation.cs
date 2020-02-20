using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SmartStore.Core.Domain.Media
{
    [DataContract]
    public partial class MediaRelation : BaseEntity
    {
        /// <summary>
        /// Gets or sets the media file identifier.
        /// </summary>
        [DataMember]
        public int MediaFileId { get; set; }

        /// <summary>
        /// Gets or sets the media file.
        /// </summary>
        public virtual MediaFile MediaFile { get; set; }

        /// <summary>
        /// Gets or sets the related entity identifier.
        /// </summary>
        [DataMember]
        [Index("IX_MediaRelation_EntityIdName", 0)]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the related entity set name.
        /// </summary>
        [DataMember]
        [Index("IX_MediaRelation_EntityIdName", 1)]
        public string EntityName { get; set; }
    }
}
