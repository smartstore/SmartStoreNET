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
using SmartStore.Core.Caching;
using SmartStore.Collections;
using Autofac.Features.Indexed;
using SmartStore.Utilities;

namespace SmartStore.Services.Media
{
    public class MediaTracker : IMediaTracker
    {
        internal const string TrackedPropertiesKey = "media:trackedprops:all";

        private readonly ICacheManager _cache;
        private readonly IDbContext _dbContext;
        private readonly IAlbumRegistry _albumRegistry;
        private readonly IFolderService _folderService;
        private readonly IIndex<Type, IAlbumProvider> _albumProviderFactory;

        private bool _makeFilesTransientWhenOrphaned;

        public MediaTracker(
            ICacheManager cache,
            IDbContext dbContext,
            IAlbumRegistry albumRegistry,
            IFolderService folderService,
            IIndex<Type, IAlbumProvider> albumProviderFactory)
        {
            _cache = cache;
            _dbContext = dbContext;
            _albumRegistry = albumRegistry;
            _folderService = folderService;
            _albumProviderFactory = albumProviderFactory;
        }

        public IDisposable BeginScope(bool makeFilesTransientWhenOrphaned)
        {
            var makeTransient = _makeFilesTransientWhenOrphaned;
            _makeFilesTransientWhenOrphaned = makeFilesTransientWhenOrphaned;

            return new ActionDisposable(() => _makeFilesTransientWhenOrphaned = makeTransient);
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

            if (mediaFileId < 1 || entity.IsTransientRecord())
                return;

            var file = _dbContext.Set<MediaFile>().Find(mediaFileId);
            if (file != null)
            {
                var albumName = _folderService.FindAlbum(file)?.Value.Name;
                if (albumName.IsEmpty())
                {
                    throw new InvalidOperationException("Cannot track a media file that is not assigned to any album.");
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

                if (file.Tracks.Count > 0)
                {
                    // A file with tracks can NEVER be transient
                    file.IsTransient = false;
                }
                else if (_makeFilesTransientWhenOrphaned)
                {
                    // But an untracked file can OPTIONALLY be transient
                    file.IsTransient = true;
                }

                _dbContext.SaveChanges();
            }
        }

        public void TrackMany(IEnumerable<MediaTrackAction> actions)
        {
            TrackManyCore(actions, null, false);
        }

        public void TrackMany(string albumName, IEnumerable<MediaTrackAction> actions, bool isMigration = false)
        {
            TrackManyCore(actions, albumName, isMigration);
        }

        protected virtual void TrackManyCore(IEnumerable<MediaTrackAction> actions, string albumName, bool isMigration)
        {
            Guard.NotEmpty(albumName, nameof(albumName));
            Guard.NotNull(actions, nameof(actions));

            if (!actions.Any())
                return;

            var ctx = _dbContext;

            using (var scope = new DbContextScope(ctx, 
                validateOnSave: false, 
                hooksEnabled: false, 
                autoDetectChanges: false))
            {
                // Get the id for an album (necessary later to set FolderId)...
                int? albumId = albumName.HasValue() 
                    ? _albumRegistry.GetAlbumByName(albumName)?.Id
                    : null;

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
                        if (albumName.HasValue() && track.Album.IsEmpty())
                        {
                            // Overwrite track album if scope album was passed.
                            track.Album = albumName;
                        }

                        // add or remove the track from file
                        if (action.Operation == MediaTrackOperation.Track)
                        {
                            file.Tracks.Add(track);
                        }
                        else
                        {
                            file.Tracks.Remove(track);
                        }

                        if (file.Tracks.Count > 0)
                        {
                            // A file with tracks can NEVER be transient
                            file.IsTransient = false;
                        }
                        else if (_makeFilesTransientWhenOrphaned)
                        {
                            // But an untracked file can OPTIONALLY be transient
                            file.IsTransient = true;
                        }
                    }
                }

                // Save whole batch to database
                int num = ctx.SaveChanges();

                // Breathe
                ctx.DetachEntities<MediaFile>(deep: true);
            }
        }

        public int DeleteAllTracks(string albumName)
        {
            Guard.NotEmpty(albumName, nameof(albumName));
            
            string sql = "DELETE FROM [MediaTrack] WHERE [Album] = '{0}'".FormatCurrent(albumName);
            return _dbContext.ExecuteSqlCommand(sql);
        }

        public void DetectAllTracks(string albumName, bool isMigration = false)
        {
            Guard.NotEmpty(albumName, nameof(albumName));

            // Get album info...
            var albumInfo = _albumRegistry.GetAlbumByName(albumName);
            if (albumInfo == null)
            {
                throw new InvalidOperationException($"The album '{albumName}' does not exist.");
            }

            // load corresponding detector provider for current album...
            var provider = _albumProviderFactory[albumInfo.ProviderType] as IMediaTrackDetector;
            if (provider == null)
            {
                throw new InvalidOperationException($"The album '{albumName}' does not support track detection.");
            }

            if (!isMigration)
            {
                // first delete all tracks for current album...
                DeleteAllTracks(albumName);
            }

            // >>>>> DO detection (potentially a very long process)...
            var tracks = provider.DetectAllTracks(albumName);

            // (perf) batch result data...
            foreach (var batch in tracks.Slice(500))
            {
                // process the batch
                TrackManyCore(batch, albumName, isMigration);
            }
        }

        public bool TryGetTrackedPropertiesFor(Type forType, out IEnumerable<TrackedMediaProperty> properties)
        {
            properties = null;

            var allTrackedProps = GetAllTrackedProperties();
            if (allTrackedProps.ContainsKey(forType))
            {
                properties = allTrackedProps[forType];
                return true;
            }

            return false;
        }

        protected virtual Multimap<Type, TrackedMediaProperty> GetAllTrackedProperties()
        {
            var propsMap = _cache.Get(TrackedPropertiesKey, () =>
            {
                var map = new Multimap<Type, TrackedMediaProperty>();
                var props = _albumRegistry.GetAllAlbums()
                    .Where(x => x.TrackedProperties != null && x.TrackedProperties.Length > 0)
                    .SelectMany(x => x.TrackedProperties);

                foreach (var prop in props)
                {
                    map.Add(prop.EntityType, prop);
                }

                return map;
            });

            return propsMap;
        }
    }
}
