using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Web.Framework.Pdf
{
	public class RouteAsPdfResult : PdfResultBase
	{
		private readonly string _routeName;
		private readonly RouteValueDictionary _routeValues;

		public RouteAsPdfResult(string routeName)
			: this(routeName, new RouteValueDictionary())
		{
		}

		public RouteAsPdfResult(string routeName, object routeValues)
			: this(routeName, new RouteValueDictionary(routeValues))
		{
		}

		public RouteAsPdfResult(string routeName, RouteValueDictionary routeValues)
		{
			Guard.ArgumentNotEmpty(() => routeName);
			
			this._routeName = routeName;
			this._routeValues = routeValues;
		}

		protected override string GetUrl(ControllerContext context)
		{
			var urlHelper = new UrlHelper(context.RequestContext);

			var routeUrl = string.Empty;

			if (this._routeValues != null)
			{
				routeUrl = urlHelper.RouteUrl(this._routeName, this._routeValues);
			}
			else
			{
				routeUrl = urlHelper.RouteUrl(this._routeName);
			}

			var url = String.Format("{0}://{1}{2}", context.HttpContext.Request.Url.Scheme, context.HttpContext.Request.Url.Authority, routeUrl);
			return url;
		}

	}
}
