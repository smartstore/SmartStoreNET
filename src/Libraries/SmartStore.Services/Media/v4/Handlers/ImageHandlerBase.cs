using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SmartStore.Core.Logging;
using SmartStore.Utilities.Threading;

namespace SmartStore.Services.Media
{
    public abstract class ImageHandlerBase : IMediaHandler
    {
        protected ImageHandlerBase(IImageCache imageCache)
        {
            ImageCache = imageCache;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;
        public IImageCache ImageCache { get; set; }

        public virtual int Order => -100;

        public async Task ExecuteAsync(MediaHandlerContext context)
        {
            if (!IsProcessable(context))
            {
                return;
            }

            var query = context.ImageQuery;
            var pathData = context.PathData;

            var cachedImage = ImageCache.Get4(context.MediaFileId, pathData, query);

            if (!pathData.Extension.IsCaseInsensitiveEqual(cachedImage.Extension))
            {
                // The query requests another format. 
                // Adjust extension and mime type fo proper ETag creation.
                pathData.Extension = cachedImage.Extension;
                pathData.MimeType = cachedImage.MimeType;
            }

            if (!cachedImage.Exists)
            {
                // Lock concurrent requests to same resource
                using (await KeyedLock.LockAsync("ImageHandlerBase.Execute." + cachedImage.Path))
                {
                    ImageCache.RefreshInfo(cachedImage);

                    // File could have been processed by another request in the meantime, check again.
                    if (!cachedImage.Exists)
                    {
                        // Call inner function
                        var sourceFile = context.SourceFile;
                        if (sourceFile == null || sourceFile.Size == 0)
                        {
                            context.Executed = true;
                            return;
                        }

                        using (var inputStream = sourceFile.OpenRead())
                        {
                            try
                            {
                                await ProcessImageAsync(context, cachedImage, inputStream);
                            }
                            catch (Exception ex)
                            {
                                if (!(ex is ProcessImageException))
                                {
                                    // ProcessImageException is logged already in ImageProcessor
                                    Logger.ErrorFormat(ex, "Error processing media file '{0}'.", cachedImage.Path);
                                }

                                context.Exception = ex;
                                context.Executed = true;
                                return;
                            }

                            if (context.ResultStream != null && context.ResultStream.Length > 0)
                            {
                                await ImageCache.Put4Async(cachedImage, context.ResultStream);
                                context.ResultStream.Position = 0;
                                context.ResultFile = cachedImage.File;
                            }

                            context.Executed = true;
                            return;
                        }
                    }
                }
            }

            // Cached image existed already
            context.ResultFile = cachedImage.File;
            context.Executed = true;
        }

        protected abstract bool IsProcessable(MediaHandlerContext context);

        protected abstract Task ProcessImageAsync(MediaHandlerContext context, CachedImage cachedImage, Stream inputStream);
    }
}
