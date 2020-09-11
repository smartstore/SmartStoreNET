using System.Linq;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Themes;
using SmartStore.Services.Common;

namespace SmartStore.Web.Framework.Theming
{
    public partial class ThemeContext : IThemeContext
    {
        internal const string OverriddenThemeNameKey = "OverriddenThemeName";

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ThemeSettings _themeSettings;
        private readonly IThemeRegistry _themeRegistry;
        private readonly IMobileDeviceHelper _mobileDeviceHelper;
        private readonly HttpContextBase _httpContext;

        private bool _themeIsCached;
        private string _cachedThemeName;

        private ThemeManifest _currentTheme;

        public ThemeContext(
            IWorkContext workContext,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            ThemeSettings themeSettings,
            IThemeRegistry themeRegistry,
            IMobileDeviceHelper mobileDeviceHelper,
            HttpContextBase httpContext)
        {
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._genericAttributeService = genericAttributeService;
            this._themeSettings = themeSettings;
            this._themeRegistry = themeRegistry;
            this._mobileDeviceHelper = mobileDeviceHelper;
            this._httpContext = httpContext;
        }

        public string WorkingThemeName
        {
            get
            {
                if (_themeIsCached)
                {
                    return _cachedThemeName;
                }

                var customer = _workContext.CurrentCustomer;
                bool isUserSpecific = false;
                string theme = "";
                if (_themeSettings.AllowCustomerToSelectTheme)
                {
                    if (_themeSettings.SaveThemeChoiceInCookie)
                    {
                        theme = _httpContext.GetUserThemeChoiceFromCookie();
                    }
                    else
                    {
                        if (customer != null)
                        {
                            theme = customer.GetAttribute<string>(SystemCustomerAttributeNames.WorkingThemeName, _genericAttributeService, _storeContext.CurrentStore.Id);
                        }
                    }

                    isUserSpecific = theme.HasValue();
                }

                // default store theme
                if (string.IsNullOrEmpty(theme))
                {
                    theme = _themeSettings.DefaultTheme;
                }

                // ensure that theme exists
                if (!_themeRegistry.ThemeManifestExists(theme))
                {
                    var manifest = _themeRegistry.GetThemeManifests().FirstOrDefault();
                    if (manifest == null)
                    {
                        // no active theme in system. Throw!
                        throw Error.Application("At least one theme must be in active state, but the theme registry does not contain a valid theme package.");
                    }
                    theme = manifest.ThemeName;
                    if (isUserSpecific)
                    {
                        // the customer chosen theme does not exists (anymore). Invalidate it!
                        _httpContext.SetUserThemeChoiceInCookie(null);
                        if (customer != null)
                        {
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.WorkingThemeName, string.Empty, _storeContext.CurrentStore.Id);
                        }
                    }
                }

                // cache theme
                this._cachedThemeName = theme;
                this._themeIsCached = true;
                return theme;
            }
            set
            {
                if (!_themeSettings.AllowCustomerToSelectTheme)
                    return;

                if (value.HasValue() && !_themeRegistry.ThemeManifestExists(value))
                    return;

                _httpContext.SetUserThemeChoiceInCookie(value.NullEmpty());

                if (_workContext.CurrentCustomer != null)
                {
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.WorkingThemeName, value.EmptyNull(), _storeContext.CurrentStore.Id);
                }

                // clear cache
                this._themeIsCached = false;
            }
        }

        public void SetRequestTheme(string theme)
        {
            try
            {
                var dataTokens = _httpContext.Request.RequestContext.RouteData.DataTokens;
                if (theme.HasValue())
                {
                    dataTokens[OverriddenThemeNameKey] = theme;
                }
                else if (dataTokens.ContainsKey(OverriddenThemeNameKey))
                {
                    dataTokens.Remove(OverriddenThemeNameKey);
                }

                _currentTheme = null;
            }
            catch { }
        }

        public string GetRequestTheme()
        {
            try
            {
                return (string)_httpContext.Request?.RequestContext?.RouteData?.DataTokens[OverriddenThemeNameKey];
            }
            catch
            {
                return null;
            }
        }

        public void SetPreviewTheme(string theme)
        {
            try
            {
                _httpContext.SetPreviewModeValue(OverriddenThemeNameKey, theme);
                _currentTheme = null;
            }
            catch { }
        }

        public string GetPreviewTheme()
        {
            try
            {
                var cookie = _httpContext.GetPreviewModeCookie(false);
                if (cookie != null)
                {
                    return cookie.Values[OverriddenThemeNameKey];
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public ThemeManifest CurrentTheme
        {
            get
            {
                if (_currentTheme == null)
                {
                    var themeOverride = GetRequestTheme() ?? GetPreviewTheme();
                    if (themeOverride != null)
                    {
                        // the theme to be used can be overwritten on request/session basis (e.g. for live preview, editing etc.)
                        _currentTheme = _themeRegistry.GetThemeManifest(themeOverride);
                    }
                    else
                    {
                        _currentTheme = _themeRegistry.GetThemeManifest(this.WorkingThemeName);
                    }

                }

                return _currentTheme;
            }
        }

    }
}
