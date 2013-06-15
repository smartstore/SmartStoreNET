using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Settings
{
    public class MediaSettingsModel : ModelBase
    {
        public MediaSettingsModel()
        {
            this.AvailablePictureZoomTypes = new List<SelectListItem>();
        }

		public int ActiveStoreScopeConfiguration { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.PicturesStoredIntoDatabase")]
        public bool PicturesStoredIntoDatabase { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.AvatarPictureSize")]
        public StoreDependingSetting<int> AvatarPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.ProductThumbPictureSize")]
        public StoreDependingSetting<int> ProductThumbPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.ProductDetailsPictureSize")]
        public StoreDependingSetting<int> ProductDetailsPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.ProductThumbPictureSizeOnProductDetailsPage")]
        public StoreDependingSetting<int> ProductThumbPictureSizeOnProductDetailsPage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.ProductVariantPictureSize")]
        public StoreDependingSetting<int> ProductVariantPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.CategoryThumbPictureSize")]
        public StoreDependingSetting<int> CategoryThumbPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.ManufacturerThumbPictureSize")]
        public StoreDependingSetting<int> ManufacturerThumbPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.CartThumbPictureSize")]
        public StoreDependingSetting<int> CartThumbPictureSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.MiniCartThumbPictureSize")]
        public StoreDependingSetting<int> MiniCartThumbPictureSize { get; set; }
        
        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.MaximumImageSize")]
        public StoreDependingSetting<int> MaximumImageSize { get; set; }

        // codehint: sm-add
        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.DefaultPictureZoomEnabled")]
        public StoreDependingSetting<bool> DefaultPictureZoomEnabled { get; set; }

        // codehint: sm-add (window || inner || lens)
        [SmartResourceDisplayName("Admin.Configuration.Settings.Media.PictureZoomType")]
        public StoreDependingSetting<string> PictureZoomType { get; set; }

        public List<SelectListItem> AvailablePictureZoomTypes { get; set; }

    }
}