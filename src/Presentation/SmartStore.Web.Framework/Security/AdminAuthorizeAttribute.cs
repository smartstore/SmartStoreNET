using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Security;

namespace SmartStore.Web.Framework.Security
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class AdminAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        public IPermissionService PermissionService { get; set; }

        private void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new HttpUnauthorizedResult();
        }

        private IEnumerable<AdminAuthorizeAttribute> GetAdminAuthorizeAttributes(ActionDescriptor descriptor)
        {
            return descriptor.GetCustomAttributes(typeof(AdminAuthorizeAttribute), true)
                .Concat(descriptor.ControllerDescriptor.GetCustomAttributes(typeof(AdminAuthorizeAttribute), true))
                .OfType<AdminAuthorizeAttribute>();
        }

        private bool IsAdminPageRequested(AuthorizationContext filterContext)
        {
            var adminAttributes = GetAdminAuthorizeAttributes(filterContext.ActionDescriptor);
            if (adminAttributes != null && adminAttributes.Any())
            {
                return true;
            }

            return false;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            Guard.NotNull(filterContext, nameof(filterContext));

            if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
            {
                throw new InvalidOperationException("You cannot use [AdminAuthorize] attribute when a child action cache is active.");
            }

            if (IsAdminPageRequested(filterContext))
            {
                if (!HasAdminAccess(filterContext))
                {
                    HandleUnauthorizedRequest(filterContext);
                }
            }
        }

        public virtual bool HasAdminAccess(AuthorizationContext filterContext)
        {
            if (PermissionService.Authorize(Permissions.System.AccessBackend))
            {
                return true;
            }

            if (PermissionService.AuthorizeByAlias(Permissions.System.AccessBackend))
            {
                return true;
            }

            return false;
        }
    }
}
