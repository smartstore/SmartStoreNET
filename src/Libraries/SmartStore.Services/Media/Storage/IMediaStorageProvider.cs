using SmartStore.Core.Domain.Media;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	public interface IMediaStorageProvider : IProvider
	{
		byte[] Load(Picture picture);

		void Save(Picture picture, byte[] data);

		void Remove(params Picture[] pictures);

		//string GetPublicUrl(Picture picture);
	}
}
