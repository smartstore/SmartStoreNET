using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Themes
{
    public class ThemeSettings : ISettings
    {
		public ThemeSettings()
		{
			DefaultDesktopTheme = "Alpha";
			DefaultMobileTheme = "Mobile";
			AllowCustomerToSelectTheme = true;
			MobileDevicesSupported = true;
		}

        /// <summary>
        /// Gets or sets a value indicating whether
        /// asset bundling is enabled
        /// </summary>
        /// <value>
        /// 0: Auto (decide based on web.config debug)
        /// 1: Disabled
        /// 2: Enabled
        /// </value>
        public int BundleOptimizationEnabled { get; set; }

        /// <summary>
        /// Gets or sets a default store theme for desktops
        /// </summary>
        public string DefaultDesktopTheme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to select a theme
        /// </summary>
        public bool AllowCustomerToSelectTheme { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether user's theme choice should be saved in a cookie
		/// </summary>
		/// <remarks>
		/// If <c>false</c>, user's theme choice is associated to the account, 
		/// which may be undesirable when, for example, multiple users share a guest account.
		/// </remarks>
		public bool SaveThemeChoiceInCookie { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether mobile devices supported
        /// </summary>
        public bool MobileDevicesSupported { get; set; }

        /// <summary>
        /// Gets or sets a default store theme used by mobile devices (if enabled)
        /// </summary>
        public string DefaultMobileTheme { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all requests will be handled as mobile devices (used for testing)
        /// </summary>
        public bool EmulateMobileDevice { get; set; }

    }
}
