using SmartStore.Core;

namespace SmartStore.Services.Media.Storage
{
	public partial class MediaStorageItem
	{
		public string RootPath { get; set; }

		/// <summary>
		/// Entity of the media storage item. Must support <c>IBinaryDataSupported</c>.
		/// </summary>
		public BaseEntity Entity { get; set; }

		/// <summary>
		/// New binary data
		/// </summary>
		public byte[] NewData { get; set; }

		/// <summary>
		/// Mime type
		/// </summary>
		public string MimeType { get; set; }
	}
}
