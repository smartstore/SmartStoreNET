using System;
using System.Runtime.CompilerServices;

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
    }
}
