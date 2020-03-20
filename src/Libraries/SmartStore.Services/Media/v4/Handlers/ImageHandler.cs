using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Logging;

namespace SmartStore.Services.Media
{
    public class ImageHandler : ImageHandlerBase
    {
        private readonly IImageProcessor _imageProcessor;
        
        public ImageHandler(IImageProcessor imageProcessor, IImageCache imageCache)
            : base(imageCache)
        {
            _imageProcessor = imageProcessor;
        }

        protected override bool IsProcessable(MediaHandlerContext context)
        {
            return context.ImageQuery.NeedsProcessing(true) && _imageProcessor.IsSupportedImage(context.PathData.Extension);
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

                if (!cachedImage.Extension.IsCaseInsensitiveEqual(result.FileExtension))
                {
                    // jpg <> jpeg
                    cachedImage.Path = Path.ChangeExtension(cachedImage.Path, result.FileExtension);
                    cachedImage.Extension = result.FileExtension;
                }

                context.ResultStream = result.OutputStream;
            }

            return Task.FromResult(0);
        }
    }
}
