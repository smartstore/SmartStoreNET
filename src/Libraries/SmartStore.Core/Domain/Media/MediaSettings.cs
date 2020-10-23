using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Media
{
    public class MediaSettings : ISettings
    {
        public bool DefaultPictureZoomEnabled { get; set; } = true;
        public string PictureZoomType { get; set; } = "window";

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

        /// <summary>
        /// Gets or sets the maximum size (in KB) of an uploaded media file. The default is 102,400 (100 MB).
        /// </summary>
        public long MaxUploadFileSize { get; set; } = 102400;

        #region Thumb sizes / security

        private HashSet<int> _allowedThumbSizes;

        public const int ThumbnailSizeXxs = 32;
        public const int ThumbnailSizeXs = 72;
        public const int ThumbnailSizeSm = 128;
        public const int ThumbnailSizeMd = 256;
        public const int ThumbnailSizeLg = 512;
        public const int ThumbnailSizeXl = 600;
        public const int ThumbnailSizeXxl = 1024;
        public const int MaxImageSize = 2048;

        public int AvatarPictureSize { get; set; } = ThumbnailSizeMd;
        public int ProductThumbPictureSize { get; set; } = ThumbnailSizeMd;
        public int ProductDetailsPictureSize { get; set; } = ThumbnailSizeXl;
        public int ProductThumbPictureSizeOnProductDetailsPage { get; set; } = ThumbnailSizeXs;
        public int MessageProductThumbPictureSize { get; set; } = ThumbnailSizeXs;
        public int AssociatedProductPictureSize { get; set; } = ThumbnailSizeXl;
        public int BundledProductPictureSize { get; set; } = ThumbnailSizeXs;
        public int CategoryThumbPictureSize { get; set; } = ThumbnailSizeMd;
        public int ManufacturerThumbPictureSize { get; set; } = ThumbnailSizeMd;
        public int CartThumbPictureSize { get; set; } = ThumbnailSizeMd;
        public int CartThumbBundleItemPictureSize { get; set; } = ThumbnailSizeXxs;
        public int MiniCartThumbPictureSize { get; set; } = ThumbnailSizeMd;
        public int VariantValueThumbPictureSize { get; set; } = ThumbnailSizeXs;
        public int AttributeOptionThumbPictureSize { get; set; } = ThumbnailSizeXs;

        public List<int> AllowedExtraThumbnailSizes { get; set; }

        public int MaximumImageSize { get; set; } = MaxImageSize;

        public int[] GetAllowedThumbnailSizes()
        {
            EnsureThumbSizeWhitelist();
            return _allowedThumbSizes.OrderBy(x => x).ToArray();
        }

        public bool IsAllowedThumbnailSize(int size)
        {
            EnsureThumbSizeWhitelist();
            return _allowedThumbSizes.Contains(size);
        }

        public int GetNextValidThumbnailSize(int currentSize)
        {
            var allowedSizes = GetAllowedThumbnailSizes();
            foreach (var size in allowedSizes)
            {
                if (size >= currentSize)
                {
                    return size;
                }
            }

            return MaxImageSize;
        }

        private void EnsureThumbSizeWhitelist()
        {
            if (_allowedThumbSizes != null)
                return;

            _allowedThumbSizes = new HashSet<int>
            {
                48, ThumbnailSizeXxs, ThumbnailSizeXs, ThumbnailSizeSm, ThumbnailSizeMd, ThumbnailSizeLg, ThumbnailSizeXl, ThumbnailSizeXxl,
                AvatarPictureSize,
                ProductThumbPictureSize,
                ProductDetailsPictureSize,
                ProductThumbPictureSizeOnProductDetailsPage,
                MessageProductThumbPictureSize,
                AssociatedProductPictureSize,
                BundledProductPictureSize,
                CategoryThumbPictureSize,
                ManufacturerThumbPictureSize,
                CartThumbPictureSize,
                CartThumbBundleItemPictureSize,
                MiniCartThumbPictureSize,
                VariantValueThumbPictureSize,
                AttributeOptionThumbPictureSize,
                MaxImageSize
            };

            if (AllowedExtraThumbnailSizes?.Count > 0)
            {
                _allowedThumbSizes.AddRange(AllowedExtraThumbnailSizes);
            }
        }

        #endregion

        #region Media types

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

        /// <summary>
        /// A space separated list of other types file extensions (dotless)
        /// </summary>
        public string BinTypes { get; set; }

        #endregion
    }
}