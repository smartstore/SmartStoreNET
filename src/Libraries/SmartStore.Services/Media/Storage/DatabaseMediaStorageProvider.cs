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
		private readonly IRepository<Picture> _pictureRepository;
		private readonly IBinaryDataService _binaryDataService;

		public DatabaseMediaStorageProvider(
			IRepository<Picture> pictureRepository,
			IBinaryDataService binaryDataService)
		{
			_pictureRepository = pictureRepository;
			_binaryDataService = binaryDataService;
		}

		public static string SystemName
		{
			get { return "MediaStorage.SmartStoreDatabase"; }
		}

		public byte[] Load(Picture picture)
		{
			Guard.ArgumentNotNull(() => picture);

			if ((picture.BinaryDataId ?? 0) != 0)
			{
				return picture.BinaryData.Data;
			}

			return new byte[0];
		}

		public void Save(Picture picture, byte[] data)
		{
			Guard.ArgumentNotNull(() => picture);

			if (data == null || data.LongLength == 0)
			{
				// remove picture binary if any
				if ((picture.BinaryDataId ?? 0) != 0)
				{
					_binaryDataService.DeleteBinaryData(picture.BinaryData);
				}
			}
			else
			{
				if (picture.BinaryData == null)
				{
					// insert new binary data
					picture.BinaryData = new BinaryData { Data = data };

					if (picture.IsTransientRecord())
						_pictureRepository.Insert(picture);
					else
						_pictureRepository.Update(picture);
				}
				else
				{
					if (picture.BinaryData.Data.SequenceEqual(data))
					{
						// ignore equal binary data
					}
					else
					{
						// update binary data
						picture.BinaryData.Data = data;

						_binaryDataService.UpdateBinaryData(picture.BinaryData);
					}
				}
			}
		}

		public void Remove(params Picture[] pictures)
		{
			if (pictures != null)
			{
				foreach (var picture in pictures)
				{
					if ((picture.BinaryDataId ?? 0) != 0)
					{
						_binaryDataService.DeleteBinaryData(picture.BinaryData);
					}
				}
			}
		}


		public void MoveTo(IMovableMediaSupported target, MediaStorageMoverContext context, Picture picture)
		{
			Guard.ArgumentNotNull(() => target);
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => picture);

			if (picture.BinaryData != null)
			{
				// let target store data (into a file for example)
				target.StoreMovingData(context, picture, picture.BinaryData.Data);

				// remove picture binary from DB
				try
				{
					_binaryDataService.DeleteBinaryData(picture.BinaryData, false);
				}
				catch { }

				picture.BinaryDataId = null;

				context.ShrinkDatabase = true;
			}
		}

		public void StoreMovingData(MediaStorageMoverContext context, Picture picture, byte[] data)
		{
			Guard.ArgumentNotNull(() => context);
			Guard.ArgumentNotNull(() => picture);

			// store data for later bulk commit
			if (data != null && data.LongLength > 0)
			{
				// requires autoDetectChanges set to true or remove explicit entity detaching
				picture.BinaryData = new BinaryData { Data = data };
			}
		}

		public void OnMoved(MediaStorageMoverContext context, bool succeeded)
		{
			// nothing to do
		}
	}
}
