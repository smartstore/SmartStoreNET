using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Media
{
    public interface IImage : IImageInfo, IDisposable
    {
        /// <summary>
        /// Gets the original width and height of the source image before any transform has been applied.
        /// </summary>
        Size SourceSize { get; }

        /// <summary>
        /// Gets or sets the color depth in number of bits per pixel (1, 4, 8, 16, 24, 32)
        /// </summary>
        new BitDepth BitDepth { get; set; }

        /// <summary>
        /// Gets or sets the format of the image.
        /// </summary>
        new IImageFormat Format { get; set; }

        /// <summary>
        /// Transforms the image.
        /// </summary>
        /// <param name="transformer"></param>
        IImage Transform(Action<IImageTransformer> transformer);

        /// <summary>
        /// Saves the current image to the specified output stream.
        /// </summary>
        /// <param name="stream">The stream to save the image to.</param>
        /// <returns>The current <see cref="IImage"/>.</returns>
        IImage Save(Stream stream);

        /// <summary>
        /// Saves the current image to the specified file path.
        /// </summary>
        /// <param name="path">The path to save the image to.</param>
        /// <returns>The current <see cref="IImage"/>.</returns>
        IImage Save(string path);
    }
}
