using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData.Media
{
    [DataContract]
    public partial class MediaFolderOperationResult
    {
        [DataMember]
        public int FolderId { get; set; }

        // TODO: add properties later. Complex Type with navigation property requires OData ASP.NET Web API V6.
        // See https://docs.microsoft.com/en-us/odata/webapi/complextypewithnavigationproperty
        //[DataMember]
        //public FolderNodeInfo Folder { get; set; }

        [DataMember]
        public ICollection<DuplicateFileInfo> DuplicateFiles { get; set; }

        [DataContract]
        public partial class DuplicateFileInfo
        {
            [DataMember]
            public int SourceFileId { get; set; }

            [DataMember]
            public int DestinationFileId { get; set; }

            //[DataMember]
            //public FileItemInfo SourceFile { get; set; }

            //[DataMember]
            //public FileItemInfo DestinationFile { get; set; }

            [DataMember]
            public string UniquePath { get; set; }
        }
    }
}