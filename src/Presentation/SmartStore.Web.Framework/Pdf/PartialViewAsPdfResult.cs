using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Web.Framework.Pdf
{
	public class PartialViewAsPdfResult : ViewAsPdfResult
	{
		protected override string ViewToString(ControllerContext context, string viewName, string masterName, object model)
		{
			var html = context.Controller.RenderPartialViewToString(viewName, model);
			return html;
		}

	}
}
