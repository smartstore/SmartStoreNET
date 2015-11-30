using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Customers;

namespace SmartStore.Web.Framework.Controllers
{
    public class StoreClosedAttribute : ActionFilterAttribute
    {
		private static readonly List<Tuple<string, string>> s_permittedRoutes = new List<Tuple<string, string>> 
		{
 			new Tuple<string, string>("SmartStore.Web.Controllers.CustomerController", "Login"),
			new Tuple<string, string>("SmartStore.Web.Controllers.CustomerController", "Logout"),
			new Tuple<string, string>("SmartStore.Web.Controllers.HomeController", "StoreClosed"),
			new Tuple<string, string>("SmartStore.Web.Controllers.CommonController", "SetLanguage")
		};


		public Lazy<IWorkContext> WorkContext { get; set; }
		public Lazy<StoreInformationSettings> StoreInformationSettings { get; set; }
		
		public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            HttpRequestBase request = filterContext.HttpContext.Request;
            if (request == null)
                return;

			//don't apply filter to child methods
			if (filterContext.IsChildAction)
				return;

            string actionName = filterContext.ActionDescriptor.ActionName;
            if (String.IsNullOrEmpty(actionName))
                return;

            string controllerName = filterContext.Controller.ToString();
            if (String.IsNullOrEmpty(controllerName))
                return;

			if (!DataSettings.DatabaseIsInstalled())
                return;

            var storeInformationSettings = StoreInformationSettings.Value;
            if (!storeInformationSettings.StoreClosed)
                return;

            if (!IsPermittedRoute(controllerName, actionName)) 
			{
                if (storeInformationSettings.StoreClosedAllowForAdmins && WorkContext.Value.CurrentCustomer.IsAdmin())
                {
                    //do nothing - allow admin access
                }
                else
                {
                    var storeClosedUrl = new UrlHelper(filterContext.RequestContext).RouteUrl("StoreClosed");
                    filterContext.Result = new RedirectResult(storeClosedUrl);
                }
            }
        }

		private static bool IsPermittedRoute(string controllerName, string actionName)
		{
			foreach (var route in s_permittedRoutes)
			{
				if (controllerName.IsCaseInsensitiveEqual(route.Item1) && actionName.IsCaseInsensitiveEqual(route.Item2))
				{
					return true;
				}
			}

			return false;
		}
    }
}
