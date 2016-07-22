using System.Collections.Generic;
using System.Drawing;
using SmartStore.Core;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Services.Media
{
	/// <summary>
	/// Picture service interface
	/// </summary>
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
		/// Finds an equal picture by comparing the binary buffer
		/// </summary>
		/// <param name="path">The picture to find a duplicate for</param>
		/// <param name="pictures">The sequence of pictures to seek within for duplicates</param>
		/// <param name="equalPictureId">Id of equal picture if any</param>
		/// <returns>The picture binary for <c>path</c> when no picture equals in the sequence, <c>null</c> otherwise.</returns>
		byte[] FindEqualPicture(string path, IEnumerable<Picture> pictures, out int equalPictureId);

		/// <summary>
		/// Finds an equal picture by comparing the binary buffer
		/// </summary>
		/// <param name="pictureBinary">Binary picture data</param>
		/// <param name="pictures">The sequence of pictures to seek within for duplicates</param>
		/// <param name="equalPictureId">Id of equal picture if any</param>
		/// <returns>The picture binary for <c>path</c> when no picture equals in the sequence, <c>null</c> otherwise.</returns>
		byte[] FindEqualPicture(byte[] pictureBinary, IEnumerable<Picture> pictures, out int equalPictureId);
        
        /// <summary>
        /// Gets the loaded picture binary depending on picture storage settings
        /// </summary>
        /// <param name="picture">Picture</param>
        /// <returns>Picture binary</returns>
        byte[] LoadPictureBinary(Picture picture);

        /// <summary>
        /// Get picture SEO friendly name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Result</returns>
        string GetPictureSeName(string name);

        /// <summary>
        /// Gets the size of a picture
        /// </summary>
        /// <param name="picture"></param>
        /// <returns></returns>
        Size GetPictureSize(Picture picture);

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
        /// Get a picture URL
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <param name="defaultPictureType">Default picture type</param>
        /// <returns>Picture URL</returns>
        string GetPictureUrl(int pictureId,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity);

        /// <summary>
        /// Get a picture URL
        /// </summary>
        /// <param name="picture">Picture instance</param>
        /// <param name="targetSize">The target picture size (longest side)</param>
        /// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <param name="defaultPictureType">Default picture type</param>
        /// <returns>Picture URL</returns>
        string GetPictureUrl(Picture picture,
            int targetSize = 0,
            bool showDefaultPicture = true,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity);

        ///// <summary>
        ///// Get a picture local path
        ///// </summary>
        ///// <param name="picture">Picture instance</param>
        ///// <param name="targetSize">The target picture size (longest side)</param>
        ///// <param name="showDefaultPicture">A value indicating whether the default picture should be shown</param>
        ///// <returns></returns>
        //string GetThumbLocalPath(Picture picture, int targetSize = 0, bool showDefaultPicture = true);

        /// <summary>
        /// Gets a picture
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <returns>Picture</returns>
        Picture GetPictureById(int pictureId);

        /// <summary>
        /// Deletes a picture
        /// </summary>
        /// <param name="picture">Picture</param>
        void DeletePicture(Picture picture);

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
		/// Gets pictures by picture identifier
		/// </summary>
		/// <param name="pictureIds">Picture identifier</param>
		/// <returns>Pictures</returns>
		IList<Picture> GetPicturesByIds(int[] pictureIds);

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
        /// <param name="pictureId">The picture identifier</param>
        /// <param name="pictureBinary">The picture binary</param>
        /// <param name="mimeType">The picture MIME type</param>
        /// <param name="seoFilename">The SEO filename</param>
        /// <param name="isNew">A value indicating whether the picture is new</param>
        /// <param name="validateBinary">A value indicating whether to validated provided picture binary</param>
        /// <returns>Picture</returns>
        Picture UpdatePicture(int pictureId, byte[] pictureBinary, string mimeType, string seoFilename, bool isNew, bool validateBinary = true);

        /// <summary>
        /// Updates a SEO filename of a picture
        /// </summary>
        /// <param name="pictureId">The picture identifier</param>
        /// <param name="seoFilename">The SEO filename</param>
        /// <returns>Picture</returns>
        Picture SetSeoFilename(int pictureId, string seoFilename);

        /// <summary>
        /// Gets or sets a value indicating whether the images should be stored in data base.
        /// </summary>
        bool StoreInDb { get; set; }
    }
}
