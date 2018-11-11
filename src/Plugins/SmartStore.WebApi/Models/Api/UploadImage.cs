using System.Net.Http.Headers;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Media;

namespace SmartStore.WebApi.Models.Api
{
	[DataContract]
	public partial class UploadImage : UploadFileBase
	{
		public UploadImage()
		{
		}

		public UploadImage(HttpContentHeaders headers) : base(headers)
		{
		}

		/// <summary>
		/// Url of the default size image
		/// </summary>
		[DataMember]
		public string ImageUrl { get; set; }

		/// <summary>
		/// Url of the thumbnail image
		/// </summary>
		[DataMember]
		public string ThumbImageUrl { get; set; }

		/// <summary>
		/// Url of the full size image
		/// </summary>
		[DataMember]
		public string FullSizeImageUrl { get; set; }

		/// <summary>
		/// The picture entity. Can be null.
		/// </summary>
		[DataMember]
		public Picture Picture { get; set; }
	}
}