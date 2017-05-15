using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Themes
{
    public class ThemeListModel
    {
        public ThemeListModel()
        {
            this.AvailableBundleOptimizationValues = new List<SelectListItem>();
            this.Themes = new List<ThemeManifestModel>();
			this.AvailableStores = new List<SelectListItem>();
        }

        public IList<SelectListItem> AvailableBundleOptimizationValues { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Themes.Option.BundleOptimizationEnabled")]
        public int BundleOptimizationEnabled { get; set; }

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