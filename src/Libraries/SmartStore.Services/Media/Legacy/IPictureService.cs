using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.IO;

namespace SmartStore.Services.Media
{
	public partial interface IPictureService : IScopedService
    {
        /// <summary>
        /// Validates input picture dimensions and prevents that the image size exceeds global max size
        /// </summary>
        /// <param name="pictureBinary">Picture binary</param>
        /// <param name="mimeType">MIME type</param>
        /// <returns>Picture binary or throws an exception</returns>
        byte[] ValidatePicture(byte[] pictureBinary, string mimeType);

		/// <summary>
		/// Validates input picture dimensions and prevents that the image size exceeds global max size
		/// </summary>
		/// <param name="pictureBinary">Picture binary</param>
		/// <param name="mimeType">MIME type</param>
		/// <param name="size">The size of the original input OR the resized picture</param>
		/// <returns>Picture binary or throws an exception</returns>
		byte[] ValidatePicture(byte[] pictureBinary, string mimeType, out Size size);

		/// <summary>
		/// Finds an equal picture by comparing the binary buffer
		/// </summary>
		/// <param name="pictureBinary">Binary picture data</param>
		/// <param name="pictures">The sequence of pictures to seek within for duplicates</param>
		/// <param name="equalPictureId">Id of equal picture if any</param>
		/// <returns>The picture binary for <c>path</c> when no picture equals in the sequence, <c>null</c> otherwise.</returns>
		byte[] FindEqualPicture(byte[] pictureBinary, IEnumerable<MediaFile> pictures, out int equalPictureId);

		/// <summary>
		/// Get picture SEO friendly name
		/// </summary>
		/// <param name="name">Name</param>
		/// <returns>Picture SEO name</returns>
		string GetPictureSeName(string name);

		/// <summary>
		/// Updates a SEO filename of a picture
		/// </summary>
		/// <param name="pictureId">The picture identifier</param>
		/// <param name="seoFilename">The SEO filename</param>
		/// <returns>Picture</returns>
		MediaFile SetSeoFilename(int pictureId, string seoFilename);

		/// <summary>
		/// Opens the picture stream from the underlying storage provider for reading
		/// </summary>
		/// <param name="picture">Picture</param>
		/// <returns>Picture stream</returns>
		Stream OpenPictureStream(MediaFile picture);

		/// <summary>
		/// Loads the picture binary from the underlying storage provider
		/// </summary>
		/// <param name="picture">Picture</param>
		/// <returns>Picture binary</returns>
		byte[] LoadPictureBinary(MediaFile picture);

		/// <summary>
		/// Asynchronously loads the picture binary from the underlying storage provider
		/// </summary>
		/// <param name="picture">Picture</param>
		/// <returns>Picture binary</returns>
		Task<byte[]> LoadPictureBinaryAsync(MediaFile picture);

		/// <summary>
		/// Gets the size of a picture
		/// </summary>
		/// <param name="pictureBinary">The buffer</param>
		/// <param name="mimeType">Passing MIME type can slightly speed things up</param>
		/// <returns>Size</returns>
		Size GetPictureSize(byte[] pictureBinary, string mimeType = null);

		/// <summary>
		/// TODO: (mc)
		/// </summary>
		/// <param name="pictureIds"></param>
		/// <param name="targetSize"></param>
		/// <param name="showDefaultPicture"></param>
		/// <param name="host"></param>
		/// <param name="fallbackType"></param>
		/// <returns></returns>
		IDictionary<int, PictureInfo> GetPictureInfos(IEnumerable<int> pictureIds);

		/// <summary>
		/// TODO: (mc)
		/// </summary>
		/// <param name="pictureId"></param>
		/// <returns></returns>
		PictureInfo GetPictureInfo(int? pictureId);

		/// <summary>
		/// TODO: (mc)
		/// </summary>
		/// <param name="picture"></param>
		/// <param name="targetSize"></param>
		/// <param name="showDefaultPicture"></param>
		/// <param name="host"></param>
		/// <param name="fallbackType"></param>
		/// <returns></returns>
		PictureInfo GetPictureInfo(MediaFile picture);

		/// <summary>
		/// Builds a url for a given <see cref="pictureId"/>. 
		/// </summary>
		/// <param name="pictureId">Picture identifier</param>
		/// <param name="targetSize">The target picture size (longest side)</param>
		/// <param name="host">Store location URL; null to use determine the current store location automatically</param>
		/// <param name="fallbackType">Specifies the kind of fallback url to return if the <paramref name="pictureId"/> argument is 0 or a picture with the passed id does not exist in the storage.</param>
		/// <returns>Picture URL</returns>
		string GetUrl(int pictureId, int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null);

