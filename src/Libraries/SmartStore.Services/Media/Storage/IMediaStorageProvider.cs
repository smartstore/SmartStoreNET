using System.IO;
using System.Threading.Tasks;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	public interface IMediaStorageProvider : IProvider
	{
		/// <summary>
		/// Opens the media item for reading
		/// </summary>
		/// <param name="media">Media storage item</param>
		Stream OpenRead(MediaItem media);

		/// <summary>
		/// Loads media item data
		/// </summary>
		/// <param name="media">Media storage item</param>
		byte[] Load(MediaItem media);

		/// <summary>
		/// Asynchronously loads media item data
		/// </summary>
		/// <param name="media">Media storage item</param>
		Task<byte[]> LoadAsync(MediaItem media);

		/// <summary>
		/// Saves media item data
		/// </summary>
		/// <param name="media">Media storage item</param>
		/// <param name="data">New binary data</param>
		void Save(MediaItem media, byte[] data);

		/// <summary>
		/// Asynchronously saves media item data
		/// </summary>
		/// <param name="media">Media storage item</param>
		/// <param name="data">New binary data</param>
		Task SaveAsync(MediaItem media, byte[] data);

		/// <summary>
		/// Remove media storage item(s)
		/// </summary>
		/// <param name="medias">Media storage items</param>
		void Remove(params MediaItem[] medias);
	}
}
