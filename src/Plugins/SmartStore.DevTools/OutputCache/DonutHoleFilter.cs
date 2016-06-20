using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Core.Data;

namespace SmartStore.DevTools.OutputCache
{
	public class DonutHoleFilter : IActionFilter
	{
		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!filterContext.IsChildAction)
				return;

			if (!filterContext.HttpContext.Items.Contains("OutputCache.IsCachingRequest"))
				return;

			if (filterContext.RouteData.Values.ContainsKey("OutputCache.InvokingChildAction"))
				return;

			if (!DataSettings.DatabaseIsInstalled())
				return;

			var data = GetActionData(filterContext);

			// TEMP
			if (!data.ActionName.IsCaseInsensitiveEqual("ShopBar"))
				return;
			// TEMP

			var serializedData = JsonConvert.SerializeObject(data);
			
			filterContext.Result = new ContentResult
			{
				Content = string.Format("<!--Donut#{0}#-->{1}<!--EndDonut-->", serializedData, "ReplaceMe")
			};
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			//
		}

		private ChildActionData GetActionData(ActionExecutingContext context)
		{
			return new ChildActionData
			{
				ActionName = context.ActionDescriptor.ActionName,
				ControllerName = context.ActionDescriptor.ControllerDescriptor.ControllerName,
				RouteValues = context.RouteData.Values
			};
		}
	}
}