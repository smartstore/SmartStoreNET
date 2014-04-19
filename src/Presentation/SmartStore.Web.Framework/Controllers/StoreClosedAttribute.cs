using System;
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
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            HttpRequestBase request = filterContext.HttpContext.Request;
            if (request == null)
                return;

            string actionName = filterContext.ActionDescriptor.ActionName;
            if (String.IsNullOrEmpty(actionName))
                return;

            string controllerName = filterContext.Controller.ToString();
            if (String.IsNullOrEmpty(controllerName))
                return;

            //don't apply filter to child methods
            if (filterContext.IsChildAction)
                return;

			if (!DataSettings.DatabaseIsInstalled())
                return;

            var storeInformationSettings = EngineContext.Current.Resolve<StoreInformationSettings>();
            if (!storeInformationSettings.StoreClosed)
                return;

            if (//ensure it's not the Login page
                !(controllerName.Equals("SmartStore.Web.Controllers.CustomerController", StringComparison.InvariantCultureIgnoreCase) && actionName.Equals("Login", StringComparison.InvariantCultureIgnoreCase)) &&
                //ensure it's not the Logout page
                !(controllerName.Equals("SmartStore.Web.Controllers.CustomerController", StringComparison.InvariantCultureIgnoreCase) && actionName.Equals("Logout", StringComparison.InvariantCultureIgnoreCase)) &&
                //ensure it's not the store closed page
                !(controllerName.Equals("SmartStore.Web.Controllers.CommonController", StringComparison.InvariantCultureIgnoreCase) && actionName.Equals("StoreClosed", StringComparison.InvariantCultureIgnoreCase)) &&
				//ensure it's not the change language page (request)
				!(controllerName.Equals("SmartStore.Web.Controllers.CommonController", StringComparison.InvariantCultureIgnoreCase) && actionName.Equals("SetLanguage", StringComparison.InvariantCultureIgnoreCase)))
            {
                if (storeInformationSettings.StoreClosedAllowForAdmins && EngineContext.Current.Resolve<IWorkContext>().CurrentCustomer.IsAdmin())
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
    }
}
