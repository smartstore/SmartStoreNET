using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SmartStore.Services.Media
{
    public partial class MediaQuery
    {
        [JsonProperty("folders")]
        public int[] FolderIds { get; set; }

        [JsonProperty("mediaTypes")]
        public string[] MediaTypes { get; set; }

        [JsonProperty("mimeTypes")]
        public string[] MimeTypes { get; set; }

        [JsonProperty("extensions")]
        public string[] Extensions { get; set; }

        [JsonProperty("term")]
        public string Term { get; set; } // TODO: (mm) convert pattern

        [JsonProperty("hidden")]
        public bool IncludeHidden { get; set; }

        [JsonProperty("deleted")]
        public bool IncludeDeleted { get; set; }


        [JsonProperty("skip")]
        public int Skip { get; set; }

        [JsonProperty("take")]
        public int Take { get; set; } = int.MaxValue;

        [JsonProperty("sortBy")]
        public string SortBy { get; set; }

        [JsonProperty("sortDesc")]
        public string SortDescending { get; set; }
    }
}
