//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Routing;
//using SmartStore.Services.Cms;

//namespace SmartStore.Plugin.Misc.FilterTest.Widgets
//{
//	public class ProfilerWidget : IWidget
//	{
//		public IList<string> GetWidgetZones()
//		{
//			return new string[] { "head_html_tag" };
//		}

//		public void GetDisplayWidgetRoute(string widgetZone, object model, int storeId, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
//		{
//			actionName = "MiniProfiler";
//			controllerName = "MyCheckout";
//			routeValues = new RouteValueDictionary() 
//			{ 
//				{ "Namespaces", "SmartStore.Plugin.Developer.DevTools.Controllers" }, 
//				{ "area", "Deveoper.DevTools" } 
//			};
//		}
//	}
//}