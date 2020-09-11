using System.IO;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
    public interface IMediaStorageProvider : IProvider
    {
        /// <summary>
        /// Gets a value indicating whether the provider saves data in a remote cloud storage (e.g. Azure)
        /// </summary>
        bool IsCloudStorage { get; }

        /// <summary>
        /// Retrieves the public URL for a given file within the storage provider.
        /// Returns <c>null</c> if the file is stored as a database BLOB.
        /// </summary>
        /// <param name="mediaFile">Media file item</param>
        /// <returns>The public URL.</returns>
        string GetPublicUrl(MediaFile mediaFile);

        /// <summary>
        /// Gets the size of the media item in bytes.
        /// </summary>
        /// <param name="mediaFile">Media file item</param>
        long GetSize(MediaFile mediaFile);

        /// <summary>
        /// Opens the media item for reading
        /// </summary>
        /// <param name="mediaFile">Media file item</param>
        Stream OpenRead(MediaFile mediaFile);

        /// <summary>
        /// Loads media item data
        /// </summary>
        /// <param name="mediaFile">Media storage item</param>
        byte[] Load(MediaFile mediaFile);

        /// <summary>
        /// Asynchronously loads media item data
        /// </summary>
        /// <param name="mediaFile">Media file item</param>
        Task<byte[]> LoadAsync(MediaFile mediaFile);

        /// <summary>
        /// Saves media item data
        /// </summary>
        /// <param name="mediaFile">Media file item</param>
        /// <param name="item">The source item</param>
        void Save(MediaFile mediaFile, MediaStorageItem item);

        /// <summary>
        /// Asynchronously saves media item data
        /// </summary>
        /// <param name="mediaFile">Media file item</param>
        /// <param name="item">The source item</param>
        Task SaveAsync(MediaFile mediaFile, MediaStorageItem item);

        /// <summary>
        /// Remove media storage item(s)
        /// </summary>
        /// <param name="mediaFiles">Media file items</param>
        void Remove(params MediaFile[] mediaFiles);

        /// <summary>
        /// Changes the extension of the stored file if the provider supports
        /// </summary>
        /// <param name="mediaFile">Media file item</param>
        /// <param name="extension">The nex file extension</param>
        void ChangeExtension(MediaFile mediaFile, string extension);
    }
}
