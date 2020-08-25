using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
//using System.Drawing;
//using ImageProcessor;
//using ImageProcessor.Imaging;
//using ImageProcessor.Imaging.Formats;
using SmartStore.Core.Logging;
//using ImageProcessor.Configuration;
using SixLabors.ImageSharp;
using SmartStore.Core.Events;
using SmartStore.Utilities;
using SmartStore.Core.IO;
using SixLabors.ImageSharp.Formats;
using ImageSharpConfig = SixLabors.ImageSharp.Configuration;
using System.Collections.Concurrent;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace SmartStore.Services.Media
{
	public partial class ImageSharpImageProcessor : IImageProcessor
    {
		class SourceImage
        {
			public Image Image { get; set; }
			public IImageFormat Format { get; set; }
			public Stream Stream { get; set; }
			public long Length { get; set; }
			public int Width { get; set; }
			public int Height { get; set; }
		}
		
		private static readonly ConcurrentDictionary<string, bool> _supportedFormats = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
		private static long _totalProcessingTime;

		private readonly IEventPublisher _eventPublisher;

		static ImageSharpImageProcessor()
        {
			ImageSharpConfig.Default.MemoryAllocator = new SixLabors.ImageSharp.Memory.SimpleGcMemoryAllocator();

		}

		public ImageSharpImageProcessor(IEventPublisher eventPublisher)
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

			return _supportedFormats.GetOrAdd(extension, k =>
			{
				return ImageSharpConfig.Default.ImageFormats
					.SelectMany(x => x.FileExtensions)
					.Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase));
			});

			//return ImageProcessorBootstrapper.Instance.SupportedImageFormats
			//	.SelectMany(x => x.FileExtensions)
			//	.Any(x => x.Equals(extension, StringComparison.OrdinalIgnoreCase));
        }

		public ProcessImageResult ProcessImage(ProcessImageQuery query, bool disposeOutput = true)
		{
			Guard.NotNull(query, nameof(query));

			ValidateQuery(query);

			var watch = new Stopwatch();
			SourceImage source = null;

			try
			{
				watch.Start();

				// Load source
				source = LoadImage(query);

				// Pre-process event
				_eventPublisher.Publish(new ImageProcessingEvent(query, source.Image));

				var result = new ProcessImageResult
				{
					Query = query,
					SourceWidth = source.Image.Width,
					SourceHeight = source.Image.Height,
					SourceMimeType = source.Format.DefaultMimeType,
					DisposeOutputStream = disposeOutput
				};

				// Core processing
				ProcessImageCore(query, source.Image, out var fxApplied);

				// Create & prepare result
				//var outStream = new MemoryStream();
				//source.Image.Save(outStream, source.Format); // TODO: outFormat
				var dest = SaveImage(source, query);

				var fmt = dest.Format;
				result.FileExtension = fmt.FileExtensions.First();
				result.MimeType = fmt.DefaultMimeType;

				result.HasAppliedVisualEffects = fxApplied;
				result.Width = source.Image.Width;
				result.Height = source.Image.Height;

                if (source.Length > 0)
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

                    if (compare && source.Length <= dest.OutStream.Length)
                    {
						// Source is smaller. Throw away result and get back to source.
						dest.OutStream.Dispose();
						dest.OutStream = null;
						source.Stream.Position = 0;
						result.OutputStream = source.Stream;
                    }
                }

                // Set output stream
                if (result.OutputStream == null)
				{
					result.OutputStream = dest.OutStream;
				}

				// Post-process event
				_eventPublisher.Publish(new ImageProcessedEvent(query, source.Image, result));

				result.OutputStream.Position = 0;

				result.ProcessTimeMs = watch.ElapsedMilliseconds;				

				return result;
			}
			catch (Exception ex)
			{
				throw new ProcessImageException(query, ex);
			}
			finally
			{
				if (source?.Image != null)
                {
					source?.Image.Dispose();
                }

				if (source?.Stream != null && query.DisposeSource)
                {
					source?.Stream.Dispose();
                }

				watch.Stop();
				_totalProcessingTime += watch.ElapsedMilliseconds;
			}
		}

		private SourceImage LoadImage(ProcessImageQuery query)
        {
			var source = query.Source;
			
			Image image;
			IImageFormat format;
			Stream stream;
			long len;

			// Load source
			if (source is byte[] b)
			{
				stream = new MemoryStream(b);
				len = b.LongLength;
			}
			else if (source is Stream s)
			{
				stream = s;
				len = s.Length;
			}
			else if (source is string str)
			{
				var fi = new FileInfo(NormalizePath(str));
				stream = fi.OpenRead();
				len = fi.Length;
			}
			else if (source is IFile file)
			{
				len = file.Size;
				stream = file.OpenRead();
			}
			else
			{
				throw new ProcessImageException("Invalid source type '{0}' in query.".FormatInvariant(query.Source.GetType().FullName), query);
			}

			image = Image.Load(stream, out format);

			return new SourceImage
			{
				Image = image,
				Format = format,
				Stream = stream,
				Length = len,
				Width = image.Width,
				Height = image.Height
			};
		}

		private (MemoryStream OutStream, IImageFormat Format, IImageEncoder Encoder) SaveImage(SourceImage source, ProcessImageQuery query)
        {
			// Format
			IImageFormat format = null;
			if (query.Format != null)
			{
				format = query.Format as IImageFormat;

				if (format == null && query.Format is string)
				{
					var requestedFormat = ((string)query.Format).ToLowerInvariant();
					format = ImageSharpConfig.Default.ImageFormatsManager.FindFormatByFileExtension(requestedFormat);
				}
			}

			format ??= source.Format;

			// Encoder
			var encoder = ImageSharpConfig.Default.ImageFormatsManager.FindEncoder(format);

			// Set Quality
			if (query.Quality.HasValue && encoder is JpegEncoder)
			{
				// Create new encoder instance to avoid altering a shared static encoder instance.
				encoder = new JpegEncoder { Quality = query.Quality };
			}

			var outStream = new MemoryStream();
			source.Image.Save(outStream, encoder);

			return (outStream, format, encoder);
		}

		/// <summary>
		/// Processes the loaded image. Inheritors should NOT save the image, this is done by the main method. 
		/// </summary>
		/// <param name="query">Query</param>
		/// <param name="image">Processor instance</param>
		/// <param name="fxApplied">
		/// Should be true if any effect has been applied that potentially changes the image visually (like background color, contrast, sharpness etc.).
		/// Resize and compression quality does NOT count as FX.
		/// </param>
		protected virtual void ProcessImageCore(ProcessImageQuery query, Image image, out bool fxApplied)
		{
			bool fxAppliedPrivate = false;

			// Resize
			var size = query.MaxWidth != null || query.MaxHeight != null
				? new Size(query.MaxWidth ?? 0, query.MaxHeight ?? 0)
				: Size.Empty;

			image.Mutate(x => 
			{
				if (!size.IsEmpty && (image.Width > size.Width || image.Height > size.Height))
				{
					//image.Resize(new ResizeLayer(
					//	size,
					//	resizeMode: ConvertScaleMode(query.ScaleMode),
					//	anchorPosition: ConvertAnchorPosition(query.AnchorPosition),
					//	upscale: false));

					x.Resize(new ResizeOptions
					{
						Size = size,
						Mode = ConvertScaleMode(query.ScaleMode),
						Position = ConvertAnchorPosition(query.AnchorPosition)
					});
				}

				if (query.BackgroundColor.HasValue())
				{
					x.BackgroundColor(Color.ParseHex(query.BackgroundColor));
					fxAppliedPrivate = true;
				}
			});

            //// Format
            //if (query.Format != null)
            //{
            //    var format = query.Format as IImageFormat;

            //    if (format == null && query.Format is string)
            //    {
            //        var requestedFormat = ((string)query.Format).ToLowerInvariant();
            //        format = ImageSharpConfig.Default.ImageFormatsManager.FindFormatByFileExtension(requestedFormat);
            //    }

            //    if (format != null)
            //    {
            //        image.Format(format);
            //    }
            //}

            //// Set Quality
            //if (query.Quality.HasValue)
            //{
            //    image.Quality(query.Quality.Value);
            //}

            fxApplied = fxAppliedPrivate;
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

		private AnchorPositionMode ConvertAnchorPosition(string anchor)
		{
			switch (anchor.EmptyNull().ToLower())
			{
				case "top":
					return AnchorPositionMode.Top;
				case "bottom":
					return AnchorPositionMode.Bottom;
				case "left":
					return AnchorPositionMode.Left;
				case "right":
					return AnchorPositionMode.Right;
				case "top-left":
					return AnchorPositionMode.TopLeft;
				case "top-right":
					return AnchorPositionMode.TopRight;
				case "bottom-left":
					return AnchorPositionMode.BottomLeft;
				case "bottom-right":
					return AnchorPositionMode.BottomRight;
				default:
					return AnchorPositionMode.Center;
			}
		}
	}
}
