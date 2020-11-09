using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using SmartStore.Core.Data.Hooks;

namespace SmartStore.Core.Domain.DataExchange
{
    /// <summary>
    /// Holds info about a synchronization operation with an external system
    /// </summary>
    [DataContract]
    [Hookable(false)]
    public partial class SyncMapping : BaseEntity
    {
        public SyncMapping()
        {
            this.SyncedOnUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the entity identifier in SmartStore
        /// </summary>
        [Index("IX_SyncMapping_ByEntity", 0, IsUnique = true)]
        [DataMember]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the entity's key in the external application
        /// </summary>
        [Index("IX_SyncMapping_BySource", 0, IsUnique = true)]
        [DataMember]
        public string SourceKey { get; set; }

        /// <summary>
        /// Gets or sets a name representing the entity type
        /// </summary>
        [Index("IX_SyncMapping_ByEntity", 1, IsUnique = true)]
        [Index("IX_SyncMapping_BySource", 1, IsUnique = true)]
        [DataMember]
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets a name for the external application
        /// </summary>
        [Index("IX_SyncMapping_ByEntity", 2, IsUnique = true)]
        [Index("IX_SyncMapping_BySource", 2, IsUnique = true)]
        [DataMember]
        public string ContextName { get; set; }

        /// <summary>
        /// Gets or sets an optional content hash reflecting the source model at the time of last sync
        /// </summary>
        [DataMember]
        public string SourceHash { get; set; }

        /// <summary>
        /// Gets or sets a custom integer value
        /// </summary>
        [DataMember]
        public int? CustomInt { get; set; }

        /// <summary>
        /// Gets or sets a custom string value
        /// </summary>
        [DataMember]
        public string CustomString { get; set; }

        /// <summary>
        /// Gets or sets a custom bool value
        /// </summary>
        [DataMember]
        public bool? CustomBool { get; set; }

        /// <summary>
        /// Gets or sets the date of the last sync operation
        /// </summary>
        [DataMember]
        public DateTime SyncedOnUtc { get; set; }

    }
}
