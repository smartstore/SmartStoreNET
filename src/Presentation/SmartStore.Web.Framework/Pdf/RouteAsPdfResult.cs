using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Services.Pdf;

namespace SmartStore.Web.Framework.Pdf
{
	public class RouteAsPdfResult : PdfResultBase
	{
		private readonly string _routeName;
		private readonly RouteValueDictionary _routeValues;

		public RouteAsPdfResult(string routeName, IPdfConverter converter, PdfConvertOptions options)
			: this(routeName, new RouteValueDictionary(), converter, options)
		{
		}

		public RouteAsPdfResult(string routeName, object routeValues, IPdfConverter converter, PdfConvertOptions options)
			: this(routeName, new RouteValueDictionary(routeValues), converter, options)
		{
		}

		public RouteAsPdfResult(string routeName, RouteValueDictionary routeValues, IPdfConverter converter, PdfConvertOptions options)
			: base(converter, options)
		{
			Guard.ArgumentNotEmpty(() => routeName);
			
			this._routeName = routeName;
			this._routeValues = routeValues;
		}

		protected override string GetUrl(ControllerContext context)
		{
			string protocol = context.HttpContext.Request.Url.Scheme;
			string host = context.HttpContext.Request.Url.Host;
			return UrlHelper.GenerateUrl(_routeName, null, null, protocol, host, null, _routeValues, RouteTable.Routes, context.RequestContext, true);
		}

	}
}
