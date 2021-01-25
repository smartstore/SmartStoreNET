using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Services.Media
{
    public class MediaType : IEquatable<MediaType>
    {
        private readonly static IDictionary<string, string[]> _defaultExtensionsMap = new Dictionary<string, string[]>
        {
            ["image"] = new[] { "png", "jpg", "jpeg", "jfif", "gif", "webp", "bmp", "svg", "ico" },
            ["video"] = new[] { "mp4", "m4v", "mkv", "wmv", "avi", "asf", "mpg", "mpeg", "webm", "flv", "ogv", "mov", "3gp" },
            ["audio"] = new[] { "mp3", "wav", "wma", "aac", "flac", "oga", "wav", "m4a", "ogg" },
            ["document"] = new[] { "pdf", "doc", "docx", "ppt", "pptx", "pps", "ppsx", "docm", "odt", "ods", "dot", "dotx", "dotm", "psd", "xls", "xlsx", "rtf" },
            ["text"] = new[] { "txt", "xml", "csv", "htm", "html", "json", "css", "js" },
            ["bin"] = new string[0]
        };

        private readonly static IDictionary<string, MediaType> _map = new Dictionary<string, MediaType>(StringComparer.OrdinalIgnoreCase);

        public readonly static MediaType Image = new MediaType("image", _defaultExtensionsMap["image"]);
        public readonly static MediaType Video = new MediaType("video", _defaultExtensionsMap["video"]);
        public readonly static MediaType Audio = new MediaType("audio", _defaultExtensionsMap["audio"]);
        public readonly static MediaType Document = new MediaType("document", _defaultExtensionsMap["document"]);
        public readonly static MediaType Text = new MediaType("text", _defaultExtensionsMap["text"]);
        public readonly static MediaType Binary = new MediaType("bin", _defaultExtensionsMap["bin"]);

        protected MediaType(string name, params string[] defaultExtensions)
        {
            Guard.NotEmpty(name, nameof(name));

            Name = name;
            DefaultExtensions = defaultExtensions.OrderBy(x => x).ToArray();

            _map[name] = this;
        }

        public string Name { get; private set; }

        public string[] DefaultExtensions { get; private set; }

        public static IEnumerable<string> AllExtensions => _defaultExtensionsMap.SelectMany(x => x.Value);

        public override string ToString()
        {
            return Name;
        }

        public static implicit operator string(MediaType obj)
        {
            return obj?.Name;
        }

        public static implicit operator MediaType(string obj)
        {
            return GetMediaType(obj);
        }

        internal static MediaType GetMediaType(string name)
        {
            if (name.IsEmpty())
            {
                return null;
            }

            if (_map.TryGetValue(name, out var instance))
            {
                return instance;
            }

            return null;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return obj.GetType() == GetType() && Equals((MediaType)obj);
        }

        public bool Equals(MediaType other)
        {
            return string.Equals(Name, other.Name);
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }
    }
}
