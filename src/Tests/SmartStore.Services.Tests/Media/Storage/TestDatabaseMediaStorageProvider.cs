using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Tests.Media.Storage
{
	public class TestDatabaseMediaStorageProvider : IMediaStorageProvider
	{
		public byte[] Load(MediaStorageItem media)
		{
			return media.Entity.BinaryData.Data;
		}

		public void Save(MediaStorageItem media, byte[] data)
		{
		}

		public void Remove(params MediaStorageItem[] medias)
		{
		}
	}
}
