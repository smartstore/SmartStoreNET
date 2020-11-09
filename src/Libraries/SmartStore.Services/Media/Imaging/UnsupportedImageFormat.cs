using System;
using System.Collections.Generic;

namespace SmartStore.Services.Media.Imaging
{
    public class UnsupportedImageFormat : IImageFormat
    {
        public UnsupportedImageFormat(string mimeType, string extension)
        {
            Guard.NotEmpty(mimeType, nameof(mimeType));
            Guard.NotEmpty(extension, nameof(extension));

            if (extension[0] == '.' && extension.Length > 1)
            {
                extension = extension.Substring(1);
            }

            Name = extension;
            DefaultMimeType = mimeType;
            DefaultExtension = extension;
        }

        public string Name { get; }
        public string DefaultExtension { get; }
        public string DefaultMimeType { get; }

        public IEnumerable<string> FileExtensions => new string[] { DefaultExtension };

        public IEnumerable<string> MimeTypes => new string[] { DefaultMimeType };
    }
}
