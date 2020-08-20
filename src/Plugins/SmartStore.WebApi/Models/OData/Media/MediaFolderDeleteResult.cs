using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData.Media
{
    [DataContract]
    public partial class MediaFolderDeleteResult
    {
        [DataMember]
        public ICollection<int> DeletedFolderIds { get; set; }

        [DataMember]
        public ICollection<string> DeletedFileNames { get; set; }
    }
}