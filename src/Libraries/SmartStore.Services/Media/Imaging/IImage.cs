using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Media.Imaging
{
    /// <summary>
    /// Represents a processable image.
    /// </summary>
    public interface IProcessableImage : IImage
    {
        /// <summary>
        /// Transforms the image.
        /// </summary>
        /// <param name="transformer"></param>
        IImage Transform(Action<IImageTransformer> transformer);
    }

    /// <summary>
    /// Represents an image.
    /// </summary>
    public interface IImage : IImageInfo, IDisposable
    {
        /// <summary>
        /// Gets the original width and height of the source image before any transform has been applied.
        /// </summary>
        Size SourceSize { get; }

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