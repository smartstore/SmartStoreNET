﻿using System.IO;
using System.Web.Mvc;
using System.ComponentModel;
using System;
using SmartStore.Web.Framework.UI;
using System.Collections.Generic;

namespace SmartStore.Web.Framework.Controllers
{
    public static class ContollerExtensions
    {
        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <returns>Result</returns>
        public static string RenderPartialViewToString(this Controller controller)
        {
            return RenderPartialViewToString(controller, null, null);
        }
        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <returns>Result</returns>
        public static string RenderPartialViewToString(this Controller controller, string viewName)
        {
            return RenderPartialViewToString(controller, viewName, null);
        }
        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        public static string RenderPartialViewToString(this Controller controller, object model)
        {
            return RenderPartialViewToString(controller, null, model);
        }
        /// <summary>
        /// Render partial view to string
        /// </summary>
        /// <param name="viewName">View name</param>
        /// <param name="model">Model</param>
        /// <returns>Result</returns>
        public static string RenderPartialViewToString(this Controller controller, string viewName, object model)
        {
            //Original source code: http://craftycodeblog.com/2010/05/15/asp-net-mvc-render-partial-view-to-string/
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = System.Web.Mvc.ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }

		/// <summary>
		/// Display notification
		/// </summary>
		/// <remarks>codehint: sm-add</remarks>
		/// <param name="type">Notification type</param>
		/// <param name="message">Message</param>
		/// <param name="persistForTheNextRequest">A value indicating whether a message should be persisted for the next request</param>
		public static void AddNotificationMessage(this ControllerBase controller, NotifyType type, string message, bool persistForTheNextRequest) {
			if (message.IsNullOrEmpty())
				return;

			List<string> lst = null;
			string dataKey = string.Format("sm.notifications.{0}", type);

			if (persistForTheNextRequest) {
				if (controller.TempData[dataKey] == null)
					controller.TempData[dataKey] = new List<string>();
				lst = (List<string>)controller.TempData[dataKey];
			}
			else {
				if (controller.ViewData[dataKey] == null)
					controller.ViewData[dataKey] = new List<string>();
				lst = (List<string>)controller.ViewData[dataKey];
			}

			if (lst != null && !lst.Exists(m => m == message))
				lst.Add(message);
		}
    }
}
