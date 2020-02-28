using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public class AlbumRegistry : IAlbumRegistry
    {
        internal const string AlbumInfosKey = "media:albums:all";

        private readonly IDbContext _dbContext;
        private readonly ICacheManager _cache;
        private readonly IEnumerable<Lazy<IAlbumProvider>> _albumProviders;
        
        public AlbumRegistry(
            IDbContext dbContext,
            ICacheManager cache,
            IEnumerable<Lazy<IAlbumProvider>> albumProviders)
        {
            _dbContext = dbContext;
            _cache = cache;
            _albumProviders = albumProviders;
        }

        public virtual IReadOnlyCollection<AlbumInfo> GetAllAlbums()
        {
            return GetAlbumDictionary().Values;
        }

        public IEnumerable<string> GetAlbumNames(bool withTrackDetectors = false)
        {
            var dict = GetAlbumDictionary();

            if (!withTrackDetectors)
            {
                return dict.Keys;
            }

            return dict.Where(x => x.Value.IsTrackDetector).Select(x => x.Key).ToArray();
        }

        private Dictionary<string, AlbumInfo> GetAlbumDictionary()
        {
            var albums = _cache.Get(AlbumInfosKey, () =>
            {
                return LoadAllAlbums().ToDictionary(x => x.Name);
            }, TimeSpan.FromHours(24));

            return albums;
        }

        protected virtual IEnumerable<AlbumInfo> LoadAllAlbums()
        {
            var setFolders = _dbContext.Set<MediaFolder>();
            var dbAlbums = setFolders
                .AsNoTracking()
                .OfType<MediaFolder>()
                .Select(x => new { x.Id, x.Name })
                .ToDictionary(x => x.Name);

            foreach (var lazyProvider in _albumProviders)
            {
                var provider = lazyProvider.Value;
                var albums = provider.GetAlbums().DistinctBy(x => x.Name).ToArray();

                foreach (var album in albums)
                {
                    var info = new AlbumInfo
                    {
                        Name = album.Name,
                        ProviderType = provider.GetType(),
                        DisplayHint = provider.GetDisplayHint(album) ?? new AlbumDisplayHint()
                    };

                    if (provider is IMediaTrackDetector detector)
                    {
                        var propertyTable = new TrackedMediaPropertyTable(album.Name);
                        detector.ConfigureTracks(album.Name, propertyTable);

                        info.IsTrackDetector = true;
                        info.TrackedProperties = propertyTable.GetProperties();
                    }

                    if (dbAlbums.TryGetValue(album.Name, out var dbAlbum))
                    {
                        info.Id = dbAlbum.Id;
                    }
                    else
                    {
                        setFolders.Add(album);
                        _dbContext.SaveChanges();
                        info.Id = album.Id;
                    }

                    yield return info;
                }
            }
        }

        public virtual AlbumInfo GetAlbumByName(string name)
        {
            Guard.NotEmpty(name, nameof(name));
            
            return GetAlbumDictionary().Get(name);
        }

        public virtual AlbumInfo GetAlbumById(int id)
        {
            if (id > 0)
            {
                return GetAlbumDictionary().FirstOrDefault(x => x.Value.Id == id).Value;
            }

            return null;
        }

        public void UninstallAlbum(string albumName)
        {
            Guard.NotEmpty(albumName, nameof(albumName));

            throw new NotImplementedException();

            //ClearCache();
        }

        public void DeleteAlbum(string albumName, string moveFilesToAlbum)
        {
            Guard.NotEmpty(albumName, nameof(albumName));
            Guard.NotEmpty(moveFilesToAlbum, nameof(moveFilesToAlbum));

            throw new NotImplementedException();

            //ClearCache();
        }

        public void ClearCache()
        {
            _cache.Remove(AlbumInfosKey);
        }
    }
}
