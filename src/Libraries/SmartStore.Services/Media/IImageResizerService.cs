using System;
using System.IO;
using System.Collections.Generic;

namespace SmartStore.Services.Media
{
    
    /// <summary>
    /// A service interface responsible for resizing images.
    /// </summary>
    public interface IImageResizerService
    {
        /// <summary>
        /// Determines whether the given file name is processable by the image resizer
        /// </summary>
        /// <param name="fileName">The name of the file (without path but including extension)</param>
        /// <returns>A value indicating whether processing is possible</returns>
        bool IsSupportedImage(string fileName);

        /// <summary>
        /// Resizes an image
        /// </summary>
        /// <param name="source">The source stream</param>
        /// <param name="maxWidth">The max width of the destination image</param>
        /// <param name="maxHeight">The max height of the destination image</param>
        /// <param name="quality">The output quality</param>
        /// <param name="mode">The resize mode</param>
        /// <param name="settings">A provider specific settings object.</param>
        /// <returns>The result image as a <c>MemoryStream</c> object</returns>
        MemoryStream ResizeImage(Stream source, int? maxWidth = null, int? maxHeight = null, int? quality = 0, object settings = null);
    }
}
