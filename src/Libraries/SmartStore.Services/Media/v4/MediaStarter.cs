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
        private readonly IRepository<MediaAlbum> _mediaAlbumRepository;
        private readonly IMediaFolderService _mediaFolderService;

        public MediaStarter(IRepository<MediaAlbum> mediaAlbumRepository, IMediaFolderService mediaFolderService)
        {
            _mediaAlbumRepository = mediaAlbumRepository;
            _mediaFolderService = mediaFolderService;
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
   
            var providers = new List<IMediaAlbumProvider>();
            
            if (PluginManager.PluginChangeDetected || !_mediaAlbumRepository.TableUntracked.Any())
            {
                providers.AddRange(_mediaFolderService.LoadAlbumProviders());
            }
            else
            {
                // Always execute system provider.
                providers.Add(new SystemMediaAlbumProvider());
            }

            _mediaFolderService.InstallAlbums(providers);
        }

        public void OnFail(Exception exception, bool willRetry)
        {
        }
    }
}
