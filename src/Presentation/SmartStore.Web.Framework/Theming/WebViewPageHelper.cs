using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Themes;
using SmartStore.Services;
using SmartStore.Services.Cms;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.Filters;

namespace SmartStore.Web.Framework.Theming
{
    public class FileManagerUrlRequested
    {
        public UrlHelper UrlHelper { get; set; }
        public string Url { get; set; }
    }

    public class WebViewPageHelper
    {
        private bool _initialized;
        private ControllerContext _controllerContext;
        private ExpandoObject _themeVars;
        private ICollection<NotifyEntry> _internalNotifications;

        private string _currentPageType;
        private object _currentPageId;

        private bool? _isHomePage;
        private bool? _isMobileDevice;
        private bool? _isStoreClosed;
        private bool? _enableHoneypot;
        private string _fileManagerUrl;

        public WebViewPageHelper()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }
        public ILocalizationFileResolver LocalizationFileResolver { get; set; }
        public ICommonServices Services { get; set; }
        public IThemeRegistry ThemeRegistry { get; set; }
        public IThemeContext ThemeContext { get; set; }
        public IMobileDeviceHelper MobileDeviceHelper { get; set; }
        public ILinkResolver LinkResolver { get; set; }
        public UrlHelper UrlHelper { get; set; }

        public void Initialize(ControllerContext controllerContext)
        {
            if (!_initialized)
            {
                _controllerContext = controllerContext.GetRootControllerContext();
                IdentifyPage();
                _initialized = true;
            }
        }

        public string CurrentPageType => _currentPageType;

        public object CurrentPageId => _currentPageId;

        private void IdentifyPage()
        {
            var routeData = _controllerContext.RequestContext.RouteData;
            var controllerName = routeData.GetRequiredString("controller").ToLowerInvariant();
            var actionName = routeData.GetRequiredString("action").ToLowerInvariant();

            _currentPageType = "system";
            _currentPageId = controllerName + "." + actionName;

            if (IsHomePage)
            {
                _currentPageType = "home";
                _currentPageId = 0;
            }
            else if (controllerName == "catalog")
            {
                if (actionName == "category")
                {
                    _currentPageType = "category";
                    _currentPageId = routeData.Values.Get("categoryId");
                }
                else if (actionName == "manufacturer")
                {
                    _currentPageType = "brand";
                    _currentPageId = routeData.Values.Get("manufacturerId");
                }
            }
            else if (controllerName == "product")
            {
                if (actionName == "productdetails")
                {
                    _currentPageType = "product";
                    _currentPageId = routeData.Values.Get("productId");
                }
            }
            else if (controllerName == "topic")
            {
                if (actionName == "topicdetails")
                {
                    _currentPageType = "topic";
                    _currentPageId = routeData.Values.Get("topicId");
                }
            }
        }

        public bool IsHomePage
        {
            get
            {
                if (!_isHomePage.HasValue)
                {
                    var routeData = _controllerContext.RequestContext.RouteData;
                    var response = _controllerContext.RequestContext.HttpContext.Response;
                    _isHomePage = response.StatusCode != 404 &&
                        routeData.GetRequiredString("controller").IsCaseInsensitiveEqual("Home") &&
                        routeData.GetRequiredString("action").IsCaseInsensitiveEqual("Index");
                }

                return _isHomePage.Value;
            }
        }

        public bool IsMobileDevice
        {
            get
            {
                if (!_isMobileDevice.HasValue)
                {
                    _isMobileDevice = MobileDeviceHelper.IsMobileDevice();
                }

                return _isMobileDevice.Value;
            }
        }

        public bool IsStoreClosed
        {
            get
            {
                if (!_isStoreClosed.HasValue)
                {
                    var settings = Services.Settings.LoadSetting<StoreInformationSettings>(Services.StoreContext.CurrentStore.Id);
                    _isStoreClosed = Services.WorkContext.CurrentCustomer.IsAdmin() && settings.StoreClosedAllowForAdmins ? false : settings.StoreClosed;
                }

                return _isStoreClosed.Value;
            }
        }

        public bool EnableHoneypotProtection
        {
            get
            {
                if (!_enableHoneypot.HasValue)
                {
                    var settings = Services.Settings.LoadSetting<SecuritySettings>(Services.StoreContext.CurrentStore.Id);
                    _enableHoneypot = settings.EnableHoneypotProtection;
                }

                return _enableHoneypot.Value;
            }
        }

        public string FileManagerUrl
        {
            get
            {
                if (_fileManagerUrl == null)
                {
                    var defaultUrl = UrlHelper.Action("Index", "RoxyFileManager", new { area = "admin" });
                    var message = new FileManagerUrlRequested
                    {
                        UrlHelper = UrlHelper,
                        Url = defaultUrl
                    };

                    Services.EventPublisher.Publish(message);

                    _fileManagerUrl = message.Url ?? defaultUrl;
                }

                return _fileManagerUrl;
            }
        }

        public IEnumerable<LocalizedString> ResolveNotifications(NotifyType? type)
        {
            IEnumerable<NotifyEntry> result = Enumerable.Empty<NotifyEntry>();

            if (_internalNotifications == null)
            {
                string key = NotifyAttribute.NotificationsKey;
                ICollection<NotifyEntry> entries;

                var tempData = _controllerContext.Controller.TempData;
                if (tempData.ContainsKey(key))
                {
                    entries = tempData[key] as ICollection<NotifyEntry>;
                    if (entries != null)
                    {
                        result = result.Concat(entries);
                    }
                }

                var viewData = _controllerContext.Controller.ViewData;
                if (viewData.ContainsKey(key))
                {
                    entries = viewData[key] as ICollection<NotifyEntry>;
                    if (entries != null)
                    {
                        result = result.Concat(entries);
                    }
                }

                _internalNotifications = new HashSet<NotifyEntry>(result);
            }

            if (type == null)
            {
                return _internalNotifications.Select(x => x.Message);
            }

            return _internalNotifications.Where(x => x.Type == type.Value).Select(x => x.Message);
        }

        public dynamic ThemeVariables
        {
            get
            {
                if (_themeVars == null)
                {
                    var storeContext = Services?.StoreContext;
                    var themeManifest = ThemeContext?.CurrentTheme;

                    if (storeContext == null || themeManifest == null)
                    {
                        _themeVars = new ExpandoObject();
                    }
                    else
                    {
                        var repo = new ThemeVarsRepository();
                        _themeVars = repo.GetRawVariables(themeManifest.ThemeName, storeContext.CurrentStore.Id);
                    }
                }

                return _themeVars;
            }
        }
    }
}
