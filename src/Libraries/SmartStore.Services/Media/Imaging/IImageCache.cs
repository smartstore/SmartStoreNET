using System;
using System.IO;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media.Imaging
{
    /// <summary>
    /// Image cache interface
    /// </summary>
    public interface IImageCache
    {
        /// <summary>
        /// Adds an image to the cache.
        /// </summary>
        /// <param name="cachedImage">An instance of the <see cref="CachedImage"/> object which is returned by the <c>Get()</c> method.</param>
        /// <param name="image">The image object.</param>
        void Put(CachedImage cachedImage, IImage image);

        /// <summary>
        /// Adds an image to the cache.
        /// </summary>
        /// <param name="cachedImage">An instance of the <see cref="CachedImage"/> object which is returned by the <c>Get()</c> method.</param>
        /// <param name="stream">The file input stream.</param>
        void Put(CachedImage cachedImage, Stream stream);

        /// <summary>
        /// Asynchronously adds an image to the cache.
        /// </summary>
        /// <param name="cachedImage">An instance of the <see cref="CachedImage"/> object which is returned by the <c>Get()</c> method.</param>
        /// <param name="stream">The file input stream.</param>
        Task PutAsync(CachedImage cachedImage, Stream stream);

        /// <summary>
        /// Gets an instance of the <see cref="CachedImage"/> object which contains information about a cached image.
        /// </summary>
        /// <param name="mediaFileId">The id of the file to resolve a thumbnail for.</param>
        /// <param name="pathData">The path data.</param>
        /// <param name="query">The image processing query.</param>
        /// <returns>An instance of the <see cref="CachedImage"/> object</returns>
        /// <remarks>If the requested thumbnail does not exist in the cache, the value of the <c>Exists</c> property will be <c>false</c>.</remarks>
        CachedImage Get(int? mediaFileId, MediaPathData pathData, ProcessImageQuery query = null);

        /// <summary>
        /// Deletes all cached images for the given <see cref="MediaFile"/>
        /// </summary>
        /// <param name="picture">The <see cref="MediaFile"/> for which to delete cached images</param>
        void Delete(MediaFile picture);

        /// <summary>
        /// Deletes all cached images (nukes all files in the cache folder)
        /// </summary>
        void Clear();

        /// <summary>
        /// Refreshes the file info.
        /// </summary>
        void RefreshInfo(CachedImage cachedImage);

        /// <summary>
        /// Calculates statistics about the image cache data.
        /// </summary>
        /// <param name="fileCount">The total count of files in the cache.</param>
        /// <param name="totalSize">The total size of files in the cache (in bytes)</param>
        void CacheStatistics(out long fileCount, out long totalSize);
    }
}
