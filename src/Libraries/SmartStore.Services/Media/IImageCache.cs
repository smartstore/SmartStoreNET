using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    /// <summary>
    /// Image cache interface
    /// </summary>
    public interface IImageCache
    {
        /// <summary>
        /// Resolves an http url to the image 
        /// </summary>
        /// <param name="imagePath">The path of the image relative to the cache root path</param>
        /// <param name="storeLocation">Store location URL; <c>null</c> to use determine the current store location automatically</param>
        /// <returns>The image http url</returns>
        string GetImageUrl(string imagePath, string storeLocation = null);

        /// <summary>
        /// Adds an image to the cache.
        /// </summary>
        /// <param name="cachedImage">An instance of the <see cref="CachedImageResult"/> object, which is returned by the <c>GetCachedImage()</c> method.</param>
        /// <param name="buffer">The image binary data.</param>
        void AddImageToCache(CachedImageResult cachedImage, byte[] buffer);

        /// <summary>
        /// Gets an instance of the <see cref="CachedImageResult"/> object, which contains information about a cached image.
        /// </summary>
        /// <param name="pictureId">The picture id of the image to be resolved.</param>
        /// <param name="seoFileName">The seo friendly picture name of the image to be resolved.</param>
        /// <param name="extension">The extension of the image to be resolved.</param>
        /// <param name="settings">The image processing settings.</param>
        /// <returns>An instance of the <see cref="CachedImageResult"/> object</returns>
        /// <remarks>If the requested image does not exist in the cache, the value of the <c>Exists</c> property will be <c>false</c>.</remarks>
        CachedImageResult GetCachedImage(int? pictureId, string seoFileName, string extension, object settings = null);

        /// <summary>
        /// Deletes all cached images for the given <see cref="Picture"/>
        /// </summary>
        /// <param name="picture">The <see cref="Picture"/> for which to delete cached images</param>
        void DeleteCachedImages(Picture picture);

        /// <summary>
        /// Deletes all cached images (nukes all files in the cache folder)
        /// </summary>
        void DeleteCachedImages();

        /// <summary>
        /// Calculates statistics about the image cache data.
        /// </summary>
        /// <param name="fileCount">The total count of files in the cache.</param>
        /// <param name="totalSize">The total size of files in the cache (in bytes)</param>
        void CacheStatistics(out long fileCount, out long totalSize);
    }
}
