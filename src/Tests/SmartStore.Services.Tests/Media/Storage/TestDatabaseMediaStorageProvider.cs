using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Tests.Media.Storage
{
	public class TestDatabaseMediaStorageProvider : IMediaStorageProvider
	{
		public byte[] Load(MediaItem media)
		{
			return media.Entity.BinaryData.Data;
		}

		public void Save(MediaItem media, byte[] data)
		{
		}

		public void Remove(params MediaItem[] medias)
		{
		}
	}
}
