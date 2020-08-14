using System.Runtime.Serialization;
using SmartStore.Core.Domain.Media;

namespace SmartStore.WebApi.Models.OData.Media
{
    /// <summary>
    /// File information returned by the API.
    /// </summary>
    [DataContract]
    public partial class FileItemInfo
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Directory { get; set; }

        [DataMember]
        public string Path { get; set; }

        [DataMember]
        public string Url { get; set; }

        [DataMember]
        public string ThumbUrl { get; set; }

        [DataMember]
        public MediaFile File { get; set; }
    }
}