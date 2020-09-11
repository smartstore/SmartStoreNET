using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
    public interface IMediaMover
    {
        /// <summary>
        /// Moves media items from one storage provider to another
        /// </summary>
        /// <param name="sourceProvider">The source media storage provider</param>
        /// <param name="targetProvider">The target media storage provider</param>
        /// <returns><c>true</c> success, <c>failure</c></returns>
        bool Move(Provider<IMediaStorageProvider> sourceProvider, Provider<IMediaStorageProvider> targetProvider);
    }
}
