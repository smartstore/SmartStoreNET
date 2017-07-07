using System;
using System.Linq;
using BundleTransformer.Core;
using BundleTransformer.Core.Assets;
using BundleTransformer.Core.Transformers;

namespace SmartStore.Web.Framework.Theming.Assets
{
	internal static class AssetTranslationUtil
	{
		internal static IAsset PostProcessAsset(IAsset asset, bool isDebugMode)
		{
			if (asset is CachedAsset)
			{
				// Has been post-processed already previously
				return asset;
			}
				
			var transformer = BundleTransformerContext.Current.Styles.GetDefaultTransformInstance() as ITransformer;
			if (transformer != null)
			{
				var processors = transformer.PostProcessors.Where(x => x.UseInDebugMode || !isDebugMode);
				foreach (var processor in processors)
				{
					asset = processor.PostProcess(asset);
				}

				//if (!isDebugMode && canMinify)
				//{
				//	asset = transformer.Minifier.Minify(asset);
				//	asset.Minified = true;
				//}
			}

			return asset;
		}
	}
}
