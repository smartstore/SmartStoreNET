using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Services.Pdf;
using SmartStore.Core;

namespace SmartStore.Web.Framework.Pdf
{
	public class ViewAsPdfResult : PdfResultBase
	{
		public ViewAsPdfResult(IPdfConverter converter, PdfConvertOptions options)
			: base(converter, options)
		{
		}

		public string ViewName { get; set; }

		public string MasterName { get; set; }

		public object Model { get; set; }

		protected override string GetUrl(ControllerContext context)
		{
			return string.Empty;
		}

		protected override byte[] CallConverter(ControllerContext context)
		{
			if (this.ViewName.IsEmpty())
				this.ViewName = context.RouteData.GetRequiredString("action");
			
			var html = ViewToString(context, this.ViewName, this.MasterName, this.Model);

			html = WebHelper.MakeAllUrlsAbsolute(html, context.HttpContext.Request);

			if (Options.PageHeader == null)
			{
				Options.PageHeader = PdfHeaderFooter.FromPartialView(this.ViewName + ".Header", this.Model, context, false);
			}

			if (Options.PageFooter == null)
			{
				Options.PageFooter = PdfHeaderFooter.FromPartialView(this.ViewName + ".Footer", this.Model, context, false);
			}

			var buffer = Converter.ConvertHtml(html, Options);
			return buffer;
		}

		protected virtual string ViewToString(ControllerContext context, string viewName, string masterName, object model)
		{
			var html = context.Controller.RenderViewToString(viewName, masterName, model);
			return html;
		}

	}
}
