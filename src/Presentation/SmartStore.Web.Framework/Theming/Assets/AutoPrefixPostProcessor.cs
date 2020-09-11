using System.Collections.Generic;
using System.Linq;
using BundleTransformer.Core.Assets;
using BundleTransformer.Core.PostProcessors;

namespace SmartStore.Web.Framework.Theming.Assets
{
    public class AutoprefixPostProcessor : IPostProcessor
    {
        private readonly BundleTransformer.Autoprefixer.PostProcessors.AutoprefixCssPostProcessor _inner;

        public AutoprefixPostProcessor()
        {
            _inner = new BundleTransformer.Autoprefixer.PostProcessors.AutoprefixCssPostProcessor();
        }

        public bool UseInDebugMode
        {
            get => _inner.UseInDebugMode;
            set => _inner.UseInDebugMode = value;
        }

        public IList<IAsset> PostProcess(IList<IAsset> assets)
        {
            Guard.NotNull(assets, nameof(assets));

            var list = assets.Where(CanProcess).ToList();

            if (list.Count == 0)
            {
                return assets;
            }

            // Process only unprocessed assets...
            _inner.PostProcess(list);

            // Set Autoprefixed flag to true in our custom cached assets
            list.OfType<CachedAsset>().Each(x => x.Autoprefixed = true);

            // ...but return the original list
            return assets;
        }

        public IAsset PostProcess(IAsset asset)
        {
            Guard.NotNull(asset, nameof(asset));

            var cachedAsset = asset as CachedAsset;
            if (cachedAsset == null || cachedAsset.Autoprefixed == false)
            {
                asset = _inner.PostProcess(asset);

                if (cachedAsset != null)
                {
                    cachedAsset.Autoprefixed = true;
                }

                return asset;
            }

            return asset;
        }

        private static bool CanProcess(IAsset asset)
        {
            var cachedAsset = asset as CachedAsset;
            return asset.IsStylesheet && (cachedAsset == null || cachedAsset.Autoprefixed == false);
        }
    }
}
