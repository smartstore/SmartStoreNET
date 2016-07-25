using SmartStore.Core.IO;

namespace SmartStore.Services.Media.Storage
{
	public static class MediaStorageExtensions
	{
		public static string GetFileName(this MediaStorageItem media)
		{
			if (media != null)
			{
				return string.Format("{0}-0.{1}", media.Entity.Id.ToString("0000000"), MimeTypes.MapMimeTypeToExtension(media.MimeType));
			}
			return null;
		}
	}
}
