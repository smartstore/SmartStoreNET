using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Theming;
using SmartStore.Core.Themes;

namespace SmartStore.Admin.Models.Themes
{
    public class ThemeListModel
    {
        public ThemeListModel()
        {
            this.AvailableBundleOptimizationValues = new List<SelectListItem>();
            this.DesktopThemes = new List<ThemeManifestModel>();
            this.MobileThemes = new List<ThemeManifestModel>();
			this.AvailableStores = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailableBundleOptimizationValues { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Themes.Option.BundleOptimizationEnabled")]
        public int BundleOptimizationEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Themes.Option.DefaultDesktopTheme")]
        public string DefaultDesktopTheme { get; set; }
        public IList<ThemeManifestModel> DesktopThemes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Themes.Option.DefaultMobileTheme")]
        public string DefaultMobileTheme { get; set; }
        public IList<ThemeManifestModel> MobileThemes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Themes.Option.AllowCustomerToSelectTheme")]
        public bool AllowCustomerToSelectTheme { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Themes.Option.SaveThemeChoiceInCookie")]
		public bool SaveThemeChoiceInCookie { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Themes.Option.MobileDevicesSupported")]
        public bool MobileDevicesSupported { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store")]
		public int StoreId { get; set; }
		public IList<SelectListItem> AvailableStores { get; set; }
    }
}