using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Services.Media.Imaging;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetUrl(this IMediaService service, MediaFileInfo file, int thumbnailSize, string host = null, bool doFallback = true)
        {
            ProcessImageQuery query = thumbnailSize > 0
                ? new ProcessImageQuery { MaxSize = thumbnailSize }
                : null;

            return service.GetUrl(file, query, host, doFallback);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// <param name="equalFile">A file from the <paramref name="files"/> collection whose content is equal to <paramref name="sourceBuffer"/>.</param>
        /// <returns>The passed file binary when no file equals in the sequence, <c>null</c> otherwise.</returns>
        public static byte[] FindEqualFile(this IMediaService service, byte[] sourceBuffer, IEnumerable<MediaFile> files, out MediaFile equalFile)
        {
            Guard.NotNull(sourceBuffer, nameof(sourceBuffer));

            if (!service.FindEqualFile(new MemoryStream(sourceBuffer), files, false, out equalFile))
            {
                return sourceBuffer;
            }

            return null;
        }

        /// <summary>
        /// Tries to find an equal file by file name, then by comparing the binary contents of the matched files to <paramref name="sourcePath"/> binary content.
        /// </summary>
        /// <param name="sourcePath">The full physical path to the source file to find a duplicate for (e.g. a local or downloaded file during an import process).</param>
        /// <param name="targetFolderId">The id of the folder in which to look for duplicates.</param>
        /// <param name="deepSearch">Whether to search in subfolders too.</param>
        /// <param name="equalFile">The first file whose content is equal to the content of <paramref name="sourcePath"/>.</param>
        /// <returns><c>true</c> when a duplicate file was found, <c>false</c> otherwise.</returns>
        public static bool FindEqualFile(this IMediaService service, string sourcePath, int targetFolderId, bool deepSearch, out MediaFile equalFile)
        {
            Guard.NotEmpty(sourcePath, nameof(sourcePath));

            var fi = new FileInfo(sourcePath);
            if (!fi.Exists)
            {
                equalFile = null;
                return false;
            }

            return FindEqualFile(service, fi.OpenRead(), fi.Name, targetFolderId, deepSearch, out equalFile);
        }

        /// <summary>
        /// Tries to find an equal file by file name, then by comparing the binary contents of the matched files to <paramref name="source"/> content.
        /// </summary>
        /// <param name="source">The source file stream to find a duplicate for (e.g. a local or downloaded file during an import process).</param>
        /// <param name="fileName">The file name used to determine potential duplicates to check against.</param>
        /// <param name="targetFolderId">The id of the folder in which to look for duplicates.</param>
        /// <param name="deepSearch">Whether to search in subfolders too.</param>
        /// <param name="equalFile">The first file whose content is equal to the content of <paramref name="source"/>.</param>
        /// <returns><c>true</c> when a duplicate file was found, <c>false</c> otherwise.</returns>
        public static bool FindEqualFile(this IMediaService service, Stream source, string fileName, int targetFolderId, bool deepSearch, out MediaFile equalFile)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotEmpty(fileName, nameof(fileName));
            Guard.IsPositive(targetFolderId, nameof(targetFolderId));
            
            equalFile = null;

            var query = new MediaSearchQuery
            {
                FolderId = targetFolderId,
                DeepSearch = deepSearch,
                ExactMatch = true,
                Term = fileName,
                IncludeAltForTerm = false
            };

            var matches = service.SearchFiles(query);

            if (matches.TotalCount == 0)
            {
                return false;
            }

            return service.FindEqualFile(source, matches.Select(x => x.File), true, out equalFile);
        }
    }
}
