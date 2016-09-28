using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media.Storage
{
	public partial class MediaItem
	{
		/// <summary>
		/// Entity of the media storage item
		/// </summary>
		public IHasMedia Entity { get; set; }

		/// <summary>
		/// Storage path
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Mime type
		/// </summary>
		public string MimeType { get; set; }

		/// <summary>
		/// File extension
		/// </summary>
		public string FileExtension { get; set; }
	}
}
