using ImageProcessor.Imaging.Formats;
using System.Collections.Generic;

namespace SmartStore.Services.Media.Imaging.Impl
{
    internal class IPImageFormat : IImageFormat
    {
        private readonly ISupportedImageFormat _format;

        public IPImageFormat(ISupportedImageFormat format)
        {
            Guard.NotNull(format, nameof(format));
            _format = format;
        }

        public ISupportedImageFormat WrappedFormat => _format;

        public string Name => _format.DefaultExtension;

        public string DefaultExtension => _format.DefaultExtension == "jpeg" ? "jpg" : _format.DefaultExtension;

        public string DefaultMimeType => _format.MimeType;

        public IEnumerable<string> FileExtensions => _format.FileExtensions;

        public IEnumerable<string> MimeTypes => new[] { _format.MimeType };
    }
}
