using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.OData.Media
{
    [DataContract]
    public partial class MediaFileOperationResult
    {
        [DataMember]
        public int DestinationFileId { get; set; }

        // TODO: add property later. Complex Type with navigation property requires OData ASP.NET Web API V6.
        // See https://docs.microsoft.com/en-us/odata/webapi/complextypewithnavigationproperty
        //[DataMember]
        //public FileItemInfo DestinationFile { get; set; }

        [DataMember]
        public bool IsDuplicate { get; set; }

        [DataMember]
        public string UniquePath { get; set; }
    }
}