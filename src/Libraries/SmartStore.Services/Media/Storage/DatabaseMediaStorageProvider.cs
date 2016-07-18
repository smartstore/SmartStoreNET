using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media.Storage
{
	public class DatabaseMediaStorageProvider : IMediaStorageProvider
	{
		public byte[] Load(Picture picture)
		{
			if ((picture.BinaryDataId ?? 0) != 0)
			{
				return picture.BinaryData.Data;
			}

			return new byte[0];
		}

		public void Save(Picture picture, byte[] data)
		{
			// TODO
		}

		public void Remove(params Picture[] pictures)
		{
			// TODO
		}
	}
}
