using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Common;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Themes
{
	public abstract class ThemeableVirtualPathProviderViewEngine : BuildManagerViewEngine
	{
		#region Fields

		internal Func<string, string> GetExtensionThunk;

		private readonly string[] _emptyLocations = null;
		private readonly string _mobileViewModifier = "Mobile";
		private readonly bool _enableLocalizedViews = false;
		// format is ":ViewCacheEntry:{cacheType}:{prefix}:{name}:{controllerName}:{areaName}:{theme}:{lang}"
		private readonly string _cacheKeyEntry = ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:{5}:{6}";

		#endregion

		#region Ctor

		protected ThemeableVirtualPathProviderViewEngine()
		{
			GetExtensionThunk = new Func<string, string>(VirtualPathUtility.GetExtension);
			this._enableLocalizedViews = CommonHelper.GetAppSetting<bool>("sm:EnableLocalizedViews", false);
		}

		#endregion

		#region Utilities

		protected virtual string GetPath(ControllerContext controllerContext, string[] locations, string[] areaLocations, string locationsPropertyName, string name, string controllerName, string theme, string cacheKeyPrefix, bool useCache, bool mobile, out string[] searchedLocations)
		{
			searchedLocations = _emptyLocations;
			if (string.IsNullOrEmpty(name))
			{
				return string.Empty;
			}

			string areaName = controllerContext.RouteData.GetAreaName();
			bool isArea = !string.IsNullOrEmpty(areaName);

			if (isArea)
			{
				var isAdminArea = areaName.IsCaseInsensitiveEqual("admin");

				// admin area does not support mobile devices
				if (isAdminArea && mobile)
				{
					searchedLocations = new string[0];
					return string.Empty;
				}

				// "ExtraAreaViewLocations" gets injected by AdminThemedAttribute
				var extraAreaViewLocations = controllerContext.RouteData.DataTokens["ExtraAreaViewLocations"] as string[];

				if (extraAreaViewLocations != null && extraAreaViewLocations.Length > 0)
				{
					var newLocations = areaLocations.ToList();
					if (isAdminArea)
					{
						// the admin area cannot fallback to itself. Prepend to list.
						extraAreaViewLocations.Reverse().Each(x => newLocations.Insert(0, x));
					}
					else
					{
						newLocations.AddRange(extraAreaViewLocations);
					}

					areaLocations = newLocations.ToArray();
				}
			}

			List<ViewLocation> viewLocations = GetViewLocations(locations, isArea ? areaLocations : null);
			if (viewLocations.Count == 0)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Properties cannot be null or empty.", new object[] { locationsPropertyName }));
			}

			string lang = _enableLocalizedViews ? this.GetCurrentLanguageSeoCode() : null;
			bool isSpecificPath = IsSpecificPath(name);
			string key = this.CreateCacheKey(cacheKeyPrefix, name, isSpecificPath ? string.Empty : controllerName, areaName, theme, lang);
			if (useCache)
			{
				var cached = this.ViewLocationCache.GetViewLocation(controllerContext.HttpContext, key);
				if (cached != null)
				{
					return cached;
				}
			}
			if (!isSpecificPath)
			{
				return this.GetPathFromGeneralName(controllerContext, viewLocations, name, controllerName, areaName, theme, lang, key, ref searchedLocations);
			}
			return this.GetPathFromSpecificName(controllerContext, name, key, ref searchedLocations);
		}

		protected virtual bool FilePathIsSupported(string virtualPath)
		{
			if (this.FileExtensions == null)
			{
				return true;
			}
			string str = this.GetExtensionThunk(virtualPath).TrimStart(new char[] { '.' });
			return this.FileExtensions.Contains<string>(str, StringComparer.OrdinalIgnoreCase);
		}

		protected virtual string GetPathFromSpecificName(ControllerContext controllerContext, string name, string cacheKey, ref string[] searchedLocations)
		{
			string virtualPath = name;
			if (!this.FilePathIsSupported(name) || !this.FileExists(controllerContext, name))
			{
				virtualPath = string.Empty;
				searchedLocations = new string[] { name };
			}
			this.ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, virtualPath);
			return virtualPath;
		}

		protected virtual string GetPathFromGeneralName(ControllerContext controllerContext, List<ViewLocation> locations, string name, string controllerName, string areaName, string theme, string lang, string cacheKey, ref string[] searchedLocations)
		{
			string result = string.Empty;
			var tempSearchedLocations = new List<string>();

			for (int i = 0; i < locations.Count; i++)
			{
				var location = locations[i];
				string virtualPath = location.Format(name, controllerName, areaName, theme, lang);

				if (lang != null && !this.FileExists(controllerContext, virtualPath))
				{
					// lang specific view does not exist, try again without lang suffix
					tempSearchedLocations.Add(virtualPath);
					virtualPath = location.Format(name, controllerName, areaName, theme, null);
				}

				if (this.FileExists(controllerContext, virtualPath))
				{
					result = virtualPath;
					this.ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, result);
					break;
				}

				tempSearchedLocations.Add(virtualPath);
			}

			searchedLocations = result.HasValue() ? _emptyLocations : tempSearchedLocations.ToArray();
			return result;
		}

		protected virtual string CreateCacheKey(string prefix, string name, string controllerName, string areaName, string theme, string lang)
		{
			//return string.Format(CultureInfo.InvariantCulture, ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:{5}", new object[] { base.GetType().AssemblyQualifiedName, prefix, name, controllerName, areaName, theme });
			return _cacheKeyEntry.FormatInvariant(
				base.GetType().AssemblyQualifiedName,
				prefix,
				name,
				controllerName,
				areaName,
				theme,
				lang.EmptyNull());
		}

		protected virtual List<ViewLocation> GetViewLocations(string[] viewLocationFormats, string[] areaViewLocationFormats)
		{
			var list = new List<ViewLocation>();
			if (areaViewLocationFormats != null)
			{
				list.AddRange(areaViewLocationFormats.Select(str => new AreaAwareViewLocation(str)).Cast<ViewLocation>());
			}
			if (viewLocationFormats != null)
			{
				list.AddRange(viewLocationFormats.Select(str2 => new ViewLocation(str2)));
			}
			return list;
		}

		protected virtual bool IsSpecificPath(string name)
		{
			char ch = name[0];
			if (ch != '~')
			{
				return (ch == '/');
			}
			return true;
		}

		protected virtual bool IsMobileDevice()
		{
			var mobileDeviceHelper = EngineContext.Current.Resolve<IMobileDeviceHelper>();
			var result = mobileDeviceHelper.IsMobileDevice()
						 && mobileDeviceHelper.MobileDevicesSupported()
						 && !mobileDeviceHelper.CustomerDontUseMobileVersion();
			return result;
		}

		protected virtual string GetCurrentLanguageSeoCode()
		{
			string result = null;

			var workContext = EngineContext.Current.Resolve<IWorkContext>();
			if (workContext != null && workContext.WorkingLanguage != null)
			{
				result = workContext.WorkingLanguage.UniqueSeoCode;
			}

			return result ?? "en";
		}

		protected virtual string GetCurrentTheme(ControllerContext controllerContext, bool mobile)
		{
			object themeOverride;
			if (controllerContext.RouteData.DataTokens.TryGetValue("ThemeOverride", out themeOverride))
			{
				var theme = themeOverride as string;
				if (!string.IsNullOrEmpty(theme))
				{
					return theme;
				}
			}

			var themeContext = EngineContext.Current.Resolve<IThemeContext>();
			if (mobile)
				//mobile theme
				return themeContext.WorkingMobileTheme;
			else
				//desktop theme
				return themeContext.WorkingDesktopTheme;
		}

		protected virtual ViewEngineResult FindThemeableView(ControllerContext controllerContext, string viewName, string masterName, bool useCache, bool mobile)
		{
			string[] viewLocationsSearched;
			string[] masterLocationsSearched;
			if (controllerContext == null)
			{
				throw new ArgumentNullException("controllerContext");
			}
			if (string.IsNullOrEmpty(viewName))
			{
				throw new ArgumentException("View name cannot be null or empty.", "viewName");
			}

			var theme = GetCurrentTheme(controllerContext, mobile);

			string controllerName = controllerContext.RouteData.GetRequiredString("controller");
			string viewPath = this.GetPath(controllerContext, this.ViewLocationFormats, this.AreaViewLocationFormats, "ViewLocationFormats", viewName, controllerName, theme, "View", useCache, mobile, out viewLocationsSearched);
			string masterPath = this.GetPath(controllerContext, this.MasterLocationFormats, this.AreaMasterLocationFormats, "MasterLocationFormats", masterName, controllerName, theme, "Master", useCache, mobile, out masterLocationsSearched);
			if (!string.IsNullOrEmpty(viewPath) && (!string.IsNullOrEmpty(masterPath) || string.IsNullOrEmpty(masterName)))
			{
				return new ViewEngineResult(this.CreateView(controllerContext, viewPath, masterPath), this);
			}
			if (masterLocationsSearched == null)
			{
				masterLocationsSearched = new string[0];
			}
			return new ViewEngineResult(viewLocationsSearched.Union<string>(masterLocationsSearched));

		}

		protected virtual ViewEngineResult FindThemeablePartialView(ControllerContext controllerContext, string partialViewName, bool useCache, bool mobile)
		{
			string[] searched;
			if (controllerContext == null)
			{
				throw new ArgumentNullException("controllerContext");
			}
			if (string.IsNullOrEmpty(partialViewName))
			{
				throw new ArgumentException("Partial view name cannot be null or empty.", "partialViewName");
			}

			var theme = GetCurrentTheme(controllerContext, mobile);

			string controllerName = controllerContext.RouteData.GetRequiredString("controller");
			string partialPath = this.GetPath(controllerContext, this.PartialViewLocationFormats, this.AreaPartialViewLocationFormats, "PartialViewLocationFormats", partialViewName, controllerName, theme, "Partial", useCache, mobile, out searched);
			if (string.IsNullOrEmpty(partialPath))
			{
				return new ViewEngineResult(searched);
			}
			return new ViewEngineResult(this.CreatePartialView(controllerContext, partialPath), this);

		}

		#endregion

		#region Methods

		public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
		{
			var mobileDeviceHelper = EngineContext.Current.Resolve<IMobileDeviceHelper>();
			bool useMobileDevice = this.IsMobileDevice();

			string overrideViewName = useMobileDevice ?
				string.Format("{0}.{1}", viewName, _mobileViewModifier)
				: viewName;

			ViewEngineResult result = FindThemeableView(controllerContext, overrideViewName, masterName, useCache, useMobileDevice);

			// If we're looking for a Mobile view and couldn't find it try again without modifying the viewname
			if (useMobileDevice && (result == null || result.View == null))
			{
				result = FindThemeableView(controllerContext, viewName, masterName, useCache, false);
			}

			return result;
		}

		public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
		{
			var mobileDeviceHelper = EngineContext.Current.Resolve<IMobileDeviceHelper>();
			bool useMobileDevice = this.IsMobileDevice();

			string overrideViewName = useMobileDevice ?
				string.Format("{0}.{1}", partialViewName, _mobileViewModifier)
				: partialViewName;

			ViewEngineResult result = FindThemeablePartialView(controllerContext, overrideViewName, useCache, useMobileDevice);

			// If we're looking for a Mobile view and couldn't find it try again without modifying the viewname
			if (useMobileDevice && (result == null || result.View == null))
			{
				result = FindThemeablePartialView(controllerContext, partialViewName, useCache, false);
			}

			return result;
		}

		#endregion
	}

	public class AreaAwareViewLocation : ViewLocation
	{
		public AreaAwareViewLocation(string virtualPathFormatString)
			: base(virtualPathFormatString)
		{
		}

		public override string Format(string viewName, string controllerName, string areaName, string theme, string lang = null)
		{
			return _virtualPathFormatString.FormatInvariant(
				lang.HasValue() ? "{0}.{1}".FormatInvariant(viewName, lang) : viewName,
				controllerName,
				areaName,
				theme);
		}
	}

	public class ViewLocation
	{
		protected readonly string _virtualPathFormatString;

		public ViewLocation(string virtualPathFormatString)
		{
			_virtualPathFormatString = virtualPathFormatString;
		}

		public virtual string Format(string viewName, string controllerName, string areaName, string theme, string lang = null)
		{
			return _virtualPathFormatString.FormatInvariant(
				lang.HasValue() ? "{0}.{1}".FormatInvariant(viewName, lang) : viewName,
				controllerName,
				theme);
		}
	}

}
