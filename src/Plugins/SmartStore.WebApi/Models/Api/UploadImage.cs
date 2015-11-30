using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Media;

namespace SmartStore.WebApi.Models.Api
{
	[DataContract]
	public partial class UploadImage
	{
		/// <summary>
		/// Unquoted name attribute of content-disposition multipart header
		/// </summary>
		[DataMember]
		public string Name { get; set; }

		/// <summary>
		/// Unquoted filename attribute of content-disposition multipart header
		/// </summary>
		[DataMember]
		public string FileName { get; set; }

		/// <summary>
		/// Media (mime) type of content-type multipart header
		/// </summary>
		[DataMember]
		public string MediaType { get; set; }

		/// <summary>
		/// Indicates whether the uploaded image already exist and therefore has been skipped
		/// </summary>
		[DataMember]
		public bool Exists { get; set; }

		/// <summary>
		/// Indicates whether the uploaded image has been inserted
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
		/// Raw custom parameters of the content-disposition multipart header
		/// </summary>
		[DataMember]
		public ICollection<NameValueHeaderValue> ContentDisposition { get; set; }

		/// <summary>
		/// The picture entity. Can be null.
		/// </summary>
		[DataMember]
		public Picture Picture { get; set; }
	}
}