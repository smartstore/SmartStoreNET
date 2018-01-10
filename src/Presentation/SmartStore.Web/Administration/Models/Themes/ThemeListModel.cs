using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Themes
{
    public class ThemeListModel: TabbableModel
    {
        public ThemeListModel()
        {
            AvailableBundleOptimizationValues = new List<SelectListItem>();
			AvailableAssetCachingValues = new List<SelectListItem>();
			Themes = new List<ThemeManifestModel>();
			AvailableStores = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailableBundleOptimizationValues { get; set; }
		public IList<SelectListItem> AvailableAssetCachingValues { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Themes.Option.BundleOptimizationEnabled")]
        public int BundleOptimizationEnabled { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Themes.Option.AssetCachingEnabled")]
		public int AssetCachingEnabled { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Themes.Option.DefaultDesktopTheme")]
        public string DefaultTheme { get; set; }
        public IList<ThemeManifestModel> Themes { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Themes.Option.AllowCustomerToSelectTheme")]
        public bool AllowCustomerToSelectTheme { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Themes.Option.SaveThemeChoiceInCookie")]
		public bool SaveThemeChoiceInCookie { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store")]
		public int StoreId { get; set; }
		public IList<SelectListItem> AvailableStores { get; set; }
    }
}