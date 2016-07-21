using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	[SystemName("MediaStorage.SmartStoreFileSystem")]
	[FriendlyName("File system media storage")]
	public class FileSystemMediaStorageProvider : IMediaStorageProvider
	{
		private readonly IFileSystem _fileSystem;
		private readonly string _mediaRoot;

		public FileSystemMediaStorageProvider(
			IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;

			_mediaRoot = "Media";
		}

		public static string SystemName
		{
			get { return "MediaStorage.SmartStoreFileSystem"; }
		}

		protected virtual string GetPictureName(int pictureId, string mimeType)
		{
			return string.Format("{0}-0.{1}", pictureId.ToString("0000000"), MimeTypes.MapMimeTypeToExtension(mimeType));
		}

		protected virtual string GetPicturePath(Picture picture)
		{
			var fileName = GetPictureName(picture.Id, picture.MimeType);
			return _fileSystem.Combine(_mediaRoot, fileName);
		}

		public byte[] Load(Picture picture)
		{
			Guard.ArgumentNotNull(() => picture);

			var filePath = GetPicturePath(picture);

			return (_fileSystem.ReadAllBytes(filePath) ?? new byte[0]);
		}

		public void Save(Picture picture, byte[] data)
		{
			Guard.ArgumentNotNull(() => picture);

			var filePath = GetPicturePath(picture);

			if (data != null)
			{
				_fileSystem.WriteAllBytes(filePath, data);
			}
			else if (_fileSystem.FileExists(filePath))
			{
				_fileSystem.DeleteFile(filePath);
			}
		}

		public void Remove(params Picture[] pictures)
		{
			if (pictures != null)
			{
				foreach (var picture in pictures)
				{
					var filePath = GetPicturePath(picture);

					if (_fileSystem.FileExists(filePath))
					{
						_fileSystem.DeleteFile(filePath);
					}
				}
			}
		}
	}
}
