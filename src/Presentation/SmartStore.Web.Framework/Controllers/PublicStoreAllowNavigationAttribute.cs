using System;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Security;

namespace SmartStore.Web.Framework.Controllers
{
    public class PublicStoreAllowNavigationAttribute : ActionFilterAttribute
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

            var permissionService = EngineContext.Current.Resolve<IPermissionService>();
            var publicStoreAllowNavigation = permissionService.Authorize(StandardPermissionProvider.PublicStoreAllowNavigation);
            if (!publicStoreAllowNavigation &&
                //ensure it's not the Login page
                !(controllerName.Equals("SmartStore.Web.Controllers.CustomerController", StringComparison.InvariantCultureIgnoreCase) && actionName.Equals("Login", StringComparison.InvariantCultureIgnoreCase)) &&
                //ensure it's not the Logout page
                !(controllerName.Equals("SmartStore.Web.Controllers.CustomerController", StringComparison.InvariantCultureIgnoreCase) && actionName.Equals("Logout", StringComparison.InvariantCultureIgnoreCase)) &&
                //ensure it's not the Register page
                !(controllerName.Equals("SmartStore.Web.Controllers.CustomerController", StringComparison.InvariantCultureIgnoreCase) && actionName.Equals("Register", StringComparison.InvariantCultureIgnoreCase)) &&
                //ensure it's not the Password recovery page
                !(controllerName.Equals("SmartStore.Web.Controllers.CustomerController", StringComparison.InvariantCultureIgnoreCase) && actionName.Equals("PasswordRecovery", StringComparison.InvariantCultureIgnoreCase)) &&
                !(controllerName.Equals("SmartStore.Web.Controllers.CustomerController", StringComparison.InvariantCultureIgnoreCase) && actionName.Equals("PasswordRecoveryConfirm", StringComparison.InvariantCultureIgnoreCase)) &&
                //ensure it's not the Account activation page
                !(controllerName.Equals("SmartStore.Web.Controllers.CustomerController", StringComparison.InvariantCultureIgnoreCase) && actionName.Equals("AccountActivation", StringComparison.InvariantCultureIgnoreCase)) &&
                //ensure it's not the Register page
                !(controllerName.Equals("SmartStore.Web.Controllers.CustomerController", StringComparison.InvariantCultureIgnoreCase) && actionName.Equals("CheckUsernameAvailability", StringComparison.InvariantCultureIgnoreCase)))
            {
                //var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                //var loginPageUrl = webHelper.GetStoreLocation() + "login";
                //var loginPageUrl = new UrlHelper(filterContext.RequestContext).RouteUrl("login");
                //filterContext.Result = new RedirectResult(loginPageUrl);
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}
