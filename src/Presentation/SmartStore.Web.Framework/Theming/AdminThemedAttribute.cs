﻿using System;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.Theming
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
	public class AdminThemedAttribute : FilterAttribute, IResultFilter
	{
		public virtual void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (filterContext == null || filterContext.Result == null)
				return;

			// add extra view location formats to all view results (even the partial ones)
			// {0} is appended by view engine
			filterContext.RouteData.DataTokens["ExtraAreaViewLocations"] = new string[]
			{
				"~/Administration/Views/{1}/{0}",
				"~/Administration/Views/Shared/{0}"
			};
		}

		public virtual void OnResultExecuted(ResultExecutedContext filterContext)
		{
		}
	}
}
