using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Controllers
{

    public abstract partial class PluginControllerBase : SmartController
    {
		///// <summary>
		///// Initialize controller
		///// </summary>
		///// <param name="requestContext">Request context</param>
		//protected override void Initialize(RequestContext requestContext)
		//{
		//	////set work context to admin mode
		//	//EngineContext.Current.Resolve<IWorkContext>().IsAdmin = true;

		//	base.Initialize(requestContext);
		//}

		/// <summary>
		/// Access denied view
		/// </summary>
		/// <returns>Access denied view</returns>
		protected ActionResult AccessDeniedView()
		{
			return RedirectToAction("AccessDenied", "Security", new { pageUrl = this.Request.RawUrl, area = "Admin" });
		}

        /// <summary>
        /// Renders default access denied view as a partial
        /// </summary>
        protected ActionResult AccessDeniedPartialView()
        {
            return PartialView("~/Administration/Views/Security/AccessDenied.cshtml");
        }

    }
}
