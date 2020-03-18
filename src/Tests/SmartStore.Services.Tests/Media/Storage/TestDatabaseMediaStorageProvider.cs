using System.IO;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Media.Storage;

namespace SmartStore.Services.Tests.Media.Storage
{
    public class TestDatabaseMediaStorageProvider : IMediaStorageProvider
	{
		public Stream OpenRead(MediaFile media)
		{
			return new MemoryStream(media.MediaStorage.Data);
		}

		public byte[] Load(MediaFile media)
		{
			return media.MediaStorage.Data;
		}

		public Task<byte[]> LoadAsync(MediaFile media)
		{
			return Task.FromResult(Load(media));
		}

		public void Save(MediaFile media, Stream stream)
		{
		}

		public Task SaveAsync(MediaFile media, Stream stream)
		{
			return Task.FromResult(0);
		}

		public void Remove(params MediaFile[] medias)
		{
		}

		public long GetSize(MediaFile media)
		{
			return media.MediaStorage?.Data?.Length ?? 0;
		}
	}
}
