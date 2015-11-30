using System.Web.Mvc;

namespace SmartStore.Web.Framework
{
    public static class UrlHelperExtensions
    {
        public static string LogOn(this UrlHelper urlHelper, string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
                return urlHelper.Action("Login", "Customer", new { ReturnUrl = returnUrl, area = "" });
			return urlHelper.Action("Login", "Customer", new { area = "" });
        }

        public static string LogOff(this UrlHelper urlHelper, string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl))
                return urlHelper.Action("Logout", "Customer", new { ReturnUrl = returnUrl, area = "" });
			return urlHelper.Action("Logout", "Customer", new { area = "" });
        }

		public static string Referrer(this UrlHelper urlHelper, string fallbackUrl = "")
		{
			var request = urlHelper.RequestContext.HttpContext.Request;
			if (request.UrlReferrer != null && request.UrlReferrer.ToString().HasValue())
			{
				return request.UrlReferrer.ToString();
			}

			return fallbackUrl;
		}
    }
}
