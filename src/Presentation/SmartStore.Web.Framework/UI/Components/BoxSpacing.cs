using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace SmartStore.Web.Framework.UI
{
    public class BoxSpacing
    {
        [Range(0, 6)]
        [JsonProperty("t")]
        public byte? Top { get; set; }

        [Range(0, 6)]
        [JsonProperty("r")]
        public byte? Right { get; set; }

        [Range(0, 6)]
        [JsonProperty("b")]
        public byte? Bottom { get; set; }

        [Range(0, 6)]
        [JsonProperty("l")]
        public byte? Left { get; set; }
    }
}
