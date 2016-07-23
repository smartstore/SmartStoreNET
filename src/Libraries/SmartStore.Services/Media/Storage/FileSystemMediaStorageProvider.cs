using System.Linq;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	[SystemName("MediaStorage.SmartStoreFileSystem")]
	[FriendlyName("File system media storage")]
	public class FileSystemMediaStorageProvider : IMediaStorageProvider, IMovableMediaSupported
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

			if (data != null && data.LongLength != 0)
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

					_fileSystem.DeleteFile(filePath);
				}
			}
		}


		public void MoveTo(IMovableMediaSupported target, MediaStorageMoverContext context, Picture picture)
		{
			Guard.ArgumentNotNull(() => target);
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => picture);

			var filePath = GetPicturePath(picture);

			// read data from file
			var data = _fileSystem.ReadAllBytes(filePath);

			// let target store data (into database for example)
			target.StoreMovingData(context, picture, data);

			// remember file path: we must be able to rollback IO operations on transaction failure
			context.AffectedFiles.Add(filePath);
		}

		public void StoreMovingData(MediaStorageMoverContext context, Picture picture, byte[] data)
		{
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => picture);

			// store data into file
			if (data != null && data.LongLength != 0)
			{
				var filePath = GetPicturePath(picture);

				_fileSystem.WriteAllBytes(filePath, data);

				context.AffectedFiles.Add(filePath);
			}
		}

		public void OnMoved(MediaStorageMoverContext context, bool succeeded)
		{
			if (context.AffectedFiles.Any())
			{
				var toFileSystem = context.TargetSystemName.IsCaseInsensitiveEqual(SystemName);

				if ((!toFileSystem && succeeded) || (toFileSystem && !succeeded))
				{
					// FS > DB sucessful OR DB > FS failed: delete all physical files
					// run a background task for the deletion of files (fire & forget)

					Task.Factory.StartNew(state =>
					{
						var files = state as string[];
						foreach (var file in files)
						{
							_fileSystem.DeleteFile(file);
						}
					}, context.AffectedFiles.ToArray()).ConfigureAwait(false);
				}
			}
		}
	}
}
