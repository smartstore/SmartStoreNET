using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{
    public static class IMediaServiceExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MediaSearchResult SearchFiles(this IMediaService service, MediaSearchQuery query, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking)
        {
            return service.SearchFiles(query, null, flags);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static async Task<MediaSearchResult> SearchFilesAsync(this IMediaService service, MediaSearchQuery query, MediaLoadFlags flags = MediaLoadFlags.AsNoTracking)
        {
            return await service.SearchFilesAsync(query, null, flags);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetUrl(this IMediaService service, int? fileId, ProcessImageQuery imageQuery, string host = null, bool doFallback = true)
        {
            return service.GetUrl(service.GetFileById(fileId ?? 0, MediaLoadFlags.AsNoTracking), imageQuery, host, doFallback);
        }

        public static string GetUrl(this IMediaService service, int? fileId, int thumbnailSize, string host = null, bool doFallback = true)
        {
            ProcessImageQuery query = thumbnailSize > 0 
                ? new ProcessImageQuery { MaxSize = thumbnailSize } 
                : null;
            
            return service.GetUrl(service.GetFileById(fileId ?? 0, MediaLoadFlags.AsNoTracking), query, host, doFallback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetUrl(this IMediaService service, MediaFile file, int thumbnailSize, string host = null, bool doFallback = true)
        {
            ProcessImageQuery query = thumbnailSize > 0
                ? new ProcessImageQuery { MaxSize = thumbnailSize }
                : null;

            return service.GetUrl(file != null ? service.ConvertMediaFile(file) : null, query, host, doFallback);
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

        /// <summary>
        /// Tries to find an equal file by comparing the source buffer to a list of files.
        /// </summary>
        /// <param name="source">Binary source file data to find a match for.</param>
        /// <param name="files">The sequence of files to seek within for duplicates.</param>
        /// <param name="equalFileId">Id of equal file if any</param>
        /// <returns>The passed file binary when no file equals in the sequence, <c>null</c> otherwise.</returns>
        public static byte[] FindEqualFile(this IMediaService service, byte[] sourceBuffer, IEnumerable<MediaFile> files, out int equalFileId)
        {
            Guard.NotNull(sourceBuffer, nameof(sourceBuffer));

            if (!service.FindEqualFile(new MemoryStream(sourceBuffer), files, false, out equalFileId))
            {
                return sourceBuffer;
            }

            return null;
        }
    }
}
