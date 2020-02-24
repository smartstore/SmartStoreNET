using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Services.Media.Migration;

namespace SmartStore.Services.Media
{
    /// <summary>
    /// Checks for new media albums and installs them.
    /// </summary>
    public sealed class MediaStarter : IPostApplicationStart
    {
        private readonly IRepository<MediaAlbum> _albumRepository;
        private readonly IAlbumService _albumService;

        public MediaStarter(IRepository<MediaAlbum> albumRepository, IAlbumService albumService)
        {
            _albumRepository = albumRepository;
            _albumService = albumService;
        }

        public int Order => 0;
        public bool ThrowOnError => true;
        public int MaxAttempts => 1;

        public void Start(HttpContextBase httpContext)
        {
            if (MediaMigrator.Executed)
            {
                // Don't do stuff again after initial migration
                return;
            }
   
            var providers = new List<IAlbumProvider>();
            
            if (PluginManager.PluginChangeDetected || !_albumRepository.TableUntracked.Any())
            {
                providers.AddRange(_albumService.LoadAllAlbumProviders());
            }
            else
            {
                // Always execute system provider.
                providers.Add(_albumService.LoadAlbumProvider<SystemAlbumProvider>());
            }

            _albumService.InstallAlbums(providers);
        }

        public void OnFail(Exception exception, bool willRetry)
        {
        }
    }
}
