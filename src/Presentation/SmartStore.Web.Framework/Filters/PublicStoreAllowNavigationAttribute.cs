using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Data;
using SmartStore.Core.Security;

namespace SmartStore.Web.Framework.Filters
{
    public class PublicStoreAllowNavigationAttribute : FilterAttribute, IAuthorizationFilter
    {
        private static readonly List<Tuple<string, string>> s_permittedRoutes = new List<Tuple<string, string>>
        {
             new Tuple<string, string>("SmartStore.Web.Controllers.CustomerController", "Login"),
            new Tuple<string, string>("SmartStore.Web.Controllers.CustomerController", "Logout"),
            new Tuple<string, string>("SmartStore.Web.Controllers.CustomerController", "Register"),
            new Tuple<string, string>("SmartStore.Web.Controllers.CustomerController", "PasswordRecovery"),
            new Tuple<string, string>("SmartStore.Web.Controllers.CustomerController", "PasswordRecoveryConfirm"),
            new Tuple<string, string>("SmartStore.Web.Controllers.CustomerController", "AccountActivation"),
            new Tuple<string, string>("SmartStore.Web.Controllers.CustomerController", "CheckUsernameAvailability"),
            new Tuple<string, string>("SmartStore.Web.Controllers.MenuController", "OffCanvas"),
            new Tuple<string, string>("SmartStore.Web.Controllers.ShoppingCartController", "CartSummary")
        };

        public Lazy<IPermissionService> PermissionService { get; set; }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null || filterContext.HttpContext == null)
                return;

            HttpRequestBase request = filterContext.HttpContext.Request;
            if (request == null)
                return;

            // Don't apply filter to child methods.
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

            if (!HasStoreAccess() && !IsPermittedRoute(controllerName, actionName))
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }

        protected virtual bool HasStoreAccess()
        {
            if (PermissionService.Value.Authorize(Permissions.System.AccessShop))
            {
                return true;
            }

            if (PermissionService.Value.AuthorizeByAlias(Permissions.System.AccessShop))
            {
                return true;
            }

            return false;
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
