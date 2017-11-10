using System;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{
    public static class ImageCacheExtensions
    {
		/// <summary>
		/// Gets an instance of the <see cref="CachedImageResult"/> object, which contains information about a cached image.
		/// </summary>
		/// <param name="picture">The picture object for which to resolve a cached image.</param>
		/// <param name="query">The image processing query.</param>
		/// <returns>An instance of the <see cref="CachedImageResult"/> object</returns>
		/// <remarks>If the requested image does not exist in the cache, the value of the <c>Exists</c> property will be <c>false</c>.</remarks>
		public static CachedImageResult GetCachedImage(this IImageCache imageCache, Picture picture, ProcessImageQuery query = null)
        {
            Guard.NotNull(picture, nameof(picture));

            return imageCache.GetCachedImage(picture.Id, picture.SeoFilename, MimeTypes.MapMimeTypeToExtension(picture.MimeType), query);
        }

		/// <summary>
		/// Adds an image to the cache.
		/// </summary>
		/// <param name="pictureId">The picture id, which will be part of the resulting file name.</param>
		/// <param name="seoFileName">The seo friendly picture name, which will be part of the resulting file name.</param>
		/// <param name="extension">The extension of the resulting file</param>
		/// <param name="buffer">The image binary data.</param>
		/// <param name="query">The image processing query. This object, if not <c>null</c>, is hashed and appended to the resulting file name.</param>
		public static void AddImageToCache(this IImageCache imageCache, int? pictureId, string seoFileName, string extension, byte[] buffer, ProcessImageQuery query = null)
        {
            var cachedImage = imageCache.GetCachedImage(pictureId, seoFileName, extension, query);
            imageCache.AddImageToCache(cachedImage, buffer);
        }

		/// <summary>
		/// Adds an image to the cache.
		/// </summary>
		/// <param name="picture">The picture object needed for building the resulting file name.</param>
		/// <param name="buffer">The image binary data.</param>
		/// <param name="query">The image processing query. This object, if not <c>null</c>, is hashed and appended to the resulting file name.</param>
		public static void AddImageToCache(this IImageCache imageCache, Picture picture, byte[] buffer, ProcessImageQuery query = null)
        {
            Guard.NotNull(picture, nameof(picture));
            imageCache.AddImageToCache(picture.Id, picture.SeoFilename, MimeTypes.MapMimeTypeToExtension(picture.MimeType), buffer, query);
        }
    }
}
