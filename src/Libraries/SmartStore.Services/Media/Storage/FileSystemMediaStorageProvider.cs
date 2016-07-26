using System.Linq;
using System.Threading.Tasks;
using SmartStore.Core.IO;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	[SystemName("MediaStorage.SmartStoreFileSystem")]
	[FriendlyName("File system")]
	[DisplayOrder(1)]
	public class FileSystemMediaStorageProvider : IMediaStorageProvider, IMovableMediaSupported
	{
		private readonly IFileSystem _fileSystem;

		public FileSystemMediaStorageProvider(
			IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public static string SystemName
		{
			get { return "MediaStorage.SmartStoreFileSystem"; }
		}

		protected string GetPicturePath(MediaStorageItem media)
		{
			Guard.ArgumentNotEmpty(() => media.Path);

			return _fileSystem.Combine(media.Path, media.GetFileName());
		}

		public byte[] Load(MediaStorageItem media)
		{
			Guard.ArgumentNotNull(() => media);

			var filePath = GetPicturePath(media);

			return (_fileSystem.ReadAllBytes(filePath) ?? new byte[0]);
		}

		public void Save(MediaStorageItem media, byte[] data)
		{
			Guard.ArgumentNotNull(() => media);

			var filePath = GetPicturePath(media);

			if (data != null && data.LongLength != 0)
			{
				_fileSystem.WriteAllBytes(filePath, data);
			}
			else if (_fileSystem.FileExists(filePath))
			{
				_fileSystem.DeleteFile(filePath);
			}
		}

		public void Remove(params MediaStorageItem[] medias)
		{
			if (medias != null)
			{
				foreach (var media in medias)
				{
					var filePath = GetPicturePath(media);

					_fileSystem.DeleteFile(filePath);
				}
			}
		}


		public void MoveTo(IMovableMediaSupported target, MediaStorageMoverContext context, MediaStorageItem media)
		{
			Guard.ArgumentNotNull(() => target);
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => media);

			var filePath = GetPicturePath(media);

			// read data from file
			var data = _fileSystem.ReadAllBytes(filePath);

			// let target store data (into database for example)
			target.StoreMovingData(context, media, data);

			// remember file path: we must be able to rollback IO operations on transaction failure
			context.AffectedFiles.Add(filePath);
		}

		public void StoreMovingData(MediaStorageMoverContext context, MediaStorageItem media, byte[] data)
		{
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => media);

			// store data into file
			if (data != null && data.LongLength != 0)
			{
				var filePath = GetPicturePath(media);

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
