using ImageProcessor.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Media.Impl
{
    internal class IPImageFormat : IImageFormat
    {
        private readonly ISupportedImageFormat _format;
        
        public IPImageFormat(ISupportedImageFormat format)
        {
            Guard.NotNull(format, nameof(format));
            _format = format;
        }

        public ISupportedImageFormat WrappedFormat
        {
            get => _format;
        }

        public string Name => _format.MimeType;

        public string DefaultExtension => _format.DefaultExtension == "jpeg" ? "jpg" : _format.DefaultExtension;

        public string DefaultMimeType => _format.MimeType;

        public IEnumerable<string> FileExtensions => _format.FileExtensions;

        public IEnumerable<string> MimeTypes => new[] { _format.MimeType };

        //public override bool Equals(object obj)
        //{
        //    if (!(obj is IImageFormat format))
        //    {
        //        return false;
        //    }

        //    return this.DefaultMimeType.Equals(format.DefaultMimeType);
        //}

        //public override int GetHashCode()
        //{
        //    unchecked
        //    {
        //        int hashCode = this.DefaultMimeType.GetHashCode();
        //        hashCode = (hashCode * 397) ^ this.IsIndexed.GetHashCode();
        //        return (hashCode * 397) ^ this.Quality;
        //    }
        //}
    }
}
