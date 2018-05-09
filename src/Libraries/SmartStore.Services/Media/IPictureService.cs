using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
	public partial interface IPictureService
    {
        /// <summary>
        /// Validates input picture dimensions and prevents that the image size exceeds global max size
        /// </summary>
        /// <param name="pictureBinary">Picture binary</param>
        /// <param name="mimeType">MIME type</param>
        /// <returns>Picture binary or throws an exception</returns>
        byte[] ValidatePicture(byte[] pictureBinary);

		/// <summary>
		/// Validates input picture dimensions and prevents that the image size exceeds global max size
		/// </summary>
		/// <param name="pictureBinary">Picture binary</param>
		/// <param name="mimeType">MIME type</param>
		/// <param name="size">The size of the original input OR the resized picture</param>
		/// <returns>Picture binary or throws an exception</returns>
		byte[] ValidatePicture(byte[] pictureBinary, out Size size);

		/// <summary>
		/// Finds an equal picture by comparing the binary buffer
		/// </summary>
		/// <param name="pictureBinary">Binary picture data</param>
		/// <param name="pictures">The sequence of pictures to seek within for duplicates</param>
		/// <param name="equalPictureId">Id of equal picture if any</param>
		/// <returns>The picture binary for <c>path</c> when no picture equals in the sequence, <c>null</c> otherwise.</returns>
		byte[] FindEqualPicture(byte[] pictureBinary, IEnumerable<Picture> pictures, out int equalPictureId);

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
		Picture SetSeoFilename(int pictureId, string seoFilename);

		/// <summary>
		/// Loads the picture binary from the underlying storage provider
		/// </summary>
		/// <param name="picture">Picture</param>
		/// <returns>Picture binary</returns>
		byte[] LoadPictureBinary(Picture picture);

		/// <summary>
		/// Asynchronously loads the picture binary from the underlying storage provider
		/// </summary>
		/// <param name="picture">Picture</param>
		/// <returns>Picture binary</returns>
		Task<byte[]> LoadPictureBinaryAsync(Picture picture);

		/// <summary>
		/// Gets the size of a picture
		/// </summary>
		/// <param name="pictureBinary">The buffer</param>
		/// <returns>Size</returns>
		Size GetPictureSize(byte[] pictureBinary);

		/// <summary>
		/// Gets a picture URL
		/// </summary>
		/// <param name="pictureId">Picture identifier</param>
		/// <param name="targetSize">The target picture size (longest side)</param>
		/// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
		/// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
		/// <param name="defaultPictureType">Default picture type</param>
		/// <returns>Picture URL</returns>
		string GetPictureUrl(
			int pictureId,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity);

		/// <summary>
		/// Gets a picture URL asynchronously
		/// </summary>
		/// <param name="pictureId">Picture identifier</param>
		/// <param name="targetSize">The target picture size (longest side)</param>
		/// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
		/// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
		/// <param name="defaultPictureType">Default picture type</param>
		/// <returns>Picture URL</returns>
		Task<string> GetPictureUrlAsync(
			int pictureId,
			int targetSize = 0,
			bool showDefaultPicture = true,
			string storeLocation = null,
			PictureType defaultPictureType = PictureType.Entity);

		/// <summary>
		/// Gets a picture URL
		/// </summary>
		/// <param name="picture">Picture instance</param>
		/// <param name="targetSize">The target picture size (longest side)</param>
		/// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
		/// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
		/// <param name="defaultPictureType">Default picture type</param>
		/// <returns>Picture URL</returns>
		string GetPictureUrl(
			Picture picture,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity);

		/// <summary>
		/// Gets a picture URL asynchronously
		/// </summary>
		/// <param name="picture">Picture instance</param>
		/// <param name="targetSize">The target picture size (longest side)</param>
		/// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
		/// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
		/// <param name="defaultPictureType">Default picture type</param>
		/// <returns>Picture URL</returns>
		Task<string> GetPictureUrlAsync(
			Picture picture,
			int targetSize = 0,
			bool showDefaultPicture = true,
			string storeLocation = null,
			PictureType defaultPictureType = PictureType.Entity);

		/// <summary>
		/// Gets the default picture URL
		/// </summary>
		/// <param name="targetSize">The target picture size (longest side)</param>
		/// <param name="defaultPictureType">Default picture type</param>
		/// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
		/// <returns>Picture URL</returns>
		string GetDefaultPictureUrl(int targetSize = 0,
			PictureType defaultPictureType = PictureType.Entity,
			string storeLocation = null);

		/// <summary>
		/// Gets a picture
		/// </summary>
		/// <param name="pictureId">Picture identifier</param>
		/// <returns>Picture</returns>
		Picture GetPictureById(int pictureId);

        /// <summary>
        /// Gets a collection of pictures
        /// </summary>
        /// <param name="pageIndex">Current page</param>
        /// <param name="pageSize">Items on each page</param>
        /// <returns>Paged list of pictures</returns>
        IPagedList<Picture> GetPictures(int pageIndex, int pageSize);

        /// <summary>
        /// Gets pictures by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="recordsToReturn">Number of records to return. 0 if you want to get all items</param>
        /// <returns>Pictures</returns>
        IList<Picture> GetPicturesByProductId(int productId, int recordsToReturn = 0);

		/// <summary>
		/// Gets a pictures map by product identifiers
		/// </summary>
		/// <param name="productIds">The ids of products to retrieve pictures for</param>
		/// <param name="maxPicturesPerProduct">Max number of pictures to retrieve per product, <c>null</c> to load all pictures</param>
		/// <param name="withBlobs">Whether the blob in MediaStorage table should be eager loaded</param>
		/// <returns>A lookup map of product ids and pictures</returns>
		Multimap<int, Picture> GetPicturesByProductIds(int[] productIds, int? maxPicturesPerProduct = null, bool withBlobs = false);

		/// <summary>
		/// Gets pictures by picture identifier
		/// </summary>
		/// <param name="pictureIds">Picture identifier</param>
		/// <param name="withBlobs">Whether the blob in MediaStorage table should be eager loaded</param>
		/// <returns>Pictures</returns>
		IList<Picture> GetPicturesByIds(int[] pictureIds, bool withBlobs = false);

		/// <summary>
		/// Deletes a picture
		/// </summary>
		/// <param name="picture">Picture</param>
		void DeletePicture(Picture picture);

		/// <summary>
		/// Inserts a picture
		/// </summary>
		/// <param name="pictureBinary">The picture binary</param>
		/// <param name="mimeType">The picture MIME type</param>
		/// <param name="seoFilename">The SEO filename</param>
		/// <param name="isNew">A value indicating whether the picture is new</param>
		/// <param name="width">Picture width</param>
		/// <param name="height">Picture height</param>
		/// <param name="isTransient">A value indicating whether the picture is initially in transient state</param>
		/// <returns>Picture</returns>
		Picture InsertPicture(
			byte[] pictureBinary,
			string mimeType,
			string seoFilename,
			bool isNew,
			int width = 0,
			int height = 0,
			bool isTransient = true);

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
		Picture InsertPicture(byte[] pictureBinary, string mimeType, string seoFilename, bool isNew, bool isTransient = true, bool validateBinary = true);

		/// <summary>
		/// Updates the picture
		/// </summary>
		/// <param name="picture">The picture</param>
		/// <param name="pictureBinary">The picture binary</param>
		/// <param name="mimeType">The picture MIME type</param>
		/// <param name="seoFilename">The SEO filename</param>
		/// <param name="isNew">A value indicating whether the picture is new</param>
		/// <param name="validateBinary">A value indicating whether to validated provided picture binary</param>
		void UpdatePicture(Picture picture, byte[] pictureBinary, string mimeType, string seoFilename, bool isNew, bool validateBinary = true);
	}

	public static class IPictureServiceExtensions
	{
		public static Picture UpdatePicture(this IPictureService pictureService, 
			int pictureId, 
			byte[] pictureBinary, 
			string mimeType, 
			string seoFilename, 
			bool isNew, 
			bool validateBinary = true)
		{
			var picture = pictureService.GetPictureById(pictureId);

			if (picture != null)
			{
				pictureService.UpdatePicture(picture, pictureBinary, mimeType, seoFilename, isNew, validateBinary);
			}

			return picture;
		}

		public static Size GetPictureSize(this IPictureService pictureService, Picture picture)
		{
			var pictureBinary = pictureService.LoadPictureBinary(picture);
			return pictureService.GetPictureSize(pictureBinary);
		}
	}
}
