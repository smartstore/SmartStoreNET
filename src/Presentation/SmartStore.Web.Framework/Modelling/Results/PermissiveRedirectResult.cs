using System;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Modelling
{
	/// <summary>
	/// Allows redirects from within child actions while keeping TempData
	/// </summary>
	public class PermissiveRedirectResult : ActionResult
	{
		private readonly string _url;

		public PermissiveRedirectResult(string url)
		{
			_url = url;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			var url = UrlHelper.GenerateContentUrl(_url, context.HttpContext);
			context.Controller.TempData.Keep();
			context.HttpContext.Response.Redirect(url, false);
		}
	}
}
