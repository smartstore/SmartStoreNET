using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Web;

namespace SmartStore.Web.Framework.Controllers
{
	
	/// <summary>
	/// Instructs the view engine to additionally search in the admin area for views.
	/// </summary>
	/// <remarks>
	/// The "admin area" corresponds to the <c>~/Administration</c> base folder.
	/// This attribute is useful in plugins - which usually are areas on its own - where views
	/// should be rendered as part of the admin backend.
	/// Without this attribute the view resolver would directly fallback to the default nameless area
	/// when a view could not be resolved from within the plugin area.
	/// </remarks>
	public class AdminThemedAttribute : ActionFilterAttribute
	{

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (filterContext == null)
				return;

			// add extra view location formats to all view results (even the partial ones)
			var viewResultBase = filterContext.Result as ViewResultBase;
			if (viewResultBase != null)
			{
				filterContext.RouteData.DataTokens["ExtraAreaViewLocations"] = new string[] 
				{
					"~/Administration/Views/{1}/{0}.cshtml",
					"~/Administration/Views/Shared/{0}.cshtml"
				};
			}

			// set MasterName (aka Layout) to view results if not explicitly specified
			var viewResult = filterContext.Result as ViewResult;
			if (viewResult != null && viewResult.MasterName.IsEmpty())
			{
				viewResult.MasterName = "_AdminLayout";
			}
		}

	}
}
