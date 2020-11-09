using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessor;
using ImageProcessor.Imaging;

namespace SmartStore.Services.Media.Imaging.Impl
{
    internal class IPImageTransformer : IImageTransformer
    {
        private readonly ImageFactory _image;

        public IPImageTransformer(ImageFactory image)
        {
            _image = image;
        }

        public IImageTransformer Resize(Size size)
        {
            return Resize(new ResizeOptions { Size = size });
        }

        public IImageTransformer Resize(ResizeOptions options)
        {
            Guard.NotNull(options, nameof(options));

            var layer = new ResizeLayer(
                size: options.Size,
                resizeMode: (ImageProcessor.Imaging.ResizeMode)options.ResizeMode,
                anchorPosition: (ImageProcessor.Imaging.AnchorPosition)options.AnchorPosition,
                upscale: options.Upscale,
                centerCoordinates: options.CenterCoordinates,
                maxSize: null,
                restrictedSizes: null,
                anchorPoint: options.AnchorPoint);

            _image.Resize(layer);
            return this;
        }

        public IImageTransformer BackgroundColor(Color color)
        {
            _image.BackgroundColor(color);
            return this;
        }

        public IImageTransformer Alpha(int percentage)
        {
            _image.Alpha(percentage);
            return this;
        }

        public IImageTransformer Brightness(int percentage)
        {
            _image.Brightness(percentage);
            return this;
        }

        public IImageTransformer Contrast(int percentage)
        {
            _image.Contrast(percentage);
            return this;
        }

        public IImageTransformer Crop(Rectangle rect)
        {
            _image.Crop(rect);
            return this;
        }

        public IImageTransformer EntropyCrop(byte threshold = 128)
        {
            _image.EntropyCrop(threshold);
            return this;
        }

        public IImageTransformer Flip(bool flipVertically = false, bool flipBoth = false)
        {
            _image.Flip(flipVertically, flipBoth);
            return this;
        }

        public IImageTransformer Format(IImageFormat format)
        {
            if (format is IPImageFormat ipFormat)
            {
                _image.Format(ipFormat.WrappedFormat);
                return this;
            }

            throw new ArgumentException("Passed image type must be '{0}'.".FormatCurrent(typeof(IPImageFormat)), nameof(format));
        }

        public IImageTransformer Gamma(float value)
        {
            _image.Gamma(value);
            return this;
        }

        public IImageTransformer GaussianBlur(int size)
        {
            _image.GaussianBlur(size);
            return this;
        }

        public IImageTransformer GaussianSharpen(int size)
        {
            _image.GaussianSharpen(size);
            return this;
        }

        public IImageTransformer Hue(int degrees, bool rotate = false)
        {
            _image.Hue(degrees, rotate);
            return this;
        }

        public IImageTransformer Quality(int percentage)
        {
            _image.Quality(percentage);
            return this;
        }

        public IImageTransformer BitDepth(BitDepth bitDepth)
        {
            _image.BitDepth((long)bitDepth);
            return this;
        }

        public IImageTransformer Overlay(IImage image, Size size, int opacity, Point? position)
        {
            if (image is IPImage ipImage)
            {
                _image.Overlay(new ImageLayer
                {
                    Image = ipImage.ImageFactory.Image,
                    Size = size,
                    Opacity = opacity,
                    Position = position
                });

                return this;
            }

            throw new ArgumentException("Passed image type must be '{0}'.".FormatCurrent(typeof(IPImage)), nameof(image));
        }

        public IImageTransformer Mask(IImage image, Size size, int opacity, Point? position)
        {
            if (image is IPImage ipImage)
            {
                _image.Mask(new ImageLayer
                {
                    Image = ipImage.ImageFactory.Image,
                    Size = size,
                    Opacity = opacity,
                    Position = position
                });

                return this;
            }

            throw new ArgumentException("Passed image type must be '{0}'.".FormatCurrent(typeof(IPImage)), nameof(image));
        }

        public IImageTransformer Rotate(float degrees, bool bounded = false, bool keepSize = false)
        {
            if (bounded)
                _image.Rotate(degrees);
            else
                _image.RotateBounded(degrees, keepSize);

            return this;
        }

        public IImageTransformer RoundedCorners(int radius)
        {
            _image.RoundedCorners(radius);
            return this;
        }

        public IImageTransformer Saturation(int percentage)
        {
            _image.Saturation(percentage);
            return this;
        }

        public IImageTransformer Tint(Color color)
        {
            _image.Tint(color);
            return this;
        }

        public IImageTransformer Vignette(Color? color = null)
        {
            _image.Vignette(color);
            return this;
        }
    }
}
