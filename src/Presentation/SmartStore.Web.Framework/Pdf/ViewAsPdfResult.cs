using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using SmartStore.Web.Framework.Controllers;
using NReco.PdfGenerator;

namespace SmartStore.Web.Framework.Pdf
{
	public class ViewAsPdfResult : PdfResultBase
	{
		private static readonly Regex _htmlPathPattern = new Regex(@"(?<=(?:href|src)=(?:""|'))(?!https?://)(?<url>[^(?:""|')]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
		private static readonly Regex _cssPathPattern = new Regex(@"url\('(?<url>.+)'\)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

		public string ViewName { get; set; }

		public string MasterName { get; set; }

		public object Model { get; set; }

		protected override string GetUrl(ControllerContext context)
		{
			return string.Empty;
		}

		protected override byte[] CallConverter(ControllerContext context, HtmlToPdfConverter converter)
		{
			var html = ViewToString(context, this.ViewName, this.MasterName, this.Model);

			html = ReplaceRelativeUrls(html);

			var buffer = converter.GeneratePdf(html);
			return buffer;
		}

		protected virtual string ViewToString(ControllerContext context, string viewName, string masterName, object model)
		{
			var html = context.Controller.RenderViewToString(viewName, masterName, model);
			return html;
		}

		protected string ReplaceRelativeUrls(string html)
		{
			string baseUrl = string.Format("{0}://{1}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority.TrimEnd('/'));
			
			MatchEvaluator evaluator = (match) => {
				var url = match.Groups["url"].Value;
				return "{0}{1}".FormatCurrent(baseUrl, url.EnsureStartsWith("/"));
			};
			
			html = _htmlPathPattern.Replace(html, evaluator);
			html = _cssPathPattern.Replace(html, evaluator);

			return html;
		}
	}
}
