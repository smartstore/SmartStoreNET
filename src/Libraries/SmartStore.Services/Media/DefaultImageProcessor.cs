using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Drawing;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using ImageProcessor.Imaging.Formats;
using SmartStore.Core.Logging;
using ImageProcessor.Configuration;

namespace SmartStore.Services.Media
{
	public class DefaultImageProcessor : IImageProcessor
    {
		private static long _totalProcessingTime;

		public DefaultImageProcessor()
		{
			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		public bool IsSupportedImage(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            if (ext != null)
            {
                var extension = ext.Trim('.').ToLower();
                return ImageProcessorBootstrapper.Instance.SupportedImageFormats
					.SelectMany(x => x.FileExtensions)
					.Any(x => x == extension);
            }

            return false;
        }

		public ProcessImageResult ProcessImage(ProcessImageQuery query)
		{
			Guard.NotNull(query, nameof(query));

			ValidateQuery(query);

			var watch = new Stopwatch();

			try
			{
				watch.Start();

				using (var processor = new ImageFactory(preserveExifData: false, fixGamma: false))
				{
					var source = query.Source;

					// Load source
					if (source is byte[])
					{
						processor.Load((byte[])source);
					}
					else if (source is Stream)
					{
						processor.Load((Stream)source);
					}
					else if (source is Image)
					{
						processor.Load((Image)source);
					}
					else if (source is string)
					{
						// TODO: (mc) map virtual pathes
						processor.Load((string)source);
					}
					else
					{
						throw new ArgumentException("Invalid source type '{0}' in query.".FormatInvariant(query.Source.GetType().FullName), nameof(query));
					}

					var result = new ProcessImageResult
					{
						Query = query,
						SourceWidth = processor.Image.Width,
						SourceHeight = processor.Image.Height
					};

					// Resize
					var size = query.MaxWidth != null || query.MaxHeight != null
						? new Size(query.MaxWidth ?? 0, query.MaxHeight ?? 0)
						: Size.Empty;

					if (!size.IsEmpty)
					{
						var mode = ResizeMode.Max;
						if (query.ScaleMode.HasValue() && query.ScaleMode != "max")
						{
							// TODO: (mc) Implement! [...]
						}
						processor.Resize(new ResizeLayer(size, resizeMode: mode, upscale: false));
					}

					// Format
					if (query.Format != null)
					{
						var format = query.Format as ISupportedImageFormat;

						if (format == null && query.Format is string)
						{
							var requestedFormat = ((string)query.Format).ToLowerInvariant();
							switch (requestedFormat)
							{
								case "jpg":
								case "jpeg":
									format = new JpegFormat();
									break;
								case "png":
									format = new PngFormat();
									break;
								case "gif":
									format = new GifFormat();
									break;
								case "webp":
									format = new WebPFormat();
									break;
							}
						}

						if (format != null)
						{
							processor.Format(format);
						}
					}

					// Set Quality
					if (query.Quality.HasValue)
					{
						processor.Quality(query.Quality.Value);
					}

					// IsIndexed (for PNGs)
					if (true)
					{
						// TODO: (mc) get from settings
						processor.CurrentImageFormat.IsIndexed = true;
					}

					// Process
					var outStream = new MemoryStream();
					processor.Save(outStream);

					result.Width = processor.Image.Width;
					result.Height = processor.Image.Height;
					result.FileExtension = processor.CurrentImageFormat.DefaultExtension;
					result.MimeType = processor.CurrentImageFormat.MimeType;

					if (query.ExecutePostProcessor) // TODO: (mc) ask if enabled for current format
					{
						outStream = ImagePostProcessor.PostProcessImage(outStream, query.FileName, result.FileExtension, this.Logger);
					}

					result.ProcessTimeMs = watch.ElapsedMilliseconds;
					result.Result = outStream;

					return result;
				}
			}
			catch
			{
				throw;
			}
			finally
			{
				if (query.DisposeSource && query.Source is IDisposable)
				{
					((IDisposable)query.Source).Dispose();
				}

				watch.Stop();
				_totalProcessingTime += watch.ElapsedMilliseconds;
			}
		}

		private void ValidateQuery(ProcessImageQuery query)
		{
			if (query.Source == null)
			{
				throw new ArgumentException("During image processing 'ProcessImageQuery.Source' must not be null.", nameof(query));
			}
		}

		public long TotalProcessingTimeMs
		{
			get { return _totalProcessingTime; }
		}
	}
}
