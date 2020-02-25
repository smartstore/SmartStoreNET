using System;
using SmartStore.Core.Domain.Media;
using SmartStore.Utilities;

namespace SmartStore.Services.Media
{
    public class MediaTrackAction : IEquatable<MediaTrackAction>
    {
        public string EntityName { get; set; }

        public int EntityId { get; set; }

        public int MediaFileId { get; set; }

        public string Album { get; set; }

        public MediaTrackOperation Operation { get; set; }

        public MediaTrack ToTrack()
        {
            return new MediaTrack
            {
                EntityName = EntityName,
                EntityId = EntityId,
                MediaFileId = MediaFileId,
                Album = Album
            };
        }

        public override bool Equals(object other)
        {
            return ((IEquatable<MediaTrackAction>)this).Equals(other as MediaTrackAction);
        }

        bool IEquatable<MediaTrackAction>.Equals(MediaTrackAction other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return this.MediaFileId == other.MediaFileId
                && this.EntityId == other.EntityId
                && this.EntityName.Equals(other.EntityName, StringComparison.OrdinalIgnoreCase)
                && this.Operation == other.Operation;
        }

        public override int GetHashCode()
        {
            var combiner = HashCodeCombiner
                .Start()
                .Add(GetType().GetHashCode())
                .Add(this.MediaFileId)
                .Add(this.EntityId)
                .Add(this.EntityName)
                .Add(this.Operation);

            return combiner.CombinedHash;
        }
    }

    public enum MediaTrackOperation
    {
        Track,
        Untrack
    }
}
