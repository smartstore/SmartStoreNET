using System.Runtime.Serialization;
using System.Web.OData.Builder;
using SmartStore.Core.Domain.Media;

namespace SmartStore.WebApi.Models.OData.Media
{
    /// <summary>
    /// File information returned by the API.
    /// </summary>
    /// <remarks>
    /// Should not be inherited from MediaFile because then navigation properties cannot be expanded using $expand (e.g. $expand=File($expand=Tracks)).
    /// Should not use [Contained] because throws "The Path property in ODataMessageWriterSettings.ODataUri must be set when writing contained elements".
    /// </remarks>
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

        /// <remarks>AutoExpand only works with <see cref="WebApiQueryableAttribute" />.</remarks>
        [DataMember]
        [AutoExpand]
        public MediaFile File { get; set; }
    }
}