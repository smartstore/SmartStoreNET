using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Media
{
    public class MediaSettings : ISettings
    {
		public MediaSettings()
		{
			AvatarPictureSize = 250;
			ProductThumbPictureSize = 250;
			CategoryThumbPictureSize = 250;
			ManufacturerThumbPictureSize = 250;
			ProductDetailsPictureSize = 600;
			ProductThumbPictureSizeOnProductDetailsPage = 70;
			MessageProductThumbPictureSize = 70;
			AssociatedProductPictureSize = 600;
			BundledProductPictureSize = 70;
			CartThumbPictureSize = ProductThumbPictureSize;
			CartThumbBundleItemPictureSize = 32;
			MiniCartThumbPictureSize = ProductThumbPictureSize;
			VariantValueThumbPictureSize = 70;
			MaximumImageSize = 2048;
			DefaultPictureZoomEnabled = true;
			PictureZoomType = "window";
			DefaultImageQuality = 90;
			MultipleThumbDirectories = true;
			DefaultThumbnailAspectRatio = 1;
			AutoGenerateAbsoluteUrls = true;
		}

		public int AvatarPictureSize { get; set; }
        public int ProductThumbPictureSize { get; set; }
        public int ProductDetailsPictureSize { get; set; }
        public int ProductThumbPictureSizeOnProductDetailsPage { get; set; }
		public int MessageProductThumbPictureSize { get; set; }
		public int AssociatedProductPictureSize { get; set; }
		public int BundledProductPictureSize { get; set; }
		public int CategoryThumbPictureSize { get; set; }
        public int ManufacturerThumbPictureSize { get; set; }
        public int CartThumbPictureSize { get; set; }
		public int CartThumbBundleItemPictureSize { get; set; }
        public int MiniCartThumbPictureSize { get; set; }
        public int VariantValueThumbPictureSize { get; set; }

		public bool DefaultPictureZoomEnabled { get; set; }
        public string PictureZoomType { get; set; }

        public int MaximumImageSize { get; set; }

        /// <summary>
        /// Geta or sets a default quality used for image generation
        /// </summary>
        public int DefaultImageQuality { get; set; }

		/// <summary>
		/// Gets or sets the height to width ratio for thumbnails in grid style lists (0.2 - 2)
		/// </summary>
		/// <remarks>
		/// A value greater than 1 indicates, that your product pictures are generally
		/// in portrait format, less than 1 indicates landscape format.
		/// </remarks>
		public int DefaultThumbnailAspectRatio { get; set; }

        /// <summary>
        /// Geta or sets a vaue indicating whether single (/media/thumbs/) or multiple (/media/thumbs/0001/ and /media/thumbs/0002/) directories will used for picture thumbs
        /// </summary>
        public bool MultipleThumbDirectories { get; set; }

		/// <summary>
		/// Generates absolute media urls based upon current request uri instead of relative urls.
		/// </summary>
		public bool AutoGenerateAbsoluteUrls { get; set; }
	}
}