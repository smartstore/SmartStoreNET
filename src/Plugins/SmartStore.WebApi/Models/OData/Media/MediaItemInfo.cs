using System.Runtime.Serialization;
using SmartStore.Core.Domain.Media;

namespace SmartStore.WebApi.Models.OData
{
    /// <summary>
    /// Information about a media file returned by the API.
    /// </summary>
    [DataContract]
    public partial class MediaItemInfo
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