using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using BundleTransformer.Core;
using BundleTransformer.Core.Assets;
using BundleTransformer.Core.Constants;
using BundleTransformer.Core.Transformers;
using BundleTransformer.Core.Translators;
using BundleTransformer.SassAndScss.Translators;
using SmartStore.Core.Infrastructure;

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
					// canMinify = false, because the bundler is processing the request
					// and internally handles minification later (after combining all bundle parts)
					yield return TranslateInternal(asset, false);
				}
				else
				{
					yield return asset;
				}
			}
		}

		public IAsset Translate(IAsset asset)
		{
			// canMinify, because the HTTP handler is processing the request (no bundling)
			return TranslateInternal(asset, true);
		}

		private IAsset TranslateInternal(IAsset asset, bool canMinify)
		{
			IAsset result;

			if (!TryGetCachedAsset(asset, out result))
			{
				lock (String.Intern("CachedAsset:" + asset.VirtualPath))
				{
					if (!TryGetCachedAsset(asset, out result))
					{
						result = _inner.Translate(asset);

						bool validate = HttpContext.Current?.Request?.QueryString["validate"] != null;

						if (!validate)
						{
							result = AssetTranslationUtil.PostProcessAsset(result, this.IsDebugMode, canMinify);
							AssetCache.InsertAsset(
								result.VirtualPath,
								result.VirtualPathDependencies,
								result.Content);
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
					Minified = true,
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
