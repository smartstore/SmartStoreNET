using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Logging;
using SmartStore.Services.Media.Imaging;

namespace SmartStore.Services.Media
{
    public class ImageHandler : ImageHandlerBase
    {
        private readonly IImageProcessor _imageProcessor;

        public ImageHandler(IImageProcessor imageProcessor, IImageCache imageCache, MediaExceptionFactory exceptionFactory)
            : base(imageCache, exceptionFactory)
        {
            _imageProcessor = imageProcessor;
        }

        protected override bool IsProcessable(MediaHandlerContext context)
        {
            return context.ImageQuery.NeedsProcessing(true) && _imageProcessor.Factory.IsSupportedImage(context.PathData.Extension);
        }

        protected override Task ProcessImageAsync(MediaHandlerContext context, CachedImage cachedImage, Stream inputStream)
        {
            var processQuery = new ProcessImageQuery(context.ImageQuery)
            {
                Source = inputStream,
                Format = context.ImageQuery.Format ?? cachedImage.Extension,
                FileName = cachedImage.FileName,
                DisposeSource = false
            };

            using (var result = _imageProcessor.ProcessImage(processQuery, false))
            {
                Logger.DebugFormat($"Processed image '{cachedImage.FileName}' in {result.ProcessTimeMs} ms.", null);

                var ext = result.Image.Format.DefaultExtension;

                if (!cachedImage.Extension.IsCaseInsensitiveEqual(ext))
                {
                    // jpg <> jpeg
                    cachedImage.Path = Path.ChangeExtension(cachedImage.Path, ext);
                    cachedImage.Extension = ext;
                }

                context.ResultImage = result.Image;
            }

            return Task.CompletedTask;
        }
    }
}
