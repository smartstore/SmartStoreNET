﻿using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Common;
using SmartStore.Services.Stores;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Modelling.Results;

#pragma warning disable 1573

namespace SmartStore.Web.Framework.Controllers
{
    public static class ContollerExtensions
    {
		#region InvokeAction

		public static MvcHtmlString InvokeAction(this ControllerBase controller, string actionName, string controllerName = null, RouteValueDictionary routeValues = null)
		{
			Guard.NotNull(controller, nameof(controller));
			Guard.NotEmpty(actionName, nameof(actionName));

			using (StringWriter writer = new StringWriter(CultureInfo.CurrentCulture))
			{
				InvokeAction(controller, writer, actionName, controllerName, routeValues);
				return MvcHtmlString.Create(writer.ToString());
			}
		}

		public static void InvokeAction(this ControllerBase controller, TextWriter writer, string actionName, string controllerName = null, RouteValueDictionary routeValues = null)
		{
			Guard.NotNull(controller, nameof(controller));
			Guard.NotNull(writer, nameof(writer));
			Guard.NotEmpty(actionName, nameof(actionName));

			var viewContext = new ViewContext(
				controller.ControllerContext,
				new WebFormView(controller.ControllerContext, "tmp"),
				controller.ViewData,
				controller.TempData,
				writer
			);

			var htmlHelper = new HtmlHelper(viewContext, new ViewPage());
			htmlHelper.RenderAction(actionName, controllerName, routeValues);
		}

		#endregion

		#region Render view

		/// <summary>
		/// Render partial view to string
		/// </summary>
		/// <returns>Result</returns>
		public static string RenderPartialViewToString(this ControllerBase controller)
        {
            return RenderPartialViewToString(controller, null, null, null);
        }

	    /// <summary>
	    /// Render partial view to string
	    /// </summary>
	    /// <param name="controller"></param>
	    /// <param name="viewName">View name</param>
	    /// <returns>Result</returns>
	    public static string RenderPartialViewToString(this ControllerBase controller, string viewName)
        {
            return RenderPartialViewToString(controller, viewName, null, null);
        }

	    /// <summary>
	    /// Render partial view to string
	    /// </summary>
	    /// <param name="controller"></param>
	    /// <param name="model">Model</param>
	    /// <returns>Result</returns>
	    public static string RenderPartialViewToString(this ControllerBase controller, object model)
        {
            return RenderPartialViewToString(controller, null, model, null);
        }

	    /// <summary>
	    /// Render partial view to string
	    /// </summary>
	    /// <param name="controller"></param>
	    /// <param name="viewName">View name</param>
	    /// <param name="model">Model</param>
	    /// <returns>Result</returns>
	    public static string RenderPartialViewToString(this ControllerBase controller, string viewName, object model)
		{
			return RenderPartialViewToString(controller, viewName, model, null);
		}

	    /// <summary>
	    /// Render partial view to string
	    /// </summary>
	    /// <param name="controller"></param>
	    /// <param name="viewName">View name</param>
	    /// <param name="model">Model</param>
	    /// <param name="additionalViewData">Additional view data</param>
	    /// <returns>Result</returns>
	    public static string RenderPartialViewToString(this ControllerBase controller, string viewName, object model, object additionalViewData)
        {
            if (viewName.IsEmpty())
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

			if (additionalViewData != null)
			{
				controller.ViewData.AddRange(CommonHelper.ObjectToDictionary(additionalViewData));

				if (additionalViewData is ViewDataDictionary vdd)
				{
					controller.ViewData.TemplateInfo.HtmlFieldPrefix = vdd.TemplateInfo.HtmlFieldPrefix;
				}
			}

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = System.Web.Mvc.ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName.EmptyNull());

                ThrowIfViewNotFound(viewResult, viewName);

                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

		/// <summary>
		/// Render view to string
		/// </summary>
		/// <returns>Result</returns>
		public static string RenderViewToString(this ControllerBase controller)
		{
			return RenderViewToString(controller, null, null, null);
		}

		/// <summary>
		/// Render view to string
		/// </summary>
		/// <param name="model">Model</param>
		/// <returns>Result</returns>
		public static string RenderViewToString(this ControllerBase controller, object model)
		{
			return RenderViewToString(controller, null, null, model);
		}

		/// <summary>
		/// Render view to string
		/// </summary>
		/// <param name="viewName">View name</param>
		/// <returns>Result</returns>
		public static string RenderViewToString(this ControllerBase controller, string viewName)
		{
			return RenderViewToString(controller, viewName, null, null);
		}

