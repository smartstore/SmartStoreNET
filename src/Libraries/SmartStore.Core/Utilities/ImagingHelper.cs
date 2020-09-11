using System;
using System.Drawing;

namespace SmartStore.Utilities
{
    public static class ImagingHelper
    {
        public static int GetPerceivedBrightness(string htmlColor)
        {
            if (String.IsNullOrEmpty(htmlColor))
                htmlColor = "#ffffff";

            return GetPerceivedBrightness(ColorTranslator.FromHtml(htmlColor));
        }

        /// <summary>
        /// Calculates the perceived brightness of a color.
        /// </summary>
        /// <param name="color">The color</param>
        /// <returns>
        /// A number in the range of 0 (black) to 255 (White). 
        /// For text contrast colors, an optimal cutoff value is 130.
        /// </returns>
        public static int GetPerceivedBrightness(Color color)
        {
            return (int)Math.Sqrt(
               color.R * color.R * .241 +
               color.G * color.G * .691 +
               color.B * color.B * .068);
        }

        /// <summary>
        /// Recalculates an image size while keeping aspect ratio
        /// </summary>
        /// <param name="original">Original size</param>
        /// <param name="maxSize">New max size</param>
        /// <returns>The rescaled size</returns>
        public static Size Rescale(Size original, int maxSize)
        {
            Guard.IsPositive(maxSize, nameof(maxSize));

            return Rescale(original, new Size(maxSize, maxSize));
        }

        /// <summary>
        /// Recalculates an image size while keeping aspect ratio
        /// </summary>
        /// <param name="original">Original size</param>
        /// <param name="maxSize">New max size</param>
        /// <returns>The rescaled size</returns>
        public static Size Rescale(Size original, Size maxSize)
        {
            if (original.IsEmpty || maxSize.IsEmpty || (original.Width <= maxSize.Width && original.Height <= maxSize.Height))
            {
                return original;
            }

            // Figure out the ratio
            double ratioX = (double)maxSize.Width / (double)original.Width;
            double ratioY = (double)maxSize.Height / (double)original.Height;
            // use whichever multiplier is smaller
            double ratio = ratioX < ratioY ? ratioX : ratioY;

            return new Size(Convert.ToInt32(original.Width * ratio), Convert.ToInt32(original.Height * ratio));
        }
    }
}
