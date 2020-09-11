using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Media;

namespace SmartStore.WebApi.Models.Api
{
    /// <summary>
    /// Information about an uploaded image returned by the API.
    /// </summary>
    [DataContract]
    public partial class UploadImage : UploadFileBase
    {
        public UploadImage()
        {
        }

        public UploadImage(HttpContentHeaders headers)
            : base(headers)
        {
            if (headers.ContentDisposition.Parameters != null)
            {
                var pictureId = headers.ContentDisposition.Parameters.FirstOrDefault(x => x.Name == "PictureId");
                if (pictureId != null)
                {
                    PictureId = pictureId.Value.ToUnquoted().ToInt();
                }
            }
        }

        /// <summary>
        /// URL of the default size image.
        /// </summary>
        [DataMember]
        public string ImageUrl { get; set; }

        /// <summary>
        /// URL of the thumbnail image.
        /// </summary>
        [DataMember]
        public string ThumbImageUrl { get; set; }

        /// <summary>
        /// URL of the full size image.
        /// </summary>
        [DataMember]
        public string FullSizeImageUrl { get; set; }

        /// <summary>
        /// Optional media file identifier. Can be 0.
        /// </summary>
        [DataMember]
        public int PictureId { get; set; }

        /// <summary>
        /// The media file entity. Can be <c>null</c>.
        /// </summary>
        [DataMember]
        public MediaFile Picture { get; set; }
    }
}