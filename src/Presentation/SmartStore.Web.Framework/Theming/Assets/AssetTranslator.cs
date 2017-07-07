using System;
using System.Collections.Generic;
using System.Linq;
using BundleTransformer.Core.Assets;
using BundleTransformer.Core.Constants;
using BundleTransformer.Core.Translators;
using BundleTransformer.SassAndScss.Translators;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;

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

		private IAssetCache AssetCache
		{
			get { return EngineContext.Current.Resolve<IAssetCache>(); }
		}

		public bool IsDebugMode
		{
			get { return _inner.IsDebugMode; }
			set { _inner.IsDebugMode = value; }
		}

		public IList<IAsset> Translate(IList<IAsset> assets)
		{
			return _inner.Translate(assets);
			//var assets2 = WalkAndFindCachedAssets(assets).ToList();
			//var result = _inner.Translate(assets2);
			//return result;
		}

		private IEnumerable<IAsset> WalkAndFindCachedAssets(IList<IAsset> assets)
		{
			foreach (var asset in assets)
			{
				if (ValidTypeCodes.Contains(asset.AssetTypeCode.ToLowerInvariant()))
				{
					// canMinify = false, because the bundler is processing the request
					// and internally handles minification later (after combining all bundle parts)
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
			return _inner.Translate(asset);
			//return TranslateInternal(asset);
		}

		private IAsset TranslateInternal(IAsset asset)
		{
			var chronometer = EngineContext.Current.Resolve<IChronometer>();

			IAsset result;

			using (chronometer.Step("Translate asset {0}".FormatInvariant(asset.VirtualPath)))
			{
				bool validationMode = ThemeHelper.IsStyleValidationRequest();

				if (validationMode || !TryGetCachedAsset(asset, out result))
				{
					lock (String.Intern("CachedAsset:" + asset.VirtualPath))
					{
						if (validationMode || !TryGetCachedAsset(asset, out result))
						{
							using (chronometer.Step("Compile asset {0}".FormatInvariant(asset.VirtualPath)))
							{
								result = _inner.Translate(asset);
								result = AssetTranslationUtil.PostProcessAsset(result, this.IsDebugMode);

								if (!validationMode)
								{
									AssetCache.InsertAsset(
										result.VirtualPath,
										result.VirtualPathDependencies,
										result.Content);
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
					AssetTypeCode = AssetTypeCode.Unknown, // Unknown to prevent AutoPrefixer to run twice
					Combined = true,
					Content = entry.Content,
					IsStylesheet = true,
					Minified = false,
					OriginalAssets = new List<IAsset>(),
					VirtualPath = asset.VirtualPath,
					VirtualPathDependencies = entry.VirtualPathDependencies.ToList(),
					RelativePathsResolved = true,
					Url = asset.Url
				};
			}

			return cachedAsset != null;
		}
	}

	public sealed class SassTranslator : AssetTranslator<SassAndScssTranslator>
	{
		protected override string[] ValidTypeCodes
		{
			get { return new[] { "sass", "scss" }; }
		}
	}

	public sealed class LessTranslator : AssetTranslator<BundleTransformer.Less.Translators.LessTranslator>
	{
		protected override string[] ValidTypeCodes
		{
			get { return new[] { "less" }; }
		}
	}
}
