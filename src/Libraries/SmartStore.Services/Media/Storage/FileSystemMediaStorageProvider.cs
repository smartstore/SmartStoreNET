using System.Linq;
using System.Threading.Tasks;
using SmartStore.Core.IO;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	[SystemName("MediaStorage.SmartStoreFileSystem")]
	[FriendlyName("File system")]
	[DisplayOrder(1)]
	public class FileSystemMediaStorageProvider : IMediaStorageProvider, ISupportsMediaMoving
	{
		private readonly IFileSystem _fileSystem;

		public FileSystemMediaStorageProvider(IFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public static string SystemName
		{
			get { return "MediaStorage.SmartStoreFileSystem"; }
		}

		protected string GetPicturePath(MediaItem media)
		{
			Guard.ArgumentNotEmpty(() => media.Path);

			return _fileSystem.Combine(media.Path, media.GetFileName());
		}

		public byte[] Load(MediaItem media)
		{
			Guard.NotNull(media, nameof(media));

			var filePath = GetPicturePath(media);

			return (_fileSystem.ReadAllBytes(filePath) ?? new byte[0]);
		}

		public void Save(MediaItem media, byte[] data)
		{
			Guard.ArgumentNotNull(() => media);

			// TODO: (?) if the new file extension differs from the old one then the old file never gets deleted

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

		public void Remove(params MediaItem[] medias)
		{
			foreach (var media in medias)
			{
				var filePath = GetPicturePath(media);

				_fileSystem.DeleteFile(filePath);
			}
		}


		public void MoveTo(ISupportsMediaMoving target, MediaMoverContext context, MediaItem media)
		{
			Guard.ArgumentNotNull(() => target);
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => media);

			var filePath = GetPicturePath(media);

			// read data from file
			var data = _fileSystem.ReadAllBytes(filePath);

			// let target store data (into database for example)
			target.Receive(context, media, data);

			// remember file path: we must be able to rollback IO operations on transaction failure
			context.AffectedFiles.Add(filePath);
		}

		public void Receive(MediaMoverContext context, MediaItem media, byte[] data)
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

		public void OnCompleted(MediaMoverContext context, bool succeeded)
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
