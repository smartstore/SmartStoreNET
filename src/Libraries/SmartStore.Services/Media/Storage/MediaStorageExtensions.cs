using System.IO;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media.Storage
{
	public static class MediaStorageExtensions
	{
		/// <summary>
		/// Converts a picture entity into a media storage item
		/// </summary>
		/// <param name="picture">Picture entity</param>
		/// <returns>Media storage item</returns>
		public static MediaItem ToMedia(this Picture picture)
		{
			Guard.NotNull(picture, nameof(picture));

			var media = new MediaItem
			{
				Entity = picture,
				Path = "Storage",
				MimeType = picture.MimeType
			};

			return media;
		}

		/// <summary>
		/// Converts a download entity into a media storage item
		/// </summary>
		/// <param name="download">Download entity</param>
		/// <returns>Media storage item</returns>
		public static MediaItem ToMedia(this Download download)
		{
			Guard.NotNull(download, nameof(download));

			var media = new MediaItem
			{
				Entity = download,
				MimeType = download.ContentType,
				FileExtension = download.Extension,
				Path = "Downloads"
			};

			return media;
		}

		/// <summary>
		/// Converts a queued email attachment entity into a media storage item
		/// </summary>
		/// <param name="attachment">Queued email attachment</param>
		/// <returns>Media storage item</returns>
		public static MediaItem ToMedia(this QueuedEmailAttachment attachment)
		{
			Guard.NotNull(attachment, nameof(attachment));

			var media = new MediaItem
			{
				Entity = attachment,
				MimeType = attachment.MimeType,
				FileExtension = Path.GetExtension(attachment.Name),
				Path = "QueuedEmailAttachment"
			};

			return media;
		}

		/// <summary>
		/// Create a file name for a media item
		/// </summary>
		/// <param name="media">Media storage item</param>
		/// <returns>File name including extension</returns>
		public static string GetFileName(this MediaItem media)
		{
			if (media != null)
			{
				var extension = media.FileExtension;

				if (extension.IsEmpty())
					extension = MimeTypes.MapMimeTypeToExtension(media.MimeType);

				var baseEntity = media.Entity as BaseEntity;

				var fileName = string.Concat(
					baseEntity.Id.ToString(ImageCache.IdFormatString), 
					extension.EmptyNull().EnsureStartsWith("."));

				return fileName;
			}

			return null;
		}
	}
}
