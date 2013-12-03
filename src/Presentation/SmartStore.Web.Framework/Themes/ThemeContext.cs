using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Themes;
using SmartStore.Services.Common;

namespace SmartStore.Web.Framework.Themes
{
    /// <summary>
    /// Theme context
    /// </summary>
    public partial class ThemeContext : IThemeContext
    {
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ThemeSettings _themeSettings;
        private readonly IThemeRegistry _themeRegistry;
        private readonly IMobileDeviceHelper _mobileDeviceHelper;

        private bool _desktopThemeIsCached;
        private string _cachedDesktopThemeName;

        private bool _mobileThemeIsCached;
        private string _cachedMobileThemeName;
        private ThemeManifest _currentTheme;

        public ThemeContext(
            IWorkContext workContext,
			IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            ThemeSettings themeSettings, 
            IThemeRegistry themeRegistry,
            IMobileDeviceHelper mobileDeviceHelper)
        {
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._genericAttributeService = genericAttributeService;
            this._themeSettings = themeSettings;
            this._themeRegistry = themeRegistry;
            this._mobileDeviceHelper = mobileDeviceHelper;
        }

        /// <summary>
        /// Get or set current theme for desktops (e.g. Alpha)
        /// </summary>
        public string WorkingDesktopTheme
        {
            get
            {
                if (_desktopThemeIsCached)
                {
                    return _cachedDesktopThemeName;
                }

                bool isCustomerSpecific = false;
                string theme = "";
                if (_themeSettings.AllowCustomerToSelectTheme)
                {
					if (_workContext.CurrentCustomer != null)
					{
						theme = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.WorkingDesktopThemeName, _genericAttributeService, _storeContext.CurrentStore.Id);										isCustomerSpecific = theme.HasValue();
					}
                }

                // default store theme
                if (string.IsNullOrEmpty(theme))
                {
                    theme = _themeSettings.DefaultDesktopTheme;
                }

                // ensure that theme exists
                if (!_themeRegistry.ThemeManifestExists(theme))
                {
                    theme = _themeRegistry.GetThemeManifests().Where(x => !x.MobileTheme).FirstOrDefault().ThemeName;
                    if (isCustomerSpecific)
                    {
                        // the customer chosen theme does not exists (anymore). Invalidate it!
                        _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.WorkingDesktopThemeName, string.Empty, _storeContext.CurrentStore.Id);
                    }
                }
                
                // cache theme
                this._cachedDesktopThemeName = theme;
                this._desktopThemeIsCached = true;
                return theme;
            }
            set
            {
                if (!_themeSettings.AllowCustomerToSelectTheme)
                    return;

                if (_workContext.CurrentCustomer == null)
                    return;

				_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.WorkingDesktopThemeName, value, _storeContext.CurrentStore.Id);

                //clear cache
                this._desktopThemeIsCached = false;
            }
        }

        /// <summary>
        /// Get current theme for mobile (e.g. Mobile)
        /// </summary>
        public string WorkingMobileTheme
        {
            get
            {
                if (_mobileThemeIsCached)
                    return _cachedMobileThemeName;

                //default store theme
                string theme = _themeSettings.DefaultMobileTheme;

                //ensure that theme exists
                if (!_themeRegistry.ThemeManifestExists(theme))
                    theme = _themeRegistry.GetThemeManifests()
                        .Where(x => x.MobileTheme)
                        .FirstOrDefault()
                        .ThemeName;

                //cache theme
                this._cachedMobileThemeName = theme;
                this._mobileThemeIsCached = true;
                return theme;
            }
        }

        public ThemeManifest CurrentTheme
        {
            get
            {
                if (_currentTheme == null)
                {
                    bool useMobileDevice = _mobileDeviceHelper.IsMobileDevice()
                        && _mobileDeviceHelper.MobileDevicesSupported()
                        && !_mobileDeviceHelper.CustomerDontUseMobileVersion();

                    if (useMobileDevice)
                    {
                        _currentTheme = _themeRegistry.GetThemeManifest(this.WorkingMobileTheme);
                    }
                    else
                    {
                        _currentTheme = _themeRegistry.GetThemeManifest(this.WorkingDesktopTheme);
                    }

                }
                return _currentTheme;
            }
        }

    }
}
