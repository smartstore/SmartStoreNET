using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Seo;
using SmartStore.Web.Framework.Theming;

namespace SmartStore.Web.Framework.Controllers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class NonAdminAttribute : FilterAttribute, IActionFilter
    {
        public Lazy<IWorkContext> WorkContext { get; set; }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (!filterContext.IsChildAction)
            {
                WorkContext.Value.IsAdmin = false;
                filterContext.HttpContext.Items["IsNonAdmin"] = true;
            }
        }
    }

    [AdminValidateIpAddress]
    [RewriteUrl(SslRequirement.Yes, Order = 0, AppendTrailingSlash = false, LowercaseUrls = false)]
    [CustomerLastActivity(Order = int.MaxValue)]
    [StoreIpAddress(Order = int.MaxValue)]
    [AdminThemed]
    public abstract class AdminControllerBase : ManageController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            var isNonAdmin = filterContext.ActionDescriptor.HasAttribute<NonAdminAttribute>(true);
            Services.WorkContext.IsAdmin = !isNonAdmin;
        }
    }
}