		/// <summary>
		/// Builds a url for a given <see cref="MediaFile"/> instance. 
		/// </summary>
		/// <param name="picture">Picture instance</param>
		/// <param name="targetSize">The target picture size (longest side)</param>
		/// <param name="host">Store location URL; null to use determine the current store location automatically</param>
		/// <param name="fallbackType">Specifies the kind of fallback url to return if the <paramref name="picture"/> argument is null.</param>
		/// <returns>Picture URL</returns>
		string GetUrl(MediaFile picture, int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null);

		/// <summary>
		/// Builds a url for a given <see cref="PictureInfo"/> instance. 
		/// </summary>
		/// <param name="info">The PictureInfo instance to build a url for</param>
		/// <param name="targetSize">The maximum size of the picture. If greather than null, a query is appended to the generated url.</param>
		/// <param name="host">The host (including scheme) to prepend to the url.</param>
		/// <param name="fallbackType">Specifies the kind of fallback url to return if the <paramref name="info"/> argument is null.</param>
		/// <returns>Generated url which can be processed by the media middleware controller</returns>
		string GetUrl(PictureInfo info, int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null);

		/// <summary>
		/// Gets the fallback picture URL
		/// </summary>
		/// <param name="targetSize">The target picture size (longest side)</param>
		/// <param name="host">Store location URL; null to use determine the current store location automatically</param>
		/// <param name="fallbackType">Default picture type</param>
		/// <returns>Picture URL</returns>
		string GetFallbackUrl(int targetSize = 0, FallbackPictureType fallbackType = FallbackPictureType.Entity, string host = null);

		/// <summary>
		/// Gets a picture
		/// </summary>
		/// <param name="pictureId">Picture identifier</param>
		/// <returns>Picture</returns>
		MediaFile GetPictureById(int pictureId);

        /// <summary>
        /// Gets a collection of pictures
        /// </summary>
        /// <param name="pageIndex">Current page</param>
        /// <param name="pageSize">Items on each page</param>
        /// <returns>Paged list of pictures</returns>
        IPagedList<MediaFile> GetPictures(int pageIndex, int pageSize);

        /// <summary>
        /// Gets pictures by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="recordsToReturn">Number of records to return. 0 if you want to get all items</param>
        /// <returns>Pictures</returns>
        IList<MediaFile> GetPicturesByProductId(int productId, int recordsToReturn = 0);

		/// <summary>
		/// Gets a pictures map by product identifiers
		/// </summary>
		/// <param name="productIds">The ids of products to retrieve pictures for</param>
		/// <param name="maxPicturesPerProduct">Max number of pictures to retrieve per product, <c>null</c> to load all pictures</param>
		/// <param name="withBlobs">Whether the blob in MediaStorage table should be eager loaded</param>
		/// <returns>A lookup map of product ids and pictures</returns>
		Multimap<int, MediaFile> GetPicturesByProductIds(int[] productIds, int? maxPicturesPerProduct = null, bool withBlobs = false);

		/// <summary>
		/// Gets pictures by picture identifier
		/// </summary>
		/// <param name="pictureIds">Picture identifier</param>
		/// <param name="withBlobs">Whether the blob in MediaStorage table should be eager loaded</param>
		/// <returns>Pictures</returns>
		IList<MediaFile> GetPicturesByIds(int[] pictureIds, bool withBlobs = false);

		/// <summary>
		/// Deletes a picture
		/// </summary>
		/// <param name="picture">Picture</param>
		void DeletePicture(MediaFile picture);

		/// <summary>
		/// Inserts a picture
		/// </summary>
		/// <param name="pictureBinary">The picture binary</param>
		/// <param name="mimeType">The picture MIME type</param>
		/// <param name="seoFilename">The SEO filename</param>
		/// <param name="width">Picture width</param>
		/// <param name="height">Picture height</param>
		/// <param name="isTransient">A value indicating whether the picture is initially in transient state</param>
		/// <returns>Picture</returns>
		MediaFile InsertPicture(
			byte[] pictureBinary,
			string mimeType,
			string seoFilename,
			int width = 0,
			int height = 0,
			bool isTransient = true,
			string album = null);

