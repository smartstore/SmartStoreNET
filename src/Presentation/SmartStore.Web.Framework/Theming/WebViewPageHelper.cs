using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Themes;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.Filters;
using SmartStore.Core.Domain;
using SmartStore.Services.Customers;
using SmartStore.Core.Domain.Security;

namespace SmartStore.Web.Framework.Theming
{
	public class WebViewPageHelper
	{
		private bool _initialized;
		private ControllerContext _controllerContext;
		private ExpandoObject _themeVars;
		private ICollection<NotifyEntry> _internalNotifications;

		private int? _currentCategoryId;
		private int? _currentManufacturerId;
		private int? _currentProductId;

		private bool? _isHomePage;
		private bool? _isMobileDevice;
        private bool? _isStoreClosed;
		private bool? _enableHoneypot;

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

		public void Initialize(ViewContext viewContext)
		{
			if (!_initialized)
			{
				_controllerContext = viewContext.GetMasterControllerContext();
				_initialized = true;
			}
		}


		public int CurrentCategoryId
		{
			get
			{
				if (!_currentCategoryId.HasValue)
				{
					int id = 0;
					var routeValues = _controllerContext.RequestContext.RouteData.Values;

					if (routeValues["controller"].ToString().IsCaseInsensitiveEqual("catalog")
						&& routeValues["action"].ToString().IsCaseInsensitiveEqual("category")
						&& routeValues.ContainsKey("categoryId"))
					{
						id = Convert.ToInt32(routeValues["categoryId"].ToString());
					}
					_currentCategoryId = id;
				}

				return _currentCategoryId.Value;
			}
		}

		public int CurrentManufacturerId
		{
			get
			{
				if (!_currentManufacturerId.HasValue)
				{
					var routeValues = _controllerContext.RequestContext.RouteData.Values;
					int id = 0;
					if (routeValues["controller"].ToString().IsCaseInsensitiveEqual("catalog")
						&& routeValues["action"].ToString().IsCaseInsensitiveEqual("manufacturer")
						&& routeValues.ContainsKey("manufacturerId"))
					{
						id = Convert.ToInt32(routeValues["manufacturerId"].ToString());
					}
					_currentManufacturerId = id;
				}

				return _currentManufacturerId.Value;
			}
		}

		public int CurrentProductId
		{
			get
			{
				if (!_currentProductId.HasValue)
				{
					var routeValues = _controllerContext.RequestContext.RouteData.Values;
					int id = 0;
					if (routeValues["controller"].ToString().IsCaseInsensitiveEqual("product")
						&& routeValues["action"].ToString().IsCaseInsensitiveEqual("productdetails")
						&& routeValues.ContainsKey("productId"))
					{
						id = Convert.ToInt32(routeValues["productId"].ToString());
					}
					_currentProductId = id;
				}

				return _currentProductId.Value;
			}
		}

		public bool IsHomePage
		{
			get
			{
				if (!_isHomePage.HasValue)
				{
					var routeData = _controllerContext.RequestContext.RouteData;
					_isHomePage = routeData.GetRequiredString("controller").IsCaseInsensitiveEqual("Home") &&
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
					_isStoreClosed = Services.WorkContext.CurrentCustomer.IsAdmin() && settings.StoreClosedAllowForAdmins ?  false : settings.StoreClosed;
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
