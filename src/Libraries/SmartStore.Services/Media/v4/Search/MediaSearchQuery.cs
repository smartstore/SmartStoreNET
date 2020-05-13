using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    
    public partial class MediaFilesFilter
    {
        [JsonProperty("mediaTypes")]
        public string[] MediaTypes { get; set; }

        [JsonProperty("mimeTypes")]
        public string[] MimeTypes { get; set; }

        [JsonProperty("extensions")]
        public string[] Extensions { get; set; }

        [JsonProperty("dimensions")]
        public ImageDimension[] Dimensions { get; set; }

        [JsonProperty("tags")]
        public int[] Tags { get; set; }

        [JsonProperty("hidden")]
        public bool? Hidden { get; set; }

        [JsonProperty("deleted")]
        public bool? Deleted { get; set; }

        [JsonProperty("term")]
        public string Term { get; set; }

        [JsonProperty("exact")]
        public bool ExactMatch { get; set; }

        [JsonProperty("includeAlt")]
        public bool IncludeAltForTerm { get; set; }
    }

    public partial class MediaSearchQuery : MediaFilesFilter
    {
        [JsonProperty("folderId")]
        public int? FolderId { get; set; }

        [JsonProperty("deep")]
        public bool DeepSearch { get; set; }


        [JsonProperty("page")]
        public int PageIndex { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = int.MaxValue;

        [JsonProperty("sortBy")]
        public string SortBy { get; set; } = nameof(MediaFile.Name);

        [JsonProperty("sortDesc")]
        public bool SortDescending { get; set; }
    }
}
