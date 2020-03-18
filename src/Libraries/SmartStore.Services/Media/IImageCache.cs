using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{
    /// <summary>
    /// Image cache interface
    /// </summary>
    public interface IImageCache
    {
		/// <summary>
		/// Adds an image to the cache.
		/// </summary>
		/// <param name="cachedImage">An instance of the <see cref="CachedImage"/> object, which is returned by the <c>Get()</c> method.</param>
		/// <param name="buffer">The image binary buffer.</param>
		/// <returns><c>true</c> when the operation succeded, <c>false</c> otherwise</returns>
		void Put(CachedImage cachedImage, byte[] buffer);
		void Put4(CachedImage cachedImage, Stream stream);

		/// <summary>
		/// Asynchronously adds an image to the cache.
		/// </summary>
		/// <param name="cachedImage">An instance of the <see cref="CachedImage"/> object, which is returned by the <c>Get()</c> method.</param>
		/// <param name="buffer">The image binary buffer.</param>
		/// <returns><c>true</c> when the operation succeded, <c>false</c> otherwise</returns>
		Task PutAsync(CachedImage cachedImage, byte[] buffer);
		Task Put4Async(CachedImage cachedImage, Stream stream);

		/// <summary>
		/// Gets an instance of the <see cref="CachedImage"/> object, which contains information about a cached image.
		/// </summary>
		/// <param name="pictureId">The picture id of the image to be resolved.</param>
		/// <param name="seoFileName">The seo friendly picture name of the image to be resolved.</param>
		/// <param name="extension">The extension of the image to be resolved.</param>
		/// <param name="query">The image processing query.</param>
		/// <returns>An instance of the <see cref="CachedImage"/> object</returns>
		/// <remarks>If the requested image does not exist in the cache, the value of the <c>Exists</c> property will be <c>false</c>.</remarks>
		CachedImage Get(int? pictureId, string seoFileName, string extension, ProcessImageQuery query = null);
		CachedImage Get4(int? mediaFileId, MediaPathData data, ProcessImageQuery query = null);

		/// <summary>
		/// Gets an instance of the <see cref="CachedImage"/> object, which contains information about a cached image.
		/// Use this overload to get thumbnail info about uploaded media manager asset files.
		/// </summary>
		/// <param name="file">The file to get info about.</param>
		/// <param name="query">The image processing query.</param>
		/// <returns>An instance of the <see cref="CachedImage"/> object</returns>
		/// <remarks>If the requested image does not exist in the cache, the value of the <c>Exists</c> property will be <c>false</c>.</remarks>
		CachedImage Get(IFile file, ProcessImageQuery query);

		/// <summary>
		/// Opens a readonly file stream to the cached image
		/// </summary>
		/// <param name="cachedImage">An instance of the <see cref="CachedImage"/> object, which is returned by the <c>GetCachedImage()</c> method.</param>
		/// <returns>File stream</returns>
		Stream Open(CachedImage cachedImage);

		/// <summary>
		/// Deletes all cached images for the given <see cref="MediaFile"/>
		/// </summary>
		/// <param name="picture">The <see cref="MediaFile"/> for which to delete cached images</param>
		void Delete(MediaFile picture);

		/// <summary>
		/// Deletes all cached images for the given <see cref="IFile"/>
		/// </summary>
		/// <param name="file">The <see cref="IFile"/> for which to delete cached images</param>
		void Delete(IFile file);

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

		/// <summary>
		/// Resolves a publicly accessible http url to the image 
		/// </summary>
		/// <param name="imagePath">The path of the image relative to the cache root path</param>
		/// <returns>The image http url</returns>
		string GetPublicUrl(string imagePath);
	}
}
