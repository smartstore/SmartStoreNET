using System;
using Autofac;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Media.Imaging;
using SmartStore.Services.Media.Imaging.Impl;
using SmartStore.Services.Media.Migration;
using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Media
{
    public class MediaModule : Module
    {
        private readonly ITypeFinder _typeFinder;

        public MediaModule(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Utils
            builder.RegisterType<MediaMigrator>().InstancePerRequest();
            builder.RegisterType<MediaMigrator3>().InstancePerRequest();
            builder.RegisterType<MediaHelper>().InstancePerRequest();
            builder.RegisterType<MediaExceptionFactory>().InstancePerRequest();

            builder.RegisterType<MediaTypeResolver>().As<IMediaTypeResolver>().InstancePerRequest();
            builder.RegisterType<MediaUrlGenerator>().As<IMediaUrlGenerator>().InstancePerRequest();
            builder.RegisterType<AlbumRegistry>().As<IAlbumRegistry>().InstancePerRequest();
            builder.RegisterType<FolderService>().As<IFolderService>().InstancePerRequest();
            builder.RegisterType<MediaTracker>().As<IMediaTracker>().InstancePerRequest();
            builder.RegisterType<MediaSearcher>().As<IMediaSearcher>().InstancePerRequest();
            builder.RegisterType<MediaService>().As<IMediaService>().InstancePerRequest();
            builder.RegisterType<DownloadService>().As<IDownloadService>().InstancePerRequest();

            // ImageProcessor adapter factory
            builder.RegisterType<IPImageFactory>().As<IImageFactory>().SingleInstance();

            builder.RegisterType<ImageCache>().As<IImageCache>().InstancePerRequest();
            builder.RegisterType<DefaultImageProcessor>().As<IImageProcessor>().InstancePerRequest();
            builder.RegisterType<MediaMover>().As<IMediaMover>().InstancePerRequest();

            // Register factory for currently active media storage provider
            if (DataSettings.DatabaseIsInstalled())
            {
                builder.Register(MediaStorageProviderFactory);
            }
            else
            {
                builder.Register<Func<IMediaStorageProvider>>(c => () => new FileSystemMediaStorageProvider(new MediaFileSystem()));
            }

            // Register all album providers
            var albumProviderTypes = _typeFinder.FindClassesOfType<IAlbumProvider>(ignoreInactivePlugins: true);
            foreach (var type in albumProviderTypes)
            {
                builder.RegisterType(type).As<IAlbumProvider>().Keyed<IAlbumProvider>(type).InstancePerRequest();
            }

            // Handlers
            builder.RegisterType<ImageHandler>().As<IMediaHandler>().InstancePerRequest();
        }

        private static Func<IMediaStorageProvider> MediaStorageProviderFactory(IComponentContext c)
        {
            var systemName = c.Resolve<ISettingService>().GetSettingByKey("Media.Storage.Provider", FileSystemMediaStorageProvider.SystemName);
            var provider = c.Resolve<IProviderManager>().GetProvider<IMediaStorageProvider>(systemName);
            return () => provider.Value;
        }
    }
}
