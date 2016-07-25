using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	public interface IMediaStorageProvider : IProvider
	{
		/// <summary>
		/// Load media item data
		/// </summary>
		/// <param name="media">Media storage item</param>
		byte[] Load(MediaStorageItem media);

		/// <summary>
		/// Save media item data
		/// </summary>
		/// <param name="media">Media storage item</param>
		void Save(MediaStorageItem media);

		/// <summary>
		/// Remove media storage item(s)
		/// </summary>
		/// <param name="medias">Media storage items</param>
		void Remove(params MediaStorageItem[] medias);
	}
}
