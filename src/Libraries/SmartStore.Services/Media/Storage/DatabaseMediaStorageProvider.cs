using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media.Storage
{
	public class DatabaseMediaStorageProvider : IMediaStorageProvider
	{
		public byte[] Load(Picture picture)
		{
			return picture.PictureBinary;
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
