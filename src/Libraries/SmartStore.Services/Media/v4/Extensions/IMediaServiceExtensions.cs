using System.Runtime.CompilerServices;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{
    public static class IMediaServiceExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetUrl(this IMediaService service, int? fileId, ProcessImageQuery imageQuery, string host = null, bool doFallback = true)
        {
            return service.GetUrl(service.GetFileById(fileId ?? 0), imageQuery, host, doFallback);
        }

        public static string GetUrl(this IMediaService service, int? fileId, int thumbnailSize, string host = null, bool doFallback = true)
        {
            ProcessImageQuery query = thumbnailSize > 0 
                ? new ProcessImageQuery { MaxSize = thumbnailSize } 
                : null;
            
            return service.GetUrl(service.GetFileById(fileId ?? 0), query, host, doFallback);
        }

        public static string GetUrl(this IMediaService service, MediaFileInfo file, int thumbnailSize, string host = null, bool doFallback = true)
        {
            ProcessImageQuery query = thumbnailSize > 0
                ? new ProcessImageQuery { MaxSize = thumbnailSize }
                : null;

            return service.GetUrl(file, query, host, doFallback);
        }

        public static string GetFallbackUrl(this IMediaService service, int thumbnailSize = 0)
        {
            ProcessImageQuery query = thumbnailSize > 0
                ? new ProcessImageQuery { MaxSize = thumbnailSize }
                : null;

            return service.GetUrl((MediaFileInfo)null, query, null, true);
        }

        public static string CreatePath(
            this IMediaService service,
            string album,
            string mimeType,
            string fileName,
            bool unique = true)
        {
            Guard.NotEmpty(album, nameof(album));
            Guard.NotEmpty(mimeType, nameof(mimeType));
            Guard.NotEmpty(fileName, nameof(fileName));

            var extension = MimeTypes.MapMimeTypeToExtension(mimeType).NullEmpty() ?? ".jpg";
            var path = string.Concat(album, "/", fileName.ToValidFileName(), extension.EnsureStartsWith("."));

            if (unique && service.CheckUniqueFileName(path, out var uniquePath))
            {
                path = uniquePath;
            }

            return path;
        }
    }
}
