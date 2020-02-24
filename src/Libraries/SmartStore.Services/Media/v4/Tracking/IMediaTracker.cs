using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core;

namespace SmartStore.Services.Media
{
    public interface IMediaTracker
    {
        void Track(BaseEntity entity, int mediaFileId);
        void Untrack(BaseEntity entity, int mediaFileId);
        void TrackMany(string albumName, IEnumerable<MediaTrackAction> actions, bool isMigration);

        void RemoveAllTracks(string albumName);
        void DetectAllTracks(string albumName, bool isMigration = false);
        bool TryGetTrackedPropertiesFor(Type forType, out PropertyInfo[] properties);
    }
}
