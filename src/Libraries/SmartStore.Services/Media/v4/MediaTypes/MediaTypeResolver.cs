using System;
using System.Linq;
using System.Collections.Generic;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Events;
using SmartStore.Core.IO;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Data.Hooks;
using SmartStore.Utilities;

namespace SmartStore.Services.Media
{
    public partial class MediaTypeResolver : DbSaveHook<Setting>, IMediaTypeResolver
    {
        const string MapCacheKey = "media:exttypemap";
        
        private readonly ICacheManager _cache;
        private readonly Lazy<MediaSettings> _mediaSettings;

        private static HashSet<string> _mapInvalidatorSettingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            TypeHelper.NameOf<MediaSettings>(x => x.ImageTypes, true),
            TypeHelper.NameOf<MediaSettings>(x => x.VideoTypes, true),
            TypeHelper.NameOf<MediaSettings>(x => x.AudioTypes, true),
            TypeHelper.NameOf<MediaSettings>(x => x.DocumentTypes, true),
            TypeHelper.NameOf<MediaSettings>(x => x.TextTypes, true)
        };
        
        public MediaTypeResolver(ICacheManager cache, Lazy<MediaSettings> mediaSettings)
        {
            _cache = cache;
            _mediaSettings = mediaSettings;
        }

        public MediaType Resolve(MediaFile file)
        {
            Guard.NotNull(file, nameof(file));

            var extension = file.Extension;

            if (extension.IsEmpty())
            {
                extension = MimeTypes.MapMimeTypeToExtension(file.MimeType);
            }

            var map = GetExtensionMediaTypeMap();

            if (extension.HasValue() && map.TryGetValue(file.Extension.ToLower(), out var mediaType))
            {
                return (MediaType)mediaType;
            }

            // Get first mime token (e.g. IMAGE/png, VIDEO/mp4 etc.)
            var type = file.MimeType.Split('/')[0];

            return MediaType.GetMediaType(type) ?? MediaType.Binary;
        }

        private Dictionary<string, string> GetExtensionMediaTypeMap()
        {
            return _cache.Get(MapCacheKey, () => 
            {
                var mediaSettings = _mediaSettings.Value;
                var map = new Dictionary<string, string>();

                AddExtensionsToMap(mediaSettings.ImageTypes, MediaType.Image);
                AddExtensionsToMap(mediaSettings.VideoTypes, MediaType.Video);
                AddExtensionsToMap(mediaSettings.AudioTypes, MediaType.Audio);
                AddExtensionsToMap(mediaSettings.DocumentTypes, MediaType.Document);
                AddExtensionsToMap(mediaSettings.TextTypes, MediaType.Text);

                return map;

                void AddExtensionsToMap(string extensions, MediaType forType)
                {
                    var arr = extensions.EmptyNull()
                        .Replace(Environment.NewLine, " ")
                        .ToLower()
                        .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (arr.Length == 0)
                    {
                        arr = forType.DefaultExtensions;
                    }

                    arr.Each(x => map[x] = forType.Name);
                }
            });
        }

        #region Invalidation Hook

        public override void OnAfterSave(IHookedEntity entry)
        {
            var setting = entry.Entity as Setting;
            if (_mapInvalidatorSettingKeys.Contains(setting.Name))
            {
                _cache.Remove(MapCacheKey);
            }
        }

        #endregion
    }
}
