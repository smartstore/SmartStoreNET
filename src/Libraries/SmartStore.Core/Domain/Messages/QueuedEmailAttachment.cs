using System;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Core.Domain.Messages
{
	/// <summary>
	/// Represents an e-mail attachment
	/// </summary>
	public partial class QueuedEmailAttachment : BaseEntity, IHasMedia
	{
		/// <summary>
		/// Gets or sets the queued email identifier
		/// </summary>
		public int QueuedEmailId { get; set; }

		/// <summary>
		/// Gets or sets the queued email entity instance
		/// </summary>
		public virtual QueuedEmail QueuedEmail { get; set; }

		/// <summary>
		/// Gets or sets the storage location
		/// </summary>
		public EmailAttachmentStorageLocation StorageLocation { get; set; }

		/// <summary>
		/// A physical or virtual path to the file (only applicable if location is <c>Path</c>)
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// The id of a <see cref="Download"/> record (only applicable if location is <c>FileReference</c>)
		/// </summary>
		public int? FileId { get; set; }

		/// <summary>
		/// Gets the file object
		/// </summary>
		/// <remarks>
		/// This property is not named <c>Download</c> on purpose, because we're going to rename Download to File in a future release.
		/// </remarks>
		public virtual Download File { get; set; }

		/// <summary>
		/// The attachment's binary data (only applicable if location is <c>Blob</c>)
		/// </summary>
		[Obsolete("Use property MediaStorage instead")]
		public byte[] Data { get; set; }

		/// <summary>
		/// The attachment file name (without path)
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The attachment file's mime type, e.g. <c>application/pdf</c>
		/// </summary>
		public string MimeType { get; set; }

		/// <summary>
		/// Gets or sets the media storage identifier
		/// </summary>
		public int? MediaStorageId { get; set; }

		/// <summary>
		/// Gets or sets the media storage
		/// </summary>
		public virtual MediaStorage MediaStorage { get; set; }
	}
}
