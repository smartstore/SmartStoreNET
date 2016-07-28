using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	[SystemName("MediaStorage.SmartStoreDatabase")]
	[FriendlyName("Database")]
	[DisplayOrder(0)]
	public class DatabaseMediaStorageProvider : IMediaStorageProvider, IMovableMediaSupported
	{
		private readonly IDbContext _dbContext;
		private readonly IBinaryDataService _binaryDataService;

		public DatabaseMediaStorageProvider(
			IDbContext dbContext,
			IBinaryDataService binaryDataService)
		{
			_dbContext = dbContext;
			_binaryDataService = binaryDataService;
		}

		public static string SystemName
		{
			get { return "MediaStorage.SmartStoreDatabase"; }
		}

		public byte[] Load(MediaStorageItem media)
		{
			Guard.ArgumentNotNull(() => media);

			if ((media.Entity.BinaryDataId ?? 0) != 0 && media.Entity.BinaryData != null)
			{
				return media.Entity.BinaryData.Data;
			}

			return new byte[0];
		}

		public void Save(MediaStorageItem media, byte[] data)
		{
			Guard.ArgumentNotNull(() => media);

			if (data == null || data.LongLength == 0)
			{
				// remove binary data if any
				if ((media.Entity.BinaryDataId ?? 0) != 0 && media.Entity != null)
				{
					_binaryDataService.DeleteBinaryData(media.Entity.BinaryData);
				}
			}
			else
			{
				if (media.Entity.BinaryData == null)
				{
					// entity has no binary data -> insert
					var newBinary = new BinaryData { Data = data };

					_binaryDataService.InsertBinaryData(newBinary);

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
					if (media.Entity.BinaryData.Data.SequenceEqual(data))
					{
						// ignore equal binary data
					}
					else
					{
						// update existing binary data
						media.Entity.BinaryData.Data = data;

						_binaryDataService.UpdateBinaryData(media.Entity.BinaryData);
					}
				}
			}
		}

		public void Remove(params MediaStorageItem[] medias)
		{
			if (medias != null)
			{
				foreach (var media in medias)
				{
					if ((media.Entity.BinaryDataId ?? 0) != 0)
					{
						// this also nulls media.Entity.BinaryDataId
						_binaryDataService.DeleteBinaryData(media.Entity.BinaryData);
					}
				}
			}
		}


		public void MoveTo(IMovableMediaSupported target, MediaStorageMoverContext context, MediaStorageItem media)
		{
			Guard.ArgumentNotNull(() => target);
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => media);

			if (media.Entity.BinaryData != null)
			{
				// let target store data (into a file for example)
				target.StoreMovingData(context, media, media.Entity.BinaryData.Data);

				// remove picture binary from DB
				try
				{
					_binaryDataService.DeleteBinaryData(media.Entity.BinaryData, false);
				}
				catch { }

				media.Entity.BinaryDataId = null;

				context.ShrinkDatabase = true;
			}
		}

		public void StoreMovingData(MediaStorageMoverContext context, MediaStorageItem media, byte[] data)
		{
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => media);

			// store data for later bulk commit
			if (data != null && data.LongLength > 0)
			{
				// requires autoDetectChanges set to true or remove explicit entity detaching
				media.Entity.BinaryData = new BinaryData { Data = data };
			}
		}

		public void OnMoved(MediaStorageMoverContext context, bool succeeded)
		{
			// nothing to do
		}
	}
}
