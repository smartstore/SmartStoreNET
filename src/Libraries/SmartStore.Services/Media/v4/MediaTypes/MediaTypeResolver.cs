using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{
    public partial class MediaTypeResolver : IMediaTypeResolver
    {
        private readonly MediaSettings _mediaSettings;
        private IDictionary<string, MediaType> _map;
        
        public MediaTypeResolver(MediaSettings mediaSettings)
        {
            _mediaSettings = mediaSettings;
        }

        public MediaType Resolve(MediaFile file)
        {
            Guard.NotNull(file, nameof(file));

            EnsureMap();

            var extension = file.Extension;

            if (extension.IsEmpty())
            {
                extension = MimeTypes.MapMimeTypeToExtension(file.MimeType);
            }

            if (extension.HasValue() && _map.TryGetValue(file.Extension, out var mediaType))
            {
                return mediaType;
            }

            // Get first mime token (e.g. IMAGE/png, VIDEO/mp4 etc.)
            var type = file.MimeType.Split('/')[0];

            return MediaType.GetMediaType(type) ?? MediaType.Binary;
        }

        private void EnsureMap()
        {
            if (_map != null)
                return;
            
            _map = new Dictionary<string, MediaType>(StringComparer.OrdinalIgnoreCase);

            AddExtensionsToMap(_mediaSettings.ImageTypes, MediaType.Image);
            AddExtensionsToMap(_mediaSettings.VideoTypes, MediaType.Video);
            AddExtensionsToMap(_mediaSettings.AudioTypes, MediaType.Audio);
            AddExtensionsToMap(_mediaSettings.DocumentTypes, MediaType.Document);
            AddExtensionsToMap(_mediaSettings.TextTypes, MediaType.Text);

            void AddExtensionsToMap(string extensions, MediaType forType)
            {
                var arr = extensions.EmptyNull()
                    .Replace(Environment.NewLine, " ")
                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (arr.Length == 0)
                {
                    arr = forType.DefaultExtensions;
                }

                arr.Each(x => _map[x] = forType);
            }
        }
    }
}
