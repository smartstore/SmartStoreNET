using System.Web.Mvc;

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

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (filterContext == null)
				return;

			// add extra view location formats to all view results (even the partial ones)
			filterContext.RouteData.DataTokens["ExtraAreaViewLocations"] = new string[] 
			{
				"~/Administration/Views/{1}/{0}.cshtml",
				"~/Administration/Views/Shared/{0}.cshtml"
			};
		}

	}
}
