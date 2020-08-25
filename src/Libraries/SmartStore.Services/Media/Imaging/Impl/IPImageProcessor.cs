using System;
using System.IO;
using System.Linq;
using ImageProcessor;
using ImageProcessor.Configuration;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using SmartStore.Core.Events;

namespace SmartStore.Services.Media.Imaging.Impl
{
    public partial class IPImageProcessor : ImageProcessorBase
    {
		static IPImageProcessor()
		{
			ImageProcessorBootstrapper.Instance.AddImageFormats(new WebPFormat { Quality = 100 });
		}

		public IPImageProcessor(IEventPublisher eventPublisher)
			: base(eventPublisher)
		{
		}

        public override bool IsSupportedImage(string extension)
        {
            return GetInternalImageFormat(extension) != null;
        }

        public override IImageFormat GetImageFormat(string extension)
        {
			var internalFormat = GetInternalImageFormat(extension);
			if (internalFormat != null)
            {
				return new IPImageFormat(internalFormat);
            }

			return null;
        }

		private ISupportedImageFormat GetInternalImageFormat(string extension)
        {
			if (extension.IsEmpty())
			{
				return null;
			}

			if (extension[0] == '.' && extension.Length > 1)
			{
				extension = extension.Substring(1);
			}

			return ImageProcessorBootstrapper.Instance.SupportedImageFormats
				.FirstOrDefault(x => x.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
		}

        public override IProcessableImage LoadImage(string path)
        {
			var internalImage = new ImageFactory(preserveExifData: false, fixGamma: false)
				.Load(NormalizePath(path));

			return new IPImage(internalImage);
		}

		public override IProcessableImage LoadImage(Stream stream)
		{
			var internalImage = new ImageFactory(preserveExifData: false, fixGamma: false)
				.Load(stream);

			return new IPImage(internalImage);
		}
	}
}
