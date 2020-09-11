using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public static class IMediaTrackerExtensions
    {
        public static void Track<T>(this IMediaTracker tracker, T entity, Expression<Func<T, int>> path) where T : BaseEntity, new()
        {
            Guard.NotNull(path, nameof(path));

            var getter = path.CompileFast();
            var mediaFileId = getter.Invoke(entity);
            if (mediaFileId > 0)
                tracker.Track(entity, mediaFileId, path.ExtractPropertyInfo().Name);
        }

        public static void Track<T>(this IMediaTracker tracker, T entity, Expression<Func<T, int?>> path) where T : BaseEntity, new()
        {
            Guard.NotNull(path, nameof(path));

            var getter = path.CompileFast();
            var mediaFileId = getter.Invoke(entity);
            if (mediaFileId > 0)
                tracker.Track(entity, mediaFileId.Value, path.ExtractPropertyInfo().Name);
        }

        public static void Untrack<T>(this IMediaTracker tracker, T entity, Expression<Func<T, int>> path) where T : BaseEntity, new()
        {
            Guard.NotNull(path, nameof(path));

            var getter = path.CompileFast();
            var mediaFileId = getter.Invoke(entity);
            if (mediaFileId > 0)
                tracker.Untrack(entity, mediaFileId, path.ExtractPropertyInfo().Name);
        }

        public static void Untrack<T>(this IMediaTracker tracker, T entity, Expression<Func<T, int?>> path) where T : BaseEntity, new()
        {
            Guard.NotNull(path, nameof(path));

            var getter = path.CompileFast();
            var mediaFileId = getter.Invoke(entity);
            if (mediaFileId > 0)
                tracker.Untrack(entity, mediaFileId.Value, path.ExtractPropertyInfo().Name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Track(this IMediaTracker tracker, MediaTrack track)
        {
            if (track == null)
                return;

            tracker.TrackMany(new[] { track });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateTracks(this IMediaTracker tracker, BaseEntity entity, int? prevMediaFileId, int? currentMediaFileId, string propertyName)
        {
            UpdateTracks(tracker, entity.Id, entity.GetEntityName(), prevMediaFileId, currentMediaFileId, propertyName);
        }

        public static void UpdateTracks(this IMediaTracker tracker, int entityId, string entityName, int? prevMediaFileId, int? currentMediaFileId, string propertyName)
        {
            var tracks = new List<MediaTrack>(2);
            bool isModified = prevMediaFileId != currentMediaFileId;

            if (prevMediaFileId > 0 && isModified)
            {
                tracks.Add(new MediaTrack
                {
                    EntityId = entityId,
                    EntityName = entityName,
                    MediaFileId = prevMediaFileId.Value,
                    Property = propertyName,
                    Operation = MediaTrackOperation.Untrack
                });
            }

            if (currentMediaFileId > 0 && isModified)
            {
                tracks.Add(new MediaTrack
                {
                    EntityId = entityId,
                    EntityName = entityName,
                    MediaFileId = currentMediaFileId.Value,
                    Property = propertyName,
                    Operation = MediaTrackOperation.Track
                });
            }

            if (tracks.Count > 0)
            {
                tracker.TrackMany(tracks);
            }
        }
    }
}
