using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using SmartStore.Core.Localization;

namespace SmartStore.Core.Security
{
    /// <summary>
    /// Checks request permission for the current customer.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public partial class PermissionAttribute : FilterAttribute, IAuthorizationFilter
    {
        /// <summary>
        /// e.g. [Permission(PermissionSystemNames.Customer.Read)]
        /// </summary>
        /// <param name="systemName">The system name of the permission.</param>
        public PermissionAttribute(string systemName)
        {
            Guard.NotEmpty(systemName, nameof(systemName));

            SystemName = systemName;

            T = NullLocalizer.Instance;
        }

        /// <summary>
        /// The system name of the permission.
        /// </summary>
        public string SystemName { get; private set; }

        public Localizer T { get; set; }
        public IWorkContext WorkContext { get; set; }
        public IPermissionService2 PermissionService { get; set; }

        public virtual void OnAuthorization(AuthorizationContext filterContext)
        {
            Guard.NotNull(filterContext, nameof(filterContext));

            if (PermissionService.Authorize(SystemName, WorkContext.CurrentCustomer))
            {
                return;
            }

            try
            {
                HandleUnauthorizedRequest(filterContext);
            }
            catch
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }
        }

        protected virtual void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            var request = httpContext?.Request;

            if (request == null)
                return;

            if (request.IsAjaxRequest())
            {
                httpContext.Response.AddHeader("X-Message-Type", "error");
                httpContext.Response.AddHeader("X-Message", T("Admin.AccessDenied.Description"));

                if (request.AcceptTypes?.Any(x => x.IsCaseInsensitiveEqual("text/html")) ?? false)
                {
                    filterContext.Result = AccessDeniedResult();
                }
                else
                {
                    filterContext.Result = new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new
                        {
                            error = true,
                            success = false,
                            controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                            action = filterContext.ActionDescriptor.ActionName,
                            message = T("Admin.AccessDenied.Description").Text
                        }
                    };
                }
            }
            else
            {
                if (filterContext.IsChildAction)
                {
                    filterContext.Result = AccessDeniedResult();
                }
                else
                {
                    var urlHelper = new UrlHelper(request.RequestContext);
                    var url = urlHelper.Action("AccessDenied", "Security", new { pageUrl = request.RawUrl, area = "Admin" });
                    filterContext.Result = new RedirectResult(url);
                }
            }
        }

        protected virtual ContentResult AccessDeniedResult()
        {
            return new ContentResult
            {
                Content = "<div class=\"alert alert-danger\">{0}</div>".FormatInvariant(T("Admin.AccessDenied.Description")),
                ContentType = "text/html",
                ContentEncoding = Encoding.UTF8
            };
        }
    }
}
