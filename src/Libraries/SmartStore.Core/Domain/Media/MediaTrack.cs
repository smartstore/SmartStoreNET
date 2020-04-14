using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using SmartStore.Core.Data.Hooks;
using SmartStore.Utilities;

namespace SmartStore.Core.Domain.Media
{
    public enum MediaTrackOperation
    {
        Track,
        Untrack
    }

    [DataContract]
    [Hookable(false)]
    public partial class MediaTrack : BaseEntity, IEquatable<MediaTrack>
    {
        private int _mediaFileId;
        private int _entityId;
        private string _entityName;
        private string _property;
        private int? _hashCode;

        /// <summary>
        /// Gets or sets the media file identifier.
        /// </summary>
        [DataMember]
        [Index("IX_MediaTrack_Composite", IsUnique = true, Order = 0)]
        public int MediaFileId
        {
            get => _mediaFileId;
            set
            {
                _mediaFileId = value;
                _hashCode = null;
            }
        }

        /// <summary>
        /// Gets or sets the media file.
        /// </summary>
        public virtual MediaFile MediaFile { get; set; }

        /// <summary>
        /// Gets or sets the origin album system name.
        /// </summary>
        [DataMember]
        public string Album { get; set; }

        /// <summary>
        /// Gets or sets the related entity identifier.
        /// </summary>
        [DataMember]
        [Index("IX_MediaTrack_Composite", IsUnique = true, Order = 1)]
        public int EntityId
        {
            get => _entityId;
            set
            {
                _entityId = value;
                _hashCode = null;
            }
        }

        /// <summary>
        /// Gets or sets the related entity set name.
        /// </summary>
        [DataMember]
        [Index("IX_MediaTrack_Composite", IsUnique = true, Order = 2)]
        public string EntityName
        {
            get => _entityName;
            set
            {
                _entityName = value;
                _hashCode = null;
            }
        }

        /// <summary>
        /// Gets or sets the media file property name in the tracked entity.
        /// </summary>
        [DataMember]
        [Index("IX_MediaTrack_Composite", IsUnique = true, Order = 3)]
        public string Property
        {
            get => _property;
            set
            {
                _property = value;
                _hashCode = null;
            }
        }

        [NotMapped]
        public MediaTrackOperation Operation { get; set; }

        protected override bool Equals(BaseEntity other)
        {
            return ((IEquatable<MediaTrack>)this).Equals(other as MediaTrack);
        }

        bool IEquatable<MediaTrack>.Equals(MediaTrack other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return this.MediaFileId == other.MediaFileId
                && this.EntityId == other.EntityId
                && string.Equals(this.EntityName, other.EntityName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(this.Property, other.Property, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            if (_hashCode == null)
            {
                var combiner = HashCodeCombiner
                    .Start()
                    .Add(GetUnproxiedType().GetHashCode())
                    .Add(this.MediaFileId)
                    .Add(this.EntityId)
                    .Add(this.EntityName)
                    .Add(this.Property);

                _hashCode = combiner.CombinedHash;
            }

            return _hashCode.Value;
        }

        public override string ToString()
        {
            return $"MediaTrack (MediaFileId: {MediaFileId}, EntityName: {EntityName}, EntityId: {EntityId}, Property: {Property})";
        }
    }
}
