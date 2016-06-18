using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Newtonsoft.Json;

namespace SmartStore.DevTools.OutputCache
{
	public class DonutHoleProcessor
	{
		private static readonly Regex s_rgDonutHoles = new Regex("<!--Donut#(.*?)#-->(.*?)<!--EndDonut-->", RegexOptions.Compiled | RegexOptions.Singleline);

		private readonly ChildActionDataSerializer _actionDataSerialiser;

		public DonutHoleProcessor()
		{
			_actionDataSerialiser = new ChildActionDataSerializer();
		}

		public string RemoveHoleWrappers(string content, ControllerContext filterContext)
		{
			return s_rgDonutHoles.Replace(content, match => match.Groups[2].Value);
		}

		public string ReplaceHole(string content, ControllerContext filterContext)
		{
			return s_rgDonutHoles.Replace(content, match =>
			{
				var actionSettings = _actionDataSerialiser.Deserialise(match.Groups[1].Value);
				
				return InvokeAction(
					filterContext.Controller,
					actionSettings.ActionName,
					actionSettings.ControllerName,
					actionSettings.RouteValues
				);
			});
		}

		private static string InvokeAction(ControllerBase controller, string actionName, string controllerName, RouteValueDictionary routeValues)
		{
			var viewContext = new ViewContext(
				controller.ControllerContext,
				new WebFormView(controller.ControllerContext, "tmp"),
				controller.ViewData,
				controller.TempData,
				TextWriter.Null
			);

			var htmlHelper = new HtmlHelper(viewContext, new ViewPage());

			return htmlHelper.Action(actionName, controllerName, routeValues).ToString();
		}
	}
}