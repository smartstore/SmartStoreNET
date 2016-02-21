using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.Serialization;

namespace SmartStore.WebApi.Models.Api
{
	[DataContract]
	public abstract partial class UploadFileBase
	{
		public UploadFileBase()
		{
		}

		public UploadFileBase(HttpContentHeaders headers)
		{
			Name = headers.ContentDisposition.Name.ToUnquoted();
			FileName = headers.ContentDisposition.FileName.ToUnquoted();
			ContentDisposition = headers.ContentDisposition.Parameters;

			if (headers.ContentType != null)
			{
				MediaType = headers.ContentType.MediaType.ToUnquoted();
			}

			if (FileName.HasValue())
			{
				FileExtension = Path.GetExtension(FileName);
			}
		}

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
		/// Extension of FileName
		/// </summary>
		[DataMember]
		public string FileExtension { get; set; }

		/// <summary>
		/// Media (mime) type of content-type multipart header
		/// </summary>
		[DataMember]
		public string MediaType { get; set; }

		/// <summary>
		/// Indicates whether the uploaded file already exist
		/// </summary>
		[DataMember]
		public bool Exists { get; set; }

		/// <summary>
		/// Indicates whether the uploaded file has been inserted
		/// </summary>
		[DataMember]
		public bool Inserted { get; set; }

		/// <summary>
		/// Raw custom parameters of the content-disposition multipart header
		/// </summary>
		[DataMember]
		public ICollection<NameValueHeaderValue> ContentDisposition { get; set; }
	}
}