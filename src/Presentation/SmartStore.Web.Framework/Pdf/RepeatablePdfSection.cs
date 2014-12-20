using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Services.Pdf;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Web.Framework.Pdf
{
	public class RepeatablePdfSection : IRepeatablePdfSection
	{
		private string _html;
		private string _url;

		internal string Html
		{
			get
			{
				return _html;
			}
			set
			{
				_html = value;
				_url = null;
			}
		}

		internal string Url
		{
			get
			{
				return _url;
			}
			set
			{
				_url = value;
				_html = null;
			}
		}

		public virtual string Process(out bool isUrl)
		{
			isUrl = false;

			if (this.Html.HasValue())
			{
				if (HttpContext.Current != null && HttpContext.Current.Request != null)
				{
					var html = WebHelper.MakeAllUrlsAbsolute(this.Html, new HttpRequestWrapper(HttpContext.Current.Request));
					return html;
				}
				else
				{
					return this.Html;
				}
			}
			else if (this.Url.HasValue())
			{
				isUrl = true;
				return this.Url;
			}

			return null;
		}

		public static RepeatablePdfSection FromHtml(string html)
		{
			Guard.ArgumentNotEmpty(() => html);

			return new RepeatablePdfSection
			{
				Html = html
			};
		}

		public static RepeatablePdfSection FromUrl(string url, HttpRequestBase request)
		{
			Guard.ArgumentNotEmpty(() => url);
			Guard.ArgumentNotNull(() => request);

			return new RepeatablePdfSection
			{
				Url = WebHelper.GetAbsoluteUrl(url, request)
			};
		}

		public static RepeatablePdfSection FromAction(string action, string controller, RouteValueDictionary routeValues, ControllerContext context)
		{
			Guard.ArgumentNotNull(() => context);

			string protocol = context.HttpContext.Request.Url.Scheme;
			string host = context.HttpContext.Request.Url.Host;
			return new RepeatablePdfSection
			{
				Url = UrlHelper.GenerateUrl(null, action, controller, protocol, host, null, routeValues, RouteTable.Routes, context.RequestContext, true)
			};
		}

		public static RepeatablePdfSection FromRoute(string routeName, RouteValueDictionary routeValues, ControllerContext context)
		{
			Guard.ArgumentNotEmpty(() => routeName);
			Guard.ArgumentNotNull(() => context);

			string protocol = context.HttpContext.Request.Url.Scheme;
			string host = context.HttpContext.Request.Url.Host;
			return new RepeatablePdfSection
			{
				Url = UrlHelper.GenerateUrl(routeName, null, null, protocol, host, null, routeValues, RouteTable.Routes, context.RequestContext, true)
			};
		}

		//public static RepeatablePdfSection FromView(string viewName, string masterName, object model, ControllerContext context, bool throwOnError)
		//{
		//	return FromViewInternal(viewName, masterName, model, false, context, throwOnError);
		//}

		public static RepeatablePdfSection FromPartialView(string partialViewName, object model, ControllerContext context, bool throwOnError)
		{
			return FromViewInternal(partialViewName, null, model, true, context, throwOnError);
		}

		private static RepeatablePdfSection FromViewInternal(string viewName, string masterName, object model, bool isPartial, ControllerContext context, bool throwOnError)
		{
			Guard.ArgumentNotNull(() => context);

			string html = null;

			try
			{
				if (isPartial)
				{
					html = context.Controller.RenderPartialViewToString(viewName, model);
				}
				else
				{
					html = context.Controller.RenderViewToString(viewName, masterName, model);
				}
			}
			catch (Exception ex)
			{
				if (throwOnError)
				{
					throw ex;
				}
			}

			return new RepeatablePdfSection
			{
				Html = html
			};
		}

	}
}
