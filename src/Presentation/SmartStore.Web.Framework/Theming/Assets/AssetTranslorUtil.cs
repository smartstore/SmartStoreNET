using System;
using System.Linq;
using BundleTransformer.Core;
using BundleTransformer.Core.Assets;
using BundleTransformer.Core.Transformers;

namespace SmartStore.Web.Framework.Theming.Assets
{
	internal static class AssetTranslorUtil
	{
		internal static IAsset PostProcessAsset(IAsset asset, bool isDebugMode)
		{
			var transformer = BundleTransformerContext.Current.Styles.GetDefaultTransformInstance() as ITransformer;
			if (transformer != null)
			{
				var processors = transformer.PostProcessors.Where(x => x.UseInDebugMode || !isDebugMode);
				foreach (var processor in processors)
				{
					asset = processor.PostProcess(asset);
				}
			}

			return asset;
		}
	}
}
