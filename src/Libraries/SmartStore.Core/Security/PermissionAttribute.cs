﻿using System;
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
        /// <param name="addMessageHeader">Whether to show an unauthorization message for AJAX operations.</param>
        public PermissionAttribute(string systemName, bool addMessageHeader = true)
        {
            Guard.NotEmpty(systemName, nameof(systemName));

            SystemName = systemName;
            AddMessageHeader = addMessageHeader;

            T = NullLocalizer.Instance;
        }

        /// <summary>
        /// The system name of the permission.
        /// </summary>
        public string SystemName { get; private set; }

        /// <summary>
        /// Whether to show an unauthorization message for AJAX operations.
        /// </summary>
        public bool AddMessageHeader { get; private set; }

        public Localizer T { get; set; }
        public IWorkContext WorkContext { get; set; }
        public IPermissionService PermissionService { get; set; }

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
            {
                return;
            }

            var message = PermissionService.GetUnauthorizedMessage(SystemName);

            if (request.IsAjaxRequest())
            {
                if (AddMessageHeader)
                {
                    httpContext.Response.AddHeader("X-Message-Type", "error");
                    httpContext.Response.AddHeader("X-Message", message);
                }

                if (request.AcceptTypes?.Any(x => x.IsCaseInsensitiveEqual("text/html")) ?? false)
                {
                    filterContext.Result = AccessDeniedResult(message);
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
                            message
                        }
                    };
                }
            }
            else
            {
                if (filterContext.IsChildAction)
                {
                    filterContext.Result = AccessDeniedResult(message);
                }
                else
                {
                    var urlHelper = new UrlHelper(request.RequestContext);
                    var url = urlHelper.Action("AccessDenied", "Security", new { pageUrl = request.RawUrl, area = "Admin" });

                    filterContext.Controller.TempData["UnauthorizedMessage"] = message;
                    filterContext.Result = new RedirectResult(url);
                }
            }
        }

        protected virtual ActionResult AccessDeniedResult(string message)
        {
            return new ContentResult
            {
                Content = "<div class=\"alert alert-danger\">{0}</div>".FormatInvariant(message),
                ContentType = "text/html",
                ContentEncoding = Encoding.UTF8
            };
        }
    }
}
