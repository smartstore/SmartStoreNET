using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData.Media
{
    [DataContract]
    public partial class MediaFileOperationResult
    {
        [DataMember]
        public FileItemInfo DestinationFile { get; set; }

        [DataMember]
        public bool IsDuplicate { get; set; }

        [DataMember]
        public string UniquePath { get; set; }
    }
}