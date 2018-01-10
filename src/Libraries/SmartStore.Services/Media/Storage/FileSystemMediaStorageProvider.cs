using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	[SystemName("MediaStorage.SmartStoreFileSystem")]
	[FriendlyName("File system")]
	[DisplayOrder(1)]
	public class FileSystemMediaStorageProvider : IMediaStorageProvider, ISupportsMediaMoving
	{
		private readonly IMediaFileSystem _fileSystem;

		public FileSystemMediaStorageProvider(IMediaFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public static string SystemName
		{
			get { return "MediaStorage.SmartStoreFileSystem"; }
		}

		protected string GetPath(MediaItem media)
		{
			var fileName = media.GetFileName();

			var picture = media.Entity as Picture;
			if (picture != null)
			{
				var subfolder = fileName.Substring(0, ImageCache.MaxDirLength);
				fileName = _fileSystem.Combine(subfolder, fileName);
			}

			return _fileSystem.Combine(media.Path, fileName);
		}

		public Stream OpenRead(MediaItem media)
		{
			var file = _fileSystem.GetFile(GetPath(media));

			return file.Exists ? file.OpenRead() : null;
		}

		public byte[] Load(MediaItem media)
		{
			Guard.NotNull(media, nameof(media));

			var filePath = GetPath(media);
			return _fileSystem.ReadAllBytes(filePath) ?? new byte[0];
		}

		public async Task<byte[]> LoadAsync(MediaItem media)
		{
			Guard.NotNull(media, nameof(media));

			var filePath = GetPath(media);
			return (await _fileSystem.ReadAllBytesAsync(filePath)) ?? new byte[0];
		}

		public void Save(MediaItem media, byte[] data)
		{
			Guard.NotNull(media, nameof(media));

			// TODO: (?) if the new file extension differs from the old one then the old file never gets deleted

			var filePath = GetPath(media);

			if (data != null && data.LongLength != 0)
			{
				_fileSystem.WriteAllBytes(filePath, data);
			}
			else if (_fileSystem.FileExists(filePath))
			{
				_fileSystem.DeleteFile(filePath);
			}
		}

		public async Task SaveAsync(MediaItem media, byte[] data)
		{
			Guard.NotNull(media, nameof(media));

			// TODO: (?) if the new file extension differs from the old one then the old file never gets deleted

			var filePath = GetPath(media);

			if (data != null && data.LongLength != 0)
			{
				await _fileSystem.WriteAllBytesAsync(filePath, data);
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
				var filePath = GetPath(media);
				_fileSystem.DeleteFile(filePath);
			}
		}


		public void MoveTo(ISupportsMediaMoving target, MediaMoverContext context, MediaItem media)
		{
			Guard.NotNull(target, nameof(target));
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(media, nameof(media));

			var filePath = GetPath(media);

			try
			{
				// read data from file
				var data = _fileSystem.ReadAllBytes(filePath);

				// let target store data (into database for example)
				target.Receive(context, media, data);

				// remember file path: we must be able to rollback IO operations on transaction failure
				context.AffectedFiles.Add(filePath);
			}
			catch (Exception exception)
			{
				Debug.WriteLine(exception.Message);
			}
		}

		public void Receive(MediaMoverContext context, MediaItem media, byte[] data)
		{
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(media, nameof(media));

			// store data into file
			if (data != null && data.LongLength != 0)
			{
				var filePath = GetPath(media);

				if (!_fileSystem.FileExists(filePath))
				{
					// TBD: (mc) We only save the file if it doesn't exist yet.
					// This should save time and bandwidth in the case where the target
					// is a cloud based file system (like Azure BLOB).
					// In such a scenario it'd be advisable to copy the files manually
					// with other - maybe more performant - tools before performing the provider switch.
					_fileSystem.WriteAllBytes(filePath, data);
					context.AffectedFiles.Add(filePath);
				}
			}
		}

		public async Task ReceiveAsync(MediaMoverContext context, MediaItem media, byte[] data)
		{
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(media, nameof(media));

			// store data into file
			if (data != null && data.LongLength != 0)
			{
				var filePath = GetPath(media);

				await _fileSystem.WriteAllBytesAsync(filePath, data);

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
