using SmartStore.Core.Domain.Media;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	public interface IMediaStorageProvider : IProvider
	{
		/// <summary>
		/// Load media item data
		/// </summary>
		/// <param name="picture">Picture entity</param>
		/// <returns>Binary data of the media item</returns>
		byte[] Load(Picture picture);

		/// <summary>
		/// Save media item data
		/// </summary>
		/// <param name="picture">Picture entity</param>
		/// <param name="data">Binary data of the media item</param>
		void Save(Picture picture, byte[] data);

		/// <summary>
		/// Remove picture entities
		/// </summary>
		/// <param name="pictures">Picture entities</param>
		void Remove(params Picture[] pictures);

		//string GetPublicUrl(Picture picture);
	}
}
