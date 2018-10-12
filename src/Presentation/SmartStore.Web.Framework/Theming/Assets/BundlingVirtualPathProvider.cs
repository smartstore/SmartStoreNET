using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web.Caching;
using System.Web.Hosting;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Theming.Assets
{
	public sealed class BundlingVirtualPathProvider : ThemingVirtualPathProvider
	{
		private readonly SassCheckedPathStack _sassCheckedPathStack;

		private string _yoooooo;

		public BundlingVirtualPathProvider(VirtualPathProvider previous)
			: base(previous)
        {
			_sassCheckedPathStack = new SassCheckedPathStack();
		}

        public override bool FileExists(string virtualPath)
        {
			var exists = false;
			var styleResult = ThemeHelper.IsStyleSheet(virtualPath);

			TokenizedSassPath sassPath = null;

			if (styleResult != null)
			{
				exists = _sassCheckedPathStack.Check(styleResult, out sassPath);
				if (exists)
				{
					// It seems awkward to return false when check result actually says true.
					// But true in this context means: a check for THIS sass file pattern (e.g. _slick.scss.sass)
					// yielded true previously (because _slick.scss really exists on disks), therefore _slick.scss.sass
					// CANNOT exist (or SHOULD not by convention). By returning false we prevent that a real filesystem check
					// is made against _slick.scss.sass (which is huge performace saver considering that IThemeFileResolver
					// also does some additional checks).
					return false;
				}

				if (styleResult.IsThemeVars || styleResult.IsModuleImports)
				{
					_sassCheckedPathStack.PushExistingPath(sassPath);
					return true;
				}
			}

			//if (virtualPath.Contains("slick.scss"))
			//{
			//	var xxx = true;
			////	throw new Exception("dfsfs");
			//}

			//System.Diagnostics.Debug.WriteLine("VPATH: " + virtualPath);
			_yoooooo += virtualPath + Environment.NewLine;

			exists = base.FileExists(virtualPath);

			if (exists && sassPath != null)
			{
				_sassCheckedPathStack.PushExistingPath(sassPath);
			}

			return exists;
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

	internal class SassCheckedPathStack
	{
		private readonly string[] _styleExtensions = new[] { ".scss", ".sass", ".css" };

		private readonly ContextState<Stack<TokenizedSassPath>> _state 
			= new ContextState<Stack<TokenizedSassPath>>("SassCheckedPathStack.State", () => new Stack<TokenizedSassPath>());

		public SassCheckedPathStack()
		{
		}

		/// <summary>
		/// Checks last path existence
		/// </summary>
		/// <returns>true = does exist, no need to check | false = not checked yet</returns>
		public bool Check(StyleSheetResult styleResult, out TokenizedSassPath path)
		{
			path = new TokenizedSassPath(styleResult);

			var state = _state.GetState();
			if (state.Count == 0)
				return false;

			var currentPath = path;
			var lastPath = _state.GetState().Peek();

			if (currentPath.Extension.IsEmpty())
			{
				// We dont't allow extension-less Sass files, so no need to check.
				return true;
			}

			if (lastPath.Dir != currentPath.Dir)
			{
				return false;
			}

			if (currentPath.StyleResult.IsExplicit && lastPath.StyleResult.IsExplicit && currentPath.FileName == lastPath.FileName)
			{
				return true;
			}

			if (currentPath.StyleResult.IsModuleImports && lastPath.StyleResult.IsModuleImports)
			{
				return true;
			}

			if (currentPath.StyleResult.IsThemeVars && lastPath.StyleResult.IsThemeVars)
			{
				return true;
			}

			// slick.scss.(scss|sass|css) > slick.scss
			if (Path.GetExtension(currentPath.FileNameWithoutExtension) == ".scss")
			{
				return true;
			}

			// slick.(sass|css) > slick.scss
			if (!currentPath.StyleResult.IsExplicit && _styleExtensions.Contains(currentPath.Extension) && currentPath.FileNameWithoutExtension == lastPath.FileNameWithoutExtension)
			{
				return true;
			}

			// _slick.scss > slick.scss
			if (currentPath.FileName.StartsWith("_"))
			{
				if (currentPath.FileName.Substring(1) == lastPath.FileName)
				{
					return true;
				}
			}

			// slick.(scss|sass|css) > _slick.scss
			if (lastPath.FileNameWithoutExtension.StartsWith("_"))
			{
				if (lastPath.FileNameWithoutExtension == "_" + currentPath.FileNameWithoutExtension)
				{
					return true;
				}
			}

			return false;
		}

		public void PushExistingPath(TokenizedSassPath path)
		{
			_state.GetState().Push(path);
		}
	}

	internal class TokenizedSassPath
	{
		public TokenizedSassPath(StyleSheetResult styleResult)
		{
			StyleResult = styleResult;
			VirtualPath = styleResult.Path;
			Extension = styleResult.Extension.EmptyNull();
			FileName = Path.GetFileName(styleResult.Path);
			FileNameWithoutExtension = FileName.Substring(0, FileName.Length - Extension.Length);
			Dir = VirtualPath.Substring(0, VirtualPath.Length - FileName.Length);
		}

		public StyleSheetResult StyleResult { get; set; }
		public string VirtualPath { get; private set; }
		public string Dir { get; private set; }
		public string FileName { get; private set; }
		public string FileNameWithoutExtension { get; private set; }
		public string Extension { get; private set; }
	}
}