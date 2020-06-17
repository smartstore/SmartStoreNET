using System;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
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

	[AdminValidateIpAddress(Order = 100)]
	[RewriteUrl(SslRequirement.Yes, Order = 110)]
    [CustomerLastActivity(Order = 100)]
    [StoreIpAddress(Order = 100)]
	[AdminThemed(Order = -1)]
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
