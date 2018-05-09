﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Optimization;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Themes;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Theming.Assets;

namespace SmartStore.Web.Framework.Theming
{
    internal static class ThemeHelper
    {
		private static readonly Regex s_inheritableThemeFilePattern;
		private static readonly Regex s_themeVarsPattern;
		private static readonly Regex s_moduleImportsPattern;
		private static readonly Regex s_extensionlessPathPattern;

		public static readonly string ThemesBasePath;

		static ThemeHelper()
		{
			ThemesBasePath = CommonHelper.GetAppSetting<string>("sm:ThemesBasePath", "~/Themes/").EnsureEndsWith("/");

			var pattern = @"^{0}(.*)/(.+)(\.)(png|gif|jpg|jpeg|css|scss|less|js|cshtml|svg|json)$".FormatInvariant(ThemesBasePath);
			s_inheritableThemeFilePattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
			s_themeVarsPattern = new Regex(@"\.(db|app)/themevars(.scss|.less)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
			s_moduleImportsPattern = new Regex(@"\.app/moduleimports.scss$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
			s_extensionlessPathPattern = new Regex(@"~/(.+)/([^/.]*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}

		internal static IEnumerable<string> RemoveVirtualImports(IEnumerable<string> virtualPathDependencies)
		{
			Guard.NotNull(virtualPathDependencies, nameof(virtualPathDependencies));

			// determine the virtual themevars.(scss|less) import reference
			var themeVarsFile = virtualPathDependencies.Where(x => ThemeHelper.PathIsThemeVars(x)).FirstOrDefault();
			var moduleImportsFile = virtualPathDependencies.Where(x => ThemeHelper.PathIsModuleImports(x)).FirstOrDefault();

			if (themeVarsFile == null && moduleImportsFile == null)
			{
				// no themevars or moduleimports import... so no special considerations here
				return virtualPathDependencies;
			}

			// exclude the special imports from the file dependencies list,
			// 'cause this one cannot be monitored by the physical file system
			return virtualPathDependencies
				.Except((new string[] { themeVarsFile, moduleImportsFile })
				.Where(x => x.HasValue()))
				.ToArray();
		}

        internal static bool PathIsThemeVars(string virtualPath)
        {
			string extension = null;
			return PathIsThemeVars(virtualPath, out extension);
        }

		internal static bool PathIsThemeVars(string virtualPath, out string extension)
		{
			extension = null;

			if (string.IsNullOrEmpty(virtualPath))
				return false;

			var match = s_themeVarsPattern.Match(virtualPath);
			if (match.Success)
			{
				extension = match.Groups[2].Value;
				return true;
			}

			return false;
		}

		internal static bool PathIsModuleImports(string virtualPath)
		{
			if (string.IsNullOrEmpty(virtualPath))
				return false;

			return s_moduleImportsPattern.IsMatch(virtualPath);
		}

		internal static bool PathIsInheritableThemeFile(string virtualPath)
		{
			if (string.IsNullOrWhiteSpace(virtualPath))
				return false;

			if (virtualPath[0] != '~')
			{
				virtualPath = VirtualPathUtility.ToAppRelative(virtualPath);
			}

			return s_inheritableThemeFilePattern.IsMatch(virtualPath);
		}

		internal static bool IsAdminArea()
		{
			if (HttpContext.Current != null)
			{
				return HttpContext.Current.Request.IsAdminArea();
			}

			return false;
		}

		internal static bool IsStyleValidationRequest()
		{
			return HttpContext.Current?.Request?.QueryString["validate"] != null;
		}

		internal static IsStyleSheetResult IsStyleSheet(string path)
		{
			var extension = Path.GetExtension(path).ToLowerInvariant();

			if (extension == ".cshtml")
			{
				// Perf
				return null;
			}
			else if (extension == ".css")
			{
				return new IsStyleSheetResult { Path = path, IsCss = true };
			}
			else if (extension == ".scss")
			{
				return new IsStyleSheetResult { Path = path, IsSass = true };
			}
			else if (extension == ".less")
			{
				return new IsStyleSheetResult { Path = path, IsLess = true };
			}
			else if (extension.IsEmpty())
			{
				// StyleBundles are  extension-less, so we have to ask 'BundleTable' 
				// if a style bundle has been registered for the given path.
				if (s_extensionlessPathPattern.IsMatch(path))
				{
					var bundle = BundleTable.Bundles.GetBundleFor(path);
					if (bundle != null && ((bundle is SmartStyleBundle || bundle is StyleBundle)))
					{
						return new IsStyleSheetResult { Path = path, IsBundle = true };
					}
				}
			}

			return null;
		}

		internal static ThemeManifest ResolveCurrentTheme()
		{
			return EngineContext.Current.Resolve<IThemeContext>().CurrentTheme;
		}

		internal static int ResolveCurrentStoreId()
		{
			return EngineContext.Current.Resolve<IStoreContext>().CurrentStore.Id;
		}

		internal static string TokenizePath(string virtualPath, out string themeName, out string relativePath, out string query)
		{
			themeName = null;
			relativePath = null;
			query = null;

			var unrooted = virtualPath.Substring(ThemesBasePath.Length); // strip "~/Themes/"
			themeName = unrooted.Substring(0, unrooted.IndexOf('/'));
			relativePath = unrooted.Substring(themeName.Length + 1);

			var idx = relativePath.IndexOf('?');
			if (idx > 0)
			{
				query = relativePath.Substring(idx + 1);
				relativePath = relativePath.Substring(0, idx);
			}

			// strip out query
			return "{0}{1}/{2}".FormatCurrent(ThemesBasePath, themeName, relativePath);
		}

		internal class IsStyleSheetResult
		{
			public string Path { get; set; }
			public bool IsCss { get; set; }
			public bool IsLess { get; set; }
			public bool IsSass { get; set; }
			public bool IsBundle { get; set; }

			public string Extension
			{
				get
				{
					if (IsSass)
						return ".scss";
					else if (IsLess)
						return ".less";
					else if (IsBundle)
						return "";

					return ".css";
				}
			}

			public bool IsPreprocessor
			{
				get { return IsLess || IsSass; }
			}

			public bool IsThemeVars
			{
				get { return IsPreprocessor && ThemeHelper.PathIsThemeVars(Path); }
			}

			public bool IsModuleImports
			{
				get { return IsSass && ThemeHelper.PathIsModuleImports(Path); }
			}
		}
	}
}
