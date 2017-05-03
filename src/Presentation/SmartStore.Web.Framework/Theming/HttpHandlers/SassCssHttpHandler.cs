using System;
using System.Web;
using BundleTransformer.Core.Assets;
using BundleTransformer.Core.Transformers;
using BundleTransformer.SassAndScss.Translators;

namespace SmartStore.Web.Framework.Theming
{
	public class SassCssHttpHandler : CssHttpHandlerBase
	{
		protected override IAsset TranslateAssetCore(IAsset asset, ITransformer transformer, bool isDebugMode)
		{
			var validate = _context.Request.QueryString["validate"].HasValue();

			try
			{
				return InnerTranslateAsset<SassAndScssTranslator>("SassAndScssTranslator", asset, transformer, isDebugMode);
			}
			catch (Exception ex)
			{
				if (validate)
				{
					_context.Response.Write(ex.Message);
					_context.Response.StatusCode = 500;
					_context.Response.End();
				}

				throw;
			}
		}
	}
}
