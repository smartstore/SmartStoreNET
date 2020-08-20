using System.Runtime.Serialization;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
    public enum ImageDimension
    {
        VerySmall = 0,
        Small = 1,
        Medium = 2,
        Large = 3,
        VeryLarge = 4
    }

    [DataContract]
    public partial class MediaFilesFilter
    {
        [DataMember]
        [JsonProperty("mediaTypes")]
        public string[] MediaTypes { get; set; }

        [DataMember]
        [JsonProperty("mimeTypes")]
        public string[] MimeTypes { get; set; }

        [DataMember]
        [JsonProperty("extensions")]
        public string[] Extensions { get; set; }

        [DataMember]
        [JsonProperty("dimensions")]
        public ImageDimension[] Dimensions { get; set; }

        [DataMember]
        [JsonProperty("tags")]
        public int[] Tags { get; set; }

        [DataMember]
        [JsonProperty("hidden")]
        public bool? Hidden { get; set; }

        [DataMember]
        [JsonProperty("deleted")]
        public bool? Deleted { get; set; }

        [DataMember]
        [JsonProperty("term")]
        public string Term { get; set; }

        [DataMember]
        [JsonProperty("exact")]
        public bool ExactMatch { get; set; }

        [DataMember]
        [JsonProperty("includeAlt")]
        public bool IncludeAltForTerm { get; set; }
    }

    [DataContract]
    public partial class MediaSearchQuery : MediaFilesFilter
    {
        [DataMember]
        [JsonProperty("folderId")]
        public int? FolderId { get; set; }

        [DataMember]
        [JsonProperty("deep")]
        public bool DeepSearch { get; set; }


        [DataMember]
        [JsonProperty("page")]
        public int PageIndex { get; set; }

        [DataMember]
        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = int.MaxValue;

        [DataMember]
        [JsonProperty("sortBy")]
        public string SortBy { get; set; } = nameof(MediaFile.Id);

        [DataMember]
        [JsonProperty("sortDesc")]
        public bool SortDesc { get; set; }
    }
}
