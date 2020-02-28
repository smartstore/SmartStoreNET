using System;
using System.Collections.Generic;
using SmartStore.Core;

namespace SmartStore.Services.Media
{
    public interface IMediaTracker
    {
        IDisposable BeginScope(bool makeFilesTransientWhenOrphaned);
        
        void Track(BaseEntity entity, int mediaFileId);
        void Untrack(BaseEntity entity, int mediaFileId);
        
        void TrackMany(IEnumerable<MediaTrackAction> actions);
        void TrackMany(string albumName, IEnumerable<MediaTrackAction> actions, bool isMigration);

        int DeleteAllTracks(string albumName);
        void DetectAllTracks(string albumName, bool isMigration = false);
        bool TryGetTrackedPropertiesFor(Type forType, out IEnumerable<TrackedMediaProperty> properties);
    }
}
