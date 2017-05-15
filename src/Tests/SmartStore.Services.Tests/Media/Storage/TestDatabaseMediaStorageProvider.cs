using System;
using System.IO;
using System.Threading.Tasks;
using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Tests.Media.Storage
{
	public class TestDatabaseMediaStorageProvider : IMediaStorageProvider
	{
		public Stream OpenRead(MediaItem media)
		{
			return new MemoryStream(media.Entity.MediaStorage.Data);
		}

		public byte[] Load(MediaItem media)
		{
			return media.Entity.MediaStorage.Data;
		}

		public Task<byte[]> LoadAsync(MediaItem media)
		{
			return Task.FromResult(Load(media));
		}

		public void Save(MediaItem media, byte[] data)
		{
		}

		public Task SaveAsync(MediaItem media, byte[] data)
		{
			return Task.FromResult(0);
		}

		public void Remove(params MediaItem[] medias)
		{
		}
	}
}
