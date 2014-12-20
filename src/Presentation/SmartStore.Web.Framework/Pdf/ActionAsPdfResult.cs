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
	public class ActionAsPdfResult : PdfResultBase
	{
		private readonly string _action;
		private readonly RouteValueDictionary _routeValues;

		public ActionAsPdfResult(string action, IPdfConverter converter, PdfConvertOptions options)
			: this(action, new RouteValueDictionary(), converter, options)
		{
		}

		public ActionAsPdfResult(string action, object routeValues, IPdfConverter converter, PdfConvertOptions options)
			: this(action, new RouteValueDictionary(routeValues), converter, options)
		{
		}

		public ActionAsPdfResult(string action, RouteValueDictionary routeValues, IPdfConverter converter, PdfConvertOptions options)
			: base(converter, options)
		{
			Guard.ArgumentNotEmpty(() => action);
			
			this._action = action;
			this._routeValues = routeValues;
		}

		protected override string GetUrl(ControllerContext context)
		{
			string protocol = context.HttpContext.Request.Url.Scheme;
			string host = context.HttpContext.Request.Url.Host;
			return UrlHelper.GenerateUrl(null, _action, null, protocol, host, null, _routeValues, RouteTable.Routes, context.RequestContext, true);
		}

	}
}
