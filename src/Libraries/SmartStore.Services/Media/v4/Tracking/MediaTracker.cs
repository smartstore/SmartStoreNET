using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core;
using System.Reflection;

namespace SmartStore.Services.Media
{
    public class MediaTracker : IMediaTracker
    {
        private readonly IAlbumService _albumService;
        private readonly IDbContext _dbContext;

        public MediaTracker(IAlbumService albumService, IDbContext dbContext)
        {
            _albumService = albumService;
            _dbContext = dbContext;
        }

        public void Track(BaseEntity entity, int mediaFileId)
        {
            TrackSingle(entity, mediaFileId, MediaTrackOperation.Track);
        }

        public void Untrack(BaseEntity entity, int mediaFileId)
        {
            TrackSingle(entity, mediaFileId, MediaTrackOperation.Untrack);
        }

        protected virtual void TrackSingle(BaseEntity entity, int mediaFileId, MediaTrackOperation operation)
        {
            Guard.NotNull(entity, nameof(entity));

            if (mediaFileId < 1)
                return;

            var file = _dbContext.Set<MediaFile>().Find(mediaFileId);
            if (file != null)
            {
                var albumName = _albumService.FindAlbum(file)?.Value.Name;
                if (albumName.IsEmpty())
                {
                    // We cannot track files that are not part of any album.
                    return;
                }

                var track = new MediaTrack
                {
                    EntityId = entity.Id,
                    EntityName = entity.GetEntityName(),
                    MediaFileId = mediaFileId,
                    Album = albumName
                };

                if (operation == MediaTrackOperation.Track)
                {
                    file.Tracks.Add(track);
                }
                else
                {
                    file.Tracks.Remove(track);
                }

                _dbContext.SaveChanges();
            }
        }


        public void RemoveAllTracks(string albumName)
        {
            throw new NotImplementedException();
        }

        public void DetectAllTracks(string albumName, bool isMigration = false)
        {
            Guard.NotEmpty(albumName, nameof(albumName));

            // load corresponding detector provider for current album...
            var provider = _albumService.LoadAlbumProvider(albumName) as IMediaTrackDetector;

            if (provider == null)
            {
                throw new InvalidOperationException($"The album '{albumName}' does not exist or does not support track detection.");
            }

            // >>>>> DO detection (potentially a very long process)...
            var tracks = provider.DetectAllTracks(albumName);

            // (perf) batch result data...
            foreach (var batch in tracks.Slice(500))
            {
                // process the batch
                TrackManyCore(albumName, batch, isMigration);
            }
        }

        public void TrackMany(string albumName, IEnumerable<MediaTrackAction> actions, bool isMigration = false)
        {
            TrackManyCore(albumName, actions, isMigration);
        }

        protected virtual void TrackManyCore(string albumName, IEnumerable<MediaTrackAction> actions, bool isMigration)
        {
            Guard.NotEmpty(albumName, nameof(albumName));
            Guard.NotNull(actions, nameof(actions));

            if (!actions.Any())
                return;

            var ctx = _dbContext;

            using (var scope = new DbContextScope(ctx, validateOnSave: false, hooksEnabled: false, autoDetectChanges: false))
            {
                // Get the id for an album (necessary later to set FolderId)...
                var albumId = _albumService.GetAlbumIdByName(albumName);

                // Get distinct ids of all detected files...
                var mediaFileIds = actions.Select(x => x.MediaFileId).Distinct().ToArray();

                // fetch these files from database...
                var query = ctx.Set<MediaFile>().Include(x => x.Tracks).Where(x => mediaFileIds.Contains(x.Id));
                if (isMigration)
                {
                    query = query.Where(x => x.Version == 1);
                }
                var files = query.ToDictionary(x => x.Id);

                // for each media file relation to an entity...
                foreach (var action in actions)
                {
                    // fetch the file from local dictionary by its id...
                    if (files.TryGetValue(action.MediaFileId, out var file))
                    {
                        if (isMigration)
                        {
                            // set album id as folder id (during initial migration there are no sub-folders)
                            file.FolderId = albumId;

                            // remember that we processed tracks for this file already
                            file.Version = 2;
                        }

                        var track = action.ToTrack();

                        // add or remove the track from file
                        if (action.Operation == MediaTrackOperation.Track)
                        {
                            file.Tracks.Add(track);
                        }
                        else
                        {
                            file.Tracks.Remove(track);
                        }
                    }
                }

                // Save whole batch to database
                ctx.SaveChanges();

                // Breathe
                ctx.DetachEntities(x => x is MediaFile || x is MediaTrack, false);
            }
        }

        public bool TryGetTrackedPropertiesFor(Type forType, out PropertyInfo[] properties)
        {
            throw new NotImplementedException();
        }
    }
}
