using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Controllers
{
    public abstract partial class PluginControllerBase : SmartController
    {
		/// <summary>
		/// Access denied view
		/// </summary>
		/// <returns>Access denied view</returns>
		[SuppressMessage("ReSharper", "Mvc.AreaNotResolved")]
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
