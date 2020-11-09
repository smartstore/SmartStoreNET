using System;
using Newtonsoft.Json;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI
{
    public class IconDescription : IEquatable<IconDescription>
    {
        [JsonIgnore]
        public string Name { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("unicode")]
        public string Unicode { get; set; }

        [JsonProperty("styles")]
        public string[] Styles { get; set; }

        [JsonProperty("search")]
        public Search SearchInfo { get; set; }

        [JsonIgnore]
        public bool IsBrandIcon
        {
            get;
            internal set;
        }

        [JsonIgnore]
        public bool HasRegularStyle
        {
            get;
            internal set;
        }

        [JsonIgnore]
        public bool IsPro
        {
            get;
            internal set;
        }

        public string GetCssClass(string style)
        {
            var prefix = "fa";

            if (IsBrandIcon)
            {
                prefix = "fab";
            }
            else
            {
                switch (style)
                {
                    case "solid":
                    case "fas":
                        prefix = "fas";
                        break;
                    case "regular":
                    case "far":
                        prefix = "far";
                        break;
                    case "light":
                    case "fal":
                        prefix = "fal";
                        break;
                    case "duotone":
                    case "fad":
                        prefix = "fad";
                        break;
                }
            }

            return string.Concat(prefix, " fa-", Name);
        }

        public override bool Equals(object other)
        {
            return this.Equals(other as IconDescription);
        }

        public bool Equals(IconDescription other)
        {
            if (other == null)
                return false;

            return this.Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCodeCombiner.Start().Add(typeof(IconDescription)).Add(this.Name).CombinedHash;
        }

        public class Search
        {
            [JsonProperty("terms")]
            public string[] Terms { get; set; }
        }
    }
}
