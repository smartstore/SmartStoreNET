using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Media
{
    public class MediaSettings : ISettings
    {
		public int AvatarPictureSize { get; set; } = 250;
        public int ProductThumbPictureSize { get; set; } = 250;
		public int ProductDetailsPictureSize { get; set; } = 600;
		public int ProductThumbPictureSizeOnProductDetailsPage { get; set; } = 70;
		public int MessageProductThumbPictureSize { get; set; } = 70;
		public int AssociatedProductPictureSize { get; set; } = 600;
		public int BundledProductPictureSize { get; set; } = 70;
		public int CategoryThumbPictureSize { get; set; } = 250;
		public int ManufacturerThumbPictureSize { get; set; } = 250;
		public int CartThumbPictureSize { get; set; } = 250;
		public int CartThumbBundleItemPictureSize { get; set; } = 32;
		public int MiniCartThumbPictureSize { get; set; } = 250;
		public int VariantValueThumbPictureSize { get; set; } = 70;
		public int AttributeOptionThumbPictureSize { get; set; } = 70;

		public bool DefaultPictureZoomEnabled { get; set; } = true;
		public string PictureZoomType { get; set; } = "window";

		public int MaximumImageSize { get; set; } = 2048;

		/// <summary>
		/// Geta or sets a default quality used for image generation
		/// </summary>
		public int DefaultImageQuality { get; set; } = 90;

		/// <summary>
		/// Gets or sets the height to width ratio for thumbnails in grid style lists (0.2 - 2)
		/// </summary>
		/// <remarks>
		/// A value greater than 1 indicates, that your product pictures are generally
		/// in portrait format, less than 1 indicates landscape format.
		/// </remarks>
		public float DefaultThumbnailAspectRatio { get; set; } = 1;

		/// <summary>
		/// Geta or sets a vaue indicating whether single (/media/thumbs/) or multiple (/media/thumbs/0001/ and /media/thumbs/0002/) directories will used for picture thumbs
		/// </summary>
		public bool MultipleThumbDirectories { get; set; } = true;

		/// <summary>
		/// Generates absolute media urls based upon current request uri instead of relative urls.
		/// </summary>
		public bool AutoGenerateAbsoluteUrls { get; set; } = true;

		/// <summary>
		/// Whether orphaned files should automatically be marked as transient so that the daily cleanup task may delete them.
		/// </summary>
		public bool MakeFilesTransientWhenOrphaned { get; set; } = true;

		#region MediaTypes

		/// <summary>
		/// A space separated list of image type file extensions (dotless)
		/// </summary>
		public string ImageTypes { get; set; }

		/// <summary>
		/// A space separated list of video type file extensions (dotless)
		/// </summary>
		public string VideoTypes { get; set; }

		/// <summary>
		/// A space separated list of audio type file extensions (dotless)
		/// </summary>
		public string AudioTypes { get; set; }

		/// <summary>
		/// A space separated list of document type file extensions (dotless)
		/// </summary>
		public string DocumentTypes { get; set; }

		/// <summary>
		/// A space separated list of text type file extensions (dotless)
		/// </summary>
		public string TextTypes { get; set; }

		#endregion
	}
}