		/// <summary>
		/// Render view to string
		/// </summary>
		/// <param name="viewName">View name</param>
		/// <param name="model">Model</param>
		/// <returns>Result</returns>
		public static string RenderViewToString(this ControllerBase controller, string viewName, string masterName)
		{
			return RenderViewToString(controller, viewName, masterName, null);
		}

		/// <summary>
		/// Render view to string
		/// </summary>
		/// <param name="viewName">View name</param>
		/// <param name="masterName">Master name</param>
		/// <param name="model">Model</param>
		/// <returns>Result</returns>
		public static string RenderViewToString(this ControllerBase controller, string viewName, string masterName, object model)
		{
			if (viewName.IsEmpty())
				viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

			controller.ViewData.Model = model;
			
			using (var sw = new StringWriter())
			{
				var viewResult = System.Web.Mvc.ViewEngines.Engines.FindView(controller.ControllerContext, viewName.EmptyNull(), masterName.EmptyNull());

				ThrowIfViewNotFound(viewResult, viewName);

				var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
				viewResult.View.Render(viewContext, sw);

				return sw.GetStringBuilder().ToString();
			}
		}

		private static void ThrowIfViewNotFound(ViewEngineResult viewResult, string viewName)
		{
			// if view not found, throw an exception with searched locations
			if (viewResult.View == null)
			{
				throw new ViewNotFoundException(viewName, viewResult.SearchedLocations);
			}
		}

        #endregion

        #region RootActionView

        /// <summary>
        /// Creates a <see cref="ViewResult"/> object using the root actions's <see cref="ControllerContext"/> for view resolution.
        /// </summary>
        public static ViewResult RootActionView(this Controller controller, string viewName)
		{
			return RootActionView(controller, viewName, (string)null, null);
		}

		/// <summary>
		/// Creates a <see cref="ViewResult"/> object using the root actions's <see cref="ControllerContext"/> for view resolution.
		/// </summary>
		public static ViewResult RootActionView(this Controller controller, string viewName, string masterName)
		{
			return RootActionView(controller, viewName, masterName, null);
		}

		/// <summary>
		/// Creates a <see cref="ViewResult"/> object using the root actions's <see cref="ControllerContext"/> for view resolution.
		/// </summary>
		public static ViewResult RootActionView(this Controller controller, string viewName, object model)
		{
			return RootActionView(controller, viewName, (string)null, model);
		}

		/// <summary>
		/// Creates a <see cref="ViewResult"/> object using the root actions's <see cref="ControllerContext"/> for view resolution.
		/// </summary>
		public static ViewResult RootActionView(this Controller controller, string viewName, string masterName, object model)
		{
			Guard.NotEmpty(viewName, nameof(viewName));

			if (model != null)
			{
				controller.ViewData.Model = model;
			}

			return new RootActionViewResult
			{
				ViewName = viewName,
				MasterName = masterName,
				ViewData = controller.ViewData,
				TempData = controller.TempData,
				ViewEngineCollection = controller.ViewEngineCollection
			};
		}

		/// <summary>
		/// Creates a <see cref="PartialViewResult"/> object using the root actions's <see cref="ControllerContext"/> for view resolution.
		/// </summary>
		public static PartialViewResult RootActionPartialView(this Controller controller, string viewName)
		{
			return RootActionPartialView(controller, viewName, null);
		}

		/// <summary>
		/// Creates a <see cref="PartialViewResult"/> object using the root actions's <see cref="ControllerContext"/> for view resolution.
		/// </summary>
		public static PartialViewResult RootActionPartialView(this Controller controller, string viewName, object model)
		{
			Guard.NotEmpty(viewName, nameof(viewName));

			if (model != null)
			{
				controller.ViewData.Model = model;
			}

			return new RootActionPartialViewResult
			{
				ViewName = viewName,
				ViewData = controller.ViewData,
				TempData = controller.TempData,
				ViewEngineCollection = controller.ViewEngineCollection
			};
		}

		#endregion

		/// <summary>
		/// Get active store scope (for multi-store configuration mode)
		/// </summary>
		/// <param name="controller">Controller</param>
		/// <param name="storeService">Store service</param>
		/// <param name="workContext">Work context</param>
		/// <returns>Store ID; 0 if we are in a shared mode</returns>
		public static int GetActiveStoreScopeConfiguration(this IController controller, IStoreService storeService, IWorkContext workContext)
		{
			//ensure that we have 2 (or more) stores
			if (storeService.GetAllStores().Count < 2)
				return 0;

			var storeId = workContext.CurrentCustomer.GetAttribute<int>(SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration);
			var store = storeService.GetStoreById(storeId);
			return store != null ? store.Id : 0;
		}
    }
}
