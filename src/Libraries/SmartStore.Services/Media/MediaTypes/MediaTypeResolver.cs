using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
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
            TypeHelper.NameOf<MediaSettings>(x => x.TextTypes, true),
            TypeHelper.NameOf<MediaSettings>(x => x.BinTypes, true)
        };

        public MediaTypeResolver(ICacheManager cache, Lazy<MediaSettings> mediaSettings)
        {
            _cache = cache;
            _mediaSettings = mediaSettings;
        }

        public MediaType Resolve(string extension, string mimeType = null)
        {
            if (extension.IsEmpty() && mimeType.HasValue())
            {
                extension = MimeTypes.MapMimeTypeToExtension(mimeType);
            }

            var map = GetExtensionMediaTypeMap();

            string mediaType = null;
            if (extension.HasValue() && map.TryGetValue(extension.TrimStart('.').ToLower(), out mediaType))
            {
                return (MediaType)mediaType;
            }

            if (mimeType.HasValue())
            {
                // Get first mime token (e.g. IMAGE/png, VIDEO/mp4 etc.)
                var mimeGroup = mimeType.Split('/')[0];
                mediaType = MediaType.GetMediaType(mimeGroup);
            }

            return (MediaType)mediaType ?? MediaType.Binary;
        }

        public IEnumerable<string> ParseTypeFilter(string typeFilter)
        {
            if (typeFilter.IsEmpty() || typeFilter == "*")
            {
                return GetExtensionMediaTypeMap().Keys;
            }
            else
            {
                return ParseTypeFilter(typeFilter.SplitSafe(","));
            }
        }

        public IEnumerable<string> ParseTypeFilter(string[] typeFilter)
        {
            if (typeFilter == null || typeFilter.Length == 0)
                return Enumerable.Empty<string>();

            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var filter in typeFilter.Select(x => x.Trim()))
            {
                if (filter[0] == '.')
                {
                    extensions.Add(filter.Substring(1));
                }
                else
                {
                    extensions.AddRange(GetExtensionMediaTypeMap().Where(x => filter == "*" || x.Value == filter).Select(x => x.Key));
                }
            }

            return extensions;
        }

        public IReadOnlyDictionary<string, string> GetExtensionMediaTypeMap()
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
                AddExtensionsToMap(mediaSettings.BinTypes, MediaType.Binary);

                return map;

                void AddExtensionsToMap(string extensions, MediaType forType)
                {
                    var arr = extensions.EmptyNull()
                        .Replace(Environment.NewLine, " ")
                        .ToLower()
                        .Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (arr.Length == 0)
                    {
                        arr = forType.DefaultExtensions;
                    }

                    arr.Each(x => map[x.Trim()] = forType.Name);
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
