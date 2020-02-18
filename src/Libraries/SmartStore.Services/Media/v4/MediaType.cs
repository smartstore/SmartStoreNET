using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.Media
{
    public class MediaType : IEquatable<MediaType>
    {
        private readonly static IDictionary<string, MediaType> _map = new Dictionary<string, MediaType>(StringComparer.OrdinalIgnoreCase);

        public readonly static MediaType Image = new MediaType("image");
        public readonly static MediaType Video = new MediaType("video");
        public readonly static MediaType Audio = new MediaType("audio");
        public readonly static MediaType Document = new MediaType("document");
        public readonly static MediaType Text = new MediaType("text");
        public readonly static MediaType Binary = new MediaType("bin");

        protected MediaType(string name)
        {
            Guard.NotEmpty(name, nameof(name));

            Name = name;
            _map[name] = this;
        }

        public string Name { get; private set; }

        public override string ToString() => Name;

        public static implicit operator string(MediaType obj) => obj.Name;

        public static implicit operator MediaType(string obj) => GetMediaType(obj);

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

            throw new InvalidCastException("No media type has been registered for '{0}'.".FormatInvariant(name));
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
