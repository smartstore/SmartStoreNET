using System.Runtime.Serialization;
using SmartStore.Core.Domain.Media;

namespace SmartStore.WebApi.Models.Api
{
	[DataContract]
	public partial class UploadImage
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string FileName { get; set; }

		[DataMember]
		public string MediaType { get; set; }

		[DataMember]
		public bool Exists { get; set; }

		[DataMember]
		public bool Inserted { get; set; }

		[DataMember]
		public string ImageUrl { get; set; }

		[DataMember]
		public string ThumbImageUrl { get; set; }

		[DataMember]
		public string FullSizeImageUrl { get; set; }

		[DataMember]
		public Picture Picture { get; set; }
	}
}