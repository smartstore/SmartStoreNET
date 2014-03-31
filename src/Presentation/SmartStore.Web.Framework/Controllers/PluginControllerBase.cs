using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.Controllers
{

    [AdminAuthorize]
    public abstract partial class PluginControllerBase : SmartController
    {
        /// <summary>
        /// Initialize controller
        /// </summary>
        /// <param name="requestContext">Request context</param>
        protected override void Initialize(RequestContext requestContext)
        {
            //set work context to admin mode
            EngineContext.Current.Resolve<IWorkContext>().IsAdmin = true;

            base.Initialize(requestContext);
        }

        /// <summary>
        /// Renders default access denied view as a partial
        /// </summary>
        /// <remarks>codehint: sm-add</remarks>
        protected ActionResult AccessDeniedPartialView()
        {
            return PartialView("~/Administration/Views/Security/AccessDenied.cshtml");
        }

    }
}
