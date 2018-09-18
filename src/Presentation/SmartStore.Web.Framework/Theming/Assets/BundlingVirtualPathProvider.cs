using System;
using System.Collections;
using System.Linq;
using System.Web.Caching;
using System.Web.Hosting;

namespace SmartStore.Web.Framework.Theming.Assets
{
    public sealed class BundlingVirtualPathProvider : ThemingVirtualPathProvider
    {
        public BundlingVirtualPathProvider(VirtualPathProvider previous)
			: base(previous)
        {
        }

        public override bool FileExists(string virtualPath)
        {
			var styleResult = ThemeHelper.IsStyleSheet(virtualPath);
			if (styleResult != null && (styleResult.IsThemeVars || styleResult.IsModuleImports))
			{
				return true;
			}

			return base.FileExists(virtualPath);
        }
         
        public override VirtualFile GetFile(string virtualPath)
        {
			var styleResult = ThemeHelper.IsStyleSheet(virtualPath);
			if (styleResult != null)
			{
				if (styleResult.IsThemeVars)
				{
					var theme = ThemeHelper.ResolveCurrentTheme();
					int storeId = ThemeHelper.ResolveCurrentStoreId();
					return new ThemeVarsVirtualFile(virtualPath, styleResult.Extension, theme.ThemeName, storeId);
				}
				else if (styleResult.IsModuleImports)
				{
					return new ModuleImportsVirtualFile(virtualPath, ThemeHelper.IsAdminArea());
				}
			}

			return base.GetFile(virtualPath);
        }

		public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
		{
			var styleResult = ThemeHelper.IsStyleSheet(virtualPath);
			if (styleResult.IsPreprocessor && !(styleResult.IsThemeVars || styleResult.IsModuleImports) && virtualPathDependencies != null)
			{
				// Exclude the special imports from the file dependencies list
				return base.GetFileHash(virtualPath, ThemeHelper.RemoveVirtualImports(virtualPathDependencies.Cast<string>()));
			}

			return base.GetFileHash(virtualPath, virtualPathDependencies);
		}

		public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
			var styleResult = ThemeHelper.IsStyleSheet(virtualPath);

			if (styleResult == null || styleResult.IsCss)
			{
				return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
			}

			if (styleResult.IsThemeVars || styleResult.IsModuleImports)
			{
				return null;
			}

			// Is Sass Or Less Or StyleBundle

            var arrPathDependencies = virtualPathDependencies.Cast<string>().ToArray();


			// Exclude the special imports from the file dependencies list,
			// 'cause this one cannot be monitored by the physical file system
			var fileDependencies = ThemeHelper.RemoveVirtualImports(arrPathDependencies);

			if (fileDependencies == arrPathDependencies)
			{
				// No themevars or moduleimports import... so no special considerations here
				return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
			}

			if (fileDependencies.Any())
            {
				string cacheKey = null;

				var isThemeableAsset = (!styleResult.IsBundle && ThemeHelper.PathIsInheritableThemeFile(virtualPath))
					|| (styleResult.IsBundle && fileDependencies.Any(x => ThemeHelper.PathIsInheritableThemeFile(x)));

				if (isThemeableAsset)
				{
					var theme = ThemeHelper.ResolveCurrentTheme();
					int storeId = ThemeHelper.ResolveCurrentStoreId();
					// invalidate the cache when variables change
					cacheKey = FrameworkCacheConsumer.BuildThemeVarsCacheKey(theme.ThemeName, storeId);

					if (styleResult.IsSass && (ThemeHelper.IsStyleValidationRequest()))
					{
						// Special case: ensure that cached validation result gets nuked in a while,
						// when ThemeVariableService publishes the entity changed messages.
						return new CacheDependency(new string[0], new string[] { cacheKey }, utcStart);
					}
				}

				var files = ThemingVirtualPathProvider.MapDependencyPaths(fileDependencies);

				return new CacheDependency(
					files, 
					cacheKey == null ? new string[0] : new string[] { cacheKey }, 
					utcStart);
            }

			return null;
        }
    }
}