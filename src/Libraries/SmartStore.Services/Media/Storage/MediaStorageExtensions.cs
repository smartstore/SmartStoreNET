using SmartStore.Core;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media.Storage
{
	public static class MediaStorageExtensions
	{
		/// <summary>
		/// Convert a picture entity into a media storage item
		/// </summary>
		/// <param name="picture">Picture entity</param>
		/// <returns>Media storage item</returns>
		public static MediaStorageItem ToMedia(this Picture picture)
		{
			Guard.ArgumentNotNull(() => picture);

			var media = new MediaStorageItem
			{
				Entity = picture,
				Path = "Media",
				MimeType = picture.MimeType
			};

			return media;
		}

		/// <summary>
		/// Convert a download entity into a media storage item
		/// </summary>
		/// <param name="download">Download entity</param>
		/// <returns>Media storage item</returns>
		public static MediaStorageItem ToMedia(this Download download)
		{
			Guard.ArgumentNotNull(() => download);

			var media = new MediaStorageItem
			{
				Entity = download,
				MimeType = download.ContentType,
				FileExtension = download.Extension,
				Path = @"Media\Downloads"
			};

			return media;
		}

		/// <summary>
		/// Create a file name for a media item
		/// </summary>
		/// <param name="media">Media storage item</param>
		/// <returns>File name including extension</returns>
		public static string GetFileName(this MediaStorageItem media)
		{
			if (media != null)
			{
				var extension = media.FileExtension;

				if (extension.IsEmpty())
					extension = MimeTypes.MapMimeTypeToExtension(media.MimeType);

				var baseEntity = media.Entity as BaseEntity;

				var fileName = string.Format("{0}-0{1}",
					baseEntity.Id.ToString("0000000"),
					extension.EmptyNull().EnsureStartsWith(".")
				);

				return fileName;
			}
			return null;
		}
	}
}
