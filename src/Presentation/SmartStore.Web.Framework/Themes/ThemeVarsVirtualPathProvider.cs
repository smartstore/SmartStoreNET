using System;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Optimization;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Themes
{
    public class ThemeVarsVirtualPathProvider : VirtualPathProvider
    {
        private readonly VirtualPathProvider _previous;

        public ThemeVarsVirtualPathProvider(VirtualPathProvider previous)
        {
            _previous = previous;
        }

        private bool IsThemeVars(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
                return false;

            return virtualPath.ToLower().EndsWith("/.db/themevars.less");
        }

        public override bool FileExists(string virtualPath)
        {
            return (IsThemeVars(virtualPath) || _previous.FileExists(virtualPath));
        }
        

        public override VirtualFile GetFile(string virtualPath)
        {
            if (IsThemeVars(virtualPath))
            {
                string themeName = EngineContext.Current.Resolve<IThemeContext>().WorkingDesktopTheme;
                int storeId = EngineContext.Current.Resolve<IStoreContext>().CurrentStore.Id;
                //virtualPath = ToStoreSpecificPath(virtualPath, storeId);
                return new ThemeVarsVirtualFile(virtualPath, themeName, storeId);
            }

            return _previous.GetFile(virtualPath);
        }

        //public override string GetCacheKey(string virtualPath)
        //{
        //    if (virtualPath.EndsWith(".less", StringComparison.OrdinalIgnoreCase))
        //    {
        //        return Guid.NewGuid().ToString();
        //    }
        //    return _previous.GetCacheKey(virtualPath);
        //}

        //public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        //{
        //    if (virtualPath.EndsWith(".less", StringComparison.OrdinalIgnoreCase))
        //    {
        //        //return virtualPath;
        //        return Guid.NewGuid().ToString();
        //    }
        //    return _previous.GetFileHash(virtualPath, virtualPathDependencies);
        //}
        
        public override CacheDependency GetCacheDependency(
            string virtualPath,
            IEnumerable virtualPathDependencies,
            DateTime utcStart)
        {
            //if (virtualPath.EndsWith(".less", StringComparison.OrdinalIgnoreCase))
            //{
            //    return null;
            //}
            //return _previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);

            bool isLess;
            if (IsStyleSheet(virtualPath, out isLess))
            {
                if (!isLess)
                {
                    var bundle = BundleTable.Bundles.GetBundleFor(virtualPath);
                    if (bundle == null)
                    {
                        return _previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
                    }
                }
                
                var arrPathDependencies = virtualPathDependencies.Cast<string>().ToArray();

                // determine the virtual themevars.less import reference
                var themeVarsFile = arrPathDependencies.Where(x => IsThemeVars(x)).FirstOrDefault();

                if (themeVarsFile.IsEmpty())
                {
                    // no themevars import... so no special considerations here
                    return _previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
                }

                // exclude the themevars import from the file dependencies list,
                // 'cause this one cannot be monitored by the physical file system
                var fileDependencies = arrPathDependencies.Except(new string[] { themeVarsFile });

                if (arrPathDependencies.Any())
                {
                    string themeName = EngineContext.Current.Resolve<IThemeContext>().WorkingDesktopTheme;
                    int storeId = EngineContext.Current.Resolve<IStoreContext>().CurrentStore.Id;
                    // invalidate the cache when variables change
                    string cacheKey = AspNetCache.BuildKey(FrameworkCacheConsumer.BuildThemeVarsCacheKey(themeName, storeId));
                    
                    //var cacheDependency = new CacheDependency(fileDependencies.ToArray(), new string[] { cacheKey }, utcStart);
                    var cacheDependency = new CacheDependency(fileDependencies.Select(x => HostingEnvironment.MapPath(x)).ToArray(), new string[] { cacheKey }, utcStart);
                    //var fileDep = _previous.GetCacheDependency(virtualPath, fileDependencies, utcStart);

                    //var aggDep = new AggregateCacheDependency();
                    //aggDep.Add(themeVarsDep, fileDep);
                    //return aggDep;

                    return cacheDependency;
                }

                return null;
            }

            return _previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

        private static bool IsStyleSheet(string virtualPath, out bool isLess)
        {
            bool isCss = false;
            isLess = virtualPath.EndsWith(".less", StringComparison.OrdinalIgnoreCase);
            if (!isLess)
                isCss = virtualPath.EndsWith(".css", StringComparison.OrdinalIgnoreCase);
            return isLess || isCss;
        }

        private string ToStoreSpecificPath(string virtualPath, int storeId)
        {
            //return virtualPath; // TODO: Temp

            if (VirtualPathUtility.IsAbsolute(virtualPath)) {
                return virtualPath;
            }
            return "/{0}{1}".FormatInvariant(storeId, virtualPath.TrimStart('~'));
        }
    }
}