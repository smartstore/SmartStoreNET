using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SmartStore.Core.Data;
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
		const string MediaRootPath = "Storage";
		
		private readonly IMediaFileSystem _fileSystem;

		public FileSystemMediaStorageProvider(IMediaFileSystem fileSystem)
		{
			_fileSystem = fileSystem;
		}

		public static string SystemName
		{
			get { return "MediaStorage.SmartStoreFileSystem"; }
		}

		protected string GetPath(MediaFile mediaFile)
		{
			var ext = mediaFile.Extension.NullEmpty() ?? MimeTypes.MapMimeTypeToExtension(mediaFile.MimeType);

			var fileName = mediaFile.Id.ToString(ImageCache.IdFormatString).Grow(ext, ".");
			var subfolder = fileName.Substring(0, ImageCache.MaxDirLength);
			var path = _fileSystem.Combine(subfolder, fileName);

			return _fileSystem.Combine(MediaRootPath, path);
		}

		public long GetSize(MediaFile mediaFile)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			if (mediaFile.Size > 0)
			{
				return mediaFile.Size;
			}

			var file = _fileSystem.GetFile(GetPath(mediaFile));
			if (file.Exists)
			{
				// Hopefully a future commit will save this
				mediaFile.Size = (int)file.Size;
			}

			return mediaFile.Size;
		}

		public Stream OpenRead(MediaFile mediaFile)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			var file = _fileSystem.GetFile(GetPath(mediaFile));
			return file.Exists ? file.OpenRead() : null;
		}

		public byte[] Load(MediaFile mediaFile)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));
			
			var filePath = GetPath(mediaFile);
			return _fileSystem.ReadAllBytes(filePath) ?? new byte[0];
		}

		public async Task<byte[]> LoadAsync(MediaFile mediaFile)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			var filePath = GetPath(mediaFile);
			return (await _fileSystem.ReadAllBytesAsync(filePath)) ?? new byte[0];
		}

		public void Save(MediaFile mediaFile, Stream stream)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			// TODO: (?) if the new file extension differs from the old one then the old file never gets deleted

			var filePath = GetPath(mediaFile);

			if (stream != null)
			{
				using (stream)
				{
					_fileSystem.SaveStream(filePath, stream);
				}	
			}
			else if (_fileSystem.FileExists(filePath))
			{
				// Remove media storage if any
				_fileSystem.DeleteFile(filePath);
			}
		}

		public async Task SaveAsync(MediaFile mediaFile, Stream stream)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			// TODO: (?) if the new file extension differs from the old one then the old file never gets deleted

			var filePath = GetPath(mediaFile);

			if (stream != null)
			{
				using (stream)
				{
					await _fileSystem.SaveStreamAsync(filePath, stream);
				}
			}
			else if (_fileSystem.FileExists(filePath))
			{
				// Remove media storage if any
				_fileSystem.DeleteFile(filePath);
			}
		}

		public void Remove(params MediaFile[] mediaFiles)
		{
			foreach (var media in mediaFiles)
			{
				_fileSystem.DeleteFile(GetPath(media));
			}
		}

		public void MoveTo(ISupportsMediaMoving target, MediaMoverContext context, MediaFile mediaFile)
		{
			Guard.NotNull(target, nameof(target));
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(mediaFile, nameof(mediaFile));

			var filePath = GetPath(mediaFile);

			try
			{
				// Read data from file
				var data = _fileSystem.ReadAllBytes(filePath);

				// Let target store data (into database for example)
				target.Receive(context, mediaFile, data);

				// Remember file path: we must be able to rollback IO operations on transaction failure
				context.AffectedFiles.Add(filePath);
			}
			catch (Exception exception)
			{
				Debug.WriteLine(exception.Message);
			}
		}

		public void Receive(MediaMoverContext context, MediaFile mediaFile, byte[] data)
		{
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(mediaFile, nameof(mediaFile));

			// store data into file
			if (data != null && data.LongLength != 0)
			{
				var filePath = GetPath(mediaFile);

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

		public async Task ReceiveAsync(MediaMoverContext context, MediaFile mediaFile, byte[] data)
		{
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(mediaFile, nameof(mediaFile));

			// store data into file
			if (data != null && data.LongLength != 0)
			{
				var filePath = GetPath(mediaFile);

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
