using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public interface IMediaTracker
    {
        IDisposable BeginScope(bool makeFilesTransientWhenOrphaned);

        void Track(BaseEntity entity, int mediaFileId, string propertyName);
        void Untrack(BaseEntity entity, int mediaFileId, string propertyName);

        void Track<TSetting>(TSetting settings, int? prevMediaFileId, Expression<Func<TSetting, int>> path) where TSetting : ISettings, new();
        void Track<TSetting>(TSetting settings, int? prevMediaFileId, Expression<Func<TSetting, int?>> path) where TSetting : ISettings, new();

        void TrackMany(IEnumerable<MediaTrack> tracks);
        void TrackMany(string albumName, IEnumerable<MediaTrack> tracks, bool isMigration = false);

        int DeleteAllTracks(string albumName);
        void DetectAllTracks(string albumName, bool isMigration = false);
        bool TryGetTrackedPropertiesFor(Type forType, out IEnumerable<TrackedMediaProperty> properties);
    }
}
