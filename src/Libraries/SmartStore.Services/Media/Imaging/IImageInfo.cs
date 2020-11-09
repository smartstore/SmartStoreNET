using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Media.Imaging
{
    /// <summary>
    /// Encapsulates properties that describe basic image information including dimensions, pixel type information
    /// and additional metadata.
    /// </summary>
    public interface IImageInfo
    {
        /// <summary>
        /// Gets width and height.
        /// </summary>
        Size Size { get; }

        /// <summary>
        /// Gets the color depth in number of bits per pixel (1, 4, 8, 16, 24, 32)
        /// </summary>
        BitDepth BitDepth { get; }

        /// <summary>
        /// Gets the format of the image.
        /// </summary>
        IImageFormat Format { get; }
    }
}
