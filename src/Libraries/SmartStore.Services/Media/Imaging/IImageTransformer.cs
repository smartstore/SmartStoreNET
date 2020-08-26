using System;
using System.Drawing;

namespace SmartStore.Services.Media.Imaging
{
    public interface IImageTransformer
    {
        /// <summary>
        /// Changes the opacity of the current image.
        /// </summary>
        /// <param name="percentage">The percentage by which to alter the images opacity. Any integer between 0 and 100.</param>
        IImageTransformer Alpha(int percentage);

        /// <summary>
        /// Changes the background color of the current image.
        /// </summary>
        /// <param name="color">
        /// The <see cref="T:System.Drawing.Color"/> to paint the image background with.
        /// </param>
        IImageTransformer BackgroundColor(Color color);

        /// <summary>
        /// Alters the bit depth of the current image.
        /// </summary>
        /// <param name="bitDepth">The new bit depth.</param>
        IImageTransformer BitDepth(BitDepth bitDepth);

        /// <summary>
        /// Changes the brightness of the current image. 
        /// </summary>
        /// <param name="percentage">The percentage by which to alter the images brightness. Any integer between -100 and 100.</param>
        IImageTransformer Brightness(int percentage);

        /// <summary>
        /// Changes the contrast of the current image. 
        /// </summary>
        /// <param name="percentage">The percentage by which to alter the images contrast. Any integer between -100 and 100.</param>
        IImageTransformer Contrast(int percentage);

        /// <summary>
        /// Crops the current image to the given location and size.
        /// </summary>
        /// <param name="rect">The rectangle containing the coordinates to crop the image to.</param>
        IImageTransformer Crop(Rectangle rect);

        /// <summary>
        /// Crops an image to the area of greatest entropy.
        /// </summary>
        /// <param name="threshold">The threshold in bytes to control the entropy.</param>
        IImageTransformer EntropyCrop(byte threshold = 128);

        /// <summary>
        /// Flips the current image either horizontally or vertically. 
        /// </summary>
        /// <param name="flipVertically">Whether to flip the image vertically.</param>
        /// <param name="flipBoth">Whether to flip the image both vertically and horizontally.</param>
        IImageTransformer Flip(bool flipVertically = false, bool flipBoth = false);

        /// <summary>
        /// Sets the output format of the current image to the matching ImageFormat.
        /// </summary>
        /// <param name="format">The new format to set the image to.</param>
        IImageTransformer Format(IImageFormat format);

        /// <summary>
        /// Adjust the gamma (intensity of the light) component of the given image.
        /// </summary>
        /// <param name="value">The value to adjust the gamma by (typically between .2 and 5).</param>
        IImageTransformer Gamma(float value);

        /// <summary>
        /// Uses a Gaussian kernel to blur the current image.
        /// </summary>
        /// <param name="size">The size to set the Gaussian kernel to.</param>
        /// <remarks>The sigma and threshold values applied to the kernel are 1.4 and 0 respectively.</remarks>
        IImageTransformer GaussianBlur(int size);

        /// <summary>
        /// Uses a Gaussian kernel to sharpen the current image.
        /// </summary>
        /// <param name="size">The size to set the Gaussian kernel to.</param>
        /// <remarks>The sigma and threshold values applied to the kernel are 1.4 and 0 respectively.</remarks>
        IImageTransformer GaussianSharpen(int size);

        /// <summary>
        /// Alters the hue of the current image changing the overall color.
        /// </summary>
        /// <param name="degrees">The angle by which to alter the images hue. Any integer between 0 and 360.</param>
        /// <param name="rotate">Whether to rotate the hue of the current image altering each color.</param>
        IImageTransformer Hue(int degrees, bool rotate = false);

        /// <summary>
        /// Applies an image mask to the current image.
        /// </summary>
        /// <param name="image">The mask image.</param>
        /// <param name="size">Size of the mask.</param>
        /// <param name="opacity">Opacity of the mask.</param>
        /// <param name="position">
        ///     Position of the mask. 
        ///     Point is used to place the image mask if it has not the same dimensions as the original image. 
        ///     Pass <c>null</c> to center the mask. 
        /// </param>
        IImageTransformer Mask(IImage image, Size size, int opacity, Point? position);

        /// <summary>
        /// Adds an image overlay to the current image. 
        /// </summary>
        /// <param name="image">The overlay image.</param>
        /// <param name="size">Size of the layer.</param>
        /// <param name="opacity">Opacity of the layer.</param>
        /// <param name="position">
        ///     Position of the layer. 
        ///     Point is used to place the image overlay if it has not the same dimensions as the original image. 
        ///     Pass <c>null</c> to center the overlay. 
        /// </param>
        IImageTransformer Overlay(IImage image, Size size, int opacity, Point? position);

        /// <summary>
        /// Alters the output quality of the current image.
        /// </summary>
        /// <param name="percentage">A value between 1 and 100 to set the quality to.</param>
        /// <remarks>This method will only effect the output quality of jpeg and webp images.</remarks>
        IImageTransformer Quality(int percentage);

        /// <summary>
        /// Rotates the current image by the given angle.
        /// </summary>
        /// <param name="degrees">The angle at which to rotate the image in degrees.</param>
        /// <param name="bounded">If <c>true</c>, rotates the image without expanding the canvas to fit the image.</param>
        /// <param name="keepSize">
        ///    Whether to keep the original image dimensions. .
        ///    Only applicable if <paramref name="bounded"/> is <c>true</c>.
        ///    If set to true, the image is zoomed to fit the bounding area.
        ///    If set to false, the area is cropped to fit the rotated image. 
        /// </param>
        IImageTransformer Rotate(float degrees, bool bounded = false, bool keepSize = false);

        /// <summary>
        /// Resizes the current image according to the given options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>
        IImageTransformer Resize(ResizeOptions options);

        /// <summary>
        /// Adds rounded corners to the current image. 
        /// </summary>
        /// <param name="radius">The radius at which the corner will be rounded.</param>
        IImageTransformer RoundedCorners(int radius);

        /// <summary>
        /// Changes the saturation of the current image.
        /// </summary>
        /// <param name="percentage">The percentage by which to alter the images saturation. Any integer between -100 and 100.</param>
        IImageTransformer Saturation(int percentage);

        /// <summary>
        /// Tints the current image with the given color.
        /// </summary>
        /// <param name="color">The color to tint the image with.</param>
        IImageTransformer Tint(Color color);

        /// <summary>
        /// Adds a vignette image effect to the current image.
        /// </summary>
        /// <param name="color">The color of the vignette. Defaults to black.</param>
        /// <returns></returns>
        IImageTransformer Vignette(Color? color = null);
    }

    public static class IImageTransformerExtensions
    {
        /// <summary>
        /// Resizes the current image to the given dimensions.
        /// </summary>
        /// <param name="size">
        /// The <see cref="T:System.Drawing.Size"/> containing the width and height to set the image to.
        /// </param>
        /// <returns>
        public static IImageTransformer Resize(this IImageTransformer transformer, Size size)
        {
            return transformer.Resize(new ResizeOptions { Size = size });
        }
    }
}
