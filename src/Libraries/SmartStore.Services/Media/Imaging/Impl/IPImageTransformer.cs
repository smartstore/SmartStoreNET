using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessor;
using ImageProcessor.Imaging;

namespace SmartStore.Services.Media.Impl
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
    }
}
