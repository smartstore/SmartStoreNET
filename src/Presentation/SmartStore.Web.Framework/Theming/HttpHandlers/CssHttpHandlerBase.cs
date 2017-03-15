using System;
using System.Web;
using System.Web.Caching;
using BundleTransformer.Core;
using BundleTransformer.Core.Assets;
using BundleTransformer.Core.Configuration;
using BundleTransformer.Core.FileSystem;
using BundleTransformer.Core.Transformers;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Theming
{
	public abstract class CssHttpHandlerBase : BundleTransformer.Core.HttpHandlers.StyleAssetHandlerBase
	{
		protected CssHttpHandlerBase()
            : this(HttpContext.Current.Cache,
				BundleTransformerContext.Current.FileSystem.GetVirtualFileSystemWrapper(),
				BundleTransformerContext.Current.Configuration.GetCoreSettings().AssetHandler)
        { }

		protected CssHttpHandlerBase(
            Cache cache,
            IVirtualFileSystemWrapper virtualFileSystemWrapper,
            AssetHandlerSettings assetHandlerConfig)
            : base(cache, virtualFileSystemWrapper, assetHandlerConfig)
        {
		}

		protected override bool IsStaticAsset
		{
			get { return false; }
		}

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

		protected override string GetCacheKey(string assetVirtualPath, string bundleVirtualPath)
		{
			string cacheKey = base.GetCacheKey(assetVirtualPath, bundleVirtualPath);

			if (IsThemeableRequest())
			{
				var httpContext = HttpContext.Current;
				if (httpContext != null && httpContext.Request != null)
				{
					var qs = QueryString.Current;
					if (qs.Count > 0)
					{
						// required for Theme editing validation: See Admin.Controllers.ThemeController.ValidateLess()
						if (qs.Contains("theme"))
						{
							EngineContext.Current.Resolve<IThemeContext>().SetRequestTheme(qs["theme"]);
						}
						if (qs.Contains("storeId"))
						{
							EngineContext.Current.Resolve<IStoreContext>().SetRequestStore(qs["storeId"].ToInt());
						}
					}
				}

				cacheKey += "_" + EngineContext.Current.Resolve<IThemeContext>().CurrentTheme.ThemeName + "_" + EngineContext.Current.Resolve<IStoreContext>().CurrentStore.Id;
			}

			return cacheKey;
		}

		protected override IAsset TranslateAsset(IAsset asset, ITransformer transformer, bool isDebugMode)
		{
			var processedAsset = TranslateAssetCore(asset, transformer, isDebugMode);

			if (transformer == null)
			{
				// BundleTransformer does NOT PostProcess when transformer instance is null,
				// therefore we handle it ourselves, because we desperately need
				// AutoPrefixer even in debug mode.
				return PostProcessAsset(processedAsset, isDebugMode);
			}
			else
			{
				return processedAsset;
			}
		}	

		private IAsset PostProcessAsset(IAsset asset, bool isDebugMode)
		{
			var transformer = BundleTransformerContext.Current.Styles.GetDefaultTransformInstance() as ITransformer;
			if (transformer != null)
			{
				return this.PostProcessAsset(asset, transformer);
			}

			return asset;
		}

		protected abstract IAsset TranslateAssetCore(IAsset asset, ITransformer transformer, bool isDebugMode);
	}
}
