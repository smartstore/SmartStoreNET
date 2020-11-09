using System;
using System.Collections.Generic;

namespace SmartStore.Services.Media.Imaging
{
    /// <summary>
    /// Defines the contract for an image format.
    /// </summary>
    public interface IImageFormat // TODO: >> IEquatable<IImageFormat> & Equals & GetHashCode()
    {
        /// <summary>
        /// Gets the name that describes this image format.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the default file extension.
        /// </summary>
        string DefaultExtension { get; }

        /// <summary>
        /// Gets the default mimetype that the image format uses
        /// </summary>
        string DefaultMimeType { get; }

        /// <summary>
        /// Gets the file extensions this image format commonly uses.
        /// </summary>
        IEnumerable<string> FileExtensions { get; }

        /// <summary>
        /// Gets all the mimetypes that have been used by this image format.
        /// </summary>
        IEnumerable<string> MimeTypes { get; }
    }
}
