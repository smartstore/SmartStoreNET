using System.IO;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	[SystemName("MediaStorage.SmartStoreDatabase")]
	[FriendlyName("Database")]
	[DisplayOrder(0)]
	public class DatabaseMediaStorageProvider : IMediaStorageProvider, ISupportsMediaMoving
	{
		private readonly IDbContext _dbContext;
		private readonly IRepository<MediaStorage> _mediaStorageRepo;
		private readonly IRepository<MediaFile> _mediaFileRepo;

		public DatabaseMediaStorageProvider(
			IDbContext dbContext,
			IRepository<MediaStorage> mediaStorageRepo,
			IRepository<MediaFile> mediaFileRepo)
		{
			_dbContext = dbContext;
			_mediaStorageRepo = mediaStorageRepo;
			_mediaFileRepo = mediaFileRepo;
		}

		public static string SystemName
		{
			get { return "MediaStorage.SmartStoreDatabase"; }
		}

		public long GetSize(MediaFile mediaFile)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			return mediaFile.MediaStorage?.Data?.LongLength ?? 0;
		}

		public Stream OpenRead(MediaFile mediaFile)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			return mediaFile.MediaStorage?.Data?.ToStream();
		}

		public byte[] Load(MediaFile mediaFile)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			return mediaFile.MediaStorage?.Data ?? new byte[0];
		}

		public Task<byte[]> LoadAsync(MediaFile mediaFile)
		{
			return Task.FromResult(Load(mediaFile));
		}

		public void Save(MediaFile mediaFile, Stream stream)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			using (stream ?? new MemoryStream())
			{
				mediaFile.ApplyBlob(stream.ToByteArray());
			}

			_mediaFileRepo.Update(mediaFile);
		}

		public async Task SaveAsync(MediaFile mediaFile, Stream stream)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			using (stream ?? new MemoryStream())
			{
				mediaFile.ApplyBlob(await stream.ToByteArrayAsync());
			}

			_mediaFileRepo.Update(mediaFile);
		}

		public void Remove(params MediaFile[] mediaFiles)
		{
			using (var scope = new DbContextScope(ctx: _mediaFileRepo.Context, autoCommit: false))
			{
				foreach (var media in mediaFiles)
				{
					media.ApplyBlob(null);
				}

				scope.Commit();
			}
		}

		public void MoveTo(ISupportsMediaMoving target, MediaMoverContext context, MediaFile mediaFile)
		{
			Guard.NotNull(target, nameof(target));
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(mediaFile, nameof(mediaFile));

			if (mediaFile.MediaStorage != null)
			{
				// Let target store data (into a file for example)
				target.Receive(context, mediaFile, mediaFile.MediaStorage.Data);

				// Remove picture binary from DB
				try
				{
					mediaFile.MediaStorageId = null;
					mediaFile.MediaStorage = null;
					_mediaFileRepo.Update(mediaFile);
				}
				catch { }
				
				context.ShrinkDatabase = true;
			}
		}

		public void Receive(MediaMoverContext context, MediaFile mediaFile, byte[] data)
		{
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(mediaFile, nameof(mediaFile));

			// store data for later bulk commit
			if (data != null && data.LongLength > 0)
			{
				// Requires autoDetectChanges set to true or remove explicit entity detaching
				mediaFile.MediaStorage = new MediaStorage { Data = data };
			}
		}

		public Task ReceiveAsync(MediaMoverContext context, MediaFile mediaFile, byte[] data)
		{
			Receive(context, mediaFile, data);
			return Task.FromResult(0);
		}

		public void OnCompleted(MediaMoverContext context, bool succeeded)
		{
			// nothing to do
		}
	}
}
