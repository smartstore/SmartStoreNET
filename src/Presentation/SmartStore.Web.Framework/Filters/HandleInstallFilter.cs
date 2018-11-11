﻿using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Data;

namespace SmartStore.Web.Framework.Filters
{ 
    public class HandleInstallFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var actionName = filterContext.ActionDescriptor.ActionName;

            if (controllerName == "Install" && actionName != "Index" && filterContext.HttpContext.Request.IsAjaxRequest())
            {
                // probably "progress" or "finalize" call
                return;
            }

            if (!DataSettings.DatabaseIsInstalled() && controllerName != "Install")
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                        { "Controller", "Install" }, 
                        { "Action", "Index" } 
                    });
            }
        }
        
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // nada
        }

    }

}
