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

            var cachedImage = ImageCache.Get(context.MediaFileId, pathData, query);

            if (!pathData.Extension.IsCaseInsensitiveEqual(cachedImage.Extension))
            {
                // The query requests another format. 
                // Adjust extension and mime type fo proper ETag creation.
                pathData.Extension = cachedImage.Extension;
                pathData.MimeType = cachedImage.MimeType;
            }

            var exists = cachedImage.Exists;

            if (exists && cachedImage.FileSize == 0)
            {
                // Empty file means: thumb extraction failed before and will most likely fail again.
                // Don't bother proceeding.
                context.Exception = new ExtractThumbnailException(cachedImage.FileName, null);
                context.Executed = true;
                return;
            }

            if (!exists)
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

                        var inputStream = sourceFile.OpenRead();

                        if (inputStream == null)
                        {
                            context.Exception = new ExtractThumbnailException("Input stream was null.", null);
                            context.Executed = true;
                            return;
                        }

                        try
                        {
                            await ProcessImageAsync(context, cachedImage, inputStream);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);

                            if (ex is ExtractThumbnailException)
                            {
                                // Thumbnail extraction failed and we must assume that it always will fail.
                                // Therefore we create an empty file to prevent repetitive processing.
                                using (var memStream = new MemoryStream())
                                {
                                    await ImageCache.PutAsync(cachedImage, memStream);
                                }
                            }

                            context.Exception = ex;
                            context.Executed = true;
                            return;
                        }
                        finally
                        {
                            if (inputStream != null)
                            {
                                inputStream.Dispose();
                            }
                        }

                        if (context.ResultStream != null && context.ResultStream.Length > 0)
                        {
                            await ImageCache.PutAsync(cachedImage, context.ResultStream);
                            context.ResultStream.Position = 0;
                            context.ResultFile = cachedImage.File;
                        }

                        context.Executed = true;
                        return;

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
