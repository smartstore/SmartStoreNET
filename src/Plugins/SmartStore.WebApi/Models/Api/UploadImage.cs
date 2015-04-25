using System.Runtime.Serialization;
using SmartStore.Core.Domain.Media;

namespace SmartStore.WebApi.Models.Api
{
	[DataContract]
	public partial class UploadImage
	{
		/// <summary>
		/// Name attribute of content-disposition request header
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Filename attribute of content-disposition request header
		/// </summary>
		[DataMember]
		public string FileName { get; set; }

		/// <summary>
		/// Media (mime) type of content-type request header
		/// </summary>
		[DataMember]
		public string MediaType { get; set; }

		/// <summary>
		/// Indicates whether the uploaded image already exist and therefore has been skipped
		/// </summary>
		[DataMember]
		public bool Exists { get; set; }

		/// <summary>
		/// Indicates whether the uploaded image was inserted
		/// </summary>
		[DataMember]
		public bool Inserted { get; set; }

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