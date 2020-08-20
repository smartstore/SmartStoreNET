using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData.Media
{
    [DataContract]
    public partial class MediaFolderOperationResult
    {
        [DataMember]
        public FolderItemInfo Folder { get; set; }

        [DataMember]
        public ICollection<DuplicateFileInfo> DuplicateFiles { get; set; }

        [DataContract]
        public class DuplicateFileInfo
        {
            [DataMember]
            public FileItemInfo SourceFile { get; set; }

            [DataMember]
            public FileItemInfo DestinationFile { get; set; }

            [DataMember]
            public string UniquePath { get; set; }
        }
    }
}