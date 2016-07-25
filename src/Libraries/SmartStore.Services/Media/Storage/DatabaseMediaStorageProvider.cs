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

			var existingBinary = media.Entity as IMediaStorageSupported;

			if ((existingBinary.BinaryDataId ?? 0) != 0 && existingBinary.BinaryData != null)
			{
				return existingBinary.BinaryData.Data;
			}

			return new byte[0];
		}

		public void Save(MediaStorageItem media)
		{
			Guard.ArgumentNotNull(() => media);

			var existingBinary = media.Entity as IMediaStorageSupported;

			if (media.NewData == null || media.NewData.LongLength == 0)
			{
				// remove picture binary if any
				if ((existingBinary.BinaryDataId ?? 0) != 0)
				{
					_binaryDataService.DeleteBinaryData(existingBinary.BinaryData);
				}
			}
			else
			{
				if (existingBinary.BinaryData == null)
				{
					var newBinary = new BinaryData { Data = media.NewData };

					_binaryDataService.InsertBinaryData(newBinary);

					if (newBinary.Id == 0)
					{
						// actually we should never get here
						_dbContext.SaveChanges();
					}

					existingBinary.BinaryDataId = newBinary.Id;

					_dbContext.SaveChanges();
				}
				else
				{
					if (existingBinary.BinaryData.Data.SequenceEqual(media.NewData))
					{
						// ignore equal binary data
					}
					else
					{
						// update binary data
						existingBinary.BinaryData.Data = media.NewData;

						_binaryDataService.UpdateBinaryData(existingBinary.BinaryData);
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
					var existingBinary = media.Entity as IMediaStorageSupported;

					if ((existingBinary.BinaryDataId ?? 0) != 0)
					{
						_binaryDataService.DeleteBinaryData(existingBinary.BinaryData);
					}
				}
			}
		}


		public void MoveTo(IMovableMediaSupported target, MediaStorageMoverContext context, MediaStorageItem media)
		{
			Guard.ArgumentNotNull(() => target);
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => media);

			var existingBinary = media.Entity as IMediaStorageSupported;

			if (existingBinary.BinaryData != null)
			{
				// let target store data (into a file for example)
				target.StoreMovingData(context, media);

				// remove picture binary from DB
				try
				{
					_binaryDataService.DeleteBinaryData(existingBinary.BinaryData, false);
				}
				catch { }

				existingBinary.BinaryDataId = null;

				context.ShrinkDatabase = true;
			}
		}

		public void StoreMovingData(MediaStorageMoverContext context, MediaStorageItem media)
		{
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => media);

			// store data for later bulk commit
			if (media.NewData != null && media.NewData.LongLength > 0)
			{
				var existingBinary = media.Entity as IMediaStorageSupported;

				// requires autoDetectChanges set to true or remove explicit entity detaching
				existingBinary.BinaryData = new BinaryData { Data = media.NewData };
			}
		}

		public void OnMoved(MediaStorageMoverContext context, bool succeeded)
		{
			// nothing to do
		}
	}
}
