using System.IO;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.IO;
using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Media
{
	public static class MediaFileExtensions
	{
		/// <summary>
		/// Applies Blob to file
		/// </summary>
		/// <param name="blob">The file binary (can be null)</param>
		public static void ApplyBlob(this MediaFile file, byte[] blob)
		{
			Guard.NotNull(file, nameof(file));

			if (blob == null)
			{
				file.MediaStorageId = null;
				file.MediaStorage = null;
			}
			else
			{
				if (file.MediaStorage != null)
				{
					file.MediaStorage.Data = blob;
				}
				else
				{
					file.MediaStorage = new MediaStorage { Data = blob };
				}
			}
		}

		/// <summary>
		/// Refreshes file metadata like size, dimensions rtc.
		/// </summary>
		/// <param name="blob">The file binary (can be null)</param>
		public static void RefreshMetadata(this MediaFile file, byte[] blob)
		{
			Guard.NotNull(file, nameof(file));

			file.Size = blob != null ? blob.Length : 0;
			file.Width = null;
			file.Height = null;
			file.PixelSize = null;

			if (blob != null && file.MediaType == MediaType.Image)
			{
				try
				{
					var size = ImageHeader.GetDimensions(blob, file.MimeType);
					file.Width = size.Width;
					file.Height = size.Height;
					file.PixelSize = size.Width * size.Height;
				}
				catch { }
			}
		}
	}
}
