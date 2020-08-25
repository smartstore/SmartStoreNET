using System;
using System.Drawing;
using System.Linq;

namespace SmartStore.Services.Media.Imaging
{
    /// <summary>
    /// Enumerated resize modes to apply to resized images.
    /// </summary>
    public enum ResizeMode
    {
        /// <summary>
        /// Pads the resized image to fit the bounds of its container.
        /// If only one dimension is passed, will maintain the original aspect ratio.
        /// </summary>
        Pad,

        /// <summary>
        /// Stretches the resized image to fit the bounds of its container.
        /// </summary>
        Stretch,

        /// <summary>
        /// Crops the resized image to fit the bounds of its container.
        /// </summary>
        Crop,

        /// <summary>
        /// Constrains the resized image to fit the bounds of its container maintaining
        /// the original aspect ratio. 
        /// </summary>
        Max,

        /// <summary>
        /// Resizes the image until the shortest side reaches the set given dimension.
        /// Sets <see cref="ResizeLayer.Upscale"/> to <c>false</c> only allowing downscaling.
        /// </summary>
        Min,

        /// <summary>
        /// Pads the image to fit the bound of the container without resizing the 
        /// original source. Sets <see cref="ResizeLayer.Upscale"/> to <c>true</c>.
        /// When downscaling, performs the same functionality as <see cref="ResizeMode.Pad"/>
        /// </summary>
        BoxPad
    }

    /// <summary>
    /// Enumerated anchor positions to apply to resized images.
    /// </summary>
    public enum AnchorPosition
    {
        /// <summary>
        /// Anchors the position of the image to the center of it's bounding container.
        /// </summary>
        Center,

        /// <summary>
        /// Anchors the position of the image to the top of it's bounding container.
        /// </summary>
        Top,

        /// <summary>
        /// Anchors the position of the image to the bottom of it's bounding container.
        /// </summary>
        Bottom,

        /// <summary>
        /// Anchors the position of the image to the left of it's bounding container.
        /// </summary>
        Left,

        /// <summary>
        /// Anchors the position of the image to the right of it's bounding container.
        /// </summary>
        Right,

        /// <summary>
        /// Anchors the position of the image to the top left side of it's bounding container.
        /// </summary>
        TopLeft,

        /// <summary>
        /// Anchors the position of the image to the top right side of it's bounding container.
        /// </summary>
        TopRight,

        /// <summary>
        /// Anchors the position of the image to the bottom right side of it's bounding container.
        /// </summary>
        BottomRight,

        /// <summary>
        /// Anchors the position of the image to the bottom left side of it's bounding container.
        /// </summary>
        BottomLeft
    }

    public class ResizeOptions
    {
        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the resize mode.
        /// </summary>
        public ResizeMode ResizeMode { get; set; } = ResizeMode.Pad;

        /// <summary>
        /// Gets or sets the anchor position.
        /// </summary>
        public AnchorPosition AnchorPosition { get; set; } = AnchorPosition.Center;

        /// <summary>
        /// Gets or sets a value indicating whether to allow up-scaling of images.
        /// For <see cref="T:ResizeMode.BoxPad"/> this is always true.
        /// </summary>
        public bool Upscale { get; set; }

        /// <summary>
        /// Gets or sets the center coordinates.
        /// </summary>
        public float[] CenterCoordinates { get; set; }

        /// <summary>
        /// Gets or sets the anchor point.
        /// </summary>
        public Point? AnchorPoint { get; set; }

        /// <summary>
        /// Returns a value that indicates whether the specified object is an 
        /// <see cref="ResizeLayer"/> object that is equivalent to 
        /// this <see cref="ResizeLayer"/> object.
        /// </summary>
        /// <param name="obj">
        /// The object to test.
        /// </param>
        /// <returns>
        /// True if the given object  is an <see cref="ResizeLayer"/> object that is equivalent to 
        /// this <see cref="ResizeLayer"/> object; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is ResizeOptions options))
            {
                return false;
            }

            return this.Size == options.Size
                && this.ResizeMode == options.ResizeMode
                && this.AnchorPosition == options.AnchorPosition
                && this.Upscale == options.Upscale
                && ((this.CenterCoordinates != null
                    && options.CenterCoordinates != null
                    && this.CenterCoordinates.SequenceEqual(options.CenterCoordinates))
                    || (this.CenterCoordinates == options.CenterCoordinates))
                && this.AnchorPoint == options.AnchorPoint;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = this.Size.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)this.ResizeMode;
                hashCode = (hashCode * 397) ^ (int)this.AnchorPosition;
                hashCode = (hashCode * 397) ^ this.Upscale.GetHashCode();
                hashCode = (hashCode * 397) ^ (this.CenterCoordinates?.GetHashCode() ?? 0);
                return (hashCode * 397) ^ this.AnchorPoint.GetHashCode();
            }
        }
    }
}
