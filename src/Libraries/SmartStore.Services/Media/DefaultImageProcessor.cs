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
using SmartStore.Utilities;

namespace SmartStore.Services.Media
{
	public partial class DefaultImageProcessor : IImageProcessor
    {
		private static long _totalProcessingTime;

		private readonly IEventPublisher _eventPublisher;

		public DefaultImageProcessor(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public ILogger Logger { get; set; } = NullLogger.Instance;

		public bool IsSupportedImage(string extension)
        {
			if (extension.IsEmpty())
			{
				return false;
			}		
			
			if (extension[0] == '.' && extension.Length > 1)
			{
				extension = extension.Substring(1);
			}

            return ImageProcessorBootstrapper.Instance.SupportedImageFormats
				.SelectMany(x => x.FileExtensions)
				.Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

		public ProcessImageResult ProcessImage(ProcessImageQuery query)
		{
			Guard.NotNull(query, nameof(query));

			ValidateQuery(query);

			var watch = new Stopwatch();
			byte[] inBuffer = null;

			try
			{
				watch.Start();

				using (var processor = new ImageFactory(preserveExifData: false, fixGamma: false))
				{
					var source = query.Source;
					
					// Load source
					if (source is byte[] b)
					{
						inBuffer = b;
					}
					else if (source is Stream s)
					{
						inBuffer = s.ToByteArray();
					}
					else if (source is Image img)
					{
						processor.Load(img);
					}
					else if (source is string str)
					{
						var path = NormalizePath(str);
						using (var fs = File.OpenRead(path))
						{
							inBuffer = fs.ToByteArray();
						}
					}
					else
					{
						throw new ProcessImageException("Invalid source type '{0}' in query.".FormatInvariant(query.Source.GetType().FullName), query);
					}

					if (inBuffer != null)
					{
						processor.Load(inBuffer);
					}

					// Pre-process event
					_eventPublisher.Publish(new ImageProcessingEvent(query, processor));

					var result = new ProcessImageResult
					{
						Query = query,
						SourceWidth = processor.Image.Width,
						SourceHeight = processor.Image.Height,
						SourceMimeType = processor.CurrentImageFormat.MimeType
					};

					// Core processing
					ProcessImageCore(query, processor, out var fxApplied);
					
					// Create & prepare result
					var outStream = new MemoryStream();
					processor.Save(outStream);

					var fmt = processor.CurrentImageFormat;
					result.FileExtension = fmt.DefaultExtension == "jpeg" ? "jpg" : fmt.DefaultExtension;
					result.MimeType = fmt.MimeType;

					result.HasAppliedVisualEffects = fxApplied;
					result.Width = processor.Image.Width;
					result.Height = processor.Image.Height;

					if (inBuffer != null)
					{
						// Check whether it is more beneficial to return the source instead of the result.
						// Prefer result only if its size is smaller than the source size.
						// Result size may be larger if a high-compressed image has been uploaded.
						// During image processing the source compression algorithm gets lost and the image may be larger in size
						// after encoding with default encoders.
						var compare =
							// only when image was not altered visually...
							!fxApplied
							// ...size has not changed
							&& result.Width == result.SourceWidth
							&& result.Height == result.SourceHeight
							// ...and format has not changed
							&& result.MimeType == result.SourceMimeType;

						if (compare && inBuffer.LongLength <= outStream.GetBuffer().LongLength)
						{
							// Source is smaller. Throw away result and get back to source.
							outStream.Dispose();
							result.OutputStream = new MemoryStream(inBuffer, 0, inBuffer.Length, true, true);
						}
					}

					// Set output stream
					if (result.OutputStream == null)
					{
						result.OutputStream = outStream;
					}

					// Post-process event
					_eventPublisher.Publish(new ImageProcessedEvent(query, processor, result));

					result.OutputStream.Position = 0;

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
				if (query.DisposeSource && query.Source is IDisposable source)
				{
					source.Dispose();
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
		/// <param name="fxApplied">
		/// Should be true if any effect has been applied that potentially changes the image visually (like background color, contrast, sharpness etc.).
		/// Resize and compression quality does NOT count as FX.
		/// </param>
		protected virtual void ProcessImageCore(ProcessImageQuery query, ImageFactory processor, out bool fxApplied)
		{
			fxApplied = false;

			// Resize
			var size = query.MaxWidth != null || query.MaxHeight != null
				? new Size(query.MaxWidth ?? 0, query.MaxHeight ?? 0)
				: Size.Empty;

			if (!size.IsEmpty)
			{
				processor.Resize(new ResizeLayer(
					size, 
					resizeMode: ConvertScaleMode(query.ScaleMode),
					anchorPosition: ConvertAnchorPosition(query.AnchorPosition), 
					upscale: false));
			}		

			if (query.BackgroundColor.HasValue())
			{
				processor.BackgroundColor(ColorTranslator.FromHtml(query.BackgroundColor));
				fxApplied = true;
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

		private string NormalizePath(string path)
		{
			if (path.IsWebUrl())
			{
				throw new NotSupportedException($"Remote images cannot be processed: Path: {path}");
			}

			if (!PathHelper.IsAbsolutePhysicalPath(path))
			{
				path = CommonHelper.MapPath(path);
			}

			return path;
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

		private AnchorPosition ConvertAnchorPosition(string anchor)
		{
			switch (anchor.EmptyNull().ToLower())
			{
				case "top":
					return AnchorPosition.Top;
				case "bottom":
					return AnchorPosition.Bottom;
				case "left":
					return AnchorPosition.Left;
				case "right":
					return AnchorPosition.Right;
				case "top-left":
					return AnchorPosition.TopLeft;
				case "top-right":
					return AnchorPosition.TopRight;
				case "bottom-left":
					return AnchorPosition.BottomLeft;
				case "bottom-right":
					return AnchorPosition.BottomRight;
				default:
					return AnchorPosition.Center;
			}
		}
	}
}
