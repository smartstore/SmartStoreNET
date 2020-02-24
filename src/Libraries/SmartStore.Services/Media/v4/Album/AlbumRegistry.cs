using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public class AlbumRegistry
    {
        private readonly MediaTrackPropertyTable _propertyTable;
        private readonly IRepository<MediaAlbum> _albumRepository;
        private readonly IEnumerable<Lazy<IAlbumProvider>> _albumProviders;
        private readonly IIndex<Type, IAlbumProvider> _albumProviderIndexer;

        private readonly static ConcurrentDictionary<string, AlbumProviderInfo> _albumProviderInfoCache = new ConcurrentDictionary<string, AlbumProviderInfo>();
        
        public AlbumRegistry(
            IRepository<MediaAlbum> albumRepository,
            IEnumerable<Lazy<IAlbumProvider>> albumProviders,
            IIndex<Type, IAlbumProvider> albumProvider)
        {
            _propertyTable = new MediaTrackPropertyTable();

            _albumRepository = albumRepository;
            _albumProviders = albumProviders;
            _albumProviderIndexer = albumProvider;
        }

        public void RegisterAlbums(IEnumerable<IAlbumProvider> albumProviders)
        {
            Guard.NotNull(albumProviders, nameof(albumProviders));

            foreach (var provider in albumProviders)
            {
                var albums = provider.GetAlbums().DistinctBy(x => x.Name).ToArray();

                foreach (var album in albums)
                {
                    var info = new AlbumProviderInfo
                    {
                        Name = album.Name,
                        ProviderType = provider.GetType(),
                        DisplayHint = provider.GetDisplayHint(album) ?? new AlbumDisplayHint()
                    };

                    if (provider is IMediaTrackDetector detector)
                    {
                        info.IsTrackDetector = true;
                        detector.ConfigureTracks(album.Name, _propertyTable);
                    }

                    //if (dbAlbums.TryGetValue(album.Name, out var dbAlbum))
                    //{
                    //    info.Id = dbAlbum.Id;
                    //}
                    //else
                    //{
                    //    _albumRepository.Insert(album);
                    //    hasChanges = true;
                    //    info.Id = album.Id;
                    //}

                    _albumProviderInfoCache.AddOrUpdate(album.Name, info, (key, val) => info);
                }
            }
        }

        public

        class AlbumProviderInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Type ProviderType { get; set; }
            public bool IsTrackDetector { get; set; }
            public AlbumDisplayHint DisplayHint { get; set; }
        }
    }
}
