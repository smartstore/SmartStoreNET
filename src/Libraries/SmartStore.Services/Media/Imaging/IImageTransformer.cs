using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Media.Imaging
{
    public interface IImageTransformer
    {
        /// <summary>
        /// Resizes the current image to the given dimensions.
        /// </summary>
        /// <param name="size">
        /// The <see cref="T:System.Drawing.Size"/> containing the width and height to set the image to.
        /// </param>
        /// <returns>
        IImageTransformer Resize(Size size);

        /// <summary>
        /// Resizes the current image according to the given options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>
        IImageTransformer Resize(ResizeOptions options);

        /// <summary>
        /// Changes the background color of the current image.
        /// </summary>
        /// <param name="color">
        /// The <see cref="T:System.Drawing.Color"/> to paint the image background with.
        /// </param>
        IImageTransformer BackgroundColor(Color color);
    }
}
