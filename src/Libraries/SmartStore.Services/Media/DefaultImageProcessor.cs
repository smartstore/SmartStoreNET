using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Drawing;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Formats;
using SmartStore.Core.Logging;
using ImageProcessor.Configuration;
using SmartStore.Core.Events;

namespace SmartStore.Services.Media
{
	public partial class DefaultImageProcessor : IImageProcessor
    {
		private static long _totalProcessingTime;

		private readonly IEventPublisher _eventPublisher;

		public DefaultImageProcessor(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;

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
						throw new ProcessImageException("Invalid source type '{0}' in query.".FormatInvariant(query.Source.GetType().FullName), query);
					}

					// Pre-process event
					_eventPublisher.Publish(new ImageProcessingEvent(query, processor));

					var result = new ProcessImageResult
					{
						Query = query,
						SourceWidth = processor.Image.Width,
						SourceHeight = processor.Image.Height
					};

					// Core processing
					ProcessImageCore(query, processor);
					
					// Create & prepare result
					var outStream = new MemoryStream();
					processor.Save(outStream);

					result.Width = processor.Image.Width;
					result.Height = processor.Image.Height;
					result.FileExtension = processor.CurrentImageFormat.DefaultExtension;
					result.MimeType = processor.CurrentImageFormat.MimeType;
					result.OutputStream = outStream;

					// Post-process event
					_eventPublisher.Publish(new ImageProcessedEvent(query, processor, result));

					result.ProcessTimeMs = watch.ElapsedMilliseconds;				

					return result;
				}
			}
			catch (Exception ex)
			{
				var pex = new ProcessImageException(query, ex);
				Logger.Error(pex);
				throw pex;
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

		/// <summary>
		/// Processes the loaded image. Inheritors should NOT save the image, this is done by the main method. 
		/// </summary>
		/// <param name="query">Query</param>
		/// <param name="processor">Processor instance</param>
		protected virtual void ProcessImageCore(ProcessImageQuery query, ImageFactory processor)
		{
			// Resize
			var size = query.MaxWidth != null || query.MaxHeight != null
				? new Size(query.MaxWidth ?? 0, query.MaxHeight ?? 0)
				: Size.Empty;

			if (!size.IsEmpty)
			{
				var scaleMode = ConvertScaleMode(query.ScaleMode);
				processor.Resize(new ResizeLayer(size, resizeMode: scaleMode, upscale: false));
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

		private ResizeMode ConvertScaleMode(string mode)
		{
			switch (mode.EmptyNull().ToLower())
			{
				case "boxpad":
					return ResizeMode.BoxPad;
				case "crop":
					return ResizeMode.Crop;
				case "min":
					return ResizeMode.Min;
				case "pad":
					return ResizeMode.Pad;
				case "stretch":
					return ResizeMode.Stretch;
				default:
					return ResizeMode.Max;
			}
		}
	}
}
