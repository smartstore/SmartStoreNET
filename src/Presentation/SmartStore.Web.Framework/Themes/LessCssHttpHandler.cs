using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Optimization;
using System.Web.SessionState;
using BundleTransformer.Core;
using BundleTransformer.Core.Assets;
using BundleTransformer.Core.Configuration;
using BundleTransformer.Core.FileSystem;
using BundleTransformer.Less.HttpHandlers;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Themes
{
	
	public class LessCssHttpHandler : BundleTransformer.Less.HttpHandlers.LessAssetHandlerBase
    {
		
		public LessCssHttpHandler()
            : this(HttpContext.Current.Cache,
				BundleTransformerContext.Current.GetVirtualFileSystemWrapper(),
                BundleTransformerContext.Current.GetCoreConfiguration().AssetHandler)
        { }

        public LessCssHttpHandler(
            Cache cache,
            IVirtualFileSystemWrapper virtualFileSystemWrapper,
            AssetHandlerSettings assetHandlerConfig)
            : base(cache, virtualFileSystemWrapper, assetHandlerConfig)
        { }

        private bool IsThemeableRequest()
        {
			if (!DataSettings.DatabaseIsInstalled())
            {
                return false;
            }
            else
            {
                var webHelper = EngineContext.Current.Resolve<IWebHelper>();

                var requestUrl = webHelper.GetThisPageUrl(false);
                string themeUrl = string.Format("{0}themes", webHelper.GetStoreLocation());
                var isThemeableRequest = requestUrl.StartsWith(themeUrl + "/", StringComparison.InvariantCultureIgnoreCase);

                return isThemeableRequest;
            }
        }

        public override string GetCacheKey(string assetUrl)
        {
            string cacheKey = base.GetCacheKey(assetUrl);

            if (IsThemeableRequest())
            {
				var httpContext = HttpContext.Current;
				if (httpContext != null && httpContext.Request != null)
				{
					var qs = QueryString.Current;
					if (qs.Count > 0)
					{
						// required for Theme editing validation: See Admin.Controllers.ThemeController.ValidateLess()
						if (qs.Contains("storeId"))
						{
							httpContext.Request.SetStoreOverride(qs["storeId"].ToInt());
						}
						if (qs.Contains("theme"))
						{
							httpContext.Request.SetThemeOverride(qs["theme"]);
						}
					}
				}
				
				cacheKey += "_" + EngineContext.Current.Resolve<IThemeContext>().CurrentTheme.ThemeName + "_" + EngineContext.Current.Resolve<IStoreContext>().CurrentStore.Id;
            }

            return cacheKey;
        }

    }
}
