using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Services.Common;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Theming
{
	public abstract class ThemeableVirtualPathProviderViewEngine : BuildManagerViewEngine
	{
		internal Func<string, string> GetExtensionThunk = VirtualPathUtility.GetExtension;

		private static readonly string[] _emptyLocations = new string[0];
		private static bool? _enableLocalizedViews;
        private static bool? _enableVbViews;
        private readonly string _cacheKeyType = typeof(ThemeableRazorViewEngine).Name;
		private readonly string _cacheKeyEntry = ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:{5}";

		protected ThemeableVirtualPathProviderViewEngine()
		{
			this.ViewLocationCache = new TwoLevelViewLocationCache(base.ViewLocationCache);

			// prepare localized mobile & desktop display modes
			DisplayModeProvider.Modes.Clear();
			var mobileDisplayMode = new LocalizedDisplayMode(DisplayModeProvider.MobileDisplayModeId, EnableLocalizedViews)
			{
				ContextCondition = IsMobileDevice
			};
			var desktopDisplayMode = new LocalizedDisplayMode(DisplayModeProvider.DefaultDisplayModeId, EnableLocalizedViews);
			
			DisplayModeProvider.Modes.Add(mobileDisplayMode);
			DisplayModeProvider.Modes.Add(desktopDisplayMode);
		}

		public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
		{
			Guard.NotNull(controllerContext, nameof(controllerContext));
			Guard.NotEmpty(viewName, nameof(viewName));

			var chronometer = EngineContext.Current.Resolve<IChronometer>();
			using (chronometer.Step("Find view '{0}'".FormatInvariant(viewName)))
			{
				string[] viewLocationsSearched;
				string[] masterLocationsSearched;

				var theme = GetCurrentThemeName(controllerContext);

				string controllerName = controllerContext.RouteData.GetRequiredString("controller");
				string viewPath = this.GetPath(controllerContext, ViewLocationFormats, AreaViewLocationFormats, "ViewLocationFormats", viewName, controllerName, theme, "View", useCache, out viewLocationsSearched);
				string masterPath = this.GetPath(controllerContext, MasterLocationFormats, AreaMasterLocationFormats, "MasterLocationFormats", masterName, controllerName, theme, "Master", useCache, out masterLocationsSearched);

				if (!string.IsNullOrEmpty(viewPath) && (!string.IsNullOrEmpty(masterPath) || string.IsNullOrEmpty(masterName)))
				{
					return new ViewEngineResult(CreateView(controllerContext, viewPath, masterPath), this);
				}

				return new ViewEngineResult(viewLocationsSearched.Union(masterLocationsSearched));
			}
		}

		public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
		{
			Guard.NotNull(controllerContext, nameof(controllerContext));
			Guard.NotEmpty(partialViewName, nameof(partialViewName));

			var chronometer = EngineContext.Current.Resolve<IChronometer>();
			using (chronometer.Step("Find partial view '{0}'".FormatInvariant(partialViewName)))
			{
				string[] searched;

				var theme = GetCurrentThemeName(controllerContext);

				string controllerName = controllerContext.RouteData.GetRequiredString("controller");
				string partialPath = this.GetPath(controllerContext, PartialViewLocationFormats, AreaPartialViewLocationFormats, "PartialViewLocationFormats", partialViewName, controllerName, theme, "Partial", useCache, out searched);

				if (string.IsNullOrEmpty(partialPath))
				{
					return new ViewEngineResult(searched);
				}

				return new ViewEngineResult(CreatePartialView(controllerContext, partialPath), this);
			}
		}

		public static bool EnableLocalizedViews
		{
			get
			{
				if (!_enableLocalizedViews.HasValue)
				{
					_enableLocalizedViews = CommonHelper.GetAppSetting<bool>("sm:EnableLocalizedViews", false);
				}

				return _enableLocalizedViews.Value;
			}
			set
			{
				_enableLocalizedViews = value;
			}
		}

        public static bool EnableVbViews
        {
            get
            {
                if (!_enableVbViews.HasValue)
                {
                    _enableVbViews = CommonHelper.GetAppSetting<bool>("sm:EnableVbViews", false);
                }

                return _enableVbViews.Value;
            }
            set
            {
                _enableVbViews = value;
            }
        }

        protected virtual string GetPath(ControllerContext controllerContext, string[] locations, string[] areaLocations, string locationsPropertyName, string name, string controllerName, string theme, string cacheKeyPrefix, bool useCache, out string[] searchedLocations)
		{
			searchedLocations = _emptyLocations;

			if (String.IsNullOrEmpty(name))
			{
				return String.Empty;
			}

			string areaName = controllerContext.RouteData.GetAreaName();
			bool usingAreas = !String.IsNullOrEmpty(areaName);

			if (usingAreas)
			{
				var isAdminArea = areaName.IsCaseInsensitiveEqual("admin");

				// "ExtraAreaViewLocations" gets injected by AdminThemedAttribute
				var extraAreaViewLocations = controllerContext.RouteData.DataTokens["ExtraAreaViewLocations"] as string[];

				if (extraAreaViewLocations != null && extraAreaViewLocations.Length > 0)
				{
					var newLocations = areaLocations.ToList();
					var viewType = ViewType.View;

					if (cacheKeyPrefix == "Partial")
					{
						viewType = ViewType.Partial;
					}
					else
					{
						viewType = ViewType.Layout;
					}

					if (isAdminArea)
					{
						// the admin area cannot fallback to itself. Prepend to list.
						ExpandLocationFormats(extraAreaViewLocations, viewType).Reverse().Each(x => newLocations.Insert(0, x));
					}
					else
					{
						newLocations.AddRange(ExpandLocationFormats(extraAreaViewLocations, viewType));
					}

					areaLocations = newLocations.ToArray();
				}
			}

			List<ViewLocation> viewLocations = GetViewLocations(locations, (usingAreas) ? areaLocations : null);

			if (viewLocations.Count == 0)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Properties cannot be null or empty.", new object[] { locationsPropertyName }));
			}

			bool nameRepresentsPath = IsSpecificPath(name);
			string cacheKey = CreateCacheKey(cacheKeyPrefix, name, (nameRepresentsPath) ? String.Empty : controllerName, areaName, theme);

			if (useCache)
			{
				// Only look at cached display modes that can handle the context.
				IEnumerable<IDisplayMode> possibleDisplayModes = DisplayModeProvider.GetAvailableDisplayModesForContext(controllerContext.HttpContext, controllerContext.DisplayMode);
				foreach (IDisplayMode displayMode in possibleDisplayModes)
				{
					string cachedLocation = ViewLocationCache.GetViewLocation(controllerContext.HttpContext, AppendDisplayModeToCacheKey(cacheKey, displayMode.DisplayModeId));

					if (cachedLocation == null)
					{
						// If any matching display mode location is not in the cache, fall back to the uncached behavior, which will repopulate all of our caches.
						return null;
					}

					// A non-empty cachedLocation indicates that we have a matching file on disk. Return that result.
					if (cachedLocation.Length > 0)
					{
						if (controllerContext.DisplayMode == null)
						{
							controllerContext.DisplayMode = displayMode;
						}

						return cachedLocation;
					}
					// An empty cachedLocation value indicates that we don't have a matching file on disk. Keep going down the list of possible display modes.
				}

				// GetPath is called again without using the cache.
				return null;
			}
			else
			{
				return nameRepresentsPath
					? GetPathFromSpecificName(controllerContext, name, cacheKey, ref searchedLocations)
					: GetPathFromGeneralName(controllerContext, viewLocations, name, controllerName, areaName, theme, cacheKey, ref searchedLocations);
			}
		}

		protected virtual bool FilePathIsSupported(string virtualPath)
		{
			if (this.FileExtensions == null)
			{
				// legacy behavior for custom ViewEngine that might not set the FileExtensions property
				return true;
			}

			// get rid of the '.' because the FileExtensions property expects extensions withouth a dot.
			string extension = GetExtensionThunk(virtualPath).TrimStart('.');
			return FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
		}

		protected virtual string GetPathFromSpecificName(ControllerContext controllerContext, string name, string cacheKey, ref string[] searchedLocations)
		{
			string result = name;

			if (!(FilePathIsSupported(name) && FileExists(controllerContext, name)))
			{
				result = String.Empty;
				searchedLocations = new[] { name };
			}

			ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, result);
			return result;
		}

		protected virtual string GetPathFromGeneralName(ControllerContext controllerContext, List<ViewLocation> locations, string name, string controllerName, string areaName, string theme, string cacheKey, ref string[] searchedLocations)
		{
			string result = String.Empty;
			searchedLocations = new string[locations.Count];
			
			for (int i = 0; i < locations.Count; i++)
			{
				ViewLocation location = locations[i];
				string virtualPath = location.Format(name, controllerName, areaName, theme);
				DisplayInfo virtualPathDisplayInfo = DisplayModeProvider.GetDisplayInfoForVirtualPath(virtualPath, controllerContext.HttpContext, path => FileExists(controllerContext, path), controllerContext.DisplayMode);

				if (virtualPathDisplayInfo != null)
				{
					string resolvedVirtualPath = virtualPathDisplayInfo.FilePath;

					searchedLocations = _emptyLocations;
					result = resolvedVirtualPath;
					ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, AppendDisplayModeToCacheKey(cacheKey, GetDisplayModeId(virtualPathDisplayInfo)), result);

					if (controllerContext.DisplayMode == null)
					{
						controllerContext.DisplayMode = virtualPathDisplayInfo.DisplayMode;
					}

					// Populate the cache for all other display modes. We want to cache both file system hits and misses so that we can distinguish
					// in future requests whether a file's status was evicted from the cache (null value) or if the file doesn't exist (empty string).
					IEnumerable<IDisplayMode> allDisplayModes = DisplayModeProvider.Modes;
					foreach (IDisplayMode displayMode in allDisplayModes)
					{
						if (displayMode.DisplayModeId != virtualPathDisplayInfo.DisplayMode.DisplayModeId)
						{
							DisplayInfo displayInfoToCache = displayMode.GetDisplayInfo(controllerContext.HttpContext, virtualPath, virtualPathExists: path => FileExists(controllerContext, path));

							string displayModeId = String.Empty;
							string cacheValue = String.Empty;
							if (displayInfoToCache != null && displayInfoToCache.FilePath != null)
							{
								cacheValue = displayInfoToCache.FilePath;
								displayModeId = GetDisplayModeId(displayInfoToCache);
							}
							else
							{
								displayModeId = displayMode.DisplayModeId;
							}
							ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, AppendDisplayModeToCacheKey(cacheKey, displayModeId), cacheValue);
						}
					}
					break;
				}

				searchedLocations[i] = virtualPath;
			}

			return result;
		}

		private string GetDisplayModeId(DisplayInfo displayInfo)
		{
			var localizedDisplayInfo = displayInfo as LocalizedDisplayInfo;
			if (localizedDisplayInfo != null)
			{
				return localizedDisplayInfo.DisplayModeId;
			}

			return displayInfo.DisplayMode.DisplayModeId;
		}
	
		protected virtual string CreateCacheKey(string prefix, string name, string controllerName, string areaName, string theme/*, string lang*/)
		{
			return _cacheKeyEntry.FormatInvariant(
				_cacheKeyType,
				prefix,
				name,
				controllerName,
				areaName,
				theme);
		}

		internal static string AppendDisplayModeToCacheKey(string cacheKey, string displayMode)
		{
			// key format is ":ViewCacheEntry:{cacheType}:{prefix}:{name}:{controllerName}:{areaName}:{theme}"
			// so append "{displayMode}:" to the key
			return cacheKey + displayMode + ":";
		}

		protected virtual IEnumerable<string> ExpandLocationFormats(IEnumerable<string> formats, ViewType viewType)
		{
			// Appends razor view file extensions to location formats
			Guard.NotNull(formats, nameof(formats));

			foreach (var format in formats)
			{
				yield return format + ".cshtml";

				if (EnableVbViews)
				{
					yield return format + ".vbhtml";
				}
			}

			if (viewType > ViewType.View)
			{
				// Inserts special location formats for layouts and partials
				var subfolder = viewType == ViewType.Layout ? "Layouts" : "Partials";

				foreach (var format in formats)
				{
					yield return format.Replace("{0}", subfolder + "/{0}.cshtml");

					if (EnableVbViews)
					{
						yield return format.Replace("{0}", subfolder + "/{0}.vbhtml");
					}
				}
			}
		}

		protected virtual List<ViewLocation> GetViewLocations(string[] viewLocationFormats, string[] areaViewLocationFormats)
		{
			List<ViewLocation> allLocations = new List<ViewLocation>();

			if (areaViewLocationFormats != null)
			{
				foreach (string areaViewLocationFormat in areaViewLocationFormats)
				{
					allLocations.Add(new AreaAwareViewLocation(areaViewLocationFormat));
				}
			}

			if (viewLocationFormats != null)
			{
				foreach (string viewLocationFormat in viewLocationFormats)
				{
					allLocations.Add(new ViewLocation(viewLocationFormat));
				}
			}

			return allLocations;
		}

		protected virtual bool IsSpecificPath(string name)
		{
			char c = name[0];
			return (c == '~' || c == '/');
		}

		protected virtual bool IsMobileDevice(HttpContextBase httpContext)
		{
			var mobileDeviceHelper = EngineContext.Current.Resolve<IMobileDeviceHelper>();
			var result = mobileDeviceHelper.IsMobileDevice();
			return result;
		}

		protected virtual string GetCurrentThemeName(ControllerContext controllerContext)
		{
			var theme = EngineContext.Current.Resolve<IThemeContext>().CurrentTheme;
			return theme.ThemeName;
		}

		public enum ViewType
		{
			View,
			Layout,
			Partial
		}
	}

	public class AreaAwareViewLocation : ViewLocation
	{
		public AreaAwareViewLocation(string virtualPathFormatString)
			: base(virtualPathFormatString)
		{
		}

		public override string Format(string viewName, string controllerName, string areaName, string theme)
		{
			return _virtualPathFormatString.FormatInvariant(
				viewName,
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

		public virtual string Format(string viewName, string controllerName, string areaName, string theme)
		{
			return _virtualPathFormatString.FormatInvariant(
				viewName,
				controllerName,
				theme);
		}
	}

}
