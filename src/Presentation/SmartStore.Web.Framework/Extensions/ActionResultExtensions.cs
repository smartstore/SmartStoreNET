using System;
using System.Web.Mvc;

// use base SmartStore Namespace to ensure the extension methods are always available
namespace SmartStore
{
	public static class ActionResultExtensions
	{
		public static bool IsHtmlViewResult(this ActionResult result)
		{
			if (result is ViewResultBase)
				return true;

			var contentResult = result as ContentResult;
			if (contentResult != null)
			{
				return contentResult.ContentType.IsCaseInsensitiveEqual("text/html");
			}

			return false;
		}
	}
}
