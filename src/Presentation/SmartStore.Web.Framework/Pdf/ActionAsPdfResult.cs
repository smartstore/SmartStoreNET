using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.Web.Framework.Pdf
{
	public class ActionAsPdfResult : PdfResultBase
	{
		private readonly string _action;
		private readonly RouteValueDictionary _routeValues;

		public ActionAsPdfResult(string action)
			: this(action, new RouteValueDictionary())
		{
		}

		public ActionAsPdfResult(string action, object routeValues)
			: this(action, new RouteValueDictionary(routeValues))
		{
		}

		public ActionAsPdfResult(string action, RouteValueDictionary routeValues)
		{
			Guard.ArgumentNotEmpty(() => action);
			
			this._action = action;
			this._routeValues = routeValues;
		}

		protected override string GetUrl(ControllerContext context)
		{
			var urlHelper = new UrlHelper(context.RequestContext);

			string actionUrl = string.Empty;

			if (this._routeValues != null)
			{
				actionUrl = urlHelper.Action(this._action, this._routeValues);
			}
			else
			{
				actionUrl = urlHelper.Action(this._action);
			}

			string url = String.Format("{0}://{1}{2}", context.HttpContext.Request.Url.Scheme, context.HttpContext.Request.Url.Authority, actionUrl);
			return url;
		}

	}
}
