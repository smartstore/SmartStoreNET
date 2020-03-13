using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmartStore.Services.Media
{
    public partial class MediaSearchQuery
    {
        [JsonProperty("folderId")]
        public int? FolderId { get; set; }

        [JsonProperty("deep")]
        public bool DeepSearch { get; set; }

        [JsonProperty("mediaTypes")]
        public string[] MediaTypes { get; set; }

        [JsonProperty("mimeTypes")]
        public string[] MimeTypes { get; set; }

        [JsonProperty("extensions")]
        public string[] Extensions { get; set; }

        [JsonProperty("tags")]
        public int[] Tags { get; set; }

        [JsonProperty("hidden")]
        public bool? Hidden { get; set; }

        [JsonProperty("deleted")]
        public bool? Deleted { get; set; }

        [JsonProperty("term")]
        public string Term { get; set; }


        [JsonProperty("page")]
        public int PageIndex { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = int.MaxValue;

        [JsonProperty("sortBy")]
        public string SortBy { get; set; }

        [JsonProperty("sortDesc")]
        public bool SortDescending { get; set; }
    }
}
