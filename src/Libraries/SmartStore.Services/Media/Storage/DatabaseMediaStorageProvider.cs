using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Plugins;
using SmartStore.Data;
using SmartStore.Data.Utilities;

namespace SmartStore.Services.Media.Storage
{
	[SystemName("MediaStorage.SmartStoreDatabase")]
	[FriendlyName("Database")]
	[DisplayOrder(0)]
	public class DatabaseMediaStorageProvider : IMediaStorageProvider, ISupportsMediaMoving
	{
		class MediaStorageBlobStream : SqlBlobStream
		{
			public MediaStorageBlobStream(
				IDbConnectionFactory connectionFactory,
				string connectionString,
				int mediaStorageId)
				: base(connectionFactory, connectionString, "MediaStorage", "Data", "Id", mediaStorageId)
			{
			}
		}

		private readonly IDbContext _dbContext;
		private readonly IRepository<MediaStorage> _mediaStorageRepo;
		private readonly IRepository<MediaFile> _mediaFileRepo;
		private readonly Lazy<IEfDataProvider> _dataProvider;

		private readonly bool _isSqlServer;

		public DatabaseMediaStorageProvider(
			IDbContext dbContext,
			IRepository<MediaStorage> mediaStorageRepo,
			IRepository<MediaFile> mediaFileRepo,
			Lazy<IEfDataProvider> dataProvider)
		{
			_dbContext = dbContext;
			_mediaStorageRepo = mediaStorageRepo;
			_mediaFileRepo = mediaFileRepo;
			_dataProvider = dataProvider;
			_isSqlServer = DataSettings.Current.IsSqlServer;
		}

		public static string SystemName
		{
			get { return "MediaStorage.SmartStoreDatabase"; }
		}

		private Stream CreateBlobStream(int mediaStorageId)
		{
			return new MediaStorageBlobStream(_dataProvider.Value.GetConnectionFactory(), DataSettings.Current.DataConnectionString, mediaStorageId);
		}

		public bool IsCloudStorage { get; } = false;

		public string GetPublicUrl(MediaFile mediaFile)
		{
			return null;
		}

		public long GetSize(MediaFile mediaFile)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			var id = mediaFile.MediaStorageId ?? 0;
			if (id == 0)
			{
				return 0;
			}

			if (_isSqlServer)
			{
				using (var stream = CreateBlobStream(id))
				{
					return stream.Length;
				}
			}
			else
			{
				return mediaFile.MediaStorage?.Data?.LongLength ?? 0;
			}
		}

		public Stream OpenRead(MediaFile mediaFile)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			if (_isSqlServer)
			{
				if (mediaFile.MediaStorageId > 0)
				{
					return CreateBlobStream(mediaFile.MediaStorageId.Value);
				}

				return null;
			}
			else
			{
				return mediaFile.MediaStorage?.Data?.ToStream();
			}
		}

		public byte[] Load(MediaFile mediaFile)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			if (mediaFile.MediaStorageId == null)
			{
				return new byte[0];
			}

			using (var stream = CreateBlobStream(mediaFile.MediaStorageId.Value))
			{
				return stream.ToByteArray();
			}
		}

		public async Task<byte[]> LoadAsync(MediaFile mediaFile)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			if (mediaFile.MediaStorageId == null)
			{
				return new byte[0];
			}

			using (var stream = CreateBlobStream(mediaFile.MediaStorageId.Value))
			{
				return await stream.ToByteArrayAsync();
			}
		}

		public void Save(MediaFile mediaFile, Stream stream)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			if (_isSqlServer)
			{
				SaveFast(mediaFile, stream);
			}
			else
			{
				byte[] buffer;
				using (stream ?? new MemoryStream())
				{
					buffer = stream.ToByteArray();
				}
				mediaFile.ApplyBlob(buffer);
			}

			_mediaFileRepo.Update(mediaFile);
		}

		public async Task SaveAsync(MediaFile mediaFile, Stream stream)
		{
			Guard.NotNull(mediaFile, nameof(mediaFile));

			if (_isSqlServer)
			{
				SaveFast(mediaFile, stream);
			}
			else
			{
				byte[] buffer;
				using (stream ?? new MemoryStream())
				{
					buffer = await stream.ToByteArrayAsync();
				}
				mediaFile.ApplyBlob(buffer);
			}

			_mediaFileRepo.Update(mediaFile);
		}

		private int SaveFast(MediaFile mediaFile, Stream stream)
		{
			var sql = "INSERT INTO [MediaStorage] (Data) Values(@p0)";
			var storageId = ((DbContext)_dbContext).InsertInto(sql, stream);
			mediaFile.MediaStorageId = storageId;

			return storageId;
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

		public void ChangeExtension(MediaFile mediaFile, string extension)
		{
			// Do nothing
		}

		public void MoveTo(ISupportsMediaMoving target, MediaMoverContext context, MediaFile mediaFile)
		{
			Guard.NotNull(target, nameof(target));
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(mediaFile, nameof(mediaFile));

			if (mediaFile.MediaStorageId != null)
			{
				// Let target store data (into a file for example)
				target.Receive(context, mediaFile, OpenRead(mediaFile));	

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

		public void Receive(MediaMoverContext context, MediaFile mediaFile, Stream stream)
		{
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(mediaFile, nameof(mediaFile));

			// Store data for later bulk commit
			if (stream != null && stream.Length > 0)
			{
				// Requires AutoDetectChanges set to true or remove explicit entity detaching
				Save(mediaFile, stream);
			}
		}

		public async Task ReceiveAsync(MediaMoverContext context, MediaFile mediaFile, Stream stream)
		{
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(mediaFile, nameof(mediaFile));

			// Store data for later bulk commit
			if (stream != null && stream.Length > 0)
			{
				// Requires AutoDetectChanges set to true or remove explicit entity detaching
				await SaveAsync(mediaFile, stream);
			}
		}

		public void OnCompleted(MediaMoverContext context, bool succeeded)
		{
			// nothing to do
		}
	}
}
