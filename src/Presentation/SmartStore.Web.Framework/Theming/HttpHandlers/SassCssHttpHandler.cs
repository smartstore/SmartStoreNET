using System;
using System.Web;
using BundleTransformer.Core.Assets;
using BundleTransformer.Core.Transformers;
using SmartStore.Web.Framework.Theming.Assets;

namespace SmartStore.Web.Framework.Theming
{
	public class SassCssHttpHandler : CssHttpHandlerBase
	{
		protected override IAsset TranslateAssetCore(IAsset asset, ITransformer transformer, bool isDebugMode)
		{
			return InnerTranslateAsset<SassTranslator>("SassTranslator", asset, transformer, isDebugMode);
		}
	}
}
