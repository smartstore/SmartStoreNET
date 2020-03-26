using System;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{
    public interface IMediaTypeResolver
    {
        /// <summary>
        /// Resolves the conceptual media type for a given file extension or mime type.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <param name="mimeType">Used to map to an extension if <paramref name="extension"/> parameter is null or empty.
        /// <returns>The media type or <see cref="MediaType.Binary"/> as fallback.</returns>
        MediaType Resolve(string extension, string mimeType = null);
    }

    public static class IMediaTypeResolverExtensions 
    {
        public static MediaType Resolve(this IMediaTypeResolver resolver, MediaFile file)
        {
            Guard.NotNull(file, nameof(file));
            return resolver.Resolve(file.Extension, file.MimeType);
        }

        //public static MediaType Resolve(this IMediaTypeResolver resolver, IFile file)
        //{
        //    Guard.NotNull(file, nameof(file));
        //    return resolver.Resolve(file.Extension?.TrimStart('.'));
        //}
    }
}