		/// <summary>
		/// Inserts a picture
		/// </summary>
		/// <param name="pictureBinary">The picture binary</param>
		/// <param name="mimeType">The picture MIME type</param>
		/// <param name="seoFilename">The SEO filename</param>
		/// <param name="isNew">A value indicating whether the picture is new</param>
		/// <param name="isTransient">A value indicating whether the picture is initially in transient state</param>
		/// <param name="validateBinary">A value indicating whether to validated provided picture binary</param>
		/// <returns>Picture</returns>
		MediaFile InsertPicture(
			byte[] pictureBinary, 
			string mimeType, 
			string seoFilename, 
			bool isTransient = true, 
			bool validateBinary = true,
			string album = null);

		/// <summary>
		/// Updates the picture
		/// </summary>
		/// <param name="picture">The picture</param>
		/// <param name="pictureBinary">The picture binary</param>
		/// <param name="mimeType">The picture MIME type</param>
		/// <param name="seoFilename">The SEO filename</param>
		/// <param name="validateBinary">A value indicating whether to validated provided picture binary</param>
		void UpdatePicture(
			MediaFile picture, 
			byte[] pictureBinary, 
			string mimeType, 
			string seoFilename, 
			bool validateBinary = true);
	}

	public static class IPictureServiceExtensions
	{
		public static MediaFile UpdatePicture(this IPictureService pictureService, 
			int pictureId, 
			byte[] pictureBinary, 
			string mimeType, 
			string seoFilename,
			bool validateBinary = true)
		{
			var picture = pictureService.GetPictureById(pictureId);

			if (picture != null)
			{
				pictureService.UpdatePicture(picture, pictureBinary, mimeType, seoFilename, validateBinary);
			}

			return picture;
		}

		public static Size GetPictureSize(this IPictureService pictureService, MediaFile picture)
		{
			return ImageHeader.GetDimensions(pictureService.OpenPictureStream(picture), picture.MimeType, false);
		}

		/// <summary>
		/// TODO: (mc)
		/// </summary>
		/// <param name="products"></param>
		/// <returns></returns>
		public static IDictionary<int, PictureInfo> GetPictureInfos(this IPictureService pictureService, IEnumerable<Product> products)
		{
			Guard.NotNull(products, nameof(products));

			return pictureService.GetPictureInfos(products.Select(x => x.MainPictureId.GetValueOrDefault()));
		}

		/// <summary>
		/// Builds a picture url
		/// </summary>
		/// <param name="pictureId">The picture id to build a url for</param>
		/// <param name="targetSize">The maximum size of the picture. If greather than null, a query is appended to the generated url.</param>
		/// <param name="host">The host (including scheme) to prepend to the url.</param>
		/// <param name="fallback">Specifies whether to return a fallback url if the picture does not exist in the storage (default: true).</param>
		/// <returns>Generated url which can be processed by the media middleware controller</returns>
		public static string GetUrl(this IPictureService pictureService, int pictureId, int targetSize, bool fallback, string host = null)
		{
			var fallbackType = fallback ? FallbackPictureType.Entity : FallbackPictureType.NoFallback;
			return pictureService.GetUrl(pictureId, targetSize, fallbackType, host);
		}

		/// <summary>
		/// Builds a picture url
		/// </summary>
		/// <param name="picture">The picture to build a url for</param>
		/// <param name="targetSize">The maximum size of the picture. If greather than null, a query is appended to the generated url.</param>
		/// <param name="host">The host (including scheme) to prepend to the url.</param>
		/// <param name="fallback">Specifies whether to return a fallback url if the picture does not exist in the storage (default: true).</param>
		/// <returns>Generated url which can be processed by the media middleware controller</returns>
		public static string GetUrl(this IPictureService pictureService, MediaFile picture, int targetSize, bool fallback, string host = null)
		{
			var fallbackType = fallback ? FallbackPictureType.Entity : FallbackPictureType.NoFallback;
			return pictureService.GetUrl(picture, targetSize, fallbackType, host);
		}

		/// <summary>
		/// Builds a picture url
		/// </summary>
		/// <param name="PictureInfo">The picture info to build a url for</param>
		/// <param name="targetSize">The maximum size of the picture. If greather than null, a query is appended to the generated url.</param>
		/// <param name="host">The host (including scheme) to prepend to the url.</param>
		/// <param name="fallback">Specifies whether to return a fallback url if the picture does not exist in the storage (default: true).</param>
		/// <returns>Generated url which can be processed by the media middleware controller</returns>
		public static string GetUrl(this IPictureService pictureService, PictureInfo info, int targetSize, bool fallback, string host = null)
		{
			var fallbackType = fallback ? FallbackPictureType.Entity : FallbackPictureType.NoFallback;
			return pictureService.GetUrl(info, targetSize, fallbackType, host);
		}
	}
}
