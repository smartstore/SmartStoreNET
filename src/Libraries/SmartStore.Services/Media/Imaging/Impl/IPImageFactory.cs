using System;
using System.IO;
using System.Linq;
using ImageProcessor;
using ImageProcessor.Configuration;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Plugins.WebP.Imaging.Formats;

namespace SmartStore.Services.Media.Imaging.Impl
{
    public class IPImageFactory : IImageFactory
    {
        static IPImageFactory()
        {
            ImageProcessorBootstrapper.Instance.AddImageFormats(new WebPFormat { Quality = 100 });
        }

        public bool IsSupportedImage(string extension)
        {
            return GetInternalImageFormat(extension) != null;
        }

        public IImageFormat GetImageFormat(string extension)
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

        public IProcessableImage LoadImage(string path, bool preserveExif = false)
        {
            return new IPImage(new ImageFactory(preserveExif, fixGamma: false).Load(path));
        }

        public IProcessableImage LoadImage(Stream stream, bool preserveExif = false)
        {
            return new IPImage(new ImageFactory(preserveExif, fixGamma: false).Load(stream));
        }
    }
}
