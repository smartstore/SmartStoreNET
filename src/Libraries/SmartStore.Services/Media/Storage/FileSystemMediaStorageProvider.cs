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
		private readonly string _defaultRootPath;

		public FileSystemMediaStorageProvider(
			IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;

			_defaultRootPath = "Media";
		}

		public static string SystemName
		{
			get { return "MediaStorage.SmartStoreFileSystem"; }
		}

		protected string GetPicturePath(MediaStorageItem media)
		{
			if (media.RootPath.HasValue())
				return _fileSystem.Combine(media.RootPath, media.GetFileName());

			return _fileSystem.Combine(_defaultRootPath, media.GetFileName());
		}

		public byte[] Load(MediaStorageItem media)
		{
			Guard.ArgumentNotNull(() => media);

			var filePath = GetPicturePath(media);

			return (_fileSystem.ReadAllBytes(filePath) ?? new byte[0]);
		}

		public void Save(MediaStorageItem media)
		{
			Guard.ArgumentNotNull(() => media);

			var filePath = GetPicturePath(media);

			if (media.NewData != null && media.NewData.LongLength != 0)
			{
				_fileSystem.WriteAllBytes(filePath, media.NewData);
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
			media.NewData = _fileSystem.ReadAllBytes(filePath);

			// let target store data (into database for example)
			target.StoreMovingData(context, media);

			// remember file path: we must be able to rollback IO operations on transaction failure
			context.AffectedFiles.Add(filePath);
		}

		public void StoreMovingData(MediaStorageMoverContext context, MediaStorageItem media)
		{
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => media);

			// store data into file
			if (media.NewData != null && media.NewData.LongLength != 0)
			{
				var filePath = GetPicturePath(media);

				_fileSystem.WriteAllBytes(filePath, media.NewData);

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
