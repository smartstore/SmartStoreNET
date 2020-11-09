using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData.Media
{
    [DataContract]
    public partial class MediaCountResult
    {
        [DataMember]
        public int Total { get; set; }

        [DataMember]
        public int Trash { get; set; }

        [DataMember]
        public int Unassigned { get; set; }

        [DataMember]
        public int Transient { get; set; }

        [DataMember]
        public int Orphan { get; set; }

        [DataMember]
        public ICollection<FolderCount> Folders { get; set; }

        [DataContract]
        public partial class FolderCount
        {
            [DataMember]
            public int FolderId { get; set; }

            [DataMember]
            public int Count { get; set; }
        }
    }
}