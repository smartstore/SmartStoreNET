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
	
	public class PdfHeaderFooter : IPdfHeaderFooter
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

		public PdfHeaderFooterKind Kind
		{
			get 
			{
				return _url.HasValue() ? PdfHeaderFooterKind.Url : PdfHeaderFooterKind.Html; 
			}
		}

		public virtual string Process(string flag)
		{
			if (this.Kind == PdfHeaderFooterKind.Html)
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

			return this.Url;
		}

		public static IPdfHeaderFooter FromText(string textLeft = null, string textCenter = null, string textRight = null, string fontName = null, float? fontSize = null)
		{
			return new SimplePdfHeaderFooter
			{
				TextLeft = textLeft,
				TextCenter = textCenter,
				TextRight = textRight,
				FontName = fontName,
				FontSize = fontSize
			};
		}

		public static IPdfHeaderFooter FromHtml(string html)
		{
			Guard.ArgumentNotEmpty(() => html);

			return new PdfHeaderFooter
			{
				Html = html
			};
		}

		public static IPdfHeaderFooter FromUrl(string url, HttpRequestBase request)
		{
			Guard.ArgumentNotEmpty(() => url);
			Guard.ArgumentNotNull(() => request);

			return new PdfHeaderFooter
			{
				Url = WebHelper.GetAbsoluteUrl(url, request)
			};
		}

		public static IPdfHeaderFooter FromAction(string action, string controller, RouteValueDictionary routeValues, ControllerContext context)
		{
			Guard.ArgumentNotNull(() => context);

			string protocol = context.HttpContext.Request.Url.Scheme;
			string host = context.HttpContext.Request.Url.Host;
			return new PdfHeaderFooter
			{
				Url = UrlHelper.GenerateUrl(null, action, controller, protocol, host, null, routeValues, RouteTable.Routes, context.RequestContext, true)
			};
		}

		public static IPdfHeaderFooter FromRoute(string routeName, RouteValueDictionary routeValues, ControllerContext context)
		{
			Guard.ArgumentNotEmpty(() => routeName);
			Guard.ArgumentNotNull(() => context);

			string protocol = context.HttpContext.Request.Url.Scheme;
			string host = context.HttpContext.Request.Url.Host;
			return new PdfHeaderFooter
			{
				Url = UrlHelper.GenerateUrl(routeName, null, null, protocol, host, null, routeValues, RouteTable.Routes, context.RequestContext, true)
			};
		}

		public static IPdfHeaderFooter FromPartialView(string partialViewName, object model, ControllerContext context, bool throwOnError)
		{
			return FromViewInternal(partialViewName, null, model, true, context, throwOnError);
		}

		private static PdfHeaderFooter FromViewInternal(string viewName, string masterName, object model, bool isPartial, ControllerContext context, bool throwOnError)
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

			return new PdfHeaderFooter
			{
				Html = html
			};
		}

	}

}
