using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Web.Framework.Pdf
{
	public class UrlAsPdfResult : PdfResultBase
	{
		private readonly string _url;

		public UrlAsPdfResult(string url)
		{
			Guard.ArgumentNotEmpty(() => url);
			this._url = url;
		}

		protected override string GetUrl(ControllerContext context)
		{
			if (_url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || _url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
			{
				return _url;
			}

			string url = _url;
			if (url.StartsWith("~"))
			{
				url = VirtualPathUtility.ToAbsolute(url);
			}

			url = String.Format("{0}://{1}{2}", context.HttpContext.Request.Url.Scheme, context.HttpContext.Request.Url.Authority, url);
			return url;
		}

	}
}
