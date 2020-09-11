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
using System.Runtime.CompilerServices;
using SmartStore.Services.Configuration;
using SmartStore.Core.Configuration;
using System.Linq.Expressions;
using SmartStore.ComponentModel;
using SmartStore.Core.Localization;

namespace SmartStore.Services.Media
{
    public class MediaTracker : IMediaTracker
    {
        public Localizer T { get; set; } = NullLocalizer.Instance;

        internal const string TrackedPropertiesKey = "media:trackedprops:all";

        private readonly ICacheManager _cache;
        private readonly IDbContext _dbContext;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IAlbumRegistry _albumRegistry;
        private readonly IFolderService _folderService;
        private readonly MediaSettings _mediaSettings;
        private readonly IIndex<Type, IAlbumProvider> _albumProviderFactory;

        private bool _makeFilesTransientWhenOrphaned;

        public MediaTracker(
            ICacheManager cache,
            IDbContext dbContext,
            ISettingService settingService,
            IStoreContext storeContext,
            IAlbumRegistry albumRegistry,
            IFolderService folderService,
            MediaSettings mediaSettings,
            IIndex<Type, IAlbumProvider> albumProviderFactory)
        {
            _cache = cache;
            _dbContext = dbContext;
            _settingService = settingService;
            _storeContext = storeContext;
            _albumRegistry = albumRegistry;
            _folderService = folderService;
            _mediaSettings = mediaSettings;
            _albumProviderFactory = albumProviderFactory;

            _makeFilesTransientWhenOrphaned = _mediaSettings.MakeFilesTransientWhenOrphaned;
        }

        public IDisposable BeginScope(bool makeFilesTransientWhenOrphaned)
        {
            var makeTransient = _makeFilesTransientWhenOrphaned;
            _makeFilesTransientWhenOrphaned = makeFilesTransientWhenOrphaned;

            return new ActionDisposable(() => _makeFilesTransientWhenOrphaned = makeTransient);
        }

        public void Track<TSetting>(TSetting settings, int? prevMediaFileId, Expression<Func<TSetting, int>> path) where TSetting : ISettings, new()
        {
            Guard.NotNull(settings, nameof(settings));
            Guard.NotNull(path, nameof(path));

            TrackSetting(settings, path.ExtractPropertyInfo().Name, prevMediaFileId, path.CompileFast().Invoke(settings));
        }

        public void Track<TSetting>(TSetting settings, int? prevMediaFileId, Expression<Func<TSetting, int?>> path) where TSetting : ISettings, new()
        {
            Guard.NotNull(settings, nameof(settings));
            Guard.NotNull(path, nameof(path));

            TrackSetting(settings, path.ExtractPropertyInfo().Name, prevMediaFileId, path.CompileFast().Invoke(settings));
        }

        protected void TrackSetting<TSetting>(
            TSetting settings,
            string propertyName,
            int? prevMediaFileId,
            int? currentMediaFileId) where TSetting : ISettings, new()
        {
            Guard.NotNull(settings, nameof(settings));
            Guard.NotEmpty(propertyName, nameof(propertyName));

            var key = nameof(TSetting) + "." + propertyName;
            var storeId = _storeContext.CurrentStoreIdIfMultiStoreMode;

            var settingEntity = _settingService.GetSettingEntityByKey(key, storeId);
            if (settingEntity != null)
            {
                this.UpdateTracks(settingEntity, prevMediaFileId, currentMediaFileId, propertyName);
            }
        }

        public void Track(BaseEntity entity, int mediaFileId, string propertyName)
        {
            Guard.NotNull(entity, nameof(entity));
            Guard.NotEmpty(propertyName, nameof(propertyName));

            TrackSingle(entity, mediaFileId, propertyName, MediaTrackOperation.Track);
        }

        public void Untrack(BaseEntity entity, int mediaFileId, string propertyName)
        {
            Guard.NotNull(entity, nameof(entity));
            Guard.NotEmpty(propertyName, nameof(propertyName));

            TrackSingle(entity, mediaFileId, propertyName, MediaTrackOperation.Untrack);
        }

