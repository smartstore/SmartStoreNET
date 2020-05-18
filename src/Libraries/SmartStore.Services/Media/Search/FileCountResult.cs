using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SmartStore.Services.Media
{
    public partial class FileCountResult
    {
        [JsonIgnore]
        public MediaFilesFilter Filter { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("trash")]
        public int Trash { get; set; }

        [JsonProperty("unassigned")]
        public int Unassigned { get; set; }

        [JsonProperty("transient")]
        public int Transient { get; set; }

        [JsonProperty("orphan")]
        public int Orphan { get; set; }

        [JsonProperty("folders")]
        public IDictionary<int, int> Folders { get; set; }
    }
}
