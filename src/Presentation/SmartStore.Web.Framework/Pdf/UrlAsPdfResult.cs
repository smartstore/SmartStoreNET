using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Services.Pdf;

namespace SmartStore.Web.Framework.Pdf
{
	public class UrlAsPdfResult : PdfResultBase
	{
		private readonly string _url;

		public UrlAsPdfResult(string url, IPdfConverter converter, PdfConvertOptions options)
			: base(converter, options)
		{
			Guard.ArgumentNotEmpty(() => url);
			this._url = url;
		}

		protected override string GetUrl(ControllerContext context)
		{
			return WebHelper.GetAbsoluteUrl(_url, context.HttpContext.Request);
		}

	}
}
