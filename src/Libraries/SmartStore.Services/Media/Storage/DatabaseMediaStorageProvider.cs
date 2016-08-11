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
		private readonly IRepository<BinaryData> _binaryDataRepository;

		public DatabaseMediaStorageProvider(
			IDbContext dbContext,
			IRepository<BinaryData> binaryDataRepository)
		{
			_dbContext = dbContext;
			_binaryDataRepository = binaryDataRepository;
		}

		public static string SystemName
		{
			get { return "MediaStorage.SmartStoreDatabase"; }
		}

		public byte[] Load(MediaItem media)
		{
			Guard.NotNull(media, nameof(media));

			if ((media.Entity.BinaryDataId ?? 0) != 0 && media.Entity.BinaryData != null)
			{
				return media.Entity.BinaryData.Data;
			}

			return new byte[0];
		}

		public void Save(MediaItem media, byte[] data)
		{
			Guard.NotNull(media, nameof(media));

			if (data == null || data.LongLength == 0)
			{
				// remove binary data if any
				if ((media.Entity.BinaryDataId ?? 0) != 0 && media.Entity != null && media.Entity.BinaryData != null)
				{
					_binaryDataRepository.Delete(media.Entity.BinaryData);
				}
			}
			else
			{
				if (media.Entity.BinaryData == null)
				{
					// entity has no binary data -> insert
					var newBinary = new BinaryData { Data = data };

					_binaryDataRepository.Insert(newBinary);

					if (newBinary.Id == 0)
					{
						// actually we should never get here
						_dbContext.SaveChanges();
					}

					media.Entity.BinaryDataId = newBinary.Id;

					_dbContext.SaveChanges();
				}
				else
				{
					// update existing binary data
					media.Entity.BinaryData.Data = data;

					_binaryDataRepository.Update(media.Entity.BinaryData);
				}
			}
		}

		public void Remove(params MediaItem[] medias)
		{
			foreach (var media in medias)
			{
				if ((media.Entity.BinaryDataId ?? 0) != 0)
				{
					// this also nulls media.Entity.BinaryDataId
					_binaryDataRepository.Delete(media.Entity.BinaryData);
				}
			}
		}


		public void MoveTo(ISupportsMediaMoving target, MediaMoverContext context, MediaItem media)
		{
			Guard.NotNull(target, nameof(target));
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(media, nameof(media));

			if (media.Entity.BinaryData != null)
			{
				// let target store data (into a file for example)
				target.Receive(context, media, media.Entity.BinaryData.Data);

				// remove picture binary from DB
				try
				{
					_binaryDataRepository.Delete(media.Entity.BinaryData);
				}
				catch { }

				media.Entity.BinaryDataId = null;

				context.ShrinkDatabase = true;
			}
		}

		public void Receive(MediaMoverContext context, MediaItem media, byte[] data)
		{
			Guard.NotNull(context, nameof(context));
			Guard.NotNull(media, nameof(media));

			// store data for later bulk commit
			if (data != null && data.LongLength > 0)
			{
				// requires autoDetectChanges set to true or remove explicit entity detaching
				media.Entity.BinaryData = new BinaryData { Data = data };
			}
		}

		public void OnCompleted(MediaMoverContext context, bool succeeded)
		{
			// nothing to do
		}
	}
}