        protected virtual void TrackSingle(BaseEntity entity, int mediaFileId, string propertyName, MediaTrackOperation operation)
        {
            Guard.NotNull(entity, nameof(entity));

            if (mediaFileId < 1 || entity.IsTransientRecord())
                return;

            var file = _dbContext.Set<MediaFile>().Find(mediaFileId);
            if (file != null)
            {
                var album = _folderService.FindAlbum(file)?.Value;
                if (album == null)
                {
                    throw new InvalidOperationException(T("Admin.Media.Exception.TrackUnassignedFile"));
                }
                else if (!album.CanDetectTracks)
                {
                    // No support for tracking on album level, so get outta here.
                    return;
                }

                var track = new MediaTrack
                {
                    EntityId = entity.Id,
                    EntityName = entity.GetEntityName(),
                    MediaFileId = mediaFileId,
                    Property = propertyName,
                    Album = album.Name
                };

                if (operation == MediaTrackOperation.Track)
                {
                    file.Tracks.Add(track);
                }
                else
                {
                    var dbTrack = file.Tracks.FirstOrDefault(x => x == track);
                    if (dbTrack != null)
                    {
                        file.Tracks.Remove(track);
                        _dbContext.ChangeState(dbTrack, System.Data.Entity.EntityState.Deleted);
                    }
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrackMany(IEnumerable<MediaTrack> actions)
        {
            TrackManyCore(actions, null, false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TrackMany(string albumName, IEnumerable<MediaTrack> tracks, bool isMigration = false)
        {
            TrackManyCore(tracks, albumName, isMigration);
        }

        protected virtual void TrackManyCore(IEnumerable<MediaTrack> tracks, string albumName, bool isMigration)
        {
            Guard.NotNull(tracks, nameof(tracks));

            if (!tracks.Any())
                return;

            var ctx = _dbContext;

            using (var scope = new DbContextScope(ctx,
                validateOnSave: false,
                hooksEnabled: false,
                autoDetectChanges: false))
            {
                // Get the album (necessary later to set FolderId)...
                MediaFolderNode albumNode = albumName.HasValue()
                    ? _folderService.GetNodeByPath(albumName)?.Value
                    : null;

                // Get distinct ids of all detected files...
                var mediaFileIds = tracks.Select(x => x.MediaFileId).Distinct().ToArray();

                // fetch these files from database...
                var query = ctx.Set<MediaFile>().Include(x => x.Tracks).Where(x => mediaFileIds.Contains(x.Id));
                if (isMigration)
                {
                    query = query.Where(x => x.Version == 1);
                }
                var files = query.ToDictionary(x => x.Id);

                // for each media file relation to an entity...
                foreach (var track in tracks)
                {
                    // fetch the file from local dictionary by its id...
                    if (files.TryGetValue(track.MediaFileId, out var file))
                    {
                        if (isMigration)
                        {
                            // set album id as folder id (during initial migration there are no sub-folders)
                            file.FolderId = albumNode?.Id;

                            // remember that we processed tracks for this file already
                            file.Version = 2;
                        }

                        if (track.Album.IsEmpty())
                        {
                            if (albumNode != null)
                            {
                                // Overwrite track album if scope album was passed.
                                track.Album = albumNode.Name;
                            }
                            else if (file.FolderId.HasValue)
                            {
                                // Determine album from file
                                albumNode = _folderService.FindAlbum(file)?.Value;
                                track.Album = albumNode?.Name;
                            }
                        }

                        if (track.Album.IsEmpty())
                            continue; // cannot track without album

                        if ((albumNode ?? _folderService.FindAlbum(file)?.Value)?.CanDetectTracks == false)
                            continue; // should not track in albums that do not support track detection

                        // add or remove the track from file
                        if (track.Operation == MediaTrackOperation.Track)
                        {
                            file.Tracks.Add(track);
                        }
                        else
                        {
                            var dbTrack = file.Tracks.FirstOrDefault(x => x == track);
                            if (dbTrack != null)
                            {
                                file.Tracks.Remove(track);
                                _dbContext.ChangeState(dbTrack, System.Data.Entity.EntityState.Deleted);
                            }
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

                if (num > 0)
                {
                    // Breathe
                    ctx.DetachEntities<MediaFile>(deep: true);
                }
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
                throw new InvalidOperationException(T("Admin.Media.Exception.AlbumNonexistent", albumName));
            }

            // load corresponding detector provider for current album...
            var provider = _albumProviderFactory[albumInfo.ProviderType] as IMediaTrackDetector;
            if (provider == null)
            {
                throw new InvalidOperationException(T("Admin.Media.Exception.AlbumNoTrack", albumName));
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
