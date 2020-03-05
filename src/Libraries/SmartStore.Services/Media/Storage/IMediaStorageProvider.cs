using System.IO;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Media.Storage
{
	public interface IMediaStorageProvider : IProvider
	{
		/// <summary>
		/// Gets the size of the media item in bytes.
		/// </summary>
		/// <param name="mediaFile">Media file item</param>
		long GetSize(MediaFile mediaFile);

		/// <summary>
		/// Opens the media item for reading
		/// </summary>
		/// <param name="mediaFile">Media file item</param>
		Stream OpenRead(MediaFile mediaFile);

		/// <summary>
		/// Loads media item data
		/// </summary>
		/// <param name="mediaFile">Media storage item</param>
		byte[] Load(MediaFile mediaFile);

		/// <summary>
		/// Asynchronously loads media item data
		/// </summary>
		/// <param name="mediaFile">Media file item</param>
		Task<byte[]> LoadAsync(MediaFile mediaFile);

		/// <summary>
		/// Saves media item data
		/// </summary>
		/// <param name="mediaFile">Media file item</param>
		/// <param name="data">New binary data</param>
		void Save(MediaFile mediaFile, byte[] data);

		/// <summary>
		/// Asynchronously saves media item data
		/// </summary>
		/// <param name="mediaFile">Media file item</param>
		/// <param name="data">New binary data</param>
		Task SaveAsync(MediaFile mediaFile, byte[] data);

		/// <summary>
		/// Remove media storage item(s)
		/// </summary>
		/// <param name="mediaFiles">Media file items</param>
		void Remove(params MediaFile[] mediaFiles);
	}
}
