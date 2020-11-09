using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Caching;
using System.Web.Hosting;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Theming.Assets
{
    public sealed class BundlingVirtualPathProvider : ThemingVirtualPathProvider
    {
        private readonly SassCheckedPathStack _sassCheckedPathStack;

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

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (ThemeHelper.IsStyleValidationRequest())
            {
                var themeVarsPath = virtualPathDependencies.Cast<string>().FirstOrDefault(x => ThemeHelper.PathIsThemeVars(x));
                if (themeVarsPath.HasValue())
                {
                    return base.GetCacheDependency(virtualPath, new[] { themeVarsPath }, utcStart);
                }
            }

            return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
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

            if (currentPath.StyleResult.IsBaseImport && lastPath.StyleResult.IsBaseImport && currentPath.FileName == lastPath.FileName)
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
            if (!currentPath.StyleResult.IsBaseImport && _styleExtensions.Contains(currentPath.Extension) && currentPath.FileNameWithoutExtension == lastPath.FileNameWithoutExtension)
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