using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Utilities;

namespace SmartStore.Services.Media.Storage
{
	public class FileSystemMediaStorageProvider : IMediaStorageProvider
	{
		private readonly IFileSystem _fileSystem;
		private string _mediaPath;

		public FileSystemMediaStorageProvider(
			IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		protected virtual string GetPictureName(int pictureId, string mimeType)
		{
			return string.Format("{0}-0.{1}", pictureId.ToString("0000000"), MimeTypes.MapMimeTypeToExtension(mimeType));
		}

		protected virtual string GetPictureLocalPath(string fileName)
		{
			var path = _mediaPath ?? (_mediaPath = CommonHelper.MapPath("~/Media/", false));
			return _fileSystem.Combine(path, fileName);
		}

		protected virtual string GetPictureLocalPath(Picture picture)
		{
			var fileName = GetPictureName(picture.Id, picture.MimeType);
			return GetPictureLocalPath(fileName);
		}

		public byte[] Load(Picture picture)
		{
			Guard.ArgumentNotNull(() => picture);

			var filePath = GetPictureLocalPath(picture);

			return (_fileSystem.ReadAllBytes(filePath) ?? new byte[0]);
		}

		public void Save(Picture picture, byte[] data)
		{
			Guard.ArgumentNotNull(() => picture);

			var filePath = GetPictureLocalPath(picture);

			_fileSystem.WriteAllBytes(filePath, data);
		}

		public void Remove(params Picture[] pictures)
		{
			if (pictures != null)
			{
				foreach (var picture in pictures)
				{
					var filePath = GetPictureLocalPath(picture);

					if (_fileSystem.FileExists(filePath))
					{
						_fileSystem.DeleteFile(filePath);
					}
				}
			}
		}
	}
}
