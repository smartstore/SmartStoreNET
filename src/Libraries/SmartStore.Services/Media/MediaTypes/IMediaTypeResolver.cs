using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{
    public interface IMediaTypeResolver
    {
        /// <summary>
        /// Resolves the conceptual media type for a given file extension or mime type.
        /// </summary>
        /// <param name="extension">The file extension (with dot).</param>
        /// <param name="mimeType">Used to map to an extension if <paramref name="extension"/> parameter is null or empty.
        /// <returns>The media type or <see cref="MediaType.Binary"/> as fallback.</returns>
        MediaType Resolve(string extension, string mimeType = null);

        /// <summary>
        /// Parses and expands a given type filter, returning a distinct list of all suitable file extensions.
        /// If <paramref name="typeFilter"/> is empty or '*', all supported file extensions are returned.
        /// </summary>
        /// <param name="typeFilter">A comma separated list of either file extensions and/or media type names, e.g.: "image,.mp4,audio,.pdf". Extensions must start with a dot.</param>
        /// <returns>All suitable file extensions.</returns>
        IEnumerable<string> ParseTypeFilter(string typeFilter);

        /// <summary>
        /// Parses and expands a given type filter, returning a distinct list of all suitable file extensions.
        /// </summary>
        /// <param name="typeFilter">A list of either file extensions and/or media type names, e.g.: [ "image", ".mp4", "audio", ".pdf" ]. Extensions must start with a dot.</param>
        /// <returns>All suitable file extensions.</returns>
        IEnumerable<string> ParseTypeFilter(string[] typeFilter);

        /// <summary>
        /// Get the cached "file extension to media type name" map.
        /// "Key" is the dot-less file extension and "Value" is the corresponding media type name.
        /// </summary>
        /// <returns>The cached dictionary</returns>
        IReadOnlyDictionary<string, string> GetExtensionMediaTypeMap();
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
