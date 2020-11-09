using System.Collections.Generic;
using System.Linq;
using BundleTransformer.Core.Assets;
using BundleTransformer.Core.Constants;
using BundleTransformer.Core.Translators;
using BundleTransformer.SassAndScss.Translators;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;
using SmartStore.Utilities.Threading;

namespace SmartStore.Web.Framework.Theming.Assets
{
    public abstract class AssetTranslator<TTranslator> : ITranslator
        where TTranslator : ITranslator, new()
    {
        private readonly ITranslator _inner;

        public AssetTranslator()
        {
            _inner = new TTranslator();
        }

        protected abstract string[] ValidTypeCodes { get; }

        private IAssetCache AssetCache => EngineContext.Current.Resolve<IAssetCache>();

        public bool IsDebugMode
        {
            get => _inner.IsDebugMode;
            set => _inner.IsDebugMode = value;
        }

        public IList<IAsset> Translate(IList<IAsset> assets)
        {
            var assets2 = WalkAndFindCachedAssets(assets).ToList();
            var result = _inner.Translate(assets2);
            return result;
        }

        private IEnumerable<IAsset> WalkAndFindCachedAssets(IList<IAsset> assets)
        {
            foreach (var asset in assets)
            {
                if (ValidTypeCodes.Contains(asset.AssetTypeCode.ToLowerInvariant()))
                {
                    yield return TranslateInternal(asset);
                }
                else
                {
                    yield return asset;
                }
            }
        }

        public IAsset Translate(IAsset asset)
        {
            return TranslateInternal(asset);
        }

        private IAsset TranslateInternal(IAsset asset)
        {
            IAsset result;
            var chronometer = EngineContext.Current.Resolve<IChronometer>();

            using (chronometer.Step("Translate asset {0}".FormatInvariant(asset.VirtualPath)))
            {
                bool validationMode = ThemeHelper.IsStyleValidationRequest();

                if (validationMode || !TryGetCachedAsset(asset, out result))
                {
                    using (KeyedLock.Lock("CachedAsset:" + asset.VirtualPath))
                    {
                        if (validationMode || !TryGetCachedAsset(asset, out result))
                        {
                            using (chronometer.Step("Compile asset {0}".FormatInvariant(asset.VirtualPath)))
                            {
                                result = _inner.Translate(asset);

                                var cachedAsset = new CachedAsset
                                {
                                    AssetTypeCode = AssetTypeCode.Css,
                                    IsStylesheet = true,
                                    Minified = result.Minified,
                                    Combined = result.Combined,
                                    Content = result.Content,
                                    OriginalAssets = asset.OriginalAssets,
                                    VirtualPath = asset.VirtualPath,
                                    VirtualPathDependencies = result.VirtualPathDependencies,
                                    Url = asset.Url
                                };

                                result = AssetTranslorUtil.PostProcessAsset(cachedAsset, this.IsDebugMode);

                                if (!validationMode)
                                {
                                    var pCodes = new List<string>(3);
                                    if (cachedAsset.Minified) pCodes.Add(DefaultAssetCache.MinificationCode);
                                    if (cachedAsset.RelativePathsResolved) pCodes.Add(DefaultAssetCache.UrlRewriteCode);
                                    if (cachedAsset.Autoprefixed) pCodes.Add(DefaultAssetCache.AutoprefixCode);

                                    AssetCache.InsertAsset(
                                        cachedAsset.VirtualPath,
                                        cachedAsset.VirtualPathDependencies,
                                        cachedAsset.Content,
                                        pCodes.ToArray());
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private bool TryGetCachedAsset(IAsset asset, out IAsset cachedAsset)
        {
            cachedAsset = null;
            var entry = AssetCache.GetAsset(asset.VirtualPath);

            if (entry != null)
            {
                cachedAsset = new CachedAsset
                {
                    AssetTypeCode = AssetTypeCode.Css,
                    Combined = true,
                    Content = entry.Content,
                    IsStylesheet = true,
                    Minified = entry.ProcessorCodes.Contains(DefaultAssetCache.MinificationCode),
                    RelativePathsResolved = entry.ProcessorCodes.Contains(DefaultAssetCache.UrlRewriteCode),
                    Autoprefixed = entry.ProcessorCodes.Contains(DefaultAssetCache.AutoprefixCode),
                    OriginalAssets = new List<IAsset>(),
                    VirtualPath = asset.VirtualPath,
                    VirtualPathDependencies = entry.VirtualPathDependencies.ToList(),
                    Url = asset.Url
                };
            }

            return cachedAsset != null;
        }
    }

    public sealed class SassTranslator : AssetTranslator<SassAndScssTranslator>
    {
        protected override string[] ValidTypeCodes => new[] { "sass", "scss" };
    }
}
